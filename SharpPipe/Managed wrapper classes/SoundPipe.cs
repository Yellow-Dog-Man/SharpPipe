
using System.Runtime.CompilerServices;
using System.Text;

namespace SharpPipe;

public class SoundPipe : IDisposable
{
    internal IntPtr pipeObject;
    public bool Disposed { get; private set; }
    internal unsafe ref sp_data Data => ref Unsafe.AsRef<sp_data>(pipeObject.ToPointer());


    public unsafe float OutputSample
    {
        get => *Data.outSample;
        set => *Data.outSample = value;
    }

    public int SampleRate
    {
        get => Data.sr;
        set => Data.sr = value;
    }

    public int Channels
    {
        get => Data.nchan;
        set => Data.nchan = value;
    }

    public ulong Length
    {
        get => Data.len;
        set => Data.len = value;
    }

    public ulong Pos
    {
        get => Data.pos;
        set => Data.pos = value;
    }

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

    public uint Random
    {
        get => Data.rand;
        set => Data.rand = value;
    }


    public SoundPipe()
    {
        SharpPipeNatives.sp_create(ref pipeObject);
    }
    
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);

        Dispose(true);
    }



    public void Dispose(bool disposing)
    {
        Disposed = true;
        SharpPipeNatives.sp_destroy(ref pipeObject);
    }


    ~SoundPipe()
    {
        Dispose(false);
    }
}
