using System;
using System.Collections.Generic;
using System.Threading;
using Controllers;
using Data;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Shared.Enums;

namespace Gameplay.Weapons
{
    /// <summary>
    /// Base class for projectile weapon spawners that handle enemy detection, attack loops, and object pooling.
    /// </summary>
    public class ProjectileSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] protected WeaponConfig _weaponConfig;
        [SerializeField] protected WeaponAfflictionSprite _weaponAfflictionSprite;
        [SerializeField] protected WeaponAffliction _weaponAffliction;
        
        [Header("Enemies in Range")]
        [SerializeField] private List<EnemyController> _enemies = new();
        
        [Header("Object Pooling Settings")]
        [SerializeField] private GameObject _pooledProjectile;
        [SerializeField] private Transform _pooledTransform;
        
        private CircleCollider2D _circleCollider2D;
        private CancellationTokenSource _attackCts;
        
        private int _amountToPool;
        protected float _currentAtkSpeed;
        private float _currentRange;
        protected readonly float _projectileRotationOffset = -90f;
        
        private Queue<GameObject> _pooledObjects;

        /// <summary>
        /// Gets the effective attack speed with a enforced minimum delay floor of 0.1 seconds.
        /// </summary>
        protected float GetEffectiveAttackSpeed()
        {
            float atkSpeed = _weaponConfig != null ? _weaponConfig.WeaponAtkSpeed : 1f;
            return Mathf.Max(0.1f, atkSpeed);
        }

        private void Awake()
        {
            _enemies ??= new List<EnemyController>();
            UpdateWeaponStats();
        }

        private void Start()
        {
            PoolObjects();
            RefreshAfflictionVisuals();

            if (_weaponConfig != null)
                _weaponConfig.OnAfflictionsChanged += RefreshAfflictionVisuals;
        }

        private void OnDestroy()
        {
            StopAttackLoop();
            
            if (_weaponConfig != null)
                _weaponConfig.OnAfflictionsChanged -= RefreshAfflictionVisuals;
        }

        public virtual void RefreshAfflictionVisuals()
        {
            if (_weaponConfig == null || _weaponConfig.Afflictions == null)
            {
                return;
            }

            AfflictionType type = _weaponConfig.Afflictions.Count > 0 
                ? _weaponConfig.Afflictions[0].Type 
                : AfflictionType.None;

            if (_weaponAfflictionSprite != null)
            {
                _weaponAfflictionSprite.UpdateSprite(type);
            }

            if (_weaponAffliction != null)
            {
                _weaponAffliction.ToggleVisual(type, true);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out EnemyController enemy))
            {
                _enemies.RemoveAll(e => e == null || !e.gameObject.activeInHierarchy);

                if (!_enemies.Contains(enemy))
                {
                    _enemies.Add(enemy);
                }

                if (_attackCts == null)
                {
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
                _enemies.RemoveAll(e => e == null || !e.gameObject.activeInHierarchy);

                if (_enemies.Count == 0)
                {
                    StopAttackLoop();
                }
            }
        }

        private void UpdateWeaponStats()
        {
            if (_weaponConfig == null) return;

            _pooledProjectile = _weaponConfig.WeaponPrefab;
            _currentAtkSpeed = GetEffectiveAttackSpeed();
            _currentRange = _weaponConfig.WeaponRange;
            _amountToPool = _weaponConfig.WeaponAmountToPool;
            
            if (_circleCollider2D == null)
                _circleCollider2D = GetComponent<CircleCollider2D>();

            if (_circleCollider2D != null)
                _circleCollider2D.radius = _currentRange;
        }

        private void PoolObjects()
        {
            _pooledObjects = new Queue<GameObject>();
            if (_pooledProjectile == null) return;

            for (int i = 0; i < _amountToPool; i++)
            {
                GameObject pool = Instantiate(_pooledProjectile, _pooledTransform);
                pool.SetActive(false);
                _pooledObjects.Enqueue(pool);
            }
        }

        protected void StopAttackLoop()
        {
            if (_attackCts == null)
            {
                return;
            }

            var cts = _attackCts;
            _attackCts = null;
            cts.Cancel();
            cts.Dispose();
        }

        /// <summary>
        /// Retrieves an inactive pooled object, dynamically expanding the pool if all objects are active.
        /// </summary>
        public GameObject GetPooledObject()
        {
            _pooledObjects ??= new Queue<GameObject>();

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

            // Pool exhausted: Dynamically expand pool if prefab reference is available
            if (_pooledProjectile != null)
            {
                GameObject newObject = Instantiate(_pooledProjectile, _pooledTransform);
                newObject.SetActive(false);
                _pooledObjects.Enqueue(newObject);
                return newObject;
            }

            return null;
        }

        protected List<EnemyController> GetSortedEnemies()
        {
            _enemies.RemoveAll(e => e == null || !e.gameObject.activeInHierarchy);

            if (_enemies.Count > 1)
            {
                Vector2 spawnerPosition = transform.position;
                _enemies.Sort((a, b) =>
                {
                    float distASqr = ((Vector2)a.transform.position - spawnerPosition).sqrMagnitude;
                    float distBSqr = ((Vector2)b.transform.position - spawnerPosition).sqrMagnitude;
                    return distASqr.CompareTo(distBSqr);
                });
            }

            return _enemies;
        }

        protected EnemyController GetNearestEnemy()
        {
            var sortedEnemies = GetSortedEnemies();
            return sortedEnemies.Count > 0 ? sortedEnemies[0] : null;
        }

        protected virtual async UniTask AttackEnemyAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    EnemyController target = GetNearestEnemy();

                    if (target == null)
                    {
                        StopAttackLoop();
                        return;
                    }

                    Vector2 direction = (Vector2)target.transform.position - (Vector2)transform.position;
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    Quaternion rotation = Quaternion.Euler(0, 0, angle + _projectileRotationOffset);
                    
                    GameObject projectile = GetPooledObject();

                    if (projectile != null)
                    {
                        projectile.transform.position = transform.position;
                        projectile.transform.rotation = rotation;
                        projectile.SetActive(true);

                        if (projectile.TryGetComponent(out Rigidbody2D rb))
                        {
                            float speed = _weaponConfig != null ? _weaponConfig.WeaponSpeed : 10f;
                            rb.linearVelocity = direction.normalized * speed;
                        }
                        else
                        {
                            Debug.LogWarning($"{nameof(ProjectileSpawner)} spawned projectile without Rigidbody2D.", this);
                            projectile.SetActive(false);
                        }
                    }
                    
                    float delay = GetEffectiveAttackSpeed();
                    await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: token);
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
