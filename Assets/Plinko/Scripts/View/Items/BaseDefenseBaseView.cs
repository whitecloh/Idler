using Plinko.Scripts.Models.ViewData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Items
{
    public sealed class BaseDefenseBaseView : MonoBehaviour
    {
        [SerializeField] private Image baseImage;
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private Image progressFillImage;
        [SerializeField] private TMP_Text progressText;

        public void Refresh(BattleBaseViewData baseViewData, int currentProgress, int maxProgress)
        {
            baseImage.sprite = baseViewData.Sprite;
            baseImage.enabled = baseViewData.Sprite != null;
            healthText.text = $"{baseViewData.CurrentHealth}/{baseViewData.MaxHealth}";
            if (progressFillImage != null)
            {
                progressFillImage.fillAmount = maxProgress > 0 ? Mathf.Clamp01((float)currentProgress / maxProgress) : 0f;
            }

            if (progressText != null)
            {
                progressText.text = $"{currentProgress}/{maxProgress}";
            }
        }
    }
}
