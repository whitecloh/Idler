namespace Plinko.Scripts.Data.Common
{
    public static class Enums
    {
        public enum LevelType
        {
            None = 0,
            Purchase = 1,
            Upgrade = 2,
            FieldUpgrade = 3
        }

        public enum PhaseType
        {
            None = 0,
            MainMenu = 1,
            Location = 2,
            PurchasePhase = 3,
            UpgradePhase = 4,
            FieldUpgradePhase = 5,
            Battle = 6,
            BattlePlayback = 7,
            Result = 8
        }

        public enum RunStatus
        {
            None = 0,
            InProgress = 1,
            Victory = 2,
            Defeat = 3
        }
    }
}