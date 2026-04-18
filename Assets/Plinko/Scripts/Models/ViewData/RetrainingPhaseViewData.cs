using System;
using System.Collections.Generic;
using UnityEngine;

namespace Plinko.Scripts.Models.ViewData
{
    [Serializable]
    public sealed class RetrainingPhaseViewData
    {
        public string LevelKey;
        public string LocationDisplayName;
        public string FieldSignature;
        public float FieldHorizontalSpacing;
        public float FieldVerticalSpacing;
        public int OfferCount;
        public int EligibleCount;
        public int CurrentOfferCount;
        public int CurrentGold;
        public int BatchPrice;
        public int RerollCount;
        public int RerollPrice;
        public bool CanBuyBatch;
        public bool CanReroll;
        public bool CanAdvance;
        public bool IsInteractionLocked;
        public int ActiveTrainingCount;
        public int PlayerBaseHealth;
        public int PlayerBaseMaxHealth;
        public string PrimaryActionLabel;
        public Sprite NextBattleBackgroundSprite;
        public Sprite PlayerBaseSprite;
        public List<RetrainingOfferViewData> Offers = new();
        public List<PurchaseLevelProgressEntryViewData> Levels = new();
        public List<PurchaseFieldPinViewData> Pins = new();
        public List<PurchaseFieldBasketViewData> Baskets = new();
        public List<PurchaseTrainingRunViewData> ActiveTrainings = new();
        public List<PurchaseTrainingStartedViewData> StartedTrainings = new();
        public List<PurchaseTrainedUnitCardViewData> CompletedTrainings = new();
        public List<PurchaseArmyPreviewUnitViewData> AllOwnedArmyPreviewUnits = new();
        public List<PurchaseArmyPreviewUnitViewData> PendingArmyPreviewUnits = new();
        public List<PurchaseArmyPreviewUnitViewData> RetrainedArmyPreviewUnits = new();
    }
}
