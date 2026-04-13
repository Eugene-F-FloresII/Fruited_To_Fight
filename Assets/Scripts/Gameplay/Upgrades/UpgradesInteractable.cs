using System;
using Collection;
using Managers;
using Obvious.Soap;
using Shared.Enums;
using TMPro;
using UnityEngine;

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
        
        private UpgradesManager _upgradesManager;


        private void Start()
        {
            _upgradesManager = ServiceLocator.Get<UpgradesManager>();

            if (_upgradesManager != null)
            {
                if (_categoryType == UpgradesCategoryType.Damage)
                {
                    ApplyTexts(((_upgradesManager.GetDamageMultiplier() - 1) * 100),
                        _overallDamageLevel.Value,
                        _upgradesManager.GetSeedPriceDamageUpgrade());
                
                } else if (_categoryType == UpgradesCategoryType.Range)
                {
                    ApplyTexts(((_upgradesManager.GetRangeMultiplier() - 1) * 100),
                        _overallRangeLevel.Value,
                        _upgradesManager.GetSeedPriceRangeUpgrade());
                
                } else if (_categoryType == UpgradesCategoryType.Speed)
                {
                    ApplyTexts(((_upgradesManager.GetSpeedMultiplier() - 1) * 100),
                        _overallSpeedLevel.Value,
                        _upgradesManager.GetSeedPriceSpeedUpgrade());
                }
            }
        }

        private void OnEnable()
        {
            
            if (_categoryType == UpgradesCategoryType.Damage)
            {
                _overallDamageLevel.OnValueChanged += OnDamageLevelChanged;
                
            } else if (_categoryType == UpgradesCategoryType.Range)
            {
                _overallRangeLevel.OnValueChanged += OnRangeLevelChanged;
                
            } else if (_categoryType == UpgradesCategoryType.Speed)
            {
                _overallSpeedLevel.OnValueChanged += OnSpeedLevelChanged;
            }
        }

        private void OnDisable()
        {
            if (_categoryType == UpgradesCategoryType.Damage)
            {
                _overallDamageLevel.OnValueChanged -= OnDamageLevelChanged;
                
            } else if (_categoryType == UpgradesCategoryType.Range)
            {
                _overallRangeLevel.OnValueChanged -= OnRangeLevelChanged;
                
            } else if (_categoryType == UpgradesCategoryType.Speed)
            {
                _overallSpeedLevel.OnValueChanged -= OnSpeedLevelChanged;
            }
        }


        private void OnDamageLevelChanged(int i)
        {
            
            ApplyTexts((_upgradesManager.GetDamageMultiplier() - 1 * 100),
                _overallDamageLevel.Value,
                _upgradesManager.GetSeedPriceDamageUpgrade());
        }
        
        private void OnRangeLevelChanged(int i)
        {
            ApplyTexts((_upgradesManager.GetRangeMultiplier() - 1 * 100),
                _overallRangeLevel.Value,
                _upgradesManager.GetSeedPriceRangeUpgrade());
        }
        
        private void OnSpeedLevelChanged(int i)
        {
            ApplyTexts((_upgradesManager.GetSpeedMultiplier() - 1 * 100),
                 _overallSpeedLevel.Value,
                _upgradesManager.GetSeedPriceSpeedUpgrade());
        }

        private void ApplyTexts(float damagePercentage, int level, float price)
        {
            _percentageText.text = "+" + damagePercentage + "% Damage & Pierce";
            _levelText.text = "Level " + level;
            _priceText.text = price + " seeds";
        }
    }

}
