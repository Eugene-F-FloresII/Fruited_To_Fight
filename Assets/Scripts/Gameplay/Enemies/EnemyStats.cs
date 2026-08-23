using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using Data;

namespace Gameplay.Enemies
{
    public class EnemyStats : MonoBehaviour
    {
        [SerializeField] private AssetReferenceT<EnemyConfig> _enemyConfigReference;

        private EnemyConfig _enemyConfig;
        private bool _hasRuntimeStats;

        public EnemyConfig Config => _enemyConfig;
        
        public float MaxHealth { get; private set; }
        public float CurrentDamage { get; private set; }
        public float CurrentSpeed { get; private set; }
        public float CurrentAttackSpeed { get; private set; }
        public float CurrentKnockbackForce { get; private set; }

        private void OnDisable()
        {
            _hasRuntimeStats = false;
        }

        private void OnDestroy()
        {
            if (_enemyConfigReference != null && _enemyConfigReference.IsValid())
            {
                _enemyConfigReference.ReleaseAsset();
            }
        }

        public async UniTask LoadEnemyConfigAsyncAddress()
        {
            if (_enemyConfigReference != null && _enemyConfigReference.RuntimeKeyIsValid())
            {
                _enemyConfig = await _enemyConfigReference.LoadAssetAsync<EnemyConfig>().ToUniTask();
            }
            ResetStatsFromConfig();
        }

        public void ResetStatsFromConfig()
        {
            if (_enemyConfig == null)
            {
                return;
            }

            if (_hasRuntimeStats)
            {
                return;
            }

            MaxHealth = _enemyConfig.EnemyHealth;
            CurrentDamage = _enemyConfig.EnemyDamage;
            CurrentKnockbackForce = _enemyConfig.EnemyKnockbackForce;
            CurrentSpeed = _enemyConfig.EnemyMoveSpeed;
            CurrentAttackSpeed = _enemyConfig.EnemyAtkSpeed;
        }

        public void ApplyRuntimeStats(EnemyRuntimeStats runtimeStats)
        {
            _hasRuntimeStats = true;
            MaxHealth = runtimeStats.Health;
            CurrentDamage = runtimeStats.Damage;
            CurrentSpeed = runtimeStats.MoveSpeed;
            CurrentAttackSpeed = runtimeStats.AttackSpeed;
            CurrentKnockbackForce = runtimeStats.KnockbackForce;
        }

#if UNITY_EDITOR
        public void EditorSetup(AssetReferenceT<EnemyConfig> reference) => _enemyConfigReference = reference;
#endif
    }
}
