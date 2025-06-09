using Game.Events;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Systems
{
    public class AutosaveSystem : IEcsRunSystem
    {
        private float _timer;
        private float _interval;

        public void Init(float autosaveInterval)
        {
            _interval = autosaveInterval;
        }

        public void Run(IEcsSystems systems)
        {
            _timer += Time.deltaTime;
            
            if (!(_timer >= _interval)) 
                return;
            
            _timer = 0f;
            var world = systems.GetWorld();
            var entity = world.NewEntity();
            world.GetPool<SaveEvent>().Add(entity);
        }
    }
}