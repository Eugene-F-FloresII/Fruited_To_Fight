using UnityEngine;
using Shared.Enums;


namespace Data.Upgrades
{
    public class UpgradeWisp : UpgradeData
    {
        public override UpgradeWispResult BuyWispUpgrade(int seed, float damageValue, float rangeValue, float atkSpeedValue, float projectileSpeedValue)
        {
            if (GetSeedPriceUpgrade() > seed)
            {
                Debug.Log("Not enough seeds");
                return new UpgradeWispResult { Currency = seed, Damage = damageValue, Range = rangeValue, AtkSpeed = atkSpeedValue, ProjectileSpeed = projectileSpeedValue };
            }

            if (UpgradeLevel.Value > MaxLevel)
            {
                Debug.Log("Level Maxed");
                IsMaxed = true;
                return new UpgradeWispResult { Currency = seed, Damage = damageValue, Range = rangeValue, AtkSpeed = atkSpeedValue, ProjectileSpeed = projectileSpeedValue };
            }
            
            int currency = seed - GetSeedPriceUpgrade();
            UpgradeLevel.Value++;

            float resultDamage;
            float resultAtkSpeed;
            float resultProjectileSpeed;
            float resultRange;
          
            resultDamage = (damageValue * GetMultiplier()) - damageValue;
            resultAtkSpeed = (atkSpeedValue * GetMultiplier()) - atkSpeedValue;
            resultProjectileSpeed = (projectileSpeedValue * GetMultiplier()) - projectileSpeedValue;
            resultRange = (rangeValue * GetMultiplier()) - rangeValue;
        
            
            return new UpgradeWispResult { Currency = currency, Damage = resultDamage, Range = resultRange, AtkSpeed = resultAtkSpeed, ProjectileSpeed = resultProjectileSpeed };
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
