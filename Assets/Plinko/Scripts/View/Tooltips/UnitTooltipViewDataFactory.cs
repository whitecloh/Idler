using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.Services;

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

        public static UnitTooltipViewData FromPowerLineEnemy(PowerLineUnitViewData viewData, StatTypeConfigService statTypeConfigService)
        {
            return Build(
                viewData != null ? viewData.DisplayName : string.Empty,
                viewData != null ? viewData.PortraitSprite : null,
                viewData != null ? viewData.ManaCost : 0,
                BuildPowerLineStats(viewData, statTypeConfigService));
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

        private static System.Collections.Generic.IReadOnlyList<StatDisplayViewData> BuildPowerLineStats(
            PowerLineUnitViewData viewData,
            StatTypeConfigService statTypeConfigService)
        {
            if (viewData == null)
            {
                return null;
            }

            return new System.Collections.Generic.List<StatDisplayViewData>
            {
                new()
                {
                    StatTypeId = Data.Common.StatTypeIds.Attack,
                    DisplayName = statTypeConfigService?.GetStat(Data.Common.StatTypeIds.Attack)?.DisplayName ?? "ATK",
                    Icon = statTypeConfigService?.GetStat(Data.Common.StatTypeIds.Attack)?.Icon,
                    ValueText = viewData.Attack.ToString()
                },
                new()
                {
                    StatTypeId = Data.Common.StatTypeIds.Health,
                    DisplayName = statTypeConfigService?.GetStat(Data.Common.StatTypeIds.Health)?.DisplayName ?? "HP",
                    Icon = statTypeConfigService?.GetStat(Data.Common.StatTypeIds.Health)?.Icon,
                    ValueText = viewData.Health.ToString()
                },
                new()
                {
                    StatTypeId = Data.Common.StatTypeIds.MoveSpeed,
                    DisplayName = statTypeConfigService?.GetStat(Data.Common.StatTypeIds.MoveSpeed)?.DisplayName ?? "Move",
                    Icon = statTypeConfigService?.GetStat(Data.Common.StatTypeIds.MoveSpeed)?.Icon,
                    ValueText = viewData.MoveSpeed.ToString("0.##")
                },
                new()
                {
                    StatTypeId = Data.Common.StatTypeIds.AttackRange,
                    DisplayName = statTypeConfigService?.GetStat(Data.Common.StatTypeIds.AttackRange)?.DisplayName ?? "Range",
                    Icon = statTypeConfigService?.GetStat(Data.Common.StatTypeIds.AttackRange)?.Icon,
                    ValueText = viewData.AttackRange.ToString()
                },
                new()
                {
                    StatTypeId = Data.Common.StatTypeIds.AttackSpeed,
                    DisplayName = statTypeConfigService?.GetStat(Data.Common.StatTypeIds.AttackSpeed)?.DisplayName ?? "ASPD",
                    Icon = statTypeConfigService?.GetStat(Data.Common.StatTypeIds.AttackSpeed)?.Icon,
                    ValueText = viewData.AttackSpeed.ToString("0.##")
                }
            };
        }
    }
}
