using UnityEngine;
using PrimeTween;

namespace Gameplay.ButtonAnimator
{
    public class LoopingScaleUpDown : MonoBehaviour
    {
        [SerializeField] private float _scaleFactor = 1.15f;
        [SerializeField] private float _duration = 0.2f;
        [SerializeField] private Ease _ease = Ease.OutQuad;
        
        private Vector3 _initialScale;
        private Tween _tween;

        private void Awake()
        {
            _initialScale = transform.localScale;
        }

        private void OnEnable()
        {
            // Set to "up" scale first
            transform.localScale = _initialScale * _scaleFactor;
            
            // Loop from "up" to "normal" (initial) and back
            _tween = Tween.Scale(transform, _initialScale, _duration, _ease, cycles: -1, cycleMode: CycleMode.Yoyo, useUnscaledTime: true);
        }

        private void OnDisable()
        {
            _tween.Stop();
            // Reset to normal when disabled
            transform.localScale = _initialScale;
        }

        private void OnDestroy()
        {
            _tween.Stop();
        }
    }
}
