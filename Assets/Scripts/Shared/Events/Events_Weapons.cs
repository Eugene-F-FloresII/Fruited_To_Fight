using System;
using UnityEngine;

namespace Shared.Events
{
    public static class Events_Weapons
    {
        public static Action<Transform> OnEnemyDetectedHoming { get; set; }
    }

}
