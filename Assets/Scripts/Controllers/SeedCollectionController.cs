
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
            _seedsCollected.OnValueChanged += OnSeedsValueChanged;
            UpdateSeedCollection();
        }

        private void OnDisable()
        {
            _seedsCollected.OnValueChanged -= OnSeedsValueChanged;
        }

        private void OnSeedsValueChanged(int seedsCollected)
        {
            _seedsCollectedText.text = seedsCollected.ToString();

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

        private async UniTask IncreaseScale()
        {
            if (_seedsCollectedGameObject == null || _seedsCollectionGameObject == null)
            {
                return;
            }

            Tween scaleCollectedUp = Tween.Scale(_seedsCollectedGameObject, Vector3.one * 1.1f, duration: 0.3f, Ease.OutBack);
            Tween scaleCollectionUp = Tween.Scale(_seedsCollectionGameObject, Vector3.one * 1.1f, duration: 0.3f, Ease.OutBack);
            await UniTask.WhenAll(scaleCollectedUp.ToUniTask(), scaleCollectionUp.ToUniTask());

            Tween scaleCollectedDown = Tween.Scale(_seedsCollectedGameObject, Vector3.one, duration: 0.3f, Ease.InBack);
            Tween scaleCollectionDown = Tween.Scale(_seedsCollectionGameObject, Vector3.one, duration: 0.3f, Ease.InBack);
            await UniTask.WhenAll(scaleCollectedDown.ToUniTask(), scaleCollectionDown.ToUniTask());
        }
    }

}
