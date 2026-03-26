using System.Collections.Generic;
using Data;
using Shared.Events;
using UnityEngine;

namespace Gameplay
{
    public class PlayerCharacter : MonoBehaviour
    {
        [SerializeField] private CharacterConfig _characterConfig;
        [SerializeField] private List<GameObject> _characters;

        private void Start()
        {
            foreach (GameObject character in _characters)
            {
                character.SetActive(false);
            }
            
            _characters[_characterConfig.CharacterId].SetActive(true);
        }

        private void OnEnable()
        {
            Events_Character.OnCharacterChosen += ChosenCharacter;
        }

        private void OnDisable()
        {
            Events_Character.OnCharacterChosen -= ChosenCharacter;
        }

        private void ChosenCharacter(CharacterConfig characterConfig)
        {
            _characterConfig = characterConfig;
        }
    }

}
