using System;
using Data;
using Obvious.Soap;
using Shared.Enums;
using Shared.Events;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Controllers
{
    public class DefendingController : MonoBehaviour
    {
        [SerializeField] private CharacterConfig _characterConfig;
        [SerializeField] private FloatVariable _characterHealth;
        private float _initialHealth;
        private GameCondition _gameCondition;

        private void Awake()
        {
            ConfigureStats();
        }

        private void OnEnable()
        {
            Events_Game.OnGameRestarted += OnGameRestarted;
            Events_Game.OnGameExited += OnGameRestarted;
        }

        private void OnDisable()
        {
            Events_Game.OnGameRestarted -= OnGameRestarted;
            Events_Game.OnGameExited -= OnGameRestarted;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out EnemyController enemyController))
            {
                _characterHealth.Value += enemyController.GotHitByEnemy();
                enemyController.KillEnemy();
                
                if (_characterHealth.Value >= 100)
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
            ResetStats();
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
            _gameCondition = GameCondition.Win;
            // Game over logic
            Debug.Log("Corndalf ded");
            gameObject.SetActive(false);
            ResetStats();
            Events_Game.OnGameFinished?.Invoke(_gameCondition);
        }
    }
}