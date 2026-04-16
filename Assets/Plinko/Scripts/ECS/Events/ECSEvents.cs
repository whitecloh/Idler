
using Plinko.Scripts.Data.Common;

namespace Plinko.Scripts.ECS.Events
{
    public struct RunStartedEvent { }
    public struct LevelLoadedEvent { public int LevelIndex; public Enums.LevelType LevelType; }
    public struct PhaseChangedEvent { public Enums.PhaseType Value; }
    public struct GoldChangedEvent { public int Value; }
    public struct RunSavedEvent { }
    public struct OwnedUnitRegisteredEvent { public int RuntimeId; }
    public struct OwnedUnitReplacedEvent { public int RuntimeId; }
    public struct OwnedUnitPoolChangedEvent { }
    public struct PurchasePhaseEnteredEvent { }
    public struct RetrainingPhaseEnteredEvent { }
    public struct FieldUpgradePhaseEnteredEvent { }
    public struct ShopOffersChangedEvent { }
    public struct PinShopOffersChangedEvent { }
    public struct RetrainingSelectionChangedEvent { public int SelectedCount; }
    public struct RetrainingSelectionConfirmedEvent { }
    public struct BoardSlotSelectionChangedEvent { public int SlotIndex; }
    public struct PlinkoBoardChangedEvent { }
    public struct PinPurchasedEvent { public int OfferId; public string PinTypeId; }
    public struct UnitPurchasedEvent { public int OfferId; public int RuntimeId; }
    public struct UnitTrainingStartedEvent { public int RuntimeId; }
    public struct TrainingPlaybackStartedEvent { public int RuntimeId; }
    public struct TrainingCompletedEvent { public int RuntimeId; public bool IsRetraining; }
    public struct HandGeneratedEvent { }
    public struct HandClearedEvent { }
    public struct UnitDeployedEvent { public int OwnedUnitRuntimeId; }
    public struct ManaChangedEvent { public int Value; }
    public struct EnemyWaveSelectedEvent { public int ThresholdPercent; }
    public struct BattleResolvedEvent { }
    public struct BattlePlaybackStartedEvent { }
    public struct BattlePlaybackCompletedEvent { }
    public struct TurnCompletedEvent { }
    public struct LevelCompletedEvent { }
    public struct RunCompletedEvent { }
    public struct RunFailedEvent { }
}