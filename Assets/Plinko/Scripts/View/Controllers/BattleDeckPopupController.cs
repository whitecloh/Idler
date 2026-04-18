using System.Collections.Generic;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Items;
using UnityEngine;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Controllers
{
    public sealed class BattleDeckPopupController : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private BattleDeckUnitCardView cardPrefab;
        [SerializeField] private Button closeButton;

        private readonly List<BattleDeckUnitCardView> _views = new();
        private bool _listenersBound;

        public void Init()
        {
            if (_listenersBound)
            {
                return;
            }

            closeButton.onClick.AddListener(Hide);
            _listenersBound = true;
        }

        public void Refresh(IReadOnlyList<BattleDeckUnitViewData> units)
        {
            Rebuild(units);
        }

        public void Toggle()
        {
            root.SetActive(!root.activeSelf);
        }

        public void Show()
        {
            root.SetActive(true);
        }

        public void Hide()
        {
            root.SetActive(false);
        }

        public void HideImmediate()
        {
            root.SetActive(false);
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
