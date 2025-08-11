namespace Game.Data.Settings
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "IdleClicker/GameSettings", fileName = "GeneralGameSettingsData")]
    public class GeneralGameSettingsData : ScriptableObject
    {
        [SerializeField] private int startBalance;

        public int StartBalance => startBalance;
    }
}