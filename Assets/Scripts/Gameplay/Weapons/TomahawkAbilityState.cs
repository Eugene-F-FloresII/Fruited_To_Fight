using System;
using System.Collections.Generic;
using System.Threading;
using Collection;
using Controllers;
using Cysharp.Threading.Tasks;
using Data;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.Weapons
{
    public class TomahawkAbilityState : WeaponAbilityState
    {
        [Header("Spin Settings")] 
        [SerializeField] private float _spinRadius = 2.5f;
        [SerializeField] private float _spinSpeed = 300f;
        [SerializeField] private float _projectileScale = 2f;
        [SerializeField] private Transform _target;

        private readonly List<GameObject> _activeProjectiles = new List<GameObject>();

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<TomahawkAbilityState>();
            _abilityCts?.Cancel();
            _abilityCts?.Dispose();
            _cooldownCts?.Cancel();
            _cooldownCts?.Dispose();
            CleanupProjectiles();
        }
        
        [Button("Test Weapon Ability")]
        public override void UseWeaponAbility()
        {
            if (_weaponConfig == null)
            {
                Debug.LogError("WeaponConfig is NULL on TomahawkAbilityState!", this);
                return;
            }

            Debug.Log($"[TomahawkAbility] Triggered! Duration: {_weaponConfig.AbilityDuration}, Target: {(_target != null ? _target.name : "NULL")}", this);
            
            _abilityCts?.Cancel();
            _abilityCts?.Dispose();
            _abilityCts = new CancellationTokenSource();
            
            UseWeaponAbilityAsync(_abilityCts.Token).Forget();
        }

        public override async UniTask UseWeaponAbilityAsync(CancellationToken token)
        {
            try
            {
                Debug.Log("[TomahawkAbility] Starting Async Task", this);
                SpawnProjectiles();

                float duration = _weaponConfig.AbilityDuration;
                float startTime = Time.time;
                float currentAngle = 0f;

                if (_target == null)
                {
                    Debug.LogWarning("[TomahawkAbility] Target is NULL, ability will not move!", this);
                }

                while (Time.time - startTime < duration && !token.IsCancellationRequested)
                {
                    if (_target == null) break;

                    currentAngle += _spinSpeed * Time.deltaTime;
                    UpdateProjectilesPosition(currentAngle);

                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
                
                Debug.Log("[TomahawkAbility] Ability loop finished", this);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("[TomahawkAbility] Ability cancelled", this);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                CleanupProjectiles();
                
                _cooldownCts?.Cancel();
                _cooldownCts?.Dispose();
                _cooldownCts = new CancellationTokenSource();
                WeaponAbilityCooldown(_cooldownCts.Token).Forget();
            }
        }

        public override async UniTask WeaponAbilityCooldown(CancellationToken token)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_weaponConfig.AbilityCooldown), cancellationToken: token);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void SpawnProjectiles()
        {
            CleanupProjectiles();

            for (int i = 0; i < 4; i++)
            {
                GameObject projectile = Instantiate(_weaponConfig.WeaponPrefab);
                projectile.transform.localScale = Vector3.one * _projectileScale;
                
                // Disable original ProjectileWeapon to stop its autonomous movement/despawn
                if (projectile.TryGetComponent(out ProjectileWeapon weapon))
                {
                    weapon.enabled = false;
                    
                    // Add a simple damage handler that uses the weapon's stats
                    var damageHandler = projectile.AddComponent<TomahawkAbilityDamageHandler>();
                    damageHandler.Initialize(_weaponConfig, weapon, _percentageIncrease);
                }

                if (projectile.TryGetComponent(out Rigidbody2D rb))
                {
                    rb.bodyType = RigidbodyType2D.Kinematic;
                    rb.simulated = true; 
                }

                _activeProjectiles.Add(projectile);
            }
        }

        private void UpdateProjectilesPosition(float baseAngle)
        {
            for (int i = 0; i < _activeProjectiles.Count; i++)
            {
                if (_activeProjectiles[i] == null) continue;

                float angle = (baseAngle + (i * 90f)) * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * _spinRadius;
                _activeProjectiles[i].transform.position = _target.position + offset;
                
                // Rotate to face the direction of rotation (tangent)
                float rotationAngle = (baseAngle + (i * 90f) + 90f);
                _activeProjectiles[i].transform.rotation = Quaternion.Euler(0, 0, rotationAngle);
            }
        }

        private void CleanupProjectiles()
        {
            foreach (var proj in _activeProjectiles)
            {
                if (proj != null)
                {
                    Destroy(proj);
                }
            }
            _activeProjectiles.Clear();
        }
        
        
    }

    /// <summary>
    /// Simple helper to handle damage for ability projectiles without the autonomous movement of ProjectileWeapon.
    /// </summary>
    public class TomahawkAbilityDamageHandler : MonoBehaviour
    {
        private WeaponConfig _config;
        private ProjectileWeapon _weapon;
        private float _percentageIncrease;

        public void Initialize(WeaponConfig config, ProjectileWeapon weapon, float percentage)
        {
            _config = config;
            _weapon = weapon;
            _percentageIncrease = percentage;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out EnemyController enemy))
            {
                var damage = _config.WeaponDamage * _percentageIncrease;
                enemy.TakeDamage(damage, _weapon);
            }
        }
    }
}
