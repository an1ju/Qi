using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest_Qi_NetFly_Core
{
    /// <summary>
    /// 服务器端测试
    /// </summary>
    [TestClass]
    public class UnitTest_ForService
    {
        [TestMethod("服务器端启动测试")]
        public void TestMethod1()
        {
            Qi.NetFly.Core.Service s = new Qi.NetFly.Core.Service();
        }
    }
}
