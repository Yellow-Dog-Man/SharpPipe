using System.Runtime.CompilerServices;

namespace SharpPipe;

public class ZitaReverb : IDisposable
{
    internal IntPtr zitaRevObject;
    public SoundPipe Pipe;
    public bool Disposed { get; private set; }
    internal unsafe ref sp_zitarev Data => ref Unsafe.AsRef<sp_zitarev>(zitaRevObject.ToPointer());

    public unsafe float InDelay
    {
        get => *Data.in_delay;
        set => *Data.in_delay = value;
    }

    public unsafe float Crossover
    {
        get => *Data.lf_x;
        set => *Data.lf_x = value;
    }

    public unsafe float RT60Low
    {
        get => *Data.rt60_low;
        set => *Data.rt60_low = value;
    }

    public unsafe float RT60Mid
    {
        get => *Data.rt60_mid;
        set => *Data.rt60_mid = value;
    }

    public unsafe float HighFrequencyDamping
    {
        get => *Data.hf_damping;
        set => *Data.hf_damping = value;
    }

    public unsafe float EQ1Frequency
    {
        get => *Data.eq1_freq;
        set => *Data.eq1_freq = value;
    }

    public unsafe float EQ1Level
    {
        get => *Data.eq1_level;
        set => *Data.eq1_level = value;
    }

    public unsafe float EQ2Frequency
    {
        get => *Data.eq2_freq;
        set => *Data.eq2_freq = value;
    }

    public unsafe float EQ2Level
    {
        get => *Data.eq2_level;
        set => *Data.eq2_level = value;
    }

    public unsafe float Mix
    {
        get => *Data.mix;
        set => *Data.mix = value;
    }

    public unsafe float Level
    {
        get => *Data.level;
        set => *Data.level = value;
    }

    public ZitaReverb(SoundPipe pipe)
    {
        Pipe = pipe;

        SharpPipeNatives.sp_zitarev_create(ref zitaRevObject);
        SharpPipeNatives.sp_zitarev_init(Pipe.pipeObject, zitaRevObject);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Compute(float left, float right, ref float outLeft, ref float outRight)
    {
        SharpPipeNatives.sp_zitarev_compute(Pipe.pipeObject, zitaRevObject, ref left, ref right, ref outLeft, ref outRight);
    }
    
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);

        Dispose(true);
    }



    public void Dispose(bool disposing)
    {
        Disposed = true;
        SharpPipeNatives.sp_zitarev_destroy(ref zitaRevObject);
    }


    ~ZitaReverb()
    {
        Dispose(false);
    }
}