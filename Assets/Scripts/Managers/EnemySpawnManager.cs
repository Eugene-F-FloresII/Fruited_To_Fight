using System;
using System.Collections.Generic;
using Collection;
using Cysharp.Threading.Tasks;
using Controllers;
using Data;
using NaughtyAttributes;
using Shared.Enums;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = UnityEngine.Random;

namespace Managers
{
    public class EnemySpawnManager : MonoBehaviour
    {
        [Header("Enemy Spawn References")] 
        [SerializeField] private AssetReferenceT<EnemyConfig> _enemyConfigReference;
        [SerializeField] private DefendingController _defendingController;
        [SerializeField] private Camera _camera;

        [Header("Spawn Settings")]
        [SerializeField] private SpawnMode _spawnMode = SpawnMode.CameraEdge;
        [SerializeField] [ShowIf("_spawnMode", SpawnMode.AroundTarget)] private float _minSpawnDistance = 10f;
        [SerializeField] [ShowIf("_spawnMode", SpawnMode.AroundTarget)] private float _maxSpawnDistance = 15f;
        
        [Header("Enemy Oranges Settings")]
        [SerializeField] private EnemyController _pooledEnemy;
        [SerializeField] private Transform _pooledTransform;
        [SerializeField] private int _enemiesToSpawn;
        
        private int _enemiesAmountToPool;
        private int _totalPooledEnemies;
        private EnemyConfig _enemyConfig;
        private Queue<EnemyController> _pooledEnemies = new();
        private bool _isInitialized;

        public bool IsInitialized => _isInitialized;

         private void Awake()
         {
             ServiceLocator.Register(this);
         }

         private void Start()
        {
            LoadEnemyConfigAsync().Forget();
            _camera = Camera.main;
        }

        private void OnDestroy()
        {
            if(_enemyConfigReference.IsValid())
            {
                _enemyConfigReference.ReleaseAsset();
            }
            
            ServiceLocator.Unregister<EnemySpawnManager>();
        }

        public EnemyController GetPooledEnemy()
        {
            if (_pooledEnemies.Count > 0)
            {
                int poolSize = _pooledEnemies.Count;

                for (int i = 0; i < poolSize; i++)
                {
                    EnemyController enemy = _pooledEnemies.Dequeue();
                    _pooledEnemies.Enqueue(enemy);

                    if (!enemy.gameObject.activeSelf)
                    {
                        enemy.gameObject.SetActive(true);
                        return enemy;
                    }
                }

                Debug.LogWarning("All pooled enemies are currently active");
                return null;
            }
            
            Debug.LogWarning("No enemies left in pool");
            return null;
            
        }
        
        [Button("Spawn Enemies")]
         public void SpawnEnemies()
        {
            SpawnEnemies(_enemiesToSpawn, BuildBaseRuntimeStats());
        }

        public int SpawnEnemies(int enemyCount, EnemyRuntimeStats runtimeStats)
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

            if (_camera == null || _defendingController == null)
            {
                Debug.LogWarning("EnemySpawnManager is missing camera or player reference.", this);
                return 0;
            }

            int spawnCount = Mathf.Max(0, enemyCount);
            EnsurePoolCapacity(spawnCount);

            int spawnedEnemies = 0;

            for (int i = 0; i < spawnCount; i++)
            {
                EnemyController enemy = GetPooledEnemy();
                if (enemy == null)
                {
                    break;
                }

                Transform enemyTransform = enemy.gameObject.transform;
                enemyTransform.position = _spawnMode == SpawnMode.CameraEdge 
                    ? GetEdgeSpawnPosition() 
                    : GetAroundTargetSpawnPosition();
                
                enemyTransform.rotation = Quaternion.identity;
                enemy.InitializePlayer(_defendingController);
                enemy.ApplyRuntimeStats(runtimeStats);

                spawnedEnemies++;
            }

            return spawnedEnemies;
        }

        private Vector2 GetAroundTargetSpawnPosition()
        {
            if (_defendingController == null)
            {
                return GetEdgeSpawnPosition();
            }

            Vector2 center = _defendingController.transform.position;
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(_minSpawnDistance, _maxSpawnDistance);

            return center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
        }

        public int GetEnemiesAmount()
        {
            return _enemiesToSpawn;
        }
         
        private void PoolEnemies()
        {
            _pooledEnemies = new Queue<EnemyController>();
            _totalPooledEnemies = 0;

            for (int i = 0; i < _enemiesAmountToPool; i++)
            {
                CreatePooledEnemy();
            }
        }

        private void EnsurePoolCapacity(int requiredCount)
        {
            if (requiredCount <= _totalPooledEnemies)
            {
                return;
            }

            int enemiesToCreate = requiredCount - _totalPooledEnemies;
            for (int i = 0; i < enemiesToCreate; i++)
            {
                CreatePooledEnemy();
            }
        }

        private void CreatePooledEnemy()
        {
            EnemyController pooledEnemy = Instantiate(_pooledEnemy, _pooledTransform);
            pooledEnemy.gameObject.SetActive(false);
            _pooledEnemies.Enqueue(pooledEnemy);
            _totalPooledEnemies++;
        }
        
        private void UpdateEnemyStats()
        {
            _enemiesAmountToPool = _enemyConfig.EnemyAmountToPool;
            _enemiesToSpawn = _enemyConfig.EnemyAmountToPool;
            _pooledEnemy = _enemyConfig.EnemyPrefab;
        }

        private EnemyRuntimeStats BuildBaseRuntimeStats()
        {
            if (_enemyConfig == null)
            {
                return default;
            }

            return new EnemyRuntimeStats(
                _enemyConfig.EnemyHealth,
                _enemyConfig.EnemyDamage,
                _enemyConfig.EnemyMoveSpeed,
                _enemyConfig.EnemyAtkSpeed,
                _enemyConfig.EnemyKnockbackForce
            );
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
        
        private async UniTaskVoid LoadEnemyConfigAsync()
        {
            _enemyConfig = await _enemyConfigReference.LoadAssetAsync<EnemyConfig>().ToUniTask();
    
            UpdateEnemyStats();
            PoolEnemies();
            _isInitialized = true;
        }

    }

}
