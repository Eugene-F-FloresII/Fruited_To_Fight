using System;
using Controllers;
using Data;

namespace Shared.Events
{
    public static class Events_Boss
    {
        public static Action<EnemyController, EnemyBossConfig> OnBossSpawned;
    }
}
