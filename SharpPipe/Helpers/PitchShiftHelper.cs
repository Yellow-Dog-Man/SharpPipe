namespace SharpPipe;

/// <summary>
/// Helpers for pitch shifter filters.
/// </summary>
public static class PitchShiftHelpers
{
    /// <summary>
    /// Copies the parameters from one pitch shift filter implementation to another
    /// </summary>
    /// <param name="shifter">This pitch shift filter</param>
    /// <param name="other">The pitch shift filter to copy parameters from</param>
    public static TFrom FromOther<TFrom, TTo>(this TFrom shifter, TTo other)
        where TFrom : IPitchShiftFilter
        where TTo : IPitchShiftFilter
    {
        shifter.SemitoneShift   = other.SemitoneShift;
        shifter.WindowSize      = other.WindowSize;
        shifter.CrossfadeSamples = other.CrossfadeSamples;

        return shifter;
    }


    /// <summary>
    /// Ensures that the input values for a given <see cref="IPitchShiftFilter"/> are within acceptable ranges.
    /// </summary>
    /// <typeparam name="TPitchShifter">The <see cref="IPitchShiftFilter"/> type.</typeparam>
    /// <param name="shifter">The <typeparamref name="TPitchShifter"/> to validate the parameters for.</param>
    /// <returns>The original <typeparamref name="TPitchShifter"/> but with validated parameters.</returns>
    public static TPitchShifter EnsureValidParameters<TPitchShifter>(this TPitchShifter shifter)
        where TPitchShifter : IPitchShiftFilter
    {
        #if NET8_0_OR_GREATER
        shifter.SemitoneShift   = Math.Clamp(shifter.SemitoneShift,     PitchShift.PITCHSHIFT_SEMITONE_SHIFT_MIN,       PitchShift.PITCHSHIFT_SEMITONE_SHIFT_MAX)   .EnsureValidNum(PitchShift.PITCHSHIFT_SEMITONE_SHIFT_DEFAULT);
        shifter.WindowSize      = Math.Clamp(shifter.WindowSize,        PitchShift.PITCHSHIFT_WINDOW_SIZE_MIN,          PitchShift.PITCHSHIFT_WINDOW_SIZE_MAX)      .EnsureValidNum(PitchShift.PITCHSHIFT_WINDOW_SIZE_DEFAULT);
        shifter.CrossfadeSamples = Math.Clamp(shifter.CrossfadeSamples,   PitchShift.PITCHSHIFT_SAMPLE_CROSSFADE_MIN,     PitchShift.PITCHSHIFT_SAMPLE_CROSSFADE_MAX) .EnsureValidNum(PitchShift.PITCHSHIFT_SAMPLE_CROSSFADE_DEFAULT);
        #else
        shifter.SemitoneShift   = ParameterHelpers.Clamp(shifter.SemitoneShift,     PitchShift.PITCHSHIFT_SEMITONE_SHIFT_MIN,       PitchShift.PITCHSHIFT_SEMITONE_SHIFT_MAX)   .EnsureValidNum(PitchShift.PITCHSHIFT_SEMITONE_SHIFT_DEFAULT);
        shifter.WindowSize      = ParameterHelpers.Clamp(shifter.WindowSize,        PitchShift.PITCHSHIFT_WINDOW_SIZE_MIN,          PitchShift.PITCHSHIFT_WINDOW_SIZE_MAX)      .EnsureValidNum(PitchShift.PITCHSHIFT_WINDOW_SIZE_DEFAULT);
        shifter.CrossfadeSamples = ParameterHelpers.Clamp(shifter.CrossfadeSamples,   PitchShift.PITCHSHIFT_SAMPLE_CROSSFADE_MIN,     PitchShift.PITCHSHIFT_SAMPLE_CROSSFADE_MAX) .EnsureValidNum(PitchShift.PITCHSHIFT_SAMPLE_CROSSFADE_DEFAULT);
        #endif

        return shifter;
    }
}
