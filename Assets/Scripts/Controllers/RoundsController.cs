using System;
using Obvious.Soap;
using TMPro;
using UnityEngine;

namespace Controllers
{
    public class RoundsController : MonoBehaviour
    {
        [SerializeField] private IntVariable _currentRound;
        [SerializeField] private IntVariable _maxRounds;
        [SerializeField] private TextMeshProUGUI _roundText;

        private void Start()
        {
            _roundText.text = "Round: " + _currentRound.Value + " / " + _maxRounds.Value;
        }

        private void OnEnable()
        {
            _currentRound.OnValueChanged += OnCurrentRoundChanged;
        }

        private void OnDisable()
        {
            _currentRound.OnValueChanged -= OnCurrentRoundChanged;

        }

        private void OnCurrentRoundChanged(int i)
        {
            if (_roundText != null)
            {
                 _roundText.text = "Round: " + _currentRound.Value + " / " + _maxRounds.Value;
            }
           
        }
    }

}
