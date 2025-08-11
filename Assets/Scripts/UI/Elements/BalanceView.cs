namespace UI.Elements
{
    using TMPro;
    using UnityEngine;
    
    public sealed class BalanceView : MonoBehaviour
    {
        [SerializeField] private TMP_Text balanceText;

        public void SetBalance(long value)
        {
            balanceText.text = value.ToString();
        }
    }
}