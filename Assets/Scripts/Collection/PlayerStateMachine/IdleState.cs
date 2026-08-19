using Controllers;
using UnityEngine;

using Collection.StateMachine;

namespace Collection.StateMachine
{
    public class IdleState : IState
    {
        private PlayerController _playerController;
        private StateMachine _playerStateMachine;
        
        private RunningState _runningState;
        
        public IdleState(PlayerController player, StateMachine playerStateMachine)
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
