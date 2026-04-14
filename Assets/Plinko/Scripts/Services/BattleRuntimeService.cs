using Plinko.Scripts.Models;

namespace Plinko.Scripts.Services
{
    public sealed class BattleRuntimeService
    {
        public BattleTimelineModel CurrentTimeline { get; set; }
        public BattleResultModel CurrentResult { get; set; }

        public void Clear()
        {
            CurrentTimeline = null;
            CurrentResult = null;
        }
    }
}