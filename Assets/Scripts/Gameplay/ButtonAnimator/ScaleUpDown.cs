using UnityEngine;
using UnityEngine.EventSystems;
using PrimeTween;
using Shared.Events;

namespace Gameplay.ButtonAnimator
{
    [RequireComponent(typeof(RectTransform))]
    public class ScaleUpDown : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private AudioClip _hoverAudioClip;
        [SerializeField] private AudioClip _clickAudioClip;
        
        [SerializeField] private float _scaleFactor = 1.15f;
        [SerializeField] private float _duration = 0.2f;
        [SerializeField] private Ease _ease = Ease.OutQuad;

        private RectTransform _rectTransform;
        private Tween _currentTween;

        private Vector3 _initialScale;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _initialScale = _rectTransform.localScale;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _currentTween.Stop();
            // Using useUnscaledTime: true to ensure UI animation works even when paused
            _currentTween = Tween.Scale(_rectTransform, _scaleFactor, _duration, _ease, useUnscaledTime: true);
            
            Events_Sound.PlaySound?.Invoke(_hoverAudioClip);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Events_Sound.PlaySound?.Invoke(_clickAudioClip);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _currentTween.Stop();
            // Using useUnscaledTime: true for consistent behavior
            _currentTween = Tween.Scale(_rectTransform, _initialScale, _duration, _ease, useUnscaledTime: true);
        }

        private void OnDestroy()
        {
            _currentTween.Stop();
        }
    }
}
