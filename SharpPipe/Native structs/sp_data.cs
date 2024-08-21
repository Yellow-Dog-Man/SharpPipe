using System.Runtime.InteropServices;

namespace SharpPipe;

/// <summary>
/// Mirrors the memory layout of the unmanaged sp_data struct from soundpipe
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct sp_data
{   
    /// <summary>
    /// Pointer to the output sample
    /// </summary>
    public float* outSample;

    /// <summary>
    /// The sample rate (hz)
    /// </summary>
    public int sr;

    /// <summary>
    /// Number of channels
    /// </summary>
    public int nchan;

    /// <summary>
    /// Length of data
    /// </summary>
    public ulong len;

    /// <summary>
    /// Position in data
    /// </summary>
    public ulong pos;

    /// <summary>
    /// File name byte array
    /// </summary>
    public fixed byte filename[SharpPipeNatives.FILENAME_BUFFERLENGTH];

    /// <summary>
    /// Random uint
    /// </summary>
    public uint rand;
}