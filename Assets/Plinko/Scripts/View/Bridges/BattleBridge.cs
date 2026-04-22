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
        public void RequestDeployCard(int handCardRuntimeId)
        {
            ref var request = ref _world.GetPool<DeployCardRequest>().Add(_world.NewEntity());
            request.HandCardRuntimeId = handCardRuntimeId;
            request.HasBoardTarget = false;
            request.TargetLaneIndex = -1;
            request.TargetCellIndex = -1;
        }

        public void RequestDeployCard(int handCardRuntimeId, int targetLaneIndex, int targetCellIndex)
        {
            ref var request = ref _world.GetPool<DeployCardRequest>().Add(_world.NewEntity());
            request.HandCardRuntimeId = handCardRuntimeId;
            request.HasBoardTarget = true;
            request.TargetLaneIndex = targetLaneIndex;
            request.TargetCellIndex = targetCellIndex;
        }
        public void RequestRerollPowerLineHand() => _world.GetPool<RerollPowerLineHandRequest>().Add(_world.NewEntity());
        public void RequestFinalizePowerLineBattleResult() => _world.GetPool<FinalizePowerLineBattleResultRequest>().Add(_world.NewEntity());
        public void RequestStartBattle() => _world.GetPool<StartBattleRequest>().Add(_world.NewEntity());
        public void RequestReturnToMenu()
        {
            _world.GetPool<SaveRunRequest>().Add(_world.NewEntity());
            _world.GetPool<ReturnToMenuRequest>().Add(_world.NewEntity());
        }
    }
}
