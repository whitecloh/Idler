using UnityEngine;
using UnityEngine.UI;

namespace Plinko.Scripts.View.Items
{
    public sealed class MenuPlinkoPreviewTokenView : MonoBehaviour
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private Graphic graphic;

        public RectTransform RectTransform => root;

        public void SetColor(Color color)
        {
            graphic.color = color;
        }
    }
}
