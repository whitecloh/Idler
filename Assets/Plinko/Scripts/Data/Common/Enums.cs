namespace Plinko.Scripts.Data.Common
{
    public static class Enums
    {
        public enum LevelType
        {
            None = 0,
            Purchase = 1,
            Retraining = 2,
            FieldUpgrade = 3,
            Battle = 4
        }

        public enum PhaseType
        {
            None = 0,
            MainMenu = 1,
            Location = 2,
            PurchasePhase = 3,
            RetrainingPhase = 4,
            FieldUpgradePhase = 5,
            PlinkoTrainingPlayback = 6,
            BattlePreparation = 7,
            Battle = 8,
            BattlePlayback = 9,
            Result = 10
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
