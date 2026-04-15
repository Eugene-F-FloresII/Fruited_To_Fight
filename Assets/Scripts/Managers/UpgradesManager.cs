using Collection;
using Data;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using Obvious.Soap;
using Shared.Events;


namespace Managers
{
    public class UpgradesManager : MonoBehaviour
    {
        [Header("Settings")] 
        [SerializeField] private float _percentageIncreasePerLevel = 0.25f;
        
        [Header("Soap Level References")] 
        [SerializeField] private IntVariable _overallDamageLevel;
        [SerializeField] private IntVariable _overallRangeLevel;
        [SerializeField] private IntVariable _overallSpeedLevel;
        
        private WeaponConfig _firstWeaponConfig;
        private WeaponConfig _secondWeaponConfig;
        
        private float _overallRange;
        private float _overallDamage;
        private int _overallPierce;
        private float _overallSpeed;
        private float _overallAtkSpeed;
        private float _overallKnockbackForce;
        private int _overallMaxLevel = 3;
        
        private int _priceDamageUpgrade = 10;
        private int _priceRangeUpgrade = 7;
        private int _priceSpeedUpgrade = 9;
        
        
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

        private bool _isDamageMaxed;
        private bool _isRangedMaxed;
        private bool _isSpeedMaxed;

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        private void OnEnable()
        {
            Events_Weapons.OnChosenWeapon += InitializeCurrentWeapon;
            Events_Game.OnGameRestarted += ResetAllUpgrades;
        }

        private void OnDisable()
        {
            Events_Weapons.OnChosenWeapon -= InitializeCurrentWeapon;
            Events_Game.OnGameRestarted -= ResetAllUpgrades;
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<UpgradesManager>();
        }

        public int UpgradeOverallDamage(int seed)
        {
            if (GetSeedPriceDamageUpgrade() > seed)
            {
                Debug.Log("Not enough seeds");
                return seed;
            }

            if (_overallDamageLevel.Value > _overallMaxLevel)
            {
                _isDamageMaxed = true;
                return seed;
            }

            int currency = seed - GetSeedPriceDamageUpgrade();
            
            _overallDamageLevel.Value++;
            
            if (_overallDamageLevel.Value > _overallMaxLevel) _isDamageMaxed = true;
            
            if (_firstWeaponConfig != null)
            {
                _firstWeaponConfig.WeaponDamage = _firstWeaponInitialDamage * GetDamageMultiplier();
                _firstWeaponConfig.WeaponPierce = _firstWeaponInitialPierce * Mathf.RoundToInt(GetDamageMultiplier());
            }

            if (_secondWeaponConfig != null)
            {
                _secondWeaponConfig.WeaponDamage = _secondWeaponInitialDamage * GetDamageMultiplier();
                _secondWeaponConfig.WeaponPierce = _secondWeaponInitialPierce * Mathf.RoundToInt(GetDamageMultiplier());
            }
            
            return currency;
        }
        
        public int UpgradeOverallRange(int seed)
        {
            if (GetSeedPriceRangeUpgrade() > seed)
            {
                Debug.Log("Not enough seeds");
                return seed;
            }

            if (_overallRangeLevel.Value > _overallMaxLevel)
            {
                _isRangedMaxed = true;
                return seed;
            }

            int currency = seed - GetSeedPriceRangeUpgrade();
            
            _overallRangeLevel.Value++;
            
            if (_overallRangeLevel.Value > _overallMaxLevel) _isRangedMaxed = true;
            
            if (_firstWeaponConfig != null)
            {
                _firstWeaponConfig.WeaponRange = _firstWeaponInitialRange * GetRangeMultiplier();
                _firstWeaponConfig.WeaponKnockback = _firstWeaponInitialKnockback * GetRangeMultiplier();
            }

            if (_secondWeaponConfig != null)
            {
                _secondWeaponConfig.WeaponRange = _secondWeaponInitialRange * GetRangeMultiplier();
                _secondWeaponConfig.WeaponKnockback = _secondWeaponInitialKnockback * GetRangeMultiplier();
            }
            
            return currency;
        }
        
        public int UpgradeOverallSpeed(int seed)
        {
            if (GetSeedPriceSpeedUpgrade() > seed)
            {
                Debug.Log("Not enough seeds");
                return seed;
            }
            
            if (_overallSpeedLevel.Value > _overallMaxLevel)
            {
                _isSpeedMaxed = true;
                return seed;
            }

            int currency = seed - GetSeedPriceSpeedUpgrade();
            _overallSpeedLevel.Value++;
            
            if (_overallSpeedLevel.Value > _overallMaxLevel) _isSpeedMaxed = true;
            
            if (_firstWeaponConfig != null)
            {
                _firstWeaponConfig.WeaponSpeed = _firstWeaponInitialSpeed * GetSpeedMultiplier();
                _firstWeaponConfig.WeaponAtkSpeed = _firstWeaponInitialAtkSpeed / GetSpeedMultiplier();
            }
            
            if (_secondWeaponConfig != null)
            {
                _secondWeaponConfig.WeaponSpeed = _secondWeaponInitialSpeed * GetSpeedMultiplier();
                _secondWeaponConfig.WeaponAtkSpeed = _secondWeaponInitialAtkSpeed / GetSpeedMultiplier();
            }
            
            return currency;
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

            if (_overallDamageLevel != null) _overallDamageLevel.Value = 1;
            if (_overallRangeLevel != null) _overallRangeLevel.Value = 1;
            if (_overallSpeedLevel != null) _overallSpeedLevel.Value = 1;

            _isDamageMaxed = false;
            _isRangedMaxed = false;
            _isSpeedMaxed = false;
            
            Debug.Log("Upgrades and Weapon Configs have been reset to initial values.");
        }

        public bool AreAllLevelsMaxed()
        {
            return GetDamageLevelMaxed() && GetRangedLevelMaxed() && GetSpeedLevelMaxed();
        }

        public bool GetDamageLevelMaxed() => _overallDamageLevel.Value >= _overallMaxLevel;
        public bool GetRangedLevelMaxed() => _overallRangeLevel.Value >= _overallMaxLevel;
        public bool GetSpeedLevelMaxed() => _overallSpeedLevel.Value >= _overallMaxLevel;

        public int GetSeedPriceDamageUpgrade()
        {
            return _overallDamageLevel.Value * _priceDamageUpgrade;
        }
        public int GetSeedPriceRangeUpgrade()
        {
            return _overallRangeLevel.Value * _priceRangeUpgrade;
        }
        public int GetSeedPriceSpeedUpgrade()
        {
            return _overallSpeedLevel.Value * _priceSpeedUpgrade;
        }
        
        public float GetDamageMultiplier()
        {
            // Example: each level adds 10% damage
            return 1f + (_overallDamageLevel.Value * _percentageIncreasePerLevel);
        }
        public float GetRangeMultiplier()
        {
            // Example: each level adds 10% damage
            return 1f + (_overallRangeLevel.Value * _percentageIncreasePerLevel);
        }

        public float GetSpeedMultiplier()
        {
            // Example: each level adds 10% damage
            return 1f + (_overallSpeedLevel.Value * _percentageIncreasePerLevel);
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
        
        private void ApplyAllUpgrades(WeaponConfig config, 
            float initialDamage, 
            float initialRange, 
            float initialSpeed, 
            int initialPierce,
            float initialKnockback,
            float initialAtkSpeed)
        {
            config.WeaponDamage = initialDamage * GetDamageMultiplier();
            config.WeaponPierce = initialPierce * (int) GetDamageMultiplier();
            config.WeaponRange  = initialRange  * GetRangeMultiplier();
            config.WeaponKnockback = initialKnockback * GetRangeMultiplier();
            config.WeaponSpeed  = initialSpeed  * GetSpeedMultiplier();
            config.WeaponAtkSpeed = initialAtkSpeed / GetSpeedMultiplier();
        }

        

        
        
    }

}
