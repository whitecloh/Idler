using System;
using System.IO;
using Game.Save;
using UnityEngine;
using Utils;

namespace Game.Services
{
    public static class SaveService
    {
        public static SaveData Load()
        {
            var path = Path.Combine(Application.persistentDataPath, Constants.SaveFileName);
            if (!File.Exists(path))
            {
                var data = new SaveData();
                
                var allBizIds = ConfigService.Instance.GetAllBusinessIds();
                if (allBizIds is { Count: > 0 })
                {
                    data.Businesses[allBizIds[0]].Level = 1;
                }
                data.Balance = ConfigService.Instance?.GetStartBalance ?? 0;
                
                Save(data);
                return data;
            }
            var json = File.ReadAllText(path);
            try
            {
                return JsonUtility.FromJson<SaveData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError("Save load error: " + e.Message);
                
                var data = new SaveData
                {
                    Balance = ConfigService.Instance != null
                        ? ConfigService.Instance.GetStartBalance
                        : 0
                };
                return data;
            }
        }

        public static void Save(SaveData data)
        {
            data.LastSaveTimestamp = new SerializedDateTime(DateTime.UtcNow);
            var json = JsonUtility.ToJson(data, true);
            var path = Path.Combine(Application.persistentDataPath, Constants.SaveFileName);
            File.WriteAllText(path, json);
        }
    }
}