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
        [SerializeField] private List<Upgrades> _upgradesList;
        [SerializeField] private List<UpgradesCategoryType> _upgradesCategoryList;
        
        private Dictionary<UpgradesCategoryType, Upgrades> _upgradesDictionary = new Dictionary<UpgradesCategoryType, Upgrades>();
        
        private WeaponConfig _firstWeaponConfig;
        private WeaponConfig _secondWeaponConfig;

        private Upgrades _damage;
        private Upgrades _pierce;
        private Upgrades _range;
        private Upgrades _knockback;
        private Upgrades _speed;
        private Upgrades _attackSpeed;
        
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
            UpgradeResult result = _damage.BuyUpgrade(seed, _firstWeaponInitialDamage);
            if (_firstWeaponConfig != null)
            {
                _firstWeaponConfig.WeaponDamage = result.Value; 
            }
            if (_secondWeaponConfig != null)
            {
                UpgradeResult secondResult = _damage.BuyUpgrade(seed, _secondWeaponInitialDamage);
                _secondWeaponConfig.WeaponDamage = secondResult.Value;

            }
            return result.Currency;     
        }
        
        public int UpgradeRange(int seed)
        {
            if (_range.GetUpgradeLevelMaxed()) return seed;
            UpgradeResult result = _range.BuyUpgrade(seed, _firstWeaponInitialRange);
            if (_firstWeaponConfig != null)
            {
                _firstWeaponConfig.WeaponRange = result.Value; 
            }
            if (_secondWeaponConfig != null)
            {
                UpgradeResult secondResult = _range.BuyUpgrade(seed, _secondWeaponInitialRange);
                _secondWeaponConfig.WeaponRange = secondResult.Value;

            }
            return result.Currency;     
        }

        public int UpgradeSpeed(int seed)
        {
            if (_speed.GetUpgradeLevelMaxed()) return seed;
            UpgradeResult result = _speed.BuyUpgrade(seed, _firstWeaponInitialSpeed);
            if (_firstWeaponConfig != null)
            {
                _firstWeaponConfig.WeaponSpeed = result.Value; 
            }
            if (_secondWeaponConfig != null)
            {
                UpgradeResult secondResult = _speed.BuyUpgrade(seed, _secondWeaponInitialSpeed);
                _secondWeaponConfig.WeaponSpeed = secondResult.Value;

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

        private Upgrades GetUpgrade(UpgradesCategoryType upgradesCategory)
        {
            if (_upgradesDictionary.TryGetValue(upgradesCategory, out Upgrades upgrade))
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
            
        }
        
        private void ApplyAllUpgrades(WeaponConfig config, 
            float initialDamage, 
            float initialRange, 
            float initialSpeed, 
            int initialPierce,
            float initialKnockback,
            float initialAtkSpeed)
        {
         
            config.WeaponDamage = initialDamage * _damage.GetMultiplier();
          //  config.WeaponPierce = initialPierce + (int)_pierce.GetMultiplier();
            config.WeaponRange  = initialRange  * _range.GetMultiplier();
          //  config.WeaponKnockback = initialKnockback + (int)_knockback.GetMultiplier();
            config.WeaponSpeed  = initialSpeed  * _speed.GetMultiplier();
           // config.WeaponAtkSpeed = initialAtkSpeed / _attackSpeed.GetMultiplier();
        }

        

        
        
    }

}
