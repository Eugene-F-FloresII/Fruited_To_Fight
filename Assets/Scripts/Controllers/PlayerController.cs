using System;
using Collection.PlayerStateMachine;
using Data;
using UnityEngine;
using UnityEngine.InputSystem;


namespace Controllers
{
    public class PlayerController : MonoBehaviour
    {
        
        [Header("References")]
        [SerializeField] public Rigidbody2D _rb;
        [SerializeField] private BoxCollider2D _boxCollider;
        [SerializeField] private InputActionReference _movementInput;
        
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
        }

        private void Update()
        {
            _playerStateMachine.Execute();
            TransitionHandler();
        }

        private void TransitionHandler()
        {
            if (_rb.linearVelocity != Vector2.zero)
            {
                _playerStateMachine.ChangeState(_runningState);
            }
            else if (_rb.linearVelocity ==  Vector2.zero)
            {
                _playerStateMachine.ChangeState(_idleState);
            }
        }
    }
}

