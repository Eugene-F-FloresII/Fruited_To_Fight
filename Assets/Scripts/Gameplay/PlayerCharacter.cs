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
            CharacterWeaponReady(_characterConfig);
        }

        private void OnEnable()
        {
            Events_Character.OnCharacterChosen += ChosenCharacter;
            Events_Weapons.OnSpawnedWeapon += OnSpawnedWeapon;
        }

        
        private void OnDisable()
        {
            Events_Character.OnCharacterChosen -= ChosenCharacter;
            Events_Weapons.OnSpawnedWeapon -= OnSpawnedWeapon;
        }
        
        private void OnSpawnedWeapon(GameObject obj)
        {
            _weapons.Add(obj);
        }
        
        private void CharacterWeaponReady(CharacterConfig characterConfig)
        {
            foreach (GameObject character in _characters) character.SetActive(false);
            
            _characters[characterConfig.CharacterId].SetActive(true);
            
            CharacterAnimator = _characters[characterConfig.CharacterId].GetComponentInChildren<Animator>();
        }

        private void ChosenCharacter(CharacterConfig characterConfig)
        {
            _characterConfig = characterConfig;
            CharacterWeaponReady(_characterConfig);
        }
    }

}
