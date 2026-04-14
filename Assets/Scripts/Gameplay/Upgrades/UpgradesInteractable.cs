using System;
using Collection;
using Managers;
using Obvious.Soap;
using Shared.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.HID;
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
        
        private UpgradesManager _upgradesManager;
        private Button _button;


        private void Start()
        {
            _button =  GetComponent<Button>();
            _upgradesManager = ServiceLocator.Get<UpgradesManager>();

            if (_button !=  null)
            {
                _button.interactable = true;
            }

            if (_upgradesManager != null)
            {
                if (_categoryType == UpgradesCategoryType.Damage)
                {
                    ApplyStatusTexts(((_upgradesManager.GetDamageMultiplier() - 1) * 100),
                        "% Damage & Pierce",
                        _overallDamageLevel.Value,
                        _upgradesManager.GetSeedPriceDamageUpgrade());
                
                } else if (_categoryType == UpgradesCategoryType.Range)
                {
                    ApplyStatusTexts(((_upgradesManager.GetRangeMultiplier() - 1) * 100),
                        "% Range & Knockback Force",
                        _overallRangeLevel.Value,
                        _upgradesManager.GetSeedPriceRangeUpgrade());
                
                } else if (_categoryType == UpgradesCategoryType.Speed)
                {
                    ApplyStatusTexts(((_upgradesManager.GetSpeedMultiplier() - 1) * 100),
                        "% Speed & Attack Speed",
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
            if (_upgradesManager.GetDamageLevelMaxed() == true)
            {
                ApplyMaxedTexts();
                if (_button != null)
                {
                    _button.interactable = false;
                }
                return;
            }
            
            _button.interactable = true;
            ApplyStatusTexts((_upgradesManager.GetDamageMultiplier() - 1) * 100,
                "% Damage & Pierce",
                _overallDamageLevel.Value,
                _upgradesManager.GetSeedPriceDamageUpgrade());
        }
        
        private void OnRangeLevelChanged(int i)
        {
            if (_upgradesManager.GetRangedLevelMaxed() == true)
            {
                ApplyMaxedTexts();
                if (_button != null)
                {
                    _button.interactable = false;
                }
                return;
            }
            
            _button.interactable = true;
            ApplyStatusTexts((_upgradesManager.GetRangeMultiplier() - 1) * 100,
                "% Range & Knockback Force",
                _overallRangeLevel.Value,
                _upgradesManager.GetSeedPriceRangeUpgrade());
        }
        
        private void OnSpeedLevelChanged(int i)
        {
            if (_upgradesManager.GetSpeedLevelMaxed() == true)
            {
                ApplyMaxedTexts();
                if (_button != null)
                {
                    _button.interactable = false;
                }
                return;
            }
            
            _button.interactable = true;
            ApplyStatusTexts((_upgradesManager.GetSpeedMultiplier() - 1) * 100,
                "% Speed & Attack Speed",
                 _overallSpeedLevel.Value,
                _upgradesManager.GetSeedPriceSpeedUpgrade());
        }

        private void ApplyStatusTexts(float damagePercentage, string upgradeType, int level, float price)
        {
            _percentageText.text = "+" + damagePercentage + upgradeType;
            _levelText.text = "Level " + level;
            _priceText.text = price + " seeds";
        }

        private void ApplyMaxedTexts()
        {
            _percentageText.text = "Maxed";
            _levelText.text = "Level " + "Maxed ";
            _priceText.text = " ";
        } 
        
    }

}
