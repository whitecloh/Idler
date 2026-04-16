using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Requests;
using UnityEngine;

namespace Plinko.Scripts.View.Bridges
{
    public sealed class RetrainingPhaseBridge : MonoBehaviour
    {
        private EcsWorld _world;
        public void Init(EcsWorld world) => _world = world;
        public void RequestSelectOwnedUnit(int runtimeId) => _world.GetPool<SelectUnitsForRetrainingRequest>().Add(_world.NewEntity()).RuntimeId = runtimeId;
        public void RequestConfirmRetrainingSelection() => _world.GetPool<ConfirmRetrainingSelectionRequest>().Add(_world.NewEntity());
    }
}