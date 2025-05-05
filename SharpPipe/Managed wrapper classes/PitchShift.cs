
using System.Runtime.CompilerServices;

namespace SharpPipe;


/// <summary>
/// Represents an object that imlements pitch shift filter parameters.
/// </summary>
public interface IPitchShiftFilter
{
	/// <summary>
	/// Semitones to shift the incoming signal by.
	/// </summary>
	float SemitoneShift { get; set; }

	/// <summary>
	/// Window size (in samples) the incoming signal will be sliced into for processing.
	/// </summary>
	float WindowSize { get; set; }

	/// <summary>
	/// Number of samples used for crossfading between each slice.
	/// <para>NOTE: Lower sample counts may produce clicking from quick crossfades.</para>
	/// <para>High sample counts may produce a "wah-wah" effect in the perceived volume.</para>
	/// </summary>
	float CrossfadeSamples { get; set; }
}


/// <summary>
/// Provides temporal pitch shifting to processed samples.
/// </summary>
public sealed class PitchShift : IPitchShiftFilter
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public const float PITCHSHIFT_SEMITONE_SHIFT_DEFAULT = 0f;
    public const float PITCHSHIFT_WINDOW_SIZE_DEFAULT = 512f;
	public const float PITCHSHIFT_SAMPLE_CROSSFADE_DEFAULT = 64f;

	public const float PITCHSHIFT_SEMITONE_SHIFT_MIN = -12f;
	public const float PITCHSHIFT_WINDOW_SIZE_MIN = 50f;
	public const float PITCHSHIFT_SAMPLE_CROSSFADE_MIN = 1f;

	public const float PITCHSHIFT_SEMITONE_SHIFT_MAX = 12f;
	public const float PITCHSHIFT_WINDOW_SIZE_MAX = 10000;
	public const float PITCHSHIFT_SAMPLE_CROSSFADE_MAX = 10000;

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

	/// <inheritdoc/>
    public unsafe float SemitoneShift
    {
        get => _impl.SemitoneShift;
        set => _impl.SemitoneShift = value;
    }

	/// <inheritdoc/>
    public unsafe float WindowSize
    {
        get => _impl.WindowSize;
        set => _impl.WindowSize = value;
    }

	/// <inheritdoc/>
    public unsafe float CrossfadeSamples
    {
        get => _impl.CrossfadeSamples;
        set => _impl.CrossfadeSamples = value;
    }

    internal readonly PShift _impl;

	/// <summary>
	/// Instantiates a new pitch shift filter.
	/// </summary>
	/// <param name="pipe">The soundpipe object.</param>
    public PitchShift(SoundPipe pipe)
    {
        _impl = new(pipe.SampleRate);
    }


	/// <summary>
    /// Computes pitch-shift from an arbitrarily-sized span of mono samples and places them into an output buffer. Processes the signal in chunks.
    /// <para>1 sample == 1 float. E.g. A chunkSize of 1024 will process 1024 floats at once.</para>
    /// </summary>
    /// <param name="samplesIn">Interleaved input stereo signal.</param>
    /// <param name="samplesOut">Interleaved output stereo signal.</param>
    /// <param name="chunkSize">The size of each chunk to be processed.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Compute(Span<float> samplesIn, Span<float> samplesOut, int chunkSize = 1024)
    {
        if (samplesIn.Length > samplesOut.Length)
            throw new ArgumentOutOfRangeException(nameof(samplesIn), $"The input span is larger than {nameof(samplesOut)}!");

        int length = samplesIn.Length;
        int blockSize = Math.Abs(chunkSize); // One channel.

        // Process a batch of samples at once.
        for (int i = 0; i < length;)
        {
            // Get the length to process. This accounts for when you're close to the end of input buffer.
            int remainingLength = length - i;
            int lengthToProcess = Math.Min(remainingLength, blockSize);

            // Slice the input into a chunk that can be computed.
            Span<float> inputSliced = samplesIn.Slice(i, lengthToProcess);

            // Slice the output into an equivalent chunk.
            Span<float> outputSliced = samplesOut.Slice(i, lengthToProcess);

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
    /// <param name="samplesIn">Interleaved input stereo signal.</param>
    /// <param name="samplesOut">Interleaved output stereo signal.</param>
    public void ComputeBlock(Span<float> samplesIn, Span<float> samplesOut) => _impl.Compute(samplesIn.Length, samplesIn, samplesOut);
}


/* ------------------------------------------------------------
author: "Grame"
copyright: "(c)GRAME 2006"
license: "BSD"
name: "Test", "pitchShifter"
version: "1.0"
Code generated with Faust 2.79.3 (https://faust.grame.fr)
Compilation options: -lang csharp -ct 1 -es 1 -mcd 16 -mdd 1024 -mdy 33 -single -ftz 0
------------------------------------------------------------ */

// NOTE: Organized & edited by Cyro.

sealed class PShift
{
	
	public float SemitoneShift;
	public float WindowSize;
	public float CrossfadeSamples;


	private readonly int fSampleRate;
	private readonly float[] fRec0 = new float[2];
	private readonly float[] fVec0 = new float[131072];
	int IOTA0;
	
	public PShift(int sample_rate)
	{
		fSampleRate = sample_rate;
		InstanceResetUserInterface();
		InstanceClear();
	}

	public void InstanceResetUserInterface()
	{
		SemitoneShift = (float)(0.0f);
		WindowSize = (float)(1e+03f);
		CrossfadeSamples = (float)(1e+01f);
	}
	
	public void InstanceClear()
	{
		for (int l0 = 0; l0 < 2; l0++) {
			fRec0[l0] = 0.0f;
		}
		IOTA0 = 0;
		for (int l1 = 0; l1 < 131072; l1++) {
			fVec0[l1] = 0.0f;
		}
	}
	
	public void Compute(int count, Span<float> input, Span<float> output)
	{
		Span<float> input0 = input;
		Span<float> output0 = output;
		float fSlow0 = (float)Math.Pow(2.0f, 0.083333336f * (float)(SemitoneShift));
		float fSlow1 = (float)(WindowSize);
		float fSlow2 = 1.0f / (float)(CrossfadeSamples);
		for (int i0 = 0; i0 < count; i0++) {
			fRec0[0] = (fSlow1 + (fRec0[1] + 1.0f - fSlow0)) % fSlow1;
			float fTemp0 = (float)Math.Min(fSlow2 * fRec0[0], 1.0f);
			float fTemp1 = (float)(input0[i0]);
			fVec0[IOTA0 & 131071] = fTemp1;
			float fTemp2 = fSlow1 + fRec0[0];
			int iTemp3 = (int)(fTemp2);
			float fTemp4 = (float)Math.Floor(fTemp2);
			float fTemp5 = 1.0f - fRec0[0];
			int iTemp6 = (int)(fRec0[0]);
			float fTemp7 = (float)Math.Floor(fRec0[0]);
			output0[i0] = (float)((fVec0[(IOTA0 - Math.Min(65537, Math.Max(0, iTemp6))) & 131071] * (fTemp7 + fTemp5) + (fRec0[0] - fTemp7) * fVec0[(IOTA0 - Math.Min(65537, Math.Max(0, iTemp6 + 1))) & 131071]) * fTemp0 + (fVec0[(IOTA0 - Math.Min(65537, Math.Max(0, iTemp3))) & 131071] * (fTemp4 + fTemp5 - fSlow1) + (fSlow1 + (fRec0[0] - fTemp4)) * fVec0[(IOTA0 - Math.Min(65537, Math.Max(0, iTemp3 + 1))) & 131071]) * (1.0f - fTemp0));
			fRec0[1] = fRec0[0];
			IOTA0++;
		}
	}
};