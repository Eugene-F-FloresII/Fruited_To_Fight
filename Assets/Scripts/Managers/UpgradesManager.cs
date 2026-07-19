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
        private struct WeaponStatsSnapshot
        {
            public float Damage;
            public int Pierce;
            public float Range;
            public float Knockback;
            public float Speed;
            public float AtkSpeed;

            public WeaponStatsSnapshot(WeaponConfig config)
            {
                Damage = config.WeaponDamage;
                Pierce = config.WeaponPierce;
                Range = config.WeaponRange;
                Knockback = config.WeaponKnockback;
                Speed = config.WeaponSpeed;
                AtkSpeed = config.WeaponAtkSpeed;
            }

            public void RestoreTo(WeaponConfig config)
            {
                config.WeaponDamage = Damage;
                config.WeaponPierce = Pierce;
                config.WeaponRange = Range;
                config.WeaponKnockback = Knockback;
                config.WeaponSpeed = Speed;
                config.WeaponAtkSpeed = AtkSpeed;
            }
        }

        private struct WispStatsSnapshot
        {
            public float Damage;
            public float Range;
            public float AtkSpeed;
            public float ProjectileSpeed;

            public WispStatsSnapshot(WispConfig config)
            {
                Damage = config.Damage;
                Range = config.Range;
                AtkSpeed = config.AtkSpeed;
                ProjectileSpeed = config.ProjectileSpeed;
            }

            public void RestoreTo(WispConfig config)
            {
                config.Damage = Damage;
                config.Range = Range;
                config.AtkSpeed = AtkSpeed;
                config.ProjectileSpeed = ProjectileSpeed;
            }
        }

        [Header("Lists of Upgrades")]
        [SerializeField] private List<UpgradeData> _upgradesList;
        [SerializeField] private List<UpgradesCategoryType> _upgradesCategoryList;
        
        private Dictionary<UpgradesCategoryType, UpgradeData> _upgradesDictionary = new Dictionary<UpgradesCategoryType, UpgradeData>();
        
        private List<WeaponConfig> _activeWeapons = new List<WeaponConfig>(2);
        private List<WispConfig> _activeWisps = new List<WispConfig>(3);

        private Dictionary<WeaponConfig, WeaponStatsSnapshot> _weaponInitialStats = new Dictionary<WeaponConfig, WeaponStatsSnapshot>();
        private Dictionary<WispConfig, WispStatsSnapshot> _wispInitialStats = new Dictionary<WispConfig, WispStatsSnapshot>();

        private UpgradeData _damage;
        private UpgradeData _pierce;
        private UpgradeData _range;
        private UpgradeData _knockback;
        private UpgradeData _speed;
        private UpgradeData _attackSpeed;
        private UpgradeData _tomahawk;
        private UpgradeData _staff;
        private UpgradeData _spear;
        private UpgradeData _lightningWisp;
        
        private bool _lightningWispPicked;
        
        public WeaponConfig FirstWeaponConfig => _activeWeapons.Count > 0 ? _activeWeapons[0] : null;
        public WeaponConfig SecondWeaponConfig => _activeWeapons.Count > 1 ? _activeWeapons[1] : null;

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
            if (_damage.GetUpgradeLevelMaxed() || _activeWeapons.Count == 0) return seed;
            
            UpgradeResult result = _damage.BuyUpgrade(seed, _activeWeapons[0].WeaponDamage);
            
            foreach (var weapon in _activeWeapons)
            {
                weapon.WeaponDamage += result.Value;
            }
            
            return result.Currency;     
        }
        
        public int UpgradeRange(int seed)
        {
            if (_range.GetUpgradeLevelMaxed() || _activeWeapons.Count == 0) return seed;
            
            UpgradeResult result = _range.BuyUpgrade(seed, _activeWeapons[0].WeaponRange);
            
            foreach (var weapon in _activeWeapons)
            {
                weapon.WeaponRange += result.Value;
            }
            
            return result.Currency;     
        }

        public int UpgradeSpeed(int seed)
        {
            if (_speed.GetUpgradeLevelMaxed() || _activeWeapons.Count == 0) return seed;
            
            UpgradeResult result = _speed.BuyUpgrade(seed, _activeWeapons[0].WeaponSpeed);
            
            foreach (var weapon in _activeWeapons)
            {
                weapon.WeaponSpeed += result.Value;
            }
            
            return result.Currency;  
        }
        
        public int UpgradeTomahawk(int seed)
        {
            if (_tomahawk.GetUpgradeLevelMaxed()) return seed;

            WeaponConfig target = _activeWeapons.FirstOrDefault(w => w.WeaponClass == WeaponClass.Tomahawk);
            if (target == null) return seed;

            UpgradeWeaponResult result = _tomahawk.BuyWeaponUpgrade(seed, target.WeaponDamage, target.WeaponSpeed, target.WeaponRange);
            
            foreach (var weapon in _activeWeapons.Where(w => w.WeaponClass == WeaponClass.Tomahawk))
            {
                weapon.WeaponDamage += result.Damage;
                weapon.WeaponSpeed += result.Speed;
                weapon.WeaponRange += result.Range;
            }

            return result.Currency;
        }

        /// <summary>
        /// Purchases the upgrade for the Staff weapon, increasing its damage, speed, and range stats.
        /// </summary>
        /// <param name="seed">The current seed count (currency).</param>
        /// <returns>The remaining seed count after purchase.</returns>
        public int UpgradeStaff(int seed)
        {
            if (_staff.GetUpgradeLevelMaxed()) return seed;

            WeaponConfig target = _activeWeapons.FirstOrDefault(w => w.WeaponClass == WeaponClass.Staff);
            if (target == null) return seed;

            UpgradeWeaponResult result = _staff.BuyWeaponUpgrade(seed, target.WeaponDamage, target.WeaponSpeed, target.WeaponRange);
            
            foreach (var weapon in _activeWeapons.Where(w => w.WeaponClass == WeaponClass.Staff))
            {
                weapon.WeaponDamage += result.Damage;
                weapon.WeaponSpeed += result.Speed;
                weapon.WeaponRange += result.Range;
            }

            return result.Currency;
        }

        /// <summary>
        /// Purchases the upgrade for the Spear weapon, increasing its damage, speed, and range stats.
        /// </summary>
        /// <param name="seed">The current seed count (currency).</param>
        /// <returns>The remaining seed count after purchase.</returns>
        public int UpgradeSpear(int seed)
        {
            if (_spear.GetUpgradeLevelMaxed()) return seed;

            WeaponConfig target = _activeWeapons.FirstOrDefault(w => w.WeaponClass == WeaponClass.Spear);
            if (target == null) return seed;

            UpgradeWeaponResult result = _spear.BuyWeaponUpgrade(seed, target.WeaponDamage, target.WeaponSpeed, target.WeaponRange);
            
            foreach (var weapon in _activeWeapons.Where(w => w.WeaponClass == WeaponClass.Spear))
            {
                weapon.WeaponDamage += result.Damage;
                weapon.WeaponSpeed += result.Speed;
                weapon.WeaponRange += result.Range;
            }

            return result.Currency;
        }

        public int UpgradeLightningWisp(int seed)
        {
            if (!_lightningWispPicked)
            {
                Debug.Log("Bought LightningWisp");
                _lightningWispPicked = true;
                Events_Wisps.OnChosenWisp?.Invoke("LightningWisp");
                UpgradeWispResult initialResult = _lightningWisp.BuyWispUpgrade(seed, 0, 0, 0, 0);
                return initialResult.Currency;
            }

            WispConfig target = _activeWisps.FirstOrDefault(w => w.WispType == WispType.Lightning);
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
            foreach (var weapon in _activeWeapons)
            {
                if (weapon.Afflictions != null)
                {
                    weapon.ResetAfflictions();
                }

                if (_weaponInitialStats.TryGetValue(weapon, out var snapshot))
                {
                    snapshot.RestoreTo(weapon);
                }
            }

            foreach (var wisp in _activeWisps)
            {
                if (_wispInitialStats.TryGetValue(wisp, out var snapshot))
                {
                    snapshot.RestoreTo(wisp);
                }
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
            return _activeWeapons.Any(weapon => weapon.Afflictions.Count < 1);
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
            if (_activeWeapons.Count >= 2)
            {
                Debug.Log("both arms are occupied");
                return;
            }

            _activeWeapons.Add(weaponConfig);

            if (!_weaponInitialStats.ContainsKey(weaponConfig))
            {
                var snapshot = new WeaponStatsSnapshot(weaponConfig);
                _weaponInitialStats.Add(weaponConfig, snapshot);
                
                ApplyAllUpgrades(weaponConfig, 
                    snapshot.Damage,
                    snapshot.Range, 
                    snapshot.Speed, 
                    snapshot.Pierce, 
                    snapshot.Knockback, 
                    snapshot.AtkSpeed);
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
            if (_activeWisps.Count >= 3)
            {
                Debug.Log("all wisp slots are occupied");
                return;
            }

            _activeWisps.Add(wispConfig);

            if (!_wispInitialStats.ContainsKey(wispConfig))
            {
                _wispInitialStats.Add(wispConfig, new WispStatsSnapshot(wispConfig));
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
            _tomahawk = GetUpgrade(UpgradesCategoryType.Tomahawk);
           _staff = GetUpgrade(UpgradesCategoryType.Staff);
           _spear = GetUpgrade(UpgradesCategoryType.Spear);
           
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
            float weaponMultiplier = 1f;
            if (config.WeaponClass == WeaponClass.Tomahawk && _tomahawk != null)
            {
                weaponMultiplier = _tomahawk.GetMultiplier();
            }
            else if (config.WeaponClass == WeaponClass.Staff && _staff != null)
            {
                weaponMultiplier = _staff.GetMultiplier();
            }
            else if (config.WeaponClass == WeaponClass.Spear && _spear != null)
            {
                weaponMultiplier = _spear.GetMultiplier();
            }
         
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

        private void RefreshAllWeaponStats()
        {
            foreach (var weapon in _activeWeapons)
            {
                if (_weaponInitialStats.TryGetValue(weapon, out var snapshot))
                {
                    ApplyAllUpgrades(weapon, 
                        snapshot.Damage,
                        snapshot.Range, 
                        snapshot.Speed, 
                        snapshot.Pierce, 
                        snapshot.Knockback, 
                        snapshot.AtkSpeed);
                }
            }
        }

        

        
        
    }

}
