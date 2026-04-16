using System;
using System.Collections.Generic;

namespace Plinko.Scripts.Models
{
    [Serializable]
    public sealed class PlinkoPathResultModel
    {
        public List<PlinkoPathNodeModel> Nodes = new();
        public string FinalBasketId;
        public int FinalBasketManaValue;
        public TrainedUnitResultModel Result;
    }
}