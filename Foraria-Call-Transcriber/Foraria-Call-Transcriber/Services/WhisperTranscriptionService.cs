using Microsoft.Extensions.Configuration;
using Whisper.net;
using NAudio.Wave;
using System.Security.Cryptography;

namespace Foraria.CallTranscriber.Services;

public class WhisperTranscriptionService : IWhisperTranscriptionService
{
    private readonly WhisperFactory _factory;
    private readonly string _language;

    public WhisperTranscriptionService(IConfiguration configuration)
    {
        var modelPath = configuration["Whisper:ModelPath"]
            ?? throw new InvalidOperationException("Whisper:ModelPath no configurado.");

        _language = configuration["Whisper:Language"] ?? "es";

        if (!File.Exists(modelPath))
            throw new FileNotFoundException("No se encontró el modelo de Whisper.", modelPath);

        _factory = WhisperFactory.FromPath(modelPath);
    }

    public async Task<string> TranscribeAsync(string audioFilePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(audioFilePath))
            throw new FileNotFoundException("No se encontró el archivo de audio.", audioFilePath);

        var wavPath = ConvertTo16KHzWav(audioFilePath);

        using var processor = _factory.CreateBuilder()
            .WithLanguage(_language)
            .Build();

        using var fileStream = File.OpenRead(wavPath);

        var segments = processor.ProcessAsync(fileStream, cancellationToken);

        List<string> result = new();

        await foreach (var segment in segments.WithCancellation(cancellationToken))
            result.Add(segment.Text);

        return string.Join(" ", result).Trim();
    }

    private string ConvertTo16KHzWav(string inputPath)
    {
        var outputPath = Path.ChangeExtension(inputPath, ".converted.wav");

        using var reader = new AudioFileReader(inputPath);

        var format = new WaveFormat(16000, 1); // 16kHz, mono

        using var resampler = new MediaFoundationResampler(reader, format)
        {
            ResamplerQuality = 60
        };

        WaveFileWriter.CreateWaveFile(outputPath, resampler);

        return outputPath;
    }

    public static string ComputeSha256(string filePath)
    {
        using var sha = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var bytes = sha.ComputeHash(stream);
        return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
    }
}
