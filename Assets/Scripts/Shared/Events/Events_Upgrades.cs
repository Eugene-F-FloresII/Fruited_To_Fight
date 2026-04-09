using System;
using Data;
using Shared.Enums;

namespace Shared.Events
{
    public static class Events_Upgrades 
    {
        public static Action<UpgradesPanelType, bool> OnHoveredUpgrade{ get; set; }
        public static Action<UpgradesWeaponSlot> OnActiveUpgradeWeaponSlotChanged { get; set; }
        public static Action<UpgradesWeaponSlot, UpgradesPanelType> OnUpgradeDataChanged { get; set; }
        public static Action<WeaponConfig, bool> OnChosenWeapon { get; set;}
    }

}
