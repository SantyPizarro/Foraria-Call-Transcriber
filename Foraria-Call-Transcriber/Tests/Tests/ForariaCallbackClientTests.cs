using Foraria.CallTranscriber.Models;
using Foraria.CallTranscriber.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Foraria.CallTranscriberTests.Services;


public class ForariaCallbackClientTests
{
    private IConfiguration BuildConfig()
    {
        var dict = new Dictionary<string, string?>
        {
            ["Foraria:BaseUrl"] = "https://foraria.test"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(dict!)
            .Build();
    }

    [Fact]
    public async Task NotifyTranscriptionCompleteAsync_ReturnsTrue_OnSuccess()
    {
        // Arrange
        var handler = new Mock<HttpMessageHandler>();

        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var client = new HttpClient(handler.Object);
        var logger = Mock.Of<ILogger<ForariaCallbackClient>>();

        var svc = new ForariaCallbackClient(client, BuildConfig(), logger);

        // Act
        var ok = await svc.NotifyTranscriptionCompleteAsync(
            callId: 1,
            new ForariaTranscriptionCompleteDto()
        );

        // Assert
        Assert.True(ok);
    }

    [Fact]
    public async Task NotifyTranscriptionCompleteAsync_ReturnsFalse_OnFailure()
    {
        // Arrange
        var handler = new Mock<HttpMessageHandler>();

        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("error test")
            });

        var client = new HttpClient(handler.Object);
        var logger = Mock.Of<ILogger<ForariaCallbackClient>>();

        var svc = new ForariaCallbackClient(client, BuildConfig(), logger);

        // Act
        var ok = await svc.NotifyTranscriptionCompleteAsync(
            1,
            new ForariaTranscriptionCompleteDto()
        );

        // Assert
        Assert.False(ok);
    }
}
