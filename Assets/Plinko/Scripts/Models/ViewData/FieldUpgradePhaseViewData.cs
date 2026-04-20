using System;
using System.Collections.Generic;
using UnityEngine;

namespace Plinko.Scripts.Models.ViewData
{
    [Serializable]
    public sealed class FieldUpgradePhaseViewData
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
        public bool HasPendingPin;
        public bool IsSelectionOverlayActive;
        public int SelectedSlotIndex;
        public bool CanReplace;
        public bool CanCancelSelection;
        public bool CanAdvance;
        public string PrimaryActionLabel;
        public int PlayerBaseHealth;
        public int PlayerBaseMaxHealth;
        public Sprite NextBattleBackgroundSprite;
        public Sprite PlayerBaseSprite;
        public FieldUpgradeSelectedPinViewData PendingPin;
        public FieldUpgradeSelectedPinViewData SelectedPin;
        public List<PinOfferViewData> Offers = new();
        public List<FieldUpgradeStartedPurchaseViewData> StartedPurchases = new();
        public List<BoardSlotViewData> Slots = new();
        public List<PurchaseFieldBasketViewData> Baskets = new();
        public List<PurchaseLevelProgressEntryViewData> Levels = new();
        public List<PurchaseArmyPreviewUnitViewData> ArmyPreviewUnits = new();
    }

    [Serializable]
    public sealed class FieldUpgradeStartedPurchaseViewData
    {
        public int OfferId;
        public string PinTypeId;
    }

    [Serializable]
    public sealed class FieldUpgradeSelectedPinViewData
    {
        public string PinTypeId;
        public string DisplayName;
        public Sprite Sprite;
        public List<StatDisplayViewData> ModifierLines = new();
    }
}
