using UnityEngine;
using Controllers;

namespace Collection.EnemyStateMachine
{
    public class EnemyKnockbackState : EnemyBaseState
    {
        private Vector2 _force;
        private float _duration;
        private float _endTime;

        public EnemyKnockbackState(EnemyController enemyController, StateMachine.StateMachine stateMachine) : base(enemyController, stateMachine)
        {
        }

        public void SetParameters(Vector2 force, float duration)
        {
            _force = force;
            _duration = duration;
        }

        public override void Enter()
        {
            EnemyController.ApplyImpulse(_force);
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
            EnemyController.ResetVelocity();
        }
    }
}
