using System.Runtime.InteropServices;

namespace SharpPipe;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct sp_data
{
    public float* outSample;
    public int sr;
    public int nchan;
    public ulong len;
    public ulong pos;
    public fixed byte filename[SharpPipeNatives.FILENAME_BUFFERLENGTH];
    public uint rand;
}