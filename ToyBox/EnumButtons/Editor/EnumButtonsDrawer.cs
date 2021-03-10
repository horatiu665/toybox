namespace ToyBoxHHH.EnumButtons.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using Random = UnityEngine.Random;

    /// <summary>
    /// Draws a one-line sequence of named buttons, that makes a short enum easy to select.
    /// For longer enums consider using <see cref="EnumLongSelectionAttribute"/>
    /// 
    /// Inspired from http://www.sharkbombs.com/2015/02/17/unity-editor-enum-flags-as-toggle-buttons/
    /// 
    /// made by @horatiu665
    /// </summary>
    [CustomPropertyDrawer(typeof(EnumButtonsAttribute))]
    public class EnumButtonsDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var eba = ((EnumButtonsAttribute)this.attribute);
            bool isFlags = eba.isFlags;
            if (isFlags)
            {
                OnGUI_FlagsVersion(position, property, label);
            }
            else
            {
                OnGUI_EnumVersion(position, property, label);
            }

        }

        private void OnGUI_EnumVersion(Rect position, SerializedProperty property, GUIContent label)
        {
            int buttonsIntValue = 0;
            int enumLength = property.enumNames.Length;
            float buttonWidth = (position.width - EditorGUIUtility.labelWidth) / enumLength;

            EditorGUI.LabelField(new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height), label);

            var obj = property.serializedObject.targetObject;
            var ownerType = obj.GetType();
            var binding = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            var field = ownerType.GetField(property.name, binding);
            var type = field.FieldType;
            int[] enumValues = Enum.GetValues(type).Cast<int>().ToArray();

            EditorGUI.BeginChangeCheck();
            {
                for (int i = 0; i < enumLength; i++)
                {
                    var style = ButtonStyle(enumLength, i);

                    bool buttonWasPressed = false;

                    // Check if the button was pressed. can account for sparse enum values
                    if (property.intValue == enumValues[i])
                    {
                        buttonWasPressed = true;
                    }

                    Rect buttonPos = new Rect(position.x + EditorGUIUtility.labelWidth + buttonWidth * i, position.y, buttonWidth, position.height);

                    var userPressButton = GUI.Toggle(buttonPos, buttonWasPressed, new GUIContent(property.enumNames[i], property.enumNames[i] + " (" + enumValues[i] + ")"), style);

                    // if not flags, only one button can be on at a time. so if the button was not pressed, but is now, then we change the value. otherwise no change.
                    if (userPressButton && !buttonWasPressed)
                    {
                        buttonsIntValue = enumValues[i];
                        // we cannot break; because we want to draw the rest of the interface
                    }
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                property.intValue = buttonsIntValue;
            }
        }

        private static void OnGUI_FlagsVersion(Rect position, SerializedProperty property, GUIContent label)
        {
            int buttonsIntValue = 0;
            int enumLength = property.enumNames.Length;
            bool[] buttonPressed = new bool[enumLength];
            float buttonWidth = (position.width - EditorGUIUtility.labelWidth) / enumLength;

            EditorGUI.LabelField(new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height), label);

            // we use this like in the non-flags version, but we bit operate rather than set value.
            var obj = property.serializedObject.targetObject;
            var ownerType = obj.GetType();
            var binding = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            var field = ownerType.GetField(property.name, binding);
            var type = field.FieldType;
            int[] enumValues = Enum.GetValues(type).Cast<int>().ToArray();

            EditorGUI.BeginChangeCheck();

            for (int i = 0; i < enumLength; i++)
            {
                var style = ButtonStyle(enumLength, i);


                // Check if the button was pressed 
                bool isPowerOfTwo = Mathf.IsPowerOfTwo(enumValues[i]);
                if (enumValues[i] == 0)
                {
                    // if is zero, and flags, if button is pressed, no other button can be pressed, and viceversa. clicking it should not do anything if already true.
                    buttonPressed[i] = property.intValue == 0;
                }
                // here we check if value is power of two. then it's just a simple button. but if it's not, then it's a combination of buttons ;)
                else if (isPowerOfTwo)
                {
                    if ((property.intValue & enumValues[i]) == enumValues[i])
                    {
                        buttonPressed[i] = true;
                    }
                }
                else
                {
                    // if not power of two, the button is a combination of other buttons. just check if it is pressed, but don't add stuff directly, but set flags instead.
                    if ((property.intValue & enumValues[i]) == enumValues[i])
                    {
                        buttonPressed[i] = true;
                    }
                }


                Rect buttonPos = new Rect(position.x + EditorGUIUtility.labelWidth + buttonWidth * i, position.y, buttonWidth, position.height);

                // check if button is pressed
                var userPressButton = GUI.Toggle(buttonPos, buttonPressed[i], new GUIContent(property.enumNames[i], property.enumNames[i] + " (" + enumValues[i] + ")"), style);

                // if flags, every button can be on/off independently from each other. so we always set pressed immediately. beware: enum must have consecutive power of two values.
                if (userPressButton)
                {
                    if (enumValues[i] == 0)
                    {
                        // zero was just pressed
                        buttonsIntValue = 0;
                        property.intValue = 0;
                    }
                    else if (isPowerOfTwo)
                    {
                        buttonPressed[i] = userPressButton;
                        buttonsIntValue += enumValues[i];
                    }
                    else
                    {
                        // not power of two, but button was just made true
                        if (!buttonPressed[i])
                        {
                            buttonsIntValue = buttonsIntValue | enumValues[i];
                        }
                    }
                }
                else
                {
                    // button was just made false. it was true before.
                    if (buttonPressed[i])
                    {
                        if (!isPowerOfTwo)
                        {
                            // XOR makes it opposite hehh, so it will basically flip all the selected flags.
                            buttonsIntValue = buttonsIntValue ^ enumValues[i];
                        }
                    }
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                property.intValue = buttonsIntValue;
            }
        }

        private static GUIStyle ButtonStyle(int enumLength, int i)
        {
            if (i == 0)
            {
                return EditorStyles.miniButtonLeft;
            }
            else if (i == enumLength - 1)
            {
                return EditorStyles.miniButtonRight;
            }
            else
            {
                return EditorStyles.miniButtonMid;
            }
        }
    }
}