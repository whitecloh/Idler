using System.Collections.Generic;
using Plinko.Scripts.Models;

namespace Plinko.Scripts.Services
{
    public sealed class PlinkoRuntimeService
    {
        private readonly Dictionary<int, PlinkoPathResultModel> _resultsByRuntimeId = new();

        public void SetResult(int runtimeId, PlinkoPathResultModel result)
        {
            _resultsByRuntimeId[runtimeId] = result;
        }

        public bool TryGetResult(int runtimeId, out PlinkoPathResultModel result)
        {
            return _resultsByRuntimeId.TryGetValue(runtimeId, out result);
        }

        public void RemoveResult(int runtimeId)
        {
            _resultsByRuntimeId.Remove(runtimeId);
        }

        public void Clear()
        {
            _resultsByRuntimeId.Clear();
        }
    }
}