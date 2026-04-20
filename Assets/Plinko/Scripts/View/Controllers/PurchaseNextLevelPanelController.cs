using System.Collections.Generic;
using DG.Tweening;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Animations;
using Plinko.Scripts.View.Bridges;
using Plinko.Scripts.View.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Controllers
{
    public sealed class PurchaseNextLevelPanelController : MonoBehaviour
    {
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image playerBaseImage;
        [SerializeField] private TMP_Text playerBaseHealthText;
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private RectTransform armyUnitsRoot;
        [SerializeField] private PurchaseArmyPreviewUnitView armyUnitPrefab;
        [SerializeField] private RectTransform armyLeftEdgeAnchor;
        [SerializeField] private RectTransform armyRightEdgeAnchor;
        [SerializeField] private float newUnitScaleDuration = 0.2f;
        [SerializeField] private float moveDuration = 0.18f;

        private readonly Dictionary<int, PurchaseArmyPreviewUnitView> _armyViewsByRuntimeId = new();
        private LocationBridge _locationBridge;
        private bool _listenersBound;

        public void Init(LocationBridge locationBridge)
        {
            _locationBridge = locationBridge;
            BindListeners();
        }

        public void ResetState()
        {
            foreach (var pair in _armyViewsByRuntimeId)
            {
                Destroy(pair.Value.gameObject);
            }

            _armyViewsByRuntimeId.Clear();
        }

        public void Refresh(PurchasePhaseViewData viewData)
        {
            backgroundImage.sprite = viewData.NextBattleBackgroundSprite;
            backgroundImage.enabled = viewData.NextBattleBackgroundSprite != null;
            playerBaseImage.sprite = viewData.PlayerBaseSprite;
            playerBaseImage.enabled = viewData.PlayerBaseSprite != null;
            playerBaseHealthText.text = $"{viewData.PlayerBaseHealth}/{viewData.PlayerBaseMaxHealth}";
            nextLevelButton.interactable = viewData.CanAdvance;

            SyncArmyPreview(viewData.ArmyPreviewUnits);
        }

        public void Refresh(FieldUpgradePhaseViewData viewData)
        {
            backgroundImage.sprite = viewData.NextBattleBackgroundSprite;
            backgroundImage.enabled = viewData.NextBattleBackgroundSprite != null;
            playerBaseImage.sprite = viewData.PlayerBaseSprite;
            playerBaseImage.enabled = viewData.PlayerBaseSprite != null;
            playerBaseHealthText.text = $"{viewData.PlayerBaseHealth}/{viewData.PlayerBaseMaxHealth}";
            nextLevelButton.interactable = viewData.CanAdvance;
            
            SyncArmyPreview(viewData.ArmyPreviewUnits);
        }

        public void Refresh(SignalPurchasePhaseViewData viewData)
        {
            Refresh(viewData, null);
        }

        public void Refresh(SignalPurchasePhaseViewData viewData, IReadOnlyCollection<int> hiddenRuntimeIds)
        {
            backgroundImage.sprite = viewData.NextBattleBackgroundSprite;
            backgroundImage.enabled = viewData.NextBattleBackgroundSprite != null;
            playerBaseImage.sprite = viewData.PlayerBaseSprite;
            playerBaseImage.enabled = viewData.PlayerBaseSprite != null;
            playerBaseHealthText.text = $"{viewData.PlayerBaseHealth}/{viewData.PlayerBaseMaxHealth}";
            nextLevelButton.interactable = viewData.CanAdvance;

            if (hiddenRuntimeIds == null || hiddenRuntimeIds.Count == 0)
            {
                SyncArmyPreview(viewData.ArmyPreviewUnits);
                return;
            }

            var filteredUnits = new List<PurchaseArmyPreviewUnitViewData>();
            for (var index = 0; index < viewData.ArmyPreviewUnits.Count; index++)
            {
                var unit = viewData.ArmyPreviewUnits[index];
                if (!ContainsRuntimeId(hiddenRuntimeIds, unit.RuntimeId))
                {
                    filteredUnits.Add(unit);
                }
            }

            SyncArmyPreview(filteredUnits);
        }

        public Vector3 GetArmyPreviewWorldPosition(int index, int totalCount)
        {
            return armyUnitsRoot.TransformPoint(GetArmySlotPosition(index, totalCount));
        }

        private void BindListeners()
        {
            if (_listenersBound)
            {
                return;
            }

            nextLevelButton.onClick.AddListener(() =>
            {
                UiAnimationManager.Instance.PlaySpringPunch(nextLevelButton.transform as RectTransform);
                _locationBridge.RequestAdvanceToNextLevel();
            });
            _listenersBound = true;
        }

        private void SyncArmyPreview(IReadOnlyList<PurchaseArmyPreviewUnitViewData> units)
        {
            var activeRuntimeIds = new HashSet<int>();
            var visibleCount = units.Count;

            for (var index = 0; index < visibleCount; index++)
            {
                var unit = units[index];
                activeRuntimeIds.Add(unit.RuntimeId);

                var isNew = false;
                if (!_armyViewsByRuntimeId.TryGetValue(unit.RuntimeId, out var view))
                {
                    view = Instantiate(armyUnitPrefab, armyUnitsRoot);
                    _armyViewsByRuntimeId[unit.RuntimeId] = view;
                    isNew = true;
                }

                var rect = view.RectTransform;
                var targetPosition = GetArmySlotPosition(index, visibleCount);
                view.Refresh(unit);

                if (isNew)
                {
                    rect.anchoredPosition = targetPosition;
                    rect.localScale = Vector3.zero;
                    UiAnimationManager.Instance.PlayScaleTo(rect, "army-unit-spawn", Vector3.one, newUnitScaleDuration, Ease.OutBack);
                }
                else
                {
                    rect.localScale = Vector3.one;
                    UiAnimationManager.Instance.PlayMoveAndScale(
                        rect,
                        $"army-unit-move-{unit.RuntimeId}",
                        targetPosition,
                        Vector3.one,
                        moveDuration,
                        Ease.OutQuad,
                        Ease.OutQuad);
                }
            }

            var staleRuntimeIds = new List<int>();
            foreach (var pair in _armyViewsByRuntimeId)
            {
                if (!activeRuntimeIds.Contains(pair.Key))
                {
                    staleRuntimeIds.Add(pair.Key);
                }
            }

            for (var index = 0; index < staleRuntimeIds.Count; index++)
            {
                var runtimeId = staleRuntimeIds[index];
                Destroy(_armyViewsByRuntimeId[runtimeId].gameObject);

                _armyViewsByRuntimeId.Remove(runtimeId);
            }
        }

        private Vector2 GetArmySlotPosition(int index, int totalCount)
        {
            var left = GetLocalAnchorPosition(armyLeftEdgeAnchor);
            var right = GetLocalAnchorPosition(armyRightEdgeAnchor);
            if (totalCount <= 1)
            {
                return Vector2.Lerp(left, right, 0.5f);
            }

            var t = index / (float)(totalCount - 1);
            return Vector2.Lerp(left, right, t);
        }

        private Vector2 GetLocalAnchorPosition(RectTransform anchor)
        {
            if (anchor == null || armyUnitsRoot == null)
            {
                return Vector2.zero;
            }

            return UiRectTransformUtility.WorldToAnchoredPosition(armyUnitsRoot, null, anchor.position);
        }

        private static bool ContainsRuntimeId(IReadOnlyCollection<int> runtimeIds, int runtimeId)
        {
            foreach (var value in runtimeIds)
            {
                if (value == runtimeId)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
