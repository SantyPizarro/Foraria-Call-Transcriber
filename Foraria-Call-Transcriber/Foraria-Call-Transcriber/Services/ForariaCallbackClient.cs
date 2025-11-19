using System.Net.Http.Json;
using Foraria.CallTranscriber.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Foraria.CallTranscriber.Services;

public class ForariaCallbackClient : IForariaCallbackClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly ILogger<ForariaCallbackClient> _logger;

    public ForariaCallbackClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<ForariaCallbackClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = configuration["Foraria:BaseUrl"]
            ?? throw new InvalidOperationException("Foraria:BaseUrl no configurado.");
    }

    public async Task<bool> NotifyTranscriptionCompleteAsync(
        int callId,
        ForariaTranscriptionCompleteDto dto,
        CancellationToken ct = default)
    {

        var url = $"{_baseUrl}/api/transcriptions/internal/{callId}/complete";

        _logger.LogInformation("Notificando a Foraria en {Url}...", url);

        var response = await _httpClient.PostAsJsonAsync(url, dto, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Error al notificar a Foraria. Status: {Status}, Body: {Body}",
                response.StatusCode, body);
            return false;
        }

        return true;
    }
}
