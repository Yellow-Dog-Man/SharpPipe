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
        byte[] inputFile = File.ReadAllBytes("./Input.raw"); // Stereo, 32 bit float, 44100hz, DTMF tones as a test (use audacity or similar to import/export raw samples)
        Span<float> samples = MemoryMarshal.Cast<byte, float>(inputFile);

        zita.Compute(samples, samples);

        // Print a happy little message to the console.
        Console.WriteLine("Done!");

        // Write processed samples back out to a separate raw
        File.WriteAllBytes("./Output.raw", inputFile);
    }
}
