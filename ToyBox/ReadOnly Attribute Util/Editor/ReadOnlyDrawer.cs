namespace ToyBoxHHH.ReadOnlyUtil.Editor
{
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Convenience <see cref="PropertyAttribute"/> for marking fields as "Read Only".
    /// Used for exposing values and references in the Unity inspector, which cannot be changed or modified outside of code.
    /// If runtimeOnly is true, will only be read-only at runtime, while being changeable at edit time.
    /// </summary>
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
}