using Controllers;
using Data;
using UnityEngine;

namespace Gameplay.Upgrades
{
    public class UpgradeItemWeapon : UpgradeItemController
    {
        [Header("Weapon Settings")]
        [SerializeField] private WeaponConfig _weaponConfig;
    }

}
