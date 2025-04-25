using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpPipe;


/// <summary>
/// Uses the 8 FDB zitareverb algorithm to provide reverb to processed samples ( see: https://paulbatchelor.github.io/res/soundpipe/docs/zitarev.html )
/// </summary>
public class ZitaReverb : IDisposable, IZitaFilter
{
    #region Default Values
    #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public const float ZITA_IN_DELAY_DEFAULT = 60f;
    public const float ZITA_CROSSOVER_DEFAULT = 200f;
    public const float ZITA_RT60LOW_DEFAULT = 3f;
    public const float ZITA_RT60MID_DEFAULT = 2f;
    public const float ZITA_HIGH_FREQUENCY_DAMPING_DEFAULT = 6000f;
    public const float ZITA_EQ1_FREQUENCY_DEFAULT = 315f;
    public const float ZITA_EQ1_LEVEL_DEFAULT = 0f;
    public const float ZITA_EQ2_FREQUENCY_DEFAULT = 1500f;
    public const float ZITA_EQ2_LEVEL_DEFAULT = 0f;
    public const float ZITA_MIX_DEFAULT = 1f;
    public const float ZITA_LEVEL_DEFAULT = 0f;
    #endregion

    #region Min Values
    public const float ZITA_IN_DELAY_MIN = 0f;
    public const float ZITA_CROSSOVER_MIN = 20f;
    public const float ZITA_RT60LOW_MIN = 0f;
    public const float ZITA_RT60MID_MIN = 0f;
    public const float ZITA_HIGH_FREQUENCY_DAMPING_MIN = 20f;
    public const float ZITA_EQ1_FREQUENCY_MIN = 20f;
    public const float ZITA_EQ1_LEVEL_MIN = -90f;
    public const float ZITA_EQ2_FREQUENCY_MIN = 20f;
    public const float ZITA_EQ2_LEVEL_MIN = -90f;
    public const float ZITA_MIX_MIN = 0f;
    public const float ZITA_LEVEL_MIN = -90f;
    #endregion

    #region Max Values
    public const float ZITA_IN_DELAY_MAX = 900f;
    public const float ZITA_CROSSOVER_MAX = 20000f;
    public const float ZITA_RT60LOW_MAX = 30f;
    public const float ZITA_RT60MID_MAX = 30f;
    public const float ZITA_HIGH_FREQUENCY_DAMPING_MAX = 20000f;
    public const float ZITA_EQ1_FREQUENCY_MAX = 20000f;
    public const float ZITA_EQ1_LEVEL_MAX = 20f;
    public const float ZITA_EQ2_FREQUENCY_MAX = 20000f;
    public const float ZITA_EQ2_LEVEL_MAX = 20f;
    public const float ZITA_MIX_MAX = 1f;
    public const float ZITA_LEVEL_MAX = 20f;
    #pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    #endregion


    internal IntPtr zitaRevObject;

    /// <summary>
    /// The soundpipe object used by this reverb
    /// </summary>
    public SoundPipe Pipe { get; }

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
    /// Computes reverb on a single audio sample for left and right channels.
    /// </summary>
    /// <param name="left">Left audio sample</param>
    /// <param name="right">Right audio sample</param>
    /// <param name="outLeft">Reference to the variable where the result for the left channel will be stored</param>
    /// <param name="outRight">Reference to the variable where the result for the right channel will be stored</param>
    [Obsolete("Use the span-accepting overload to avoid significant performance penalties.")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Compute(float left, float right, ref float outLeft, ref float outRight)
    {
        SharpPipeNatives.sp_zitarev_compute(Pipe.pipeObject, zitaRevObject, ref left, ref right, ref outLeft, ref outRight);
    }


    /// <summary>
    /// Computes reverb from an arbitrarily-sized span of stereo samples and places them into an output buffer. Processes the signal in chunks.
    /// <para>1 stereo sample == 2 floats. E.g. A chunkSize of 1024 will process 2048 floats at once.</para>
    /// </summary>
    /// <param name="stereoIn">Interleaved input stereo signal.</param>
    /// <param name="stereoOut">Interleaved output stereo signal.</param>
    /// <param name="chunkSize">The size of each chunk to be processed.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void Compute(Span<float> stereoIn, Span<float> stereoOut, int chunkSize = 1024)
    {
        if (stereoIn.Length > stereoOut.Length)
            throw new ArgumentOutOfRangeException(nameof(stereoIn), $"The input span is larger than {nameof(stereoOut)}!");

        int length = stereoIn.Length;
        int stereoBlockSize = Math.Abs(chunkSize) * 2; // Two channels.

        // Process a batch of samples at once.
        for (int i = 0; i < length;)
        {
            // Get the length to process. This accounts for when you're close to the end of input buffer.
            int remainingLength = length - i;
            int lengthToProcess = Math.Min(remainingLength, stereoBlockSize);

            // Slice the input into a chunk that can be computed.
            Span<float> inputSliced = stereoIn.Slice(i, lengthToProcess);

            // Slice the output into an equivalent chunk.
            Span<float> outputSliced = stereoOut.Slice(i, lengthToProcess);

            // Compute a N blocks of data depending on the length of the input data.
            ComputeBlock(inputSliced, outputSliced);

            // Increment the index by the amount of data that was just processed.
            i += lengthToProcess;
        }
    }



    /// <summary>
    /// Computes reverb on a span of audio samples and places them into an output buffer. Processes a single chunk of samples. Does not do bounds checking.
    /// <para>
    /// NOTE: This reorients the interleaved samples from left/right/left/right into two separate left and right
    /// buffers on the stack. Be mindful of how big your buffer size is with this function. If you want to process an arbitrary
    /// amount of samples, use: <see cref="Compute(Span{float}, Span{float}, int)"/>
    /// </para>
    /// </summary>
    /// <param name="stereoIn">Interleaved input stereo signal.</param>
    /// <param name="stereoOut">Interleaved output stereo signal.</param>
    public unsafe void ComputeBlock(Span<float> stereoIn, Span<float> stereoOut)
    {
        int halfLength = stereoIn.Length / 2;
        Span<float> inLeft = stackalloc float[halfLength];
        Span<float> inRight = stackalloc float[halfLength];

        for (int i = 0; i < halfLength; i++)
        {
            inLeft[i] = stereoIn[i * 2];
            inRight[i] = stereoIn[i * 2 + 1];
        }

        Span<nint> inSamples = stackalloc nint[2];
        unsafe
        {
            fixed (float* inLeftPtr = inLeft)
            fixed (float* inRightPtr = inRight)
            fixed (nint* inSamplesPtr = inSamples)
            {
                inSamplesPtr[0] = (nint)inLeftPtr;
                inSamplesPtr[1] = (nint)inRightPtr;

                SharpPipeNatives.sp_zitarev_compute_many(Pipe.pipeObject, zitaRevObject, halfLength, ref *inSamplesPtr, ref *inSamplesPtr);
            }
        }


        for (int i = 0; i < halfLength; i++)
        {
            stereoOut[i * 2] = inLeft[i];
            stereoOut[i * 2 + 1] = inRight[i];
        }
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