using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(UIListBox))]
public class UIListBoxEditor : Editor {

	SerializedProperty offset;

	void OnEnable()
	{
		offset = serializedObject.FindProperty("offsetY");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		base.OnInspectorGUI();
		var t = (UIListBox)target;
		EditorGUILayout.PropertyField(offset);

		serializedObject.ApplyModifiedProperties();

	}

}
