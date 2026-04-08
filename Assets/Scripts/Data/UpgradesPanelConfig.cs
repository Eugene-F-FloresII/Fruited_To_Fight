using Shared.Enums;
using UnityEngine;


namespace Data
{
    [CreateAssetMenu(fileName = "UpgradePanelConfig", menuName = "Data/Upgrade Panel Configuration")]
    public class UpgradesPanelConfig : ScriptableObject
    {
        public UpgradesPanelType UpgradesPanelType;
        public bool IsFirstWeapon;
        [TextArea] public string Description;
    }

}
