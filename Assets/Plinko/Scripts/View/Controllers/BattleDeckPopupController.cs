using DG.Tweening;
using System.Collections.Generic;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Items;
using UnityEngine;

namespace Plinko.Scripts.View.Controllers
{
    public sealed class BattleDeckPopupController : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private BattleDeckUnitCardView cardPrefab;
        [SerializeField] private float fadeDuration = 0.12f;

        private readonly List<BattleDeckUnitCardView> _views = new();
        private bool _listenersBound;
        private Tween _fadeTween;

        public void Init()
        {
            if (_listenersBound)
            {
                return;
            }
            
            _listenersBound = true;
        }

        public void Refresh(IReadOnlyList<BattleDeckUnitViewData> units)
        {
            Rebuild(units);
        }

        public void Toggle()
        {
            var shouldOpen = !root.activeSelf;
            if (shouldOpen)
            {
                Show();
                return;
            }

            Hide();
        }

        public void Show()
        {
            _fadeTween?.Kill();
            root.SetActive(true);
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
                _fadeTween = canvasGroup.DOFade(1f, fadeDuration).SetEase(Ease.OutQuad);
            }
        }

        public void Hide()
        {
            _fadeTween?.Kill();
            if (canvasGroup == null)
            {
                root.SetActive(false);
                return;
            }

            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            _fadeTween = canvasGroup
                .DOFade(0f, fadeDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => root.SetActive(false));
        }

        public void HideImmediate()
        {
            _fadeTween?.Kill();
            root.SetActive(false);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
        }

        private void Rebuild(IReadOnlyList<BattleDeckUnitViewData> units)
        {
            for (var index = 0; index < _views.Count; index++)
            {
                Destroy(_views[index].gameObject);
            }

            _views.Clear();
            for (var index = 0; index < units.Count; index++)
            {
                var view = Instantiate(cardPrefab, contentRoot);
                view.Refresh(units[index]);
                _views.Add(view);
            }
        }
    }
}
