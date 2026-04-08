using Shared.Enums;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Data
{
    [CreateAssetMenu(menuName = "Data/Create Weapon Config", fileName = "WeaponConfig")]
    public class WeaponConfig : ScriptableObject
    {
        [Header("Weapon Settings")] 
        public AssetReferenceT<GameObject> WeaponSpawner;
        public GameObject WeaponPrefab;
        public string WeaponName;
        public int WeaponID;
        public WeaponType WeaponType;
        public int WeaponAmountToPool;
        public bool WeaponHoming;
        
        [Header("Weapon Statistics")]
        public float WeaponRange;
        public float WeaponDamage;
        public int WeaponPierce;
        public float WeaponSpeed;
        public float WeaponAtkSpeed;
        public float WeaponKnockback;
        
        [Header("Weapon Upgrade Desc")]
        [TextArea] public string DamageDescription;
        [TextArea] public string RangeDescription;
        [TextArea] public string SpeedDescription;
    }

}
