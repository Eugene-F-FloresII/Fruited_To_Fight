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
        [SerializeField] private float _statUpgradePercentage = 0.5f; // 50% per level

        private float _baseDamage;
        private int _basePierce;
        private float _baseRange;
        private float _baseProjectileSpeed;
        private float _baseAttackSpeed;
        private float _baseKnockback;

        protected override void Awake()
        {
            base.Awake();
            InitializeBaseStats();
        }

        private void InitializeBaseStats()
        {
            if (_weaponConfig == null) return;

            int currentLvl = (_levelVariable != null) ? _levelVariable.Value : 1;
            if (currentLvl < 1) currentLvl = 1;

            float damage = _weaponConfig.WeaponDamage;
            int pierce = _weaponConfig.WeaponPierce;
            float range = _weaponConfig.WeaponRange;
            float speed = _weaponConfig.WeaponSpeed;
            float atkSpeed = _weaponConfig.WeaponAtkSpeed;
            float knockback = _weaponConfig.WeaponKnockback;

            for (int i = currentLvl; i > 1; i--)
            {
                if (!Mathf.Approximately(1f + _statUpgradePercentage, 0f))
                {
                    damage /= (1f + _statUpgradePercentage);
                    range /= (1f + _statUpgradePercentage);
                    speed /= (1f + _statUpgradePercentage);
                    knockback /= (1f + _statUpgradePercentage);
                }
                if (!Mathf.Approximately(1f - _statUpgradePercentage, 0f))
                {
                    atkSpeed /= (1f - _statUpgradePercentage);
                }
                pierce = ReversePierce(pierce);
            }

            _baseDamage = damage;
            _basePierce = pierce;
            _baseRange = range;
            _baseProjectileSpeed = speed;
            _baseAttackSpeed = atkSpeed;
            _baseKnockback = knockback;
        }

        private int ReversePierce(int newPierce)
        {
            for (int old = newPierce - 1; old >= 0; old--)
            {
                if (old + Mathf.Max(1, Mathf.RoundToInt(old * _statUpgradePercentage)) == newPierce)
                {
                    return old;
                }
            }
            return newPierce;
        }

        public override void ResetUpgrade()
        {
            if (_weaponConfig != null)
            {
                _weaponConfig.WeaponDamage = _baseDamage;
                _weaponConfig.WeaponPierce = _basePierce;
                _weaponConfig.WeaponRange = _baseRange;
                _weaponConfig.WeaponSpeed = _baseProjectileSpeed;
                _weaponConfig.WeaponAtkSpeed = _baseAttackSpeed;
                _weaponConfig.WeaponKnockback = _baseKnockback;
            }
            base.ResetUpgrade();
        }

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

        public override void UpdateUpgradeCost()
        {
            base.UpdateUpgradeCost();
            UpdateWeaponTexts();
        }

        public override void OnBoughtUpgradeItem()
        {
            if (_weaponConfig == null) return;

            // 1. Apply permanent stats upgrade
            ApplyPermanentStatsUpgrade();

            // 2. Update texts to show the newly upgraded stats
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
        private float GetFutureAttackSpeed() => Mathf.Max(0.1f, _weaponConfig.WeaponAtkSpeed * (1f - _statUpgradePercentage));
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
