
namespace Collection.PlayerStateMachine
{
    public interface IState
    {
        void Enter();
        void Execute();
        void Exit();
    }

}
