namespace Game.Save
{
    using System;
    using Data.Business;
    using UnityEngine;
    
    [Serializable]
    public class SaveData_v_1_0
    {
        [SerializeField] private long balance;
        [SerializeField] private BusinessSaveItems businesses = new();

        public long Balance
        {
            get => balance;
            set => balance = value;
        }

        public BusinessSaveItems Businesses => businesses;

        public void Clear()
        {
            balance = 0;
            businesses.Clear();
        }

        [Serializable]
        public class BusinessSaveItems : SaveItems<BusinessId, BusinessSave> { }

        [Serializable]
        public class BusinessSave
        {
            [SerializeField] private int level;
            [SerializeField] private float progress;
            [SerializeField] private UpgradesSaveItems upgrades = new();

            public int Level
            {
                get => level;
                set => level = value;
            }

            public float Progress
            {
                get => progress;
                set => progress = value;
            }

            public UpgradesSaveItems Upgrades
            {
                get => upgrades;
                set => upgrades = value;
            }

            public void Clear()
            {
                level = 0;
                progress = 0f;
                upgrades = new UpgradesSaveItems();
            }
        }

        [Serializable]
        public class UpgradesSaveItems : SaveItems<int, UpgradeSave> { }

        [Serializable]
        public class UpgradeSave
        {
            [SerializeField] private bool isActive;

            public bool IsActive
            {
                get => isActive;
                set => isActive = value;
            }

            public void Clear()
            {
                isActive = false;
            }
        }
    }
}