using System.Security.Cryptography;
using System.Text;
using Foraria.CallTranscriber.Models;
using Foraria.CallTranscriber.Services;
using Microsoft.AspNetCore.Mvc;

namespace Foraria.CallTranscriber.Controllers;

[ApiController]
[Route("api/transcriptions")]
public class TranscriptionController : ControllerBase
{
    private readonly IWhisperTranscriptionService _whisper;
    private readonly IForariaCallbackClient _foraria;
    private readonly ILogger<TranscriptionController> _logger;
    private readonly string _audioFolder;
    private readonly string _transcriptFolder;

    public TranscriptionController(
        IWhisperTranscriptionService whisper,
        IForariaCallbackClient foraria,
        IConfiguration configuration,
        ILogger<TranscriptionController> logger)
    {
        _whisper = whisper;
        _foraria = foraria;
        _logger = logger;

        _audioFolder = configuration["Storage:AudioFolder"] ?? "storage/audio";
        _transcriptFolder = configuration["Storage:TranscriptFolder"] ?? "storage/transcripts";

        Directory.CreateDirectory(_audioFolder);
        Directory.CreateDirectory(_transcriptFolder);
    }

    [HttpPost("{callId:int}")]
    [RequestSizeLimit(100_000_000)] // 100 MB
    public async Task<ActionResult<TranscriptionResponse>> Transcribe(
        int callId,
        [FromForm] AudioUploadDto request,
        CancellationToken ct)
    {
        if (callId <= 0)
            return BadRequest("callId inválido.");

        if (request.Audio == null || request.Audio.Length == 0)
            return BadRequest("Debe enviar un archivo de audio.");

        var audioFileName = $"{callId}_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{Path.GetExtension(request.Audio.FileName)}";
        var audioPath = Path.Combine(_audioFolder, audioFileName);

        await using (var stream = System.IO.File.Create(audioPath))
            await request.Audio.CopyToAsync(stream, ct);

        _logger.LogInformation("Audio guardado en {Path}", audioPath);

        var transcriptText = await _whisper.TranscribeAsync(audioPath, ct);

        var transcriptFileName = Path.ChangeExtension(audioFileName, ".txt");
        var transcriptPath = Path.Combine(_transcriptFolder, transcriptFileName);

        await System.IO.File.WriteAllTextAsync(transcriptPath, transcriptText, ct);
        _logger.LogInformation("Transcripción guardada en {Path}", transcriptPath);

        var transcriptHash = ComputeSha256FromText(transcriptText);
        var audioHash = ComputeSha256FromFile(audioPath);

        var dto = new ForariaTranscriptionCompleteDto
        {
            TranscriptPath = transcriptPath,
            AudioPath = audioPath,
            TranscriptHash = transcriptHash,
            AudioHash = audioHash
        };

        var notified = await _foraria.NotifyTranscriptionCompleteAsync(callId, dto, ct);

        var response = new TranscriptionResponse
        {
            CallId = callId,
            Transcript = transcriptText,
            TranscriptPath = transcriptPath,
            AudioPath = audioPath,
            NotifiedForaria = notified
        };

        if (!notified)
            return StatusCode(StatusCodes.Status502BadGateway,
                new { message = "Transcrito, pero no se pudo notificar a Foraria.", response });

        return Ok(response);
    }

    [HttpGet("{callId:int}/transcript-file")]
    public IActionResult GetTranscriptFile(int callId)
    {
        var pattern = $"{callId}_*.txt";
        var files = Directory.GetFiles(_transcriptFolder, pattern);

        if (files.Length == 0)
            return NotFound(new { message = "No se encontró archivo de transcripción para esa llamada." });

        var filePath = files
            .OrderByDescending(f => System.IO.File.GetCreationTimeUtc(f))
            .First();

        var fileName = Path.GetFileName(filePath);

        var stream = System.IO.File.OpenRead(filePath);
        return File(stream, "text/plain", fileName);
    }

    [HttpGet("{callId:int}/audio-file")]
    public IActionResult GetAudioFile(int callId)
    {
        var pattern = $"{callId}_*.*";
        var files = Directory.GetFiles(_audioFolder, pattern);

        if (files.Length == 0)
            return NotFound(new { message = "No se encontró archivo de audio para esa llamada." });

        var filePath = files
            .OrderByDescending(f => System.IO.File.GetCreationTimeUtc(f))
            .First();

        var fileName = Path.GetFileName(filePath);
        var ext = Path.GetExtension(filePath).ToLowerInvariant();

        var contentType = ext switch
        {
            ".wav" => "audio/wav",
            ".mp3" => "audio/mpeg",
            _ => "application/octet-stream"
        };

        var stream = System.IO.File.OpenRead(filePath);
        return File(stream, contentType, fileName);
    }



    [HttpGet("health")]
    public IActionResult Health()
        => Ok(new { status = "ok" });
    private static string ComputeSha256FromText(string text)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string ComputeSha256FromFile(string path)
    {
        using var sha = SHA256.Create();
        using var stream = System.IO.File.OpenRead(path);
        var hash = sha.ComputeHash(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
