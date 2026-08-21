using System.Collections.Generic;
using Shared.Enums;
using UnityEngine;

namespace Gameplay.Enemies
{
    public class EnemyAffliction : MonoBehaviour
    {
        [SerializeField] private List<GameObject> _afflictionGameObjects;

        /// <summary>
        /// Toggles the visual GameObject for the given affliction type on or off.
        /// </summary>
        public void ToggleVisual(AfflictionType afflictionType, bool isActive)
        {
            var affliction = GetAffliction(afflictionType);
            if (affliction != null)
            {
                affliction.SetActive(isActive);
            }
        }

        /// <summary>
        /// Returns the visual GameObject mapped to the given affliction type.
        /// </summary>
        private GameObject GetAffliction(AfflictionType afflictionType)
        {
            return afflictionType switch
            {
                AfflictionType.Burn => _afflictionGameObjects[0],
                AfflictionType.Ice => _afflictionGameObjects[1],
                AfflictionType.Weakness => _afflictionGameObjects[2],
                AfflictionType.Lightning => _afflictionGameObjects[3],
                _ => null
            };
        }
    }
}
