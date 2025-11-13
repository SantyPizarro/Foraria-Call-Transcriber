namespace Foraria.CallTranscriber.Models;

public class ForariaTranscriptionCompleteDto
{
    public string TranscriptPath { get; set; } = null!;
    public string? AudioPath { get; set; }
}