using System;
using Obvious.Soap;
using Shared.Events;
using UnityEngine;

namespace Gameplay
{
    public class PlayerHealth : MonoBehaviour
    {
        [Header("SOAP References")]
        [SerializeField] private FloatVariable _characterHealth;
        [SerializeField] private FloatVariable _characterMaxHealth;

        public event Action OnDeath;
        public event Action<float> OnDamageTaken;

        public float CurrentHealth => _characterHealth != null ? _characterHealth.Value : 0f;
        public float MaxHealth => _characterMaxHealth != null ? _characterMaxHealth.Value : 0f;

        private void OnEnable()
        {
            Events_PowerUps.Healing += HealPlayer;
        }

        private void OnDisable()
        {
            Events_PowerUps.Healing -= HealPlayer;
        }

        public void InitializeHealth(float maxHealth)
        {
            if (_characterHealth != null && _characterHealth == 0)
            {
                _characterHealth.Value = maxHealth;
            }
            if (_characterMaxHealth != null)
            {
                _characterMaxHealth.Value = maxHealth;
            }
        }

        public void TakeDamage(float damage)
        {
            if (damage <= 0) return;

            if (_characterHealth != null)
            {
                _characterHealth.Value -= damage;
                OnDamageTaken?.Invoke(damage);

                if (_characterHealth.Value <= 0)
                {
                    OnDeath?.Invoke();
                }
            }
        }

        public void HealPlayer(float healAmount)
        {
            if (healAmount <= 0) return;

            if (_characterHealth != null && _characterHealth.Value > 0 && _characterMaxHealth != null)
            {
                _characterHealth.Value = Mathf.Min(_characterHealth.Value + healAmount, _characterMaxHealth.Value);
            }
        }
    }
}
