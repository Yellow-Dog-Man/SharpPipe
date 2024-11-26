using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpPipe.Example;


public class Program
{
    public static void Main(string[] args)
    {
        // Make a new SoundPipe object and feed it to the ZitaReverb
        using SoundPipe pipe = new(); // Defaults to sample rate of 44100hz
        pipe.SampleRate = 44100; // Set it anyways for demonstration
        
        // Bathroom preset
        ZitaParameters zitaParams = new()
        {
            InDelay = 5f,
            Crossover = 200f,
            RT60Low = 1.49f,
            RT60Mid = 1.3f,
            HighFrequencyDamping = 5000f,
            EQ1Frequency = 120f,
            EQ1Level = 8f,
            EQ2Frequency = 1200f,
            EQ2Level = 6.5545f,
            Mix = 0.7f,
            Level = 7f
        };

        // Set up a zita reverb object
        using ZitaReverb zita = new(pipe);

        // Since ZitaParameters and ZitaReverb both implement
        // IZitaFilter, their parameters can be interchanged very easily
        zita.FromOther(zitaParams);


        // Read a raw file and cast the read bytes to floats
        byte[] inputFile = File.ReadAllBytes("./Input.raw"); // Stereo, 32 bit float, 44100hz, DTMF tones as a test
        Span<Stereo> samples = MemoryMarshal.Cast<byte, Stereo>(inputFile);


        // Process a batch of samples at once.
        int processLength = 16384;
        for (int i = 0; i < samples.Length;)
        {
            // Get the length to process. This accounts for when you're close to the end of the file
            int lengthToProcess = Math.Min(samples.Length - i, processLength);

            // Slice the input into a chunk that can be computed
            Span<Stereo> inputSliced = samples.Slice(i, lengthToProcess);

            // Interpret the sliced buffer of stereo samples to floats
            Span<float> inBuf = MemoryMarshal.Cast<Stereo, float>(inputSliced);

         /* Compute the buffer using itself as both input and output. You can
            optionally write to a separate location however if you wish. */
            zita.Compute(inBuf, inBuf);

            // Increment the index by the amount of data that was just processed.
            i += lengthToProcess;
        }
        
        // Print a happy little message to the console.
        Console.WriteLine("Done!");
        
        // Write processed samples back out to a separate raw
        File.WriteAllBytes("./Output.raw", inputFile);
    }


    public static void MonoToStereo(Span<Mono> mono, Span<Stereo> stereo)
    {
        for (int i = 0; i < mono.Length; i++)
            stereo[i] = mono[i];
    }


    public static void StereoToMono(Span<Stereo> stereo, Span<Mono> mono)
    {
        for (int i = 0; i < stereo.Length; i++)
            mono[i] = stereo[i];
    }
}



public readonly struct Mono(float amplitude)
{
    public readonly float Amplitude = amplitude;

    

    
    public readonly Stereo AsStereo() => new(Amplitude, Amplitude);
    public static implicit operator Stereo(Mono other) => other.AsStereo();
    public static implicit operator Mono(float other) => Unsafe.As<float, Mono>(ref other);
}



public readonly struct Stereo(float left, float right)
{
    public readonly float Left = left;
    public readonly float Right = right;


    public readonly Mono AsMono() => new((Left + Right) / 2);
    public static implicit operator Mono(Stereo other) => other.AsMono();
}
