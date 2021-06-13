using AssemblyCSharp.Assets.Code.Core.Storage.Impl.Texture;
using NSubstitute;
using NUnit.Framework;

public class TextureStorageProvider_Tests
{
    private TextureStorageProvider textureStorageProvider;
    private string testKey = "TestKey";
    
    [Test]
    public void Given_AnImageAsATexture_When_TheImageIsSaved_Then_TheSameImageShouldBeReturned()
    {
        textureStorageProvider = new TextureStorageProvider();
        var testTexture = Substitute.For<UnityEngine.Texture>();

        textureStorageProvider.SaveTexture(testKey, testTexture);
        var textureToTestAgainst = textureStorageProvider.GetTexture(testKey);

        Assert.AreEqual(testTexture, textureToTestAgainst);
    }

    [Test]
    public void Given_AnEmptyTexture_When_TheImageIsSaved_Then_TheImageShouldReturnNull()
    {
        textureStorageProvider = new TextureStorageProvider();

        textureStorageProvider.SaveTexture(testKey, null);
        var textureToTestAgainst = textureStorageProvider.GetTexture(testKey);

        Assert.IsNull(textureToTestAgainst);
    }
}
