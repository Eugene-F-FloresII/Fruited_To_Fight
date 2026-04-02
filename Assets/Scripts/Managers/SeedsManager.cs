using System;
using Data;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Gameplay.Seed;
using UnityEngine.AddressableAssets;

namespace Managers
{
    public class SeedsManager : MonoBehaviour
    {
        [SerializeField] private AssetReferenceT<SeedConfig> _seedConfigReference;
        
        private SeedConfig _seedConfig;
        private Seed _seed;
        
        private void Start()
        {
            InitializeSeedManager().Forget();
        }

        private async UniTask InitializeSeedManager()
        {
            _seedConfig = await _seedConfigReference.LoadAssetAsync<SeedConfig>();
            _seed = _seedConfig.SeedPrefab;
        }
    }

}
