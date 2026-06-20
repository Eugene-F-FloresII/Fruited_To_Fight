using Shared.Events;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Gameplay.Powerups
{
    public class HealingPotion : PowerUp
    {
        public override void UsePowerUp()
        {
            Events_PowerUps.Healing?.Invoke(_powerUpConfig.HealAmount);
        }
    }

}
