using AssemblyCSharp.Assets.Code.Core.DataManager.Impl.GameObject;
using AssemblyCSharp.Assets.Code.Core.Storage.Impl.GameObject;
using AssemblyCSharp.Assets.Code.Core.Storage.Impl.Providers.Resources.Interface;
using AssemblyCSharp.Assets.Code.Core.Storage.Interface.GameObject;
using NSubstitute;
using NUnit.Framework;

public class GameObjectDataManager_Tests
{   
    [Test]
    public void Given_AValidGameObject_When_GameObjectIsSaved_Then_GameObjectCanBeReturned()
    {        
        var testGameObjectStorageProvider = Substitute.For<IGameObjectStorageProvider>();
        var testGameObjectDataManager = new GameObjectDataManager(testGameObjectStorageProvider);
        var testGameObject = Substitute.For<UnityEngine.Object>() as UnityEngine.GameObject;

        testGameObjectDataManager.SaveGameObject("testKey", testGameObject);
        var returnedGameObject = testGameObjectDataManager.GetGameObject("testKey");

        testGameObjectStorageProvider.Received().SaveGameObject("testKey", testGameObject);
        Assert.AreEqual(testGameObject, returnedGameObject);
    }

    [Test]
    public void Given_AGameObjectExists_When_RemoveKeyIsCalled_Then_GameObjectShouldBeRemoved()
    {
        var testGameObjectStorageProvider = Substitute.For<IGameObjectStorageProvider>();
        var testGameObjectDataManager = new GameObjectDataManager(testGameObjectStorageProvider);
        var testGameObject = Substitute.For<UnityEngine.Object>() as UnityEngine.GameObject;

        testGameObjectDataManager.SaveGameObject("testKey", testGameObject);
        testGameObjectDataManager.RemoveGameObject("testKey");

        Assert.IsNull(testGameObjectDataManager.GetGameObject("testKey"));
    }

    [Test]
    public void Given_ACardModelAlreadyExists_When_GetCardModelIsCalled_Then_TheModelIsReturned()
    {
        var testGameObjectStorageProvider = Substitute.For<IGameObjectStorageProvider>();
        var testGameObjectDataManager = new GameObjectDataManager(testGameObjectStorageProvider);
        var testGameObject = Substitute.For<UnityEngine.Object>() as UnityEngine.GameObject;
        testGameObjectDataManager.SaveGameObject("testKey", testGameObject);

        var returnedModel = testGameObjectDataManager.GetCardModel("testKey");

        Assert.AreEqual(testGameObject, returnedModel);
    }

    [Test]
    public void Given_NoModelsExist_When_TheResourcesAreNotLoaded_TheResourceProviderIsCalledToLoadResources()
    {
        var testResourcesProvider = Substitute.For<IResourcesProvider>();
        var testGameObjectStorageProvider = new GameObjectStorageProvider(testResourcesProvider);
        var testGameObjectDataManager = new GameObjectDataManager(testGameObjectStorageProvider);

        var returnedModel = testGameObjectDataManager.GetCardModel("testKey");

        testResourcesProvider.Received().LoadAll<UnityEngine.GameObject>("Monsters");
        Assert.IsNull(returnedModel);
    }
}
