using System.Collections.Generic;
using Plinko.Scripts.Models.ViewData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Items
{
    public sealed class FieldUpgradeSelectedPinCardView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private RectTransform modifiersRoot;
        [SerializeField] private FieldUpgradeModifierLineView modifierLinePrefab;
        [SerializeField] private GameObject emptyStateRoot;

        private readonly List<FieldUpgradeModifierLineView> _modifierViews = new();

        public void Refresh(FieldUpgradeSelectedPinViewData viewData)
        {
            var hasValue = viewData != null;
            root.SetActive(hasValue);
            emptyStateRoot.SetActive(!hasValue);

            if (!hasValue)
            {
                ClearModifierLines();
                return;
            }

            iconImage.sprite = viewData.Sprite;
            iconImage.enabled = viewData.Sprite != null;
            nameText.text = viewData.DisplayName;
            SyncModifierLines(viewData.ModifierLines);
        }

        private void SyncModifierLines(IReadOnlyList<PinModifierLineViewData> modifiers)
        {
            var targetCount = modifiers != null ? modifiers.Count : 0;
            while (_modifierViews.Count < targetCount)
            {
                _modifierViews.Add(Instantiate(modifierLinePrefab, modifiersRoot));
            }

            for (var index = _modifierViews.Count - 1; index >= targetCount; index--)
            {
                Destroy(_modifierViews[index].gameObject);
                _modifierViews.RemoveAt(index);
            }

            for (var index = 0; index < targetCount; index++)
            {
                _modifierViews[index].Refresh(modifiers[index]);
            }
        }

        private void ClearModifierLines()
        {
            for (var index = _modifierViews.Count - 1; index >= 0; index--)
            {
                Destroy(_modifierViews[index].gameObject);
            }

            _modifierViews.Clear();
        }
    }
}
