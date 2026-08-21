using Controllers;
using Data;
using Shared.Enums;
using UnityEngine;

namespace Collection
{
    public abstract class AfflictionState
    {
        public AfflictionType AfflictionType { get; protected set; }
        public bool IsExpired => RemainingDuration <= 0;

        protected EnemyController Enemy;
        protected AfflictionConfig Config;
        protected float RemainingDuration;
        protected int CurrentStacks;

        /// <summary>
        /// Initializes the affliction state with the enemy and config.
        /// </summary>
        public virtual void Initialize(EnemyController enemy, AfflictionConfig config)
        {
            Enemy = enemy;
            Config = config;
            AfflictionType = config.Type;
            RemainingDuration = config.Duration;
            CurrentStacks = 1;
        }

        /// <summary>
        /// Refreshes the affliction with a new config, resetting duration and adding a stack.
        /// </summary>
        public virtual void Refresh(AfflictionConfig config)
        {
            Config = config;
            RemainingDuration = config.Duration;

            CurrentStacks = Mathf.Min(CurrentStacks + 1, Config.MaxStacks);
            OnStackAdded();
        }

        /// <summary>
        /// Called when a new stack is added via Refresh.
        /// </summary>
        protected virtual void OnStackAdded() { }

        /// <summary>
        /// Ticks the affliction state by deltaTime. Called by the handler each frame.
        /// </summary>
        public virtual void Tick(float deltaTime)
        {
            if (RemainingDuration > 0)
            {
                RemainingDuration -= deltaTime;
            }
        }

        /// <summary>
        /// Disposes the affliction state and performs cleanup.
        /// </summary>
        public virtual void Dispose() { }
    }
}
