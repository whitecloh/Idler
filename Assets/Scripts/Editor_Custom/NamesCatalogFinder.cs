#if UNITY_EDITOR

namespace Editor_Custom
{
    using Game.Data.Names;
    using UnityEditor;
    
    internal static class NamesCatalogFinder
    {
        public static NamesCatalog FindCatalog()
        {
            var guids = AssetDatabase.FindAssets("t:" + nameof(NamesCatalog));
            if (guids == null || guids.Length == 0) return null;
            
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<NamesCatalog>(path);
        }
    }
}
#endif