using System.Collections.Generic;
using Data;
using Shared.Events;
using UnityEngine;

namespace Gameplay
{
    public class PlayerCharacter : MonoBehaviour
    {
        [Header("Configurations")]
        [SerializeField] private WeaponConfig _weaponConfig;
        [SerializeField] private CharacterConfig _characterConfig;
        
        [Header("List of Player Preferences")]
        [SerializeField] private List<GameObject> _characters;
        [SerializeField] private List<GameObject> _weapons;
        
        [Header("Animators **Don't touch**")]
        public Animator CharacterAnimator;
        public Animator WeaponAnimator;

        private void Awake()
        {
            CharacterWeaponReady(_weaponConfig, _characterConfig);
        }

        private void OnEnable()
        {
            Events_Character.OnCharacterChosen += ChosenCharacter;
        }

        private void OnDisable()
        {
            Events_Character.OnCharacterChosen -= ChosenCharacter;
        }

        private void CharacterWeaponReady(WeaponConfig weaponConfig, CharacterConfig characterConfig)
        {
            foreach (GameObject character in _characters) character.SetActive(false);
            foreach (GameObject weapon in _weapons)  weapon.SetActive(false);
            
            _weapons[weaponConfig.WeaponID].SetActive(true);
            _characters[characterConfig.CharacterId].SetActive(true);
            
            if (_characters[characterConfig.CharacterId].TryGetComponent(out Animator characterAnimator)) CharacterAnimator = characterAnimator;
            if (_weapons[weaponConfig.WeaponID].TryGetComponent(out Animator weaponAnimator)) WeaponAnimator = weaponAnimator;
        }

        private void ChosenCharacter(CharacterConfig characterConfig)
        {
            _characterConfig = characterConfig;
        }
    }

}
