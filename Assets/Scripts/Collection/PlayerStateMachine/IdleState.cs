using Controllers;
using UnityEngine;

namespace Collection.PlayerStateMachine
{
    public class IdleState : IState
    {
        private PlayerController _playerController;
        private PlayerStateMachine _playerStateMachine;
        
        private RunningState _runningState;
        
        public IdleState(PlayerController player, PlayerStateMachine playerStateMachine)
        {
            this._playerController = player;
            this._playerStateMachine = playerStateMachine;
        }

        public void Enter()
        {
            
        }

        public void Execute()
        {
            if (_playerController._rb.linearVelocity != Vector2.zero)
            {
                _playerStateMachine.ChangeState(_runningState);
            }
        }

        public void Exit()
        {
            
        }
    }

}
