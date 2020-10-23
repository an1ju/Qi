using System;

namespace Qi.NetFly.Client
{
    /// <summary>
    /// 控制台程序：内网穿透客户端。
    /// 应该是临时测试时使用，最终也要使用网站和API的形式。
    /// </summary>
    class Program
    {

        
        static void Main(string[] args)
        {
            try
            {

                Qi.NetFly.Core.Client client = new Core.Client();
            }
            catch (Exception ee)
            {

            }
            Console.WriteLine("Hello World!");
            Console.ReadLine();
        }
    }
}
