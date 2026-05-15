using System.Collections.Generic;
using PrimeTweenDemo;
using Shared.Enums;
using UnityEngine;

namespace Gameplay.Enemies
{
    public class EnemyAffliction : MonoBehaviour
    {
        [SerializeField] private List<GameObject> _afflictionGameObjects;

        public void ToggleVisual(AfflictionType afflictionType, bool isActive)
        {
            var affliction = GetAffliction(afflictionType);
            if (affliction != null)
            {
                affliction.SetActive(isActive);
            }
        }

        private GameObject GetAffliction(AfflictionType afflictionType)
        {
            return afflictionType switch
            {
                AfflictionType.Burn => _afflictionGameObjects[0],
                AfflictionType.Ice => _afflictionGameObjects[1],
                AfflictionType.Weakness => _afflictionGameObjects[2],
                _ => null
            };
        }
        
        
    }

}
