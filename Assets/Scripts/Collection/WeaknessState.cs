using Controllers;
using Data;
using Shared.Events;
using Shared.Enums;

namespace Collection
{
    public class WeaknessState : AfflictionState
    {
        /// <summary>
        /// Initializes the weakness state and applies instant 10% HP damage.
        /// </summary>
        public override void Initialize(EnemyController enemy, AfflictionConfig config)
        {
            base.Initialize(enemy, config);
            ApplyWeakness();
        }

        /// <summary>
        /// Applies 10% of current HP as damage to the enemy.
        /// </summary>
        private void ApplyWeakness()
        {
            if (Enemy != null)
            {
                float damage = Enemy.CurrentHealth * 0.1f;
                Enemy.TakeDamage(damage, DamageSourceInfo.FromAffliction(AfflictionType.Weakness));
            }
        }
    }
}
