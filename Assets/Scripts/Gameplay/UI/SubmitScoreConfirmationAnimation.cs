using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;

namespace Gameplay.UI
{
    /// <summary>
    /// Handles purely visual pop-up and pop-out animations for the SubmitScoreConfirmationPanel 
    /// and its loading status content using PrimeTween.
    /// </summary>
    public class SubmitScoreConfirmationAnimation : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _contentConfirmation;
        [SerializeField] private RectTransform _contentLoadingSubmission;

        [Header("Animation Settings")]
        [SerializeField] private float _duration = 0.3f;
        [SerializeField] private Ease _scaleEase = Ease.OutBack;
        [SerializeField] private Ease _scalePopoutEase = Ease.InBack;
        [SerializeField] private Ease _fadeEase = Ease.OutQuad;
        [SerializeField] private float _startScale = 0.3f;

        private void Awake()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }
        }

        /// <summary>
        /// Pop-up transition for the confirmation form content and fades in the CanvasGroup.
        /// </summary>
        public void PlayPopUp()
        {
            if (_canvasGroup != null)
            {
                Tween.Alpha(_canvasGroup, endValue: 1f, duration: _duration, ease: _fadeEase, useUnscaledTime: true);
            }

            if (_contentConfirmation != null)
            {
                _contentConfirmation.gameObject.SetActive(true);
                _contentConfirmation.localScale = Vector3.one * _startScale;
                Tween.Scale(_contentConfirmation, endValue: 1f, duration: _duration, ease: _scaleEase, useUnscaledTime: true);
            }
        }

        /// <summary>
        /// Pop-out transition for the confirmation form and fades out the CanvasGroup.
        /// </summary>
        public async UniTask PlayPopOutAsync()
        {
            var alphaTween = _canvasGroup != null 
                ? Tween.Alpha(_canvasGroup, endValue: 0f, duration: _duration, ease: _fadeEase, useUnscaledTime: true) 
                : default;

            var scaleTween = _contentConfirmation != null 
                ? Tween.Scale(_contentConfirmation, endValue: _startScale, duration: _duration, ease: _scalePopoutEase, useUnscaledTime: true) 
                : default;

            await UniTask.WhenAll(alphaTween.ToUniTask(this), scaleTween.ToUniTask(this));
        }

        /// <summary>
        /// Scales down the confirmation form content only (keeps CanvasGroup open).
        /// </summary>
        public async UniTask PlayFormPopOutOnlyAsync()
        {
            if (_contentConfirmation != null)
            {
                await Tween.Scale(_contentConfirmation, endValue: _startScale, duration: _duration, ease: _scalePopoutEase, useUnscaledTime: true);
            }
        }

        /// <summary>
        /// Pop-up transition for the loading panel content.
        /// </summary>
        public void PlayLoadingPopUp()
        {
            if (_contentLoadingSubmission != null)
            {
                _contentLoadingSubmission.gameObject.SetActive(true);
                _contentLoadingSubmission.localScale = Vector3.one * _startScale;
                Tween.Scale(_contentLoadingSubmission, endValue: 1f, duration: _duration, ease: _scaleEase, useUnscaledTime: true);
            }
        }

        /// <summary>
        /// Pop-out transition for the loading panel content only and fades out CanvasGroup.
        /// </summary>
        public async UniTask PlayLoadingPopOutAsync()
        {
            var alphaTween = _canvasGroup != null 
                ? Tween.Alpha(_canvasGroup, endValue: 0f, duration: _duration, ease: _fadeEase, useUnscaledTime: true) 
                : default;

            var scaleTween = _contentLoadingSubmission != null 
                ? Tween.Scale(_contentLoadingSubmission, endValue: _startScale, duration: _duration, ease: _scalePopoutEase, useUnscaledTime: true) 
                : default;

            await UniTask.WhenAll(alphaTween.ToUniTask(this), scaleTween.ToUniTask(this));
        }

        /// <summary>
        /// Scales down the loading panel content only (keeps CanvasGroup open).
        /// </summary>
        public async UniTask PlayLoadingPopOutOnlyAsync()
        {
            if (_contentLoadingSubmission != null)
            {
                await Tween.Scale(_contentLoadingSubmission, endValue: _startScale, duration: _duration, ease: _scalePopoutEase, useUnscaledTime: true);
            }
        }
    }
}
