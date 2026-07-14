using System;
using Collection;
using Data;
using Shared.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using Gameplay.Weapons;
using Shared.Events;

namespace Gameplay
{
    public class SpecialAbilityInteractable : MonoBehaviour
    {
        [Header("References")] 
        [SerializeField] private TextMeshProUGUI _cooldownText;
        
        private Button _button;
        private WeaponConfig _weaponConfig;
        private float _cooldown;
        private bool _interactable;
        
        private void Awake()
        {
            _cooldownText = GetComponentInChildren<TextMeshProUGUI>();
            _cooldownText.gameObject.SetActive(false);
            _button = GetComponent<Button>();
        }

        private void Start()
        {
            _interactable = true;
        }

        /// <summary>
        /// Initializes the interactable button with its corresponding weapon configuration.
        /// </summary>
        /// <param name="weaponConfig">The WeaponConfig configuration to apply.</param>
        public void InitializeWeaponConfig(WeaponConfig weaponConfig)
        {
            _weaponConfig = weaponConfig;

            _cooldown = _weaponConfig.AbilityCooldown;
        }

        /// <summary>
        /// Triggers the special ability for the assigned weapon if the button is interactable.
        /// </summary>
        public void UseSpecialAbility()
        {
            if (_interactable && _weaponConfig != null)
            {
                Events_Weapons.OnUsedSpecialAbility?.Invoke(_weaponConfig.WeaponClass);
            }
            
            AbilityCooldown(_cooldown).Forget();
        }
        private async UniTaskVoid AbilityCooldown(float cooldown)
        {
            _cooldownText.gameObject.SetActive(true);
            _button.interactable = false;
            _interactable = false;
    
            while (cooldown >= 0)
            {
                _cooldownText.text = Mathf.CeilToInt(cooldown).ToString();
                cooldown -= Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update);
            }
            
            _cooldownText.gameObject.SetActive(false);
            _cooldownText.text = string.Empty;
            _button.interactable = true;
            _interactable = true;
        }
    }

}
