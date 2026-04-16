using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Requests;
using UnityEngine;

namespace Plinko.Scripts.View.Bridges
{
    public sealed class BattleBridge : MonoBehaviour
    {
        private EcsWorld _world;
        public void Init(EcsWorld world) => _world = world;
        public void RequestGenerateHand() => _world.GetPool<GenerateHandRequest>().Add(_world.NewEntity());
        public void RequestDeployCard(int handCardRuntimeId) => _world.GetPool<DeployCardRequest>().Add(_world.NewEntity()).HandCardRuntimeId = handCardRuntimeId;
        public void RequestStartBattle() => _world.GetPool<StartBattleRequest>().Add(_world.NewEntity());
        public void RequestReturnToMenu() => _world.GetPool<ReturnToMenuRequest>().Add(_world.NewEntity());
    }
}