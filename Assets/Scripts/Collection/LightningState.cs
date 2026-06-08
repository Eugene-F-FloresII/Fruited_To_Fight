using UnityEngine;
using Controllers;

namespace Collection
{
    public class LightningState : AfflictionState
    {
        [SerializeField] private GameObject _lightningExplosionPrefab;

        public override void Initialize(EnemyController enemy, Data.AfflictionConfig config)
        {
            base.Initialize(enemy, config);
            CheckStacks();
        }

        protected override void OnStackAdded()
        {
            CheckStacks();
        }

        private void CheckStacks()
        {
            if (CurrentStacks >= Config.MaxStacks)
            {
                TriggerLightningStrike();
                CurrentStacks = 0;
            }
        }

        private void TriggerLightningStrike()
        {
            if (_lightningExplosionPrefab != null)
            {
                Instantiate(_lightningExplosionPrefab, transform.position, Quaternion.identity);
            }

            float damage = Config.Power * 4f; // Power + 300% = 400% (Power * 4)
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, Config.ExplosionRadius);

            foreach (var hitCollider in hitEnemies)
            {
                if (hitCollider.TryGetComponent(out EnemyController enemy))
                {
                    enemy.TakeDamage(damage);
                }
            }
        }
    }
}
