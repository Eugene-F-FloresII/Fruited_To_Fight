using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Data;

namespace Shared.Events
{
    public static class Events_Leaderboard
    {
        public static Func<string, int, int, UniTask<bool>> OnSubmitScore { get; set; }
        public static Func<int, UniTask<List<LeaderboardEntry>>> OnFetchLeaderboard { get; set; }
    }
}
