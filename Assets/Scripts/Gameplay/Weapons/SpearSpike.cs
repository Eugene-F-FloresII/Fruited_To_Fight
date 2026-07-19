using System;
using System.Collections.Generic;
using UnityEngine;
using Controllers;
using Data;
using Shared.Enums;
using Shared.Events;
using PrimeTween;

namespace Gameplay.Weapons
{
    /// <summary>
    /// A temporary physical spike trigger that deals damage and knockback to enemies,
    /// animates its scaling using PrimeTween, and self-destructs.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class SpearSpike : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CircleCollider2D _circleCollider;
        [SerializeField] private Rigidbody2D _rigidbody2D;
        [SerializeField] private SpriteRenderer _spriteRenderer;

        [Header("Settings")]
        [SerializeField] private float _lifetime = 0.6f;
        [SerializeField] private float _animationDuration = 0.15f;

        private WeaponConfig _weaponConfig;
        private float _percentageIncrease;
        private readonly HashSet<EnemyController> _hitEnemies = new HashSet<EnemyController>();
        private Sequence _activeSequence;

        private void Awake()
        {
            if (_circleCollider == null) _circleCollider = GetComponent<CircleCollider2D>();
            if (_rigidbody2D == null) _rigidbody2D = GetComponent<Rigidbody2D>();
            if (_spriteRenderer == null) _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            _circleCollider.isTrigger = true;
            _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
            _rigidbody2D.simulated = true;
        }

        private void OnDisable()
        {
            _activeSequence.Stop();
        }

        /// <summary>
        /// Initializes the spike with weapon config, ability multiplier, and radius.
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

            // Run the punch/pop scale animation using PrimeTween
            transform.localScale = Vector3.zero;
            _activeSequence = Sequence.Create()
                .Group(Tween.Scale(transform, Vector3.one, _animationDuration, Ease.OutBack))
                .Chain(Tween.Delay(Mathf.Max(0f, _lifetime - (2 * _animationDuration))))
                .Chain(Tween.Scale(transform, Vector3.zero, _animationDuration, Ease.InBack))
                .OnComplete(() => Destroy(gameObject));
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_weaponConfig == null) return;

            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy == null)
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
                AfflictionType afflictionType = AfflictionType.None;
                if (_weaponConfig.Afflictions != null && _weaponConfig.Afflictions.Count > 0)
                {
                    afflictionType = _weaponConfig.Afflictions[0].Type;
                }

                // Apply damage and knockback
                enemy.TakeDamage(damage, transform.position, _weaponConfig.WeaponKnockback, DamageSourceInfo.FromWeapon(WeaponClass.Spear, afflictionType));
            }
        }
    }
}
