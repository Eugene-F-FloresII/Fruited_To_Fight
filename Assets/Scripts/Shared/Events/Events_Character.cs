using System;
using Data;

namespace Shared.Events
{
    public static class Events_Character
    {
        public static Action<CharacterConfig> OnCharacterChosen;

        public static event Action<float> OnShakeRequested;
        public static void RequestShake(float force) => OnShakeRequested?.Invoke(force);

    }

}
