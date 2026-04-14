using System.Collections.Generic;

namespace Plinko.Scripts.ECS.Indexes
{
    public sealed class ShopOfferIndex
    {
        private readonly Dictionary<int, int> _offerIdToEntity = new();

        public void Register(int offerId, int entity)
        {
            _offerIdToEntity[offerId] = entity;
        }

        public void Unregister(int offerId)
        {
            _offerIdToEntity.Remove(offerId);
        }

        public bool TryGet(int offerId, out int entity)
        {
            return _offerIdToEntity.TryGetValue(offerId, out entity);
        }

        public void Clear()
        {
            _offerIdToEntity.Clear();
        }
    }
}