// Snatched from Chinchillada.DefaultAsset
// Gets reference to first object in project that appears when searching for the referenced type.
namespace ToyBoxHHH.DefaultAsset
{
    using System;
    using System.Linq;
    using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif


    [AttributeUsage(AttributeTargets.Field)]
    public class DefaultAssetAttribute : PropertyAttribute
    {
        private readonly string _assetName;

        public DefaultAssetAttribute(string defaultAssetName)
        {
            _assetName = defaultAssetName;
        }

        public object GetDefaultAsset(Type type)
        {
#if UNITY_EDITOR

            var searchFilter = $"{_assetName} t:{type.Name}";
            var guids = AssetDatabase.FindAssets(searchFilter);
            if (!guids.Any())
                return null;

            string guid = guids.First();
            string path = AssetDatabase.GUIDToAssetPath(guid);
            return AssetDatabase.LoadAssetAtPath(path, type);
#endif
#pragma warning disable 162
            return null;
#pragma warning restore 162
        }

    }
}
