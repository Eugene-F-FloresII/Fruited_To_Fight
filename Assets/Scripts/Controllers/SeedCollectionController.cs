
using Obvious.Soap;
using TMPro;
using UnityEngine;
using Cysharp.Threading.Tasks;
using PrimeTween;

namespace Controllers
{
    public class SeedCollectionController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _seedsCollectedText;
        [SerializeField] private TextMeshProUGUI _seedsCollectionText;
        [SerializeField] private IntVariable _seedsCollected;
        
        private Transform _seedsCollectedGameObject;
        private Transform _seedsCollectionGameObject;

        private void Awake()
        {
            UpdateSeedCollection();
        }

        private void OnEnable()
        {
            if (_seedsCollected != null)
            {
                _seedsCollected.OnValueChanged += OnSeedsValueChanged;
                OnSeedsValueChanged(_seedsCollected.Value);
            }
            UpdateSeedCollection();
        }

        private void OnDisable()
        {
            if (_seedsCollected != null)
            {
                _seedsCollected.OnValueChanged -= OnSeedsValueChanged;
            }
        }

        private void OnSeedsValueChanged(int seedsCollected)
        {
            if (_seedsCollectedText != null)
            {
                _seedsCollectedText.text = seedsCollected.ToString();
            }

            if (_seedsCollectedGameObject == null || _seedsCollectionGameObject == null)
            {
                UpdateSeedCollection();
            }

            IncreaseScale().Forget();
        }

        private void UpdateSeedCollection()
        {
            _seedsCollectedGameObject = _seedsCollectedText != null ? _seedsCollectedText.transform : null;
            _seedsCollectionGameObject = _seedsCollectionText != null ? _seedsCollectionText.transform : null;
        }

        private async UniTaskVoid IncreaseScale()
        {
            if (_seedsCollectedGameObject == null || _seedsCollectionGameObject == null)
            {
                return;
            }

            bool canceled = await UniTask.WhenAll(
                Tween.Scale(_seedsCollectedGameObject, Vector3.one * 1.1f, duration: 0.3f, Ease.OutBack).ToUniTask(this), 
                Tween.Scale(_seedsCollectionGameObject, Vector3.one * 1.1f, duration: 0.3f, Ease.OutBack).ToUniTask(this)
            ).SuppressCancellationThrow();

            if (canceled || _seedsCollectedGameObject == null || _seedsCollectionGameObject == null) return;

            await UniTask.WhenAll(
                Tween.Scale(_seedsCollectedGameObject, Vector3.one, duration: 0.3f, Ease.InBack).ToUniTask(this), 
                Tween.Scale(_seedsCollectionGameObject, Vector3.one, duration: 0.3f, Ease.InBack).ToUniTask(this)
            ).SuppressCancellationThrow();
        }
    }

}
