using System;
using Data;
using Obvious.Soap;
using Shared.Events;
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

        private void OnEnable()
        {
            Events_Game.OnGameRestarted += OnGameRestarted;
        }

        private void OnDisable()
        {
            Events_Game.OnGameRestarted -= OnGameRestarted;
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
            if (_characterConfig != null)
            {
                _initialHealth = _characterConfig.CharacterHealth;
                _characterHealth.Value = _initialHealth;
            }
        }

        private void OnGameRestarted()
        {
            ConfigureStats();
        }

        private void ResetStats()
        {
            if (_characterConfig != null)
            {
                _characterConfig.CharacterHealth = _initialHealth;
            }
        }

        private void GameOver()
        {
            // Game over logic
            Debug.Log("Corndalf ded");
            gameObject.SetActive(false);
            ResetStats();
        }
    }
}
