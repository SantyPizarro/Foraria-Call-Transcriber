using System.Threading;
using System.Threading.Tasks;

namespace Foraria.CallTranscriber.Services;

public interface IWhisperTranscriptionService
{
    Task<string> TranscribeAsync(string audioFilePath, CancellationToken cancellationToken = default);
}
