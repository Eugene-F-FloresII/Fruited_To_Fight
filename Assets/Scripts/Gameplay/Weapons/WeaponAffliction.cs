using System.Collections.Generic;
using Data;
using UnityEngine;
using Shared.Enums;

namespace Gameplay.Weapons
{
    public class WeaponAffliction : MonoBehaviour
    {
        [SerializeField] private List<GameObject> _weaponAfflictions;
        
        
        public void ToggleVisual(AfflictionType afflictionType, bool isActive)
        {
            var affliction = GetAffliction(afflictionType);
            if (affliction != null)
            {
                affliction.SetActive(isActive);
            }
        }

        public void DisableAllVisuals()
        {
            foreach (var visual in _weaponAfflictions)
            {
                if (visual != null)
                    visual.SetActive(false);
            }
        }
        
        private GameObject GetAffliction(AfflictionType afflictionType)
        {
            return afflictionType switch
            {
                AfflictionType.Burn => _weaponAfflictions[0],
                AfflictionType.Ice => _weaponAfflictions[1],
                AfflictionType.Weakness => _weaponAfflictions[2],
                _ => null
            };
        }

    }

}
