using System;
using System.IO;
using Plinko.Scripts.Models;
using UnityEngine;

namespace Plinko.Scripts.Services
{
    public sealed class MetaSaveService
    {
        private readonly string _savePath;

        public string SavePath => _savePath;

        public MetaSaveService(string savePath)
        {
            _savePath = savePath;
        }

        public MetaSaveDto Load()
        {
            if (!File.Exists(_savePath))
            {
                return new MetaSaveDto();
            }

            try
            {
                var json = File.ReadAllText(_savePath);
                return JsonUtility.FromJson<MetaSaveDto>(json) ?? new MetaSaveDto();
            }
            catch (Exception exception)
            {
                Debug.LogError($"Meta save load failed: {exception.Message}");
                return new MetaSaveDto();
            }
        }

        public void Save(MetaSaveDto dto)
        {
            try
            {
                var json = JsonUtility.ToJson(dto ?? new MetaSaveDto(), true);
                File.WriteAllText(_savePath, json);
            }
            catch (Exception exception)
            {
                Debug.LogError($"Meta save write failed: {exception.Message}");
            }
        }
    }
}
