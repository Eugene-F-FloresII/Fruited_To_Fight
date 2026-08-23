using UnityEngine;
using Shared.Events;
using Obvious.Soap;
using System;
using Data;

namespace Gameplay.Enemies
{
    public class EnemyHealth : MonoBehaviour
    {
        private EnemyStats _enemyStats;

        private float _currentHealth;
        private float _maxHealth;

        public Action OnDeathEvent { get; set; }
        public Action OnHitEvent { get; set; }

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth;

        /// <summary>
        /// Initializes the reference to EnemyStats for config-driven audio.
        /// </summary>
        public void Initialize(EnemyStats enemyStats)
        {
            _enemyStats = enemyStats;
        }

        public void InitializeHealth(float maxHealth)
        {
            _maxHealth = maxHealth;
            _currentHealth = maxHealth;
        }

        public void ApplyDamage(float damage, DamageSourceInfo sourceInfo)
        {
            _currentHealth -= damage;
            Events_Enemy.OnEnemyHit?.Invoke(transform.position, Mathf.RoundToInt(damage), sourceInfo);
            
            OnHitEvent?.Invoke();
            
            if (_currentHealth <= 0)
            {
                KillEnemy();
            }
        }

        public void KillEnemy()
        {
            if (_enemyStats != null && _enemyStats.Config != null && _enemyStats.Config.DeathSFX != null)
            {
                Events_Sound.PlaySoundWithVolume?.Invoke(_enemyStats.Config.DeathSFX, _enemyStats.Config.DeathSFXVolume);
            }
            Events_Seed.OnEnemyDeath?.Invoke(transform);
            Events_Enemy.OnEnemyDeath?.Invoke();
            OnDeathEvent?.Invoke();
            gameObject.SetActive(false);
        }
    }
}
