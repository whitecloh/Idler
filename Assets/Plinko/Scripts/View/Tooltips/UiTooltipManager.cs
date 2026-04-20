using Plinko.Scripts.View.Animations;
using UnityEngine;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Tooltips
{
    public sealed class UiTooltipManager : MonoBehaviour
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Canvas tooltipCanvas;
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private UiTooltipTextView textTooltipView;
        [SerializeField] private UiTooltipUnitCardView unitCardTooltipView;
        [SerializeField] private float screenPadding = 16f;

        private Object _currentOwner;
        private RectTransform _currentTarget;
        private UiTooltipPlacement _currentPlacement;
        private Vector2 _currentOffset;
        private RectTransform _currentTooltipRect;

        public static UiTooltipManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            if (tooltipCanvas != null)
            {
                tooltipCanvas.overrideSorting = true;
            }

            HideImmediate();
        }

        private void OnDestroy()
        {
            if (ReferenceEquals(Instance, this))
            {
                Instance = null;
            }
        }

        private void LateUpdate()
        {
            if (_currentTarget == null || _currentTooltipRect == null || !root.gameObject.activeSelf)
            {
                return;
            }

            PositionCurrentTooltip();
        }

        public void ShowText(Object owner, RectTransform target, string text, UiTooltipPlacement placement = UiTooltipPlacement.Top, Vector2? offset = null)
        {
            if (target == null || string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            EnsureVisible();
            _currentOwner = owner;
            _currentTarget = target;
            _currentPlacement = placement;
            _currentOffset = offset ?? new Vector2(0f, 12f);

            if (textTooltipView != null)
            {
                textTooltipView.gameObject.SetActive(true);
                textTooltipView.Refresh(text);
                _currentTooltipRect = textTooltipView.RectTransform;
            }

            if (unitCardTooltipView != null)
            {
                unitCardTooltipView.gameObject.SetActive(false);
            }

            PositionCurrentTooltip();
        }

        public void ShowUnitCard(Object owner, RectTransform target, Models.ViewData.UnitTooltipViewData viewData, UiTooltipPlacement placement = UiTooltipPlacement.Top, Vector2? offset = null)
        {
            if (target == null || viewData == null)
            {
                return;
            }

            EnsureVisible();
            _currentOwner = owner;
            _currentTarget = target;
            _currentPlacement = placement;
            _currentOffset = offset ?? new Vector2(0f, 14f);

            if (unitCardTooltipView != null)
            {
                unitCardTooltipView.gameObject.SetActive(true);
                unitCardTooltipView.Refresh(viewData);
                _currentTooltipRect = unitCardTooltipView.RectTransform;
            }

            if (textTooltipView != null)
            {
                textTooltipView.gameObject.SetActive(false);
            }

            PositionCurrentTooltip();
        }

        public void Hide(Object owner)
        {
            if (_currentOwner != null && owner != _currentOwner)
            {
                return;
            }

            HideImmediate();
        }

        public void HideImmediate()
        {
            _currentOwner = null;
            _currentTarget = null;
            _currentTooltipRect = null;

            if (textTooltipView != null)
            {
                textTooltipView.gameObject.SetActive(false);
            }

            if (unitCardTooltipView != null)
            {
                unitCardTooltipView.gameObject.SetActive(false);
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }

            if (root != null)
            {
                root.gameObject.SetActive(false);
            }
        }

        private void EnsureVisible()
        {
            if (root == null)
            {
                return;
            }

            root.gameObject.SetActive(true);
            root.SetAsLastSibling();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
        }

        private void PositionCurrentTooltip()
        {
            if (_currentTarget == null || _currentTooltipRect == null || contentRoot == null)
            {
                return;
            }

            Canvas.ForceUpdateCanvases();

            SetPivotForPlacement(_currentTooltipRect, _currentPlacement);

            var anchorWorld = GetAnchorWorldPosition(_currentTarget, _currentPlacement);
            var anchoredPosition = UiRectTransformUtility.WorldToAnchoredPositionOverlay(contentRoot, null, anchorWorld);
            _currentTooltipRect.anchoredPosition = anchoredPosition + _currentOffset;

            Canvas.ForceUpdateCanvases();
            ClampToBounds(_currentTooltipRect, contentRoot.rect, screenPadding);
        }

        private static Vector3 GetAnchorWorldPosition(RectTransform target, UiTooltipPlacement placement)
        {
            var corners = new Vector3[4];
            target.GetWorldCorners(corners);
            return placement switch
            {
                UiTooltipPlacement.Bottom => (corners[0] + corners[3]) * 0.5f,
                UiTooltipPlacement.Left => (corners[0] + corners[1]) * 0.5f,
                UiTooltipPlacement.Right => (corners[2] + corners[3]) * 0.5f,
                _ => (corners[1] + corners[2]) * 0.5f
            };
        }

        private static void SetPivotForPlacement(RectTransform tooltipRect, UiTooltipPlacement placement)
        {
            tooltipRect.pivot = placement switch
            {
                UiTooltipPlacement.Bottom => new Vector2(0.5f, 1f),
                UiTooltipPlacement.Left => new Vector2(1f, 0.5f),
                UiTooltipPlacement.Right => new Vector2(0f, 0.5f),
                _ => new Vector2(0.5f, 0f)
            };
        }

        private static void ClampToBounds(RectTransform tooltipRect, Rect bounds, float padding)
        {
            var size = tooltipRect.rect.size;
            var position = tooltipRect.anchoredPosition;
            var pivot = tooltipRect.pivot;

            var minX = bounds.xMin + padding + size.x * pivot.x;
            var maxX = bounds.xMax - padding - size.x * (1f - pivot.x);
            var minY = bounds.yMin + padding + size.y * pivot.y;
            var maxY = bounds.yMax - padding - size.y * (1f - pivot.y);

            position.x = Mathf.Clamp(position.x, minX, maxX);
            position.y = Mathf.Clamp(position.y, minY, maxY);
            tooltipRect.anchoredPosition = position;
        }
    }
}
