using System;
using UnityEngine;

namespace Shared.Events
{
    public static class Events_Wisps
    {
        public static Action<string> OnChosenWisp { get; set; }
        public static Action<GameObject> OnSpawnedWisp { get; set; }
    }
}
