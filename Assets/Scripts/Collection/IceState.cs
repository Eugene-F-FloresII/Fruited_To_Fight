using Controllers;
using Data;
using Gameplay.Enemies;
using UnityEngine;

namespace Collection
{
    public class IceState : AfflictionState
    {
        /// <summary>
        /// Initializes the ice state and checks stacks for a freeze.
        /// </summary>
        public override void Initialize(EnemyController enemy, AfflictionConfig config, EnemyAffliction visualController)
        {
            base.Initialize(enemy, config, visualController);
            CheckStacks();
        }

        /// <summary>
        /// Called when a new stack is added, checks if max stacks reached for a freeze.
        /// </summary>
        protected override void OnStackAdded()
        {
            CheckStacks();
        }

        /// <summary>
        /// Checks if current stacks reached max and freezes the enemy.
        /// </summary>
        private void CheckStacks()
        {
            if (CurrentStacks >= Config.MaxStacks)
            {
                if (Enemy != null)
                {
                    Enemy.Freeze(Config.Power);
                }
                CurrentStacks = 0;
            }
        }
    }
}
