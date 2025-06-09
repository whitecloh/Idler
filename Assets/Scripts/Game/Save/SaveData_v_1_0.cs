using System;
using Game.Data.Business;
using UnityEngine;
using Utils;

namespace Game.Save
{
    [Serializable]
    public class SaveData_v_1_0
    {
        [SerializeField] private int balance;
        [SerializeField] private SerializedDateTime lastSaveTimestamp = new();
        [SerializeField] private BusinessSaveItems businesses = new();

        public int Balance
        {
            get => balance;
            set => balance = value;
        }

        public SerializedDateTime LastSaveTimestamp
        {
            get => lastSaveTimestamp;
            set => lastSaveTimestamp = value;
        }

        public BusinessSaveItems Businesses => businesses;

        [Serializable]
        public class BusinessSaveItems : SaveItems<BusinessId, BusinessSave>{ };

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
        public class UpgradesSaveItems : SaveItems<int , UpgradeSave>{ };
        
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

        public void Clear()
        {
            balance = 0;
            lastSaveTimestamp = new SerializedDateTime();
            businesses.Clear();
        }
    }
}