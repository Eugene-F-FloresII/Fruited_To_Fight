using System;
using System.Collections.Generic;
using System.Threading;
using Controllers;
using Data;
using UnityEngine;
using Cysharp.Threading.Tasks;


namespace Gameplay.Weapons
{
    public class ProjectileSpawner : MonoBehaviour
    {
        [Header("Weapon Config")]
        [SerializeField] private WeaponConfig _weaponConfig;
        
        [Header("Enemies in Range")]
        [SerializeField] private List<EnemyController> _enemies = new();
        
        [Header("Object Pooling Settings")]
      
        [SerializeField] private GameObject _pooledProjectile;
        [SerializeField] private Transform _pooledTransform;
        
        private CircleCollider2D _circleCollider2D;
        private CancellationTokenSource _attackCts;
        
        private int _amountToPool;
        private float _currentAtkSpeed;
        private float _currentRange;
        private readonly float _projectileRotationOffset = -90f;
        
        private Queue<GameObject> _pooledObjects;
        private void Awake()
        {
            _enemies ??= new List<EnemyController>();
            UpdateWeaponStats();
        }

        private void Start()
        {
            PoolObjects();
        }

        private void OnDestroy()
        {
            StopAttackLoop();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out EnemyController enemy))
            {
                if (!_enemies.Contains(enemy))
                {
                    _enemies.Add(enemy);
                }

                if (_enemies.Count == 1)
                {
                    StopAttackLoop();
                    _attackCts = new CancellationTokenSource();
                    AttackEnemyAsync(_attackCts.Token).Forget();
                }
            }
        }
        

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.TryGetComponent(out EnemyController enemy))
            {
                _enemies.Remove(enemy);

                if (_enemies.Count == 0)
                {
                    StopAttackLoop();
                }
            }
        }

        private void UpdateWeaponStats()
        {
            _pooledProjectile = _weaponConfig.WeaponPrefab;
            _currentAtkSpeed = _weaponConfig.WeaponAtkSpeed;
            _currentRange = _weaponConfig.WeaponRange;
            _amountToPool = _weaponConfig.WeaponAmountToPool;
            
            _circleCollider2D = GetComponent<CircleCollider2D>();
            _circleCollider2D.radius = _currentRange;
        }

        private void PoolObjects()
        {
            _pooledObjects = new Queue<GameObject>();
            GameObject pool;

            for (int i = 0; i < _amountToPool; i++)
            {
                pool = Instantiate(_pooledProjectile, _pooledTransform);
                pool.SetActive(false);
                _pooledObjects.Enqueue(pool);
            }
        }

        private void StopAttackLoop()
        {
            if (_attackCts == null)
            {
                return;
            }

            _attackCts?.Cancel();
            _attackCts?.Dispose();
            _attackCts = null;
        }

        public GameObject GetPooledObject()
        {
            if (_pooledObjects == null || _pooledObjects.Count == 0)
            {
                return null;
            }

            int poolSize = _pooledObjects.Count;

            for (int i = 0; i < poolSize; i++)
            {
                GameObject pooledObject = _pooledObjects.Dequeue();
                _pooledObjects.Enqueue(pooledObject);

                if (pooledObject != null && !pooledObject.activeInHierarchy)
                {
                    return pooledObject;
                }
            }

            return null;
        }

        private async UniTask AttackEnemyAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    _enemies.RemoveAll(e => e == null);

                    if (_enemies.Count == 0)
                    {
                        StopAttackLoop();
                        return;
                    }

                    EnemyController target = _enemies[0];

                    if (target == null)
                    {
                        continue;
                    }

                    Vector2 direction = target.transform.position - transform.position;
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    Quaternion rotation = Quaternion.Euler(0, 0, angle + _projectileRotationOffset);
                    
                    GameObject spear = GetPooledObject();

                    if (spear != null)
                    {
                        spear.transform.position = transform.position;
                        spear.transform.rotation = rotation;
                        spear.SetActive(true);

                        if (spear.TryGetComponent(out Rigidbody2D rb))
                        {
                            rb.linearVelocity = direction.normalized * _weaponConfig.WeaponSpeed;
                        }
                        else
                        {
                            Debug.LogWarning($"{nameof(ProjectileSpawner)} spawned projectile without Rigidbody2D.", this);
                            spear.SetActive(false);
                        }
                    }
                    
                    await UniTask.Delay(TimeSpan.FromSeconds(Mathf.Max(0.01f, _currentAtkSpeed)), cancellationToken: token);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Debug.LogException(ex, this);
                StopAttackLoop();
            }
        }
        
    }

}
