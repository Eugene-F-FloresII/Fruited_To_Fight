using Controllers;
using UnityEngine;

namespace Collection.PlayerStateMachine
{
    public class RunningState : IState
    {
        private PlayerController _playerController;
        private PlayerStateMachine _playerStateMachine;

        public RunningState(PlayerController playerController,PlayerStateMachine playerStateMachine)
        {
            _playerController = playerController;
            _playerStateMachine = playerStateMachine;
        }
        
        public void Enter()
        {
            
        }

        public void Execute()
        {
            
        }

        public void Exit()
        {
            
        }
    }

}
