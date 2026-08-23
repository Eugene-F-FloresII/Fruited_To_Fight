using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "CharacterConfig", menuName = "Character/Create Character Config")]
    public class CharacterConfig : ScriptableObject
    {
        public int CharacterId;
        
        public string CharacterName;
        public float CharacterSpeed;
        public float CharacterHealth;
        public float CharacterKnockbackResistance;
        public float CharacterArmor;

        [Header("VFX")]
        public UnityEngine.AddressableAssets.AssetReferenceGameObject DeathVFX;
        public float DeathVFXDuration = 1f;
    }

}
