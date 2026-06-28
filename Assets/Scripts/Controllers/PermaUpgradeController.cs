using Data;
using TMPro;
using UnityEngine;

namespace Controllers
{
    public class PermaUpgradeController : MonoBehaviour
    {
        [Header("Currency Settings")]
        [SerializeField] private CurrencyConfig _currencyConfig;
        [SerializeField] private TextMeshProUGUI _currencyText;
    }

}
