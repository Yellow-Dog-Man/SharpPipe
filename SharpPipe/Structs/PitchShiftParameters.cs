using SharpPipe;

/// <summary>
/// Parameter struct that implements the pitch shift filter interface
/// </summary>
public struct PitchShiftParameters : IPitchShiftFilter
{
    /// <inheritdoc/>
    public float SemitoneShift { get; set; }

    /// <inheritdoc/>
    public float WindowSize { get; set; }

    /// <inheritdoc/>
    public float CrossfadeSamples { get; set; }
}