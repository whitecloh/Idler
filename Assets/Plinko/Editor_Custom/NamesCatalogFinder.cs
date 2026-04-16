using Plinko.Scripts.Data.Names;
using UnityEditor;

#if UNITY_EDITOR

namespace Plinko.Editor_Custom
{
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