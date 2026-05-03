using System;
using System.Collections.Generic;
using Collection;
using Cysharp.Threading.Tasks;
using Controllers;
using Data;
using NaughtyAttributes;
using Obvious.Soap;
using Shared.Enums;
using Shared.Events;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = UnityEngine.Random;

namespace Managers
{
    public class EnemySpawnManager : MonoBehaviour
    {
        [Serializable]
        public struct EnemyVariantSettings
        {
            public AssetReferenceT<EnemyConfig> ConfigReference;
            public int UnlockRound;
            public float BaseWeight;
            public float WeightGrowthPerRound;
        }

        [Header("Enemy Spawn References")] 
        [SerializeField] private List<EnemyVariantSettings> _enemyVariants;
        [SerializeField] private Camera _camera;

        [Header("Spawn Settings")]
        [SerializeField] private SpawnMode _spawnMode = SpawnMode.CameraEdge;
        [SerializeField] [ShowIf("_spawnMode", SpawnMode.AroundTarget)] private float _minSpawnDistance = 10f;
        [SerializeField] [ShowIf("_spawnMode", SpawnMode.AroundTarget)] private float _maxSpawnDistance = 15f;

        private PlayerController _playerController;
        private Dictionary<int, Queue<EnemyController>> _enemyPools = new();
        private Dictionary<int, EnemyConfig> _loadedConfigs = new();
        private Dictionary<int, Transform> _poolParents = new();
        private bool _isInitialized;
        

        public bool IsInitialized => _isInitialized;

         private void Awake()
         {
             ServiceLocator.Register(this);
         }

         private void Start()
        {
            LoadEnemyConfigsAsync().Forget();
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
            foreach (var variant in _enemyVariants)
            {
                if (variant.ConfigReference.IsValid())
                {
                    variant.ConfigReference.ReleaseAsset();
                }
            }
            
            ServiceLocator.Unregister<EnemySpawnManager>();
        }

        public EnemyController GetPooledEnemy(int enemyId)
        {
            if (_enemyPools.TryGetValue(enemyId, out var pool) && pool.Count > 0)
            {
                int poolSize = pool.Count;

                for (int i = 0; i < poolSize; i++)
                {
                    EnemyController enemy = pool.Dequeue();
                    pool.Enqueue(enemy);

                    if (!enemy.gameObject.activeSelf)
                    {
                        enemy.gameObject.SetActive(true);
                        return enemy;
                    }
                }
            }
            
            // If we reach here, we need to expand the pool
            if (_loadedConfigs.TryGetValue(enemyId, out var config))
            {
                return CreatePooledEnemy(config);
            }

            Debug.LogWarning($"No pool or config found for EnemyID: {enemyId}");
            return null;
        }
        
        public int SpawnEnemies(int enemyCount, int currentRound, EnemyStatMultipliers multipliers)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("EnemySpawnManager is not initialized yet.", this);
                return 0;
            }

            if (_camera == null)
            {
                _camera = Camera.main;
            }

            if (_camera == null || _playerController == null)
            {
                Debug.LogWarning("EnemySpawnManager is missing camera or player reference.", this);
                return 0;
            }

            int spawnCount = Mathf.Max(0, enemyCount);
            int spawnedEnemies = 0;

            for (int i = 0; i < spawnCount; i++)
            {
                EnemyConfig config = GetRandomVariantConfig(currentRound);
                if (config == null) continue;

                EnemyController enemy = GetPooledEnemy(config.EnemyID);
                if (enemy == null) continue;

                Transform enemyTransform = enemy.gameObject.transform;
                enemyTransform.position = _spawnMode == SpawnMode.CameraEdge 
                    ? GetEdgeSpawnPosition() 
                    : GetAroundTargetSpawnPosition();
                
                enemyTransform.rotation = Quaternion.identity;
                enemy.InitializePlayer(_playerController);
                
                EnemyRuntimeStats scaledStats = new EnemyRuntimeStats(
                    config.EnemyHealth * multipliers.HealthMultiplier,
                    config.EnemyDamage * multipliers.DamageMultiplier,
                    config.EnemyMoveSpeed * multipliers.MoveSpeedMultiplier,
                    config.EnemyAtkSpeed * multipliers.AttackSpeedMultiplier,
                    config.EnemyKnockbackForce * multipliers.KnockbackMultiplier
                );
                
                enemy.ApplyRuntimeStats(scaledStats);

                spawnedEnemies++;
            }

            return spawnedEnemies;
        }

        private EnemyConfig GetRandomVariantConfig(int currentRound)
        {
            List<(EnemyConfig config, float weight)> availableVariants = new();
            float totalWeight = 0;

            foreach (var variant in _enemyVariants)
            {
                if (currentRound >= variant.UnlockRound && variant.ConfigReference.Asset != null)
                {
                    EnemyConfig config = variant.ConfigReference.Asset as EnemyConfig;
                    float currentWeight = variant.BaseWeight + (currentRound - variant.UnlockRound) * variant.WeightGrowthPerRound;
                    currentWeight = Mathf.Max(0, currentWeight);
                    
                    availableVariants.Add((config, currentWeight));
                    totalWeight += currentWeight;
                }
            }

            if (availableVariants.Count == 0) return null;

            float randomValue = Random.Range(0, totalWeight);
            float cumulativeWeight = 0;

            foreach (var variant in availableVariants)
            {
                cumulativeWeight += variant.weight;
                if (randomValue <= cumulativeWeight)
                {
                    return variant.config;
                }
            }

            return availableVariants[0].config;
        }

        private Vector2 GetAroundTargetSpawnPosition()
        {
            if (_playerController == null)
            {
                return GetEdgeSpawnPosition();
            }

            Vector2 center = _playerController.transform.position;
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(_minSpawnDistance, _maxSpawnDistance);

            return center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
        }

        private void CreatePools()
        {
            foreach (var config in _loadedConfigs.Values)
            {
                if (!_enemyPools.ContainsKey(config.EnemyID))
                {
                    _enemyPools[config.EnemyID] = new Queue<EnemyController>();
                    
                    GameObject poolParent = new GameObject($"Pool_{config.name}");
                    poolParent.transform.SetParent(this.transform);
                    _poolParents[config.EnemyID] = poolParent.transform;

                    for (int i = 0; i < config.EnemyAmountToPool; i++)
                    {
                        CreatePooledEnemy(config);
                    }
                }
            }
        }

        private EnemyController CreatePooledEnemy(EnemyConfig config)
        {
            EnemyController pooledEnemy = Instantiate(config.EnemyPrefab, _poolParents[config.EnemyID]);
            pooledEnemy.gameObject.SetActive(false);
            _enemyPools[config.EnemyID].Enqueue(pooledEnemy);
            return pooledEnemy;
        }

        private Vector2 GetEdgeSpawnPosition() { 
            float height = _camera.orthographicSize;
            float width = height * _camera.aspect;
            float margin = 2f;

            Vector3 camPos = _camera.transform.position;
            int side = Random.Range(0, 4);

            return side switch {
                0 => new Vector2(Random.Range(camPos.x - width - margin, camPos.x + width + margin), camPos.y + height + margin), // top
                1 => new Vector2(camPos.x + width + margin, Random.Range(camPos.y - height - margin, camPos.y + height + margin)), // right
                2 => new Vector2(Random.Range(camPos.x - width - margin, camPos.x + width + margin), camPos.y - height - margin), // bottom
                _ => new Vector2(camPos.x - width - margin, Random.Range(camPos.y - height - margin, camPos.y + height + margin)), // left
            };
        }

        private void InitializePlayer(PlayerController player)
        {
            _playerController = player;
        }
        
        private async UniTaskVoid LoadEnemyConfigsAsync()
        {
            List<UniTask<EnemyConfig>> loadTasks = new();
            foreach (var variant in _enemyVariants)
            {
                loadTasks.Add(variant.ConfigReference.LoadAssetAsync<EnemyConfig>().ToUniTask());
            }

            EnemyConfig[] configs = await UniTask.WhenAll(loadTasks);
            foreach (var config in configs)
            {
                _loadedConfigs[config.EnemyID] = config;
            }

            CreatePools();
            _isInitialized = true;
        }

    }
}
