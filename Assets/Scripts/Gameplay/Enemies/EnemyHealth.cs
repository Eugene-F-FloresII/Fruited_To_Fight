using UnityEngine;
using Shared.Events;
using Obvious.Soap;
using System;

namespace Gameplay.Enemies
{
    public class EnemyHealth : MonoBehaviour
    {
        [SerializeField] private AudioClip _deathAudioClip;
        
        private float _currentHealth;
        private float _maxHealth;
        
        public Action OnDeathEvent { get; set; }
        public Action OnHitEvent { get; set; }

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth;

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
            if (_deathAudioClip != null)
            {
                Events_Sound.PlaySound?.Invoke(_deathAudioClip);
            }
            Events_Seed.OnEnemyDeath?.Invoke(transform);
            Events_Enemy.OnEnemyDeath?.Invoke();
            OnDeathEvent?.Invoke();
            gameObject.SetActive(false);
        }
        
#if UNITY_EDITOR
        public void SetDeathAudioClip(AudioClip clip) => _deathAudioClip = clip;
#endif
    }
}
