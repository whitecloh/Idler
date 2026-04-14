using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Requests;
using UnityEngine;

namespace Plinko.Scripts.View.Bridges
{
    public sealed class BattleBridge : MonoBehaviour
    {
        private EcsWorld _world;

        public void Init(EcsWorld world)
        {
            _world = world;
        }

        public void RequestGenerateHand()
        {
            var entity = _world.NewEntity();
            _world.GetPool<GenerateHandRequest>().Add(entity);
        }

        public void RequestDeployUnit(int cardId)
        {
            var entity = _world.NewEntity();
            _world.GetPool<DeployUnitRequest>().Add(entity).CardId = cardId;
        }

        public void RequestStartBattle()
        {
            var entity = _world.NewEntity();
            _world.GetPool<StartBattleRequest>().Add(entity);
        }

        public void RequestAdvanceToNextLevel()
        {
            var entity = _world.NewEntity();
            _world.GetPool<AdvanceToNextLevelRequest>().Add(entity);
        }

        public void RequestReturnToMenu()
        {
            var entity = _world.NewEntity();
            _world.GetPool<ReturnToMenuRequest>().Add(entity);
        }
    }
}