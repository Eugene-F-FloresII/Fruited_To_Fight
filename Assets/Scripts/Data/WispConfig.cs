using Shared.Enums;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "WispConfig", menuName = "Data/Create Wisp Config")]
    public class WispConfig : ScriptableObject
    {
        public float Damage;
        public float Range;
        public float ProjectileSpeed;
        public WispType WispType;
    }

}
