using System;
using Obvious.Soap;
using PrimeTween;
using Shared.Events;
using UnityEngine;


namespace Gameplay
{
    public class HealthBar : MonoBehaviour
    {
        [Header("Health Bar References")]
        [SerializeField] private GameObject _healthBar;
        [SerializeField] private AudioClip _audioClip;


        [Header("SOAP reference")]
        [SerializeField] private FloatVariable _characterHealth;

        private Vector3 _baseLocalPosition;
        private Tween _shakeTween;
        private float _lastHealthValue;

        private void Awake()
        {
            if (_healthBar != null)
            {
                _baseLocalPosition = _healthBar.transform.localPosition;
            }
        }

        private void OnEnable()
        {
            if (_characterHealth != null)
            {
                _lastHealthValue = _characterHealth.Value;
                _characterHealth.OnValueChanged += OnHealthValueChanged;
            }
        }

        private void OnDisable()
        {
            if (_characterHealth != null)
            {
                _characterHealth.OnValueChanged -= OnHealthValueChanged;
            }

            _shakeTween.Stop();
        }


        private void OnHealthValueChanged(float newValue)
        {
            if (newValue < _lastHealthValue)
            {
                if (_healthBar != null)
                {
                    Events_Sound.PlaySound?.Invoke(_audioClip);
                    
                    _shakeTween.Stop();
                    _healthBar.transform.localPosition = _baseLocalPosition;
                    // Increased strength slightly and added frequency for a more noticeable shake
                    _shakeTween = Tween.ShakeLocalPosition(_healthBar.transform, strength: new Vector3(10f, 10f, 0f), duration: 0.2f, frequency: 20);
                }
            }
            _lastHealthValue = newValue;
        }
    }
}
