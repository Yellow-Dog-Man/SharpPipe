using System.Runtime.InteropServices;

namespace SharpPipe;

public static partial class SharpPipeNatives
{
    [DllImport("libsoundpipe")]
    public static unsafe extern int sp_zitarev_create(ref IntPtr zitaRevObject);

    [DllImport("libsoundpipe")]
    public static unsafe extern int sp_zitarev_init(IntPtr spObject, IntPtr zitaRevObject);

    [DllImport("libsoundpipe")]
    public static unsafe extern int sp_zitarev_compute(IntPtr spObject, IntPtr zitaRevObject, ref float in1, ref float in2, [In, Out] ref float out1, [In, Out] ref float out2);

    [DllImport("libsoundpipe")]
    public static unsafe extern int sp_zitarev_destroy(ref IntPtr zitaRevObject);
}
