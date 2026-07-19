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
    /// Refactored Spear Weapon. Now a MeleeWeapon that thrusts in the player's facing direction.
    /// When max level, it thrusts in 4 directions simultaneously.
    /// Limits hit targets up to WeaponPierce (sorted by proximity).
    /// </summary>
    public class Spear : MeleeWeapon
    {
        [Header("Spear Combat Settings")]
        [SerializeField] private LayerMask _enemyLayerMask;
        [SerializeField] private int _maxLevel = 5;

        protected override void PerformAttack(Vector2 facingDirection)
        {
            if (_weaponConfig == null) return;

            bool isMaxed = _weaponConfig.WeaponLevel != null && _weaponConfig.WeaponLevel.Value >= _maxLevel;

            if (!isMaxed)
            {
                Thrust(facingDirection);
            }
            else
            {
                Vector2 front = facingDirection;
                Vector2 back = -facingDirection;
                Vector2 up = new Vector2(-facingDirection.y, facingDirection.x);
                Vector2 down = new Vector2(facingDirection.y, -facingDirection.x);

                Thrust(front);
                Thrust(back);
                Thrust(up);
                Thrust(down);
            }
        }

        private void Thrust(Vector2 direction)
        {
            // 1. Spawn Visual Thrust Prefab
            GameObject visual = GetPooledObject();
            if (visual != null)
            {
                float baseRangeForScale = 3f;
                float scaleFactor = _weaponConfig.WeaponRange > 0 ? _weaponConfig.WeaponRange / baseRangeForScale : 1f;

                visual.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
                visual.transform.position = transform.position + (Vector3)direction * (0.7f * scaleFactor);

                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                visual.transform.rotation = Quaternion.Euler(0, 0, angle);

                visual.SetActive(true);

                // Set up affliction visuals on the visual object itself
                if (visual.TryGetComponent(out WeaponAffliction weaponAffliction))
                {
                    weaponAffliction.DisableAllVisuals();
                    if (_weaponConfig.Afflictions != null && _weaponConfig.Afflictions.Count > 0)
                    {
                        var primaryAffliction = _weaponConfig.Afflictions[0];
                        weaponAffliction.ToggleVisual(primaryAffliction.Type, true);
                    }
                }

                // If visual has a programmatic thrust animator, animate it
                if (visual.TryGetComponent(out SpearThrustVisual thrustVisual))
                {
                    thrustVisual.AnimateThrust(direction, _weaponConfig.WeaponRange, _weaponConfig.WeaponAtkSpeed);
                }
            }

            // 2. Perform Melee Hitbox Overlap check and apply damage
            if (_weaponConfig == null) return;

            float range = _weaponConfig.WeaponRange;
            float width = range * 0.4f; // Narrower poke width
            Vector2 hitPosition = (Vector2)transform.position + direction * (range / 2f);
            Vector2 boxSize = new Vector2(range, width);
            float dirAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(hitPosition, boxSize, dirAngle, _enemyLayerMask);

            // Sort unique enemies by distance to the player
            var enemyList = new List<(EnemyController enemy, float distance)>();
            var uniqueEnemies = new HashSet<EnemyController>();

            foreach (Collider2D collider in hitEnemies)
            {
                EnemyController enemy = collider.GetComponent<EnemyController>();
                if (enemy == null)
                {
                    enemy = collider.GetComponentInParent<EnemyController>();
                }

                if (enemy != null && uniqueEnemies.Add(enemy))
                {
                    float distance = Vector2.Distance(transform.position, enemy.transform.position);
                    enemyList.Add((enemy, distance));
                }
            }

            enemyList.Sort((a, b) => a.distance.CompareTo(b.distance));

            // Apply damage to up to WeaponPierce targets (repurposing Pierce as target limit)
            int targetsToHit = Mathf.Min(enemyList.Count, _weaponConfig.WeaponPierce);
            for (int i = 0; i < targetsToHit; i++)
            {
                var enemy = enemyList[i].enemy;

                if (_weaponConfig.Afflictions != null)
                {
                    foreach (var affliction in _weaponConfig.Afflictions)
                    {
                        enemy.ApplyAffliction(affliction);
                    }
                }

                AfflictionType afflictionType = AfflictionType.None;
                if (_weaponConfig.Afflictions != null && _weaponConfig.Afflictions.Count > 0)
                {
                    afflictionType = _weaponConfig.Afflictions[0].Type;
                }

                enemy.TakeDamage(_weaponConfig.WeaponDamage, transform.position, _weaponConfig.WeaponKnockback, DamageSourceInfo.FromWeapon(_weaponConfig.WeaponClass, afflictionType));
            }
        }
    }
}
