using System;
using UnityEngine;

public class EnumButtonsAttribute : PropertyAttribute
{
    // should the enum be treated as flags or not?
    public bool isFlags = false;

    public EnumButtonsAttribute(bool flags = false)
    {
        isFlags = flags;
    }
}