using UnityEngine;

namespace Plinko.Scripts.View.Controllers
{
    public sealed class OwnedUnitsScreenController : MonoBehaviour
    {
        [SerializeField] private GameObject root;

        public void Show(bool isVisible)
        {
            root.SetActive(isVisible);
        }

        public void SetVisibleImmediate(bool isVisible)
        {
            root.SetActive(isVisible);
        }
    }
}
