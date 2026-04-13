using Shared.Enums;
using UnityEngine;


namespace Data
{
    [CreateAssetMenu(fileName = "UpgradePanelConfig", menuName = "Data/Upgrade Panel Configuration")]
    public class UpgradesPanelConfig : ScriptableObject
    {
        public UpgradesCategoryType UpgradesCategoryType;
        public bool IsFirstWeapon;
        [TextArea] public string Description;
    }

}
