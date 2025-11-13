using Microsoft.Extensions.Configuration;
using Whisper.net;
using Whisper.net.Ggml;

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

        using var processor = _factory.CreateBuilder()
            .WithLanguage(_language)
            .Build();

        using var fileStream = File.OpenRead(audioFilePath);

        var segments = processor.ProcessAsync(fileStream, cancellationToken);

        List<string> texts = new();

        await foreach (var segment in segments.WithCancellation(cancellationToken))
        {
            texts.Add(segment.Text);
        }

        return string.Join(" ", texts).Trim();
    }
}
