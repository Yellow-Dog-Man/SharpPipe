using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpPipe;


/// <summary>
/// Uses the 8 FDB zitareverb algorithm to provide reverb to processed samples ( see: https://paulbatchelor.github.io/res/soundpipe/docs/zitarev.html )
/// </summary>
public sealed class ZitaReverb : IDisposable, IZitaFilter
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
    public const float ZITA_RT60LOW_MIN = 0.05f;
    public const float ZITA_RT60MID_MIN = 0.05f;
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
    public const float ZITA_RT60LOW_MAX = 40f;
    public const float ZITA_RT60MID_MAX = 40f;
    public const float ZITA_HIGH_FREQUENCY_DAMPING_MAX = 20000f;
    public const float ZITA_EQ1_FREQUENCY_MAX = 20000f;
    public const float ZITA_EQ1_LEVEL_MAX = 45f;
    public const float ZITA_EQ2_FREQUENCY_MAX = 20000f;
    public const float ZITA_EQ2_LEVEL_MAX = 45f;
    public const float ZITA_MIX_MAX = 1f;
    public const float ZITA_LEVEL_MAX = 45f;
    #pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    #endregion


    /// <summary>
    /// The soundpipe object used by this reverb
    /// </summary>
    public SoundPipe Pipe { get; }

    /// <summary>
    /// True if the object is disposed
    /// </summary>
    public bool Disposed { get; private set; }


    /// <summary>
    /// The input delay (ms) before reverb is applied to the incoming signal (default: 60ms)
    /// </summary>
    public float InDelay
    {
        get => _impl.InDelay;
        set => _impl.InDelay = value;
    }

    /// <summary>
    /// The separation between 'high' and 'low' frequencies (default: 200hz)
    /// </summary>
    public float Crossover
    {
        get => _impl.Crossover;
        set => _impl.Crossover = value;
    }

    /// <summary>
    /// The time in seconds it takes for low frequencies to decrease by 60dB (default: 3 seconds)
    /// </summary>
    public float RT60Low
    {
        get => _impl.RT60Low;
        set => _impl.RT60Low = value;
    }

    /// <summary>
    /// The time in seconds it takes for mid frequencies to decrease by 60dB (default: 2 seconds)
    /// </summary>
    public float RT60Mid
    {
        get => _impl.RT60Mid;
        set => _impl.RT60Mid = value;
    }

    /// <summary>
    /// Frequencies (hz) above this one are heard half as long as as the mid-range frequencies - e.g. when their T60 is half of the middle-range's T60 (default: 6000hz)
    /// </summary>
    public float HighFrequencyDamping
    {
        get => _impl.HighFrequencyDamping;
        set => _impl.HighFrequencyDamping = value;
    }

    /// <summary>
    ///  The center frequency (hz) of the Regalia Mitra peaking equalizer for the first section (default: 315hz)
    /// </summary>
    public float EQ1Frequency
    {
        get => _impl.EQ1Frequency;
        set => _impl.EQ1Frequency = value;
    }

    /// <summary>
    /// The peak level (dB) of equalizer 1 (default: 0dB)
    /// </summary>
    public float EQ1Level
    {
        get => _impl.EQ1Level;
        set => _impl.EQ1Level = value;
    }

    /// <summary>
    ///  The center frequency (hz) of the Regalia Mitra peaking equalizer for the second section (default: 1500hz)
    /// </summary>
    public float EQ2Frequency
    {
        get => _impl.EQ2Frequency;
        set => _impl.EQ2Frequency = value;
    }

    /// <summary>
    /// The peak level (dB) of equalizer 2 (default: 0dB)
    /// </summary>
    public float EQ2Level
    {
        get => _impl.EQ2Level;
        set => _impl.EQ2Level = value;
    }

    /// <summary>
    /// Mixes the wet and dry signals - e.g. 0 is no reverb, 1 is only reverb (default: 1)
    /// </summary>
    public float Mix
    {
        get => 1f - (_impl.Mix * 0.5f + 0.5f);
        set => _impl.Mix = (1f - value) * 2f - 1f;
    }

    /// <summary>
    /// The factor (dB) to scale the output by (default: -20dB)
    /// </summary>
    public float Level
    {
        get => _impl.Level;
        set => _impl.Level = value;
    }

    private readonly ZitaRev _impl;

    /// <summary>
    /// Creates a new ZitaReverb processor using the specified soundpipe object for info (sample rate, etc.)
    /// </summary>
    /// <param name="pipe">The soundpipe object to use</param>
    public unsafe ZitaReverb(SoundPipe pipe)
    {
        Pipe = pipe;

        _impl = new(pipe.SampleRate);
    }


    /// <summary>
    /// Computes reverb from an arbitrarily-sized span of stereo samples and places them into an output buffer. Processes the signal in chunks.
    /// <para>1 stereo sample == 2 floats. E.g. A chunkSize of 1024 will process 2048 floats at once.</para>
    /// </summary>
    /// <param name="stereoIn">Interleaved input stereo signal.</param>
    /// <param name="stereoOut">Interleaved output stereo signal.</param>
    /// <param name="chunkSize">The size of each chunk to be processed.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Compute(Span<float> stereoIn, Span<float> stereoOut, int chunkSize = 1024)
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
    public void ComputeBlock(Span<float> stereoIn, Span<float> stereoOut)
    {
        int halfLength = stereoIn.Length / 2;
        Span<float> inLeft = stackalloc float[halfLength];
        Span<float> inRight = stackalloc float[halfLength];

        for (int i = 0; i < halfLength; i++)
        {
            inLeft[i] = stereoIn[i * 2];
            inRight[i] = stereoIn[i * 2 + 1];
        }

        _impl.Compute(halfLength, inLeft, inRight, inLeft, inRight);


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


    private unsafe void Dispose(bool disposing)
    {
        Disposed = true;
        _impl.Clear();
    }


    /// <summary>
    /// Disposes of any unmanaged resources that ran away
    /// </summary>
    ~ZitaReverb()
    {
        Dispose(false);
    }
}


/* ------------------------------------------------------------
author: "JOS, Revised by RM"
name: "zitaRev"
version: "0.0"
Code generated with Faust 2.79.3 (https://faust.grame.fr)
Compilation options: -lang csharp -ct 1 -es 1 -mcd 16 -mdd 1024 -mdy 33 -single -ftz 0
------------------------------------------------------------ */

// NOTE: Organized & edited by Cyro.



sealed class ZitaRev
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	float mydsp_faustpower2_f(float value)
	{
		return value * value;
	}

	
	public float InDelay;
	public float Crossover;
	public float HighFrequencyDamping;
	public float RT60Mid;
	public float RT60Low;
	public float EQ1Level;
	public float EQ1Frequency;
	public float EQ2Level;
	public float EQ2Frequency;
	public float Mix;
	public float Level;
	
    readonly int fSampleRate;

	readonly float fConst0;
	readonly float fConst1;
	readonly float fConst2;
	readonly float fConst3;
	readonly float fConst4;
	readonly float fConst5;
	readonly float fConst7;
	readonly float fConst9;
	readonly float fConst10;
	readonly float fConst11;
	readonly float fConst14;
	readonly float fConst15;
	readonly float fConst16;
	readonly float fConst19;
	readonly float fConst20;
	readonly float fConst21;
	readonly float fConst24;
	readonly float fConst25;
	readonly float fConst26;
	readonly float fConst29;
	readonly float fConst30;
	readonly float fConst31;
	readonly float fConst34;
	readonly float fConst35;
	readonly float fConst36;
	readonly float fConst39;
	readonly float fConst40;
	readonly float fConst41;
	readonly float fConst44;
	readonly float fConst45;


	readonly int iConst6;
	readonly int iConst8;
	readonly int iConst12;
	readonly int iConst13;
	readonly int iConst17;
	readonly int iConst18;
	readonly int iConst22;
	readonly int iConst23;
	readonly int iConst27;
	readonly int iConst28;
	readonly int iConst32;
	readonly int iConst33;
	readonly int iConst37;
	readonly int iConst38;
	readonly int iConst42;
	readonly int iConst43;



    readonly float[] fRec13 = new float[2];
    readonly float[] fRec12 = new float[2];
    readonly float[] fVec0 = new float[16384];
    readonly float[] fVec1 = new float[16384];
    readonly float[] fVec2 = new float[4096];
    readonly float[] fRec10 = new float[2];
    readonly float[] fRec17 = new float[2];
    readonly float[] fRec16 = new float[2];
    readonly float[] fVec3 = new float[16384];
    readonly float[] fVec4 = new float[2048];
    readonly float[] fRec14 = new float[2];
    readonly float[] fRec21 = new float[2];
    readonly float[] fRec20 = new float[2];
    readonly float[] fVec5 = new float[16384];
    readonly float[] fVec6 = new float[4096];
    readonly float[] fRec18 = new float[2];
    readonly float[] fRec25 = new float[2];
    readonly float[] fRec24 = new float[2];
    readonly float[] fVec7 = new float[16384];
    readonly float[] fVec8 = new float[2048];
    readonly float[] fRec22 = new float[2];
    readonly float[] fRec29 = new float[2];
    readonly float[] fRec28 = new float[2];
    readonly float[] fVec9 = new float[32768];
    readonly float[] fVec10 = new float[16384];
    readonly float[] fVec11 = new float[4096];
    readonly float[] fRec26 = new float[2];
    readonly float[] fRec33 = new float[2];
    readonly float[] fRec32 = new float[2];
    readonly float[] fVec12 = new float[16384];
    readonly float[] fVec13 = new float[4096];
    readonly float[] fRec30 = new float[2];
    readonly float[] fRec37 = new float[2];
    readonly float[] fRec36 = new float[2];
    readonly float[] fVec14 = new float[32768];
    readonly float[] fVec15 = new float[4096];
    readonly float[] fRec34 = new float[2];
    readonly float[] fRec41 = new float[2];
    readonly float[] fRec40 = new float[2];
    readonly float[] fVec16 = new float[32768];
    readonly float[] fVec17 = new float[2048];
    readonly float[] fRec38 = new float[2];
    readonly float[] fRec2 = new float[3];
    readonly float[] fRec3 = new float[3];
    readonly float[] fRec4 = new float[3];
    readonly float[] fRec5 = new float[3];
    readonly float[] fRec6 = new float[3];
    readonly float[] fRec7 = new float[3];
    readonly float[] fRec8 = new float[3];
    readonly float[] fRec9 = new float[3];
    readonly float[] fRec1 = new float[3];
    readonly float[] fRec0 = new float[3];
    readonly float[] fRec42 = new float[2];
    readonly float[] fRec43 = new float[2];
    readonly float[] fRec45 = new float[3];
    readonly float[] fRec44 = new float[3];


	int IOTA0;
	
	public ZitaRev(int sample_rate)
	{
		fSampleRate = sample_rate;

		fConst0 = (float)Math.Min(1.92e+05f, (float)Math.Max(1.0f, (float)(fSampleRate)));
		fConst1 = 6.2831855f / fConst0;
		fConst2 = (float)Math.Floor(0.174713f * fConst0 + 0.5f);
		fConst3 = 6.9077554f * (fConst2 / fConst0);
		fConst4 = 3.1415927f / fConst0;
		fConst5 = (float)Math.Floor(0.022904f * fConst0 + 0.5f);
		iConst6 = (int)((float)Math.Min(8192.0f, (float)Math.Max(0.0f, fConst2 - fConst5)));
		fConst7 = 0.001f * fConst0;
		iConst8 = (int)((float)Math.Min(2048.0f, (float)Math.Max(0.0f, fConst5 + -1.0f)));
		fConst9 = (float)Math.Floor(0.153129f * fConst0 + 0.5f);
		fConst10 = 6.9077554f * (fConst9 / fConst0);
		fConst11 = (float)Math.Floor(0.020346f * fConst0 + 0.5f);
		iConst12 = (int)((float)Math.Min(8192.0f, (float)Math.Max(0.0f, fConst9 - fConst11)));
		iConst13 = (int)((float)Math.Min(1024.0f, (float)Math.Max(0.0f, fConst11 + -1.0f)));
		fConst14 = (float)Math.Floor(0.127837f * fConst0 + 0.5f);
		fConst15 = 6.9077554f * (fConst14 / fConst0);
		fConst16 = (float)Math.Floor(0.031604f * fConst0 + 0.5f);
		iConst17 = (int)((float)Math.Min(8192.0f, (float)Math.Max(0.0f, fConst14 - fConst16)));
		iConst18 = (int)((float)Math.Min(2048.0f, (float)Math.Max(0.0f, fConst16 + -1.0f)));
		fConst19 = (float)Math.Floor(0.125f * fConst0 + 0.5f);
		fConst20 = 6.9077554f * (fConst19 / fConst0);
		fConst21 = (float)Math.Floor(0.013458f * fConst0 + 0.5f);
		iConst22 = (int)((float)Math.Min(8192.0f, (float)Math.Max(0.0f, fConst19 - fConst21)));
		iConst23 = (int)((float)Math.Min(1024.0f, (float)Math.Max(0.0f, fConst21 + -1.0f)));
		fConst24 = (float)Math.Floor(0.210389f * fConst0 + 0.5f);
		fConst25 = 6.9077554f * (fConst24 / fConst0);
		fConst26 = (float)Math.Floor(0.024421f * fConst0 + 0.5f);
		iConst27 = (int)((float)Math.Min(16384.0f, (float)Math.Max(0.0f, fConst24 - fConst26)));
		iConst28 = (int)((float)Math.Min(2048.0f, (float)Math.Max(0.0f, fConst26 + -1.0f)));
		fConst29 = (float)Math.Floor(0.192303f * fConst0 + 0.5f);
		fConst30 = 6.9077554f * (fConst29 / fConst0);
		fConst31 = (float)Math.Floor(0.029291f * fConst0 + 0.5f);
		iConst32 = (int)((float)Math.Min(8192.0f, (float)Math.Max(0.0f, fConst29 - fConst31)));
		iConst33 = (int)((float)Math.Min(2048.0f, (float)Math.Max(0.0f, fConst31 + -1.0f)));
		fConst34 = (float)Math.Floor(0.256891f * fConst0 + 0.5f);
		fConst35 = 6.9077554f * (fConst34 / fConst0);
		fConst36 = (float)Math.Floor(0.027333f * fConst0 + 0.5f);
		iConst37 = (int)((float)Math.Min(16384.0f, (float)Math.Max(0.0f, fConst34 - fConst36)));
		iConst38 = (int)((float)Math.Min(2048.0f, (float)Math.Max(0.0f, fConst36 + -1.0f)));
		fConst39 = (float)Math.Floor(0.219991f * fConst0 + 0.5f);
		fConst40 = 6.9077554f * (fConst39 / fConst0);
		fConst41 = (float)Math.Floor(0.019123f * fConst0 + 0.5f);
		iConst42 = (int)((float)Math.Min(16384.0f, (float)Math.Max(0.0f, fConst39 - fConst41)));
		iConst43 = (int)((float)Math.Min(1024.0f, (float)Math.Max(0.0f, fConst41 + -1.0f)));
		fConst44 = 44.1f / fConst0;
		fConst45 = 1.0f - fConst44;


		ResetParameters();
		Clear();
	}
	
	public void ResetParameters()
	{
		EQ2Level = (float)(0.0f);
		EQ2Frequency = (float)(1.5e+03f);
		EQ1Level = (float)(0.0f);
		EQ1Frequency = (float)(315.0f);
		RT60Mid = (float)(2.0f);
		HighFrequencyDamping = (float)(6e+03f);
		Crossover = (float)(2e+02f);
		RT60Low = (float)(3.0f);
		InDelay = (float)(6e+01f);
		Mix = (float)(0.0f);
		Level = (float)(-2e+01f);
	}
	
	public void Clear()
	{
		for (int l0 = 0; l0 < 2; l0++)
		{
			fRec13[l0] = 0.0f;
		}
		for (int l1 = 0; l1 < 2; l1++)
		{
			fRec12[l1] = 0.0f;
		}
		IOTA0 = 0;
		for (int l2 = 0; l2 < 16384; l2++)
		{
			fVec0[l2] = 0.0f;
		}
		for (int l3 = 0; l3 < 16384; l3++)
		{
			fVec1[l3] = 0.0f;
		}
		for (int l4 = 0; l4 < 4096; l4++)
		{
			fVec2[l4] = 0.0f;
		}
		for (int l5 = 0; l5 < 2; l5++)
		{
			fRec10[l5] = 0.0f;
		}
		for (int l6 = 0; l6 < 2; l6++)
		{
			fRec17[l6] = 0.0f;
		}
		for (int l7 = 0; l7 < 2; l7++)
		{
			fRec16[l7] = 0.0f;
		}
		for (int l8 = 0; l8 < 16384; l8++)
		{
			fVec3[l8] = 0.0f;
		}
		for (int l9 = 0; l9 < 2048; l9++)
		{
			fVec4[l9] = 0.0f;
		}
		for (int l10 = 0; l10 < 2; l10++)
		{
			fRec14[l10] = 0.0f;
		}
		for (int l11 = 0; l11 < 2; l11++)
		{
			fRec21[l11] = 0.0f;
		}
		for (int l12 = 0; l12 < 2; l12++)
		{
			fRec20[l12] = 0.0f;
		}
		for (int l13 = 0; l13 < 16384; l13++)
		{
			fVec5[l13] = 0.0f;
		}
		for (int l14 = 0; l14 < 4096; l14++)
		{
			fVec6[l14] = 0.0f;
		}
		for (int l15 = 0; l15 < 2; l15++)
		{
			fRec18[l15] = 0.0f;
		}
		for (int l16 = 0; l16 < 2; l16++)
		{
			fRec25[l16] = 0.0f;
		}
		for (int l17 = 0; l17 < 2; l17++)
		{
			fRec24[l17] = 0.0f;
		}
		for (int l18 = 0; l18 < 16384; l18++)
		{
			fVec7[l18] = 0.0f;
		}
		for (int l19 = 0; l19 < 2048; l19++)
		{
			fVec8[l19] = 0.0f;
		}
		for (int l20 = 0; l20 < 2; l20++)
		{
			fRec22[l20] = 0.0f;
		}
		for (int l21 = 0; l21 < 2; l21++)
		{
			fRec29[l21] = 0.0f;
		}
		for (int l22 = 0; l22 < 2; l22++)
		{
			fRec28[l22] = 0.0f;
		}
		for (int l23 = 0; l23 < 32768; l23++)
		{
			fVec9[l23] = 0.0f;
		}
		for (int l24 = 0; l24 < 16384; l24++)
		{
			fVec10[l24] = 0.0f;
		}
		for (int l25 = 0; l25 < 4096; l25++)
		{
			fVec11[l25] = 0.0f;
		}
		for (int l26 = 0; l26 < 2; l26++)
		{
			fRec26[l26] = 0.0f;
		}
		for (int l27 = 0; l27 < 2; l27++)
		{
			fRec33[l27] = 0.0f;
		}
		for (int l28 = 0; l28 < 2; l28++)
		{
			fRec32[l28] = 0.0f;
		}
		for (int l29 = 0; l29 < 16384; l29++)
		{
			fVec12[l29] = 0.0f;
		}
		for (int l30 = 0; l30 < 4096; l30++)
		{
			fVec13[l30] = 0.0f;
		}
		for (int l31 = 0; l31 < 2; l31++)
		{
			fRec30[l31] = 0.0f;
		}
		for (int l32 = 0; l32 < 2; l32++)
		{
			fRec37[l32] = 0.0f;
		}
		for (int l33 = 0; l33 < 2; l33++)
		{
			fRec36[l33] = 0.0f;
		}
		for (int l34 = 0; l34 < 32768; l34++)
		{
			fVec14[l34] = 0.0f;
		}
		for (int l35 = 0; l35 < 4096; l35++)
		{
			fVec15[l35] = 0.0f;
		}
		for (int l36 = 0; l36 < 2; l36++)
		{
			fRec34[l36] = 0.0f;
		}
		for (int l37 = 0; l37 < 2; l37++)
		{
			fRec41[l37] = 0.0f;
		}
		for (int l38 = 0; l38 < 2; l38++)
		{
			fRec40[l38] = 0.0f;
		}
		for (int l39 = 0; l39 < 32768; l39++)
		{
			fVec16[l39] = 0.0f;
		}
		for (int l40 = 0; l40 < 2048; l40++)
		{
			fVec17[l40] = 0.0f;
		}
		for (int l41 = 0; l41 < 2; l41++)
		{
			fRec38[l41] = 0.0f;
		}
		for (int l42 = 0; l42 < 3; l42++)
		{
			fRec2[l42] = 0.0f;
		}
		for (int l43 = 0; l43 < 3; l43++)
		{
			fRec3[l43] = 0.0f;
		}
		for (int l44 = 0; l44 < 3; l44++)
		{
			fRec4[l44] = 0.0f;
		}
		for (int l45 = 0; l45 < 3; l45++)
		{
			fRec5[l45] = 0.0f;
		}
		for (int l46 = 0; l46 < 3; l46++)
		{
			fRec6[l46] = 0.0f;
		}
		for (int l47 = 0; l47 < 3; l47++)
		{
			fRec7[l47] = 0.0f;
		}
		for (int l48 = 0; l48 < 3; l48++)
		{
			fRec8[l48] = 0.0f;
		}
		for (int l49 = 0; l49 < 3; l49++)
		{
			fRec9[l49] = 0.0f;
		}
		for (int l50 = 0; l50 < 3; l50++)
		{
			fRec1[l50] = 0.0f;
		}
		for (int l51 = 0; l51 < 3; l51++)
		{
			fRec0[l51] = 0.0f;
		}
		for (int l52 = 0; l52 < 2; l52++)
		{
			fRec42[l52] = 0.0f;
		}
		for (int l53 = 0; l53 < 2; l53++)
		{
			fRec43[l53] = 0.0f;
		}
		for (int l54 = 0; l54 < 3; l54++)
		{
			fRec45[l54] = 0.0f;
		}
		for (int l55 = 0; l55 < 3; l55++)
		{
			fRec44[l55] = 0.0f;
		}
	}
	
	public void Compute(int count, Span<float> leftInputs, Span<float> rightInputs, Span<float> leftOutputs, Span<float> rightOutputs)
	{
		Span<float> input0 = leftInputs;
		Span<float> input1 = rightInputs;
		Span<float> output0 = leftOutputs;
		Span<float> output1 = rightOutputs;
		float fSlow0 = (float)Math.Pow(1e+01f, 0.05f * (float)(EQ2Level));
		float fSlow1 = (float)(EQ2Frequency);
		float fSlow2 = fConst1 * (fSlow1 / (float)Math.Sqrt((float)Math.Max(0.0f, fSlow0)));
		float fSlow3 = (1.0f - fSlow2) / (fSlow2 + 1.0f);
		float fSlow4 = (float)Math.Cos(fConst1 * fSlow1) * (fSlow3 + 1.0f);
		float fSlow5 = (float)Math.Pow(1e+01f, 0.05f * (float)(EQ1Level));
		float fSlow6 = (float)(EQ1Frequency);
		float fSlow7 = fConst1 * (fSlow6 / (float)Math.Sqrt((float)Math.Max(0.0f, fSlow5)));
		float fSlow8 = (1.0f - fSlow7) / (fSlow7 + 1.0f);
		float fSlow9 = (float)Math.Cos(fConst1 * fSlow6) * (fSlow8 + 1.0f);
		float fSlow10 = (float)(RT60Mid);
		float fSlow11 = (float)Math.Exp(-(fConst3 / fSlow10));
		float fSlow12 = mydsp_faustpower2_f(fSlow11);
		float fSlow13 = 1.0f - fSlow12;
		float fSlow14 = (float)Math.Cos(fConst1 * (float)(HighFrequencyDamping));
		float fSlow15 = 1.0f - fSlow14 * fSlow12;
		float fSlow16 = (float)Math.Sqrt((float)Math.Max(0.0f, mydsp_faustpower2_f(fSlow15) / mydsp_faustpower2_f(fSlow13) + -1.0f));
		float fSlow17 = fSlow15 / fSlow13;
		float fSlow18 = fSlow17 - fSlow16;
		float fSlow19 = 1.0f / (float)Math.Tan(fConst4 * (float)(Crossover));
		float fSlow20 = 1.0f - fSlow19;
		float fSlow21 = 1.0f / (fSlow19 + 1.0f);
		float fSlow22 = (float)(RT60Low);
		float fSlow23 = (float)Math.Exp(-(fConst3 / fSlow22)) / fSlow11 + -1.0f;
		float fSlow24 = fSlow11 * (fSlow16 + (1.0f - fSlow17));
		int iSlow25 = (int)((float)Math.Min(8192.0f, (float)Math.Max(0.0f, fConst7 * (float)(InDelay))));
		float fSlow26 = (float)Math.Exp(-(fConst10 / fSlow10));
		float fSlow27 = mydsp_faustpower2_f(fSlow26);
		float fSlow28 = 1.0f - fSlow27;
		float fSlow29 = 1.0f - fSlow27 * fSlow14;
		float fSlow30 = (float)Math.Sqrt((float)Math.Max(0.0f, mydsp_faustpower2_f(fSlow29) / mydsp_faustpower2_f(fSlow28) + -1.0f));
		float fSlow31 = fSlow29 / fSlow28;
		float fSlow32 = fSlow31 - fSlow30;
		float fSlow33 = (float)Math.Exp(-(fConst10 / fSlow22)) / fSlow26 + -1.0f;
		float fSlow34 = fSlow26 * (fSlow30 + (1.0f - fSlow31));
		float fSlow35 = (float)Math.Exp(-(fConst15 / fSlow10));
		float fSlow36 = mydsp_faustpower2_f(fSlow35);
		float fSlow37 = 1.0f - fSlow36;
		float fSlow38 = 1.0f - fSlow14 * fSlow36;
		float fSlow39 = (float)Math.Sqrt((float)Math.Max(0.0f, mydsp_faustpower2_f(fSlow38) / mydsp_faustpower2_f(fSlow37) + -1.0f));
		float fSlow40 = fSlow38 / fSlow37;
		float fSlow41 = fSlow40 - fSlow39;
		float fSlow42 = (float)Math.Exp(-(fConst15 / fSlow22)) / fSlow35 + -1.0f;
		float fSlow43 = fSlow35 * (fSlow39 + (1.0f - fSlow40));
		float fSlow44 = (float)Math.Exp(-(fConst20 / fSlow10));
		float fSlow45 = mydsp_faustpower2_f(fSlow44);
		float fSlow46 = 1.0f - fSlow45;
		float fSlow47 = 1.0f - fSlow14 * fSlow45;
		float fSlow48 = (float)Math.Sqrt((float)Math.Max(0.0f, mydsp_faustpower2_f(fSlow47) / mydsp_faustpower2_f(fSlow46) + -1.0f));
		float fSlow49 = fSlow47 / fSlow46;
		float fSlow50 = fSlow49 - fSlow48;
		float fSlow51 = (float)Math.Exp(-(fConst20 / fSlow22)) / fSlow44 + -1.0f;
		float fSlow52 = fSlow44 * (fSlow48 + (1.0f - fSlow49));
		float fSlow53 = (float)Math.Exp(-(fConst25 / fSlow10));
		float fSlow54 = mydsp_faustpower2_f(fSlow53);
		float fSlow55 = 1.0f - fSlow54;
		float fSlow56 = 1.0f - fSlow14 * fSlow54;
		float fSlow57 = (float)Math.Sqrt((float)Math.Max(0.0f, mydsp_faustpower2_f(fSlow56) / mydsp_faustpower2_f(fSlow55) + -1.0f));
		float fSlow58 = fSlow56 / fSlow55;
		float fSlow59 = fSlow58 - fSlow57;
		float fSlow60 = (float)Math.Exp(-(fConst25 / fSlow22)) / fSlow53 + -1.0f;
		float fSlow61 = fSlow53 * (fSlow57 + (1.0f - fSlow58));
		float fSlow62 = (float)Math.Exp(-(fConst30 / fSlow10));
		float fSlow63 = mydsp_faustpower2_f(fSlow62);
		float fSlow64 = 1.0f - fSlow63;
		float fSlow65 = 1.0f - fSlow14 * fSlow63;
		float fSlow66 = (float)Math.Sqrt((float)Math.Max(0.0f, mydsp_faustpower2_f(fSlow65) / mydsp_faustpower2_f(fSlow64) + -1.0f));
		float fSlow67 = fSlow65 / fSlow64;
		float fSlow68 = fSlow67 - fSlow66;
		float fSlow69 = (float)Math.Exp(-(fConst30 / fSlow22)) / fSlow62 + -1.0f;
		float fSlow70 = fSlow62 * (fSlow66 + (1.0f - fSlow67));
		float fSlow71 = (float)Math.Exp(-(fConst35 / fSlow10));
		float fSlow72 = mydsp_faustpower2_f(fSlow71);
		float fSlow73 = 1.0f - fSlow72;
		float fSlow74 = 1.0f - fSlow14 * fSlow72;
		float fSlow75 = (float)Math.Sqrt((float)Math.Max(0.0f, mydsp_faustpower2_f(fSlow74) / mydsp_faustpower2_f(fSlow73) + -1.0f));
		float fSlow76 = fSlow74 / fSlow73;
		float fSlow77 = fSlow76 - fSlow75;
		float fSlow78 = (float)Math.Exp(-(fConst35 / fSlow22)) / fSlow71 + -1.0f;
		float fSlow79 = fSlow71 * (fSlow75 + (1.0f - fSlow76));
		float fSlow80 = (float)Math.Exp(-(fConst40 / fSlow10));
		float fSlow81 = mydsp_faustpower2_f(fSlow80);
		float fSlow82 = 1.0f - fSlow81;
		float fSlow83 = 1.0f - fSlow14 * fSlow81;
		float fSlow84 = (float)Math.Sqrt((float)Math.Max(0.0f, mydsp_faustpower2_f(fSlow83) / mydsp_faustpower2_f(fSlow82) + -1.0f));
		float fSlow85 = fSlow83 / fSlow82;
		float fSlow86 = fSlow85 - fSlow84;
		float fSlow87 = (float)Math.Exp(-(fConst40 / fSlow22)) / fSlow80 + -1.0f;
		float fSlow88 = fSlow80 * (fSlow84 + (1.0f - fSlow85));
		float fSlow89 = fConst44 * (float)(Mix);
		float fSlow90 = fConst44 * (float)Math.Pow(1e+01f, 0.05f * (float)(Level));
		for (int i0 = 0; i0 < count; i0++)
		{
			float fTemp0 = fSlow4 * fRec0[1];
			float fTemp1 = fSlow9 * fRec1[1];
			fRec13[0] = -(fSlow21 * (fSlow20 * fRec13[1] - (fRec6[1] + fRec6[2])));
			fRec12[0] = fSlow24 * (fRec6[1] + fSlow23 * fRec13[0]) + fSlow18 * fRec12[1];
			fVec0[IOTA0 & 16383] = 0.35355338f * fRec12[0] + 1e-20f;
			float fTemp2 = (float)(input0[i0]);
			fVec1[IOTA0 & 16383] = fTemp2;
			float fTemp3 = 0.3f * fVec1[(IOTA0 - iSlow25) & 16383];
			float fTemp4 = fTemp3 + fVec0[(IOTA0 - iConst6) & 16383] - 0.6f * fRec10[1];
			fVec2[IOTA0 & 4095] = fTemp4;
			fRec10[0] = fVec2[(IOTA0 - iConst8) & 4095];
			float fRec11 = 0.6f * fTemp4;
			fRec17[0] = -(fSlow21 * (fSlow20 * fRec17[1] - (fRec2[1] + fRec2[2])));
			fRec16[0] = fSlow34 * (fRec2[1] + fSlow33 * fRec17[0]) + fSlow32 * fRec16[1];
			fVec3[IOTA0 & 16383] = 0.35355338f * fRec16[0] + 1e-20f;
			float fTemp5 = fVec3[(IOTA0 - iConst12) & 16383] + fTemp3 - 0.6f * fRec14[1];
			fVec4[IOTA0 & 2047] = fTemp5;
			fRec14[0] = fVec4[(IOTA0 - iConst13) & 2047];
			float fRec15 = 0.6f * fTemp5;
			float fTemp6 = fRec15 + fRec11;
			fRec21[0] = -(fSlow21 * (fSlow20 * fRec21[1] - (fRec4[1] + fRec4[2])));
			fRec20[0] = fSlow43 * (fRec4[1] + fSlow42 * fRec21[0]) + fSlow41 * fRec20[1];
			fVec5[IOTA0 & 16383] = 0.35355338f * fRec20[0] + 1e-20f;
			float fTemp7 = fVec5[(IOTA0 - iConst17) & 16383] - (fTemp3 + 0.6f * fRec18[1]);
			fVec6[IOTA0 & 4095] = fTemp7;
			fRec18[0] = fVec6[(IOTA0 - iConst18) & 4095];
			float fRec19 = 0.6f * fTemp7;
			fRec25[0] = -(fSlow21 * (fSlow20 * fRec25[1] - (fRec8[1] + fRec8[2])));
			fRec24[0] = fSlow52 * (fRec8[1] + fSlow51 * fRec25[0]) + fSlow50 * fRec24[1];
			fVec7[IOTA0 & 16383] = 0.35355338f * fRec24[0] + 1e-20f;
			float fTemp8 = fVec7[(IOTA0 - iConst22) & 16383] - (fTemp3 + 0.6f * fRec22[1]);
			fVec8[IOTA0 & 2047] = fTemp8;
			fRec22[0] = fVec8[(IOTA0 - iConst23) & 2047];
			float fRec23 = 0.6f * fTemp8;
			float fTemp9 = fRec23 + fRec19 + fTemp6;
			fRec29[0] = -(fSlow21 * (fSlow20 * fRec29[1] - (fRec3[1] + fRec3[2])));
			fRec28[0] = fSlow61 * (fRec3[1] + fSlow60 * fRec29[0]) + fSlow59 * fRec28[1];
			fVec9[IOTA0 & 32767] = 0.35355338f * fRec28[0] + 1e-20f;
			float fTemp10 = (float)(input1[i0]);
			fVec10[IOTA0 & 16383] = fTemp10;
			float fTemp11 = 0.3f * fVec10[(IOTA0 - iSlow25) & 16383];
			float fTemp12 = fTemp11 + 0.6f * fRec26[1] + fVec9[(IOTA0 - iConst27) & 32767];
			fVec11[IOTA0 & 4095] = fTemp12;
			fRec26[0] = fVec11[(IOTA0 - iConst28) & 4095];
			float fRec27 = -(0.6f * fTemp12);
			fRec33[0] = -(fSlow21 * (fSlow20 * fRec33[1] - (fRec7[1] + fRec7[2])));
			fRec32[0] = fSlow70 * (fRec7[1] + fSlow69 * fRec33[0]) + fSlow68 * fRec32[1];
			fVec12[IOTA0 & 16383] = 0.35355338f * fRec32[0] + 1e-20f;
			float fTemp13 = fVec12[(IOTA0 - iConst32) & 16383] + fTemp11 + 0.6f * fRec30[1];
			fVec13[IOTA0 & 4095] = fTemp13;
			fRec30[0] = fVec13[(IOTA0 - iConst33) & 4095];
			float fRec31 = -(0.6f * fTemp13);
			fRec37[0] = -(fSlow21 * (fSlow20 * fRec37[1] - (fRec5[1] + fRec5[2])));
			fRec36[0] = fSlow79 * (fRec5[1] + fSlow78 * fRec37[0]) + fSlow77 * fRec36[1];
			fVec14[IOTA0 & 32767] = 0.35355338f * fRec36[0] + 1e-20f;
			float fTemp14 = 0.6f * fRec34[1] + fVec14[(IOTA0 - iConst37) & 32767];
			fVec15[IOTA0 & 4095] = fTemp14 - fTemp11;
			fRec34[0] = fVec15[(IOTA0 - iConst38) & 4095];
			float fRec35 = 0.6f * (fTemp11 - fTemp14);
			fRec41[0] = -(fSlow21 * (fSlow20 * fRec41[1] - (fRec9[1] + fRec9[2])));
			fRec40[0] = fSlow88 * (fRec9[1] + fSlow87 * fRec41[0]) + fSlow86 * fRec40[1];
			fVec16[IOTA0 & 32767] = 0.35355338f * fRec40[0] + 1e-20f;
			float fTemp15 = 0.6f * fRec38[1] + fVec16[(IOTA0 - iConst42) & 32767];
			fVec17[IOTA0 & 2047] = fTemp15 - fTemp11;
			fRec38[0] = fVec17[(IOTA0 - iConst43) & 2047];
			float fRec39 = 0.6f * (fTemp11 - fTemp15);
			fRec2[0] = fRec38[1] + fRec34[1] + fRec30[1] + fRec26[1] + fRec22[1] + fRec18[1] + fRec10[1] + fRec14[1] + fRec39 + fRec35 + fRec31 + fRec27 + fTemp9;
			fRec3[0] = fRec22[1] + fRec18[1] + fRec10[1] + fRec14[1] + fTemp9 - (fRec38[1] + fRec34[1] + fRec30[1] + fRec26[1] + fRec39 + fRec35 + fRec27 + fRec31);
			float fTemp16 = fRec19 + fRec23;
			fRec4[0] = fRec30[1] + fRec26[1] + fRec10[1] + fRec14[1] + fRec31 + fRec27 + fTemp6 - (fRec38[1] + fRec34[1] + fRec22[1] + fRec18[1] + fRec39 + fRec35 + fTemp16);
			fRec5[0] = fRec38[1] + fRec34[1] + fRec10[1] + fRec14[1] + fRec39 + fRec35 + fTemp6 - (fRec30[1] + fRec26[1] + fRec22[1] + fRec18[1] + fRec31 + fRec27 + fTemp16);
			float fTemp17 = fRec11 + fRec23;
			float fTemp18 = fRec15 + fRec19;
			fRec6[0] = fRec34[1] + fRec26[1] + fRec18[1] + fRec14[1] + fRec35 + fRec27 + fTemp18 - (fRec38[1] + fRec30[1] + fRec22[1] + fRec10[1] + fRec39 + fRec31 + fTemp17);
			fRec7[0] = fRec38[1] + fRec30[1] + fRec18[1] + fRec14[1] + fRec39 + fRec31 + fTemp18 - (fRec34[1] + fRec26[1] + fRec22[1] + fRec10[1] + fRec35 + fRec27 + fTemp17);
			float fTemp19 = fRec11 + fRec19;
			float fTemp20 = fRec15 + fRec23;
			fRec8[0] = fRec38[1] + fRec26[1] + fRec22[1] + fRec14[1] + fRec39 + fRec27 + fTemp20 - (fRec34[1] + fRec30[1] + fRec18[1] + fRec10[1] + fRec35 + fRec31 + fTemp19);
			fRec9[0] = fRec34[1] + fRec30[1] + fRec22[1] + fRec14[1] + fRec35 + fRec31 + fTemp20 - (fRec38[1] + fRec26[1] + fRec18[1] + fRec10[1] + fRec39 + fRec27 + fTemp19);
			float fTemp21 = 0.37f * (fRec3[0] + fRec4[0]);
			float fTemp22 = fTemp21 + fTemp1;
			fRec1[0] = fTemp22 - fSlow8 * fRec1[2];
			float fTemp23 = fSlow8 * fRec1[0];
			float fTemp24 = 0.5f * (fTemp23 + fTemp21 + fRec1[2] - fTemp1 + fSlow5 * (fRec1[2] + fTemp23 - fTemp22));
			float fTemp25 = fTemp24 + fTemp0;
			fRec0[0] = fTemp25 - fSlow3 * fRec0[2];
			float fTemp26 = fSlow3 * fRec0[0];
			fRec42[0] = fSlow89 + fConst45 * fRec42[1];
			float fTemp27 = fRec42[0] + 1.0f;
			float fTemp28 = 1.0f - 0.5f * fTemp27;
			fRec43[0] = fSlow90 + fConst45 * fRec43[1];
			output0[i0] = (float)(0.5f * fRec43[0] * (fTemp2 * fTemp27 + fTemp28 * (fTemp26 + fTemp24 + fRec0[2] - fTemp0 + fSlow0 * (fRec0[2] + fTemp26 - fTemp25))));
			float fTemp29 = fSlow4 * fRec44[1];
			float fTemp30 = fSlow9 * fRec45[1];
			float fTemp31 = 0.37f * (fRec3[0] - fRec4[0]);
			float fTemp32 = fTemp31 + fTemp30;
			fRec45[0] = fTemp32 - fSlow8 * fRec45[2];
			float fTemp33 = fSlow8 * fRec45[0];
			float fTemp34 = 0.5f * (fTemp33 + fTemp31 + fRec45[2] - fTemp30 + fSlow5 * (fRec45[2] + fTemp33 - fTemp32));
			float fTemp35 = fTemp34 + fTemp29;
			fRec44[0] = fTemp35 - fSlow3 * fRec44[2];
			float fTemp36 = fSlow3 * fRec44[0];
			output1[i0] = (float)(0.5f * fRec43[0] * (fTemp10 * fTemp27 + fTemp28 * (fTemp36 + fTemp34 + fRec44[2] - fTemp29 + fSlow0 * (fRec44[2] + fTemp36 - fTemp35))));
			fRec13[1] = fRec13[0];
			fRec12[1] = fRec12[0];
			IOTA0++;
			fRec10[1] = fRec10[0];
			fRec17[1] = fRec17[0];
			fRec16[1] = fRec16[0];
			fRec14[1] = fRec14[0];
			fRec21[1] = fRec21[0];
			fRec20[1] = fRec20[0];
			fRec18[1] = fRec18[0];
			fRec25[1] = fRec25[0];
			fRec24[1] = fRec24[0];
			fRec22[1] = fRec22[0];
			fRec29[1] = fRec29[0];
			fRec28[1] = fRec28[0];
			fRec26[1] = fRec26[0];
			fRec33[1] = fRec33[0];
			fRec32[1] = fRec32[0];
			fRec30[1] = fRec30[0];
			fRec37[1] = fRec37[0];
			fRec36[1] = fRec36[0];
			fRec34[1] = fRec34[0];
			fRec41[1] = fRec41[0];
			fRec40[1] = fRec40[0];
			fRec38[1] = fRec38[0];
			fRec2[2] = fRec2[1];
			fRec2[1] = fRec2[0];
			fRec3[2] = fRec3[1];
			fRec3[1] = fRec3[0];
			fRec4[2] = fRec4[1];
			fRec4[1] = fRec4[0];
			fRec5[2] = fRec5[1];
			fRec5[1] = fRec5[0];
			fRec6[2] = fRec6[1];
			fRec6[1] = fRec6[0];
			fRec7[2] = fRec7[1];
			fRec7[1] = fRec7[0];
			fRec8[2] = fRec8[1];
			fRec8[1] = fRec8[0];
			fRec9[2] = fRec9[1];
			fRec9[1] = fRec9[0];
			fRec1[2] = fRec1[1];
			fRec1[1] = fRec1[0];
			fRec0[2] = fRec0[1];
			fRec0[1] = fRec0[0];
			fRec42[1] = fRec42[0];
			fRec43[1] = fRec43[0];
			fRec45[2] = fRec45[1];
			fRec45[1] = fRec45[0];
			fRec44[2] = fRec44[1];
			fRec44[1] = fRec44[0];
		}
	}
};




