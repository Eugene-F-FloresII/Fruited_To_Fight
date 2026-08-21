using System.Collections.Generic;
using Collection;
using Data;
using Gameplay.Enemies;
using Shared.Enums;
using UnityEngine;
using Controllers;

namespace Gameplay.Enemies
{
    public class EnemyAfflictionHandler : MonoBehaviour
    {
        private EnemyAffliction _visualController;
        private Dictionary<AfflictionType, AfflictionState> _activeAfflictions = new Dictionary<AfflictionType, AfflictionState>();
        private List<AfflictionType> _expiredKeys = new List<AfflictionType>();

        private void Awake()
        {
            _visualController = GetComponentInChildren<EnemyAffliction>();
        }

        private void Update()
        {
            if (_activeAfflictions.Count == 0) return;

            _expiredKeys.Clear();

            foreach (var kvp in _activeAfflictions)
            {
                kvp.Value.Tick(Time.deltaTime);

                if (kvp.Value.IsExpired)
                {
                    _expiredKeys.Add(kvp.Key);
                }
            }

            foreach (var key in _expiredKeys)
            {
                if (_activeAfflictions.TryGetValue(key, out var state))
                {
                    state.Dispose();
                    _activeAfflictions.Remove(key);
                }
            }
        }

        /// <summary>
        /// Applies an affliction to the enemy. Refreshes if the same type is already active.
        /// </summary>
        public void ApplyAffliction(AfflictionConfig config, EnemyController controller)
        {
            if (config == null) return;

            if (_activeAfflictions.TryGetValue(config.Type, out var existingState))
            {
                existingState.Refresh(config);
                return;
            }

            AfflictionState newState = CreateAfflictionState(config.Type);
            if (newState != null)
            {
                _activeAfflictions[config.Type] = newState;
                newState.Initialize(controller, config, _visualController);
            }
        }

        /// <summary>
        /// Clears all active afflictions. Called when the enemy is returned to pool.
        /// </summary>
        public void ClearAllAfflictions()
        {
            foreach (var kvp in _activeAfflictions)
            {
                kvp.Value.Dispose();
            }
            _activeAfflictions.Clear();
        }

        /// <summary>
        /// Creates a new affliction state instance based on the type.
        /// </summary>
        private AfflictionState CreateAfflictionState(AfflictionType type)
        {
            return type switch
            {
                AfflictionType.Burn => new BurnState(),
                AfflictionType.Ice => new IceState(),
                AfflictionType.Weakness => new WeaknessState(),
                AfflictionType.Lightning => new LightningState(),
                _ => null
            };
        }
    }
}
