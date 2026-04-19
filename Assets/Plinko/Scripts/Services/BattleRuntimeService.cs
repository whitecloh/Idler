using Plinko.Scripts.Models;

namespace Plinko.Scripts.Services
{
    public sealed class BattleRuntimeService
    {
        public BattleTimelineModel CurrentTimeline { get; set; }
        public BattleResultModel CurrentResult { get; set; }
        public EnemyWaveModel CurrentEnemyWave { get; set; }
        public BaseDefenseBattleStateModel CurrentBaseDefenseState { get; set; }
        public PowerLineBattleStateModel CurrentPowerLineState { get; set; }

        public void ClearTransient()
        {
            CurrentTimeline = null;
            CurrentResult = null;
            CurrentEnemyWave = null;
        }

        public void Clear()
        {
            ClearTransient();
            CurrentBaseDefenseState = null;
            CurrentPowerLineState = null;
        }
    }
}
