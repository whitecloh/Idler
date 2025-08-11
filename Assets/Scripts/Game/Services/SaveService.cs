namespace Game.Services
{
    using System;
    using System.IO;
    using Save;
    using UnityEngine;
    using Utils;
    using System.Collections.Generic;
    using Data.Business;

    public static class SaveService
    {
        public static SaveData Load(int startBalance, IReadOnlyList<BusinessId> orderedBusinessIds)
        {
            var path = Path.Combine(Application.persistentDataPath, Constants.SaveFileName);
            if (!File.Exists(path))
            {
                var data = new SaveData();

                if (orderedBusinessIds is { Count: > 0 })
                {
                    data.Businesses[orderedBusinessIds[0]].Level = 1;
                }
                data.Balance = startBalance;

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
                    Balance = startBalance
                };
                return data;
            }
        }

        public static void Save(SaveData data)
        {
            var json = JsonUtility.ToJson(data, true);
            var path = Path.Combine(Application.persistentDataPath, Constants.SaveFileName);
            File.WriteAllText(path, json);
        }
    }
}