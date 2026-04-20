
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.Models;

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
    public struct SignalPurchasePhaseEnteredEvent { }
    public struct RetrainingPhaseEnteredEvent { }
    public struct FieldUpgradePhaseEnteredEvent { }
    public struct ShopOffersChangedEvent { }
    public struct PinShopOffersChangedEvent { }
    public struct RetrainingShopOffersChangedEvent { }
    public struct RetrainingBatchPurchasedEvent { }
    public struct BoardSlotSelectionChangedEvent { public int SlotIndex; }
    public struct PlinkoBoardChangedEvent { }
    public struct PinPurchasedEvent { public int OfferId; public string PinTypeId; }
    public struct UnitPurchasedEvent { public int OfferId; public int RuntimeId; }
    public struct SignalUnitPurchasedEvent { public int OfferId; public int RuntimeId; public int SlotIndex; }
    public struct SignalLaunchStartedEvent { public bool WillBreakGenerator; }
    public struct SignalGeneratorBrokenEvent { }
    public struct UnitTrainingStartedEvent { public int RuntimeId; }
    public struct TrainingPlaybackStartedEvent { public int RuntimeId; }
    public struct TrainingCompletedEvent { public int RuntimeId; public bool IsRetraining; }
    public struct HandGeneratedEvent { }
    public struct HandClearedEvent { }
    public struct UnitDeployedEvent { public int OwnedUnitRuntimeId; }
    public struct ManaChangedEvent { public int Value; }
    public struct EnemyWaveSelectedEvent { public int ThresholdPercent; }
    public struct BaseDefenseTurnStartedEvent { public int TurnIndex; }
    public struct PowerLineUnitSpawnedEvent { public int RuntimeId; public bool IsEnemy; public Enums.PowerLineLane Lane; public float Position; }
    public struct PowerLineAttackEvent
    {
        public int AttackerRuntimeId;
        public bool AttackerIsEnemy;
        public bool TargetIsBase;
        public Enums.PowerLineLane Lane;
        public float StartPosition;
        public float TargetPosition;
        public Enums.AttackType AttackType;
        public UnityEngine.Sprite ProjectileSprite;
    }
    public struct PowerLineDamageEvent { public int TargetRuntimeId; public bool TargetIsEnemy; public bool TargetIsBase; public Enums.PowerLineLane Lane; public float Position; public int Amount; }
    public struct PowerLineUnitDiedEvent { public int RuntimeId; public bool IsEnemy; public Enums.PowerLineLane Lane; public float Position; public bool WasCarryingPlug; }
    public struct PowerLinePlugStateChangedEvent { public Enums.PowerLineLane Lane; public PowerLinePlugStatus Status; public float Position; public int CarrierRuntimeId; }
    public struct PowerLineLaneConnectedEvent { public Enums.PowerLineLane Lane; }
    public struct BattleResolvedEvent { }
    public struct BattlePlaybackStartedEvent { }
    public struct BattlePlaybackCompletedEvent { }
    public struct TurnCompletedEvent { }
    public struct LevelCompletedEvent { }
    public struct RunCompletedEvent { }
    public struct RunFailedEvent { }
}
