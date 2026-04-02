using Gameplay.Seed;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "SeedConfig", menuName = "Data/Seed Configuration")]
    public class SeedConfig : ScriptableObject
    {
        public Seed SeedPrefab;
        
        public float SeedSpeed;
        public float SeedSeekerRadius;
        public int Seedpoint;
        
    }

}
