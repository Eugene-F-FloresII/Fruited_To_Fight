using System;
using System.Collections.Generic;
using System.Threading;
using Controllers;
using Data;
using UnityEngine;

namespace Gameplay.Weapons
{
    public class WeaponHands : MonoBehaviour
    {
        [SerializeField] private WeaponConfig _weaponConfig;
        [SerializeField] private GameObject _weaponPrefab;
        [SerializeField] private float _projectileRotationOffset = -90f;
        
        [SerializeField] private List<EnemyController> _enemies = new();
        [SerializeField] private CircleCollider2D _circleCollider2D;
        
        private CancellationTokenSource _attackCts;

        private float _currentAtkSpeed;
        private float _currentRange;
        private void Awake()
        {
            _enemies ??= new List<EnemyController>();
            UpdateWeaponStats();
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
                    _ = AttackEnemyAsync(_attackCts.Token);
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
            _currentRange = _weaponConfig.WeaponRange;
            _circleCollider2D.radius = _currentRange;
            _currentAtkSpeed = _weaponConfig.WeaponAtkSpeed;
        }

        private void StopAttackLoop()
        {
            if (_attackCts == null)
            {
                return;
            }

            _attackCts.Cancel();
            _attackCts.Dispose();
            _attackCts = null;
        }

        private async Awaitable AttackEnemyAsync(CancellationToken token)
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

                    GameObject spawnedSpear = Instantiate(_weaponPrefab, transform.position, rotation);

                    if (spawnedSpear.TryGetComponent(out Rigidbody2D rb))
                    {
                        rb.linearVelocity = direction.normalized * _weaponConfig.WeaponSpeed;
                    }
                    else
                    {
                        Debug.LogWarning($"{nameof(WeaponHands)} spawned projectile without Rigidbody2D.", this);
                        Destroy(spawnedSpear);
                    }

                    await Awaitable.WaitForSecondsAsync(Mathf.Max(0.01f, _currentAtkSpeed), token);
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
