using System.Runtime.InteropServices;

namespace SharpPipe.Example;


public class Program
{
    public static void Main(string[] args)
    {
        // Make a new SoundPipe object and feed it to the ZitaReverb
        using SoundPipe pipe = new(); // Defaults to sample rate of 44100hz
        using ZitaReverb zita = new(pipe);

        zita.InDelay = 5f;
        zita.Crossover = 200f;
        zita.RT60Low = 1.49f;
        zita.RT60Mid = 1.3f;
        zita.HighFrequencyDamping = 5000f;
        zita.EQ1Frequency = 120f;
        zita.EQ1Level = 8f;
        zita.EQ2Frequency = 1200f;
        zita.EQ2Level = 6.5545f;
        zita.Mix = 0.7f;
        zita.Level = 7f;


        // Read a raw file and cast the read bytes to floats
        byte[] inputFile = File.ReadAllBytes("./DTMF.raw"); // Single-channel, 32 bit float, 44100hz
        Span<float> samples = MemoryMarshal.Cast<byte, float>(inputFile);
        
        // Create a new output buffer of bytes and write to it as floats
        byte[] outputFile = new byte[inputFile.Length];
        Span<float> outputSamples = MemoryMarshal.Cast<byte, float>(outputFile);

        // Dummy reference because the output only has a single channel
        float dummy = 0f;
        ref float dummyRef = ref dummy;

        // Process each sample individually
        for (int i = 0; i < samples.Length; i++)
        {
            zita.Compute(samples[i], samples[i], ref dummyRef, ref outputSamples[i]);
        }
        

        // Write processed samples back out to raw
        File.WriteAllBytes("./Output.raw", outputFile);
    }
}