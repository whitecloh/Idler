using System.Collections.Generic;
using Plinko.Scripts.Models;

namespace Plinko.Scripts.ECS.Requests
{
    public struct StartNewRunRequest { public string LocationId; }
    public struct ContinueRunRequest { }
    public struct StartLevelRequest { public int LevelIndex; }
    public struct AdvanceToNextLevelRequest { }
    public struct RestoreOwnedUnitsRequest
    {
        public List<OwnedUnitSaveDto> OwnedUnits;
    }
    public struct RestoreBoardStateRequest
    {
        public PlinkoBoardSaveDto Board;
    }

    public struct RegisterOwnedUnitRequest
    {
        public int RuntimeId;
        public string DisplayName;
        public int Level;
        public string UnitTypeId;
        public int Attack;
        public int Health;
        public int ManaCost;
        public string PassiveAbilityId;
        public int UpgradeCount;
    }

    public struct ReplaceOwnedUnitRequest
    {
        public int RuntimeId;
        public string DisplayName;
        public int Level;
        public string UnitTypeId;
        public int Attack;
        public int Health;
        public int ManaCost;
        public string PassiveAbilityId;
        public int UpgradeCount;
    }
    
    public struct SaveRunRequest { }
    public struct GenerateUnitShopOffersRequest { public int OfferCount; }
    public struct RerollUnitShopRequest { }
    public struct BuyUnitRequest { public int OfferId; }
    public struct GenerateRetrainingShopOffersRequest { public int OfferCount; }
    public struct RerollRetrainingShopRequest { }
    public struct BuyRetrainingBatchRequest { }
    public struct GeneratePinShopOffersRequest { public int OfferCount; }
    public struct RerollPinShopRequest { }
    public struct BuyPinRequest { public int OfferId; }
    public struct SelectBoardSlotRequest { public int SlotIndex; }
    public struct ReplaceBoardPinRequest { }
    public struct GenerateHandRequest { }
    public struct BeginBattleTurnRequest { }
    public struct ClearHandRequest { }
    public struct DeployCardRequest { public int HandCardRuntimeId; }
    public struct StartBattleRequest { }
    public struct StartBattlePlaybackRequest { }
    public struct ReturnToMenuRequest { }
}
