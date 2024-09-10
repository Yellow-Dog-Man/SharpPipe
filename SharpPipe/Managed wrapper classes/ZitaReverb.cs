using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpPipe;


/// <summary>
/// Uses the 8 FDB zitareverb algorithm to provide reverb to processed samples ( see: https://paulbatchelor.github.io/res/soundpipe/docs/zitarev.html )
/// </summary>
public class ZitaReverb : IDisposable
{
    internal IntPtr zitaRevObject;

    /// <summary>
    /// The soundpipe object used by this reverb
    /// </summary>
    public SoundPipe Pipe;

    /// <summary>
    /// True if the object is disposed
    /// </summary>
    public bool Disposed { get; private set; }

    /// <summary>
    /// Gets a reference to the struct controlled by this reverb (internal use only)
    /// </summary>
    internal unsafe ref sp_zitarev Data => ref Unsafe.AsRef<sp_zitarev>(zitaRevObject.ToPointer());

    /// <summary>
    /// The input delay (ms) before reverb is applied to the incoming signal (default: 60ms)
    /// </summary>
    public unsafe float InDelay
    {
        get => *Data.in_delay;
        set => *Data.in_delay = value;
    }

    /// <summary>
    /// The separation between 'high' and 'low' frequencies (default: 200hz)
    /// </summary>
    public unsafe float Crossover
    {
        get => *Data.lf_x;
        set => *Data.lf_x = value;
    }

    /// <summary>
    /// The time in seconds it takes for low frequencies to decrease by 60dB (default: 3 seconds)
    /// </summary>
    public unsafe float RT60Low
    {
        get => *Data.rt60_low;
        set => *Data.rt60_low = value;
    }

    /// <summary>
    /// The time in seconds it takes for mid frequencies to decrease by 60dB (default: 2 seconds)
    /// </summary>
    public unsafe float RT60Mid
    {
        get => *Data.rt60_mid;
        set => *Data.rt60_mid = value;
    }

    /// <summary>
    /// Frequencies (hz) above this one are heard half as long as as the mid-range frequencies - e.g. when their T60 is half of the middle-range's T60 (default: 6000hz)
    /// </summary>
    public unsafe float HighFrequencyDamping
    {
        get => *Data.hf_damping;
        set => *Data.hf_damping = value;
    }

    /// <summary>
    ///  The center frequency (hz) of the Regalia Mitra peaking equalizer for the first section (default: 315hz)
    /// </summary>
    public unsafe float EQ1Frequency
    {
        get => *Data.eq1_freq;
        set => *Data.eq1_freq = value;
    }

    /// <summary>
    /// The peak level (dB) of equalizer 1 (default: 0dB)
    /// </summary>
    public unsafe float EQ1Level
    {
        get => *Data.eq1_level;
        set => *Data.eq1_level = value;
    }

    /// <summary>
    ///  The center frequency (hz) of the Regalia Mitra peaking equalizer for the second section (default: 1500hz)
    /// </summary>
    public unsafe float EQ2Frequency
    {
        get => *Data.eq2_freq;
        set => *Data.eq2_freq = value;
    }

    /// <summary>
    /// The peak level (dB) of equalizer 2 (default: 0dB)
    /// </summary>
    public unsafe float EQ2Level
    {
        get => *Data.eq2_level;
        set => *Data.eq2_level = value;
    }

    /// <summary>
    /// Mixes the wet and dry signals - e.g. 0 is no reverb, 1 is only reverb (default: 1)
    /// </summary>
    public unsafe float Mix
    {
        get => *Data.mix;
        set => *Data.mix = value;
    }

    /// <summary>
    /// The factor (dB) to scale the output by (default: -20dB)
    /// </summary>
    public unsafe float Level
    {
        get => *Data.level;
        set => *Data.level = value;
    }

    /// <summary>
    /// Creates a new ZitaReverb processor using the specified soundpipe object for info (sample rate, etc.)
    /// </summary>
    /// <param name="pipe">The soundpipe object to use</param>
    public ZitaReverb(SoundPipe pipe)
    {
        Pipe = pipe;

        SharpPipeNatives.sp_zitarev_create(ref zitaRevObject);
        SharpPipeNatives.sp_zitarev_init(Pipe.pipeObject, zitaRevObject);
    }


    /// <summary>
    /// Computes reverb on a single audio sample for left and right channels
    /// </summary>
    /// <param name="left">Left audio sample</param>
    /// <param name="right">Right audio sample</param>
    /// <param name="outLeft">Reference to the variable where the result for the left channel will be stored</param>
    /// <param name="outRight">Reference to the variable where the result for the right channel will be stored</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Compute(float left, float right, ref float outLeft, ref float outRight)
    {
        SharpPipeNatives.sp_zitarev_compute(Pipe.pipeObject, zitaRevObject, ref left, ref right, ref outLeft, ref outRight);
    }


    // I don't understand P/Invoke well enough to do this yet and it was eating a lot of time.
    // /// <summary>
    // /// Computes reverb on a span of audio samples and places them into an output buffer
    // /// </summary>
    // /// <param name="stereoIn">Left audio sample</param>
    // /// <param name="stereoOut">Right audio sample</param>
    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // public unsafe void Compute(Span<float> stereoIn, Span<float> stereoOut)
    // {
    //     if (stereoIn.Length != stereoOut.Length)
    //         throw new ArgumentException($"Input and output spans are of inequal length! (stereoIn length: {stereoIn.Length}, stereoOut length: {stereoOut.Length})");


    //     IntPtr inPtr = new(Unsafe.AsPointer(ref stereoIn[0]));
    //     IntPtr outPtr = new(Unsafe.AsPointer(ref stereoOut[0]));

    //     SharpPipeNatives.sp_zitarev_compute_many(Pipe.pipeObject, zitaRevObject, stereoIn.Length, ref inPtr, ref outPtr);
    // }

    
    
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
        SharpPipeNatives.sp_zitarev_destroy(ref zitaRevObject);
    }


    /// <summary>
    /// Disposes of any unmanaged resources that ran away
    /// </summary>
    ~ZitaReverb()
    {
        Dispose(false);
    }
}