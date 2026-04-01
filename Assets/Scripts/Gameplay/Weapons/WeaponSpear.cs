using System;
using System.Threading;
using Controllers;
using Cysharp.Threading.Tasks;
using Data;
using UnityEngine;

namespace Gameplay.Weapons
{
    public class WeaponSpear : MonoBehaviour
    {
        [Header("Weapon Config")]
        [SerializeField] private WeaponConfig _weaponConfig;
        
        [Header("Weapon References")] 
        [SerializeField] private WeaponTriggerHoming _weaponTriggerHoming;
        [SerializeField] private Rigidbody2D _weaponTriggerRb;
        [SerializeField] private float _homingTurnSpeed = 540f;
        [SerializeField] private float _retargetInterval = 0.08f;
        
        private int _currentPierce;
        private float _currentDamage;
        private float _currentSpeed;
        private bool _canHome;
        private Rigidbody2D _weaponRb;
        private EnemyController _currentHomingTarget;
        private bool _isHoming;
        private float _nextRetargetTime;
        
        private const float ProjectileRotationOffset = -90f;

        private CancellationTokenSource _despawnCts;
        
        
        private void Awake()
        {
            _weaponRb = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            UpdateWeaponStats();
            _currentHomingTarget = null;
            _isHoming = false;
            _nextRetargetTime = 0f;

            if (_weaponRb != null)
            {
                _weaponTriggerRb.linearVelocity = Vector2.zero;
                _weaponTriggerRb.angularVelocity = 0f;
                _weaponRb.linearVelocity = Vector2.zero;
                _weaponRb.angularVelocity = 0f;
            }

            StopDespawnTimer();
            _despawnCts = new CancellationTokenSource();
            DespawnSpear(_despawnCts.Token).Forget();
        }

        private void OnDisable()
        {
            StopDespawnTimer();
            _currentHomingTarget = null;
            _isHoming = false;
            _nextRetargetTime = 0f;

            if (_weaponRb != null)
            {
                _weaponTriggerRb.linearVelocity = Vector2.zero;
                _weaponTriggerRb.angularVelocity = 0f;
                _weaponRb.linearVelocity = Vector2.zero;
                _weaponRb.angularVelocity = 0f;
            }
        }

        private void FixedUpdate()
        {
           HomingWeapon();
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out EnemyController enemy))
            { 
                enemy.TakeDamage(_currentDamage);
                _currentPierce--;
                OnPierceValueChanged();
                
                if (_currentPierce <= 0)
                {
                    gameObject.SetActive(false);
                }
            }
        }

        private void UpdateWeaponStats()
        {
            _currentDamage = _weaponConfig.WeaponDamage;
            _currentPierce = _weaponConfig.WeaponPierce;
            _currentSpeed = _weaponConfig.WeaponSpeed;
            _canHome = _weaponConfig.WeaponHoming;
        }

        private void StopDespawnTimer()
        {
            if (_despawnCts == null)
            {
                return;
            }

            _despawnCts.Cancel();
            _despawnCts.Dispose();
            _despawnCts = null;
        }
        
        private void OnPierceValueChanged()
        {
            if (_currentPierce < _weaponConfig.WeaponPierce && _canHome)
            {
                _currentHomingTarget = GetNearestEnemy();
                _isHoming = _currentHomingTarget != null;
            }
        }

        private void HomingWeapon()
        {
            if (!_canHome || !_isHoming || _weaponRb == null || _weaponTriggerRb == null)
            {
                return;
            }

            bool canRunRangeCheck = Time.time >= _nextRetargetTime;
            if (canRunRangeCheck)
            {
                _nextRetargetTime = Time.time + _retargetInterval;
            }

            bool targetInvalid = !IsTargetAlive(_currentHomingTarget) || (canRunRangeCheck && !IsTargetInRangeList(_currentHomingTarget));
            if (targetInvalid)
            {
                _currentHomingTarget = GetNearestEnemy();

                if (_currentHomingTarget == null)
                {
                    _isHoming = false;
                    return;
                }
            }

            Vector2 direction = (Vector2)_currentHomingTarget.transform.position - _weaponRb.position;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            float desiredAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float targetRotation = desiredAngle + ProjectileRotationOffset;
            float currentRotation = _weaponRb.rotation;
            float nextRotation = Mathf.MoveTowardsAngle(currentRotation, targetRotation, _homingTurnSpeed * Time.fixedDeltaTime);

            _weaponRb.MoveRotation(nextRotation);

            Vector2 velocity = (Vector2)(Quaternion.Euler(0f, 0f, nextRotation) * Vector3.up) * _currentSpeed;
            _weaponRb.linearVelocity = velocity;
            _weaponTriggerRb.linearVelocity = velocity;
        }

        private bool IsTargetAlive(EnemyController enemy)
        {
            return enemy != null && enemy.gameObject.activeInHierarchy;
        }

        private bool IsTargetInRangeList(EnemyController enemy)
        {
            if (enemy == null || _weaponTriggerHoming == null || _weaponTriggerHoming.Enemies == null)
            {
                return false;
            }

            return _weaponTriggerHoming.Enemies.Contains(enemy);
        }

        private EnemyController GetNearestEnemy()
        {
            if (_weaponTriggerHoming == null || _weaponTriggerHoming.Enemies == null)
            {
                return null;
            }

            EnemyController nearestEnemy = null;
            float nearestDistanceSqr = float.MaxValue;
            Vector2 spearPosition = _weaponRb != null ? _weaponRb.position : (Vector2)transform.position;

            int enemyCount = _weaponTriggerHoming.Enemies.Count;
            for (int i = 0; i < enemyCount; i++)
            {
                EnemyController enemy = _weaponTriggerHoming.Enemies[i];
                if (enemy == null || !enemy.gameObject.activeInHierarchy)
                {
                    continue;
                }

                Vector2 delta = (Vector2)enemy.transform.position - spearPosition;
                float distanceSqr = delta.sqrMagnitude;

                if (distanceSqr < nearestDistanceSqr)
                {
                    nearestDistanceSqr = distanceSqr;
                    nearestEnemy = enemy;
                }
            }

            return nearestEnemy;
        }
        
        private async UniTask DespawnSpear(CancellationToken token)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(3.5), cancellationToken: token);
                gameObject.SetActive(false);

            }
            catch (OperationCanceledException)
            {
            }
        }
    }

}
