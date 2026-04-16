using System;

namespace Plinko.Scripts.Models
{
    [Serializable]
    public sealed class TrainingPipelineRunModel
    {
        public PlinkoPathResultModel Result;
        public float PlaybackDuration;
        public int TotalNodeCount;
    }
}
