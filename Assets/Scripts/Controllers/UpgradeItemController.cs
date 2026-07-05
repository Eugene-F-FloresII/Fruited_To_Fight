using Cysharp.Threading.Tasks;
using Data;
using Obvious.Soap;
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
        [SerializeField] protected IntVariable _levelVariable;
        [SerializeField] protected int _upgradeCost;
        [SerializeField] protected TextMeshProUGUI _upgradeItemLevel;
        [SerializeField] protected Transform _starsTransform;
        [SerializeField] protected int _maxStars;
        [SerializeField] protected AssetReferenceGameObject _starPrefab;
        [SerializeField] protected TextMeshProUGUI _upgradePrice;
        [SerializeField] protected Button _buyButton;

        protected int _currentMoney => (_currencyConfig != null && _currencyConfig.SkeletalLeafCurrency != null) ? _currencyConfig.SkeletalLeafCurrency.Value : 0;
        protected int _currentStars;
        protected int _currentLevel;
        protected int _baseUpgradeCost;

        protected virtual void Awake()
        {
            _baseUpgradeCost = _upgradeCost;
        }
        
        public virtual void Start()
        {
            if (_levelVariable != null)
            {
                _currentLevel = _levelVariable.Value;
                if (_currentLevel < 1)
                {
                    _currentLevel = 1;
                    _levelVariable.Value = 1;
                }
                _currentStars = Mathf.Max(0, _currentLevel - 1);
            }
            else
            {
                _currentLevel = 1;
                _currentStars = 0;
            }

            // Sync cost based on current loaded level
            int cost = _baseUpgradeCost;
            for (int i = 1; i < _currentLevel; i++)
            {
                cost = IncreaseUpgradeCost(cost);
            }
            _upgradeCost = cost;

            if (_buyButton != null)
            {
                _buyButton.onClick.AddListener(BuyUpgradeItem);
            }

            // Populate stars matching current loaded level
            if (_starsTransform != null)
            {
                foreach (Transform child in _starsTransform)
                {
                    Destroy(child.gameObject);
                }

                for (int i = 0; i < _currentStars; i++)
                {
                    InstantiateStar();
                }
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
            if (_levelVariable != null)
            {
                _levelVariable.Value = _currentLevel;
            }
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

        public IntVariable LevelVariable => _levelVariable;

        public virtual int IncreaseUpgradeCost(int previousCost)
        {
            int percentageValue = Mathf.RoundToInt(previousCost * 0.5f);
            int addedValue = previousCost + percentageValue;
            return addedValue;
        }

        public virtual void ResetUpgrade()
        {
            if (_levelVariable != null)
            {
                _levelVariable.Value = 1;
            }
            _currentLevel = 1;
            _currentStars = 0;
            _upgradeCost = _baseUpgradeCost;
            
            if (_buyButton != null)
            {
                _buyButton.interactable = true;
            }
            
            if (_starsTransform != null)
            {
                foreach (Transform child in _starsTransform)
                {
                    Destroy(child.gameObject);
                }
            }
            
            UpdateUpgradeCost();
        }
    }

}
