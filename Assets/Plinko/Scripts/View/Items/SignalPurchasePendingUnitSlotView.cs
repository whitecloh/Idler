using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Animations;
using UnityEngine;

namespace Plinko.Scripts.View.Items
{
    public sealed class SignalPurchasePendingUnitSlotView : MonoBehaviour
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private GameObject emptyStateRoot;
        [SerializeField] private GameObject filledStateRoot;
        [SerializeField] private PurchaseUnitCardView cardView;

        public RectTransform Root => root;
        public PurchaseUnitCardView CardView => cardView;

        public void ShowEmpty(bool isVisible)
        {
            if (root != null)
            {
                root.gameObject.SetActive(isVisible);
            }

            if (emptyStateRoot != null)
            {
                emptyStateRoot.SetActive(isVisible);
            }

            if (filledStateRoot != null)
            {
                filledStateRoot.SetActive(false);
            }
        }

        public void Refresh(SignalPurchasePendingUnitCardViewData viewData)
        {
            if (root != null)
            {
                root.gameObject.SetActive(true);
            }

            if (emptyStateRoot != null)
            {
                emptyStateRoot.SetActive(false);
            }

            if (filledStateRoot != null)
            {
                filledStateRoot.SetActive(true);
            }

            if (cardView != null)
            {
                cardView.Refresh(viewData);
            }
        }

        public void Refresh(PurchaseTrainedUnitCardViewData viewData)
        {
            if (root != null)
            {
                root.gameObject.SetActive(true);
            }

            if (emptyStateRoot != null)
            {
                emptyStateRoot.SetActive(false);
            }

            if (filledStateRoot != null)
            {
                filledStateRoot.SetActive(true);
            }

            if (cardView != null)
            {
                cardView.Refresh(viewData);
            }
        }

        public Vector3 GetCardWorldCenter()
        {
            return cardView != null
                ? UiRectTransformUtility.GetWorldCenter(cardView.RectTransform)
                : UiRectTransformUtility.GetWorldCenter(root);
        }

        public void PlayCardPunch(float intensity = 1f)
        {
            if (cardView != null)
            {
                UiAnimationManager.Instance.PlaySpringPunch(cardView.RectTransform, intensity);
            }
        }
    }
}
