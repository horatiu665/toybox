using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public sealed class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var attrib = this.attribute as ReadOnlyAttribute;
        var disabling = !attrib.runtimeOnly || (attrib.runtimeOnly && Application.isPlaying);

        if (disabling)
        {
            GUI.enabled = false;
        }

        label = EditorGUI.BeginProperty(position, label, property);
        EditorGUI.PropertyField(position, property, label, true);
        EditorGUI.EndProperty();

        if (disabling)
        {
            GUI.enabled = true;
        }
    }
}
