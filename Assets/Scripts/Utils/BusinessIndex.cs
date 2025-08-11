namespace Utils
{
    using System.Collections.Generic;
    using Game.Data.Business;
    
    public sealed class BusinessIndex
    {
        private readonly Dictionary<BusinessId, int> _idToEntity = new();

        public void Register(int entity, BusinessId id)
        {
            _idToEntity[id] = entity;
        }

        public bool TryGet(BusinessId id, out int entity) => _idToEntity.TryGetValue(id, out entity);
    }
}