using Controllers;
using Data;
using Shared.Enums;
using UnityEngine;
using Gameplay.Enemies;

namespace Collection
{
    public abstract class AfflictionState : MonoBehaviour
    {
        public AfflictionType AfflictionType { get; protected set; }
        protected EnemyController Enemy;
        protected AfflictionConfig Config;
        protected float RemainingDuration;
        protected EnemyAffliction VisualController;
        protected int CurrentStacks;

        public virtual void Initialize(EnemyController enemy, AfflictionConfig config)
        {
            Enemy = enemy;
            Config = config;
            AfflictionType = config.Type;
            RemainingDuration = config.Duration;
            CurrentStacks = 1;

            VisualController = enemy.GetComponentInChildren<EnemyAffliction>();
            if (VisualController != null)
            {
                VisualController.ToggleVisual(AfflictionType, true);
            }
        }

        public virtual void Refresh(AfflictionConfig config)
        {
            // If the new config is the same type but has different values, we update it
            Config = config;
            RemainingDuration = config.Duration;
            
            CurrentStacks = Mathf.Min(CurrentStacks + 1, Config.MaxStacks);
            OnStackAdded();
        }

        protected virtual void OnStackAdded() { }

        protected virtual void Update()
        {
            if (RemainingDuration > 0)
            {
                RemainingDuration -= Time.deltaTime;
                if (RemainingDuration <= 0)
                {
                    OnDurationExpired();
                }
            }
        }

        protected virtual void OnDurationExpired()
        {
            Destroy(this);
        }

        protected virtual void OnDestroy()
        {
            if (VisualController != null)
            {
                VisualController.ToggleVisual(AfflictionType, false);
            }
        }
    }

}

