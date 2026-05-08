using System;
using Managers;
using Shared.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Upgrades
{
    public class UpgradesInteractable : MonoBehaviour
    {
        [Header("Setting")]
        [SerializeField] private Data.Upgrades.Upgrades _upgrades;
        
        [Header("Text References")]
        [SerializeField] private TextMeshProUGUI _percentageText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _priceText;

        private CanvasGroup _canvasGroup;
        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            if (_upgrades != null && _upgrades.UpgradeLevel != null)
            {
                _upgrades.UpgradeLevel.OnValueChanged += OnLevelChanged;
            }
            
            UpdateUI();
        }

        private void OnDisable()
        {
            if (_upgrades != null && _upgrades.UpgradeLevel != null)
            {
                _upgrades.UpgradeLevel.OnValueChanged -= OnLevelChanged;
            }
        }

        private void OnLevelChanged(int level) => UpdateUI();

        private void UpdateUI()
        {
            if (_upgrades == null) return;

            bool isMaxed = _upgrades.GetUpgradeLevelMaxed();
            int currentLevel = _upgrades.UpgradeLevel.Value;
            int price = _upgrades.GetSeedPriceUpgrade();

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

                string label = "";
                float displayValue = 0;

                switch (_upgrades.Category)
                {
                    case UpgradesCategoryType.Damage:
                        label = "% Damage";
                        displayValue = (_upgrades.GetMultiplier(currentLevel + 1) - 1) * 100;
                        break;
                    case UpgradesCategoryType.Pierce:
                        label = " Pierce";
                        displayValue = 1; 
                        break;
                    case UpgradesCategoryType.Range:
                        label = "% Range";
                        displayValue = (_upgrades.GetMultiplier(currentLevel + 1) - 1) * 100;
                        break;
                    case UpgradesCategoryType.Knockback:
                        label = " Knockback";
                        displayValue = 1;
                        break;
                    case UpgradesCategoryType.Speed:
                        label = "% Speed";
                        displayValue = (_upgrades.GetMultiplier(currentLevel + 1) - 1) * 100;
                        break;
                    case UpgradesCategoryType.AttackSpeed:
                        label = "% Attack Speed";
                        displayValue = (_upgrades.GetMultiplier(currentLevel + 1) - 1) * 100;
                        break;
                }

                ApplyStatusTexts(displayValue, label, currentLevel, price);
            }
        }

        private void ApplyStatusTexts(float value, string label, int level, float price)
        {
            if (_percentageText != null) _percentageText.text = "+" + value + label;
            if (_levelText != null) _levelText.text = "Level " + (level + 1);
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
