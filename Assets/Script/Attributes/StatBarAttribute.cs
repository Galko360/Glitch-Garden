using UnityEngine;

/// <summary>
/// Draws an int or float field as a colored fill bar in the Inspector.
/// The bar color shifts from red (empty) to green (full) based on value/max.
///
/// Usage:
///   [StatBar(100)] public int hp;
///   [StatBar(5f)]  public float speed;
/// </summary>
public class StatBarAttribute : PropertyAttribute
{
    public readonly float max;

    public StatBarAttribute(float max = 100f)
    {
        this.max = max;
    }
}
