using Shared.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace Data.Upgrades
{
    [CreateAssetMenu(menuName =  "Data/Create Upgrade Affliction Data")]
    public class UpgradeAfflictionData : ScriptableObject
    {
        public WeaponClass WeaponClass;
        public AfflictionType AfflictionType;
        public string AfflictionKey;
        public Button ButtonPrefab;
        
        
    }

}
