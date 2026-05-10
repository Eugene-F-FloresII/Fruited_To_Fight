using System;
using Managers;
using Shared.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PrimeTween;
using Cysharp.Threading.Tasks;
using Data.Upgrades;

namespace Gameplay.Upgrades
{
    public class UpgradesInteractable : MonoBehaviour
    {
        [Header("Setting")]
        [SerializeField] private UpgradeData _upgrades;
        
        [Header("Text References")]
        [SerializeField] private TextMeshProUGUI _percentageText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _priceText;

        [Header("Animation Settings")]
        [SerializeField] private float _animationDuration = 0.5f;

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
            
            // Set initial state for pop out animation
            transform.localScale = Vector3.zero;
            if (_canvasGroup != null) _canvasGroup.alpha = 0f;
            
            UpdateUI();
        }

        private void OnDisable()
        {
            if (_upgrades != null && _upgrades.UpgradeLevel != null)
            {
                _upgrades.UpgradeLevel.OnValueChanged -= OnLevelChanged;
            }
            
            // Stop any ongoing tweens to prevent conflicts
            Tween.StopAll(transform);
        }

        private void OnLevelChanged(int level) => DelayedUpdateUI().Forget();

        private async UniTaskVoid DelayedUpdateUI()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(4f), cancellationToken: this.GetCancellationTokenOnDestroy());
            UpdateUI();
        }

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
                    case UpgradesCategoryType.Tomahawk:
                        label = "% Increase";
                        displayValue = (_upgrades.GetMultiplier(currentLevel + 1) - 1) * 100;
                        break;
                }

                ApplyStatusTexts(Mathf.RoundToInt(displayValue), label, currentLevel, price);
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

        public void TurnOffCanvasGroup()
        {
            if (_canvasGroup == null) return;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            
            Tween.Alpha(_canvasGroup, 0f, _animationDuration * 0.6f);
            Tween.Scale(transform, 0f, _animationDuration * 0.6f, Ease.InBack);
        }

        public void TurnOnCanvasGroup()
        {
            if (_canvasGroup == null) return;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            
            Tween.Alpha(_canvasGroup, 1f, _animationDuration);
            Tween.Scale(transform, 1f, _animationDuration, Ease.OutBack);
        }
    }
}
