using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Requests;
using UnityEngine;

namespace Plinko.Scripts.View.Bridges
{
    public sealed class LocationBridge : MonoBehaviour
    {
        private EcsWorld _world;
        public void Init(EcsWorld world) => _world = world;
        public void RequestStartLevel(int levelIndex) => _world.GetPool<StartLevelRequest>().Add(_world.NewEntity()).LevelIndex = levelIndex;
        public void RequestAdvanceToNextLevel() => _world.GetPool<AdvanceToNextLevelRequest>().Add(_world.NewEntity());
        public void RequestReturnToMenu()
        {
            _world.GetPool<SaveRunRequest>().Add(_world.NewEntity());
            _world.GetPool<ReturnToMenuRequest>().Add(_world.NewEntity());
        }
    }
}
