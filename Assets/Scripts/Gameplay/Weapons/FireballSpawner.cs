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
    public class FireballSpawner : ProjectileSpawner
    {
        [Header("Audio")] 
        [SerializeField] private AudioClip _audioClip;

        [Header("Burst Settings")] 
        [SerializeField] private float _burstDelay = 0.2f;
        
        protected override async UniTask AttackEnemyAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    int weaponLevel = _weaponConfig.WeaponLevel.Value;
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
                        var enemies = GetSortedEnemies();

                        if (enemies.Count == 0)
                        {
                            StopAttackLoop();
                            return;
                        }

                        // Cycle through enemies: Shot 1 -> nearest, Shot 2 -> second nearest, etc.
                        EnemyController target = enemies[i % enemies.Count];

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
                                rb.linearVelocity = direction.normalized * _weaponConfig.WeaponSpeed;
                            }
                            else
                            {
                                Debug.LogWarning($"{nameof(ProjectileSpawner)} spawned projectile without Rigidbody2D.", this);
                                fireball.SetActive(false);
                            }
                        }

                        if (i < projectileCount - 1)
                        {
                            await UniTask.Delay(TimeSpan.FromSeconds(_burstDelay), cancellationToken: token);
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
