using System.Collections.Generic;
using System.Text;
using Plinko.Scripts.Data.Pins;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Items;
using Plinko.Scripts.View.Layouts;
using UnityEngine;

namespace Plinko.Scripts.View.Controllers
{
    [ExecuteAlways]
    public sealed class PlinkoFieldPreviewController : MonoBehaviour
    {
        [SerializeField] private bool previewEnabled = true;
        [SerializeField] private PlinkoFieldSettingsData fieldSettings;
        [SerializeField] private RectTransform pinsRoot;
        [SerializeField] private PurchaseTrainingPinView pinPrefab;
        [SerializeField] private RectTransform basketsRoot;
        [SerializeField] private PurchaseTrainingBasketView basketPrefab;
        [SerializeField] private float pixelsPerFieldUnit = 120f;

        private const string PreviewPinsContainerName = "__PreviewPins";
        private const string PreviewBasketsContainerName = "__PreviewBaskets";

        private RectTransform _previewPinsRoot;
        private RectTransform _previewBasketsRoot;
        private string _lastPreviewSignature = string.Empty;

        private void OnEnable()
        {
            _lastPreviewSignature = string.Empty;
            if (Application.isPlaying)
            {
                ClearPreview();
                return;
            }

            RefreshPreviewIfNeeded(force: true);
        }

        private void OnDisable()
        {
            ClearPreview();
        }

        private void OnValidate()
        {
            _lastPreviewSignature = string.Empty;
            if (!Application.isPlaying)
            {
                RefreshPreviewIfNeeded(force: true);
            }
        }

        private void LateUpdate()
        {
            if (Application.isPlaying)
            {
                if (_previewPinsRoot != null || _previewBasketsRoot != null)
                {
                    ClearPreview();
                }

                return;
            }

            RefreshPreviewIfNeeded(force: false);
        }

        [ContextMenu("Rebuild Preview")]
        private void RebuildPreviewContext()
        {
            _lastPreviewSignature = string.Empty;
            RefreshPreviewIfNeeded(force: true);
        }

        [ContextMenu("Clear Preview")]
        private void ClearPreviewContext()
        {
            _lastPreviewSignature = string.Empty;
            ClearPreview();
        }

        private void RefreshPreviewIfNeeded(bool force)
        {
            if (!previewEnabled || fieldSettings == null || pinsRoot == null || basketsRoot == null || pinPrefab == null || basketPrefab == null)
            {
                ClearPreview();
                return;
            }

            var signature = BuildPreviewSignature();
            if (!force && signature == _lastPreviewSignature)
            {
                return;
            }

            _lastPreviewSignature = signature;
            RebuildPreview();
        }

        private void RebuildPreview()
        {
            EnsurePreviewRoots();
            ClearGeneratedChildren(_previewPinsRoot);
            ClearGeneratedChildren(_previewBasketsRoot);

            var rowCounts = PlinkoFieldLayoutUtility.BuildRowCounts(fieldSettings);

            for (var rowIndex = 0; rowIndex < fieldSettings.Rows.Count; rowIndex++)
            {
                var row = fieldSettings.Rows[rowIndex];
                if (row == null || row.Cells == null)
                {
                    continue;
                }

                for (var columnIndex = 0; columnIndex < row.Cells.Count; columnIndex++)
                {
                    var authoredPin = row.Cells[columnIndex] != null ? row.Cells[columnIndex].PinType : null;
                    var pinView = Instantiate(pinPrefab, _previewPinsRoot);
                    SetDontSaveRecursive(pinView.gameObject);
                    pinView.gameObject.name = $"Preview_Pin_{rowIndex}_{columnIndex}";
                    pinView.RectTransform.anchoredPosition = PlinkoFieldLayoutUtility.BuildPinPosition(
                        rowIndex,
                        columnIndex,
                        rowCounts,
                        fieldSettings.HorizontalSpacing,
                        fieldSettings.VerticalSpacing,
                        pixelsPerFieldUnit);
                    pinView.Refresh(new PurchaseFieldPinViewData
                    {
                        RowIndex = rowIndex,
                        ColumnIndex = columnIndex,
                        PinTypeId = authoredPin != null ? authoredPin.Id : string.Empty,
                        DisplayName = authoredPin != null ? authoredPin.DisplayName : string.Empty,
                        Sprite = authoredPin != null ? authoredPin.FieldSprite : null
                    });
                }
            }

            for (var basketIndex = 0; basketIndex < fieldSettings.Baskets.Count; basketIndex++)
            {
                var basketType = fieldSettings.Baskets[basketIndex];
                if (basketType == null)
                {
                    continue;
                }

                var basketView = Instantiate(basketPrefab, _previewBasketsRoot);
                SetDontSaveRecursive(basketView.gameObject);
                basketView.gameObject.name = $"Preview_Basket_{basketIndex}";
                basketView.RectTransform.anchoredPosition = PlinkoFieldLayoutUtility.BuildBasketPosition(
                    basketIndex,
                    fieldSettings.Baskets.Count,
                    rowCounts,
                    fieldSettings.HorizontalSpacing,
                    fieldSettings.VerticalSpacing,
                    pixelsPerFieldUnit);
                basketView.Refresh(new PurchaseFieldBasketViewData
                {
                    BasketId = basketType.Id,
                    BasketIndex = basketIndex,
                    DisplayName = basketType.DisplayName,
                    ManaValue = basketType.ManaValue,
                    Sprite = basketType.FieldSprite
                });
            }
        }

        private void EnsurePreviewRoots()
        {
            _previewPinsRoot = EnsurePreviewRoot(_previewPinsRoot, pinsRoot, PreviewPinsContainerName);
            _previewBasketsRoot = EnsurePreviewRoot(_previewBasketsRoot, basketsRoot, PreviewBasketsContainerName);
        }

        private static RectTransform EnsurePreviewRoot(RectTransform currentRoot, RectTransform parent, string objectName)
        {
            if (parent == null)
            {
                return null;
            }

            if (currentRoot != null)
            {
                return currentRoot;
            }

            var existing = parent.Find(objectName) as RectTransform;
            if (existing != null)
            {
                return existing;
            }

            var previewRootObject = new GameObject(objectName, typeof(RectTransform));
            SetDontSaveRecursive(previewRootObject);
            var previewRoot = previewRootObject.GetComponent<RectTransform>();
            previewRoot.SetParent(parent, false);
            previewRoot.anchorMin = new Vector2(0.5f, 0.5f);
            previewRoot.anchorMax = new Vector2(0.5f, 0.5f);
            previewRoot.pivot = new Vector2(0.5f, 0.5f);
            previewRoot.anchoredPosition = Vector2.zero;
            previewRoot.localScale = Vector3.one;
            previewRoot.sizeDelta = Vector2.zero;
            return previewRoot;
        }

        private void ClearPreview()
        {
            ClearGeneratedChildren(_previewPinsRoot);
            ClearGeneratedChildren(_previewBasketsRoot);

            DestroyObjectImmediateSafe(_previewPinsRoot);
            DestroyObjectImmediateSafe(_previewBasketsRoot);

            _previewPinsRoot = null;
            _previewBasketsRoot = null;
        }

        private static void ClearGeneratedChildren(RectTransform parent)
        {
            if (parent == null)
            {
                return;
            }

            var children = new List<GameObject>();
            for (var index = 0; index < parent.childCount; index++)
            {
                var child = parent.GetChild(index);
                if (child != null)
                {
                    children.Add(child.gameObject);
                }
            }

            for (var index = 0; index < children.Count; index++)
            {
                DestroyObjectImmediateSafe(children[index]);
            }
        }

        private string BuildPreviewSignature()
        {
            var builder = new StringBuilder();
            builder.Append(previewEnabled).Append('|');
            builder.Append(fieldSettings != null ? fieldSettings.Id : string.Empty).Append('|');
            builder.Append(fieldSettings != null ? fieldSettings.HorizontalSpacing : 0f).Append('|');
            builder.Append(fieldSettings != null ? fieldSettings.VerticalSpacing : 0f).Append('|');
            builder.Append(pixelsPerFieldUnit).Append('|');
            builder.Append(pinPrefab != null ? pinPrefab.gameObject.name : string.Empty).Append('|');
            builder.Append(basketPrefab != null ? basketPrefab.gameObject.name : string.Empty).Append('|');

            if (fieldSettings != null && fieldSettings.Rows != null)
            {
                for (var rowIndex = 0; rowIndex < fieldSettings.Rows.Count; rowIndex++)
                {
                    var row = fieldSettings.Rows[rowIndex];
                    builder.Append("r").Append(rowIndex).Append(':');
                    if (row == null || row.Cells == null)
                    {
                        builder.Append("null|");
                        continue;
                    }

                    builder.Append(row.Cells.Count).Append('|');
                    for (var columnIndex = 0; columnIndex < row.Cells.Count; columnIndex++)
                    {
                        var pin = row.Cells[columnIndex] != null ? row.Cells[columnIndex].PinType : null;
                        builder.Append(pin != null ? pin.Id : string.Empty).Append(':');
                        builder.Append(pin != null ? pin.DisplayName : string.Empty).Append(':');
                        builder.Append(pin != null && pin.FieldSprite != null ? pin.FieldSprite.name : string.Empty).Append('|');
                    }
                }
            }

            if (fieldSettings != null && fieldSettings.Baskets != null)
            {
                for (var index = 0; index < fieldSettings.Baskets.Count; index++)
                {
                    var basket = fieldSettings.Baskets[index];
                    builder.Append("b").Append(index).Append(':');
                    builder.Append(basket != null ? basket.Id : string.Empty).Append(':');
                    builder.Append(basket != null ? basket.DisplayName : string.Empty).Append(':');
                    builder.Append(basket != null ? basket.ManaValue : 0).Append(':');
                    builder.Append(basket != null && basket.FieldSprite != null ? basket.FieldSprite.name : string.Empty).Append('|');
                }
            }

            return builder.ToString();
        }

        private static void DestroyObjectImmediateSafe(Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
                return;
            }

            DestroyImmediate(target);
        }

        private static void SetDontSaveRecursive(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return;
            }

            gameObject.hideFlags = HideFlags.DontSaveInEditor;
            foreach (Transform child in gameObject.transform)
            {
                if (child != null)
                {
                    SetDontSaveRecursive(child.gameObject);
                }
            }
        }
    }
}
