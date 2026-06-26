using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using Obvious.Soap;
using PrimeTween;
using Shared.Events;
using TMPro;

namespace Gameplay.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ResultSystemPanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Transform _contentResult;
        [SerializeField] private GameObject _imgStarPrefab;
        [SerializeField] private Transform _imgHollowStarsContainer;
        [SerializeField] private Button _btnBackToMainMenu;
        [SerializeField] private TextMeshProUGUI _txtResultTitle;

        [Header("Audio")]
        [SerializeField] private AudioClip _starSpawnSound;

        [Header("SOAP Variables")]
        [SerializeField] private FloatVariable _playerHealth;
        [SerializeField] private FloatVariable _playerMaxHealth;
        [SerializeField] private IntVariable _currentRounds;
        [SerializeField] private IntVariable _maxRounds;

        [Header("Objectives UI checkmarks")]
        [SerializeField] private GameObject _objective1Star; // Img_ObjectiveStar for Objective 1
        [SerializeField] private GameObject _objective2Star; // Img_ObjectiveStar for Objective 2
        [SerializeField] private GameObject _objective3Star; // Img_ObjectiveStar for Objective 3

        private void Awake()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }

            if (_contentResult == null)
            {
                _contentResult = transform.Find("Content_Result");
            }
            
            // Hide the panel at start
            HidePanel();
        }

        private void OnEnable()
        {
            Events_Game.OnShowResultPanel += ShowResults;
            if (_btnBackToMainMenu != null)
            {
                _btnBackToMainMenu.onClick.AddListener(OnBackToMainMenuClicked);
            }
        }

        private void OnDisable()
        {
            Events_Game.OnShowResultPanel -= ShowResults;
            if (_btnBackToMainMenu != null)
            {
                _btnBackToMainMenu.onClick.RemoveListener(OnBackToMainMenuClicked);
            }
        }

        private void HidePanel()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
        }

        private void ShowResults(bool didWin)
        {
            // Pause the game so nothing moves in the background
            Time.timeScale = 0f;

            // Ensure parent CanvasGroup (e.g. UICanvas) is turned on
            var rootCanvasGroup = transform.parent?.GetComponentInParent<CanvasGroup>();
            if (rootCanvasGroup != null)
            {
                rootCanvasGroup.alpha = 1f;
                rootCanvasGroup.blocksRaycasts = true;
                rootCanvasGroup.interactable = true;
            }

            // Hide HUD elements
            var rootTransform = transform.parent?.parent;
            if (rootTransform != null)
            {
                var header = rootTransform.Find("HeaderCanvas")?.gameObject;
                if (header != null) header.SetActive(false);

                var footer = rootTransform.Find("FooterCanvas")?.gameObject;
                if (footer != null) footer.SetActive(false);
            }

            // Set Title text
            if (_txtResultTitle != null)
            {
                _txtResultTitle.text = didWin ? "LEVEL COMPLETED!" : "GAME OVER";
            }

            // Determine objectives met
            bool obj1Met = false;
            if (_playerHealth != null && _playerMaxHealth != null)
            {
                obj1Met = Mathf.Approximately(_playerHealth.Value, _playerMaxHealth.Value) || (_playerHealth.Value >= _playerMaxHealth.Value);
            }

            bool obj2Met = false;
            if (_currentRounds != null && _maxRounds != null)
            {
                obj2Met = _currentRounds.Value >= _maxRounds.Value;
            }

            bool obj3Met = didWin;

            // Update objectives UI checkmarks/stars
            if (_objective1Star != null) _objective1Star.SetActive(obj1Met);
            if (_objective2Star != null) _objective2Star.SetActive(obj2Met);
            if (_objective3Star != null) _objective3Star.SetActive(obj3Met);

            // Collect met objectives list
            List<bool> objectiveResults = new List<bool> { obj1Met, obj2Met, obj3Met };

            // Start show animation
            ShowPanelWithAnimation(objectiveResults).Forget();
        }

        private async UniTaskVoid ShowPanelWithAnimation(List<bool> objectiveResults)
        {
            // Set initial state
            _canvasGroup.alpha = 1f; // The panel itself appears immediately!
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = true;

            if (_contentResult != null)
            {
                _contentResult.localScale = Vector3.one * 0.3f;
                // Animate scale of Content_Result
                await Tween.Scale(_contentResult, endValue: 1f, duration: 0.4f, ease: Ease.OutBack, useUnscaledTime: true);
            }

            // Clear any previously instantiated stars
            foreach (Transform child in _imgHollowStarsContainer)
            {
                Destroy(child.gameObject);
            }

            // Instantiate and animate stars sequentially
            await AnimateStarsSequence(objectiveResults);
        }

        private async UniTask AnimateStarsSequence(List<bool> objectiveResults)
        {
            // Count how many stars to instantiate
            int starsCount = 0;
            foreach (bool met in objectiveResults)
            {
                if (met) starsCount++;
            }

            for (int i = 0; i < starsCount; i++)
            {
                if (this == null || gameObject == null || _imgStarPrefab == null || _imgHollowStarsContainer == null)
                    break;

                // Instantiate star
                GameObject starGo = Instantiate(_imgStarPrefab, _imgHollowStarsContainer);
                
                // Animate star from scale 5 to normal (1) using PrimeTween
                starGo.transform.localScale = Vector3.one * 5f;
                Tween.Scale(starGo.transform, endValue: 1f, duration: 0.4f, ease: Ease.OutBack, useUnscaledTime: true);

                // Play star spawn sound
                if (_starSpawnSound != null)
                {
                    Events_Sound.PlaySound?.Invoke(_starSpawnSound);
                }

                // Wait 0.5s before next star (unscaled time because Time.timeScale = 0)
                await UniTask.Delay(TimeSpan.FromSeconds(0.5f), delayType: DelayType.UnscaledDeltaTime);
            }
        }

        private void OnBackToMainMenuClicked()
        {
            HidePanelWithAnimation().Forget();
        }

        private async UniTaskVoid HidePanelWithAnimation()
        {
            _canvasGroup.interactable = false;

            if (_contentResult != null)
            {
                // Animate scale of Content_Result out and fade out the panel background
                Tween.Alpha(_canvasGroup, endValue: 0f, duration: 0.3f, useUnscaledTime: true);
                await Tween.Scale(_contentResult, endValue: 0.3f, duration: 0.3f, ease: Ease.InBack, useUnscaledTime: true);
            }
            else
            {
                Tween.Alpha(_canvasGroup, endValue: 0f, duration: 0.3f, useUnscaledTime: true);
                await UniTask.Delay(TimeSpan.FromSeconds(0.3f), delayType: DelayType.UnscaledDeltaTime);
            }

            // Resume the game time scale
            Time.timeScale = 1f;

            HidePanel();
            Events_Game.OnSceneChange?.Invoke("MainMenu");
        }
    }
}
