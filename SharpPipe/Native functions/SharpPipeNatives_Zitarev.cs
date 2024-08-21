using System.Runtime.InteropServices;

namespace SharpPipe;

public static partial class SharpPipeNatives
{
    /// <summary>
    /// Allocates a <see cref="sp_zitarev"/> struct and assigns its memory address to a pointer passed in by reference
    /// </summary>
    /// <param name="zitaRevObject">Reference to a pointer that will point to the allocated struct</param>
    /// <returns>Success code</returns>
    [DllImport("libsoundpipe")]
    public static unsafe extern int sp_zitarev_create(ref IntPtr zitaRevObject);



    /// <summary>
    /// Inits a <see cref="sp_zitarev"/> struct from a pointer to it
    /// </summary>
    /// <param name="spObject">Pointer to a <see cref="sp_data"/> struct for gathering info</param>
    /// <param name="zitaRevObject">Pointer to a <see cref="sp_zitarev"/> struct</param>
    /// <returns>Success code</returns>
    [DllImport("libsoundpipe")]
    public static unsafe extern int sp_zitarev_init(IntPtr spObject, IntPtr zitaRevObject);



    /// <summary>
    /// Computes a single sample of audio from references to both a left and right channel and assigns the result to the left and right variables passed in by reference
    /// </summary>
    /// <param name="spObject">Pointer to a <see cref="sp_data"/> struct for gathering info</param>
    /// <param name="zitaRevObject">Pointer to a <see cref="sp_zitarev"/> struct</param>
    /// <param name="in1">Reference to the left audio sample</param>
    /// <param name="in2">Reference to the right audio sample</param>
    /// <param name="out1">Reference to the left output audio sample</param>
    /// <param name="out2">Reference to the right output audio sample</param>
    /// <returns>Success code</returns>
    [DllImport("libsoundpipe")]
    public static unsafe extern int sp_zitarev_compute(IntPtr spObject, IntPtr zitaRevObject, ref float in1, ref float in2, [In, Out] ref float out1, [In, Out] ref float out2);



    /// <summary>
    /// Frees a <see cref="sp_zitarev"/> struct from memory
    /// </summary>
    /// <param name="zitaRevObject">Reference to the pointer to the struct to be freed</param>
    /// <returns>Success code</returns>
    [DllImport("libsoundpipe")]
    public static unsafe extern int sp_zitarev_destroy(ref IntPtr zitaRevObject);
}
