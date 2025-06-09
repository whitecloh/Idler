using TMPro;
using UnityEngine;

namespace UI.Elements
{
    public class BalanceView : MonoBehaviour
    {
        [SerializeField] private TMP_Text balanceText;

        public void SetBalance(int value)
        {
            balanceText.text = value.ToString();
        }
    }
}