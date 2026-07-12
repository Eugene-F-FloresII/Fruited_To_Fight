using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Obvious.Soap;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI
{
    /// <summary>
    /// Handles the input validation, button listeners, transition animations, 
    /// and Supabase database submission logic on the SubmitScoreConfirmationPanel.
    /// </summary>
    public class SubmitScoreConfirmationController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private GameObject _contentConfirmation;
        [SerializeField] private GameObject _contentLoadingSubmission;
        [SerializeField] private TMP_InputField _inputName;
        [SerializeField] private TextMeshProUGUI _txtLoading;
        [SerializeField] private Button _btnSubmit;
        [SerializeField] private Button _btnExit;

        [Header("Animation")]
        [SerializeField] private SubmitScoreConfirmationAnimation _animation;

        [Header("SOAP Variables")]
        [SerializeField] private IntVariable _overallScore;
        [SerializeField] private IntVariable _currentRounds;

        [Header("Settings")]
        [SerializeField] private string _defaultName = "JohnFruited";

        private ResultSystemPanel _resultSystemPanel;
        private CancellationTokenSource _cts;

        private void Awake()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }

            _resultSystemPanel = GetComponentInParent<ResultSystemPanel>();
        }

        private void OnEnable()
        {
            if (_btnSubmit != null)
            {
                _btnSubmit.onClick.AddListener(OnSubmitClicked);
            }

            if (_btnExit != null)
            {
                _btnExit.onClick.AddListener(OnExitClicked);
            }

            ResetUI();
        }

        private void OnDisable()
        {
            if (_btnSubmit != null)
            {
                _btnSubmit.onClick.RemoveListener(OnSubmitClicked);
            }

            if (_btnExit != null)
            {
                _btnExit.onClick.RemoveListener(OnExitClicked);
            }

            CancelSequence();
        }

        private void ResetUI()
        {
            if (_contentConfirmation != null)
            {
                _contentConfirmation.SetActive(true);
                _contentConfirmation.transform.localScale = Vector3.one;
            }

            if (_contentLoadingSubmission != null)
            {
                _contentLoadingSubmission.SetActive(false);
            }

            if (_btnSubmit != null)
            {
                _btnSubmit.interactable = true;
            }

            if (_btnExit != null)
            {
                _btnExit.interactable = true;
            }

            if (_inputName != null)
            {
                _inputName.text = string.Empty;
            }
        }

        private void OnExitClicked()
        {
            ExitSequenceAsync().Forget();
        }

        private void OnSubmitClicked()
        {
            SubmitSequenceAsync().Forget();
        }

        private async UniTaskVoid ExitSequenceAsync()
        {
            if (_btnSubmit != null) _btnSubmit.interactable = false;
            if (_btnExit != null) _btnExit.interactable = false;

            if (_animation != null)
            {
                await _animation.PlayPopOutAsync();
            }

            ClosePanel();
        }

        private async UniTaskVoid SubmitSequenceAsync()
        {
            CancelSequence();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            if (_btnSubmit != null) _btnSubmit.interactable = false;
            if (_btnExit != null) _btnExit.interactable = false;

            string playerName = _inputName != null ? _inputName.text : string.Empty;
            if (string.IsNullOrWhiteSpace(playerName))
            {
                playerName = _defaultName;
            }
            else
            {
                playerName = playerName.Trim();
            }

            int score = _overallScore != null ? _overallScore.Value : 0;
            int rounds = _currentRounds != null ? _currentRounds.Value : 0;

            float startTime = Time.realtimeSinceStartup;

            if (_animation != null)
            {
                await _animation.PlayFormPopOutOnlyAsync().AttachExternalCancellation(token);
            }

            if (token.IsCancellationRequested) return;

            if (_contentConfirmation != null)
            {
                _contentConfirmation.SetActive(false);
            }

            if (_contentLoadingSubmission != null)
            {
                _contentLoadingSubmission.SetActive(true);
            }

            if (_txtLoading != null)
            {
                _txtLoading.text = "Submitting Score...";
            }

            if (_animation != null)
            {
                _animation.PlayLoadingPopUp();
            }

            bool success = false;
            var manager = Managers.SupabaseLeaderboardManager.Instance;
            if (manager != null)
            {
                try
                {
                    success = await manager.SubmitScoreAsync(playerName, score, rounds).AttachExternalCancellation(token);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    success = false;
                }
            }
            else
            {
                Debug.LogError("SupabaseLeaderboardManager Instance not found.");
                success = false;
            }

            if (token.IsCancellationRequested) return;

            if (_txtLoading != null)
            {
                _txtLoading.text = success ? "Score Submitted" : "Score Failed to Submit";
            }

            float elapsed = Time.realtimeSinceStartup - startTime;
            float remaining = 5.0f - elapsed;
            if (remaining > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(remaining), delayType: DelayType.UnscaledDeltaTime, cancellationToken: token).SuppressCancellationThrow();
            }

            if (token.IsCancellationRequested) return;

            if (success)
            {
                if (_animation != null)
                {
                    await _animation.PlayLoadingPopOutAsync().AttachExternalCancellation(token);
                }

                ClosePanel();

                if (_resultSystemPanel != null)
                {
                    _resultSystemPanel.DisableSubmitButton();
                }
            }
            else
            {
                if (_animation != null)
                {
                    await _animation.PlayLoadingPopOutOnlyAsync().AttachExternalCancellation(token);
                }

                if (_contentLoadingSubmission != null)
                {
                    _contentLoadingSubmission.SetActive(false);
                }

                ResetUI();

                if (_animation != null)
                {
                    _animation.PlayPopUp();
                }
            }
        }

        private void ClosePanel()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }
            ResetUI();
        }

        private void CancelSequence()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
        }
    }
}
