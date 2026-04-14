using Leopotam.EcsLite;

namespace Plinko.Scripts.ECS.Utils
{
    public static class EcsPoolExtensions
    {
        public static ref T GetOrAdd<T>(this EcsPool<T> pool, int entity) where T : struct
        {
            if (!pool.Has(entity))
            {
                return ref pool.Add(entity);
            }

            return ref pool.Get(entity);
        }
    }
}