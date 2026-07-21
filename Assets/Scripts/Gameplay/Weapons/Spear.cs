using UnityEngine;
using Controllers;
using Collection;
using Shared.Events;
using Shared.Enums;

namespace Gameplay.Weapons
{
    /// <summary>
    /// Represents the Spear melee weapon that performs periodic directional thrust attacks.
    /// </summary>
    public class Spear : MeleeWeapon
    {
        [Header("Spear Combat Settings")]
        [SerializeField] private LayerMask _enemyLayerMask;

        /// <summary>
        /// Performs a thrust attack in the specified facing direction.
        /// </summary>
        /// <param name="facingDirection">The direction the player is facing.</param>
        protected override void PerformAttack(Vector2 facingDirection)
        {
            base.PerformAttack(facingDirection);

            if (_weaponConfig == null) return;

            // Define hit box dimensions (narrow long thrust)
            float range = _weaponConfig.WeaponRange;
            float width = range * 0.35f;

            // Center of the box is half the range away from the player in the facing direction
            Vector2 hitPosition = (Vector2)transform.position + facingDirection * (range / 2f);
            Vector2 boxSize = new Vector2(range, width);
            float angle = Mathf.Atan2(facingDirection.y, facingDirection.x) * Mathf.Rad2Deg;

            // Detect enemies within the forward thrust box
            Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(hitPosition, boxSize, angle, _enemyLayerMask);

            foreach (Collider2D collider in hitEnemies)
            {
                if (collider.TryGetComponent(out EnemyController enemy))
                {
                    // Apply afflictions
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

                    // Apply damage and knockback
                    enemy.TakeDamage(_weaponConfig.WeaponDamage, transform.position, _weaponConfig.WeaponKnockback, DamageSourceInfo.FromWeapon(_weaponConfig.WeaponClass, afflictionType));
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_weaponConfig == null) return;

            Gizmos.color = Color.cyan;
            Vector3 gizmoPos = transform.position;
            Vector2 facingDir = Vector2.right;

            if (Application.isPlaying)
            {
                var player = ServiceLocator.Get<PlayerController>();
                if (player != null)
                {
                    facingDir = player.FacingDirection;
                }
            }

            float range = _weaponConfig.WeaponRange;
            float width = range * 0.35f;
            Vector3 boxCenter = gizmoPos + (Vector3)facingDir * (range / 2f);

            Matrix4x4 oldMatrix = Gizmos.matrix;
            float angle = Mathf.Atan2(facingDir.y, facingDir.x) * Mathf.Rad2Deg;
            Gizmos.matrix = Matrix4x4.TRS(boxCenter, Quaternion.Euler(0, 0, angle), Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(range, width, 0.1f));
            Gizmos.matrix = oldMatrix;
        }
    }
}
