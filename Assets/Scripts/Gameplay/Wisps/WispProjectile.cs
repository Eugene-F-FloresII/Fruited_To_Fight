using System;
using System.Collections.Generic;
using Controllers;
using UnityEngine;

namespace Gameplay.Wisps
{
    public abstract class WispProjectile : MonoBehaviour
    {
        [Header("Wisp Projectile Settings")]
        [SerializeField] protected float Damage;
        [SerializeField] protected float Range;
        
        protected List<EnemyController> EnemyInRange;
        protected HashSet<EnemyController> EnemyAlreadyHit;

        public abstract void OnTriggerEnter(Collider other);
        public abstract void OnTriggerExit(Collider other);
        
        public abstract void FireProjectile(Transform target);

    }

}
