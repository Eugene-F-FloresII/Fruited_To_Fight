using Shared.Enums;

namespace Shared.Events
{
    public struct DamageSourceInfo
    {
        public WeaponClass WeaponClass;
        public AfflictionType AfflictionType;
        public WispType WispType;
        public bool IsWeapon;
        public bool IsAffliction;
        public bool IsWisp;
        public AfflictionType TargetAfflictionType;
        public bool HasTargetAffliction;

        public static DamageSourceInfo Default => new DamageSourceInfo();

        public static DamageSourceInfo FromWeapon(WeaponClass weaponClass)
        {
            return new DamageSourceInfo
            {
                WeaponClass = weaponClass,
                IsWeapon = true
            };
        }

        public static DamageSourceInfo FromAffliction(AfflictionType afflictionType)
        {
            return new DamageSourceInfo
            {
                AfflictionType = afflictionType,
                IsAffliction = true
            };
        }

        public static DamageSourceInfo FromWisp(WispType wispType)
        {
            return new DamageSourceInfo
            {
                WispType = wispType,
                IsWisp = true
            };
        }
    }
}
