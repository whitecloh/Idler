using System.Collections.Generic;
using Leopotam.EcsLite;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.ECS.Events;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.ECS.Utils;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.Services;
using Plinko.Scripts.View.Controllers;

namespace Plinko.Scripts.ECS.UISystems
{
    public sealed class RefreshFieldUpgradeUiSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly FieldUpgradePhaseScreenController _controller;
        private readonly PinConfigService _pinConfigService;
        private readonly GameSettingsService _gameSettingsService;
        private readonly RunEntityIndex _runEntityIndex;

        private EcsFilter _phaseChangedFilter;
        private EcsFilter _fieldUpgradeEnteredFilter;
        private EcsFilter _pinShopOffersChangedFilter;
        private EcsFilter _goldChangedFilter;
        private EcsFilter _boardSlotSelectionChangedFilter;
        private EcsFilter _plinkoBoardChangedFilter;
        private EcsFilter _pinPurchasedFilter;
        private EcsFilter _pinOfferFilter;
        private EcsFilter _installedPinFilter;
        private EcsFilter _candidateFilter;
        private EcsPool<CurrentPhaseComponent> _phasePool;
        private EcsPool<CurrentGoldComponent> _goldPool;
        private EcsPool<FieldUpgradePhaseStateComponent> _fieldUpgradeStatePool;
        private EcsPool<PinShopOfferComponent> _pinOfferPool;
        private EcsPool<ShopOfferPinTypeIdComponent> _pinOfferTypePool;
        private EcsPool<OfferPriceComponent> _pricePool;
        private EcsPool<InstalledPinComponent> _installedPinPool;
        private EcsPool<BoughtPinCandidateComponent> _candidatePool;

        public RefreshFieldUpgradeUiSystem(FieldUpgradePhaseScreenController controller,
            PinConfigService pinConfigService, GameSettingsService gameSettingsService, RunEntityIndex runEntityIndex)
        {
            _controller = controller;
            _pinConfigService = pinConfigService;
            _gameSettingsService = gameSettingsService;
            _runEntityIndex = runEntityIndex;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            _phaseChangedFilter = world.Filter<PhaseChangedEvent>().End();
            _fieldUpgradeEnteredFilter = world.Filter<FieldUpgradePhaseEnteredEvent>().End();
            _pinShopOffersChangedFilter = world.Filter<PinShopOffersChangedEvent>().End();
            _goldChangedFilter = world.Filter<GoldChangedEvent>().End();
            _boardSlotSelectionChangedFilter = world.Filter<BoardSlotSelectionChangedEvent>().End();
            _plinkoBoardChangedFilter = world.Filter<PlinkoBoardChangedEvent>().End();
            _pinPurchasedFilter = world.Filter<PinPurchasedEvent>().End();
            _pinOfferFilter = world.Filter<PinShopOfferComponent>().End();
            _installedPinFilter = world.Filter<InstalledPinComponent>().End();
            _candidateFilter = world.Filter<BoughtPinCandidateComponent>().End();
            _phasePool = world.GetPool<CurrentPhaseComponent>();
            _goldPool = world.GetPool<CurrentGoldComponent>();
            _fieldUpgradeStatePool = world.GetPool<FieldUpgradePhaseStateComponent>();
            _pinOfferPool = world.GetPool<PinShopOfferComponent>();
            _pinOfferTypePool = world.GetPool<ShopOfferPinTypeIdComponent>();
            _pricePool = world.GetPool<OfferPriceComponent>();
            _installedPinPool = world.GetPool<InstalledPinComponent>();
            _candidatePool = world.GetPool<BoughtPinCandidateComponent>();
        }
        
        public void Run(IEcsSystems systems)
        {
            if (_controller == null || !_runEntityIndex.TryGetRunEntity(out var runEntity))
            {
                return;
            }

            var shouldRefresh = _phaseChangedFilter.GetEntitiesCount() > 0 ||
                                _fieldUpgradeEnteredFilter.GetEntitiesCount() > 0 ||
                                _pinShopOffersChangedFilter.GetEntitiesCount() > 0 ||
                                _goldChangedFilter.GetEntitiesCount() > 0 ||
                                _boardSlotSelectionChangedFilter.GetEntitiesCount() > 0 ||
                                _plinkoBoardChangedFilter.GetEntitiesCount() > 0 ||
                                _pinPurchasedFilter.GetEntitiesCount() > 0;
            if (!shouldRefresh)
            {
                return;
            }

            var isVisible = _phasePool.Get(runEntity).Value == Enums.PhaseType.FieldUpgradePhase;
            _controller.Show(isVisible);
            if (!isVisible)
            {
                return;
            }

            ref var state = ref _fieldUpgradeStatePool.GetOrAdd(runEntity);
            var installedPinsByGlobalIndex = new Dictionary<int, InstalledPinComponent>();
            foreach (var installedPinEntity in _installedPinFilter)
            {
                var installedPin = _installedPinPool.Get(installedPinEntity);
                installedPinsByGlobalIndex[installedPin.GlobalIndex] = installedPin;
            }

            var boughtPinTypeId = string.Empty;
            foreach (var candidateEntity in _candidateFilter)
            {
                boughtPinTypeId = _candidatePool.Get(candidateEntity).PinTypeId;
                break;
            }

            var rerollPrice = _gameSettingsService.GetPinShopRerollPrice();
            var currentGold = _goldPool.Get(runEntity).Value;
            var viewData = new FieldUpgradePhaseViewData
            {
                Gold = currentGold,
                RerollCount = state.RerollCount,
                RerollPrice = rerollPrice,
                CanReroll = currentGold >= rerollPrice,
                HasBoughtPinCandidate = _candidateFilter.GetEntitiesCount() > 0,
                BoughtPinTypeId = boughtPinTypeId,
                SelectedSlotIndex = state.SelectedSlotIndex,
                CanReplace = _candidateFilter.GetEntitiesCount() > 0 && state.SelectedSlotIndex >= 0,
                Offers = new List<PinOfferViewData>(),
                Slots = new List<BoardSlotViewData>()
            };

            foreach (var offerEntity in _pinOfferFilter)
            {
                var pinTypeId = _pinOfferTypePool.Get(offerEntity).Value;
                var pin = _pinConfigService.GetPin(pinTypeId);
                viewData.Offers.Add(new PinOfferViewData
                {
                    OfferId = _pinOfferPool.Get(offerEntity).OfferId,
                    PinTypeId = pinTypeId,
                    DisplayName = pin != null ? pin.DisplayName : pinTypeId,
                    Price = _pricePool.Get(offerEntity).Value
                });
            }

            var rows = _gameSettingsService.GetPlinkoBoardRows();
            var globalIndex = 0;
            for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                var row = rows[rowIndex];
                if (row == null || row.Cells == null)
                {
                    continue;
                }

                for (var columnIndex = 0; columnIndex < row.Cells.Count; columnIndex++)
                {
                    installedPinsByGlobalIndex.TryGetValue(globalIndex, out var installedPin);
                    var pinTypeId = installedPin.PinTypeId;
                    var pin = !string.IsNullOrWhiteSpace(pinTypeId) ? _pinConfigService.GetPin(pinTypeId) : null;
                    viewData.Slots.Add(new BoardSlotViewData
                    {
                        GlobalIndex = globalIndex,
                        RowIndex = rowIndex,
                        ColumnIndex = columnIndex,
                        PinTypeId = pinTypeId,
                        DisplayName = pin != null ? pin.DisplayName : string.Empty,
                        IsSelected = globalIndex == state.SelectedSlotIndex
                    });
                    globalIndex++;
                }
            }

            if (viewData.Slots.Count == 0)
            {
                foreach (var installedPin in installedPinsByGlobalIndex.Values)
                {
                    var pin = !string.IsNullOrWhiteSpace(installedPin.PinTypeId) ? _pinConfigService.GetPin(installedPin.PinTypeId) : null;
                    viewData.Slots.Add(new BoardSlotViewData
                    {
                        GlobalIndex = installedPin.GlobalIndex,
                        RowIndex = installedPin.RowIndex,
                        ColumnIndex = installedPin.ColumnIndex,
                        PinTypeId = installedPin.PinTypeId,
                        DisplayName = pin != null ? pin.DisplayName : string.Empty,
                        IsSelected = installedPin.GlobalIndex == state.SelectedSlotIndex
                    });
                }
            }

            _controller.Refresh(viewData);
        }
    }
}