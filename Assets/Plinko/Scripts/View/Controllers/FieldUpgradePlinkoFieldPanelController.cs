using System.Collections.Generic;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Bridges;
using Plinko.Scripts.View.Items;
using UnityEngine;

namespace Plinko.Scripts.View.Controllers
{
    public sealed class FieldUpgradePlinkoFieldPanelController : MonoBehaviour
    {
        [SerializeField] private RectTransform pinsRoot;
        [SerializeField] private FieldUpgradeBoardPinView pinPrefab;
        [SerializeField] private RectTransform basketsRoot;
        [SerializeField] private PurchaseTrainingBasketView basketPrefab;
        [SerializeField] private float pixelsPerFieldUnit = 120f;

        private readonly List<FieldUpgradeBoardPinView> _pinViews = new();
        private readonly List<PurchaseTrainingBasketView> _basketViews = new();
        private string _fieldSignature = string.Empty;
        private float _horizontalSpacing = 1f;
        private float _verticalSpacing = 1f;
        private FieldUpgradeBridge _fieldUpgradeBridge;

        public void Init(FieldUpgradeBridge fieldUpgradeBridge)
        {
            _fieldUpgradeBridge = fieldUpgradeBridge;
        }

        public void ResetState()
        {
            _fieldSignature = string.Empty;
            ClearFieldViews();
        }

        public void Refresh(FieldUpgradePhaseViewData viewData)
        {
            if (_fieldSignature != viewData.FieldSignature)
            {
                _fieldSignature = viewData.FieldSignature;
                _horizontalSpacing = viewData.FieldHorizontalSpacing;
                _verticalSpacing = viewData.FieldVerticalSpacing;
                RebuildField(viewData);
            }

            ApplyField(viewData);
        }

        private void RebuildField(FieldUpgradePhaseViewData viewData)
        {
            ClearFieldViews();

            var rowCounts = BuildRowCounts(viewData.Slots);
            for (var index = 0; index < viewData.Slots.Count; index++)
            {
                var slot = viewData.Slots[index];
                var pinView = Instantiate(pinPrefab, pinsRoot);
                pinView.Bind(slot.SlotIndex, HandlePinSelected);
                pinView.RectTransform.anchoredPosition = BuildPinPosition(slot.RowIndex, slot.ColumnIndex, rowCounts);
                _pinViews.Add(pinView);
            }

            for (var index = 0; index < viewData.Baskets.Count; index++)
            {
                var basket = viewData.Baskets[index];
                var basketView = Instantiate(basketPrefab, basketsRoot);
                basketView.RectTransform.anchoredPosition = BuildBasketPosition(basket.BasketIndex, viewData.Baskets.Count);
                _basketViews.Add(basketView);
            }

            ApplyField(viewData);
        }

        private void ApplyField(FieldUpgradePhaseViewData viewData)
        {
            for (var index = 0; index < _pinViews.Count && index < viewData.Slots.Count; index++)
            {
                _pinViews[index].Refresh(viewData.Slots[index]);
            }

            for (var index = 0; index < _basketViews.Count && index < viewData.Baskets.Count; index++)
            {
                _basketViews[index].Refresh(viewData.Baskets[index]);
            }
        }

        private void HandlePinSelected(int slotIndex)
        {
            _fieldUpgradeBridge.RequestSelectBoardSlot(slotIndex);
        }

        private Vector2 BuildPinPosition(int rowIndex, int columnIndex, IReadOnlyDictionary<int, int> rowCounts)
        {
            var rowCount = rowCounts.TryGetValue(rowIndex, out var count) ? count : 1;
            var x = (columnIndex - (rowCount - 1) * 0.5f) * _horizontalSpacing * pixelsPerFieldUnit;
            var y = -rowIndex * _verticalSpacing * pixelsPerFieldUnit;
            return new Vector2(x, y);
        }

        private Vector2 BuildBasketPosition(int basketIndex, int totalBasketCount)
        {
            var x = (basketIndex - (totalBasketCount - 1) * 0.5f) * _horizontalSpacing * pixelsPerFieldUnit;
            return new Vector2(x, 0f);
        }

        private static Dictionary<int, int> BuildRowCounts(IReadOnlyList<BoardSlotViewData> slots)
        {
            var result = new Dictionary<int, int>();
            for (var index = 0; index < slots.Count; index++)
            {
                var slot = slots[index];
                if (!result.TryGetValue(slot.RowIndex, out var rowCount) || rowCount < slot.ColumnIndex + 1)
                {
                    result[slot.RowIndex] = slot.ColumnIndex + 1;
                }
            }

            return result;
        }

        private void ClearFieldViews()
        {
            for (var index = 0; index < _pinViews.Count; index++)
            {
                Destroy(_pinViews[index].gameObject);
            }

            for (var index = 0; index < _basketViews.Count; index++)
            {
                Destroy(_basketViews[index].gameObject);
            }

            _pinViews.Clear();
            _basketViews.Clear();
        }
    }
}
