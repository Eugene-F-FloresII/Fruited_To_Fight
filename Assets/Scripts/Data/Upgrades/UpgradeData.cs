using Obvious.Soap;
using Shared.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace Data.Upgrades
{
    public struct UpgradeWispResult
    {
        public int Currency;
        public float Damage;
        public float Range;
        public float AtkSpeed;
        public float ProjectileSpeed;
    }
    
    public struct UpgradeWeaponResult
    {
        public int Currency;
        public float Damage;
        public float Speed;
        public float Range;
    }
    
    public struct UpgradeResult
    {
        public int Currency;
        public float Value;
    }
            
    public abstract class UpgradeData : ScriptableObject
    {
   
        [Header("Settings")] 
        public UpgradesCategoryType Category;
        [Range(0.01f, 1.0f)]
        public float PercentageIncreasePerLevel = 0.1f;
        public IntVariable UpgradeLevel;
        public int MaxLevel;
        public int PriceUpgrade;
        public bool IsMaxed;
        public Button ButtonPrefab;

        protected float InitialPercentageValue;
        protected int InitialUpgradeLevel;
        protected int InitialMaxLevel;
        protected int InitialPriceUpgrade;
        protected bool InitialIsMaxed;

        public virtual UpgradeWispResult BuyWispUpgrade(int seed, float damageValue, float rangeValue, float atkSpeedValue, float projectileSpeedValue)
        {
            throw new System.NotImplementedException();
        }
        public virtual UpgradeResult BuyUpgrade(int seed, float initialValue)
        {
            throw new System.NotImplementedException();
        }
        public virtual UpgradeWeaponResult BuyWeaponUpgrade(int seed, float damageValue, float speedValue, float rangeValue)
        {
            throw new System.NotImplementedException();
        }
        public abstract float GetMultiplier(int level = -1);
        public abstract int GetSeedPriceUpgrade();
        public abstract bool GetUpgradeLevelMaxed();
        public abstract bool IsPierceCategoryType();
        public abstract void SetInitialDataValues();
        public abstract void ResetAllDataValues();

    }

}
