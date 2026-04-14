using System;
using Collection;
using Managers;
using Obvious.Soap;
using Shared.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Upgrades
{
    public class UpgradesInteractable : MonoBehaviour
    {
        [Header("Setting")]
        [SerializeField] private UpgradesCategoryType _categoryType;
        [SerializeField] private IntVariable _overallDamageLevel;
        [SerializeField] private IntVariable _overallRangeLevel;
        [SerializeField] private IntVariable _overallSpeedLevel;
        
        [Header("Text References")]
        [SerializeField] private TextMeshProUGUI _percentageText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _priceText;

        private CanvasGroup _canvasGroup;
        private UpgradesManager _upgradesManager;
        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Start()
        {
            _upgradesManager = ServiceLocator.Get<UpgradesManager>();
            UpdateUI();
        }

        private void OnEnable()
        {
            // Re-assign in case of issues, though Awake should have handled it
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            if (_button == null) _button = GetComponent<Button>();

            if (_categoryType == UpgradesCategoryType.Damage)
                _overallDamageLevel.OnValueChanged += OnLevelChanged;
            else if (_categoryType == UpgradesCategoryType.Range)
                _overallRangeLevel.OnValueChanged += OnLevelChanged;
            else if (_categoryType == UpgradesCategoryType.Speed)
                _overallSpeedLevel.OnValueChanged += OnLevelChanged;
            
            UpdateUI();
        }

        private void OnDisable()
        {
            if (_categoryType == UpgradesCategoryType.Damage)
                _overallDamageLevel.OnValueChanged -= OnLevelChanged;
            else if (_categoryType == UpgradesCategoryType.Range)
                _overallRangeLevel.OnValueChanged -= OnLevelChanged;
            else if (_categoryType == UpgradesCategoryType.Speed)
                _overallSpeedLevel.OnValueChanged -= OnLevelChanged;
        }

        private void OnLevelChanged(int level) => UpdateUI();

        private void UpdateUI()
        {
            if (_upgradesManager == null)
            {
                _upgradesManager = ServiceLocator.Get<UpgradesManager>();
                if (_upgradesManager == null) return;
            }

            bool isMaxed = false;
            float multiplier = 0;
            string label = "";
            int currentLevel = 0;
            float price = 0;

            switch (_categoryType)
            {
                case UpgradesCategoryType.Damage:
                    isMaxed = _upgradesManager.GetDamageLevelMaxed();
                    multiplier = _upgradesManager.GetDamageMultiplier();
                    label = "% Damage & Pierce";
                    currentLevel = _overallDamageLevel.Value;
                    price = _upgradesManager.GetSeedPriceDamageUpgrade();
                    break;
                case UpgradesCategoryType.Range:
                    isMaxed = _upgradesManager.GetRangedLevelMaxed();
                    multiplier = _upgradesManager.GetRangeMultiplier();
                    label = "% Range & Knockback Force";
                    currentLevel = _overallRangeLevel.Value;
                    price = _upgradesManager.GetSeedPriceRangeUpgrade();
                    break;
                case UpgradesCategoryType.Speed:
                    isMaxed = _upgradesManager.GetSpeedLevelMaxed();
                    multiplier = _upgradesManager.GetSpeedMultiplier();
                    label = "% Speed & Attack Speed";
                    currentLevel = _overallSpeedLevel.Value;
                    price = _upgradesManager.GetSeedPriceSpeedUpgrade();
                    break;
            }

            if (isMaxed)
            {
                ApplyMaxedTexts();
                if (_button != null) _button.interactable = false;
                TurnOffCanvasGroup();
            }
            else
            {
                TurnOnCanvasGroup();
                if (_button != null) _button.interactable = true;
                ApplyStatusTexts((multiplier - 1) * 100, label, currentLevel, price);
            }
        }

        private void ApplyStatusTexts(float damagePercentage, string upgradeType, int level, float price)
        {
            if (_percentageText != null) _percentageText.text = "+" + damagePercentage + upgradeType;
            if (_levelText != null) _levelText.text = "Level " + level;
            if (_priceText != null) _priceText.text = price + " seeds";
        }

        private void ApplyMaxedTexts()
        {
            if (_percentageText != null) _percentageText.text = "Maxed";
            if (_levelText != null) _levelText.text = "Level Maxed";
            if (_priceText != null) _priceText.text = " ";
        }

        private void TurnOffCanvasGroup()
        {
            if (_canvasGroup == null) return;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.alpha = 0f;
        }

        private void TurnOnCanvasGroup()
        {
            if (_canvasGroup == null) return;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.alpha = 1f;
        }
    }
}
