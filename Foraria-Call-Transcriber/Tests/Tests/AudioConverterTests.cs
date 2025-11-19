using Foraria.CallTranscriber.Services;

namespace Foraria.CallTranscriberTests.Services;


public class AudioConverterTests
{
    [Fact]
    public void ConvertToWav_ShouldCreateWavFile()
    {
        // Arrange
        var mp3Path = Path.Combine(AppContext.BaseDirectory, "TestAssets", "hey.mp3");
        Assert.True(File.Exists(mp3Path), "El archivo MP3 de prueba no existe");

        var wavPath = Path.ChangeExtension(mp3Path, ".wav");

        if (File.Exists(wavPath))
            File.Delete(wavPath);

        // Act
        var result = AudioConverter.ConvertToWav(mp3Path);

        // Assert
        Assert.Equal(wavPath, result);
        Assert.True(File.Exists(result));
    }


}
