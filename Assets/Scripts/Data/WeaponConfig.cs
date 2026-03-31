using UnityEngine;


namespace Data
{
    [CreateAssetMenu(menuName = "Data/Create Weapon Config", fileName = "WeaponConfig")]
    public class WeaponConfig : ScriptableObject
    {
        [Header("Weapon Settings")]
        public GameObject WeaponPrefab;
        public int WeaponID;
        public int WeaponAmountToPool;
        public string WeaponName;
        public bool WeaponHoming;
        
        [Header("Weapon Statistics")]
        public float WeaponRange;
        public float WeaponDamage;
        public int WeaponPierce;
        public float WeaponSpeed;
        public float WeaponAtkSpeed;
        public float WeaponKnockback;
    }

}
