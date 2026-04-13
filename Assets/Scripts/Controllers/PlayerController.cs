using System;
using System.Threading;
using Collection;
using Collection.PlayerStateMachine;
using Data;
using Gameplay;
using Shared.Events;
using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;


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
        
        [Header("Player Settings")]
        [SerializeField] private float _maxKnockbackForce = 10f;
        
        
        private IdleState _idleState;
        private RunningState _runningState;
        private PlayerStateMachine _playerStateMachine;
        
        private Vector2 _moveDirection;
        private Vector2 _enemyDirection;
        
        private Animator _playerCharacterAnimator;
        
        private bool _isKnockedBack;
        private readonly string _velocityX = "VelocityX";
        private readonly string _velocityY = "VelocityY";
        private readonly float _cameraShakeForce = 1f;

        private CancellationTokenSource _knockBackCts;

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        private void Start()
        {
            _playerStateMachine = new PlayerStateMachine();
            _runningState = new RunningState(this, _playerStateMachine, _playerCharacter.CharacterAnimator,_moveDirection);
            _idleState = new IdleState(this, _playerStateMachine);
            
            _playerStateMachine.ChangeState(_idleState);
           
        }

        private void OnEnable()
        {
            Events_Character.OnCharacterChosen += ChosenCharacter;
            
            UpdatePlayerStats();
            
            MovementInput.action.Enable();
        }

        private void OnDisable()
        {
            Events_Character.OnCharacterChosen -= ChosenCharacter;
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
            _moveDirection = moveInput.normalized;
            
            _playerCharacterAnimator.SetFloat(_velocityX, _moveDirection.x);
            _playerCharacterAnimator.SetFloat(_velocityY, _moveDirection.y);
            _rb.linearVelocity = moveInput.normalized * CharacterConfig.CharacterSpeed;
        }
        
        private void ChosenCharacter(CharacterConfig characterConfig) => CharacterConfig = characterConfig;

        private async UniTask PlayerKnockBack(Vector2 direction, float force, float duration, CancellationToken token)
        {
            try
            {
                _rb.linearVelocity = Vector2.zero;
                _rb.AddForce(direction * CalculateKnockbackForce(force), ForceMode2D.Impulse);
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
                _rb.linearVelocity = Vector2.zero;
            }
        }
        
        private void UpdatePlayerStats()
        {
            _playerCharacterAnimator = _playerCharacter.CharacterAnimator;
        }

        private float CalculateKnockbackResistance(float maxKnockbackForce,float knockbackResistance)
        {
            if (maxKnockbackForce < knockbackResistance)
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
        
    }
}

