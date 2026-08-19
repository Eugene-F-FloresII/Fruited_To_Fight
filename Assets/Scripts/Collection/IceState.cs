using UnityEngine;

namespace Collection
{
    public class IceState : AfflictionState
    {
        public override void Initialize(Controllers.EnemyController enemy, Data.AfflictionConfig config)
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
                if (Enemy != null)
                {
                    Enemy.Freeze(Config.Power); // Using Power as freeze duration for Ice
                }
                CurrentStacks = 0;
            }
        }
    }
}
