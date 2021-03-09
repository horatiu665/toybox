// Snatched from Chinchillada.DefaultAsset
// Gets reference to first object in project that appears when searching for the referenced type.
namespace ToyBox.DefaultAsset
{
    using UnityEditor;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(DefaultAssetAttribute))]
    public class DefaultAssetDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUILayout.PropertyField(property, label, true);

            if (property.objectReferenceValue != null)
                return;

            var defaultAssetAttribute = (DefaultAssetAttribute)attribute;
            var defaultAsset = (Object)defaultAssetAttribute.GetDefaultAsset(fieldInfo.FieldType);

            property.objectReferenceValue = defaultAsset;
        }
    }
}