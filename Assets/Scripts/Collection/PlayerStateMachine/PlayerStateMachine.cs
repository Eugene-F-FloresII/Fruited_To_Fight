using UnityEngine;

namespace Collection.PlayerStateMachine
{
    public class PlayerStateMachine
    {
        private IState _currentState;

        public void ChangeState(IState newState)
        {
            _currentState?.Exit();
            _currentState = newState;
            _currentState?.Enter();
        }
        
        public void Execute() => _currentState?.Execute();
    }

}
