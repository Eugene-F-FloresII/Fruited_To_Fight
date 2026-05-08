using Obvious.Soap;
using Shared.Enums;
using UnityEngine;

namespace Data.Upgrades
{
    public struct UpgradeResult
    {
        public int Currency;
        public float Value;
    }
            
    public abstract class UpgradeData : ScriptableObject
    {
   
        [Header("Settings")] 
        public UpgradesCategoryType Category;
        [Range(0.1f, 0.9f)]
        public float PercentageIncreasePerLevel = 0.1f;
        public IntVariable UpgradeLevel;
        public int MaxLevel;
        public int PriceUpgrade;
        public bool IsMaxed;

        protected float InitialPercentageValue;
        protected int InitialUpgradeLevel;
        protected int InitialMaxLevel;
        protected int InitialPriceUpgrade;
        protected bool InitialIsMaxed;
        
        public abstract UpgradeResult BuyUpgrade(int seed, float initialValue);
        public abstract float GetMultiplier(int level = -1);
        public abstract int GetSeedPriceUpgrade();
        public abstract bool GetUpgradeLevelMaxed();
        public abstract bool IsPierceCategoryType();
        public abstract void SetInitialDataValues();
        public abstract void ResetAllDataValues();

    }

}
