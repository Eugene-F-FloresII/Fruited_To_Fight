using System;
using System.Threading;
using Controllers;
using Data;
using Obvious.Soap;
using UnityEngine;

namespace Gameplay.Weapons
{
    public class WeaponSpear : MonoBehaviour
    {
        [Header("Weapon Config")]
        [SerializeField] private WeaponConfig _weaponConfig;


        [Header("Weapon References")] 
        [SerializeField] private WeaponTriggerHoming _weaponTriggerHoming;
        
        private int _currentPierce;
        private readonly float _projectileRotationOffset = -90f;
        private float _currentDamage;
        private float _currentSpeed;
        private bool _canHome;
        private Rigidbody2D _weaponRb;
        

        private CancellationTokenSource _despawnCts;
        
        
        private async void Awake()
        {
           UpdateWeaponStats();
           _despawnCts = new CancellationTokenSource();
           await DespawnSpear(_despawnCts.Token);
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
                    _despawnCts?.Cancel();
                    Destroy(gameObject);
                }
            }
        }

        private void UpdateWeaponStats()
        {
            _currentDamage = _weaponConfig.WeaponDamage;
            _currentPierce = _weaponConfig.WeaponPierce;
            _currentSpeed = _weaponConfig.WeaponSpeed;
            _canHome = _weaponConfig.WeaponHoming;
            _weaponRb = GetComponent<Rigidbody2D>();
        }
        
        private void OnPierceValueChanged()
        {
            if (_currentPierce < _weaponConfig.WeaponPierce && _canHome)
            {
                EnemyController enemy = _weaponTriggerHoming.Enemies.Find(e => e != null);
                if (enemy == null) return;
                
                Vector2 direction = enemy.transform.position - transform.position;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                Quaternion rotation = Quaternion.Euler(0, 0, angle + _projectileRotationOffset);
                
                _weaponRb.linearVelocity = direction.normalized * _currentSpeed;
                gameObject.transform.rotation = rotation;
            }
            
        }
        
        async Awaitable DespawnSpear(CancellationToken token)
        {
            try
            {
                await Awaitable.WaitForSecondsAsync(5,  token);
                Destroy(gameObject);
                Debug.Log("Spear despawned after timeout.");

            }
            catch (OperationCanceledException)
            {
                Debug.Log("Spawn despawn cancelled");
            }
        }
    }

}
