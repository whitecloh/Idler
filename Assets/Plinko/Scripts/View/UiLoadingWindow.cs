using Plinko.Scripts.View.Animations;
using UnityEngine;

namespace Plinko.Scripts.View
{
    public sealed class UiLoadingWindow : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private UiCanvasGroupVisibility visibility;

        public void Show()
        {
            if (visibility != null)
            {
                visibility.ShowAnimated();
                return;
            }

            root.SetActive(true);
        }

        public void Hide()
        {
            if (visibility != null)
            {
                visibility.HideAnimated();
                return;
            }

            root.SetActive(false);
        }

        public void ShowImmediate()
        {
            if (visibility != null)
            {
                visibility.ShowImmediate();
                return;
            }

            root.SetActive(true);
        }

        public void HideImmediate()
        {
            if (visibility != null)
            {
                visibility.HideImmediate();
                return;
            }

            root.SetActive(false);
        }
    }
}
