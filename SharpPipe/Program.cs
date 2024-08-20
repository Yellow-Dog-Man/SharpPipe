

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace SharpPipe;


public class Program
{
    public static unsafe void Main(string[] args)
    {
        using SoundPipe pipe = new();
        // pipe.FileName = "WHO ARE YOUUU";
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


        Console.WriteLine(pipe.FileName);
        Console.WriteLine(zita.InDelay);
        Console.WriteLine(*zita.Data.args.arg0);


        byte[] inputFile = File.ReadAllBytes("./DTMF.raw");
        Span<float> samples = MemoryMarshal.Cast<byte, float>(inputFile);
        
        
        byte[] outputFile = new byte[inputFile.Length];
        Span<float> outputSamples = MemoryMarshal.Cast<byte, float>(outputFile);

        
        float dummy = 0f;
        ref float dummyRef = ref dummy;

        for (int i = 0; i < 100; i++)
        {
            Console.WriteLine(samples[i]);
        }
        
        for (int i = 0; i < samples.Length; i++)
        {

            zita.Compute(samples[i], samples[i], ref dummyRef, ref outputSamples[i]);
        }

        for (int i = 0; i < 100; i++)
        {
            Console.WriteLine(outputSamples[i]);
        }
        

        File.WriteAllBytes("./Output.raw", outputFile);
    }
}