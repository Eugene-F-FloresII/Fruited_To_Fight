using UnityEngine;
using Controllers;

namespace Collection.EnemyStateMachine
{
    public class EnemyFrozenState : EnemyBaseState
    {
        private float _duration;
        private float _endTime;

        public EnemyFrozenState(EnemyController enemyController, StateMachine.StateMachine stateMachine) : base(enemyController, stateMachine)
        {
        }

        public void SetParameters(float duration)
        {
            _duration = duration;
        }

        public override void Enter()
        {
            EnemyController.SetAnimationSpeed(0f);
            EnemyController.ResetVelocity();
            _endTime = Time.time + _duration;
        }

        public override void Execute()
        {
            if (Time.time >= _endTime)
            {
                StateMachine.ChangeState(EnemyController.ChaseState);
            }
        }

        public override void Exit()
        {
            EnemyController.SetAnimationSpeed(1f);
        }
    }
}
