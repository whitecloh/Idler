using System;
using System.IO;
using Plinko.Scripts.Models;
using UnityEngine;

namespace Plinko.Scripts.Services
{
    public sealed class RunSaveService
    {
        private readonly string _savePath;

        public string SavePath => _savePath;

        public RunSaveService(string savePath)
        {
            _savePath = savePath;
        }

        public RunSaveDto Load()
        {
            if (!File.Exists(_savePath))
            {
                return new RunSaveDto();
            }

            try
            {
                var json = File.ReadAllText(_savePath);
                return JsonUtility.FromJson<RunSaveDto>(json) ?? new RunSaveDto();
            }
            catch (Exception exception)
            {
                Debug.LogError($"Run save load failed: {exception.Message}");
                return new RunSaveDto();
            }
        }

        public void Save(RunSaveDto dto)
        {
            try
            {
                var json = JsonUtility.ToJson(dto, true);
                File.WriteAllText(_savePath, json);
            }
            catch (Exception exception)
            {
                Debug.LogError($"Run save write failed: {exception.Message}");
            }
        }

        public void Clear()
        {
            try
            {
                if (File.Exists(_savePath))
                {
                    File.Delete(_savePath);
                }
            }
            catch (Exception exception)
            {
                Debug.LogError($"Run save clear failed: {exception.Message}");
            }
        }
    }
}
