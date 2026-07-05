using System;
using System.Threading;
using Collection;
using Data;
using Shared.Events;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using Obvious.Soap;
using UnityEngine.SceneManagement;


namespace Managers
{
    public class RoundManager : MonoBehaviour
    {
        [Header("Round Config")]
        [Min(0)] [SerializeField] private int _firstRoundSpawnCount = 10;
        [Min(0)] [SerializeField] private int _spawnIncrementPerRound = 1;
        [SerializeField] private float _nextRoundDelaySeconds = 2f;
        [SerializeField] private IntVariable _currentRound;
        [SerializeField] private IntVariable _maxRounds;
        [SerializeField] private IntVariable _activeEnemyCount;
        
        [Header("Multiplicative Scaling")]
        [SerializeField] private float _healthGrowthPerRound = 1.10f;
        [SerializeField] private float _damageGrowthPerRound = 1.02f;
        [SerializeField] private float _moveSpeedGrowthPerRound = 1.02f;
        [SerializeField] private float _attackSpeedGrowthPerRound = 1.04f;
        [SerializeField] private float _knockbackGrowthPerRound = 1.05f;
        
        private EnemySpawnManager _enemySpawnManager;
        private EnemyBossSpawnManager _enemyBossSpawnManager;
        private UpgradesManager _upgradesManager;
        
        private bool _roundStarted;
        private int _enemiesRemainingToSpawn;
        private bool _isTransitioning;
        private bool _hasReachedMaxRounds;

        private CancellationTokenSource _roundFlowCts;

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        private void OnEnable()
        {
            if (_activeEnemyCount != null) _activeEnemyCount.OnValueChanged += HandleActiveEnemyCountChanged;
        }

        private void Start()
        {
            _roundFlowCts = new CancellationTokenSource();
            InitializeRoundFlow(_roundFlowCts.Token).Forget();
            
            _upgradesManager = ServiceLocator.Get<UpgradesManager>();
            
        }

        private void OnDisable()
        {
            if (_activeEnemyCount != null) _activeEnemyCount.OnValueChanged -= HandleActiveEnemyCountChanged;
            DisposeRoundFlowToken();
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<RoundManager>();
        }

        private async UniTaskVoid InitializeRoundFlow(CancellationToken token)
        {
            try
            {
                if (!ValidateRoundVariables())
                {
                    return;
                }

                _currentRound.Value = 0;
                _hasReachedMaxRounds = false;

                await ResolveSpawnManagerAsync(token);
                StartNextRound();
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Round flow cancelled.", this);
            }
        }

        private async UniTask ResolveSpawnManagerAsync(CancellationToken token)
        {
            _enemySpawnManager = ServiceLocator.Get<EnemySpawnManager>();

            if (_enemySpawnManager == null)
            {
                _enemySpawnManager = FindObjectOfType<EnemySpawnManager>();
            }

            while (_enemySpawnManager == null)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, token);
                _enemySpawnManager = FindObjectOfType<EnemySpawnManager>();
            }

            while (!_enemySpawnManager.IsInitialized)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            // Resolve EnemyBossSpawnManager if it exists in the scene
            _enemyBossSpawnManager = ServiceLocator.Get<EnemyBossSpawnManager>();
            if (_enemyBossSpawnManager == null)
            {
                _enemyBossSpawnManager = FindObjectOfType<EnemyBossSpawnManager>();
            }

            if (_enemyBossSpawnManager != null)
            {
                while (!_enemyBossSpawnManager.IsInitialized)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
            }
        }

        private void HandleActiveEnemyCountChanged(int currentCount)
        {
            if (!_roundStarted || _enemiesRemainingToSpawn > 0 || currentCount > 0)
            {
                return;
            }
            EndCurrentRound();

            if (_currentRound.Value >= _maxRounds.Value)
            {
                HandleMaxRoundsReached();
            }
            else
            { 
                StartNextRoundAfterDelay().Forget();
            }
        }

        private void EndCurrentRound()
        {
            _roundStarted = false;
            Events_Round.OnRoundEnded?.Invoke(_currentRound.Value);

            if (_upgradesManager == null) _upgradesManager = ServiceLocator.Get<UpgradesManager>();
            
            if (_currentRound.Value % 2 == 0 && _upgradesManager.CanUpgradeAfflictions())
            {
                Events_Upgrades.OnActivateUpgradeAfflictionPanel?.Invoke();
                return;
            }

            if (_upgradesManager != null && !_upgradesManager.AreAllLevelsMaxed())
            { 
                Events_Upgrades.OnActivateUpgradePanel?.Invoke();
            }
        }

        private async UniTaskVoid StartNextRoundAfterDelay()
        {
            if (_isTransitioning || _roundFlowCts == null)
            {
                return;
            }

            _isTransitioning = true;

            try
            {
                int delayMilliseconds = Mathf.Max(0, Mathf.RoundToInt(_nextRoundDelaySeconds * 1000f));
                await UniTask.Delay(delayMilliseconds, cancellationToken: _roundFlowCts.Token);
                StartNextRound();
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Round transition cancelled.", this);
            }
            finally
            {
                _isTransitioning = false;
            }
        }

        private void StartNextRound()
        {
            if (_enemySpawnManager == null)
            {
                return;
            }

            if (_currentRound.Value >= _maxRounds.Value)
            {
                HandleMaxRoundsReached();
                return;
            }
            
            _currentRound.Value++;
            Events_Round.OnRoundStarted?.Invoke(_currentRound.Value);
            
            if (_upgradesManager == null) _upgradesManager = ServiceLocator.Get<UpgradesManager>();

            if (_upgradesManager != null && !_upgradesManager.AreAllLevelsMaxed())
            { 
                Events_Upgrades.OnRoundStarted?.Invoke();
            }
           
            int spawnCount = BuildSpawnCount(_currentRound.Value);
            EnemyStatMultipliers multipliers = BuildStatMultipliers(_currentRound.Value);

            _roundStarted = false; // Ensure false while initializing spawner
            _enemiesRemainingToSpawn = spawnCount;

            if (spawnCount > 0)
            {
                SpawnEnemiesOverTime(spawnCount, _currentRound.Value, multipliers, _roundFlowCts.Token).Forget();
            }
            else
            {
                Debug.LogWarning($"Round {_currentRound.Value} has 0 enemies to spawn.", this);
                HandleActiveEnemyCountChanged(0);
            }
        }

        private bool ValidateRoundVariables()
        {
            if (_currentRound == null || _maxRounds == null)
            {
                Debug.LogError("RoundManager requires both CurrentRound and MaxRounds IntVariable references.", this);
                _roundStarted = false;
                return false;
            }

            if (_maxRounds.Value <= 0)
            {
                Debug.LogWarning("RoundManager has MaxRounds set to 0 or less. No rounds will be started.", this);
            }

            return true;
        }

        private void HandleMaxRoundsReached()
        {
            if (_hasReachedMaxRounds)
            {
                return;
            }

            _hasReachedMaxRounds = true;
            _roundStarted = false;
            _isTransitioning = false;

            Debug.Log("Max rounds reached. Run complete.", this);
            Events_Game.OnGameExited?.Invoke();
            Events_Game.OnShowResultPanel?.Invoke(true);
        }

        private int BuildSpawnCount(int roundIndex)
        {
            int elapsedRounds = Mathf.Max(0, roundIndex - 1);
            int spawnCount = Mathf.RoundToInt(15f + elapsedRounds * 4.2f + elapsedRounds * elapsedRounds * 0.2f);
            return Mathf.Max(0, spawnCount);
        }

        private EnemyStatMultipliers BuildStatMultipliers(int roundIndex)
        {
            int growthStep = Mathf.Max(0, roundIndex - 1);

            return new EnemyStatMultipliers(
                Mathf.Pow(Mathf.Max(0.01f, _healthGrowthPerRound), growthStep),
                Mathf.Pow(Mathf.Max(0.01f, _damageGrowthPerRound), growthStep),
                Mathf.Pow(Mathf.Max(0.01f, _moveSpeedGrowthPerRound), growthStep),
                Mathf.Pow(Mathf.Max(0.01f, _attackSpeedGrowthPerRound), growthStep),
                Mathf.Pow(Mathf.Max(0.01f, _knockbackGrowthPerRound), growthStep)
            );
        }

        private float GetSpawnDelay(int round)
        {
            return Mathf.Max(0.1f, 1.0f - round * 0.02f);
        }

        private async UniTaskVoid SpawnEnemiesOverTime(int totalSpawnCount, int round, EnemyStatMultipliers multipliers, CancellationToken token)
        {
            _roundStarted = true;

            if (_enemyBossSpawnManager != null)
            {
                _enemyBossSpawnManager.SpawnBoss(round, multipliers);
            }
            
            for (int i = 0; i < totalSpawnCount; i++)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                bool spawned = _enemySpawnManager.SpawnSingleEnemy(round, multipliers);
                if (spawned)
                {
                    _enemiesRemainingToSpawn--;
                }
                else
                {
                    _enemiesRemainingToSpawn--;
                }

                if (i < totalSpawnCount - 1)
                {
                    float delay = GetSpawnDelay(round);
                    int delayMs = Mathf.Max(100, Mathf.RoundToInt(delay * 1000f));
                    await UniTask.Delay(delayMs, cancellationToken: token);
                }
            }

            if (_enemiesRemainingToSpawn == 0 && _activeEnemyCount.Value == 0)
            {
                HandleActiveEnemyCountChanged(0);
            }
        }

        private void DisposeRoundFlowToken()
        {
            _roundFlowCts?.Cancel();
            _roundFlowCts?.Dispose();
            _roundFlowCts = null;
        }

        public void SkipRound()
        {
            if (!_roundStarted && !_isTransitioning)
            {
                return;
            }

            // Cancel current spawning loop
            DisposeRoundFlowToken();
            _enemiesRemainingToSpawn = 0;
            
            // Kill all active enemies
            var enemies = FindObjectsByType<Controllers.EnemyController>(FindObjectsSortMode.None);
            bool killedAny = false;
            foreach (var enemy in enemies)
            {
                if (enemy.gameObject.activeInHierarchy)
                {
                    enemy.KillEnemy();
                    killedAny = true;
                }
            }
            
            // Recreate token for the next round
            _roundFlowCts = new CancellationTokenSource();
            
            // Force transition if no enemies were active to trigger the value-changed event
            if (!killedAny || (_activeEnemyCount != null && _activeEnemyCount.Value == 0))
            {
                _roundStarted = true; // Ensure the guard passes
                HandleActiveEnemyCountChanged(0);
            }
        }
    }

}
