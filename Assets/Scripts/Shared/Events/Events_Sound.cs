using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Shared.Events
{
    public static class Events_Sound
    {
        public static Action<AudioClip> PlaySound { get; set; }
         
    }

}
