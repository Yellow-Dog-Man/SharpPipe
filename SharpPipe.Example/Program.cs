using System.Runtime.InteropServices;

namespace SharpPipe.Example;


public class Program
{
    public static void Main(string[] args)
    {
        // Make a new SoundPipe object and feed it to the ZitaReverb
        using SoundPipe pipe = new(); // Defaults to sample rate of 44100hz
        
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
        byte[] inputFile = File.ReadAllBytes("./DTMF.raw"); // Single-channel, 32 bit float, 44100hz
        Span<float> samples = MemoryMarshal.Cast<byte, float>(inputFile);
        
        // Create a new output buffer of bytes and write to it as floats
        byte[] outputFile = new byte[inputFile.Length];
        Span<float> outputSamples = MemoryMarshal.Cast<byte, float>(outputFile);

        // Left and right channel variables
        float left = 0f;
        float right = 0f;

        // Process each sample individually
        for (int i = 0; i < samples.Length; i++)
        {
            // Pass in both samples since this is a mono input.
            // You would usually pass in the left and right sample and receive
            // a processed left and right sample in return
            zita.Compute(samples[i], samples[i], ref left, ref right);
            outputSamples[i] = (left + right) / 2f; // Average into a single sample
        }
        

        // Write processed samples back out to raw
        File.WriteAllBytes("./Output.raw", outputFile);
    }
}