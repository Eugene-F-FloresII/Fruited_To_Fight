using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Data
{
    [System.Serializable]
    public struct PowerUpSpawnData
    {
        public AssetReferenceT<PowerUpConfig> PowerUpConfigReference;
        [Range(0f, 1f)] public float SpawnChance; // Acts as selection weight (0.0 to 1.0)
    }

    [CreateAssetMenu(fileName = "PowerUpManagerConfig", menuName = "Data/Create PowerUp Manager Config")]
    public class PowerUpManagerConfig : ScriptableObject
    {
        [Range(0f, 1f)] public float GlobalDropChance = 0.15f;
        public List<PowerUpSpawnData> PowerUpSpawnList = new List<PowerUpSpawnData>();
    }
}
