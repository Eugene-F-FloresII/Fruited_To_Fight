using Gameplay.Powerups;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "PowerUpConfig", menuName = "Data/Creat PowerUp Config")]
    public class PowerUpConfig : ScriptableObject
    {
        public PowerUp PowerUp;
        public float PowerUpDuration;
        public float DespawnDuration;
        public AudioClip PowerUpSound;

        [Header("Healing Potion Settings")] 
        public float HealAmount;
    }


}
