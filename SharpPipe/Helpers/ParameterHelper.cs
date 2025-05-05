namespace SharpPipe;

/// <summary>
/// Helper class for easier manipulation of parameters
/// </summary>
public static class ParameterHelpers
{
    /// <summary>
    /// Clamps a float between a min and max value. (Supplementary for .NET Framework 4.6.2/4.7.2 builds)
    /// </summary>
    /// <param name="value">Value to clamp</param>
    /// <param name="min">Minimum value</param>
    /// <param name="max">Maximum value</param>
    /// <returns>Clamped value</returns>
    public static float Clamp(float value, float min, float max) => Math.Max(min, Math.Min(max, value));


    /// <summary>
    /// Ensures that a number is valid with an optional fallback.
    /// </summary>
    /// <param name="num">The number to check.</param>
    /// <param name="fallback">The optional fallback to return if the number is invalid.</param>
    /// <returns>The input number if valid, otherwise the fallback value.</returns>
    public static float EnsureValidNum(this float num, float fallback = 0f)
    {
        if (float.IsNaN(num) || float.IsInfinity(num))
            return fallback;
        
        return num;
    }
}
