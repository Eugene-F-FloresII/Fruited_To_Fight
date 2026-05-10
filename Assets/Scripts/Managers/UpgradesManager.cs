using System;
using System.Collections.Generic;
using System.Linq;
using Collection;
using Data;
using Data.Upgrades;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Obvious.Soap;
using Shared.Enums;
using Shared.Events;


namespace Managers
{
    public class UpgradesManager : MonoBehaviour
    {
        [Header("Lists of Upgrades")]
        [SerializeField] private List<UpgradeData> _upgradesList;
        [SerializeField] private List<UpgradesCategoryType> _upgradesCategoryList;
        
        private Dictionary<UpgradesCategoryType, UpgradeData> _upgradesDictionary = new Dictionary<UpgradesCategoryType, UpgradeData>();
        
        private WeaponConfig _firstWeaponConfig;
        private WeaponConfig _secondWeaponConfig;

        private UpgradeData _damage;
        private UpgradeData _pierce;
        private UpgradeData _range;
        private UpgradeData _knockback;
        private UpgradeData _speed;
        private UpgradeData _attackSpeed;
        private UpgradeData _weapon;
        
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
        
        private void Awake()
        {
            ServiceLocator.Register(this);
            InitializeUpgradeDictionary();
        }

        private void Start()
        {
            foreach (var upgrades in _upgradesList)
            {
                upgrades.SetInitialDataValues();
            }
            
            ConfigureAllUpgrades();
        }

        private void OnEnable()
        {
            Events_Weapons.OnChosenWeapon += InitializeCurrentWeapon;
            Events_Game.OnGameRestarted += ResetAllUpgrades;
            Events_Game.OnGameExited += ResetAllUpgrades;
        }

        private void OnDisable()
        {
            Events_Weapons.OnChosenWeapon -= InitializeCurrentWeapon;
            Events_Game.OnGameRestarted -= ResetAllUpgrades;
            Events_Game.OnGameExited -= ResetAllUpgrades;
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<UpgradesManager>();
        }

        public int UpgradeDamage(int seed)
        {
            if (_damage.GetUpgradeLevelMaxed()) return seed;
            UpgradeResult result = _damage.BuyUpgrade(seed, _firstWeaponConfig.WeaponDamage);
            if (_firstWeaponConfig != null)
            {
                _firstWeaponConfig.WeaponDamage += result.Value; 
            }
            if (_secondWeaponConfig != null)
            {
                UpgradeResult secondResult = _damage.BuyUpgrade(seed, _secondWeaponConfig.WeaponDamage);
                _secondWeaponConfig.WeaponDamage += secondResult.Value;

            }
            return result.Currency;     
        }
        
        public int UpgradeRange(int seed)
        {
            if (_range.GetUpgradeLevelMaxed()) return seed;
            UpgradeResult result = _range.BuyUpgrade(seed, _firstWeaponConfig.WeaponRange);
            if (_firstWeaponConfig != null)
            {
                _firstWeaponConfig.WeaponRange += result.Value; 
            }
            if (_secondWeaponConfig != null)
            {
                UpgradeResult secondResult = _range.BuyUpgrade(seed, _secondWeaponConfig.WeaponRange);
                _secondWeaponConfig.WeaponRange += secondResult.Value;

            }
            return result.Currency;     
        }

        public int UpgradeSpeed(int seed)
        {
            if (_speed.GetUpgradeLevelMaxed()) return seed;
            UpgradeResult result = _speed.BuyUpgrade(seed, _firstWeaponConfig.WeaponSpeed);
            if (_firstWeaponConfig != null)
            {
                _firstWeaponConfig.WeaponSpeed += result.Value; 
            }
            if (_secondWeaponConfig != null)
            {
                UpgradeResult secondResult = _speed.BuyUpgrade(seed, _secondWeaponConfig.WeaponSpeed);
                _secondWeaponConfig.WeaponSpeed += secondResult.Value;

            }
            return result.Currency;  
        }
        
        public int UpgradeTomahawk(int seed)
        {
            if (_weapon.GetUpgradeLevelMaxed()) return seed;

            WeaponConfig target = null;
            if (_firstWeaponConfig != null && _firstWeaponConfig.WeaponClass == WeaponClass.Tomahawk)
                target = _firstWeaponConfig;
            else if (_secondWeaponConfig != null && _secondWeaponConfig.WeaponClass == WeaponClass.Tomahawk)
                target = _secondWeaponConfig;

            if (target == null) return seed;

            UpgradeWeaponResult result = _weapon.BuyWeaponUpgrade(seed, target.WeaponDamage, target.WeaponSpeed, target.WeaponRange);
            
            if (_firstWeaponConfig != null && _firstWeaponConfig.WeaponClass == WeaponClass.Tomahawk)
            {
                _firstWeaponConfig.WeaponDamage += result.Damage;
                _firstWeaponConfig.WeaponSpeed += result.Speed;
                _firstWeaponConfig.WeaponRange += result.Range;
            }
            
            if (_secondWeaponConfig != null && _secondWeaponConfig.WeaponClass == WeaponClass.Tomahawk && _secondWeaponConfig != _firstWeaponConfig)
            {
                _secondWeaponConfig.WeaponDamage += result.Damage;
                _secondWeaponConfig.WeaponSpeed += result.Speed;
                _secondWeaponConfig.WeaponRange += result.Range;
            }

            return result.Currency;
        }
        
        
        public void ResetAllUpgrades()
        {
            if (_secondWeaponConfig != null)
            {
                _secondWeaponConfig.WeaponDamage = _secondWeaponInitialDamage;
                _secondWeaponConfig.WeaponPierce = _secondWeaponInitialPierce;
                _secondWeaponConfig.WeaponRange = _secondWeaponInitialRange;
                _secondWeaponConfig.WeaponKnockback = _secondWeaponInitialKnockback;
                _secondWeaponConfig.WeaponSpeed = _secondWeaponInitialSpeed;
                _secondWeaponConfig.WeaponAtkSpeed = _secondWeaponInitialAtkSpeed;
            }

            if (_firstWeaponConfig != null)
            {
                _firstWeaponConfig.WeaponDamage = _firstWeaponInitialDamage;
                _firstWeaponConfig.WeaponPierce = _firstWeaponInitialPierce;
                _firstWeaponConfig.WeaponRange = _firstWeaponInitialRange;
                _firstWeaponConfig.WeaponKnockback = _firstWeaponInitialKnockback;
                _firstWeaponConfig.WeaponSpeed = _firstWeaponInitialSpeed;
                _firstWeaponConfig.WeaponAtkSpeed = _firstWeaponInitialAtkSpeed;
            }

            foreach (var upgrades in _upgradesList)
            {
                upgrades.ResetAllDataValues();
            }
            
            
            Debug.Log("Upgrades and Weapon Configs have been reset to initial values.");
        }

        public bool AreAllLevelsMaxed()
        {
            return _upgradesList.All(upgrade => upgrade.GetUpgradeLevelMaxed());        
        }
        
        private async void InitializeCurrentWeapon(string label)
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
            if (_firstWeaponConfig == null)
            {
                _firstWeaponConfig = weaponConfig;
                SetUpFirstWeaponConfig();
                
            }
            else if (_secondWeaponConfig == null)
            {
                _secondWeaponConfig = weaponConfig;
                SetUpSecondWeaponConfig();
            }
            else
            {
                Debug.Log("both arms are occupied");
            }
        }

        private void InitializeUpgradeDictionary()
        {
            if (_upgradesList.Count != _upgradesCategoryList.Count)
            {
                Debug.LogError("Both list is not the same");
            }

            for (int i = 0; i < _upgradesList.Count; i++)
            {
                _upgradesDictionary.Add(_upgradesCategoryList[i], _upgradesList[i]);
            }
        }

        private UpgradeData GetUpgrade(UpgradesCategoryType upgradesCategory)
        {
            if (_upgradesDictionary.TryGetValue(upgradesCategory, out UpgradeData upgrade))
                return upgrade;

            Debug.LogError($"Upgrade not found for category: {upgradesCategory}");
            return null;
        }
        
        private void SetUpFirstWeaponConfig()
        {
            _firstWeaponInitialDamage = _firstWeaponConfig.WeaponDamage;
            _firstWeaponInitialPierce = _firstWeaponConfig.WeaponPierce;
            _firstWeaponInitialRange = _firstWeaponConfig.WeaponRange;
            _firstWeaponInitialKnockback = _firstWeaponConfig.WeaponKnockback;
            _firstWeaponInitialSpeed = _firstWeaponConfig.WeaponSpeed;
            _firstWeaponInitialAtkSpeed = _firstWeaponConfig.WeaponAtkSpeed;
            
            ApplyAllUpgrades(_firstWeaponConfig, 
                _firstWeaponInitialDamage,
                _firstWeaponInitialRange, 
                _firstWeaponInitialSpeed, 
                _firstWeaponInitialPierce, 
                _firstWeaponInitialKnockback, 
                _firstWeaponInitialAtkSpeed);
        }
        
        private void SetUpSecondWeaponConfig()
        {
            if (_secondWeaponConfig == _firstWeaponConfig)
            {
                // Both weapons share the same ScriptableObject.
                // We copy the initial stats already captured from the first weapon 
                // to ensure our baseline for future upgrades remains correct.
                _secondWeaponInitialDamage = _firstWeaponInitialDamage;
                _secondWeaponInitialPierce = _firstWeaponInitialPierce;
                _secondWeaponInitialRange = _firstWeaponInitialRange;
                _secondWeaponInitialKnockback = _firstWeaponInitialKnockback;
                _secondWeaponInitialSpeed = _firstWeaponInitialSpeed;
                _secondWeaponInitialAtkSpeed = _firstWeaponInitialAtkSpeed;
                
                // We don't call ApplyAllUpgrades here because it's the same instance 
                // and it was already upgraded during SetUpFirstWeaponConfig.
            }
            else
            {
                _secondWeaponInitialDamage = _secondWeaponConfig.WeaponDamage;
                _secondWeaponInitialPierce = _secondWeaponConfig.WeaponPierce;
                _secondWeaponInitialRange = _secondWeaponConfig.WeaponRange;
                _secondWeaponInitialKnockback = _secondWeaponConfig.WeaponKnockback;
                _secondWeaponInitialSpeed = _secondWeaponConfig.WeaponSpeed;
                _secondWeaponInitialAtkSpeed = _secondWeaponConfig.WeaponAtkSpeed;
                
                ApplyAllUpgrades(_secondWeaponConfig,
                    _secondWeaponInitialDamage,
                    _secondWeaponInitialRange,
                    _secondWeaponInitialSpeed,
                    _secondWeaponInitialPierce,
                    _secondWeaponInitialKnockback, 
                    _secondWeaponInitialAtkSpeed);
            }
        }

        private void ConfigureAllUpgrades()
        {
            _damage = GetUpgrade(UpgradesCategoryType.Damage);
            //_pierce = GetUpgrade(UpgradesCategoryType.Pierce);
            _range = GetUpgrade(UpgradesCategoryType.Range);
            //_knockback = GetUpgrade(UpgradesCategoryType.Knockback);
            _speed = GetUpgrade(UpgradesCategoryType.Speed);
           // _attackSpeed = GetUpgrade(UpgradesCategoryType.AttackSpeed);
           _weapon = GetUpgrade(UpgradesCategoryType.Tomahawk);

        }
        
        private void ApplyAllUpgrades(WeaponConfig config, 
            float initialDamage, 
            float initialRange, 
            float initialSpeed, 
            int initialPierce,
            float initialKnockback,
            float initialAtkSpeed)
        {
            float weaponMultiplier = _weapon != null ? _weapon.GetMultiplier() : 1f;
         
            config.WeaponDamage = initialDamage * _damage.GetMultiplier() * weaponMultiplier;
          //  config.WeaponPierce = initialPierce + (int)_pierce.GetMultiplier();
            config.WeaponPierce = Mathf.CeilToInt(initialPierce * weaponMultiplier);
            config.WeaponRange  = initialRange  * _range.GetMultiplier() * weaponMultiplier;
          //  config.WeaponKnockback = initialKnockback + (int)_knockback.GetMultiplier();
            config.WeaponKnockback = initialKnockback * weaponMultiplier;
            config.WeaponSpeed  = initialSpeed  * _speed.GetMultiplier() * weaponMultiplier;
           // config.WeaponAtkSpeed = initialAtkSpeed / _attackSpeed.GetMultiplier();
           config.WeaponAtkSpeed = initialAtkSpeed / weaponMultiplier;
        }

        private void RefreshAllWeaponStats()
        {
            if (_firstWeaponConfig != null)
            {
                ApplyAllUpgrades(_firstWeaponConfig, 
                    _firstWeaponInitialDamage,
                    _firstWeaponInitialRange, 
                    _firstWeaponInitialSpeed, 
                    _firstWeaponInitialPierce, 
                    _firstWeaponInitialKnockback, 
                    _firstWeaponInitialAtkSpeed);
            }
            if (_secondWeaponConfig != null)
            {
                ApplyAllUpgrades(_secondWeaponConfig,
                    _secondWeaponInitialDamage,
                    _secondWeaponInitialRange,
                    _secondWeaponInitialSpeed,
                    _secondWeaponInitialPierce,
                    _secondWeaponInitialKnockback, 
                    _secondWeaponInitialAtkSpeed);
            }
        }

        

        
        
    }

}
