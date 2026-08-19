using UnityEngine;
using Obvious.Soap;
using Shared.Events;
using Gameplay.Enemies;
using Data;
using Cysharp.Threading.Tasks;
using Gameplay.Weapons;
using Shared.Enums;
using UnityEngine.AddressableAssets;
using Collection.StateMachine;
using Collection.EnemyStateMachine;

namespace Controllers
{
    [RequireComponent(typeof(EnemyHealth))]
    [RequireComponent(typeof(EnemyStats))]
    [RequireComponent(typeof(EnemyMovement))]
    [RequireComponent(typeof(EnemyVisuals))]
    [RequireComponent(typeof(EnemyAfflictionHandler))]
    public class EnemyController : MonoBehaviour
    {
        [Header("Enemy References")]
        [SerializeField] private IntVariable _activeEnemyCount;
        [SerializeField] private DefendingController _defendingController;
        
        [Header("Components")]
        [SerializeField] private EnemyHealth _enemyHealth;
        [SerializeField] private EnemyStats _enemyStats;
        [SerializeField] private EnemyMovement _enemyMovement;
        [SerializeField] private EnemyVisuals _enemyVisuals;
        [SerializeField] private EnemyAfflictionHandler _enemyAfflictionHandler;

        public float CurrentHealth => _enemyHealth != null ? _enemyHealth.CurrentHealth : 0f;
        public float MaxHealth => _enemyHealth != null ? _enemyHealth.MaxHealth : 0f;
        
        // State Machine
        private StateMachine _stateMachine;
        public EnemyChaseState ChaseState { get; private set; }
        public EnemyKnockbackState KnockbackState { get; private set; }
        public EnemyFrozenState FrozenState { get; private set; }

        private void Awake()
        {
            _stateMachine = new StateMachine();
            ChaseState = new EnemyChaseState(this, _stateMachine);
            KnockbackState = new EnemyKnockbackState(this, _stateMachine);
            FrozenState = new EnemyFrozenState(this, _stateMachine);

            if (_enemyStats != null)
            {
                _enemyStats.LoadEnemyConfigAsyncAddress().Forget();
            }
            
            if (_enemyHealth != null)
            {
                _enemyHealth.OnHitEvent += HandleHit;
            }
        }

        private void OnEnable()
        {
            if (_activeEnemyCount != null) _activeEnemyCount.Value++;
            
            if (_enemyStats != null)
            {
                _enemyStats.ResetStatsFromConfig();
                if (_enemyHealth != null && _enemyHealth.CurrentHealth <= 0)
                {
                    _enemyHealth.InitializeHealth(_enemyStats.MaxHealth);
                }
            }
            
            _stateMachine.ChangeState(ChaseState);
        }

        private void OnDisable()
        {
            if (_activeEnemyCount != null) _activeEnemyCount.Value--;
        }

        private void OnDestroy()
        {
            if (_enemyHealth != null)
            {
                _enemyHealth.OnHitEvent -= HandleHit;
            }
        }

        private void FixedUpdate()
        {
            _stateMachine.Execute();
        }

        private void HandleHit()
        {
            if (_enemyVisuals != null)
            {
                _enemyVisuals.PlayHitEffectAndSound();
            }
        }

        public void InitializePlayer(PlayerController playerController)
        {
            if (_enemyMovement != null)
            {
                _enemyMovement.Initialize(playerController);
            }
        }

        public void ApplyAffliction(AfflictionConfig config)
        {
            if (_enemyAfflictionHandler != null)
            {
                _enemyAfflictionHandler.ApplyAffliction(config, this);
            }
        }

        public void Freeze(float duration)
        {
            FrozenState.SetParameters(duration);
            _stateMachine.ChangeState(FrozenState);
        }

        public void TakeDamage(float damage, ProjectileWeapon projectile)
        {
            if (projectile == null) return;
            
            Vector2 projectileDirection = projectile.transform.right;
            
            KnockbackState.SetParameters(projectileDirection * projectile.GetWeaponKnockback(), 0.3f);
            _stateMachine.ChangeState(KnockbackState);
            
            WeaponClass weaponClass = WeaponClass.None;
            AfflictionType afflictionType = AfflictionType.None;
            if (projectile.WeaponConfig != null)
            {
                weaponClass = projectile.WeaponConfig.WeaponClass;
                if (projectile.WeaponConfig.Afflictions != null && projectile.WeaponConfig.Afflictions.Count > 0)
                {
                    afflictionType = projectile.WeaponConfig.Afflictions[0].Type;
                }
            }
            
            if (_enemyHealth != null)
            {
                _enemyHealth.ApplyDamage(damage, DamageSourceInfo.FromWeapon(weaponClass, afflictionType));
            }
        }

        public void TakeDamage(float damage, Vector2 sourcePosition, float knockbackForce)
        {
            TakeDamage(damage, sourcePosition, knockbackForce, DamageSourceInfo.Default);
        }

        public void TakeDamage(float damage, Vector2 sourcePosition, float knockbackForce, DamageSourceInfo sourceInfo)
        {
            Vector2 knockbackDirection = ((Vector2)transform.position - sourcePosition).normalized;
            
            KnockbackState.SetParameters(knockbackDirection * knockbackForce, 0.3f);
            _stateMachine.ChangeState(KnockbackState);
            
            if (_enemyHealth != null)
            {
                _enemyHealth.ApplyDamage(damage, sourceInfo);
            }
        }

        public void TakeDamage(float damage)
        {
            if (_enemyHealth != null)
            {
                _enemyHealth.ApplyDamage(damage, DamageSourceInfo.Default);
            }
        }

        public void TakeDamage(float damage, DamageSourceInfo sourceInfo)
        {
            if (_enemyHealth != null)
            {
                _enemyHealth.ApplyDamage(damage, sourceInfo);
            }
        }

        public float GetKnockBackForce()
        {
            return _enemyStats != null ? _enemyStats.CurrentKnockbackForce : 0f;
        }

        public int GotHitByEnemy()
        {
            return _enemyStats != null ? Mathf.RoundToInt(_enemyStats.CurrentDamage) : 0;
        }

        public void ApplyRuntimeStats(EnemyRuntimeStats runtimeStats)
        {
            if (_enemyStats != null)
            {
                _enemyStats.ApplyRuntimeStats(runtimeStats);
                if (_enemyHealth != null)
                {
                    _enemyHealth.InitializeHealth(_enemyStats.MaxHealth);
                }
            }
        }

        public void KillEnemy()
        {
            if (_enemyHealth != null)
            {
                _enemyHealth.KillEnemy();
            }
        }

        // Facade methods for States
        public void MoveTowardsPlayer()
        {
            if (_enemyMovement != null)
            {
                _enemyMovement.MoveTowardsPlayer(_enemyStats != null ? _enemyStats.CurrentSpeed : 0f);
            }
        }

        public void ApplyImpulse(Vector2 force)
        {
            if (_enemyMovement != null)
            {
                _enemyMovement.ApplyImpulse(force);
            }
        }

        public void ResetVelocity()
        {
            if (_enemyMovement != null)
            {
                _enemyMovement.ResetVelocity();
            }
        }

        public void SetAnimationSpeed(float speed)
        {
            if (_enemyVisuals != null)
            {
                _enemyVisuals.SetAnimationSpeed(speed);
            }
        }

#if UNITY_EDITOR
        public void EditorSetupComponents(EnemyHealth h, EnemyStats s, EnemyMovement m, EnemyVisuals v, EnemyAfflictionHandler a)
        {
            _enemyHealth = h;
            _enemyStats = s;
            _enemyMovement = m;
            _enemyVisuals = v;
            _enemyAfflictionHandler = a;
        }
#endif
    }
}