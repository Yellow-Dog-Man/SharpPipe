
using System.Runtime.CompilerServices;
using System.Text;

namespace SharpPipe;

/// <summary>
/// <para>
/// Defines a soundpipe object used for miscellaneous info
/// </para>
/// <para>
/// Usually used for outputting sound files, but is used in some effects for gathering info about
/// how many channels the signal has, what sample rate to use, etc. Its sound file aspects are unused in this wrapper.
/// </para>
/// </summary>
public class SoundPipe : IDisposable
{
    internal IntPtr pipeObject;

    /// <summary>
    /// True if the object is disposed
    /// </summary>
    public bool Disposed { get; private set; }
    internal unsafe ref sp_data Data => ref Unsafe.AsRef<sp_data>(pipeObject.ToPointer());


    /// <summary>
    /// The output sample from this pipe
    /// </summary>
    public unsafe float OutputSample
    {
        get => *Data.outSample;
        set => *Data.outSample = value;
    }

    /// <summary>
    /// The sample rate of the input audio (default: 44100hz)
    /// </summary>
    public int SampleRate
    {
        get => Data.sr;
        set => Data.sr = value;
    }

    /// <summary>
    /// How many channels the input audio has (default: 1)
    /// </summary>
    public int Channels
    {
        get => Data.nchan;
        set => Data.nchan = value;
    }

    /// <summary>
    /// The length of the audio data (UNUSED)
    /// </summary>
    public ulong Length
    {
        get => Data.len;
        set => Data.len = value;
    }

    /// <summary>
    /// The position in the audio data (UNUSED)
    /// </summary>
    public ulong Pos
    {
        get => Data.pos;
        set => Data.pos = value;
    }

    /// <summary>
    /// The file name to output (UNUSED)
    /// </summary>
    public unsafe string FileName // This property looks like puke
    {
        get
        {
            string fullBuffer = Encoding.UTF8.GetString(((sp_data*)pipeObject.ToPointer())->filename, SharpPipeNatives.FILENAME_BUFFERLENGTH);
            int nullIndex = fullBuffer.IndexOf(char.MinValue);
            string terminated = fullBuffer.Substring(0, nullIndex);

            return terminated;
        }
        set
        {
            Span<byte> fileNameSpan = new(((sp_data*)pipeObject.ToPointer())->filename, SharpPipeNatives.FILENAME_BUFFERLENGTH);
            Encoding.UTF8.GetBytes(value + char.MinValue).AsSpan().Slice(0, Math.Min(value.Length + 1, SharpPipeNatives.FILENAME_BUFFERLENGTH)).CopyTo(fileNameSpan);
        }
    }

    /// <summary>
    /// Random unsigned integer (UNUSED)
    /// </summary>
    public uint Random
    {
        get => Data.rand;
        set => Data.rand = value;
    }


    /// <summary>
    /// Creates a new sondpipe output
    /// </summary>
    public SoundPipe()
    {
        SharpPipeNatives.sp_create(ref pipeObject);
    }
    
    
    /// <summary>
    /// Disposes of this unmanaged resource
    /// </summary>
    public void Dispose()
    {
        GC.SuppressFinalize(this);

        Dispose(true);
    }



    private void Dispose(bool disposing)
    {
        Disposed = true;
        SharpPipeNatives.sp_destroy(ref pipeObject);
    }


    /// <summary>
    /// Disposes of any unmanaged resources that ran away
    /// </summary>
    ~SoundPipe()
    {
        Dispose(false);
    }
}
