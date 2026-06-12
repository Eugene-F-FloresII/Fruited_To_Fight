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
using UnityEditor.PackageManager;


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
        
        private WispConfig _leftWispConfig;
        private WispConfig _centerWispConfig;
        private WispConfig _rightWispConfig;


        private UpgradeData _damage;
        private UpgradeData _pierce;
        private UpgradeData _range;
        private UpgradeData _knockback;
        private UpgradeData _speed;
        private UpgradeData _attackSpeed;
        private UpgradeData _tomahawk;
        private UpgradeData _lightningWisp;
        
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

        private float _leftWispInitialDamage;
        private float _leftWispInitialRange;
        private float _leftWispInitialAtkSpeed;
        private float _leftWispInitialProjectileSpeed;

        private float _centerWispInitialDamage;
        private float _centerWispInitialRange;
        private float _centerWispInitialAtkSpeed;
        private float _centerWispInitialProjectileSpeed;

        private float _rightWispInitialDamage;
        private float _rightWispInitialRange;
        private float _rightWispInitialAtkSpeed;
        private float _rightWispInitialProjectileSpeed;

        private bool _lightningWispPicked;
        
        public WeaponConfig FirstWeaponConfig => _firstWeaponConfig;
        public WeaponConfig SecondWeaponConfig => _secondWeaponConfig;

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
            Events_Wisps.OnChosenWisp += InitializeCurrentWisp;
            Events_Game.OnGameRestarted += ResetAllUpgrades;
            Events_Game.OnGameExited += ResetAllUpgrades;
        }

        private void OnDisable()
        {
            Events_Weapons.OnChosenWeapon -= InitializeCurrentWeapon;
            Events_Wisps.OnChosenWisp -= InitializeCurrentWisp;
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
            if (_tomahawk.GetUpgradeLevelMaxed()) return seed;

            WeaponConfig target = null;
            if (_firstWeaponConfig != null && _firstWeaponConfig.WeaponClass == WeaponClass.Tomahawk)
                target = _firstWeaponConfig;
            else if (_secondWeaponConfig != null && _secondWeaponConfig.WeaponClass == WeaponClass.Tomahawk)
                target = _secondWeaponConfig;

            if (target == null) return seed;

            UpgradeWeaponResult result = _tomahawk.BuyWeaponUpgrade(seed, target.WeaponDamage, target.WeaponSpeed, target.WeaponRange);
            
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

        public int UpgradeLightningWisp(int seed)
        {
            if (!_lightningWispPicked)
            {
                _lightningWispPicked = true;
                Events_Wisps.OnChosenWisp?.Invoke("LightningWisp");
                UpgradeWispResult initialResult = _lightningWisp.BuyWispUpgrade(seed, 0, 0, 0, 0);
                return initialResult.Currency;
            }

            WispConfig target = null;
            if (_leftWispConfig != null && _leftWispConfig.WispType == WispType.Lightning)
                target = _leftWispConfig;
            else if (_centerWispConfig != null && _centerWispConfig.WispType == WispType.Lightning)
                target = _centerWispConfig;
            else if (_rightWispConfig != null && _rightWispConfig.WispType == WispType.Lightning)
                target = _rightWispConfig;

            if (target == null) return seed;

            if (_lightningWisp.GetUpgradeLevelMaxed()) return seed;

            UpgradeWispResult result = _lightningWisp.BuyWispUpgrade(seed, target.Damage, target.Range, target.AtkSpeed, target.ProjectileSpeed);

            target.Damage += result.Damage;
            target.Range += result.Range;
            target.AtkSpeed += result.AtkSpeed;
            target.ProjectileSpeed += result.ProjectileSpeed;

            return result.Currency;
        }
        
        
        public void ResetAllUpgrades()
        {
            if (_firstWeaponConfig != null && _firstWeaponConfig.Afflictions != null)
            {
                _firstWeaponConfig.ResetAfflictions();
            }

            if (_secondWeaponConfig != null && _secondWeaponConfig.Afflictions != null)
            {
                _secondWeaponConfig.ResetAfflictions();
            }
            
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

            if (_leftWispConfig != null)
            {
                _leftWispConfig.Damage = _leftWispInitialDamage;
                _leftWispConfig.Range = _leftWispInitialRange;
                _leftWispConfig.AtkSpeed = _leftWispInitialAtkSpeed;
                _leftWispConfig.ProjectileSpeed = _leftWispInitialProjectileSpeed;
            }

            if (_centerWispConfig != null)
            {
                _centerWispConfig.Damage = _centerWispInitialDamage;
                _centerWispConfig.Range = _centerWispInitialRange;
                _centerWispConfig.AtkSpeed = _centerWispInitialAtkSpeed;
                _centerWispConfig.ProjectileSpeed = _centerWispInitialProjectileSpeed;
            }

            if (_rightWispConfig != null)
            {
                _rightWispConfig.Damage = _rightWispInitialDamage;
                _rightWispConfig.Range = _rightWispInitialRange;
                _rightWispConfig.AtkSpeed = _rightWispInitialAtkSpeed;
                _rightWispConfig.ProjectileSpeed = _rightWispInitialProjectileSpeed;
            }

            foreach (var upgrades in _upgradesList)
            {
                upgrades.ResetAllDataValues();
            }
            
            
            Debug.Log("Upgrades, Weapon and Wisp Configs have been reset to initial values.");
        }

        public bool AreAllLevelsMaxed()
        {
            return _upgradesList.All(upgrade => upgrade.GetUpgradeLevelMaxed());        
        }

        public bool CanUpgradeAfflictions()
        {
            if (_firstWeaponConfig != null && _firstWeaponConfig.Afflictions.Count < 1)
            {
                return true;
            }

            if (_secondWeaponConfig != null && _secondWeaponConfig.Afflictions.Count < 1)
            {
                return true;
            }

            return false;
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

        private async void InitializeCurrentWisp(string label)
        {
            var handle = Addressables.LoadAssetAsync<WispConfig>(label);
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                InitializeWispConfig(handle.Result);
            }
            else
            {
                Debug.LogError($"Failed to load WispConfig with label \'{label}\'");
            }
        }

        private void InitializeWispConfig(WispConfig wispConfig)
        {
            if (_leftWispConfig == null)
            {
                _leftWispConfig = wispConfig;
                SetUpLeftWispConfig();
            }
            else if (_centerWispConfig == null)
            {
                _centerWispConfig = wispConfig;
                SetUpCenterWispConfig();
            }
            else if (_rightWispConfig == null)
            {
                _rightWispConfig = wispConfig;
                SetUpRightWispConfig();
            }
            else
            {
                Debug.Log("all wisp slots are occupied");
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

        private void SetUpLeftWispConfig()
        {
            _leftWispInitialDamage = _leftWispConfig.Damage;
            _leftWispInitialRange = _leftWispConfig.Range;
            _leftWispInitialAtkSpeed = _leftWispConfig.AtkSpeed;
            _leftWispInitialProjectileSpeed = _leftWispConfig.ProjectileSpeed;
        }

        private void SetUpCenterWispConfig()
        {
            _centerWispInitialDamage = _centerWispConfig.Damage;
            _centerWispInitialRange = _centerWispConfig.Range;
            _centerWispInitialAtkSpeed = _centerWispConfig.AtkSpeed;
            _centerWispInitialProjectileSpeed = _centerWispConfig.ProjectileSpeed;
        }

        private void SetUpRightWispConfig()
        {
            _rightWispInitialDamage = _rightWispConfig.Damage;
            _rightWispInitialRange = _rightWispConfig.Range;
            _rightWispInitialAtkSpeed = _rightWispConfig.AtkSpeed;
            _rightWispInitialProjectileSpeed = _rightWispConfig.ProjectileSpeed;
        }

        private void ConfigureAllUpgrades()
        {
            _damage = GetUpgrade(UpgradesCategoryType.Damage);
            //_pierce = GetUpgrade(UpgradesCategoryType.Pierce);
            _range = GetUpgrade(UpgradesCategoryType.Range);
            //_knockback = GetUpgrade(UpgradesCategoryType.Knockback);
            _speed = GetUpgrade(UpgradesCategoryType.Speed);
           // _attackSpeed = GetUpgrade(UpgradesCategoryType.AttackSpeed);
           _tomahawk = GetUpgrade(UpgradesCategoryType.Tomahawk);
           
           _lightningWisp = GetUpgrade(UpgradesCategoryType.LightningWisp);

        }
        
        private void ApplyAllUpgrades(WeaponConfig config, 
            float initialDamage, 
            float initialRange, 
            float initialSpeed, 
            int initialPierce,
            float initialKnockback,
            float initialAtkSpeed)
        {
            float weaponMultiplier = _tomahawk != null ? _tomahawk.GetMultiplier() : 1f;
         
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
