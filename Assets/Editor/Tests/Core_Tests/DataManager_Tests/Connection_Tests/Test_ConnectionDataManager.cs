using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.Connection.Entities;
using AssemblyCSharp.Assets.Code.Core.Storage.Interface.Connection.Models;
using NUnit.Framework;

public class Test_ConnectionDataManager
{
    [Test]
    public void SaveConnectionInfo_Test()
    {
        var connectionInfo = new ConnectionInfo("Test_IpAddress", "Test_Port");
        
        var model = new ConnectionInfoModel(connectionInfo.IpAddress, connectionInfo.Port);

        Assert.AreEqual("Test_IpAddress", model.IpAddress);
        Assert.AreEqual("Test_Port", model.Port);
    }
}
