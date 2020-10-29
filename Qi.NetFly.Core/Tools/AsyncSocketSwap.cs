using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using Qi.NetFly.Core.Model;
using Qi.NetFly.TcpCSFramework;

namespace Qi.NetFly.Core.Tools
{
    public class AsyncSocketSwap
    {
        private Socket m_sockt1;
        private Socket m_sockt2;
        bool m_swaping = false;


        private readonly ClientConfig clientConfig; //omega add 2020年10月29日10:52:10
        private readonly string msgID;

        private class Channel
        {
            public Socket Send { get; set; }

            public Socket Receive { get; set; }
        }

        public AsyncSocketSwap(Socket sockt1, Socket sockt2, ClientConfig clientConfig, string msgID)
        {
            m_sockt1 = sockt1;
            m_sockt2 = sockt2;
            this.clientConfig = clientConfig;
            this.msgID = msgID;
        }
        public void StartSwap()
        {
            m_swaping = true;

            var rcv1 = new DataReciver(m_sockt1);
            rcv1.OnComplete += Rcv1_OnComplete;
            rcv1.ReciveOne();

            var rcv2 = new DataReciver(m_sockt2);
            rcv2.OnComplete += Rcv2_OnComplete;
            rcv2.ReciveOne();
        }

        private void Rcv1_OnComplete(DataReciver send, byte[] buffer, int index, int count)
        {
            m_sockt2.Send(buffer, index, count, SocketFlags.None);
            send.ReciveOne();
        }

        private void Rcv2_OnComplete(DataReciver send, byte[] buffer, int index, int count)
        {
            clientConfig.TransportToService = new TransportToService();
            clientConfig.TransportToService.MsgID = msgID;
            clientConfig.TransportToService.Content = new Coder(Coder.EncodingMothord.UTF8).GetEncodingString(buffer,count);
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(clientConfig);
            byte[] newBuffer = new Coder(Coder.EncodingMothord.UTF8).GetEncodingBytes(json);
            m_sockt1.Send(newBuffer, index, newBuffer.Length, SocketFlags.None);
            clientConfig.TransportToService = null;

            //m_sockt1.Send(buffer, index, count, SocketFlags.None);
            send.ReciveOne();
        }

        internal AsyncSocketSwap BeforeSwap(Action fun)
        {
            if (m_swaping)
            {
                throw new Exception("BeforeSwap must be invoked before StartSwap!");
            }

            fun?.Invoke();
            return this;
        }
    }
}
