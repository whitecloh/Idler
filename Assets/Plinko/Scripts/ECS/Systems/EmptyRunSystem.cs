using Leopotam.EcsLite;

namespace Plinko.Scripts.ECS.Systems
{
    public abstract class EmptyRunSystem : IEcsInitSystem, IEcsRunSystem
    {
        public virtual void Init(IEcsSystems systems) { }
        public virtual void Run(IEcsSystems systems) { }
    }
}