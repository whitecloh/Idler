using UnityEngine;

namespace Game.Data.Settings
{
    [CreateAssetMenu(menuName = "IdleClicker/GameSettings", fileName = "GeneralGameSettingsData")]
    public class GeneralGameSettingsData : ScriptableObject
    {
        [SerializeField] private int startBalance;
        [SerializeField] private float autosaveInterval = 30f;
        [SerializeField] private SystemLanguage selectedLanguage = SystemLanguage.English;
        
        public int StartBalance => startBalance;
        public float AutosaveInterval => autosaveInterval;
        public SystemLanguage SelectedLanguage => selectedLanguage;
    }
}