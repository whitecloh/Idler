using Leopotam.EcsLite;
using Plinko.Scripts.Debugging;
using Plinko.Scripts.ECS.Installers;
using Plinko.Scripts.ECS.Requests;
using Plinko.Scripts.View;
using UnityEngine;

namespace Plinko.Scripts.Bootstrap
{
    public sealed class GameBootstrapper : MonoBehaviour
    {
        [SerializeField] private GameServicesInstaller gameServicesInstaller;
        [SerializeField] private UiCompositionRoot uiCompositionRoot;
        [SerializeField] private bool attachDevHarness = true;

        private EcsWorld _world;
        private IEcsSystems _systems;
        private GameServicesContext _services;

        public EcsWorld World => _world;
        public GameServicesContext Services => _services;
        public bool IsReady => _world != null && _systems != null && _services != null;

        private void Awake()
        {
            _services = gameServicesInstaller.Build();
            uiCompositionRoot.Configure(_services);
            _world = new EcsWorld();
            _systems = new EcsCompositionRoot(_services, uiCompositionRoot).Create(_world);
            _systems.Init();
            uiCompositionRoot.Init(_world);

            if (attachDevHarness)
            {
                var devHarness = GetComponent<PlinkoDevHarness>() ?? gameObject.AddComponent<PlinkoDevHarness>();
                devHarness.Initialize(this);
            }
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
