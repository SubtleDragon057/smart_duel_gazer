using AssemblyCSharp.Assets.Code.Core.Storage.Impl.Texture;
using NUnit.Framework;

public class TextureStorageProvider_Tests
{
    private TextureStorageProvider testProvider = new TextureStorageProvider();
    
    [Test]
    public void Given_ATextureInput_When_SaveTextureIsCalled_Then_TextureIsSaved()
    {
        var testKey = "testKey";
        var testTexture = Substitute.For<UnityEngine.Texture>();

        testProvider.SaveTexture(testKey, testTexture);
        var image = testProvider.GetTexture(testKey);

        Assert.AreEqual(testTexture, image);
    }
}
