using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Requests;
using UnityEngine;

namespace Plinko.Scripts.View.Bridges
{
    public sealed class MainMenuBridge : MonoBehaviour
    {
        private EcsWorld _world;
        public void Init(EcsWorld world) => _world = world;
        public void RequestStartNewRun(string locationId) => _world.GetPool<StartNewRunRequest>().Add(_world.NewEntity()).LocationId = locationId;
        public void RequestContinueRun() => _world.GetPool<ContinueRunRequest>().Add(_world.NewEntity());
    }
}