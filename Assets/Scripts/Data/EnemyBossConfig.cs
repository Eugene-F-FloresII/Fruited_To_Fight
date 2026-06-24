using UnityEngine;

namespace Data
{
    [CreateAssetMenu(menuName = "Data/Create Enemy Boss Configuration", fileName = "EnemyBoss")]
    public class EnemyBossConfig : EnemyConfig
    {
        [Header("Boss Details")]
        public string BossName;
        public int BossLevel = 1;
    }
}
