using System.Collections.Generic;
using NaughtyAttributes;
using Shared.Enums;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;

namespace Data
{
    [CreateAssetMenu(fileName = "WispConfig", menuName = "Data/Create Wisp Config")]
    public class WispConfig : ScriptableObject
    {
        public float Damage;
        public float Range;
        public float AtkSpeed;
        public float ProjectileSpeed;
        public WispType WispType;
        
        
        [Header("Afflictions")]
        public List<AfflictionConfig> Afflictions;

        public event System.Action OnAfflictionsChanged;


        [Button("Test Remove Affliction")]
        public void ResetAfflictions()
        {
            Afflictions.Clear();
            OnAfflictionsChanged?.Invoke();
        }
        
        [Button("Test Add Affliction")]
        public void AddAffliction()
        {
            AddAffliction("LightningAffliction").Forget();
        }
        
        public async UniTask AddAffliction(string afflictionKey)
        {
            var handle = Addressables.LoadAssetAsync<AfflictionConfig>(afflictionKey);
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Afflictions.Add(handle.Result);
                OnAfflictionsChanged?.Invoke();
            }
            else
            {
                Debug.LogError($"Failed to load UpgradeData with key '{afflictionKey}'");
            }
        }
    }

}
