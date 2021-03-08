using System;
using UnityEngine;

public class EnumLongSelectionAttribute : PropertyAttribute
{
    public Type enumType;

    public EnumLongSelectionAttribute(Type enumType)
    {
        this.enumType = enumType;
    }
}
