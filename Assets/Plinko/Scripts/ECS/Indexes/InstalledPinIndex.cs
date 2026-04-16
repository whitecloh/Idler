using System.Collections.Generic;

namespace Plinko.Scripts.ECS.Indexes
{
    public sealed class InstalledPinIndex
    {
        private readonly Dictionary<int, int> _slotToEntity = new();

        public void Register(int slotIndex, int entity)
        {
            _slotToEntity[slotIndex] = entity;
        }

        public bool TryGet(int slotIndex, out int entity)
        {
            return _slotToEntity.TryGetValue(slotIndex, out entity);
        }

        public void Unregister(int slotIndex)
        {
            _slotToEntity.Remove(slotIndex);
        }

        public void Clear()
        {
            _slotToEntity.Clear();
        }
    }
}