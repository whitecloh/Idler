using UnityEngine;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Layouts
{
    public sealed class BattleHandFanLayoutGroup : LayoutGroup
    {
        [SerializeField] private float spacing = 180f;
        [SerializeField] private float arcHeight = 28f;
        [SerializeField] private float maxRotation = 10f;

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
        }

        public override void CalculateLayoutInputVertical()
        {
        }

        public override void SetLayoutHorizontal()
        {
            LayoutChildren();
        }

        public override void SetLayoutVertical()
        {
            LayoutChildren();
        }

        private void LayoutChildren()
        {
            var count = rectChildren.Count;
            if (count == 0)
            {
                return;
            }

            var totalWidth = spacing * Mathf.Max(0, count - 1);
            var startX = -totalWidth * 0.5f;

            for (var index = 0; index < count; index++)
            {
                var child = rectChildren[index];
                var t = count == 1 ? 0f : index / (float)(count - 1);
                var centeredT = t - 0.5f;
                var x = startX + spacing * index;
                var y = -Mathf.Abs(centeredT) * arcHeight;
                child.anchorMin = new Vector2(0.5f, 0.5f);
                child.anchorMax = new Vector2(0.5f, 0.5f);
                child.pivot = new Vector2(0.5f, 0.5f);
                child.anchoredPosition = new Vector2(x, y);
                child.localRotation = Quaternion.Euler(0f, 0f, -centeredT * maxRotation * 2f);
            }
        }
    }
}
