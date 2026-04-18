using TMPro;
using UnityEngine;

namespace Plinko.Scripts.View.Animations
{
    public sealed class UiTextHighlightFeedback : MonoBehaviour
    {
        [SerializeField] private TMP_Text targetText;
        [SerializeField] private RectTransform punchTarget;
        [SerializeField] private Color flashColor = Color.red;
        [SerializeField] private float flashDuration = 0.32f;

        public void Play()
        {
            UiAnimationManager.Instance.PlayGraphicColorFlash(targetText, "text-highlight", flashColor, flashDuration);
            UiAnimationManager.Instance.PlayPunch(punchTarget);
        }
    }
}
