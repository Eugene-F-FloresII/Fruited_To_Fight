using Obvious.Soap;
using Shared.Enums;
using UnityEngine;

namespace Data
{
    public class UpgradeData : ScriptableObject
    {
        [Header("Settings")] 
        public UpgradesCategoryType Category;
        public IntVariable UpgradeLevel;
        public int MaxLevel;
        public int PriceUpgrade;
        public bool IsMaxed;


    }

}
