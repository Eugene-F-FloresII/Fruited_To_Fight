using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Data;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using Obvious.Soap;
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
        [SerializeField] private IntVariable _atkSpeedLevels;

        private bool _configureFirstWeapon;
        private bool _isFirstOccupied;
        private bool _isSecondOccupied;

        private float _firstWeaponInitialDamage;
        private int _firstWeaponInitialPierce;
        private float _firstWeaponInitialRange;
        private float _firstWeaponInitialKnockback;
        private float _firstWeaponInitialSpeed;
        private float _firstWeaponInitialAtkSpeed;


        private float _firstDamagePercentage = 0.25f;
        private float _firstRangePercentage = 0.25f;
        private float _firstSpeedPercentage = 0.25f;
        
        private int _priceDamageUpgrade = 10;
        private int _priceRangeUpgrade = 7;
        private int _priceSpeedUpgrade = 9;
        
        private void OnEnable()
        {
            Events_Weapons.OnChosenWeapon += CurrentWeapon;
        }

        private void OnDisable()
        {
            Events_Weapons.OnChosenWeapon -= CurrentWeapon;
        }

        public void UpgradeFirstWeaponDamage(int seed)
        { 
           
            if (_priceDamageUpgrade > seed)
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
            
            int bought = _priceDamageUpgrade - seed;
            _seeds.Value = bought;
            
            _firstWeaponConfig.WeaponDamage = (_firstWeaponConfig.WeaponDamage + (_firstWeaponInitialDamage * _firstDamagePercentage));
            _firstWeaponConfig.WeaponPierce = (_firstWeaponConfig.WeaponPierce + Mathf.RoundToInt(_firstWeaponInitialPierce * _firstDamagePercentage));
            _priceDamageUpgrade += (int)(_priceDamageUpgrade * _firstDamagePercentage);
            _firstDamagePercentage += _firstDamagePercentage;
            _firstDamageLevels.Value++;
            
        }
        
        public void UpgradeFirstWeaponRange(int seed)
        { 
           
            if (_priceRangeUpgrade > seed)
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
            
            int bought = _priceRangeUpgrade - seed;
            _seeds.Value = bought;
            
            _firstWeaponConfig.WeaponRange = (_firstWeaponConfig.WeaponRange  + (_firstWeaponInitialRange * _firstRangePercentage));
            _firstWeaponConfig.WeaponKnockback = (_firstWeaponConfig.WeaponKnockback + Mathf.RoundToInt(_firstWeaponInitialKnockback * _firstRangePercentage));
            _priceRangeUpgrade += (int)(_priceRangeUpgrade * _firstRangePercentage);
            _firstRangePercentage += _firstRangePercentage;
            _firstRangeLevels.Value++;
            
        }
        
        public void UpgradeFirstWeaponSpeed(int seed)
        { 
           
            if (_priceSpeedUpgrade > seed)
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
            
            int bought = _priceSpeedUpgrade - seed;
            _seeds.Value = bought;
            
            _firstWeaponConfig.WeaponSpeed = (_firstWeaponConfig.WeaponSpeed  + (_firstWeaponInitialSpeed * _firstSpeedPercentage));
            _firstWeaponConfig.WeaponAtkSpeed = (_firstWeaponConfig.WeaponAtkSpeed + Mathf.RoundToInt(_firstWeaponInitialAtkSpeed * _firstSpeedPercentage));
            _priceSpeedUpgrade += (int)(_priceSpeedUpgrade * _firstSpeedPercentage);
            _firstSpeedPercentage += _firstSpeedPercentage;
            _firstAtkSpeedLevels.Value++;
        }
        


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
            }
            else if (!_isSecondOccupied)
            {
                _secondWeaponConfig = weaponConfig;
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
        
    }

}
