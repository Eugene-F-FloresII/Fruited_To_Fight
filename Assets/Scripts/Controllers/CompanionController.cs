using System;
using Obvious.Soap;
using UnityEngine;

namespace Controllers
{
    public class CompanionController : MonoBehaviour
    {
        
        [SerializeField] private Vector2Variable _vector2Variable;
        [SerializeField] private Animator _animator;
        private readonly string _velocityX = "VelocityX";
        private readonly string _velocityY = "VelocityY";

        private void FixedUpdate()
        {
            CompanionMovement();
        }

        private void CompanionMovement()
        {
            _animator.SetFloat(_velocityX, _vector2Variable.Value.x);
            _animator.SetFloat(_velocityY, _vector2Variable.Value.y);
        }
    }

}
