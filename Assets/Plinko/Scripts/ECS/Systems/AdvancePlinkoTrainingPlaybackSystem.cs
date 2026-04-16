using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using UnityEngine;

namespace Plinko.Scripts.ECS.Systems
{
    public sealed class AdvancePlinkoTrainingPlaybackSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsFilter _playbackFilter;
        private EcsPool<PlinkoTrainingPlaybackComponent> _playbackPool;
        private EcsPool<TrainingCompletedEvent> _trainingCompletedEventPool;

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _playbackFilter = world.Filter<PlinkoTrainingPlaybackComponent>().End();
            _playbackPool = world.GetPool<PlinkoTrainingPlaybackComponent>();
            _trainingCompletedEventPool = world.GetPool<TrainingCompletedEvent>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            foreach (var entity in _playbackFilter)
            {
                ref var playback = ref _playbackPool.Get(entity);
                if (playback.IsCompleted)
                {
                    continue;
                }

                playback.Elapsed += Time.deltaTime;
                if (playback.TotalNodeCount > 0)
                {
                    var normalized = Mathf.Clamp01(playback.Elapsed / Mathf.Max(0.0001f, playback.Duration));
                    playback.CurrentNodeIndex = Mathf.Clamp(Mathf.FloorToInt(normalized * playback.TotalNodeCount), 0, playback.TotalNodeCount);
                }

                if (playback.Elapsed < playback.Duration)
                {
                    continue;
                }

                playback.IsCompleted = true;
                _trainingCompletedEventPool.Add(world.NewEntity()) = new TrainingCompletedEvent
                {
                    RuntimeId = playback.RuntimeId,
                    IsRetraining = playback.IsRetraining
                };
                world.DelEntity(entity);
            }
        }
    }
}