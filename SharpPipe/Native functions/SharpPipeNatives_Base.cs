using System.Runtime.InteropServices;

namespace SharpPipe;

public static partial class SharpPipeNatives
{
    public const int FILENAME_BUFFERLENGTH = 200;

    [DllImport("libsoundpipe")]
    public static unsafe extern int sp_create(ref IntPtr spObject);

    [DllImport("libsoundpipe")]
    public static unsafe extern int sp_destroy(ref IntPtr spObject);
}
