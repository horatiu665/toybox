// Snatched from Kaae.DebugButton and improved by HHH

// namespace ignored by default for easy access to the [DebugButton] shorthand
// uncomment this #define line if you have name overlapping problems with another DebugButton
//#define TOYBOX_DEBUG_BUTTON_NAMESPACE

#if TOYBOX_DEBUG_BUTTON_NAMESPACE
namespace ToyBox
{
#endif

using System;

/// <summary>
/// An attribute for drawing a debug button at the bottom of the inspector, which functionality to be run in editor through inspector.
/// </summary>
/// <seealso cref="System.Attribute" />
[AttributeUsage(AttributeTargets.Method)]
public sealed class DebugButtonAttribute : Attribute
{
}

#if (TOYBOX_DEBUG_BUTTON_NAMESPACE)
}
#endif