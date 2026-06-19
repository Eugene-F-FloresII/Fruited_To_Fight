


using Shared.Events;
using Shared.Enums;

namespace Collection
{
    public class WeaknessState : AfflictionState
    {
        public override void Initialize(Controllers.EnemyController enemy, Data.AfflictionConfig config)
        {
            base.Initialize(enemy, config);
            ApplyWeakness();
        }

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
