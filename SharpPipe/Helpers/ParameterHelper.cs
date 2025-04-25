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
    public static TFrom FromOther<TFrom, TTo>(this TFrom zita, in TTo other)
        where TFrom : IZitaFilter
        where TTo : IZitaFilter
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


    /// <summary>
    /// Ensures that the input values for a given <see cref="IZitaFilter"/> are within acceptable ranges.
    /// </summary>
    /// <typeparam name="TZita">The <see cref="IZitaFilter"/> type.</typeparam>
    /// <param name="zita">The <typeparamref name="TZita"/> to validate the parameters for.</param>
    /// <returns>The original <typeparamref name="TZita"/> but with validated parameters.</returns>
    public static TZita EnsureValidParameters<TZita>(this TZita zita)
        where TZita : IZitaFilter
    {
        #if NET8_0_OR_GREATER
        zita.InDelay              = Math.Clamp(zita.InDelay,              ZitaReverb.ZITA_IN_DELAY_MIN,               ZitaReverb.ZITA_IN_DELAY_MAX)              .EnsureValidNum(ZitaReverb.ZITA_IN_DELAY_DEFAULT);
        zita.Crossover            = Math.Clamp(zita.Crossover,            ZitaReverb.ZITA_CROSSOVER_MIN,              ZitaReverb.ZITA_CROSSOVER_MAX)             .EnsureValidNum(ZitaReverb.ZITA_CROSSOVER_DEFAULT);
        zita.RT60Low              = Math.Clamp(zita.RT60Low,              ZitaReverb.ZITA_RT60LOW_MIN,                ZitaReverb.ZITA_RT60LOW_MAX)               .EnsureValidNum(ZitaReverb.ZITA_RT60LOW_DEFAULT);
        zita.RT60Mid              = Math.Clamp(zita.RT60Mid,              ZitaReverb.ZITA_RT60MID_MIN,                ZitaReverb.ZITA_RT60MID_MAX)               .EnsureValidNum(ZitaReverb.ZITA_RT60MID_DEFAULT);
        zita.HighFrequencyDamping = Math.Clamp(zita.HighFrequencyDamping, ZitaReverb.ZITA_HIGH_FREQUENCY_DAMPING_MIN, ZitaReverb.ZITA_HIGH_FREQUENCY_DAMPING_MAX).EnsureValidNum(ZitaReverb.ZITA_HIGH_FREQUENCY_DAMPING_DEFAULT);
        zita.EQ1Frequency         = Math.Clamp(zita.EQ1Frequency,         ZitaReverb.ZITA_EQ1_FREQUENCY_MIN,          ZitaReverb.ZITA_EQ1_FREQUENCY_MAX)         .EnsureValidNum(ZitaReverb.ZITA_EQ1_FREQUENCY_DEFAULT);
        zita.EQ1Level             = Math.Clamp(zita.EQ1Level,             ZitaReverb.ZITA_EQ1_LEVEL_MIN,              ZitaReverb.ZITA_EQ1_LEVEL_MAX)             .EnsureValidNum(ZitaReverb.ZITA_EQ1_LEVEL_DEFAULT);
        zita.EQ2Frequency         = Math.Clamp(zita.EQ2Frequency,         ZitaReverb.ZITA_EQ2_FREQUENCY_MIN,          ZitaReverb.ZITA_EQ2_FREQUENCY_MAX)         .EnsureValidNum(ZitaReverb.ZITA_EQ2_FREQUENCY_DEFAULT);
        zita.EQ2Level             = Math.Clamp(zita.EQ2Level,             ZitaReverb.ZITA_EQ2_LEVEL_MIN,              ZitaReverb.ZITA_EQ2_LEVEL_MAX)             .EnsureValidNum(ZitaReverb.ZITA_EQ2_LEVEL_DEFAULT);
        zita.Mix                  = Math.Clamp(zita.Mix,                  ZitaReverb.ZITA_MIX_MIN,                    ZitaReverb.ZITA_MIX_MAX)                   .EnsureValidNum(ZitaReverb.ZITA_MIX_DEFAULT);
        zita.Level                = Math.Clamp(zita.Level,                ZitaReverb.ZITA_LEVEL_MIN,                  ZitaReverb.ZITA_LEVEL_MAX)                 .EnsureValidNum(ZitaReverb.ZITA_LEVEL_DEFAULT);
        #else
        static float _clamp(float value, float min, float max) => Math.Max(min, Math.Min(max, value));
        
        zita.InDelay              = _clamp(zita.InDelay,              ZitaReverb.ZITA_IN_DELAY_MIN,               ZitaReverb.ZITA_IN_DELAY_MAX)              .EnsureValidNum(ZitaReverb.ZITA_IN_DELAY_DEFAULT);
        zita.Crossover            = _clamp(zita.Crossover,            ZitaReverb.ZITA_CROSSOVER_MIN,              ZitaReverb.ZITA_CROSSOVER_MAX)             .EnsureValidNum(ZitaReverb.ZITA_CROSSOVER_DEFAULT);
        zita.RT60Low              = _clamp(zita.RT60Low,              ZitaReverb.ZITA_RT60LOW_MIN,                ZitaReverb.ZITA_RT60LOW_MAX)               .EnsureValidNum(ZitaReverb.ZITA_RT60LOW_DEFAULT);
        zita.RT60Mid              = _clamp(zita.RT60Mid,              ZitaReverb.ZITA_RT60MID_MIN,                ZitaReverb.ZITA_RT60MID_MAX)               .EnsureValidNum(ZitaReverb.ZITA_RT60MID_DEFAULT);
        zita.HighFrequencyDamping = _clamp(zita.HighFrequencyDamping, ZitaReverb.ZITA_HIGH_FREQUENCY_DAMPING_MIN, ZitaReverb.ZITA_HIGH_FREQUENCY_DAMPING_MAX).EnsureValidNum(ZitaReverb.ZITA_HIGH_FREQUENCY_DAMPING_DEFAULT);
        zita.EQ1Frequency         = _clamp(zita.EQ1Frequency,         ZitaReverb.ZITA_EQ1_FREQUENCY_MIN,          ZitaReverb.ZITA_EQ1_FREQUENCY_MAX)         .EnsureValidNum(ZitaReverb.ZITA_EQ1_FREQUENCY_DEFAULT);
        zita.EQ1Level             = _clamp(zita.EQ1Level,             ZitaReverb.ZITA_EQ1_LEVEL_MIN,              ZitaReverb.ZITA_EQ1_LEVEL_MAX)             .EnsureValidNum(ZitaReverb.ZITA_EQ1_LEVEL_DEFAULT);
        zita.EQ2Frequency         = _clamp(zita.EQ2Frequency,         ZitaReverb.ZITA_EQ2_FREQUENCY_MIN,          ZitaReverb.ZITA_EQ2_FREQUENCY_MAX)         .EnsureValidNum(ZitaReverb.ZITA_EQ2_FREQUENCY_DEFAULT);
        zita.EQ2Level             = _clamp(zita.EQ2Level,             ZitaReverb.ZITA_EQ2_LEVEL_MIN,              ZitaReverb.ZITA_EQ2_LEVEL_MAX)             .EnsureValidNum(ZitaReverb.ZITA_EQ2_LEVEL_DEFAULT);
        zita.Mix                  = _clamp(zita.Mix,                  ZitaReverb.ZITA_MIX_MIN,                    ZitaReverb.ZITA_MIX_MAX)                   .EnsureValidNum(ZitaReverb.ZITA_MIX_DEFAULT);
        zita.Level                = _clamp(zita.Level,                ZitaReverb.ZITA_LEVEL_MIN,                  ZitaReverb.ZITA_LEVEL_MAX)                 .EnsureValidNum(ZitaReverb.ZITA_LEVEL_DEFAULT);
        #endif

        return zita;
    }


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