namespace Foraria.CallTranscriber.Models;

public class TranscriptionResponse
{
    public int CallId { get; set; }
    public string Transcript { get; set; } = null!;
    public string TranscriptPath { get; set; } = null!;
    public string AudioPath { get; set; } = null!;
    public bool NotifiedForaria { get; set; }
}
