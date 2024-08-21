using System.Runtime.InteropServices;

namespace SharpPipe.Example;


public class Program
{
    public static void Main(string[] args)
    {
        // Make a new SoundPipe object and feed it to the ZitaReverb
        using SoundPipe pipe = new(); // Defaults to sample rate of 44100hz
        using ZitaReverb zita = new(pipe);

        zita.InDelay = 40.0f;
        zita.Crossover = 200.0f;
        zita.RT60Low = 3.0f;
        zita.RT60Mid = 8.0f;
        zita.HighFrequencyDamping = 6000.0f;
        zita.EQ1Frequency = 400.0f;
        zita.EQ1Level = 0.0f;
        zita.EQ2Frequency = 4000.0f;
        zita.EQ2Level = 0.0f;
        zita.Mix = 0.5f;
        zita.Level = 0.0f;


        // Read a raw file and cast the read bytes to floats
        byte[] inputFile = File.ReadAllBytes("./DTMF.raw");
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