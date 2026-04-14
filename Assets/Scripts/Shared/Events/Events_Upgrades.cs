using System;
using Data;
using Shared.Enums;

namespace Shared.Events
{
    public static class Events_Upgrades 
    {
        public static Action<UpgradesCategoryType, bool> OnHoveredUpgrade{ get; set; }
        public static Action<UpgradesWeaponSlot> OnActiveUpgradeWeaponSlotChanged { get; set; }
        public static Action<UpgradesWeaponSlot, UpgradesCategoryType> OnUpgradeDataChanged { get; set; }
        public static Action<WeaponConfig, bool> OnChosenWeapon { get; set;}
        public static Action OnActivateUpgradePanel {get; set;}
        
        public static Action OnRoundStarted {get; set;}
       
    }

}
