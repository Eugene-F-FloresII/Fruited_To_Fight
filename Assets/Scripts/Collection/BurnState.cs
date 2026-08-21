using Controllers;
using Data;
using Shared.Events;
using Shared.Enums;
using UnityEngine;

namespace Collection
{
    public class BurnState : AfflictionState
    {
        private float _tickTimer;

        /// <summary>
        /// Initializes the burn state and resets the tick timer.
        /// </summary>
        public override void Initialize(EnemyController enemy, AfflictionConfig config)
        {
            base.Initialize(enemy, config);
            _tickTimer = 0f;
        }

        /// <summary>
        /// Ticks the burn state, applying damage every 1 second.
        /// </summary>
        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);

            if (RemainingDuration <= 0) return;

            _tickTimer += deltaTime;
            if (_tickTimer >= 1f)
            {
                _tickTimer -= 1f;
                if (Enemy != null && Enemy.gameObject.activeInHierarchy)
                {
                    Enemy.TakeDamage(Config.Power, DamageSourceInfo.FromAffliction(AfflictionType.Burn));
                }
            }
        }
    }
}
