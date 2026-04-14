using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Requests;
using UnityEngine;

namespace Plinko.Scripts.View.Bridges
{
    public sealed class LocationBridge : MonoBehaviour
    {
        private EcsWorld _world;

        public void Init(EcsWorld world)
        {
            _world = world;
        }

        public void RequestStartLevel(int levelIndex)
        {
            var entity = _world.NewEntity();
            _world.GetPool<StartLevelRequest>().Add(entity).LevelIndex = levelIndex;
        }
    }
}