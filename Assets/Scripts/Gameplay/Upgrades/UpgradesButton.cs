using Controllers;
using Data;
using Obvious.Soap;
using PrimeTween;
using Shared.Enums;
using Shared.Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Gameplay.Upgrades
{
    public class UpgradesButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("Upgrades Panel Config")]
        [SerializeField] private UpgradesPanelConfig _upgradesPanelConfig;
        [SerializeField] private UpgradesController _upgradesController;
        
        [Header("References")]
        [SerializeField] private TextMeshProUGUI _upgradesTextPercentage;
        [SerializeField] private TextMeshProUGUI _upgradesTextLevel;
        [SerializeField] private TextMeshProUGUI _upgradesTextPrice;
        [SerializeField] private GameObject _disabledGameObject;
        [SerializeField] private Button _button;
        
        [Header("SOAP References")]
        [SerializeField] private IntVariable _firstDamageLevels;
        [SerializeField] private IntVariable _firstRangeLevels;
        [SerializeField] private IntVariable _firstAtkSpeedLevels;
        [SerializeField] private IntVariable _secondDamageLevels;
        [SerializeField] private IntVariable _secondRangeLevels;
        [SerializeField] private IntVariable _secondAtkSpeedLevels;
        
        [Header("Hover Tween")]
        [SerializeField] private float _hoverScaleMultiplier = 1.05f;
        [SerializeField] private float _hoverDuration = 0.12f;
        [SerializeField] private Ease _hoverEase = Ease.OutBack;
        [SerializeField] private Ease _unhoverEase = Ease.InBack;

        private Tween _hoverTween;
        private Vector3 _baseScale;

        private void Awake()
        {
            ResolveUpgradesController();
            _baseScale = transform.localScale;
            if (_button == null)
            {
                _button = GetComponent<Button>();
            }
            
            ResetDisabledState();
        }

        private void OnEnable()
        {
            Events_Upgrades.OnUpgradeDataChanged += HandleUpgradeDataChanged;
            Events_Upgrades.OnActiveUpgradeWeaponSlotChanged += HandleActiveWeaponSlotChanged;
            RefreshTexts();
        }

        private void OnDisable()
        {
            Events_Upgrades.OnUpgradeDataChanged -= HandleUpgradeDataChanged;
            Events_Upgrades.OnActiveUpgradeWeaponSlotChanged -= HandleActiveWeaponSlotChanged;

            if (_hoverTween.isAlive)
            {
                _hoverTween.Stop();
            }

            if (_baseScale == default)
            {
                _baseScale = transform.localScale;
            }

            transform.localScale = _baseScale;
        }

        private void ResolveUpgradesController()
        {
            if (_upgradesController == null)
            {
                _upgradesController = GetComponentInParent<UpgradesController>(true);
            }
        }

        private void PlayScaleTween(Vector3 targetScale, Ease ease)
        {
            if (_hoverTween.isAlive)
            {
                _hoverTween.Stop();
            }

            _hoverTween = Tween.Scale(transform, targetScale, _hoverDuration, ease);
        }

        private void HandleActiveWeaponSlotChanged(UpgradesWeaponSlot _)
        {
            RefreshTexts();
            ResetDisabledState();
        }

        private void HandleUpgradeDataChanged(UpgradesWeaponSlot slot, UpgradesPanelType upgradesPanelType)
        {
            if (_upgradesPanelConfig == null)
            {
                return;
            }

            ResolveUpgradesController();
            if (_upgradesController == null)
            {
                return;
            }

            if (slot != _upgradesController.ActiveUpgradeWeaponSlot)
            {
                return;
            }

            if (upgradesPanelType != _upgradesPanelConfig.UpgradesPanelType)
            {
                return;
            }

            RefreshTexts();
        }

        private void RefreshTexts()
        {
            if (_upgradesPanelConfig == null)
            {
                return;
            }

            ResolveUpgradesController();
            if (_upgradesController == null)
            {
                return;
            }

            TryAutoWireTextReferences();

            UpgradesWeaponSlot slot = _upgradesController.ActiveUpgradeWeaponSlot;
            UpgradesPanelType upgradesPanelType = _upgradesPanelConfig.UpgradesPanelType;

            int level = _upgradesController.GetUpgradeLevel(slot, upgradesPanelType);
            bool isMaxLevel = level >= _upgradesController.MaxLevel;

            if (isMaxLevel)
            {
                if (_upgradesTextLevel != null)
                {
                    _upgradesTextLevel.text = "Maxed";
                }

                if (_upgradesTextPrice != null)
                {
                    _upgradesTextPrice.text = "Maxed";
                }

                if (_upgradesTextPercentage != null)
                {
                    _upgradesTextPercentage.text = "Maxed";
                }
            }
            else
            {
                int price = _upgradesController.GetUpgradePrice(slot, upgradesPanelType);
                float percentage = _upgradesController.GetUpgradePercentage(slot, upgradesPanelType);

                if (_upgradesTextLevel != null)
                {
                    _upgradesTextLevel.text = $"Level {level}";
                }

                if (_upgradesTextPrice != null)
                {
                    _upgradesTextPrice.text = $"{price} seeds";
                }

                if (_upgradesTextPercentage != null)
                {
                    _upgradesTextPercentage.text = $"{Mathf.RoundToInt(percentage * 100f)}%";
                }
            }

            if (_button != null)
            {
                _button.interactable = !isMaxLevel;
                if (_disabledGameObject != null)
                {
                    _disabledGameObject.SetActive(isMaxLevel);
                }
            }
        }
        
        public void ResetDisabledState()
        {
            if (_disabledGameObject != null)
            {
                _disabledGameObject.SetActive(false);
            }
        }

        private void TryAutoWireTextReferences()
        {
            if (_upgradesTextPercentage != null && _upgradesTextLevel != null && _upgradesTextPrice != null)
            {
                return;
            }

            var texts = GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var text in texts)
            {
                if (text == null)
                {
                    continue;
                }

                string name = text.gameObject.name;
                if (_upgradesTextPrice == null && name == "Text_WeaponUpgradePrice")
                {
                    _upgradesTextPrice = text;
                }
                else if (_upgradesTextLevel == null && name == "Text_WeaponUpgradeLevel")
                {
                    _upgradesTextLevel = text;
                }
                else if (_upgradesTextPercentage == null && name == "Text_WeaponUpgradeIncrease")
                {
                    _upgradesTextPercentage = text;
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_disabledGameObject != null && _disabledGameObject.activeSelf)
            {
                return;
            }

            if (_upgradesPanelConfig == null)
            {
                Debug.LogError($"{nameof(UpgradesButton)}: Missing {nameof(UpgradesPanelConfig)} reference.");
                return;
            }

            ResolveUpgradesController();
            if (_upgradesController == null)
            {
                Debug.LogError($"{nameof(UpgradesButton)}: Missing {nameof(UpgradesController)} reference.");
                return;
            }

            if (_baseScale == default)
            {
                _baseScale = transform.localScale;
            }

            PlayScaleTween(_baseScale * _hoverScaleMultiplier, _hoverEase);

            bool isFirstWeapon = _upgradesController.ActiveUpgradeWeaponSlot == UpgradesWeaponSlot.First;
            Events_Upgrades.OnHoveredUpgrade?.Invoke(_upgradesPanelConfig.UpgradesPanelType, isFirstWeapon);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_baseScale == default)
            {
                _baseScale = transform.localScale;
            }

            PlayScaleTween(_baseScale, _unhoverEase);
        }



        public void OnPointerClick(PointerEventData eventData)
        {
            if (_upgradesPanelConfig == null)
            {
                Debug.LogError($"{nameof(UpgradesButton)}: Missing {nameof(UpgradesPanelConfig)} reference.");
                return;
            }

            ResolveUpgradesController();
            if (_upgradesController == null)
            {
                Debug.LogError($"{nameof(UpgradesButton)}: Missing {nameof(UpgradesController)} reference.");
                return;
            }

            _upgradesController.UpgradeActiveWeapon(_upgradesPanelConfig.UpgradesPanelType);
               
        }
    }

}
