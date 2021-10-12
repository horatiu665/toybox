// Snatched from ...??? different places.

namespace ToyBoxHHH.ReadOnlyUtil
{

	using System;
    using UnityEngine;

    /// <summary>
    /// Convenience <see cref="PropertyAttribute"/> for marking fields as "Read Only".
    /// Used for exposing values and references in the Unity inspector, which cannot be changed or modified outside of code.
    /// If runtimeOnly is true, will only be read-only at runtime, while being changeable at edit time.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ReadOnlyAttribute : PropertyAttribute
    {
        public bool runtimeOnly
        {
            get;
            set;
        }
    }

}