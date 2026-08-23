using Shared.Enums;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Data
{
    [CreateAssetMenu(menuName = "Data/Create Affliction Config", fileName = "AfflictionConfig")]
    public class AfflictionConfig : ScriptableObject
    {
        public AfflictionType Type;
        public float Duration;
        public float Power;
        public int MaxStacks = 5;
        public float ExplosionRadius = 5f;
        public float LightningStrikeDelay = 0.5f;
        public AssetReferenceGameObject VFXPrefabReference;
        public AssetReferenceGameObject EnemyVisualPrefabReference;
        public AssetReferenceGameObject HitEffectPrefab;
    }
}
