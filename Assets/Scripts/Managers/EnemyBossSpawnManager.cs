using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Collection;
using Controllers;
using Data;
using NaughtyAttributes;
using Obvious.Soap;
using Shared.Enums;
using Shared.Events;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Managers
{
    public class EnemyBossSpawnManager : MonoBehaviour
    {
        [Serializable]
        public struct BossVariantSettings
        {
            public AssetReferenceT<EnemyBossConfig> ConfigReference;
            public int SpawnRound;
            public bool SpawnOnDivisibleRound;
        }

        [Header("Boss Spawn References")]
        [SerializeField] private List<BossVariantSettings> _bossVariants;
        [SerializeField] private Camera _camera;
        [SerializeField] private IntVariable _activeEnemyCount;

        [Header("Spawn Settings")]
        [SerializeField] private SpawnMode _spawnMode = SpawnMode.CameraEdge;
        [SerializeField] [ShowIf("_spawnMode", SpawnMode.AroundTarget)] private float _minSpawnDistance = 10f;
        [SerializeField] [ShowIf("_spawnMode", SpawnMode.AroundTarget)] private float _maxSpawnDistance = 15f;

        private PlayerController _playerController;
        private Dictionary<int, EnemyBossConfig> _loadedConfigs = new();
        private List<GameObject> _instantiatedBosses = new();
        private bool _isInitialized;

        public bool IsInitialized => _isInitialized;

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        private void Start()
        {
            LoadBossConfigsAsync().Forget();
            _camera = Camera.main;
        }

        private void OnEnable()
        {
            Events_Game.OnGameStarted += InitializePlayer;
        }

        private void OnDisable()
        {
            Events_Game.OnGameStarted -= InitializePlayer;
        }

        private void OnDestroy()
        {
            foreach (var variant in _bossVariants)
            {
                if (variant.ConfigReference.IsValid())
                {
                    variant.ConfigReference.ReleaseAsset();
                }
            }

            foreach (var boss in _instantiatedBosses)
            {
                if (boss != null)
                {
                    Destroy(boss);
                }
            }

            ServiceLocator.Unregister<EnemyBossSpawnManager>();
        }

        public bool SpawnBoss(int currentRound, EnemyStatMultipliers multipliers)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("EnemyBossSpawnManager is not initialized yet.", this);
                return false;
            }

            if (_camera == null)
            {
                _camera = Camera.main;
            }

            if (_camera == null || _playerController == null)
            {
                Debug.LogWarning("EnemyBossSpawnManager is missing camera or player reference.", this);
                return false;
            }

            EnemyBossConfig config = GetBossConfigForRound(currentRound);
            if (config == null) return false;

            EnemyController boss = Instantiate(config.EnemyPrefab);
            _instantiatedBosses.Add(boss.gameObject);

            Transform bossTransform = boss.gameObject.transform;
            bossTransform.position = _spawnMode == SpawnMode.CameraEdge 
                ? GetEdgeSpawnPosition() 
                : GetAroundTargetSpawnPosition();

            bossTransform.rotation = Quaternion.identity;
            boss.InitializePlayer(_playerController);

            EnemyRuntimeStats scaledStats = new EnemyRuntimeStats(
                config.EnemyHealth * multipliers.HealthMultiplier,
                config.EnemyDamage * multipliers.DamageMultiplier,
                config.EnemyMoveSpeed * multipliers.MoveSpeedMultiplier,
                config.EnemyAtkSpeed * multipliers.AttackSpeedMultiplier,
                config.EnemyKnockbackForce * multipliers.KnockbackMultiplier
            );

            boss.ApplyRuntimeStats(scaledStats);
            return true;
        }

        private EnemyBossConfig GetBossConfigForRound(int currentRound)
        {
            foreach (var variant in _bossVariants)
            {
                if (variant.ConfigReference.Asset != null)
                {
                    EnemyBossConfig config = variant.ConfigReference.Asset as EnemyBossConfig;
                    if (config == null) continue;

                    if (variant.SpawnOnDivisibleRound)
                    {
                        if (variant.SpawnRound > 0 && currentRound % variant.SpawnRound == 0)
                        {
                            return config;
                        }
                    }
                    else
                    {
                        if (currentRound == variant.SpawnRound)
                        {
                            return config;
                        }
                    }
                }
            }
            return null;
        }

        private Vector2 GetAroundTargetSpawnPosition()
        {
            if (_playerController == null)
            {
                return GetEdgeSpawnPosition();
            }

            Vector2 center = _playerController.transform.position;
            float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = UnityEngine.Random.Range(_minSpawnDistance, _maxSpawnDistance);

            return center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
        }

        private Vector2 GetEdgeSpawnPosition()
        {
            float height = _camera.orthographicSize;
            float width = height * _camera.aspect;
            float margin = 2f;

            Vector3 camPos = _camera.transform.position;
            int side = UnityEngine.Random.Range(0, 4);

            return side switch
            {
                0 => new Vector2(UnityEngine.Random.Range(camPos.x - width - margin, camPos.x + width + margin), camPos.y + height + margin), // top
                1 => new Vector2(camPos.x + width + margin, UnityEngine.Random.Range(camPos.y - height - margin, camPos.y + height + margin)), // right
                2 => new Vector2(UnityEngine.Random.Range(camPos.x - width - margin, camPos.x + width + margin), camPos.y - height - margin), // bottom
                _ => new Vector2(camPos.x - width - margin, UnityEngine.Random.Range(camPos.y - height - margin, camPos.y + height + margin)), // left
            };
        }

        private void InitializePlayer(PlayerController player)
        {
            _playerController = player;
        }

        private async UniTaskVoid LoadBossConfigsAsync()
        {
            List<UniTask<EnemyBossConfig>> loadTasks = new();
            foreach (var variant in _bossVariants)
            {
                loadTasks.Add(variant.ConfigReference.LoadAssetAsync<EnemyBossConfig>().ToUniTask());
            }

            EnemyBossConfig[] configs = await UniTask.WhenAll(loadTasks);
            foreach (var config in configs)
            {
                _loadedConfigs[config.EnemyID] = config;
            }

            _isInitialized = true;
        }
    }
}
