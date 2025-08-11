namespace Game.Data.Business
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    
    public enum BusinessId
    {
        None = 0,
        LemonadeStand = 1,
        FoodTruck = 2,
        CoffeeShop = 3,
        PizzaDelivery = 4,
        SushiBar = 5
    }
    
    [Serializable]
    public class BusinessIdItem
    {
        [SerializeField] private string itemName = string.Empty;
        [SerializeField] private BusinessId id = BusinessId.None;
        [SerializeField] private BusinessConfigData data;

        public string ItemName => itemName;
        public BusinessId Id => id;
        public BusinessConfigData Data => data;
        

        public void SetName(string newName)
        {
            itemName = newName;
        }

        public void SetId(BusinessId newId)
        {
            id = newId;
        }

        public void SetData(BusinessConfigData value)
        {
            data = value;
        }
    }
    
    [CreateAssetMenu(menuName = "IdleClicker/BusinessesConfigsData", fileName = "BusinessesConfigsData")]
    public class BusinessesConfigsData : ScriptableObject
    {
        [SerializeField] private List<BusinessIdItem> items = new();
        public IReadOnlyList<BusinessIdItem> Items => items;
        
        public void AddItem(string itemName, BusinessId businessId, BusinessConfigData data = null)
        {
            if (string.IsNullOrWhiteSpace(itemName))
            {
                Debug.LogError("ItemName is empty.");
                return;
            }
            
            if (items.Exists(i => i.ItemName == itemName))
            {
                Debug.LogError($"Id {itemName} is not unique!");
                return;
            }
            
            if (businessId == BusinessId.None)
            {
                Debug.LogError("BusinessId cannot be None.");
                return;
            }

            if (items.Exists(i => i.Id == businessId))
            {
                Debug.LogError($"BusinessId '{businessId}' is not unique!");
                return;
            }

            var id = new BusinessIdItem();
            id.SetName(itemName);
            id.SetId(businessId);
            id.SetData(data);
            items.Add(id);
            
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        public void RemoveAt(int index)
        {
            if (index >= 0 && index < items.Count)
            {
                items.RemoveAt(index);
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }
    }
}