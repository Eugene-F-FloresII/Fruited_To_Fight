using UnityEngine;
using Controllers;
using Collection;
using Shared.Events;
using Shared.Enums;

namespace Gameplay.Weapons
{
    public class Sword : MeleeWeapon
    {
        [Header("Sword Combat Settings")]
        [SerializeField] private LayerMask _enemyLayerMask;

        protected override void PerformAttack(Vector2 facingDirection)
        {
            base.PerformAttack(facingDirection);

            if (_weaponConfig == null) return;

            // Define hit box dimensions
            float range = _weaponConfig.WeaponRange;
            float width = range * 0.75f; // Proportional width

            // Center of the box is exactly half the range away from the player in the facing direction
            Vector2 hitPosition = (Vector2)transform.position + facingDirection * (range / 2f);
            Vector2 boxSize = new Vector2(range, width);
            float angle = Mathf.Atan2(facingDirection.y, facingDirection.x) * Mathf.Rad2Deg;

            // Detect enemies within the forward-facing box
            Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(hitPosition, boxSize, angle, _enemyLayerMask);

            foreach (Collider2D collider in hitEnemies)
            {
                if (collider.TryGetComponent(out EnemyController enemy))
                {
                    // Apply damage and knockback
                    enemy.TakeDamage(_weaponConfig.WeaponDamage, transform.position, _weaponConfig.WeaponKnockback, DamageSourceInfo.FromWeapon(_weaponConfig.WeaponClass));

                    // Apply afflictions
                    if (_weaponConfig.Afflictions != null)
                    {
                        foreach (var affliction in _weaponConfig.Afflictions)
                        {
                            enemy.ApplyAffliction(affliction);
                        }
                    }
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_weaponConfig == null) return;

            Gizmos.color = Color.red;
            Vector3 gizmoPos = transform.position;
            Vector2 facingDir = Vector2.right; // Default

            if (Application.isPlaying)
            {
                var player = ServiceLocator.Get<PlayerController>();
                if (player != null)
                {
                    facingDir = player.FacingDirection;
                }
            }

            float range = _weaponConfig.WeaponRange;
            float width = range * 0.75f;
            Vector3 boxCenter = gizmoPos + (Vector3)facingDir * (range / 2f);

            Matrix4x4 oldMatrix = Gizmos.matrix;
            float angle = Mathf.Atan2(facingDir.y, facingDir.x) * Mathf.Rad2Deg;
            Gizmos.matrix = Matrix4x4.TRS(boxCenter, Quaternion.Euler(0, 0, angle), Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(range, width, 0.1f));
            Gizmos.matrix = oldMatrix;
        }
    }
    
}
