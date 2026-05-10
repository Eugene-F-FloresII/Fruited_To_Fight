using UnityEngine;
using Shared.Enums;

namespace Data.Upgrades
{
    [CreateAssetMenu(menuName = "Data/Create Weapon Upgrade Data")]
    public class UpgradeWeapon: UpgradeData
    {
        public override UpgradeWeaponResult BuyWeaponUpgrade(int seed, float damageValue, float speedValue, float rangeValue)
        {
            if (GetSeedPriceUpgrade() > seed)
            {
                Debug.Log("Not enough seeds");
                return new UpgradeWeaponResult { Currency = seed, Damage = damageValue, Speed = speedValue, Range = rangeValue};
            }

            if (UpgradeLevel.Value > MaxLevel)
            {
                Debug.Log("Level Maxed");
                IsMaxed = true;
                return new UpgradeWeaponResult {Currency = seed, Damage = damageValue, Speed =  speedValue, Range = rangeValue};
            }
            
            int currency = seed - GetSeedPriceUpgrade();
            UpgradeLevel.Value++;

            float value;
            float resultDamage;
            float resultSpeed;
            float resultRange;
            float damage;
            float speed;
            float range;
          
            damage = damageValue * GetMultiplier();
            resultDamage = damage - damageValue;
            
            speed = speedValue * GetMultiplier();
            resultSpeed = speed - speedValue;
            
            range = rangeValue * GetMultiplier();
            resultRange = range - rangeValue;
        
            
            return new UpgradeWeaponResult {Currency = currency, Damage =  resultDamage, Speed = resultSpeed, Range = resultRange};
        }

        public override UpgradeResult BuyUpgrade(int seed, float initialValue)
        {
           throw new  System.NotImplementedException();
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
