using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "CharacterConfig", menuName = "Character/Create Character Config")]
    public class CharacterConfig : ScriptableObject
    {
        public int CharacterId;
        
        public string CharacterName;
        public float CharacterSpeed;
        
    }

}
