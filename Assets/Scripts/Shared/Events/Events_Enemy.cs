using System;
using UnityEngine;

namespace Shared.Events
{
    public static class Events_Enemy
    {
        //for testing
        public static Action<Transform> OnPlayerDetected;
        public static Action<Vector2, int> OnEnemyHit;
        public static Action OnEnemyDeath;
    }

}
