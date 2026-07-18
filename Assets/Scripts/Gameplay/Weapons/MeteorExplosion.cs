using System;
using System.Collections.Generic;
using UnityEngine;
using Controllers;
using Data;
using Shared.Enums;
using Shared.Events;

namespace Gameplay.Weapons
{
    /// <summary>
    /// A temporary physical trigger collider that applies area damage and knockback to enemies entering its radius,
    /// then automatically destroys itself.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class MeteorExplosion : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CircleCollider2D _circleCollider;
        [SerializeField] private Rigidbody2D _rigidbody2D;

        [Header("Explosion Settings")]
        [SerializeField] private float _lifetime = 0.25f;

        private WeaponConfig _weaponConfig;
        private float _percentageIncrease;
        private readonly HashSet<EnemyController> _hitEnemies = new HashSet<EnemyController>();

        private void Awake()
        {
            if (_circleCollider == null)
            {
                _circleCollider = GetComponent<CircleCollider2D>();
            }
            if (_rigidbody2D == null)
            {
                _rigidbody2D = GetComponent<Rigidbody2D>();
            }

            // Set collider as trigger and rigidbody as kinematic for overlap trigger queries
            _circleCollider.isTrigger = true;
            _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
            _rigidbody2D.simulated = true;
        }

        private void Start()
        {
            // Destroy this temporary trigger object after its lifetime expires
            Destroy(gameObject, _lifetime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_weaponConfig == null) return;

            EnemyController enemy = null;
            if (other.TryGetComponent(out EnemyController e))
            {
                enemy = e;
            }
            else
            {
                enemy = other.GetComponentInParent<EnemyController>();
            }

            if (enemy != null && !_hitEnemies.Contains(enemy))
            {
                _hitEnemies.Add(enemy);
                
                if (_weaponConfig.Afflictions != null)
                {
                    foreach (var affliction in _weaponConfig.Afflictions)
                    {
                        enemy.ApplyAffliction(affliction);
                    }
                }

                float damage = _weaponConfig.WeaponDamage * _percentageIncrease;
                Debug.Log($"[MeteorExplosion] Trigger hit enemy '{enemy.gameObject.name}' for {damage} damage.");
                
                AfflictionType afflictionType = AfflictionType.None;
                if (_weaponConfig.Afflictions != null && _weaponConfig.Afflictions.Count > 0)
                {
                    afflictionType = _weaponConfig.Afflictions[0].Type;
                }

                enemy.TakeDamage(damage, transform.position, _weaponConfig.WeaponKnockback, DamageSourceInfo.FromWeapon(WeaponClass.Staff, afflictionType));
            }
        }

        /// <summary>
        /// Initializes the explosion trigger with weapon stats and the desired radius.
        /// </summary>
        public void Initialize(WeaponConfig config, float percentage, float radius)
        {
            _weaponConfig = config;
            _percentageIncrease = percentage;
            
            if (_circleCollider != null)
            {
                _circleCollider.radius = radius;
            }
            _hitEnemies.Clear();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.6f);
            Gizmos.DrawWireSphere(transform.position, _circleCollider != null ? _circleCollider.radius : 2.5f);
            Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
            Gizmos.DrawSphere(transform.position, _circleCollider != null ? _circleCollider.radius : 2.5f);
        }
    }
}
