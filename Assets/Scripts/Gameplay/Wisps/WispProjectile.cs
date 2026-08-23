using System;
using System.Collections.Generic;
using System.Linq;
using Controllers;
using Data;
using UnityEngine;

namespace Gameplay.Wisps
{
    public abstract class WispProjectile : MonoBehaviour
    {
        [Header("Wisp Projectile Settings")] 
        [SerializeField] protected WispConfig _wispConfig;
        [SerializeField] protected CircleCollider2D _circleCollider2D;
        
        [SerializeField] protected List<EnemyController> EnemyInRange = new List<EnemyController>();
        [SerializeField] protected List<EnemyController> EnemyAlreadyHit = new List<EnemyController>();

        protected float _lastFireTime;

        protected virtual void Update()
        {
            if (_wispConfig == null) return;

            if (Time.time > _lastFireTime + _wispConfig.AtkSpeed && EnemyInRange.Count > 0)
            {
                // Find the CLOSEST valid enemy still alive and in range
                EnemyController target = EnemyInRange
                    .Where(e => e != null && e.gameObject.activeInHierarchy)
                    .OrderBy(e => Vector2.Distance(transform.position, e.transform.position))
                    .FirstOrDefault();
                
                if (target != null)
                {
                    _lastFireTime = Time.time;
                    if (_wispConfig.AttackSfx != null)
                    {
                        Shared.Events.Events_Sound.PlaySoundWithVolume?.Invoke(_wispConfig.AttackSfx, _wispConfig.AttackSfxVolume);
                    }
                    FireProjectile(target);
                }
                else
                {
                    // Clean up null/inactive enemies from the list
                    EnemyInRange.RemoveAll(e => e == null || !e.gameObject.activeInHierarchy);
                }
            }
        }

        public abstract void OnTriggerEnter2D(Collider2D other);
        public abstract void OnTriggerExit2D(Collider2D other);
        
        public abstract void FireProjectile(EnemyController target);

    }

}
