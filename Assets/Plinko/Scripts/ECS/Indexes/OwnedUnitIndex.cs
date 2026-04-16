using System.Collections.Generic;

namespace Plinko.Scripts.ECS.Indexes
{
    public sealed class OwnedUnitIndex
    {
        private readonly Dictionary<int, int> _runtimeIdToEntity = new();

        public void Register(int runtimeId, int entity)
        {
            _runtimeIdToEntity[runtimeId] = entity;
        }

        public bool TryGet(int runtimeId, out int entity)
        {
            return _runtimeIdToEntity.TryGetValue(runtimeId, out entity);
        }

        public void Unregister(int runtimeId)
        {
            _runtimeIdToEntity.Remove(runtimeId);
        }

        public void Clear()
        {
            _runtimeIdToEntity.Clear();
        }
    }
}