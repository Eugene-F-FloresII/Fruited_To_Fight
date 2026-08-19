using Controllers;

namespace Collection.EnemyStateMachine
{
    public class EnemyChaseState : EnemyBaseState
    {
        public EnemyChaseState(EnemyController enemyController, StateMachine.StateMachine stateMachine) : base(enemyController, stateMachine)
        {
        }

        public override void Execute()
        {
            EnemyController.MoveTowardsPlayer();
        }
    }
}
