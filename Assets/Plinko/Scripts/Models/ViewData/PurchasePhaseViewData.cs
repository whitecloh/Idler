using System;
using System.Collections.Generic;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.Data.Visuals;
using UnityEngine;

namespace Plinko.Scripts.Models.ViewData
{
    [Serializable]
    public sealed class PurchasePhaseViewData
    {
        public string LevelKey;
        public string LocationDisplayName;
        public string FieldSignature;
        public float FieldHorizontalSpacing;
        public float FieldVerticalSpacing;
        public int Gold;
        public int RerollCount;
        public int RerollPrice;
        public bool CanReroll;
        public bool CanStartBattle;
        public bool CanAdvance;
        public string PrimaryActionLabel;
        public int ActiveTrainingCount;
        public int PlayerBaseHealth;
        public int PlayerBaseMaxHealth;
        public Sprite NextBattleBackgroundSprite;
        public Sprite PlayerBaseSprite;
        public List<UnitShopOfferViewData> Offers = new();
        public List<PurchaseLevelProgressEntryViewData> Levels = new();
        public List<PurchaseFieldPinViewData> Pins = new();
        public List<PurchaseFieldBasketViewData> Baskets = new();
        public List<PurchaseTrainingRunViewData> ActiveTrainings = new();
        public List<PurchaseTrainingStartedViewData> StartedTrainings = new();
        public List<PurchaseTrainedUnitCardViewData> CompletedTrainings = new();
        public List<PurchaseArmyPreviewUnitViewData> ArmyPreviewUnits = new();
    }

    [Serializable]
    public sealed class PurchaseLevelProgressEntryViewData
    {
        public int LevelIndex;
        public int DisplayNumber;
        public Enums.LevelType LevelType;
        public Sprite ProgressSprite;
        public bool IsCompleted;
        public bool IsCurrent;
        public bool IsUnlocked;
    }

    [Serializable]
    public sealed class PurchaseFieldPinViewData
    {
        public int SlotIndex;
        public int RowIndex;
        public int ColumnIndex;
        public string PinTypeId;
        public string DisplayName;
        public string TooltipText;
        public Sprite Sprite;
        public List<StatDisplayViewData> ModifierLines = new();
    }

    [Serializable]
    public sealed class PurchaseFieldBasketViewData
    {
        public string BasketId;
        public int BasketIndex;
        public string DisplayName;
        public int ManaValue;
        public string TooltipText;
        public Sprite Sprite;
    }

    [Serializable]
    public sealed class PurchaseTrainingNodeViewData
    {
        public int RowIndex;
        public int ColumnIndex;
        public string PinTypeId;
    }

    [Serializable]
    public sealed class PurchaseTrainingRunViewData
    {
        public int RuntimeId;
        public int SourceOfferId;
        public string DisplayName;
        public Sprite TrainingFieldSprite;
        public bool HasStarted;
        public float Elapsed;
        public float Duration;
        public int CurrentNodeIndex;
        public int TotalNodeCount;
        public string FinalBasketId;
        public List<PurchaseTrainingNodeViewData> Nodes = new();
    }

    [Serializable]
    public sealed class PurchaseTrainingStartedViewData
    {
        public int RuntimeId;
        public int SourceOfferId;
    }

    [Serializable]
    public sealed class PurchaseTrainedUnitCardViewData
    {
        public int RuntimeId;
        public string UnitTypeId;
        public string DisplayName;
        public Sprite PortraitSprite;
        public int Attack;
        public int Health;
        public int ManaCost;
        public float MoveSpeed;
        public int AttackRange;
        public float AttackSpeed;
        public int Level;
        public int UpgradeCount;
        public List<StatDisplayViewData> Stats = new();
    }

    [Serializable]
    public sealed class PurchaseArmyPreviewUnitViewData
    {
        public int RuntimeId;
        public string DisplayName;
        public Sprite PortraitSprite;
        public CharacterAnimationSetData BattleAnimations;
    }
}
