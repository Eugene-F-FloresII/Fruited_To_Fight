using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Data;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using Obvious.Soap;
using PrimeTween;
using Shared.Events;

namespace Controllers
{
    public class UpgradesController : MonoBehaviour
    {
        [Header("Weapons to Upgrade **Do not touch**")]
        [SerializeField] private WeaponConfig _firstWeaponConfig;
        [SerializeField] private WeaponConfig _secondWeaponConfig;
        
        [Header("Currency")]
        [SerializeField] private IntVariable _seeds;
        
        [Header("Upgrade Levels")]
        [SerializeField] private IntVariable _firstDamageLevels;
        [SerializeField] private IntVariable _firstRangeLevels;
        [SerializeField] private IntVariable _firstAtkSpeedLevels;
        [SerializeField] private IntVariable _secondDamageLevels;
        [SerializeField] private IntVariable _secondRangeLevels;
        [SerializeField] private IntVariable _secondAtkSpeedLevels;

        [Header("Upgrade Panel")]
        [SerializeField] private CanvasGroup _upgradesCanvasGroup;
        [SerializeField] private RectTransform _upgradesPanelRectTransform;

        private bool _configureFirstWeapon;
        private bool _isFirstOccupied = false;
        private bool _isSecondOccupied = false;

        private float _firstWeaponInitialDamage;
        private int _firstWeaponInitialPierce;
        private float _firstWeaponInitialRange;
        private float _firstWeaponInitialKnockback;
        private float _firstWeaponInitialSpeed;
        private float _firstWeaponInitialAtkSpeed;
        
        private float _secondWeaponInitialDamage;
        private int _secondWeaponInitialPierce;
        private float _secondWeaponInitialRange;
        private float _secondWeaponInitialKnockback;
        private float _secondWeaponInitialSpeed;
        private float _secondWeaponInitialAtkSpeed;


        private float _firstDamagePercentage = 0.25f;
        private float _firstRangePercentage = 0.25f;
        private float _firstSpeedPercentage = 0.25f;
        
        private float _secondDamagePercentage = 0.25f;
        private float _secondRangePercentage = 0.25f;
        private float _secondSpeedPercentage = 0.25f;

        private Sequence _showPanelSequence;
        
        private int _firstPriceDamageUpgrade = 10;
        private int _firstPriceRangeUpgrade = 7;
        private int _firstPriceSpeedUpgrade = 9;
        
        private int _secondPriceDamageUpgrade = 10;
        private int _secondPriceRangeUpgrade = 7;
        private int _secondPriceSpeedUpgrade = 9;

        private void Awake()
        {
            if (_upgradesCanvasGroup == null)
            {
                _upgradesCanvasGroup = GetComponent<CanvasGroup>();
            }

            if (_upgradesPanelRectTransform == null)
            {
                _upgradesPanelRectTransform = GetComponent<RectTransform>();
            }
        }
        
        private void OnEnable()
        {
            Events_Weapons.OnChosenWeapon += CurrentWeapon;
            Events_Round.OnRoundEnded += ShowUpgradePanel;
            Events_Round.OnRoundStarted += HideUpgradePanel;
        }

        private void OnDisable()
        {
            Events_Weapons.OnChosenWeapon -= CurrentWeapon;
            Events_Round.OnRoundEnded -= ShowUpgradePanel;
            Events_Round.OnRoundStarted -= HideUpgradePanel;

            if (_showPanelSequence.isAlive)
            {
                _showPanelSequence.Stop();
            }
        }

        public WeaponConfig FirstWeaponConfig()
        {
            if (_firstWeaponConfig == null)
            {
                return null;
            }
            return _firstWeaponConfig;
        }

        public WeaponConfig SecondWeaponConfig()
        {
            if (_secondWeaponConfig == null)
            {
                return null;
            }

            return _secondWeaponConfig;
        }

        private void ShowUpgradePanel(int _)
        {
            if (_upgradesCanvasGroup == null)
            {
                return;
            }

            _upgradesCanvasGroup.alpha = 1f;
            _upgradesCanvasGroup.interactable = true;
            _upgradesCanvasGroup.blocksRaycasts = true;

            if (_upgradesPanelRectTransform == null)
            {
                return;
            }

            if (_showPanelSequence.isAlive)
            {
                _showPanelSequence.Stop();
            }

            _upgradesPanelRectTransform.localScale = Vector3.one * 0.9f;
            _showPanelSequence = Tween.Scale(_upgradesPanelRectTransform, Vector3.one * 1.05f, 0.22f, Ease.OutBack)
                .Chain(Tween.Scale(_upgradesPanelRectTransform, Vector3.one, 0.14f, Ease.InOutSine));
        }

        private void HideUpgradePanel(int _)
        {
            if (_upgradesCanvasGroup == null)
            {
                return;
            }

            if (_showPanelSequence.isAlive)
            {
                _showPanelSequence.Stop();
            }

            _upgradesCanvasGroup.alpha = 0f;
            _upgradesCanvasGroup.interactable = false;
            _upgradesCanvasGroup.blocksRaycasts = false;

            if (_upgradesPanelRectTransform != null)
            {
                _upgradesPanelRectTransform.localScale = Vector3.one;
            }
        }

        #region First Weapon Upgrade
        
        public void UpgradeFirstWeaponDamage(int seed)
        { 
           
            if (_firstPriceDamageUpgrade > seed)
            {
                Debug.Log("Not enough seeds");
                return;
            }

            if (_firstDamageLevels.Value > 3)
            {
                Debug.Log("Max Level Upgrade");
                return;
            }

            if (_firstDamageLevels.Value <= 0)
            {
                _firstDamageLevels.Value = 1;
            }
            
            int bought = _firstPriceDamageUpgrade - seed;
            _seeds.Value = bought;
            
            _firstWeaponConfig.WeaponDamage = (_firstWeaponConfig.WeaponDamage + (_firstWeaponInitialDamage * _firstDamagePercentage));
            _firstWeaponConfig.WeaponPierce = (_firstWeaponConfig.WeaponPierce + Mathf.RoundToInt(_firstWeaponInitialPierce * _firstDamagePercentage));
            _firstPriceDamageUpgrade += (int)(_firstPriceDamageUpgrade * _firstDamagePercentage);
            _firstDamagePercentage += _firstDamagePercentage;
            _firstDamageLevels.Value++;
            
        }
        
        public void UpgradeFirstWeaponRange(int seed)
        { 
           
            if (_firstPriceRangeUpgrade > seed)
            {
                Debug.Log("Not enough seeds");
                return;
            }

            if (_firstRangeLevels.Value > 3)
            {
                Debug.Log("Max Level Upgrade");
                return;
            }

            if (_firstRangeLevels.Value <= 0)
            {
                _firstRangeLevels.Value = 1;
            }
            
            int bought = _firstPriceRangeUpgrade - seed;
            _seeds.Value = bought;
            
            _firstWeaponConfig.WeaponRange = (_firstWeaponConfig.WeaponRange  + (_firstWeaponInitialRange * _firstRangePercentage));
            _firstWeaponConfig.WeaponKnockback = (_firstWeaponConfig.WeaponKnockback + Mathf.RoundToInt(_firstWeaponInitialKnockback * _firstRangePercentage));
            _firstPriceRangeUpgrade += (int)(_firstPriceRangeUpgrade * _firstRangePercentage);
            _firstRangePercentage += _firstRangePercentage;
            _firstRangeLevels.Value++;
            
        }
        
        public void UpgradeFirstWeaponSpeed(int seed)
        { 
           
            if (_firstPriceSpeedUpgrade > seed)
            {
                Debug.Log("Not enough seeds");
                return;
            }

            if (_firstAtkSpeedLevels.Value > 3)
            {
                Debug.Log("Max Level Upgrade");
                return;
            }
            
            if (_firstAtkSpeedLevels.Value <= 0)
            {
                _firstAtkSpeedLevels.Value = 1;
            }
            
            int bought = _firstPriceSpeedUpgrade - seed;
            _seeds.Value = bought;
            
            _firstWeaponConfig.WeaponSpeed = (_firstWeaponConfig.WeaponSpeed  + (_firstWeaponInitialSpeed * _firstSpeedPercentage));
            _firstWeaponConfig.WeaponAtkSpeed = (_firstWeaponConfig.WeaponAtkSpeed + Mathf.RoundToInt(_firstWeaponInitialAtkSpeed * _firstSpeedPercentage));
            _firstPriceSpeedUpgrade += (int)(_firstPriceSpeedUpgrade * _firstSpeedPercentage);
            _firstSpeedPercentage += _firstSpeedPercentage;
            _firstAtkSpeedLevels.Value++;
        }
        
        #endregion

        #region Second Weapon Upgrade

        public void UpgradeSecondWeaponDamage(int seed)
        { 
           
            if (_secondPriceDamageUpgrade > seed)
            {
                Debug.Log("Not enough seeds");
                return;
            }

            if (_secondDamageLevels.Value > 3)
            {
                Debug.Log("Max Level Upgrade");
                return;
            }

            if (_secondDamageLevels.Value <= 0)
            {
                _secondDamageLevels.Value = 1;
            }
            
            int bought = _secondPriceDamageUpgrade - seed;
            _seeds.Value = bought;
            
            _secondWeaponConfig.WeaponDamage = (_secondWeaponConfig.WeaponDamage + (_secondWeaponInitialDamage * _secondDamagePercentage));
            _secondWeaponConfig.WeaponPierce = (_secondWeaponConfig.WeaponPierce + Mathf.RoundToInt(_secondWeaponInitialPierce * _secondDamagePercentage));
            _secondPriceDamageUpgrade += (int)(_secondPriceDamageUpgrade * _secondDamagePercentage);
            _secondDamagePercentage += _secondDamagePercentage;
            _secondDamageLevels.Value++;
            
        }
        
        public void UpgradeSecondWeaponRange(int seed)
        { 
           
            if (_secondPriceRangeUpgrade > seed)
            {
                Debug.Log("Not enough seeds");
                return;
            }

            if (_secondRangeLevels.Value > 3)
            {
                Debug.Log("Max Level Upgrade");
                return;
            }

            if (_secondRangeLevels.Value <= 0)
            {
                _secondRangeLevels.Value = 1;
            }
            
            int bought = _secondPriceRangeUpgrade - seed;
            _seeds.Value = bought;
            
            _secondWeaponConfig.WeaponRange = (_secondWeaponConfig.WeaponRange  + (_secondWeaponInitialRange * _secondRangePercentage));
            _secondWeaponConfig.WeaponKnockback = (_secondWeaponConfig.WeaponKnockback + Mathf.RoundToInt(_secondWeaponInitialKnockback * _secondRangePercentage));
            _secondPriceRangeUpgrade += (int)(_secondPriceRangeUpgrade * _secondRangePercentage);
            _secondRangePercentage += _secondRangePercentage;
            _secondRangeLevels.Value++;
            
        }
        
        public void UpgradeSecondWeaponSpeed(int seed)
        { 
           
            if (_secondPriceSpeedUpgrade > seed)
            {
                Debug.Log("Not enough seeds");
                return;
            }

            if (_secondAtkSpeedLevels.Value > 3)
            {
                Debug.Log("Max Level Upgrade");
                return;
            }
            
            if (_secondAtkSpeedLevels.Value <= 0)
            {
                _secondAtkSpeedLevels.Value = 1;
            }
            
            int bought = _secondPriceSpeedUpgrade - seed;
            _seeds.Value = bought;
            
            _secondWeaponConfig.WeaponSpeed = (_secondWeaponConfig.WeaponSpeed  + (_secondWeaponInitialSpeed * _secondSpeedPercentage));
            _secondWeaponConfig.WeaponAtkSpeed = (_secondWeaponConfig.WeaponAtkSpeed + Mathf.RoundToInt(_secondWeaponInitialAtkSpeed * _secondSpeedPercentage));
            _secondPriceSpeedUpgrade += (int)(_secondPriceSpeedUpgrade * _secondSpeedPercentage);
            _secondSpeedPercentage += _secondSpeedPercentage;
            _secondAtkSpeedLevels.Value++;
        }

        #endregion
        private async void CurrentWeapon(string label)
        {
            var handle = Addressables.LoadAssetAsync<WeaponConfig>(label); // label
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                InitializeWeaponConfig(handle.Result);
            }
            else
            {
                Debug.LogError("Failed to load WeaponConfig with label 'Spear'");
            }
        }

        private void InitializeWeaponConfig(WeaponConfig weaponConfig)
        {
            if (!_isFirstOccupied)
            {
                _firstWeaponConfig = weaponConfig;
                SetUpFirstWeaponConfig();
                Events_Upgrades.OnChosenWeapon?.Invoke(_firstWeaponConfig, true);
                _isFirstOccupied = true;
            }
            else if (!_isSecondOccupied)
            {
                _secondWeaponConfig = weaponConfig;
                SetUpSecondWeaponConfig();
                Events_Upgrades.OnChosenWeapon?.Invoke(_secondWeaponConfig, false);
                _isSecondOccupied = true;
            }
            else
            {
                Debug.Log("both arms are occupied");
            }
        }

        private void SetUpFirstWeaponConfig()
        {
            _firstWeaponInitialDamage = _firstWeaponConfig.WeaponDamage;
            _firstWeaponInitialPierce = _firstWeaponConfig.WeaponPierce;
            _firstWeaponInitialRange = _firstWeaponConfig.WeaponRange;
            _firstWeaponInitialKnockback = _firstWeaponConfig.WeaponKnockback;
            _firstWeaponInitialSpeed = _firstWeaponConfig.WeaponSpeed;
            _firstWeaponInitialAtkSpeed = _firstWeaponConfig.WeaponAtkSpeed;
        }
        
        private void SetUpSecondWeaponConfig()
        {
            _secondWeaponInitialDamage = _secondWeaponConfig.WeaponDamage;
            _secondWeaponInitialPierce = _secondWeaponConfig.WeaponPierce;
            _secondWeaponInitialRange = _secondWeaponConfig.WeaponRange;
            _secondWeaponInitialKnockback = _secondWeaponConfig.WeaponKnockback;
            _secondWeaponInitialSpeed = _secondWeaponConfig.WeaponSpeed;
            _secondWeaponInitialAtkSpeed = _secondWeaponConfig.WeaponAtkSpeed;
        }
        
    }

}
