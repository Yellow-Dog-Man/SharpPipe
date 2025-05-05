using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpPipe.Example;


public class Program
{
    public static void Main(string[] args)
    {
        string inputFile; // Stereo, 32 bit float, 44100hz (use audacity or similar to import/export raw samples)

        if (args.Length == 0)
        {
            Console.WriteLine("Please provide a valid file!");
            return;
        }
        else
        {
            inputFile = Path.GetFullPath(args[0]);

            if (!File.Exists(args[0]))
            {
                Console.WriteLine("File does not exist!");
                return;
            }
        }

        // Make a new SoundPipe object and feed it to the filters.
        using SoundPipe pipe = new(); // Defaults to sample rate of 44100hz
        pipe.SampleRate = 44100; // Set it anyways for demonstration

        ProcessZita(pipe, inputFile);
        ProcessPitchShift(pipe, inputFile);

        // Print a happy little message to the console.
        Console.WriteLine("Done!");
    }


    public static void ProcessZita(SoundPipe pipe, string filePath)
    {
        byte[] inputFile = File.ReadAllBytes(filePath);
        Span<float> samples = MemoryMarshal.Cast<byte, float>(inputFile);

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


        zita.Compute(samples, samples);

        // Write processed samples back out to a separate raw
        File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(filePath), "OutputZita.raw"), inputFile);
    }


    public static void ProcessPitchShift(SoundPipe pipe, string filePath)
    {
        byte[] inputFile = File.ReadAllBytes(filePath);
        Span<float> samples = MemoryMarshal.Cast<byte, float>(inputFile);

        // Bathroom preset
        PitchShiftParameters shifterParams = new()
        {
            SemitoneShift = -2f,
            WindowSize = 512,
            CrossfadeSamples = 48
        };

        // Set up pitch shifter objects (pitch shifter is only mono!)
        PitchShift shifterLeft = new(pipe);
        PitchShift shifterRight = new(pipe);

        // Since ZitaParameters and ZitaReverb both implement
        // IZitaFilter, their parameters can be interchanged very easily
        shifterLeft.FromOther(shifterParams);
        shifterRight.FromOther(shifterParams);

        int halfLength = samples.Length / 2;
        float[] left = new float[halfLength];
        float[] right = new float[halfLength];

        for (int i = 0; i < halfLength; i++)
        {
            left[i] = samples[i * 2];
            right[i] = samples[i * 2 + 1];
        }

        shifterLeft.Compute(left, left);
        shifterRight.Compute(right, right);

        for (int i = 0; i < halfLength; i++)
        {
             samples[i * 2] = left[i];
             samples[i * 2 + 1] = right[i];
        }


        // Write processed samples back out to a separate raw
        File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(filePath), "OutputPitchShift.raw"), inputFile);
    }
}
