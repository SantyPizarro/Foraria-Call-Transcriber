using NAudio.Wave;

namespace Foraria.CallTranscriber.Services;

public static class AudioConverter
{
    public static string ConvertToWav(string inputPath)
    {
        var outputPath = Path.ChangeExtension(inputPath, ".wav");

        using var reader = new Mp3FileReader(inputPath);
        using var pcmStream = WaveFormatConversionStream.CreatePcmStream(reader);
        WaveFileWriter.CreateWaveFile(outputPath, pcmStream);

        return outputPath;
    }
}
