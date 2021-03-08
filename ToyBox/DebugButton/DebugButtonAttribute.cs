using System;

/// <summary>
/// An attribute for drawing a debug button at the bottom of the inspector, which functionality to be run in editor through inspector.
/// </summary>
/// <seealso cref="System.Attribute" />
[AttributeUsage(AttributeTargets.Method)]
public sealed class DebugButtonAttribute : Attribute
{
}