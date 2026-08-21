using UnityEngine;
using Controllers;
using Cysharp.Threading.Tasks;
using System;
using Shared.Events;
using Shared.Enums;
using Data;

namespace Collection
{
    public class LightningState : AfflictionState
    {
        public override void Initialize(EnemyController enemy, AfflictionConfig config)
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
                TriggerLightningStrike().Forget();
                CurrentStacks = 0;
            }
        }

        private async UniTaskVoid TriggerLightningStrike()
        {
            Events_VFX.SpawnVFXEvent?.Invoke(Config.VFXPrefabReference, transform.position, Quaternion.identity, 1f + Config.LightningStrikeDelay);

            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(Config.LightningStrikeDelay), cancellationToken: this.GetCancellationTokenOnDestroy());
            }
            catch (OperationCanceledException)
            {
                return;
            }

            float damage = Config.Power * 4f; // Power + 300% = 400% (Power * 4)
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, Config.ExplosionRadius);

            foreach (var hitCollider in hitEnemies)
            {
                if (hitCollider.TryGetComponent(out EnemyController enemy))
                {
                    enemy.TakeDamage(damage, DamageSourceInfo.FromAffliction(AfflictionType.Lightning));
                }
            }
        }

       
    }
}
