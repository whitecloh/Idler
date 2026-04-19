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
            if (root != null)
            {
                root.SetActive(true);
            }

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

            if (root != null)
            {
                root.SetActive(false);
            }
        }

        public void ShowImmediate()
        {
            if (root != null)
            {
                root.SetActive(true);
            }

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

            if (root != null)
            {
                root.SetActive(false);
            }
        }
    }
}
