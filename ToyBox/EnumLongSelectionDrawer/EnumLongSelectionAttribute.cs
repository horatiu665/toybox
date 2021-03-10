namespace ToyBoxHHH
{
    using System;
    using UnityEngine;

    // Usage:
    // 
    // [EnumLongSelection]
    // public Types aFieldWithEnumValues;
    // 
    // Creates a cool button in the Inspector that you can click and then select an enum value from the generated list, without using the Windows/mac based selection that pauses the game.
    // Useful for VR when tweaking values while someone is inside VR - you don't want to interrupt their experience just because you are selecting a new enum value
    //
    // Can (should) be extended with a search function inside the little pop-up window, but that's for later
    //
    // made by @horatiu665

    public class EnumLongSelectionAttribute : PropertyAttribute
    {
        public Type enumType;

        public EnumLongSelectionAttribute(Type enumType)
        {
            this.enumType = enumType;
        }
    }
}