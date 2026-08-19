using System;
using System.Threading;
using Collection;
using Collection.StateMachine;
using Data;
using Gameplay;
using Shared.Events;
using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using Obvious.Soap;


namespace Controllers
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Character Config")]
        public CharacterConfig CharacterConfig;
        public InputActionReference MovementInput;
        
        [Header("References")]
        [SerializeField] private PlayerCharacter _playerCharacter;
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private BoxCollider2D _boxCollider;
        [SerializeField] private PlayerHealth _characterHealthComponent;
        [SerializeField] private Vector2Variable _moveDirection;
        
        [Header("Player Settings")]
        [SerializeField] private float _maxKnockbackForce = 10f;
        
        
        private IdleState _idleState;
        private RunningState _runningState;
        private StateMachine _playerStateMachine;
        
        
        private Vector2 _enemyDirection;
        
        private Animator _playerCharacterAnimator;

        private float _initialCharacterSpeed;
        private float _initialCharacterHealth;
        private float _initialCharacterKnockbackResistance;
        private float _initialCharacterArmor;
        
        private bool _isKnockedBack;
        private readonly string _velocityX = "VelocityX";
        private readonly string _velocityY = "VelocityY";
        private readonly float _cameraShakeForce = 1f;

        private CancellationTokenSource _knockBackCts;

        public Vector2 FacingDirection { get; private set; } = Vector2.right;
        public PlayerHealth HealthComponent => _characterHealthComponent;

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        private void Start()
        {
            _playerStateMachine = new StateMachine();
            _runningState = new RunningState(this, _playerStateMachine, _playerCharacter.CharacterAnimator,_moveDirection.Value);
            _idleState = new IdleState(this, _playerStateMachine);
            
            _playerStateMachine.ChangeState(_idleState);
           
            Events_Game.OnGameStarted?.Invoke(this);
        }

        private void OnEnable()
        {
            Events_Character.OnCharacterChosen += ChosenCharacter;
            if (_characterHealthComponent != null)
            {
                _characterHealthComponent.OnDeath += GameOver;
            }
            
            UpdatePlayerStats();
            
            MovementInput.action.Enable();
        }

        private void OnDisable()
        {
            Events_Character.OnCharacterChosen -= ChosenCharacter;
            if (_characterHealthComponent != null)
            {
                _characterHealthComponent.OnDeath -= GameOver;
            }
            MovementInput.action.Disable();
            
            _knockBackCts?.Cancel();  
            _knockBackCts?.Dispose();  
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<PlayerController>();
        }

        private void Update()
        {
            _playerStateMachine.Execute();
            TransitionHandler();
        }

        private void FixedUpdate()
        { 
            PlayerMovement();
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if(_isKnockedBack) return;
            if (other.TryGetComponent(out EnemyController enemy))
            {
                if (_characterHealthComponent != null)
                {
                    _characterHealthComponent.TakeDamage(enemy.GotHitByEnemy());
                    if (_characterHealthComponent.CurrentHealth <= 0)
                    {
                        GameOver();
                        return;
                    }
                }

                _enemyDirection = (transform.position - enemy.transform.position).normalized;
                
                _knockBackCts = new CancellationTokenSource();
                
                PlayerKnockBack(_enemyDirection, enemy.GetKnockBackForce() , enemy.GetKnockBackForce(), _knockBackCts.Token).Forget();
            }
        }

        private void TransitionHandler()
        {
            if (_rb.linearVelocity != Vector2.zero && _playerStateMachine.CurrentState != _runningState)
            {
                _playerStateMachine.ChangeState(_runningState);
            }
                
            else if (_rb.linearVelocity == Vector2.zero && _playerStateMachine.CurrentState != _idleState)
            {
                _playerStateMachine.ChangeState(_idleState);
            }
        }

        private void PlayerMovement()
        {
            if(_isKnockedBack) return;
            
            Vector2 moveInput =  MovementInput.action.ReadValue<Vector2>();
            _moveDirection.Value = moveInput.normalized;

            if (_moveDirection.Value != Vector2.zero)
            {
                FacingDirection = _moveDirection.Value;

                if (FacingDirection.x < 0)
                {
                    _playerCharacter.FlipCharacter(true);
                }
                else if (FacingDirection.x > 0)
                {
                    _playerCharacter.FlipCharacter(false);
                }
            }
            
            if (_playerCharacterAnimator == null) UpdatePlayerStats();
            if (_playerCharacterAnimator == null) return;

            _playerCharacterAnimator.SetFloat(_velocityX, FacingDirection.x);
            _playerCharacterAnimator.SetFloat(_velocityY, FacingDirection.y);
            _rb.linearVelocity = moveInput.normalized * CharacterConfig.CharacterSpeed;
        }
        
        private void ChosenCharacter(CharacterConfig characterConfig)
        {
            CharacterConfig = characterConfig;
            UpdatePlayerStats();
        }
        
        private void UpdatePlayerStats()
        {
            _initialCharacterArmor = CharacterConfig.CharacterArmor;
            _initialCharacterHealth = CharacterConfig.CharacterHealth;
            _initialCharacterKnockbackResistance = CharacterConfig.CharacterKnockbackResistance;
            _initialCharacterSpeed = CharacterConfig.CharacterSpeed;
            
            
            _playerCharacterAnimator = _playerCharacter.CharacterAnimator;
            if (_characterHealthComponent != null)
            {
                _characterHealthComponent.InitializeHealth(CharacterConfig.CharacterHealth);
            }
        }

        private float CalculateKnockbackResistance(float maxKnockbackForce,float knockbackResistance)
        {
            if (maxKnockbackForce <= 0.001f || maxKnockbackForce < knockbackResistance)
            {
                return 0.4f;
            }
            
            var first = maxKnockbackForce -  knockbackResistance;
            var second = first + (maxKnockbackForce * 0.5f);
            var third = second / maxKnockbackForce;

            return third;
            
        }

        private float CalculateKnockbackForce(float knockbackforce)
        {
            if (knockbackforce > _maxKnockbackForce)
            {
                return _maxKnockbackForce;
            }

            return knockbackforce;
        }

        private void GameOver()
        {
            ResetStats();
            Events_Game.OnShowResultPanel?.Invoke(false);
        }

        private void ResetStats()
        {
            CharacterConfig.CharacterArmor = _initialCharacterArmor; 
            CharacterConfig.CharacterHealth = _initialCharacterHealth;
            CharacterConfig.CharacterKnockbackResistance = _initialCharacterKnockbackResistance;
            CharacterConfig.CharacterSpeed = _initialCharacterSpeed;
        }
        
         private async UniTask PlayerKnockBack(Vector2 direction, float force, float duration, CancellationToken token)
                {
                    try
                    {
                        if (_rb != null)
                        {
                            _rb.linearVelocity = Vector2.zero;
                            _rb.AddForce(direction * CalculateKnockbackForce(force), ForceMode2D.Impulse);
                        }
                        Events_Character.RequestShake(_cameraShakeForce);
                        
                        _isKnockedBack = true;
                        await UniTask.Delay(TimeSpan.FromSeconds(CalculateKnockbackResistance(duration, CharacterConfig.CharacterKnockbackResistance)), cancellationToken: token);
        
                    }
                    catch (OperationCanceledException)
                    {
                        Debug.Log("Player Knocked Back");
                    }
                    finally
                    {
                        _isKnockedBack = false;
                        if (_rb != null)
                        {
                            _rb.linearVelocity = Vector2.zero;
                        }
                    }
                }
        
        
    }
}

