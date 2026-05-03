using System;
using Controllers;

namespace Shared.Events
{
    public static class Events_Game
    {
        public static Action<string> OnSceneChange;
        public static Action OnGamePaused;
        public static Action OnGameResumed;
        public static Action OnGameRestarted;
        public static Action OnGameExited;
        public static Action<PlayerController> OnGameStarted;
    }
}
