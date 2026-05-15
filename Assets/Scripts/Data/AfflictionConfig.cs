using Shared.Enums;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(menuName = "Data/Create Affliction Config", fileName = "AfflictionConfig")]
    public class AfflictionConfig : ScriptableObject
    {
        public AfflictionType Type;
        public float Duration;
        public float Power;
        public int MaxStacks = 5;
    }
}
