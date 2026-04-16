using Plinko.Scripts.Data.Common;

namespace Plinko.Scripts.ECS.Components
{
    public struct RunComponent { }
    public struct CurrentLocationComponent { public string LocationId; }
    public struct CurrentLevelComponent { public int LevelIndex; }
    public struct CurrentLevelTypeComponent { public Enums.LevelType Value; }
    public struct CurrentPhaseComponent { public Enums.PhaseType Value; }
    public struct CurrentGoldComponent { public int Value; }
    public struct PlayerBaseHealthComponent { public int Value; public int MaxValue; }
    public struct EnemyBaseHealthComponent { public int Value; public int MaxValue; }
    public struct RunStatusComponent { public Enums.RunStatus Value; }
    public struct CurrentManaComponent { public int Value; }

    public struct PurchasePhaseStateComponent
    {
        public int RerollCount;
        public int ActiveTrainingCount;
        public bool CanEnterBattle;
    }

    public struct RetrainingPhaseStateComponent
    {
        public int SelectedCount;
        public int SelectionLimit;
        public bool IsSelectionLocked;
        public int ActiveTrainingCount;
    }

    public struct FieldUpgradePhaseStateComponent
    {
        public int RerollCount;
        public int SelectedSlotIndex;
        public bool IsPlacementHighlighted;
    }

    public struct BattleStateComponent
    {
        public int CurrentTurn;
        public bool IsResolved;
    }

    public struct CurrentEnemyWaveComponent
    {
        public int ThresholdPercent;
        public int EnemyCount;
        public int TotalAttack;
        public int TotalHealth;
    }
    
    public struct OwnedUnitComponent { public int RuntimeId; }
    public struct UnitTypeIdComponent { public string Value; }
    public struct UnitStatsComponent { public int Attack; public int Health; }
    public struct UnitManaCostComponent { public int Value; }
    public struct UnitDisplayNameComponent { public string Value; }
    public struct UnitLevelComponent { public int Value; }
    public struct PassiveAbilityIdComponent { public string Value; }
    public struct UpgradeCountComponent { public int Value; }
    public struct SelectedForRetrainingComponent { }

    public struct InstalledPinComponent
    {
        public int SlotIndex;
        public int RowIndex;
        public int ColumnIndex;
        public string PinTypeId;
    }

    public struct PendingPurchasedPinComponent
    {
        public string PinTypeId;
        public int OfferId;
    }

    public struct HandCardComponent { public int HandCardRuntimeId; }
    public struct HandCardOwnerUnitComponent { public int OwnedUnitRuntimeId; }
    public struct DeployedForTurnComponent { }

    public struct HandStateComponent
    {
        public int CardCount;
        public int NextRuntimeId;
    }

    public struct UnitShopOfferComponent { public int OfferId; }
    public struct PinShopOfferComponent { public int OfferId; }
    public struct OfferPriceComponent { public int Value; }
    public struct ShopOfferUnitTypeIdComponent { public string Value; }
    public struct ShopOfferPinTypeIdComponent { public string Value; }

    public struct StagedTraineeComponent
    {
        public int RuntimeId;
        public bool IsRetraining;
        public int SourceOfferId;
    }
    
    public struct PlinkoTrainingPlaybackComponent
    {
        public int RuntimeId;
        public bool IsRetraining;
        public float Duration;
        public float Elapsed;
        public int CurrentNodeIndex;
        public int TotalNodeCount;
        public bool IsCompleted;
    }
}