namespace SharpPipe;

/// <summary>
/// Helper class for easier manipulation of parameters
/// </summary>
public static class ParameterHelper
{
    /// <summary>
    /// Copies the parameters from one Zita filter implementation to another
    /// </summary>
    /// <param name="zita">This Zita filter</param>
    /// <param name="other">The Zita filter to copy parameters from</param>
    public static IZitaFilter FromOther(this IZitaFilter zita, in IZitaFilter other)
    {
        zita.InDelay = other.InDelay;
        zita.Crossover = other.Crossover;
        zita.RT60Low = other.RT60Low;
        zita.RT60Mid = other.RT60Mid;
        zita.HighFrequencyDamping = other.HighFrequencyDamping;
        zita.EQ1Frequency = other.EQ1Frequency;
        zita.EQ1Level = other.EQ1Level;
        zita.EQ2Frequency = other.EQ2Frequency;
        zita.EQ2Level = other.EQ2Level;
        zita.Mix = other.Mix;
        zita.Level = other.Level;

        return zita;
    }
}