using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading;
using Collection;
using Cysharp.Threading.Tasks;
using Data;
using Shared.Events;
using TMPro;


namespace Managers
{
    public class LoadingScreenManager : MonoBehaviour
    {
        [Header("Text Config")]
        [SerializeField] private LoadingScreenTextConfig _loadingScreenTextConfig;
        
        [Header("Loading screen setting")]
        [SerializeField] private float _loadingScreenDuration = 3f;

        [Header("References")] 
        [SerializeField] private TextMeshProUGUI _messageText;

        private int _randomIndex;
        private CanvasGroup _canvasGroup;
        private CancellationTokenSource _loadingCts;

        private void Awake()
        {
            // Check if a LoadingScreenManager is already registered in ServiceLocator
            var existing = ServiceLocator.TryGet<LoadingScreenManager>();
            if (existing != null && existing != this)
            {
                Destroy(gameObject);
                return;
            }

            ServiceLocator.Register(this);
            DontDestroyOnLoad(gameObject);
            _canvasGroup = GetComponent<CanvasGroup>();
        }


        private void Start()
        {
            TurnOffCanvasGroup();
        }

        private void OnEnable()
        {
            Events_Game.OnSceneChange += ActivateLoadingScreen;
        }

        private void OnDisable()
        {
            Events_Game.OnSceneChange -= ActivateLoadingScreen;
            CancelLoading();
        }

        private void OnDestroy()
        {
            // Only unregister if this is the instance currently in the ServiceLocator
            if (ServiceLocator.Get<LoadingScreenManager>() == this)
            {
                ServiceLocator.Unregister<LoadingScreenManager>();
            }
        }


        private void ActivateLoadingScreen(string scenekey)
        {
            // Cancel any ongoing loading task before starting a new one
            CancelLoading();
            
            _loadingCts = new CancellationTokenSource();
            TurnOnCanvasGroup();
            LoadPickedScene(scenekey, _loadingCts.Token).Forget();
        }

        private void TurnOnCanvasGroup()
        {
            if (_canvasGroup == null) return;
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            
            _randomIndex = UnityEngine.Random.Range(0, _loadingScreenTextConfig.Texts.Count);

            _messageText.text = _loadingScreenTextConfig.Texts[_randomIndex];
        }
        
        private void TurnOffCanvasGroup()
        {
            if (_canvasGroup == null) return;
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        private void CancelLoading()
        {
            if (_loadingCts != null)
            {
                _loadingCts.Cancel();
                _loadingCts.Dispose();
                _loadingCts = null;
            }
        }

        private void StopLoadingScreen()
        {
            TurnOffCanvasGroup();
            CancelLoading();
        }

        private async UniTask LoadPickedScene(string scenekey, CancellationToken token)
        {
            try
            {
                var handle = SceneManager.LoadSceneAsync(scenekey);
                if (handle == null)
                {
                    Debug.LogError($"Failed to load scene: {scenekey}");
                    return;
                }

                await handle.ToUniTask(cancellationToken: token);

                // ignoreTimeScale: true ensures the delay finishes even if Time.timeScale is 0
                await UniTask.Delay(TimeSpan.FromSeconds(_loadingScreenDuration), ignoreTimeScale: true, cancellationToken: token);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Scene loading was canceled");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error during scene load: {e.Message}");
            }
            finally
            {
                // finally block ensures the loading screen is ALWAYS turned off
                StopLoadingScreen();
            }
        }
    }
}
