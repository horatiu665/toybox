namespace ToyBoxHHH
{
    using UnityEditor;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(SingleLayerAttribute))]
    class SingleLayerAttributeEditor : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            property.intValue = EditorGUI.LayerField(position, label, property.intValue);
            EditorGUI.EndProperty();

        }
    }
}