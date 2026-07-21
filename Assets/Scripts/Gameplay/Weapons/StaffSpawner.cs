using System;
using System.Collections.Generic;
using System.Threading;
using Controllers;
using Data;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Shared.Events;

namespace Gameplay.Weapons
{
    /// <summary>
    /// Handles detecting enemies in range and controlling the firing loop for Staff fireballs.
    /// </summary>
    public class StaffSpawner : ProjectileSpawner
    {
        [Header("Audio")] 
        [SerializeField] private AudioClip _audioClip;

        [Header("Burst Settings")] 
        [SerializeField] private float _burstDelay = 0.2f;
        
        /// <summary>
        /// Asynchronously executes the attack loop spawning fireballs target-by-target.
        /// </summary>
        /// <param name="token">Cancellation token for cleaning up async task on destroy.</param>
        protected override async UniTask AttackEnemyAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    int weaponLevel = (_weaponConfig != null && _weaponConfig.WeaponLevel != null) 
                        ? _weaponConfig.WeaponLevel.Value 
                        : 0;
                    int projectileCount = weaponLevel switch
                    {
                        0 => 1,
                        1 => 2,
                        2 => 3,
                        3 => 5,
                        _ => 6 // Level 4 and above
                    };

                    for (int i = 0; i < projectileCount; i++)
                    {
                        List<EnemyController> enemies = GetSortedEnemies();

                        if (enemies.Count == 0)
                        {
                            StopAttackLoop();
                            return;
                        }

                        // Cycle through enemies: Shot 1 -> nearest, Shot 2 -> second nearest, etc.
                        EnemyController target = enemies[i % enemies.Count];
                        if (target == null) continue;

                        Vector2 direction = (Vector2)target.transform.position - (Vector2)transform.position;
                        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                        Quaternion rotation = Quaternion.Euler(0, 0, angle + _projectileRotationOffset);
                        
                        GameObject fireball = GetPooledObject();

                        if (fireball != null)
                        {
                            fireball.transform.position = transform.position;
                            fireball.transform.rotation = rotation;
                            fireball.SetActive(true);
                            
                            Events_Sound.PlaySound?.Invoke(_audioClip);

                            if (fireball.TryGetComponent(out Rigidbody2D rb))
                            {
                                float speed = _weaponConfig != null ? _weaponConfig.WeaponSpeed : 15f;
                                rb.linearVelocity = direction.normalized * speed;
                            }
                            else
                            {
                                Debug.LogWarning($"{nameof(StaffSpawner)} spawned projectile without Rigidbody2D.", this);
                                fireball.SetActive(false);
                            }
                        }

                        if (i < projectileCount - 1)
                        {
                            float effectiveBurstDelay = Mathf.Max(0.05f, _burstDelay);
                            await UniTask.Delay(TimeSpan.FromSeconds(effectiveBurstDelay), cancellationToken: token);
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
