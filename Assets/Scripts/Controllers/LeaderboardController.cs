using Gameplay.UI;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using PrimeTween;

namespace Controllers
{
    public class LeaderboardController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private LeaderboardUI _leaderboardUI;

        [Header("Panel References")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _contentLeaderboard;
        [SerializeField] private Button _exitButton;

        [Header("Animation Settings")]
        [SerializeField] private float _fadeDuration = 0.5f;
        [SerializeField] private float _scaleDuration = 0.5f;

        private void Awake()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }

            if (_contentLeaderboard == null)
            {
                var contentTrans = transform.Find("Content_Leaderboard");
                if (contentTrans != null)
                {
                    _contentLeaderboard = contentTrans.GetComponent<RectTransform>();
                }
            }

            if (_exitButton == null)
            {
                var exitTrans = transform.Find("Content_Leaderboard/Content_Header/Btn_ExitLeaderboard");
                if (exitTrans != null)
                {
                    _exitButton = exitTrans.GetComponent<Button>();
                }
            }

            if (_leaderboardUI == null)
            {
                _leaderboardUI = GetComponent<LeaderboardUI>();
            }

            if (_exitButton != null)
            {
                _exitButton.onClick.AddListener(ExitPanel);
            }
        }

        private void OnEnable()
        {
            ShowPanelAsync().Forget();
        }

        private async UniTaskVoid ShowPanelAsync()
        {
            Tween.StopAll(_canvasGroup);
            if (_contentLeaderboard != null)
            {
                Tween.StopAll(_contentLeaderboard);
                _contentLeaderboard.localScale = Vector3.zero;
            }

            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = false;

            // 1. Canvas Group alpha will be 1
            await Tween.Alpha(_canvasGroup, 1f, _fadeDuration).ToUniTask(this);

            // Refresh Leaderboard scores when active/shown
            if (_leaderboardUI != null)
            {
                _leaderboardUI.RefreshLeaderboardAsync().Forget();
            }

            // 2. after Canvas Group alpha 1, Content_Leaderboard will have a Pop up effect
            if (_contentLeaderboard != null)
            {
                await Tween.Scale(_contentLeaderboard, 1f, _scaleDuration, Ease.OutBack).ToUniTask(this);
            }

            _canvasGroup.interactable = true;
        }

        public void ExitPanel()
        {
            HidePanelAsync().Forget();
        }

        private async UniTaskVoid HidePanelAsync()
        {
            Tween.StopAll(_canvasGroup);
            if (_contentLeaderboard != null)
            {
                Tween.StopAll(_contentLeaderboard);
            }

            _canvasGroup.interactable = false;

            // 3. before Canvas Group of it will be 0, it will Pop in effect the Content_Leaderboard
            if (_contentLeaderboard != null)
            {
                await Tween.Scale(_contentLeaderboard, 0f, _scaleDuration, Ease.InBack).ToUniTask(this);
            }

            // After pop in/down effect, Canvas Group alpha becomes 0
            await Tween.Alpha(_canvasGroup, 0f, _fadeDuration).ToUniTask(this);

            _canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
        }
    }
}