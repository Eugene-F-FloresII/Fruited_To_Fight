using Collection.PlayerStateMachine;
using Data;
using Gameplay;
using Shared.Events;
using UnityEngine;
using UnityEngine.InputSystem;


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
        
        
        [Header("Player States")]
        private IdleState _idleState;
        private RunningState _runningState;
        private PlayerStateMachine _playerStateMachine;
        
        private Vector2 _moveDirection;


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
            MovementInput.action.Enable();
        }

        private void OnDisable()
        {
            Events_Character.OnCharacterChosen -= ChosenCharacter;
            MovementInput.action.Disable();
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
            Vector2 moveInput =  MovementInput.action.ReadValue<Vector2>();
            _moveDirection = moveInput.normalized;
            
            _playerCharacter.CharacterAnimator.SetFloat("VelocityX", _moveDirection.x);
            _playerCharacter.CharacterAnimator.SetFloat("VelocityY", _moveDirection.y);
            _rb.linearVelocity = moveInput.normalized * CharacterConfig.CharacterSpeed;
        }
        
        private void ChosenCharacter(CharacterConfig characterConfig) => CharacterConfig = characterConfig;
        
    }
}

