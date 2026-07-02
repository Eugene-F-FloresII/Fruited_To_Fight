using Cysharp.Threading.Tasks;
using Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;

namespace Controllers
{
    public abstract class UpgradeItemController : MonoBehaviour
    {
        [Header("Upgrade Settings")]
        [SerializeField] protected CurrencyConfig _currencyConfig;
        [SerializeField] protected int _upgradeCost;
        [SerializeField] protected TextMeshProUGUI _upgradeItemLevel;
        [SerializeField] protected Transform _starsTransform;
        [SerializeField] protected int _maxStars;
        [SerializeField] protected AssetReferenceGameObject _starPrefab;
        [SerializeField] protected TextMeshProUGUI _upgradePrice;
        [SerializeField] protected Button _buyButton;

        protected int _currentMoney;
        protected int _currentStars;
        protected int _currentLevel;
        protected int _baseUpgradeCost;

        protected virtual void Awake()
        {
            _baseUpgradeCost = _upgradeCost;
        }
        
        public virtual void Start()
        {   
            if (_buyButton != null)
            {
                _buyButton.onClick.AddListener(BuyUpgradeItem);
            }
            UpdateUpgradeCost();    
        }

        public virtual void UpdateUpgradeCost()
        {
            if (_upgradeItemLevel != null)
            {
                _upgradeItemLevel.text = $"Lvl {_currentLevel}";
            }

            if (_currentLevel >= (_maxStars + 1))
            {
                Debug.Log("Max Stars Reached");
                if (_buyButton != null) _buyButton.interactable = false;
                if (_upgradePrice != null) _upgradePrice.text = "MAX";
                return;
            }
            
            if (_upgradePrice != null)
            {
                _upgradePrice.text = _upgradeCost.ToString();
            }
            _currentMoney = _currencyConfig.SkeletalLeafCurrency.Value;
        }

        public virtual void BuyUpgradeItem()
        {
            if (_currentMoney < _upgradeCost)
            {
                Debug.Log("Not enough money");
                return;
            }
            
            _currencyConfig.SkeletalLeafCurrency.Value = CalculateRemainingCost(_currentMoney, _upgradeCost);
            _currentLevel++;
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
            InstantiateStar();
        }

        protected virtual void InstantiateStar()
        {
            if (_starPrefab != null && _starsTransform != null)
            {
                Addressables.InstantiateAsync(_starPrefab, _starsTransform);
            }
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
