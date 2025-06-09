using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor_Custom
{
    public static class ScriptableObjectAutoInjector
    {
        public static T GetInstance<T>() where T : ScriptableObject
        {
#if UNITY_EDITOR
            var asset = AssetFinder.FindAssetByType<T>();
            if (asset != null) return asset;
#endif
            Debug.LogError(
                $"No instance of {typeof(T)} found! Please assign it in your GameSettings or similar singleton.");
            return null;
        }
    }

#if UNITY_EDITOR
    public static class AssetFinder
    {
        public static T FindAssetByType<T>() where T : ScriptableObject
        {
            var guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
            return guids.Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<T>).FirstOrDefault(asset => asset != null);
        }
    }
#endif
}

