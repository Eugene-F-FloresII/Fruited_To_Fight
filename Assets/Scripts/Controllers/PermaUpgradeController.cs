using Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using PrimeTween;

namespace Controllers
{
    public class PermaUpgradeController : MonoBehaviour
    {
        [Header("Currency Settings")]
        [SerializeField] private CurrencyConfig _currencyConfig;
        [SerializeField] private TextMeshProUGUI _currencyText;

        [Header("Panel References")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _contentUpgrades;
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

            if (_contentUpgrades == null)
            {
                var contentTrans = transform.Find("Content_Upgrades");
                if (contentTrans != null)
                {
                    _contentUpgrades = contentTrans.GetComponent<RectTransform>();
                }
            }

            if (_exitButton == null)
            {
                var exitTrans = transform.Find("Content_Upgrades/Content_Header/Btn_ExitUpgrades");
                if (exitTrans != null)
                {
                    _exitButton = exitTrans.GetComponent<Button>();
                }
            }

            if (_exitButton != null)
            {
                _exitButton.onClick.AddListener(ExitPanel);
            }
        }

        private void OnEnable()
        {
            if (_currencyConfig != null && _currencyConfig.SkeletalLeafCurrency != null)
            {
                _currencyConfig.SkeletalLeafCurrency.OnValueChanged += OnCurrencyChanged;
            }

            ShowPanelAsync().Forget();
        }

        private void OnDisable()
        {
            if (_currencyConfig != null && _currencyConfig.SkeletalLeafCurrency != null)
            {
                _currencyConfig.SkeletalLeafCurrency.OnValueChanged -= OnCurrencyChanged;
            }
        }

        private void OnCurrencyChanged(int newValue)
        {
            if (_canvasGroup != null && Mathf.Approximately(_canvasGroup.alpha, 1f))
            {
                UpdateCurrencyText(newValue);
            }
        }

        private void UpdateCurrencyText()
        {
            if (_currencyConfig != null && _currencyConfig.SkeletalLeafCurrency != null)
            {
                UpdateCurrencyText(_currencyConfig.SkeletalLeafCurrency.Value);
            }
        }

        private void UpdateCurrencyText(int value)
        {
            if (_currencyText != null)
            {
                _currencyText.text = value.ToString();
            }
        }

        private async UniTaskVoid ShowPanelAsync()
        {
            Tween.StopAll(_canvasGroup);
            if (_contentUpgrades != null)
            {
                Tween.StopAll(_contentUpgrades);
                _contentUpgrades.localScale = Vector3.zero;
            }

            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = false;

            // 1. Canvas Group alpha will be 1
            await Tween.Alpha(_canvasGroup, 1f, _fadeDuration).ToUniTask(this);

            // Update text anytime when CanvasGroup alpha is 1
            UpdateCurrencyText();

            // 2. after Canvas Group alpha 1, Content_Upgrades will have a Pop up effect
            if (_contentUpgrades != null)
            {
                await Tween.Scale(_contentUpgrades, 1f, _scaleDuration, Ease.OutBack).ToUniTask(this);
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
            if (_contentUpgrades != null)
            {
                Tween.StopAll(_contentUpgrades);
            }

            _canvasGroup.interactable = false;

            // 3. before Canvas Group of it will be 0, it will Pop in effect the Content_Upgrades
            if (_contentUpgrades != null)
            {
                await Tween.Scale(_contentUpgrades, 0f, _scaleDuration, Ease.InBack).ToUniTask(this);
            }

            // After pop in/down effect, Canvas Group alpha becomes 0
            await Tween.Alpha(_canvasGroup, 0f, _fadeDuration).ToUniTask(this);

            _canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
        }
    }
}

