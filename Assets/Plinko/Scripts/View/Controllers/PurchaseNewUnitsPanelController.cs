using System.Collections.Generic;
using DG.Tweening;
using Plinko.Scripts.View.Animations;
using Plinko.Scripts.View.Audio;
using Plinko.Scripts.View.Items;
using UnityEngine;

namespace Plinko.Scripts.View.Controllers
{
    public sealed class PurchaseNewUnitsPanelController : MonoBehaviour
    {
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private PurchaseUnitCardView unitCardPrefab;
        [SerializeField] private RectTransform animationLayerRoot;
        [SerializeField] private Camera uiCamera;
        [SerializeField] private float cardFlightDuration = 0.32f;

        private readonly Dictionary<int, PurchaseUnitCardView> _cardsByRuntimeId = new();

        public void ResetState()
        {
            foreach (var pair in _cardsByRuntimeId)
            {
                Destroy(pair.Value.gameObject);
            }

            _cardsByRuntimeId.Clear();
        }

        public void ApplyCompletedTrainings(IReadOnlyList<PurchaseTrainingCompletionVisualPayload> completions)
        {
            for (var index = 0; index < completions.Count; index++)
            {
                var completion = completions[index];
                if (_cardsByRuntimeId.ContainsKey(completion.RuntimeId))
                {
                    continue;
                }

                var realCard = Instantiate(unitCardPrefab, contentRoot);
                realCard.transform.SetSiblingIndex(0);
                realCard.Refresh(completion.CardData);
                realCard.RectTransform.localScale = Vector3.zero;
                _cardsByRuntimeId[completion.RuntimeId] = realCard;

                Canvas.ForceUpdateCanvases();

                var targetAnchoredPosition = UiRectTransformUtility.WorldToAnchoredPosition(
                    animationLayerRoot,
                    uiCamera,
                    UiRectTransformUtility.GetWorldCenter(realCard.RectTransform));

                var ghostCard = Instantiate(unitCardPrefab, animationLayerRoot);
                ghostCard.Refresh(completion.CardData);
                ghostCard.RectTransform.anchoredPosition = UiRectTransformUtility.WorldToAnchoredPosition(
                    animationLayerRoot,
                    uiCamera,
                    completion.WorldPosition);
                ghostCard.RectTransform.localScale = Vector3.one;

                UiAnimationManager.Instance.PlayMoveAndScale(
                    ghostCard.RectTransform,
                    "trained-card-flight",
                    targetAnchoredPosition,
                    Vector3.one * 0.85f,
                    cardFlightDuration,
                    Ease.OutCubic,
                    Ease.OutQuad,
                    () =>
                    {
                        Destroy(ghostCard.gameObject);
                        AudioManager.Instance?.Play(GameAudioCueType.CardAppear);
                        UiAnimationManager.Instance.PlayScaleTo(realCard.RectTransform, "trained-card-reveal", Vector3.one, 0.18f, Ease.OutBack);
                    });
            }
        }
    }
}
