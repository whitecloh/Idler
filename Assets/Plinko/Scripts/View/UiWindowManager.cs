using System.Collections;
using Plinko.Scripts.View.Controllers;
using UnityEngine;

namespace Plinko.Scripts.View
{
    public sealed class UiWindowManager : MonoBehaviour
    {
        public enum WindowId
        {
            None = 0,
            MainMenu = 1,
            Purchase = 2,
            Retraining = 3,
            FieldUpgrade = 4,
            Battle = 5,
            BattleResult = 6
        }

        [SerializeField] private UiLoadingWindow loadingWindow;
        [SerializeField] private float transitionDuration = 1f;

        private MainMenuScreenController mainMenuWindow;
        private PurchasePhaseScreenController purchaseWindow;
        private RetrainingPhaseScreenController retrainingWindow;
        private FieldUpgradePhaseScreenController fieldUpgradeWindow;
        private BattleScreenController battleWindow;
        private BattleResultScreenController battleResultWindow;
        private WindowId currentWindow = WindowId.None;
        private Coroutine transitionRoutine;

        public void Configure(
            MainMenuScreenController mainMenu,
            PurchasePhaseScreenController purchase,
            RetrainingPhaseScreenController retraining,
            FieldUpgradePhaseScreenController fieldUpgrade,
            BattleScreenController battle,
            BattleResultScreenController battleResult)
        {
            mainMenuWindow = mainMenu;
            purchaseWindow = purchase;
            retrainingWindow = retraining;
            fieldUpgradeWindow = fieldUpgrade;
            battleWindow = battle;
            battleResultWindow = battleResult;
        }

        public void ShowImmediate(WindowId targetWindow)
        {
            if (transitionRoutine != null)
            {
                StopCoroutine(transitionRoutine);
                transitionRoutine = null;
            }

            if (loadingWindow != null)
            {
                loadingWindow.HideImmediate();
            }

            currentWindow = targetWindow;
            ApplyWindowState(targetWindow, true);
        }

        public void Show(WindowId targetWindow)
        {
            if (currentWindow == targetWindow)
            {
                return;
            }

            if (transitionRoutine != null)
            {
                StopCoroutine(transitionRoutine);
                transitionRoutine = null;
            }

            if (!isActiveAndEnabled || loadingWindow == null || currentWindow == WindowId.None)
            {
                currentWindow = targetWindow;
                ApplyWindowState(targetWindow, false);
                return;
            }

            transitionRoutine = StartCoroutine(PlayTransition(targetWindow));
        }

        private IEnumerator PlayTransition(WindowId targetWindow)
        {
            loadingWindow.Show();
            currentWindow = targetWindow;
            ApplyWindowState(targetWindow, false);
            yield return new WaitForSecondsRealtime(transitionDuration);
            loadingWindow.Hide();
            transitionRoutine = null;
        }

        private void ApplyWindowState(WindowId targetWindow, bool immediate)
        {
            SetVisible(mainMenuWindow, targetWindow == WindowId.MainMenu, immediate);
            SetVisible(purchaseWindow, targetWindow == WindowId.Purchase, immediate);
            SetVisible(retrainingWindow, targetWindow == WindowId.Retraining, immediate);
            SetVisible(fieldUpgradeWindow, targetWindow == WindowId.FieldUpgrade, immediate);
            SetVisible(battleWindow, targetWindow == WindowId.Battle, immediate);
            SetVisible(battleResultWindow, targetWindow == WindowId.BattleResult, immediate);
        }

        private static void SetVisible(MainMenuScreenController controller, bool isVisible, bool immediate)
        {
            if (controller == null)
            {
                return;
            }

            if (immediate)
            {
                controller.SetVisibleImmediate(isVisible);
                return;
            }

            controller.Show(isVisible);
        }

        private static void SetVisible(PurchasePhaseScreenController controller, bool isVisible, bool immediate)
        {
            if (controller == null)
            {
                return;
            }

            if (immediate)
            {
                controller.SetVisibleImmediate(isVisible);
                return;
            }

            controller.Show(isVisible);
        }

        private static void SetVisible(RetrainingPhaseScreenController controller, bool isVisible, bool immediate)
        {
            if (controller == null)
            {
                return;
            }

            if (immediate)
            {
                controller.SetVisibleImmediate(isVisible);
                return;
            }

            controller.Show(isVisible);
        }

        private static void SetVisible(FieldUpgradePhaseScreenController controller, bool isVisible, bool immediate)
        {
            if (controller == null)
            {
                return;
            }

            if (immediate)
            {
                controller.SetVisibleImmediate(isVisible);
                return;
            }

            controller.Show(isVisible);
        }

        private static void SetVisible(BattleScreenController controller, bool isVisible, bool immediate)
        {
            if (controller == null)
            {
                return;
            }

            if (immediate)
            {
                controller.SetVisibleImmediate(isVisible);
                return;
            }

            controller.Show(isVisible);
        }

        private static void SetVisible(BattleResultScreenController controller, bool isVisible, bool immediate)
        {
            if (controller == null)
            {
                return;
            }

            if (immediate)
            {
                controller.SetVisibleImmediate(isVisible);
                return;
            }

            controller.Show(isVisible);
        }
    }
}
