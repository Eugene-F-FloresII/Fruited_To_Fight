using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Shared.Events;
using PrimeTween;
using UnityEngine.UI;

namespace Gameplay.Weapons
{
    public class WeaponPicker : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _animationDuration = 0.3f;
        [SerializeField] private float _buttonStaggerDelay = 0.4f;
        [SerializeField] private float _buttonAnimationDuration = 0.2f;
        
        [Header("Button References")]
        [SerializeField] private Button _spearButton;
        [SerializeField] private Button _staffButton;
        [SerializeField] private Button _tomahawkButton;

        private int _countIndex;

        private void Awake()
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Start()
        {
            Time.timeScale = 0;
            TurnOnCanvasGroup();
        }

        private void Update()
        {
            if (_countIndex == 2)
            {
                TurnOffCanvasGroup();
                Time.timeScale = 1;
                _countIndex = 0;
            }
        }

        public void OnPickedSpear()
        {
            Events_Weapons.OnChosenWeapon?.Invoke("SpearConfig");
            
            // Bounce on click
            Tween.PunchScale(_spearButton.transform, new Vector3(-0.2f, -0.2f, 0), _buttonAnimationDuration, useUnscaledTime: true);
            
            _countIndex++;
        }

        public void OnPickedStaff()
        {
            Events_Weapons.OnChosenWeapon?.Invoke("StaffConfig");
            
            // Bounce on click
            Tween.PunchScale(_staffButton.transform, new Vector3(-0.2f, -0.2f, 0), _buttonAnimationDuration, useUnscaledTime: true);
            
            _countIndex++;
        }
        
        public void OnPickedTomahawk()
        {
            Events_Weapons.OnChosenWeapon?.Invoke("TomahawkConfig");
            
            // Bounce on click
            Tween.PunchScale(_tomahawkButton.transform, new Vector3(-0.2f, -0.2f, 0), _buttonAnimationDuration, useUnscaledTime: true);
            
            _countIndex++;
        }

        public void TurnOffCanvasGroup()
        {
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            
            Tween.StopAll(transform);
            Tween.StopAll(_spearButton.transform);
            Tween.StopAll(_staffButton.transform);
            Tween.StopAll(_tomahawkButton.transform);

            // Staggered deactivation sequence
            var seq = Sequence.Create(useUnscaledTime: true);
            
            if (_spearButton.gameObject.activeSelf)
                seq.Group(Tween.Scale(_spearButton.transform, 0, _buttonAnimationDuration, Ease.InBack, useUnscaledTime: true));
            
            if (_staffButton.gameObject.activeSelf)
                seq.Chain(Tween.Delay(_buttonStaggerDelay, useUnscaledTime: true))
                   .Group(Tween.Scale(_staffButton.transform, 0, _buttonAnimationDuration, Ease.InBack, useUnscaledTime: true));

            if (_tomahawkButton.gameObject.activeSelf)
                seq.Chain(Tween.Delay(_buttonStaggerDelay, useUnscaledTime: true))
                   .Group(Tween.Scale(_tomahawkButton.transform, 0, _buttonAnimationDuration, Ease.InBack, useUnscaledTime: true));

            seq.Chain(Tween.Alpha(_canvasGroup, 0, _animationDuration, useUnscaledTime: true))
               .Group(Tween.Scale(transform, 0, _animationDuration, Ease.InBack, useUnscaledTime: true));
        }
        
        public void TurnOnCanvasGroup()
        {
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            
            Tween.StopAll(transform);
            Tween.StopAll(_spearButton.transform);
            Tween.StopAll(_staffButton.transform);
            Tween.StopAll(_tomahawkButton.transform);
            
            // Set initial state manually
            _canvasGroup.alpha = 0f;
            transform.localScale = Vector3.zero;
            
            _spearButton.gameObject.SetActive(true);
            _staffButton.gameObject.SetActive(true);
            _tomahawkButton.gameObject.SetActive(true);
            _spearButton.transform.localScale = Vector3.zero;
            _staffButton.transform.localScale = Vector3.zero;
            _tomahawkButton.transform.localScale = Vector3.zero;

            // Activation sequence: After Panel Animation
            Sequence.Create(useUnscaledTime: true)
                .Group(Tween.Alpha(_canvasGroup, 1f, _animationDuration, useUnscaledTime: true))
                .Group(Tween.Scale(transform, 1f, _animationDuration, Ease.OutBack, useUnscaledTime: true))
                .Chain(Tween.Scale(_spearButton.transform, 1f, _buttonAnimationDuration, Ease.OutBack, useUnscaledTime: true))
                .Chain(Tween.Delay(_buttonStaggerDelay, useUnscaledTime: true))
                .Chain(Tween.Scale(_staffButton.transform, 1f, _buttonAnimationDuration, Ease.OutBack, useUnscaledTime: true))
                .Chain(Tween.Delay(_buttonStaggerDelay, useUnscaledTime: true))
                .Chain(Tween.Scale(_tomahawkButton.transform, 1f, _buttonAnimationDuration, Ease.OutBack, useUnscaledTime: true));
        }
        
    }

}
