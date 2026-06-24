using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using Controllers;
using Data;

namespace Gameplay.UI
{
    public class BossHealth : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image _healthBar;
        [SerializeField] private TextMeshProUGUI _bossName;
        [SerializeField] private TextMeshProUGUI _bossLevel;
        [SerializeField] private TextMeshProUGUI _bossHealth;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Transform _shakeContainer;

        private EnemyController _targetBoss;
        private float _maxHealth;
        private float _lastHealth;
        private Action<BossHealth> _onFinishedCallback;
        private bool _isDying = false;
        private bool _isShaking = false;

        private void Awake()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                {
                    _canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }
        }

        public void Initialize(EnemyController boss, EnemyBossConfig config, Action<BossHealth> onFinishedCallback)
        {
            _targetBoss = boss;
            _maxHealth = boss.MaxHealth;
            _lastHealth = boss.CurrentHealth;
            _onFinishedCallback = onFinishedCallback;
            _bossName.text = config.BossName;
            _bossLevel.text = $"Lvl {config.BossLevel}";
            _isDying = false;
            _isShaking = false;

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
            }

            UpdateUI();
        }

        private void Update()
        {
            if (_isDying) return;

            if (_targetBoss == null || !_targetBoss.gameObject.activeInHierarchy || _targetBoss.CurrentHealth <= 0)
            {
                StartDyingSequence().Forget();
                return;
            }

            // Check for hit damage to trigger shake
            if (_targetBoss.CurrentHealth < _lastHealth)
            {
                TriggerHitShake().Forget();
            }
            _lastHealth = _targetBoss.CurrentHealth;

            UpdateUI();
        }

        private void UpdateUI()
        {
            if (_targetBoss == null) return;

            float currentHealth = Mathf.Max(0, _targetBoss.CurrentHealth);
            if (_maxHealth > 0)
            {
                _healthBar.fillAmount = currentHealth / _maxHealth;
            }
            _bossHealth.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(_maxHealth)}";
        }

        private async UniTaskVoid TriggerHitShake()
        {
            if (_isShaking || _isDying) return;
            _isShaking = true;

            Transform target = _shakeContainer != null ? _shakeContainer : transform;
            Vector3 originalPos = target.localPosition;

            float shakeDuration = 0.2f;
            float shakeMagnitude = 5f; // Small, sharp offset for hit damage
            float elapsed = 0f;

            while (elapsed < shakeDuration && !_isDying)
            {
                float offsetX = UnityEngine.Random.Range(-shakeMagnitude, shakeMagnitude);
                float offsetY = UnityEngine.Random.Range(-shakeMagnitude, shakeMagnitude);
                target.localPosition = originalPos + new Vector3(offsetX, offsetY, 0f);

                elapsed += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            // Ensure container resets to its original position
            target.localPosition = originalPos;
            _isShaking = false;
        }

        private async UniTaskVoid StartDyingSequence()
        {
            _isDying = true;
            _isShaking = false; // Stop hit shaking

            // Ensure health bar and text reflect defeat immediately
            if (_healthBar != null) _healthBar.fillAmount = 0f;
            if (_bossHealth != null) _bossHealth.text = $"0 / {Mathf.CeilToInt(_maxHealth)}";

            Transform target = _shakeContainer != null ? _shakeContainer : transform;
            Vector3 originalPos = target.localPosition;

            // 1. Shake effect (0.5 seconds)
            float shakeDuration = 0.5f;
            float shakeMagnitude = 10f; // Larger offset for death
            float elapsed = 0f;

            while (elapsed < shakeDuration)
            {
                float offsetX = UnityEngine.Random.Range(-shakeMagnitude, shakeMagnitude);
                float offsetY = UnityEngine.Random.Range(-shakeMagnitude, shakeMagnitude);
                target.localPosition = originalPos + new Vector3(offsetX, offsetY, 0f);

                elapsed += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            target.localPosition = originalPos;

            // 2. Fade out effect (0.5 seconds)
            float fadeDuration = 0.5f;
            elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                if (_canvasGroup != null)
                {
                    _canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                }
                elapsed += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
            }

            // 3. Callback to parent (which will release the Addressable instance), or destroy locally if no callback.
            if (_onFinishedCallback != null)
            {
                _onFinishedCallback.Invoke(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
