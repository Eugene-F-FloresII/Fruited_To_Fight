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
        
        private int _currentPierce;
        private float _currentDamage;
        private float _currentSpeed;
        private bool _canHome;
        private Rigidbody2D _weaponRb;
        
        private const float ProjectileRotationOffset = -90f;

        private CancellationTokenSource _despawnCts;
        
        
        private void Awake()
        {
            _weaponRb = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            UpdateWeaponStats();

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

            if (_weaponRb != null)
            {
                _weaponTriggerRb.linearVelocity = Vector2.zero;
                _weaponTriggerRb.angularVelocity = 0f;
                _weaponRb.linearVelocity = Vector2.zero;
                _weaponRb.angularVelocity = 0f;
            }
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
                EnemyController enemy = _weaponTriggerHoming.Enemies.Find(e => e != null);
                if (enemy == null) return;
                
                Vector2 direction = enemy.transform.position - transform.position;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                Quaternion rotation = Quaternion.Euler(0, 0, angle + ProjectileRotationOffset);
                
                _weaponRb.linearVelocity = direction.normalized * _currentSpeed;
                _weaponTriggerRb.linearVelocity = direction.normalized * _currentSpeed;
                gameObject.transform.rotation = rotation;
            }
            
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
