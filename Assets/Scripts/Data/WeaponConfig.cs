using Obvious.Soap;
using Shared.Enums;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

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
        public WeaponClass WeaponClass;
        public int WeaponAmountToPool;
        public bool WeaponHoming;
        
        [Header("Weapon Statistics")]
        public IntVariable WeaponLevel;
        public float WeaponRange;
        public float WeaponDamage;
        public int WeaponPierce;
        public float WeaponSpeed;
        public float WeaponAtkSpeed;
        public float WeaponKnockback;

        [Header("Special Ability Settings")] 
        public float AbilityCooldown;
        public float AbilityDuration;

        [Header("Afflictions")]
        public List<AfflictionConfig> Afflictions;


        public void ResetAfflictions()
        {
            Afflictions.Clear();
        }
        
        public async UniTask AddAffliction(string afflictionKey)
        {
            var handle = Addressables.LoadAssetAsync<AfflictionConfig>(afflictionKey);
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Afflictions.Add(handle.Result);
            }
            else
            {
                Debug.LogError($"Failed to load UpgradeData with key '{afflictionKey}'");
            }
        }
        
    }

}
