using AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelComponentsManager;
using AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelComponentsManager.Entities;
using AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelEventsHandler;
using NUnit.Framework;

public class ModelComponentsManager_Tests
{
    [Test]
    public void FirstTest()
    {
        var testEventHandler = new ModelEventHandler();
        var obj = new UnityEngine.GameObject();
        obj.AddComponent(typeof(ModelSettings));
        obj.AddComponent(typeof(ModelComponentsManager));
        var testModelComponentsManager = obj.GetComponent<ModelComponentsManager>();
        
        testModelComponentsManager.Construct(testEventHandler);
        testEventHandler.ActivateModel("testZone");

        Assert.AreEqual(new UnityEngine.Vector3(0, 0, 0), obj.transform.localScale);
    }
}
