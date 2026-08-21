using System;
using System.Threading;
using Collection;
using Cysharp.Threading.Tasks;
using Data;
using UnityEngine;
using Controllers;

namespace Gameplay.Enemies
{
    public class EnemyAfflictionHandler : MonoBehaviour
    {
        private EnemyMovement _enemyMovement;
        private EnemyVisuals _enemyVisuals;
        private void Awake()
        {
            _enemyMovement = GetComponent<EnemyMovement>();
            _enemyVisuals = GetComponent<EnemyVisuals>();
        }

        public void ApplyAffliction(AfflictionConfig config, EnemyController controller)
        {
            if (config == null) return;

            var existingAfflictions = GetComponents<AfflictionState>();
            foreach (var affliction in existingAfflictions)
            {
                if (affliction.AfflictionType == config.Type)
                {
                    affliction.Refresh(config);
                    return;
                }
            }

            AfflictionState newState = null;
            switch (config.Type)
            {
                case Shared.Enums.AfflictionType.Burn:
                    newState = gameObject.AddComponent<BurnState>();
                    break;
                case Shared.Enums.AfflictionType.Ice:
                    newState = gameObject.AddComponent<IceState>();
                    break;
                case Shared.Enums.AfflictionType.Weakness:
                    newState = gameObject.AddComponent<WeaknessState>();
                    break;
                case Shared.Enums.AfflictionType.Lightning:
                    newState = gameObject.AddComponent<LightningState>();
                    break;
            }

            if (newState != null)
            {
                newState.Initialize(controller, config);
            }
        }


    }
}
