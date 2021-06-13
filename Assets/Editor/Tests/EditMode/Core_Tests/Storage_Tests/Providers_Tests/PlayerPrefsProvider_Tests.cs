using AssemblyCSharp.Assets.Code.Core.Storage.Impl.Providers.PlayerPrefs.Impl;
using NUnit.Framework;

public class PlayerPrefsProvider_Tests
{
    PlayerPrefsProvider playerPrefsProvider;

    string testKey = "testKey";
    string testString = "testString";

    [Test]
    public void Given_PlayerPrefsHasKey_When_HasKeyIsCalled_Then_ReturnsTrue()
    {
        playerPrefsProvider = new PlayerPrefsProvider();
        UnityEngine.PlayerPrefs.SetString(testKey, testString);

        bool hasKey = playerPrefsProvider.HasKey(testKey);

        Assert.IsTrue(hasKey);
    }

    [Test]
    public void Given_PlayerPrefsDoesntHaveKey_When_HasKeyIsCalled_Then_ReturnsFalse()
    {
        playerPrefsProvider = new PlayerPrefsProvider();
        if(UnityEngine.PlayerPrefs.HasKey(testKey))
        {
            UnityEngine.PlayerPrefs.DeleteKey(testKey);
        }        

        bool hasKey = playerPrefsProvider.HasKey(testKey);

        Assert.IsFalse(hasKey);
    }

    [Test]
    public void Given_PlayerPrefsHasKey_WhenGetStringIsCalled_Then_ReturnsString()
    {
        playerPrefsProvider = new PlayerPrefsProvider();
        UnityEngine.PlayerPrefs.SetString(testKey, testString);

        string providerString = playerPrefsProvider.GetString(testKey);

        Assert.AreEqual(testString, providerString);
    }

    [Test]
    public void Given_ProperInputRecieved_When_SetStringIsCalled_Then_StringIsAddedToPlayerPrefsWithKey()
    {
        playerPrefsProvider = new PlayerPrefsProvider();
        UnityEngine.PlayerPrefs.SetString(testKey, testString);

        playerPrefsProvider.SetString(testKey, testString);

        Assert.IsTrue(UnityEngine.PlayerPrefs.HasKey(testKey));
        Assert.AreEqual(testString, UnityEngine.PlayerPrefs.GetString(testKey));
    }
}
