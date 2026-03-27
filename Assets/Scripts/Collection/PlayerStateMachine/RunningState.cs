using Controllers;
using Data;
using UnityEngine;

namespace Collection.PlayerStateMachine
{
    public class RunningState : IState
    {
        private CharacterConfig _characterConfig;
        private PlayerController _playerController;
        private PlayerStateMachine _playerStateMachine;
        private Animator _animator;
        private Vector2 _moveDirection;

        public RunningState(PlayerController playerController,PlayerStateMachine playerStateMachine, Animator animator, Vector2 moveDirection)
        {
            _playerController = playerController;
            _playerStateMachine = playerStateMachine;
            _animator = animator;
            _moveDirection = moveDirection;
        }
        
        public void Enter()
        {
            _animator.SetBool("IsMoving", true);
            _animator.CrossFade("Movement", 0.01f);
        }

        public void Execute()
        {
           
        }

        public void Exit()
        {
            _animator.SetBool("IsMoving", false);
            _animator.CrossFade("Idle", 0.01f);
        }
    }

}
