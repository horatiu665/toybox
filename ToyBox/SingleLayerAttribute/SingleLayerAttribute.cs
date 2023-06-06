namespace ToyBoxHHH
{
    using UnityEngine;

    /// <summary>
    /// Attribute to select a single layer.
    /// snatched from: https://answers.unity.com/questions/609385/type-for-layer-selection.html
    /// usage:
    /// [SingleLayer] 
    /// public int layer = 31;
    /// </summary>
    public class SingleLayerAttribute : PropertyAttribute
    {
        // NOTHING - just oxygen.
    }
}