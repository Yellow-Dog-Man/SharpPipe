using System.Runtime.InteropServices;

namespace SharpPipe;

/// <summary>
/// <para>
/// Native P/Invoke for creating and destroying <see cref="sp_data"/> structs
/// </para>
/// 
/// <para>
/// Remarks: Soundpipe uses a 'Create -> Init -> Compute -> Destroy' data model for its effects,<br/>
/// which is a very idiomatic way in C to use object-oriented patterns. This makes creating C#<br/>
/// wrappers a breeze.
/// </para>
/// </summary>
public static partial class SharpPipeNatives
{
    /// <summary>
    /// Length of the file name buffer for the native sondpipe object
    /// </summary>
    public const int FILENAME_BUFFERLENGTH = 200;



    /// <summary>
    /// Allocates a <see cref="sp_data"/> struct and assigns its memory address to a pointer passed in by reference
    /// </summary>
    /// <param name="spObject">Reference to a pointer that will point to the allocated struct</param>
    /// <returns>Success code</returns>
    [DllImport("libsoundpipe")]
    public static unsafe extern int sp_create(ref IntPtr spObject);



    /// <summary>
    /// Destroys a native struct implementation of <see cref="sp_data"/> from a reference to its pointer
    /// </summary>
    /// <param name="spObject">Reference to a pointer to the struct to be freed</param>
    /// <returns>Success code</returns>
    [DllImport("libsoundpipe")]
    public static unsafe extern int sp_destroy(ref IntPtr spObject);
}
