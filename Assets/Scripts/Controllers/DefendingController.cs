using System;
using Data;
using Unity.VisualScripting;
using UnityEngine;


namespace Controllers
{
    public class DefendingController : MonoBehaviour
    {
        [SerializeField] private CharacterConfig _characterConfig;

        private float _initialHealth;

        private void Awake()
        {
            ConfigureStats();
        }


        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out EnemyController enemyController))
            {
                
                _characterConfig.CharacterHealth -= enemyController.GotHitByEnemy();
                enemyController.KillEnemy();
                
                if (_characterConfig.CharacterHealth <= 0)
                {
                    GameOver();
                }
            }
        }


        private void ConfigureStats()
        {
            _initialHealth = _characterConfig.CharacterHealth;
        }

        private void ResetStats()
        {
            _characterConfig.CharacterHealth = _initialHealth;
            
        }

        private void GameOver()
        {
            //Game over
            Debug.Log("Corndalf ded");
            gameObject.SetActive(false);
            ResetStats();
        }
    }

}
