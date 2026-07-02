using Controllers;
using Data;
using TMPro;
using UnityEngine;

namespace Gameplay.Upgrades
{
    public class UpgradeItemWeapon : UpgradeItemController
    {
        [Header("Weapon Settings")]
        [SerializeField] private WeaponConfig _weaponConfig;
        [SerializeField] private float _statUpgradePercentage = 0.1f; // 10% per level

        [Header("Current Weapon Settings")]
        [SerializeField] private TextMeshProUGUI _currentDamageText;
        [SerializeField] private TextMeshProUGUI _currentPierceText;
        [SerializeField] private TextMeshProUGUI _currentRangeText;
        [SerializeField] private TextMeshProUGUI _currentProjectileSpeedText;
        [SerializeField] private TextMeshProUGUI _currentAttackSpeedText;
        [SerializeField] private TextMeshProUGUI _currentKnockbackText;
        
        [Header("Future Weapon Settings")]
        [SerializeField] private TextMeshProUGUI _futureDamageText;
        [SerializeField] private TextMeshProUGUI _futurePierceText;
        [SerializeField] private TextMeshProUGUI _futureRangeText;
        [SerializeField] private TextMeshProUGUI _futureProjectileSpeedText;
        [SerializeField] private TextMeshProUGUI _futureAttackSpeedText;
        [SerializeField] private TextMeshProUGUI _futureKnockbackText;

        public override void Start()
        {
            if (_weaponConfig != null && _weaponConfig.WeaponLevel != null)
            {
                _currentLevel = _weaponConfig.WeaponLevel.Value;
                
                // Keep level bounded to at least 1
                if (_currentLevel < 1)
                {
                    _currentLevel = 1;
                    _weaponConfig.WeaponLevel.Value = 1;
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

            base.Start();

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
        }

        public override void UpdateUpgradeCost()
        {
            base.UpdateUpgradeCost();
            UpdateWeaponTexts();
        }

        public override void OnBoughtUpgradeItem()
        {
            if (_weaponConfig == null) return;

            // 1. Sync level on ScriptableObject
            if (_weaponConfig.WeaponLevel != null)
            {
                _weaponConfig.WeaponLevel.Value = _currentLevel;
            }

            // 2. Apply permanent stats upgrade
            ApplyPermanentStatsUpgrade();

            // 3. Update texts to show the newly upgraded stats
            UpdateWeaponTexts();
        }

        private void UpdateWeaponTexts()
        {
            if (_weaponConfig == null) return;

            // Update Current texts
            if (_currentDamageText != null) _currentDamageText.text = _weaponConfig.WeaponDamage.ToString("F1");
            if (_currentPierceText != null) _currentPierceText.text = _weaponConfig.WeaponPierce.ToString();
            if (_currentRangeText != null) _currentRangeText.text = _weaponConfig.WeaponRange.ToString("F1");
            if (_currentProjectileSpeedText != null) _currentProjectileSpeedText.text = _weaponConfig.WeaponSpeed.ToString("F1");
            if (_currentAttackSpeedText != null) _currentAttackSpeedText.text = _weaponConfig.WeaponAtkSpeed.ToString("F1");
            if (_currentKnockbackText != null) _currentKnockbackText.text = _weaponConfig.WeaponKnockback.ToString("F1");

            // Update Future (next level preview) texts
            UpdateFutureWeaponTexts();
        }

        private void UpdateFutureWeaponTexts()
        {
            if (_weaponConfig == null) return;

            if (_currentLevel >= (_maxStars + 1))
            {
                if (_futureDamageText != null) _futureDamageText.text = "-";
                if (_futurePierceText != null) _futurePierceText.text = "-";
                if (_futureRangeText != null) _futureRangeText.text = "-";
                if (_futureProjectileSpeedText != null) _futureProjectileSpeedText.text = "-";
                if (_futureAttackSpeedText != null) _futureAttackSpeedText.text = "-";
                if (_futureKnockbackText != null) _futureKnockbackText.text = "-";
                return;
            }

            if (_futureDamageText != null) _futureDamageText.text = GetFutureDamage().ToString("F1");
            if (_futurePierceText != null) _futurePierceText.text = GetFuturePierce().ToString();
            if (_futureRangeText != null) _futureRangeText.text = GetFutureRange().ToString("F1");
            if (_futureProjectileSpeedText != null) _futureProjectileSpeedText.text = GetFutureProjectileSpeed().ToString("F1");
            if (_futureAttackSpeedText != null) _futureAttackSpeedText.text = GetFutureAttackSpeed().ToString("F1");
            if (_futureKnockbackText != null) _futureKnockbackText.text = GetFutureKnockback().ToString("F1");
        }

        private float GetFutureDamage() => _weaponConfig.WeaponDamage * (1f + _statUpgradePercentage);
        private int GetFuturePierce() => _weaponConfig.WeaponPierce + Mathf.Max(1, Mathf.RoundToInt(_weaponConfig.WeaponPierce * _statUpgradePercentage));
        private float GetFutureRange() => _weaponConfig.WeaponRange * (1f + _statUpgradePercentage);
        private float GetFutureProjectileSpeed() => _weaponConfig.WeaponSpeed * (1f + _statUpgradePercentage);
        private float GetFutureAttackSpeed() => _weaponConfig.WeaponAtkSpeed * (1f - _statUpgradePercentage);
        private float GetFutureKnockback() => _weaponConfig.WeaponKnockback * (1f + _statUpgradePercentage);

        private void ApplyPermanentStatsUpgrade()
        {
            if (_weaponConfig == null) return;

            _weaponConfig.WeaponDamage = GetFutureDamage();
            _weaponConfig.WeaponPierce = GetFuturePierce();
            _weaponConfig.WeaponRange = GetFutureRange();
            _weaponConfig.WeaponSpeed = GetFutureProjectileSpeed();
            _weaponConfig.WeaponAtkSpeed = GetFutureAttackSpeed();
            _weaponConfig.WeaponKnockback = GetFutureKnockback();
        }
    }
}
