using System;

namespace Shared.Events
{
    public static class Events_Round
    {
        public static Action<int> OnRoundStarted;
        public static Action<int> OnRoundEnded;
    }
}
