using Shared.Enums;
using UnityEngine;

namespace Data.Upgrades
{
    [CreateAssetMenu(menuName = "Data/Create Upgrade Data")]
    public class Upgrades : UpgradeData
    {
        public override UpgradeResult BuyUpgrade(int seed, float initialValue)
        {
            if (GetSeedPriceUpgrade() > seed)
            {
                Debug.Log("Not enough seeds");
                return new UpgradeResult { Currency = seed, Value = initialValue};
            }

            if (UpgradeLevel.Value > MaxLevel)
            {
                Debug.Log("Level Maxed");
                IsMaxed = true;
                return new UpgradeResult {Currency = seed, Value = initialValue};
            }
            
            int currency = seed - GetSeedPriceUpgrade();
            UpgradeLevel.Value++;

            float value;
            if (IsPierceCategoryType())
            {
                value = initialValue + 1;
            }
            else
            {
                value = initialValue * GetMultiplier();
            }
            
            return new UpgradeResult {Currency = currency, Value = Mathf.RoundToInt(value)};
        }

        public override float GetMultiplier(int level = -1)
        {
            if (IsPierceCategoryType())
            {
                return 1;
            }
            
            var targetLevel = level == -1 ? UpgradeLevel.Value : level;
            return 1f + (targetLevel * PercentageIncreasePerLevel);
        }

        public override int GetSeedPriceUpgrade()
        {
            var price = UpgradeLevel.Value * PriceUpgrade;
            
            if (price == 0)
            {
                price = PriceUpgrade / 2;
            }
            return price;
        }

        public override bool GetUpgradeLevelMaxed()
        {
            return IsMaxed;
        }

        public override bool IsPierceCategoryType()
        {
            return Category == UpgradesCategoryType.Pierce || Category == UpgradesCategoryType.Knockback;
        }

        public override void SetInitialDataValues()
        {
            InitialPercentageValue = PercentageIncreasePerLevel;
            InitialUpgradeLevel = UpgradeLevel.Value;
            InitialMaxLevel = MaxLevel;
            InitialPriceUpgrade = PriceUpgrade;
            InitialIsMaxed = IsMaxed;
        }

        public override void ResetAllDataValues()
        {
            PercentageIncreasePerLevel = InitialPercentageValue;
            UpgradeLevel.Value = InitialUpgradeLevel;
            MaxLevel = InitialMaxLevel;
            PriceUpgrade = InitialPriceUpgrade;
            IsMaxed = InitialIsMaxed;
        }
    }

}
