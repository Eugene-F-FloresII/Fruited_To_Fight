using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Controllers;
using Data;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Managers
{
    public class EnemySpawnManager : MonoBehaviour
    {
        [Header("Enemy Spawn References")]
        [SerializeField] private EnemyConfig _enemyConfig;
        [SerializeField] private EnemyController _enemyPrefab;
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private Camera _camera;
        
        [Header("Enemy Oranges Settings")]
        [SerializeField] private EnemyController _pooledRottenOrange;
        [SerializeField] private Transform _pooledRottenOrangesTransform;
        [SerializeField] private int _enemiesToSpawn;
        
        private int _enemiesAmountToPool;
        
         private Queue<EnemyController> _pooledEnemies = new();
         
        private void Start()
        {
            UpdateEnemyStats();
            PoolEnemies();
            SpawnEnemies();
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
             for (int i = 0; i < _enemiesToSpawn; i++)
             {
                  EnemyController enemy = GetPooledEnemy();
                 if (enemy == null) break;

                 enemy.gameObject.transform.position = GetEdgeSpawnPosition();
                 enemy.gameObject.transform.rotation = Quaternion.identity;
                 enemy.InitializePlayer(_playerController);
              } 
          }
         
        private void PoolEnemies()
        {
            _pooledEnemies = new Queue<EnemyController>();
            EnemyController pool;

            for (int i = 0; i < _enemiesAmountToPool; i++)
            {
                pool = Instantiate(_pooledRottenOrange, _pooledRottenOrangesTransform);
                pool.gameObject.SetActive(false);
                _pooledEnemies.Enqueue(pool);
            }
        }
        
        private void UpdateEnemyStats()
        {
            _enemiesAmountToPool = _enemyConfig.EnemyAmountToPool;
            _pooledRottenOrange = _enemyConfig.EnemyPrefab;
        }

        private Vector2 GetEdgeSpawnPosition() {
            _camera = Camera.main;
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
    }

}
