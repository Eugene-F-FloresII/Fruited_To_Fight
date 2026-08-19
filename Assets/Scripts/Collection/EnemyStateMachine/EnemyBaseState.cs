using Collection.StateMachine;
using Controllers;

namespace Collection.EnemyStateMachine
{
    public abstract class EnemyBaseState : IState
    {
        protected readonly EnemyController EnemyController;
        protected readonly StateMachine.StateMachine StateMachine;

        protected EnemyBaseState(EnemyController enemyController, StateMachine.StateMachine stateMachine)
        {
            EnemyController = enemyController;
            StateMachine = stateMachine;
        }

        public virtual void Enter() { }
        public virtual void Execute() { }
        public virtual void Exit() { }
    }
}
