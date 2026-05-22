using System.Collections.Generic;
using Data;
using UnityEngine;
using Shared.Enums;

namespace Gameplay.Weapons
{
    public class WeaponAffliction : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator _animator;
        
        [Header("Affliction GameObjects VFXs")]
        [Tooltip("Index 0: None, 1: Burn, 2: Ice, 3: Weakness")]
        [SerializeField] private List<GameObject> _weaponAfflictions;
        [SerializeField] private GameObject _defaultTrail;

        public void ToggleVisual(AfflictionType afflictionType, bool isActive)
        {
            if (isActive)
            {
                // Ensure only one is active at a time
                DisableAllVisuals();
                
                _defaultTrail.SetActive(false);
                
                var afflictionGo = GetAffliction(afflictionType);
                if (afflictionGo != null)
                {
                    afflictionGo.SetActive(true);
                }

                SetLayerWeight(afflictionType, 1f);
            }
            else
            {
                var afflictionGo = GetAffliction(afflictionType);
                if (afflictionGo != null)
                {
                    afflictionGo.SetActive(false);
                }
                
                _defaultTrail.SetActive(true);

                SetLayerWeight(afflictionType, 0f);

                // If we are disabling an affliction, we should probably fall back to showing the 'None' visual
                if (afflictionType != AfflictionType.None)
                {
                    var noneGo = GetAffliction(AfflictionType.None);
                    
                     noneGo.SetActive(true);
                }
            }
        }

        public void DisableAllVisuals()
        {
            // Disable all GameObjects
            foreach (var visual in _weaponAfflictions)
            {
                if (visual != null)
                    visual.SetActive(false);
            }

            // Reset all Animator layers (starting from 1 because 0 is Base/No Affliction and always weight 1)
            if (_animator != null)
            {
                for (int i = 1; i < _animator.layerCount; i++)
                {
                    _animator.SetLayerWeight(i, 0f);
                }
            }
        }
        
        private void SetLayerWeight(AfflictionType afflictionType, float weight)
        {
            if (_animator == null)
            {
                Debug.LogError($"[WeaponAffliction] Animator is null on {gameObject.name}", this);
                return;
            }

            string layerName = afflictionType switch
            {
                AfflictionType.None => "No Affliction",
                AfflictionType.Burn => "Fire Affliction",
                // Add more mappings here as they are added to the Animator
                _ => null
            };

            if (layerName == null)
            {
                Debug.LogWarning($"[WeaponAffliction] No layer mapping for {afflictionType}", this);
                return;
            }

            if (layerName == "No Affliction") return;

            int index = _animator.GetLayerIndex(layerName);
            if (index > 0)
            {
                Debug.Log($"[WeaponAffliction] Setting layer '{layerName}' (index {index}) weight to {weight}", this);
                _animator.SetLayerWeight(index, weight);
            }
            else
            {
                Debug.LogError($"[WeaponAffliction] Layer '{layerName}' not found or is base layer (index: {index}) on {gameObject.name}", this);
            }
        }

        private GameObject GetAffliction(AfflictionType afflictionType)
        {
            return afflictionType switch
            {
                AfflictionType.None => _weaponAfflictions.Count > 0 ? _weaponAfflictions[0] : null,
                AfflictionType.Burn => _weaponAfflictions.Count > 1 ? _weaponAfflictions[1] : null,
                AfflictionType.Ice => _weaponAfflictions.Count > 2 ? _weaponAfflictions[2] : null,
                AfflictionType.Weakness => _weaponAfflictions.Count > 3 ? _weaponAfflictions[3] : null,
                _ => null
            };
        }
    }
}
