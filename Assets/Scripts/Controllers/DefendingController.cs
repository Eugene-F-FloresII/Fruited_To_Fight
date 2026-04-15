using System;
using Data;
using Obvious.Soap;
using Unity.VisualScripting;
using UnityEngine;


namespace Controllers
{
    public class DefendingController : MonoBehaviour
    {
        [SerializeField] private CharacterConfig _characterConfig;
        [SerializeField] private FloatVariable _characterHealth;
        private float _initialHealth;

        private void Awake()
        {
            ConfigureStats();
        }


        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out EnemyController enemyController))
            {
                
                _characterHealth.Value -= enemyController.GotHitByEnemy();
                enemyController.KillEnemy();
                
                if (_characterHealth.Value <= 0)
                {
                    GameOver();
                }
            }
        }


        private void ConfigureStats()
        {
            _initialHealth = _characterConfig.CharacterHealth;
            _characterHealth.Value = _initialHealth;
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
