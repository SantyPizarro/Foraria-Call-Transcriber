using Foraria.CallTranscriber.Models;

namespace Foraria.CallTranscriber.Services;

public interface IForariaCallbackClient
{
    Task<bool> NotifyTranscriptionCompleteAsync(int callId, ForariaTranscriptionCompleteDto dto, CancellationToken ct = default);
}
