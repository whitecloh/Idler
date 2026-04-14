using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Requests;
using UnityEngine;

namespace Plinko.Scripts.View.Bridges
{
    public sealed class MainMenuBridge : MonoBehaviour
    {
        private EcsWorld _world;

        public void Init(EcsWorld world)
        {
            _world = world;
        }

        public void RequestStartNewRun(string locationId)
        {
            var entity = _world.NewEntity();
            _world.GetPool<StartNewRunRequest>().Add(entity).LocationId = locationId;
        }

        public void RequestContinueRun()
        {
            var entity = _world.NewEntity();
            _world.GetPool<ContinueRunRequest>().Add(entity);
        }
    }
}