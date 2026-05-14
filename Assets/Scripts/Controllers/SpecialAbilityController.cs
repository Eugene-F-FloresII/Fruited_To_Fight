using System;
using System.Collections.Generic;
using Collection;
using Shared.Enums;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using Data;
using Data.Upgrades;
using Gameplay;
using Gameplay.Weapons;
using NaughtyAttributes.Editor;
using Shared.Events;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine.InputSystem.HID;
using UnityEngine.UI;

namespace Controllers
{
    public class SpecialAbilityController : MonoBehaviour
    {
        [Header("Button References")]
        [SerializeField] private Button _firstButton;
        [SerializeField] private Button _secondButton;
        [SerializeField] private Image _firstButtonSprite;
        [SerializeField] private Image _secondButtonSprite;
        
        [Header("Icon List")] 
        [SerializeField] private List<Sprite> _iconList;
        [SerializeField] private List<WeaponClass> _weaponClass;

        private CanvasGroup _canvasGroup;
        private Dictionary<WeaponClass, Sprite> _specialAbilityDict = new Dictionary<WeaponClass, Sprite>();
        private int _overwriteIndex = 0;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            InitializeSpecialAbilityIcons();
        }
        
        private void OnEnable()
        {
            Events_Weapons.OnChosenWeapon += PrepareSpecialAbility;
        }

        private void OnDisable()
        {
            Events_Weapons.OnChosenWeapon -= PrepareSpecialAbility;
        }

        public void TurnOffCanvasGroup()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
        }

        public void TurnOnCanvasGroup()
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = true;
        }

        private void InitializeSpecialAbilityIcons()
        {
            if (_iconList.Count != _weaponClass.Count)
            {
                Debug.LogError("Count of both lists are not the same value!");
                return;
            }

            for (int i = 0; i < _iconList.Count; i++)
            {
                _specialAbilityDict.Add(_weaponClass[i], _iconList[i]);
            }
        }

        private Sprite GetSpecialAbilityIcon(WeaponClass weaponClass)
        {
            if (_specialAbilityDict.TryGetValue(weaponClass, out Sprite sprite))
            {
                return sprite;
            }
            else
            {
                Debug.LogWarning($"No icon found for weapon class: {weaponClass}");
                return null;
            }
            
        }

        private void PrepareSpecialAbility(WeaponConfig weaponConfig)
        {
            if (weaponConfig == null) return;

            WeaponClass weaponClass = weaponConfig.WeaponClass;
            Sprite icon = GetSpecialAbilityIcon(weaponClass);

            if (_firstButton == null)
            {
                AssignButton(_firstButton, _firstButtonSprite, weaponClass, icon, "First", weaponConfig);
            }
            else if (_secondButton == null)
            {
                AssignButton(_secondButton, _secondButtonSprite, weaponClass, icon, "Second", weaponConfig);
            }
            else
            {
                if (_overwriteIndex == 0)
                {
                    AssignButton(_firstButton, _firstButtonSprite, weaponClass, icon, "First", weaponConfig);
                }
                else
                {
                    AssignButton(_secondButton, _secondButtonSprite, weaponClass, icon, "Second", weaponConfig);
                }

                _overwriteIndex = (_overwriteIndex + 1) % 2;
            }
        }

        private async void PrepareSpecialAbility(string specialKey)
        {
            var handle = Addressables.LoadAssetAsync<WeaponConfig>(specialKey);
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                PrepareSpecialAbility(handle.Result);
            }
            else
            {
                Debug.LogError($"Failed to load UpgradeData with key '{specialKey}'");
            }                    

        }
        
        private void AssignButton(Button button, Image spriteRenderer, WeaponClass weaponClass, Sprite icon, string label, WeaponConfig weaponConfig)
        {
            if (button.TryGetComponent(out SpecialAbilityInteractable existingInteractable))
            {
                Destroy(existingInteractable);
            }
            SpecialAbilityInteractable interactable = button.AddComponent<SpecialAbilityInteractable>();
            interactable.InitializeWeaponConfig(weaponConfig);
            
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(interactable.UseSpecialAbility);
            if (spriteRenderer != null) spriteRenderer.sprite = icon;
        }
        
        
    }
}
