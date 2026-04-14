using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Requests;
using UnityEngine;

namespace Plinko.Scripts.View.Bridges
{
    public sealed class UpgradePhaseBridge : MonoBehaviour
    {
        private EcsWorld _world;

        public void Init(EcsWorld world)
        {
            _world = world;
        }

        public void RequestSelectOwnedUnit(int runtimeId)
        {
            var entity = _world.NewEntity();
            _world.GetPool<SelectUnitsForUpgradeRequest>().Add(entity).RuntimeId = runtimeId;
        }

        public void RequestConfirmUpgradeSelection()
        {
            var entity = _world.NewEntity();
            _world.GetPool<ConfirmUpgradeSelectionRequest>().Add(entity);
        }

        public void RequestStartTraining()
        {
            var entity = _world.NewEntity();
            _world.GetPool<StartTrainingRequest>().Add(entity);
        }
    }
}