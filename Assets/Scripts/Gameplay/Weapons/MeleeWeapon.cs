using System;
using System.Collections.Generic;
using System.Threading;
using Collection;
using Controllers;
using Cysharp.Threading.Tasks;
using Data;
using UnityEngine;

namespace Gameplay.Weapons
{
    public abstract class MeleeWeapon : MonoBehaviour
    {
        [Header("Weapon Config")]
        [SerializeField] protected WeaponConfig _weaponConfig;
        
        [Header("Object Pooling Settings")]
        [SerializeField] protected Transform _pooledTransform;
        
        protected float _currentAtkSpeed;
        private Queue<GameObject> _pooledObjects;
        private CancellationTokenSource _attackCts;
        private PlayerController _playerController;

        protected virtual void Awake()
        {
            UpdateWeaponStats();
        }

        protected virtual void Start()
        {
            _playerController = ServiceLocator.Get<PlayerController>();
            PoolObjects();
            StartAttackLoop();
        }

        protected virtual void OnDestroy()
        {
            StopAttackLoop();
        }

        private void UpdateWeaponStats()
        {
            if (_weaponConfig != null)
            {
                _currentAtkSpeed = _weaponConfig.WeaponAtkSpeed;
            }
        }

        private void PoolObjects()
        {
            _pooledObjects = new Queue<GameObject>();
            if (_weaponConfig == null || _weaponConfig.WeaponPrefab == null) return;

            for (int i = 0; i < _weaponConfig.WeaponAmountToPool; i++)
            {
                GameObject pool = Instantiate(_weaponConfig.WeaponPrefab, _pooledTransform);
                pool.SetActive(false);
                _pooledObjects.Enqueue(pool);
            }
        }

        protected void StartAttackLoop()
        {
            StopAttackLoop();
            _attackCts = new CancellationTokenSource();
            AttackLoopAsync(_attackCts.Token).Forget();
        }

        protected void StopAttackLoop()
        {
            if (_attackCts != null)
            {
                _attackCts.Cancel();
                _attackCts.Dispose();
                _attackCts = null;
            }
        }

        private async UniTask AttackLoopAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (_playerController != null)
                    {
                        PerformAttack(_playerController.FacingDirection);
                    }
                    await UniTask.Delay(TimeSpan.FromSeconds(Mathf.Max(0.01f, _currentAtkSpeed)), cancellationToken: token);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Debug.LogException(ex, this);
            }
        }

        public GameObject GetPooledObject()
        {
            if (_pooledObjects == null || _pooledObjects.Count == 0) return null;

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

        protected virtual void PerformAttack(Vector2 facingDirection)
        {
            GameObject slash = GetPooledObject();
            if (slash != null)
            {
                // Dynamically scale the visual based on WeaponRange
                // Using 3f as a baseline range for 1.0x scale.
                float baseRangeForScale = 3f;
                float scaleFactor = _weaponConfig != null && _weaponConfig.WeaponRange > 0 
                    ? _weaponConfig.WeaponRange / baseRangeForScale 
                    : 1f;

                slash.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);

                // Position the slash in front of the player, offset scaled by the range factor
                slash.transform.position = transform.position + (Vector3)facingDirection * (0.7f * scaleFactor); 
                
                // Rotate the slash to face the attack direction
                float angle = Mathf.Atan2(facingDirection.y, facingDirection.x) * Mathf.Rad2Deg;
                slash.transform.rotation = Quaternion.Euler(0, 0, angle);
                
                slash.SetActive(true);
            }
        }
    }
}
