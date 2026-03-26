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

        public RunningState(PlayerController playerController,PlayerStateMachine playerStateMachine)
        {
            _playerController = playerController;
            _playerStateMachine = playerStateMachine;
       
        }
        
        public void Enter()
        {
            Debug.Log("Entering Running State");
        }

        public void Execute()
        {
            
        }

        public void Exit()
        {
            Debug.Log("Exiting Running State");
        }
    }

}
