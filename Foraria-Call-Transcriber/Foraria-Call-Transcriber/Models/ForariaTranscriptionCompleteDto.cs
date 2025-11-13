namespace Foraria.CallTranscriber.Models;

public class ForariaTranscriptionCompleteDto
{
    public string TranscriptPath { get; set; } = string.Empty;
    public string AudioPath { get; set; } = string.Empty;
    public string TranscriptHash { get; set; } = string.Empty;
    public string AudioHash { get; set; } = string.Empty;
}