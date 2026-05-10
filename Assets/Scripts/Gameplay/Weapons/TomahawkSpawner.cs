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
    public class TomahawkSpawner : ProjectileSpawner
    {
        [Header("Audio")] 
        [SerializeField] private AudioClip _audioClip;
        
        protected override async UniTask AttackEnemyAsync(CancellationToken token)
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

                    Vector2 directionToTarget = (Vector2)target.transform.position - (Vector2)transform.position;
                    float baseAngle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;

                    int weaponLevel = _weaponConfig.WeaponLevel.Value;
                    int projectileCount = weaponLevel == 0 ? 1 : 1 + (weaponLevel + 1) / 2;

                    for (int i = 0; i < projectileCount; i++)
                    {
                        float angleOffset = 0;
                        if (i > 0)
                        {
                            int multiplier = (i + 1) / 2;
                            angleOffset = (i % 2 != 0) ? 15f * multiplier : -15f * multiplier;
                        }

                        float finalAngle = baseAngle + angleOffset;
                        Quaternion rotation = Quaternion.Euler(0, 0, finalAngle + _projectileRotationOffset);
                        
                        GameObject tomahawk = GetPooledObject();

                        if (tomahawk != null)
                        {
                            tomahawk.transform.position = transform.position;
                            tomahawk.transform.rotation = rotation;
                            tomahawk.SetActive(true);
                            
                            Events_Sound.PlaySound?.Invoke(_audioClip);

                            if (tomahawk.TryGetComponent(out Rigidbody2D rb))
                            {
                                Vector2 direction = new Vector2(Mathf.Cos(finalAngle * Mathf.Deg2Rad), Mathf.Sin(finalAngle * Mathf.Deg2Rad));
                                rb.linearVelocity = direction * _weaponConfig.WeaponSpeed;
                            }
                            else
                            {
                                Debug.LogWarning($"{nameof(ProjectileSpawner)} spawned projectile without Rigidbody2D.", this);
                                tomahawk.SetActive(false);
                            }
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
