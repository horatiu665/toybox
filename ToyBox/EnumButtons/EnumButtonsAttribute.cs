namespace ToyBoxHHH
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Draws a one-line sequence of named buttons, that makes a short enum easy to select.
    /// For longer enums consider using <see cref="EnumLongSelectionAttribute"/>
    /// 
    /// Inspired from http://www.sharkbombs.com/2015/02/17/unity-editor-enum-flags-as-toggle-buttons/
    /// 
    /// made by @horatiu665
    /// </summary>
    public class EnumButtonsAttribute : PropertyAttribute
    {
        // should the enum be treated as flags or not?
        public bool isFlags = false;

        public EnumButtonsAttribute(bool flags = false)
        {
            isFlags = flags;
        }
    }
}