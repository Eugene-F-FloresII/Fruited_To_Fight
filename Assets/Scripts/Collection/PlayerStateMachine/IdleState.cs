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
            Debug.Log("Entering Idle State");
        }

        public void Execute()
        {
            
        }

        public void Exit()
        {
            Debug.Log("Exiting Idle State");
        }
    }

}
