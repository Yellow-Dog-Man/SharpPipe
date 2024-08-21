using System.Runtime.InteropServices;

namespace SharpPipe;


/// <summary>
/// Mirrors the memory layout of the unmanaged sp_zitarev struct from soundpipe
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct sp_zitarev
{
    /// <summary>
    /// Pointer to the internal faust instance
    /// </summary>
    void* faust;

    /// <summary>
    /// Position in arguments
    /// </summary>
    public int argpos;

    /// <summary>
    /// Inline array of arguments
    /// </summary>
    internal sp_zitarev_args args;

    /// <summary>
    /// Input delay (ms)
    /// </summary>
    public float* in_delay;

    /// <summary>
    /// Crossover frequency
    /// </summary>
    public float* lf_x;

    /// <summary>
    /// Low-range RT60 (seconds)
    /// </summary>
    public float* rt60_low;

    /// <summary>
    /// Mid-range RT60 (seconds)
    /// </summary>
    public float* rt60_mid;

    /// <summary>
    /// High-frequency damping (hz)
    /// </summary>
    public float* hf_damping;

    /// <summary>
    /// Frequency of EQ1 (hz)
    /// </summary>
    public float* eq1_freq;

    /// <summary>
    /// Peak level of EQ1 (dB)
    /// </summary>
    public float* eq1_level;

    /// <summary>
    /// Frequency of EQ2 (hz)
    /// </summary>
    public float* eq2_freq;

    /// <summary>
    /// Peak level of EQ2 (dB)
    /// </summary>
    public float* eq2_level;

    /// <summary>
    /// Wet/Dry mix
    /// </summary>
    public float* mix;

    /// <summary>
    /// Output level attenuation (dB)
    /// </summary>
    public float* level;
}


#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// Array struct because P/Invoke is a pain in the butt and doesn't let you do fixed buffers of pointers >:(
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct sp_zitarev_args
{
    public float* arg0;
    public float* arg1;
    public float* arg2;
    public float* arg3;
    public float* arg4;
    public float* arg5;
    public float* arg6;
    public float* arg7;
    public float* arg8;
    public float* arg9;
    public float* arg10;
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member