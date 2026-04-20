using System;
using System.Collections.Generic;
using UnityEngine;

namespace Plinko.Scripts.Models.ViewData
{
    [Serializable]
    public sealed class SignalPurchasePhaseViewData
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
        public bool CanBuyUnits;
        public bool CanLaunchSignal;
        public bool CanAdvance;
        public bool IsGeneratorBroken;
        public bool IsSignalRunning;
        public int PendingUnitCount;
        public int PendingUnitSlotCount;
        public int SignalsLaunchedCount;
        public int GeneratorBreakAfterSignalCount;
        public int PlayerBaseHealth;
        public int PlayerBaseMaxHealth;
        public Sprite NextBattleBackgroundSprite;
        public Sprite PlayerBaseSprite;
        public List<UnitShopOfferViewData> Offers = new();
        public List<PurchaseLevelProgressEntryViewData> Levels = new();
        public List<PurchaseFieldPinViewData> Pins = new();
        public List<PurchaseFieldBasketViewData> Baskets = new();
        public List<PurchaseTrainingRunViewData> ActiveSignals = new();
        public List<PurchaseTrainedUnitCardViewData> CompletedTrainings = new();
        public List<SignalPurchasePendingUnitCardViewData> PendingUnits = new();
        public List<PurchaseArmyPreviewUnitViewData> ArmyPreviewUnits = new();
    }

    [Serializable]
    public sealed class SignalPurchasePendingUnitCardViewData
    {
        public int RuntimeId;
        public int SlotIndex;
        public string UnitTypeId;
        public string DisplayName;
        public Sprite PortraitSprite;
        public int Attack;
        public int Health;
        public int ManaCost;
        public float MoveSpeed;
        public int AttackRange;
        public float AttackSpeed;
    }
}
