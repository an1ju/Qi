using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest_Qi_NetFly_Core
{
    /// <summary>
    /// �������˲���
    /// </summary>
    [TestClass]
    public class UnitTest_ForService
    {
        [TestMethod("����������������")]
        public void TestMethod1()
        {
            Qi.NetFly.Core.Service s = new Qi.NetFly.Core.Service();
        }

        [TestMethod("�ͻ��˿���̨���Գ�������")]
        public void TestMethod2()
        {
            Qi.NetFly.Core.Client s = new Qi.NetFly.Core.Client();
        }
    }
}
