using Foraria.CallTranscriber.Services;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using Xunit;

namespace Foraria.CallTranscriberTests.Services;


public class WhisperTranscriptionServiceTests
{
    private IConfiguration BuildConfig(string modelPath)
    {
        var dict = new Dictionary<string, string?>
        {
            ["Whisper:ModelPath"] = modelPath,
            ["Whisper:Language"] = "es"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(dict!)
            .Build();
    }

    [Fact]
    public void Constructor_Throws_WhenModelNotFound()
    {
        Assert.Throws<FileNotFoundException>(() =>
        {
            var svc = new WhisperTranscriptionService(BuildConfig("no-model.bin"));
        });
    }

    [Fact]
    public void ComputeSha256_WorksCorrectly()
    {
        // Arrange
        var temp = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.txt");
        File.WriteAllText(temp, "hola mundo");

        using var sha = SHA256.Create();
        var expected = BitConverter
            .ToString(sha.ComputeHash(File.ReadAllBytes(temp)))
            .Replace("-", "")
            .ToLowerInvariant();

        // Act
        var result = WhisperTranscriptionService.ComputeSha256(temp);

        // Assert
        Assert.Equal(expected, result);

        File.Delete(temp);
    }
}
