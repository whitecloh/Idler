using System;
using Plinko.Scripts.Models.ViewData;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Items
{
    public sealed class BattleHandCardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private LayoutElement layoutElement;
        [SerializeField] private Image portraitImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text attackText;
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private TMP_Text manaText;

        private Action<BattleHandCardView> _pointerEntered;
        private Action<BattleHandCardView> _pointerExited;
        private Action<BattleHandCardView, PointerEventData> _beginDrag;
        private Action<BattleHandCardView, PointerEventData> _drag;
        private Action<BattleHandCardView, PointerEventData> _endDrag;

        public RectTransform RectTransform => root;
        public CanvasGroup CanvasGroup => canvasGroup;
        public LayoutElement LayoutElement => layoutElement;
        public HandCardViewData ViewData { get; private set; } = new();

        public void Bind(
            Action<BattleHandCardView> pointerEntered,
            Action<BattleHandCardView> pointerExited,
            Action<BattleHandCardView, PointerEventData> beginDrag,
            Action<BattleHandCardView, PointerEventData> drag,
            Action<BattleHandCardView, PointerEventData> endDrag)
        {
            _pointerEntered = pointerEntered;
            _pointerExited = pointerExited;
            _beginDrag = beginDrag;
            _drag = drag;
            _endDrag = endDrag;
        }

        public void Refresh(HandCardViewData viewData)
        {
            ViewData = viewData;
            portraitImage.sprite = viewData.PortraitSprite;
            portraitImage.enabled = viewData.PortraitSprite != null;
            nameText.text = viewData.DisplayName;
            attackText.text = viewData.Attack.ToString();
            healthText.text = viewData.Health.ToString();
            manaText.text = viewData.ManaCost.ToString();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _pointerEntered?.Invoke(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _pointerExited?.Invoke(this);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
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
    }
}
