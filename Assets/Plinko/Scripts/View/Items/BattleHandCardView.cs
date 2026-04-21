using System;
using System.Collections.Generic;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Tooltips;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Items
{
    public sealed class BattleHandCardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private LayoutElement layoutElement;
        [SerializeField] private Image portraitImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text manaText;
        [SerializeField] private GameObject selectedStateRoot;
        [SerializeField] private RectTransform statsRoot;
        [SerializeField] private UnitStatEntryView statEntryPrefab;
        [SerializeField] private RectTransform tooltipAnchor;

        private Action<BattleHandCardView> _pointerEntered;
        private Action<BattleHandCardView> _pointerExited;
        private Action<BattleHandCardView, PointerEventData> _beginDrag;
        private Action<BattleHandCardView, PointerEventData> _drag;
        private Action<BattleHandCardView, PointerEventData> _endDrag;
        private Action<BattleHandCardView> _clicked;
        private readonly List<UnitStatEntryView> _statViews = new();

        public RectTransform RectTransform => root;
        public CanvasGroup CanvasGroup => canvasGroup;
        public LayoutElement LayoutElement => layoutElement;
        public HandCardViewData ViewData { get; private set; } = new();

        public void Bind(
            Action<BattleHandCardView> pointerEntered,
            Action<BattleHandCardView> pointerExited,
            Action<BattleHandCardView, PointerEventData> beginDrag,
            Action<BattleHandCardView, PointerEventData> drag,
            Action<BattleHandCardView, PointerEventData> endDrag,
            Action<BattleHandCardView> clicked = null)
        {
            _pointerEntered = pointerEntered;
            _pointerExited = pointerExited;
            _beginDrag = beginDrag;
            _drag = drag;
            _endDrag = endDrag;
            _clicked = clicked;
        }

        public void Refresh(HandCardViewData viewData)
        {
            ViewData = viewData;
            portraitImage.sprite = viewData.PortraitSprite;
            portraitImage.enabled = viewData.PortraitSprite != null;
            nameText.text = viewData.DisplayName;
            if (manaText != null)
            {
                manaText.text = viewData.ManaCost.ToString();
            }
            UnitStatSyncUtility.Sync(statsRoot, statEntryPrefab, _statViews, viewData.Stats);

            SetSelected(false);
        }

        public void SetSelected(bool isSelected)
        {
            if (selectedStateRoot != null)
            {
                selectedStateRoot.SetActive(isSelected);
            }
        }

        public void SetDimmed(bool isDimmed)
        {
            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.alpha = isDimmed ? 0.55f : 1f;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            UiTooltipManager.Instance?.ShowUnitCard(this, UnitTooltipViewDataFactory.FromHandCard(ViewData));
            _pointerEntered?.Invoke(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            UiTooltipManager.Instance?.Hide(this);
            _pointerExited?.Invoke(this);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            UiTooltipManager.Instance?.Hide(this);
            _beginDrag?.Invoke(this, eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            _drag?.Invoke(this, eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _endDrag?.Invoke(this, eventData);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _clicked?.Invoke(this);
        }

        private void OnDisable()
        {
            UiTooltipManager.Instance?.Hide(this);
        }
    }
}
