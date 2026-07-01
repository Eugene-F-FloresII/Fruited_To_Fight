using Cysharp.Threading.Tasks;
using Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers
{
    public abstract class UpgradeItemController : MonoBehaviour
    {
        [Header("Upgrade Settings")]
        [SerializeField] private CurrencyConfig _currencyConfig;
        [SerializeField] private int _upgradeCost;
        [SerializeField] private TextMeshProUGUI _upgradeItemLevel;
        [SerializeField] private Transform _starsTransform;
        [SerializeField] private int _maxStars;
        [SerializeField] private GameObject _starPrefab;
        [SerializeField] private TextMeshProUGUI _upgradePrice;
        [SerializeField] private Button _buyButton;

        private int _currentMoney;
        private int _currentStars;
        private int _currentLevel;
        
        public virtual void Start()
        {   
            UpdateUpgradeCost();    
        }

        public virtual void UpdateUpgradeCost()
        {
            _currentLevel++;
            
            if (_currentLevel == (_maxStars + 1))
            {
                Debug.Log("Max Stars Reached");
                _buyButton.interactable = false;
                return;
            }
            
            _upgradePrice.text = _upgradePrice.ToString();
            _currentMoney = _currencyConfig.SkeletalLeafCurrency.Value;
            _buyButton.onClick.AddListener(BuyUpgradeItem);
        }

        public virtual void BuyUpgradeItem()
        {
            if (_currentMoney < _upgradeCost)
            {
                Debug.Log("Not enough money");
                return;
            }
            
            _currencyConfig.SkeletalLeafCurrency.Value = CalculateRemainingCost(_currentMoney, _upgradeCost);
            UpdateUpgradeCost();
            AddStars();
            OnBoughtUpgradeItem();
        }

        public virtual void OnBoughtUpgradeItem()
        {
            throw new System.NotImplementedException();
        }

        public virtual void AddStars()
        {
            if (_currentStars >= _maxStars)
            {
                Debug.Log("Max Stars Reached");
                return;
            }
            _currentStars++;
            Instantiate(_starPrefab, _starsTransform);
        }

        public virtual int CalculateRemainingCost(int currentMoney, int upgradeCost)
        {
            currentMoney -= upgradeCost;
            _upgradeCost = IncreaseUpgradeCost(upgradeCost);
            return currentMoney;
        }

        public virtual int IncreaseUpgradeCost(int previousCost)
        {
            int percentageValue = Mathf.RoundToInt(previousCost * 0.5f);
            int addedValue = previousCost + percentageValue;
            return addedValue;
        }
    }

}
