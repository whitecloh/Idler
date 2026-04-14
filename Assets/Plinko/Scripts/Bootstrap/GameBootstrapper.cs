using Leopotam.EcsLite;
using Plinko.Scripts.ECS.Installers;
using Plinko.Scripts.View;
using UnityEngine;

namespace Plinko.Scripts.Bootstrap
{
    public sealed class GameBootstrapper : MonoBehaviour
    {
        [SerializeField] private GameServicesInstaller gameServicesInstaller;
        [SerializeField] private UiCompositionRoot uiCompositionRoot;

        private EcsWorld _world;
        private IEcsSystems _systems;
        private GameServicesContext _services;

        private void Awake()
        {
            _services = gameServicesInstaller.Build();
            _world = new EcsWorld();
            _systems = new EcsCompositionRoot(_services, uiCompositionRoot).Create(_world);
            _systems.Init();
            uiCompositionRoot.Init(_world);
        }

        private void Update()
        {
            _systems?.Run();
        }

        private void OnDestroy()
        {
            _systems?.Destroy();
            _world?.Destroy();
            _systems = null;
            _world = null;
        }
    }
}