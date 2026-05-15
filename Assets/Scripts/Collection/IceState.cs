using UnityEngine;

namespace Collection
{
    public class IceState : AfflictionState
    {
        private int _currentStacks;

        public override void Initialize(Controllers.EnemyController enemy, Data.AfflictionConfig config)
        {
            base.Initialize(enemy, config);
            _currentStacks = 1;
            CheckStacks();
        }

        public override void Refresh(Data.AfflictionConfig config)
        {
            base.Refresh(config);
            _currentStacks++;
            CheckStacks();
        }

        private void CheckStacks()
        {
            if (_currentStacks >= Config.MaxStacks)
            {
                if (Enemy != null)
                {
                    Enemy.Freeze(Config.Power).Forget(); // Using Power as freeze duration for Ice
                }
                _currentStacks = 0;
            }
        }
    }
}
