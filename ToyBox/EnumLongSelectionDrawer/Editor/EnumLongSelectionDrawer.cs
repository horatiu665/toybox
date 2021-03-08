using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

[CustomPropertyDrawer(typeof(EnumLongSelectionAttribute))]
public class EnumLongSelectionDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var elsa = ((EnumLongSelectionAttribute)this.attribute);

        EditorGUI.LabelField(new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height), label);

        var enumRect = position;
        enumRect.x += EditorGUIUtility.labelWidth;
        enumRect.width -= EditorGUIUtility.labelWidth;

        var enumText = "";
        var enumValuesArray = Enum.GetValues(elsa.enumType);
        int[] enumValues = new int[enumValuesArray.Length];
        enumValuesArray.CopyTo(enumValues, 0);
        //= Enum.GetValues(elsa.enumType).Cast<int>().ToArray();
        for (int i = 0; i < property.enumDisplayNames.Length; i++)
        {
            if (property.intValue == enumValues[i])
            {
                enumText = property.enumDisplayNames[i].ToString();
                break;
            }
        }
        var style = new GUIStyle(GUI.skin.button);
        style.alignment = TextAnchor.MiddleLeft;
        style.fontSize = 9;

        if (GUI.Button(enumRect, enumText, style))
        {
            // open window that selects enum value
            EnumLongSelectionWindow.Get(enumRect, elsa.enumType, property);
        }
    }
}

public class EnumLongSelectionWindow : EditorWindow
{
    private Vector2 scroll;
    //private Type enumType;
    private SerializedProperty serProp;
    private int[] enumValues;
    private GUIStyle style;

    public static EnumLongSelectionWindow Get(Rect enumRect, Type enumType, SerializedProperty serProp)
    {
        var w = EnumLongSelectionWindow.GetWindow<EnumLongSelectionWindow>(true, "Enum Long Selection");

        // put the window at the mouse position if possible.
        var mousePos = GUIUtility.GUIToScreenPoint(enumRect.position);
        var size = new Vector2(enumRect.width, enumRect.height * 12);
        // offset is needed because of stupid title bar.
        var offset = new Vector2(0, 50);
        var rect = new Rect(mousePos + offset, size);
        w.position = rect;

        //w.enumType = enumType;
        w.serProp = serProp;

        var enumValuesArray = Enum.GetValues(enumType);
        w.enumValues = new int[enumValuesArray.Length];
        enumValuesArray.CopyTo(w.enumValues, 0);

        // init scroll to the current value of the enum property
        var heightOfOne = GUI.skin.button.CalcHeight(new GUIContent("shit"), 100) + 2;
        var index = 0;
        for (int i = 0; i < w.enumValues.Length; i++)
        {
            if (w.enumValues[i] == serProp.intValue)
            {
                index = i;
                break;
            }
        }
        w.scroll = new Vector2(0, heightOfOne * index);

        w.style = new GUIStyle(GUI.skin.button);
        w.style.alignment = TextAnchor.MiddleLeft;
        w.style.fontSize = 11;
        w.style.normal.textColor = Color.white;

        return w;
    }

    private void OnGUI()
    {

        scroll = EditorGUILayout.BeginScrollView(scroll);
        // draw all enum options and when clicked, assign that value to the property and close the window.
        for (int i = 0; i < enumValues.Length; i++)
        {
            var isThisOne = enumValues[i] == serProp.intValue;

            
            if (GUILayout.Button((isThisOne ? "* " : "   ") + serProp.enumDisplayNames[i], style))
            {
                serProp.intValue = enumValues[i];
                serProp.serializedObject.ApplyModifiedProperties();
                this.Close();
            }

        }

        EditorGUILayout.EndScrollView();

        //GUI.Label(new Rect(10, 10, 100, 100), this.scroll.ToString());

    }
}