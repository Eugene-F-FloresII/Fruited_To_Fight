using System;
using System.Threading;
using Controllers;
using Cysharp.Threading.Tasks;
using Data;
using Shared.Events;
using Shared.Enums;
using UnityEngine;

namespace Collection
{
    public class LightningState : AfflictionState
    {
        private CancellationTokenSource _lightningCts;

        /// <summary>
        /// Initializes the lightning state and checks stacks for a strike.
        /// </summary>
        public override void Initialize(EnemyController enemy, AfflictionConfig config)
        {
            base.Initialize(enemy, config);
            _lightningCts = new CancellationTokenSource();
            CheckStacks();
        }

        /// <summary>
        /// Called when a new stack is added, checks if max stacks reached for a strike.
        /// </summary>
        protected override void OnStackAdded()
        {
            CheckStacks();
        }

        /// <summary>
        /// Checks if current stacks reached max and triggers a lightning strike.
        /// </summary>
        private void CheckStacks()
        {
            if (CurrentStacks >= Config.MaxStacks)
            {
                TriggerLightningStrikeAsync().Forget();
                CurrentStacks = 0;
            }
        }

        /// <summary>
        /// Triggers an AoE lightning strike after a delay.
        /// </summary>
        private async UniTaskVoid TriggerLightningStrikeAsync()
        {
            Events_VFX.SpawnVFXEvent?.Invoke(Config.VFXPrefabReference, Enemy.transform.position, Quaternion.identity, Vector3.one, 1f + Config.LightningStrikeDelay);

            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(Config.LightningStrikeDelay), cancellationToken: _lightningCts.Token);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            if (Enemy == null || !Enemy.gameObject.activeInHierarchy) return;

            float damage = Config.Power * 4f;
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(Enemy.transform.position, Config.ExplosionRadius);

            foreach (var hitCollider in hitEnemies)
            {
                if (hitCollider.TryGetComponent(out EnemyController enemy))
                {
                    enemy.TakeDamage(damage, DamageSourceInfo.FromAffliction(AfflictionType.Lightning));
                    
                    if (Config.HitEffectPrefab != null && Config.HitEffectPrefab.RuntimeKeyIsValid())
                    {
                        Events_VFX.SpawnVFXEvent?.Invoke(Config.HitEffectPrefab, enemy.transform.position, Quaternion.identity, enemy.transform.localScale, 1f);
                    }
                }
            }
        }

        /// <summary>
        /// Disposes the lightning state and cancels any pending lightning strike.
        /// </summary>
        public override void Dispose()
        {
            if (_lightningCts != null)
            {
                _lightningCts.Cancel();
                _lightningCts.Dispose();
                _lightningCts = null;
            }

            base.Dispose();
        }
    }
}
