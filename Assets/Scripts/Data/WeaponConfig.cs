using UnityEngine;


namespace Data
{
    [CreateAssetMenu(menuName = "Data/Create Weapon Config", fileName = "WeaponConfig")]
    public class WeaponConfig : ScriptableObject
    {
        public int WeaponID;

        public string WeaponName;
        public float WeaponDamage;
        public float WeaponAtkSpeed;
    }

}
