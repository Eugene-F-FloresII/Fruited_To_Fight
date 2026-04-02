
using System;
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
        [SerializeField] private IntVariable _seedsCollected;
        
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
            IncreaseScale().Forget();
        }

        private void UpdateSeedCollection()
        {
            _seedsCollectionGameObject = _seedsCollectedText.gameObject.transform;
        }

        private async UniTask IncreaseScale()
        {
           await Sequence.Create()
                .Chain(Tween.Scale(_seedsCollectionGameObject, Vector3.one * 1.5f, duration: 0.5f, Ease.OutBack))    
                .Chain(Tween.Scale(_seedsCollectionGameObject, Vector3.one, duration: 0.5f, Ease.InBack))
                .ToUniTask();
           
           Debug.Log("Finished");
        }
    }

}
