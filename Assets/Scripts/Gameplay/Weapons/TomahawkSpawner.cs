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

                    Vector2 direction = (Vector2)target.transform.position - (Vector2)transform.position;
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    Quaternion rotation = Quaternion.Euler(0, 0, angle + _projectileRotationOffset);
                    
                    GameObject spear = GetPooledObject();

                    if (spear != null)
                    {
                        spear.transform.position = transform.position;
                        spear.transform.rotation = rotation;
                        spear.SetActive(true);
                        
                        Events_Sound.PlaySound?.Invoke(_audioClip);

                        if (spear.TryGetComponent(out Rigidbody2D rb))
                        {
                            rb.linearVelocity = direction.normalized * _weaponConfig.WeaponSpeed;
                        }
                        else
                        {
                            Debug.LogWarning($"{nameof(ProjectileSpawner)} spawned projectile without Rigidbody2D.", this);
                            spear.SetActive(false);
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
