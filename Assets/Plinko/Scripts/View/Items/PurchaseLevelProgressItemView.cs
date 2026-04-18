using Plinko.Scripts.Models.ViewData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Items
{
    public sealed class PurchaseLevelProgressItemView : MonoBehaviour
    {
        [SerializeField] private TMP_Text levelNumberText;
        [SerializeField] private Image typeImage;
        [SerializeField] private GameObject completedStateRoot;
        [SerializeField] private GameObject currentStateRoot;
        [SerializeField] private GameObject lockedStateRoot;

        public void Refresh(PurchaseLevelProgressEntryViewData viewData)
        {
            levelNumberText.text = viewData.DisplayNumber.ToString();
            typeImage.sprite = viewData.ProgressSprite;
            typeImage.enabled = viewData.ProgressSprite != null;
            completedStateRoot.SetActive(viewData.IsCompleted);
            currentStateRoot.SetActive(viewData.IsCurrent);
            lockedStateRoot.SetActive(!viewData.IsUnlocked);
        }
    }
}