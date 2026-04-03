using System;
using Cysharp.Threading.Tasks;
using PrimeTween;
using TMPro;
using UnityEngine;

namespace Gameplay
{
    public class DamageNumber : MonoBehaviour
    {
        public int DamageNum;

        private TextMeshProUGUI _damageNumbers;
        private Sequence _showSequence;
        private Vector3 _baseLocalPosition;

        private void Awake()
        {
            _damageNumbers = GetComponent<TextMeshProUGUI>();
            _baseLocalPosition = transform.localPosition;
        }

        private void OnDisable()
        {
            if (_showSequence.isAlive)
            {
                _showSequence.Stop();
            }

            transform.localScale = Vector3.one;
            transform.localPosition = _baseLocalPosition;
        }

        public void InitiateDamageNumber(int damageNum)
        {
            DamageNum = damageNum;
            if (_damageNumbers != null)
            {
                _damageNumbers.text = DamageNum.ToString();
            }
        }

        public async UniTask PlayShowTweenAsync(float popDuration, float settleDuration, float floatDistance, float floatDuration)
        {
            _baseLocalPosition = transform.localPosition;
            transform.localScale = Vector3.one * 0.65f;

            _showSequence = Tween.Scale(transform, Vector3.one * 1.1f, popDuration, Ease.OutBack)
                .Chain(Tween.Scale(transform, Vector3.one, settleDuration, Ease.InBack))
                .Group(Tween.LocalPositionY(transform, _baseLocalPosition.y + floatDistance, floatDuration, Ease.OutQuad));

            try
            {
                await _showSequence.ToUniTask();
            }
            catch (OperationCanceledException)
            {
                // ignored when the pooled object gets disabled early
            }
        }

    }

}
