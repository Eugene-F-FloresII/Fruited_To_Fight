using System;
using System.Threading;
using Collection;
using Data;
using Shared.Events;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using Obvious.Soap;
using Unity.Android.Gradle.Manifest;

namespace Managers
{
    public class RoundManager : MonoBehaviour
    {
        [Header("Round Config")]
        [SerializeField] private AssetReferenceT<EnemyConfig> _enemyConfigReference;
        [Min(0)] [SerializeField] private int _firstRoundSpawnCount = 10;
        [Min(0)] [SerializeField] private int _spawnIncrementPerRound = 1;
        [SerializeField] private float _nextRoundDelaySeconds = 2f;
        [SerializeField] private IntVariable _currentRound;
        [SerializeField] private IntVariable _maxRounds;
        
        [Header("Multiplicative Scaling")]
        [SerializeField] private float _healthGrowthPerRound = 1.15f;
        [SerializeField] private float _damageGrowthPerRound = 1.10f;
        [SerializeField] private float _moveSpeedGrowthPerRound = 1.05f;
        [SerializeField] private float _attackSpeedGrowthPerRound = 1.04f;
        [SerializeField] private float _knockbackGrowthPerRound = 1.05f;
        
        private EnemyConfig _enemyConfig;
        private EnemySpawnManager _enemySpawnManager;
        private UpgradesManager _upgradesManager;
        
        private int _enemiesRemainingInRound;
        private bool _roundStarted;
        private bool _isTransitioning;
        private bool _hasReachedMaxRounds;

        private CancellationTokenSource _roundFlowCts;

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        private void OnEnable()
        {
            Events_Seed.OnEnemyDeath += HandleEnemyDeath;
        }

        private void Start()
        {
            _roundFlowCts = new CancellationTokenSource();
            InitializeRoundFlow(_roundFlowCts.Token).Forget();
            
            _upgradesManager = ServiceLocator.Get<UpgradesManager>();
            
        }

        private void OnDisable()
        {
            Events_Seed.OnEnemyDeath -= HandleEnemyDeath;
            DisposeRoundFlowToken();
        }

        private void OnDestroy()
        {
            if (_enemyConfigReference.IsValid())
            {
                _enemyConfigReference.ReleaseAsset();
            }

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

                await LoadEnemyConfigAsync(token);
                await ResolveSpawnManagerAsync(token);
                StartNextRound();
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Round flow cancelled.", this);
            }
        }

        private async UniTask LoadEnemyConfigAsync(CancellationToken token)
        {
            if (_enemyConfig != null)
            {
                return;
            }

            _enemyConfig = await _enemyConfigReference.LoadAssetAsync<EnemyConfig>().ToUniTask(cancellationToken: token);
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
        }

        private void HandleEnemyDeath(Transform enemyTransform)
        {
            if (!_roundStarted || _enemiesRemainingInRound <= 0)
            {
                return;
            }

            _enemiesRemainingInRound--;

            if (_enemiesRemainingInRound <= 0)
            {
                Events_Seed.OnEnemiesDefeated?.Invoke();
            }
            
            if (_enemiesRemainingInRound > 0)
            {
                return;
            }

            EndCurrentRound();
            StartNextRoundAfterDelay().Forget();
        }

        private void EndCurrentRound()
        {
            _roundStarted = false;
            Events_Round.OnRoundEnded?.Invoke(_currentRound.Value);

            if (_upgradesManager == null) _upgradesManager = ServiceLocator.Get<UpgradesManager>();
            
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
            if (_enemyConfig == null || _enemySpawnManager == null)
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
            EnemyRuntimeStats runtimeStats = BuildRuntimeStats(_currentRound.Value);

            _enemiesRemainingInRound = _enemySpawnManager.SpawnEnemies(spawnCount, runtimeStats);
            _roundStarted = _enemiesRemainingInRound > 0;

            if (!_roundStarted)
            {
                Debug.LogWarning($"Round {_currentRound.Value} spawned 0 enemies.", this);
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
        }

        private int BuildSpawnCount(int roundIndex)
        {
            int additionalSpawns = Mathf.Max(0, roundIndex - 1) * _spawnIncrementPerRound;
            int spawnCount = _firstRoundSpawnCount + additionalSpawns;

            return Mathf.Max(0, spawnCount);
        }

        private EnemyRuntimeStats BuildRuntimeStats(int roundIndex)
        {
            int growthStep = Mathf.Max(0, roundIndex - 1);

            return new EnemyRuntimeStats(
                ScaleValue(_enemyConfig.EnemyHealth, _healthGrowthPerRound, growthStep),
                ScaleValue(_enemyConfig.EnemyDamage, _damageGrowthPerRound, growthStep),
                ScaleValue(_enemyConfig.EnemyMoveSpeed, _moveSpeedGrowthPerRound, growthStep),
                ScaleValue(_enemyConfig.EnemyAtkSpeed, _attackSpeedGrowthPerRound, growthStep),
                ScaleValue(_enemyConfig.EnemyKnockbackForce, _knockbackGrowthPerRound, growthStep)
            );
        }

        private static float ScaleValue(float baseValue, float growthPerRound, int growthStep)
        {
            float safeGrowth = Mathf.Max(0.01f, growthPerRound);
            return baseValue * Mathf.Pow(safeGrowth, growthStep);
        }

        private void DisposeRoundFlowToken()
        {
            _roundFlowCts?.Cancel();
            _roundFlowCts?.Dispose();
            _roundFlowCts = null;
        }
    }

}
