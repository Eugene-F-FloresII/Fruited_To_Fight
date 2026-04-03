using System;
using Obvious.Soap;
using TMPro;
using UnityEngine;
using Cysharp.Threading.Tasks;
using PrimeTween;


namespace Gameplay.Enemies
{
    public class EnemiesDefeatedController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _enemiesDefeatedText;
        [SerializeField] private IntVariable _enemiesDefeatedCount;
        
        private Transform _enemiesDefeatedGameObject;

        private void Start()
        {
            InitializeEnemyDefeated();
        }

        private void OnEnable()
        {
            if (_enemiesDefeatedCount != null)
            {
                _enemiesDefeatedCount.OnValueChanged += OnEnemyDefeated;
            }
        }

        private void OnDisable()
        {
            if (_enemiesDefeatedCount != null)
            {
                _enemiesDefeatedCount.OnValueChanged -= OnEnemyDefeated;
            }
        }

        private void OnEnemyDefeated(int i)
        {
            if (_enemiesDefeatedText !=  null && _enemiesDefeatedCount != null)
            {
                _enemiesDefeatedText.text = "ENEMIES DEFEATED: " + _enemiesDefeatedCount.Value;
            }
            
            IncreaseScale().Forget();
        }

        private void InitializeEnemyDefeated()
        {
            if (_enemiesDefeatedGameObject == null)
            { 
                _enemiesDefeatedGameObject = _enemiesDefeatedText.gameObject.transform;

            }
        }
        
        private async UniTask IncreaseScale()
        {
            if (_enemiesDefeatedGameObject == null)
            {
                return;
            }

            Tween scaleCollectedUp = Tween.Scale(_enemiesDefeatedGameObject, Vector3.one * 1.3f, duration: 0.3f, Ease.OutBack);
            Tween scaleCollectionUp = Tween.Scale(_enemiesDefeatedGameObject, Vector3.one, duration: 0.3f, Ease.InBack);
            await UniTask.WhenAll(scaleCollectedUp.ToUniTask(), scaleCollectionUp.ToUniTask());
            
        }
    }

}
