using Plinko.Scripts.Models.ViewData;

namespace Plinko.Scripts.View.Tooltips
{
    public static class UnitTooltipViewDataFactory
    {
        public static UnitTooltipViewData FromHandCard(HandCardViewData viewData)
        {
            return Build(viewData != null ? viewData.DisplayName : string.Empty, viewData != null ? viewData.PortraitSprite : null, viewData != null ? viewData.ManaCost : 0, viewData != null ? viewData.Stats : null);
        }

        public static UnitTooltipViewData FromBattleDeck(BattleDeckUnitViewData viewData)
        {
            return Build(viewData != null ? viewData.DisplayName : string.Empty, viewData != null ? viewData.PortraitSprite : null, viewData != null ? viewData.ManaCost : 0, viewData != null ? viewData.Stats : null);
        }

        public static UnitTooltipViewData FromShopOffer(UnitShopOfferViewData viewData)
        {
            return Build(viewData != null ? viewData.DisplayName : string.Empty, viewData != null ? viewData.PortraitSprite : null, viewData != null ? viewData.ManaCost : 0, viewData != null ? viewData.Stats : null);
        }

        public static UnitTooltipViewData FromPurchaseUnit(PurchaseTrainedUnitCardViewData viewData)
        {
            return Build(viewData != null ? viewData.DisplayName : string.Empty, viewData != null ? viewData.PortraitSprite : null, viewData != null ? viewData.ManaCost : 0, viewData != null ? viewData.Stats : null);
        }

        public static UnitTooltipViewData FromSignalPendingUnit(SignalPurchasePendingUnitCardViewData viewData)
        {
            return Build(viewData != null ? viewData.DisplayName : string.Empty, viewData != null ? viewData.PortraitSprite : null, viewData != null ? viewData.ManaCost : 0, viewData != null ? viewData.Stats : null);
        }

        public static UnitTooltipViewData FromRetrainingOffer(RetrainingOfferViewData viewData)
        {
            return Build(viewData != null ? viewData.DisplayName : string.Empty, viewData != null ? viewData.PortraitSprite : null, viewData != null ? viewData.ManaCost : 0, viewData != null ? viewData.Stats : null);
        }

        private static UnitTooltipViewData Build(
            string displayName,
            UnityEngine.Sprite portraitSprite,
            int manaCost,
            System.Collections.Generic.IReadOnlyList<StatDisplayViewData> stats)
        {
            var result = new UnitTooltipViewData
            {
                DisplayName = displayName,
                PortraitSprite = portraitSprite,
                ManaCost = manaCost
            };

            if (stats != null)
            {
                for (var index = 0; index < stats.Count; index++)
                {
                    var stat = stats[index];
                    if (stat == null)
                    {
                        continue;
                    }

                    result.Stats.Add(new StatDisplayViewData
                    {
                        StatTypeId = stat.StatTypeId,
                        DisplayName = stat.DisplayName,
                        Icon = stat.Icon,
                        ValueText = stat.ValueText
                    });
                }
            }

            return result;
        }
    }
}
