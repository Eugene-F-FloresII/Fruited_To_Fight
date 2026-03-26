using System;
using Collection.PlayerStateMachine;
using Data;
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
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private BoxCollider2D _boxCollider;
        
        
        [Header("Player States")]
        private IdleState _idleState;
        private RunningState _runningState;
        private PlayerStateMachine _playerStateMachine;


        private void Awake()
        {
            _playerStateMachine = new PlayerStateMachine();
            _runningState = new RunningState(this, _playerStateMachine);
            _idleState = new IdleState(this, _playerStateMachine);
            
        }

        private void Start()
        {
            _playerStateMachine.ChangeState(_idleState);
            MovementInput.action.Enable();
        }

        private void OnEnable()
        {
            Events_Character.OnCharacterChosen += ChosenCharacter;
            MovementInput.action.Disable();
        }

        private void OnDisable()
        {
            Events_Character.OnCharacterChosen -= ChosenCharacter;
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
                _playerStateMachine.ChangeState(_runningState);
            else if (_rb.linearVelocity == Vector2.zero && _playerStateMachine.CurrentState != _idleState)
                _playerStateMachine.ChangeState(_idleState);
        }

        private void PlayerMovement()
        {
            Vector2 moveInput =  MovementInput.action.ReadValue<Vector2>();
            _rb.linearVelocity = moveInput * CharacterConfig.CharacterSpeed;
        }
        
        private void ChosenCharacter(CharacterConfig characterConfig) => CharacterConfig = characterConfig;
        
    }
}

