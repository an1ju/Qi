using System;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Qi.NetFly.TcpCSFramework
{
    /// <summary>
    /// 网络通讯事件模型委托
    /// </summary>
    public delegate void NetEvent(object sender, NetEventArgs e);

    /// <summary>
    /// 提供TCP连接服务的服务器类
    /// </summary>
    public class TcpSvr
    {
        #region 定义字段

        /// <summary>
        /// 默认的服务器最大连接客户端端数据
        /// </summary>
        public const int DefaultMaxClient = 1024;

        /// <summary>
        /// 接收数据缓冲区大小64K
        /// </summary>
        public const int DefaultBufferSize = 64 * 1024;

        /// <summary>
        /// 最大数据报文大小
        /// </summary>
        public const int MaxDatagramSize = 640 * 1024;

        /// <summary>
        /// 报文解析器
        /// </summary>
        private DatagramResolver _resolver;

        /// <summary>
        /// 通讯格式编码解码器
        /// </summary>
        private Coder _coder;

        /// <summary>
        /// 服务器程序使用的端口
        /// </summary>
        private ushort _port;

        /// <summary>
        /// 服务器程序允许的最大客户端连接数
        /// </summary>
        private ushort _maxClient;

        /// <summary>
        /// 服务器的运行状态
        /// </summary>
        private bool _isRun;

        /// <summary>
        /// 接收数据缓冲区
        /// </summary>
        private byte[] _recvDataBuffer;

        /// <summary>
        /// 服务器使用的异步Socket类,
        /// </summary>
        private Socket _svrSock;

        /// <summary>
        /// 保存所有客户端会话的哈希表
        /// </summary>
        private Hashtable _sessionTable;

        /// <summary>
        /// 当前的连接的客户端数
        /// </summary>
        private ushort _clientCount;

        #endregion

        #region 事件定义

        /// <summary>
        /// 客户端建立连接事件
        /// </summary>
        public event NetEvent ClientConn;

        /// <summary>
        /// 客户端关闭事件
        /// </summary>
        public event NetEvent ClientClose;

        /// <summary>
        /// 服务器已经满事件
        /// </summary>
        public event NetEvent ServerFull;

        /// <summary>
        /// 服务器接收到数据事件
        /// </summary>
        public event NetEvent RecvData;

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="port">服务器端监听的端口号</param>
        /// <param name="maxClient">服务器能容纳客户端的最大能力</param>
        /// <param name="encodingMothord">通讯的编码方式</param>
        public TcpSvr(ushort port, ushort maxClient, Coder coder)
        {
            _port = port;
            _maxClient = maxClient;
            _coder = coder;
        }


        /// <summary>
        /// 构造函数(默认使用Default编码方式)
        /// </summary>
        /// <param name="port">服务器端监听的端口号</param>
        /// <param name="maxClient">服务器能容纳客户端的最大能力</param>
        public TcpSvr(ushort port, ushort maxClient)
        {
            _port = port;
            _maxClient = maxClient;
            _coder = new Coder(Coder.EncodingMothord.Default);
        }


        /// <summary>
        /// 构造函数(默认使用Default编码方式和DefaultMaxClient(100)个客户端的容量)
        /// </summary>
        /// <param name="port">服务器端监听的端口号</param>
        public TcpSvr(ushort port)
            : this(port, DefaultMaxClient)
        {
        }

        #endregion

        #region 属性

        /// <summary>
        /// 服务器的Socket对象
        /// </summary>
        public Socket ServerSocket
        {
            get
            {
                return _svrSock;
            }
        }

        /// <summary>
        /// 数据报文分析器
        /// </summary>
        public DatagramResolver Resovlver
        {
            get
            {
                return _resolver;
            }
            set
            {
                _resolver = value;
            }
        }

        /// <summary>
        /// 客户端会话数组,保存所有的客户端,不允许对该数组的内容进行修改
        /// </summary>
        public Hashtable SessionTable
        {
            get
            {
                return _sessionTable;
            }
        }

        /// <summary>
        /// 服务器可以容纳客户端的最大能力
        /// </summary>
        public int Capacity
        {
            get
            {
                return _maxClient;
            }
        }

        /// <summary>
        /// 当前的客户端连接数
        /// </summary>
        public int SessionCount
        {
            get
            {
                return _clientCount;
            }
        }

        /// <summary>
        /// 服务器运行状态
        /// </summary>
        public bool IsRun
        {
            get
            {
                return _isRun;
            }

        }

        public ushort Port
        {
            get
            {
                return _port;
            }
        }

        #endregion

        #region 公有方法

        /// <summary>
        /// 启动服务器程序,开始监听客户端请求
        /// </summary>
        public virtual void Start()
        {
            if (_isRun)
            {
                throw (new ApplicationException("TcpSvr已经在运行."));
            }
            _sessionTable = new Hashtable(1024);
            _recvDataBuffer = new byte[DefaultBufferSize];
            _svrSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint iep = new IPEndPoint(IPAddress.Any, _port);
            _svrSock.Bind(iep);
            _svrSock.Listen(1024);
            _svrSock.BeginAccept(new AsyncCallback(AcceptConn), _svrSock);

            _isRun = true;
        }

        /// <summary>
        /// 停止服务器程序,所有与客户端的连接将关闭
        /// </summary>
        public virtual void Stop()
        {
            if (!_isRun)
            {
                throw (new ApplicationException("TcpSvr已经停止"));
            }
            _isRun = false;
            if (_svrSock.Connected)
            {
                _svrSock.Shutdown(SocketShutdown.Both);
            }

            CloseAllClient();
            _svrSock.Close();
            _sessionTable = null;
        }


        /// <summary>
        /// 关闭所有的客户端会话,与所有的客户端连接会断开
        /// </summary>
        public virtual void CloseAllClient()
        {
            foreach (Session client in _sessionTable.Values)
            {
                client.Close();
            }

            _clientCount = 0;
            _sessionTable.Clear();
        }

        /// <summary>
        /// 关闭一个与客户端之间的会话
        /// </summary>
        /// <param name="closeClient">需要关闭的客户端会话对象</param>
        public virtual void CloseSession(Session closeClient)
        {
            Debug.Assert(closeClient != null);

            if (closeClient != null)
            {

                closeClient.Datagram = null;
                _sessionTable.Remove(closeClient.ID);
                _clientCount--;

                //客户端强制关闭链接
                if (ClientClose != null)
                {
                    ClientClose(this, new NetEventArgs(closeClient));
                }

                closeClient.Close();
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="recvDataClient">接收数据的客户端会话</param>
        /// <param name="datagram">数据报文</param>
        public virtual void Send(Session recvDataClient, string datagram)
        {
            //获得数据编码
            byte[] data = _coder.GetEncodingBytes(datagram);

            recvDataClient.ClientSocket.BeginSend(data, 0, data.Length, SocketFlags.None,
             new AsyncCallback(SendDataEnd), recvDataClient.ClientSocket);
        }
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="recvDataClient">接收数据的客户端会话</param>
        /// <param name="data"></param>
        public virtual void Send(Session recvDataClient, byte[] data)
        {
            //获得数据编码
            //byte[] data = _coder.GetEncodingBytes(datagram);

            recvDataClient.ClientSocket.BeginSend(data, 0, data.Length, SocketFlags.None,
             new AsyncCallback(SendDataEnd), recvDataClient.ClientSocket);
        }

        #endregion

        #region 受保护方法
        /// <summary>
        /// 关闭一个客户端Socket,首先需要关闭Session
        /// </summary>
        /// <param name="client">目标Socket对象</param>
        /// <param name="exitType">客户端退出的类型</param>
        protected virtual void CloseClient(Socket client, Session.ExitType exitType)
        {
            Debug.Assert(client != null);

            //查找该客户端是否存在,如果不存在,抛出异常
            Session closeClient = FindSession(client);

            if (closeClient != null)
            {
                closeClient.TypeOfExit = exitType;

                if (closeClient != null)
                {
                    CloseSession(closeClient);
                }
                else
                {
                    throw (new ApplicationException("需要关闭的Socket对象不存在"));
                }
            }

        }

        /// <summary>
        /// 客户端连接处理函数
        /// </summary>
        /// <param name="iar">欲建立服务器连接的Socket对象</param>
        protected virtual void AcceptConn(IAsyncResult iar)
        {
            //如果服务器停止了服务,就不能再接收新的客户端
            if (!_isRun)
            {
                return;
            }

            //接受一个客户端的连接请求
            Socket oldserver = (Socket)iar.AsyncState;
            Socket client = oldserver.EndAccept(iar);

            //检查是否达到最大的允许的客户端数目
            if (_clientCount == _maxClient)
            {
                if (ServerFull != null)
                {
                    ServerFull(this, new NetEventArgs(new Session(client)));
                }
            }
            else
            {
                //新建一个客户端连接
                Session newSession = new Session(client);
                _sessionTable.Add(newSession.ID, newSession);

                _clientCount++;

                newSession.RecvDataBuffer = new byte[16 * 1024];
                //开始接受来自该客户端的数据
                client.BeginReceive(newSession.RecvDataBuffer, 0, newSession.RecvDataBuffer.Length, SocketFlags.None,
                 new AsyncCallback(ReceiveData), newSession);

                //新的客户段连接,发出通知
                if (ClientConn != null)
                {
                    ClientConn(this, new NetEventArgs(newSession));
                }
            }

            //继续接受客户端
            _svrSock.BeginAccept(new AsyncCallback(AcceptConn), _svrSock);
        }

        /// <summary>
        /// 通过Socket对象查找Session对象
        /// </summary>
        /// <param name="client"></param>
        /// <returns>找到的Session对象,如果为null,说明并不存在该会话</returns>
        private Session FindSession(Socket client)
        {
            SessionId id = new SessionId((int)client.Handle);
            return (Session)_sessionTable[id];
        }

        /// <summary>
        /// 接受数据完成处理函数，异步的特性就体现在这个函数中，
        /// 收到数据后，会自动解析为字符串报文
        /// </summary>
        /// <param name="iar">目标客户端Socket</param>
        protected virtual void ReceiveData(IAsyncResult iar)
        {
            Session sendDataSession = (Session)iar.AsyncState;
            Socket client = sendDataSession.ClientSocket;

            try
            {
                //如果两次开始了异步的接收,所以当客户端退出的时候
                //会两次执行EndReceive
                int recv = client.EndReceive(iar);

                if (recv == 0)
                {
                    CloseClient(client, Session.ExitType.NormalExit);
                    return;
                }

                string receivedData = _coder.GetEncodingString(sendDataSession.RecvDataBuffer, recv);
                string receivedData2 = _coder.GetEncodingString(_recvDataBuffer, recv);

                //发布收到数据的事件
                //if (RecvData != null)
                //{
                //    //Session sendDataSession = FindSession(client);

                //    Debug.Assert(sendDataSession != null);

                //    if (_resolver != null)
                //    {
                //        if (sendDataSession.Datagram != null && sendDataSession.Datagram.Length != 0)
                //        {
                //            receivedData = sendDataSession.Datagram + receivedData;
                //        }

                //        string[] recvDatagrams = _resolver.Resolve(ref receivedData);

                //        foreach (string newDatagram in recvDatagrams)
                //        {
                //            ICloneable copySession = (ICloneable)sendDataSession;
                //            Session clientSession = (Session)copySession.Clone();

                //            string strDatagram = newDatagram;
                //            //clientSession.ClassName = this.GetClassFullName(ref strDatagram);
                //            clientSession.Datagram = strDatagram;

                //            //发布一个报文消息
                //            RecvData(this, new NetEventArgs(clientSession));
                //        }

                //        //剩余的代码片断,下次接收的时候使用
                //        sendDataSession.Datagram = receivedData;

                //        if (sendDataSession.Datagram.Length > MaxDatagramSize)
                //        {
                //            sendDataSession.Datagram = null;
                //        }
                //    }
                //    else
                //    {
                //        ICloneable copySession = (ICloneable)sendDataSession;
                //        Session clientSession = (Session)copySession.Clone();
                //        //clientSession.ClassName = this.GetClassFullName(ref receivedData);
                //        clientSession.Datagram = receivedData;

                //        RecvData(this, new NetEventArgs(clientSession));
                //    }
                //}

                {
                    ICloneable copySession = (ICloneable)sendDataSession;
                    Session clientSession = (Session)copySession.Clone();
                    //clientSession.ClassName = this.GetClassFullName(ref receivedData);
                    clientSession.Datagram = receivedData;
                    clientSession.RecvDataBuffer = sendDataSession.RecvDataBuffer;

                    RecvData(this, new NetEventArgs(clientSession));
                }

                //继续接收来自来客户端的数据
                client.BeginReceive(sendDataSession.RecvDataBuffer, 0, sendDataSession.RecvDataBuffer.Length, SocketFlags.None,
                 new AsyncCallback(ReceiveData), sendDataSession);
            }
            catch (SocketException ex)
            {
                if (10054 == ex.ErrorCode)
                {
                    //客户端强制关闭
                    CloseClient(client, Session.ExitType.ExceptionExit);
                }

            }
            catch (ObjectDisposedException ex)
            {
                if (ex != null)
                {
                    ex = null;
                    //DoNothing;
                }
            }
        }

        /// <summary>
        /// 发送数据完成处理函数
        /// </summary>
        /// <param name="iar">目标客户端Socket</param>
        protected virtual void SendDataEnd(IAsyncResult iar)
        {
            Socket client = (Socket)iar.AsyncState;

            int sent = client.EndSend(iar);
        }
        /// <summary>
        /// 协议解析方法
        /// </summary>
        /// <param name="Term"></param>
        /// <returns></returns>
        private string GetClassFullName(ref string Term)
        {
            //{[object name][channel][request id][|param1|param2|param3|...|]}

            string ClassFullName = Term.Substring(2, Term.IndexOf(']') - 2);
            Term = "{" + Term.Substring(Term.IndexOf(']') + 1);

            return ClassFullName;
        }

        #endregion

    }


    /// <summary>
    /// 提供Tcp网络连接服务的客户端类
    /// </summary>
    public class TcpCli
    {
        #region 字段

        /// <summary>
        /// 客户端与服务器之间的会话类
        /// </summary>
        private Session _session;

        /// <summary>
        /// 客户端是否已经连接服务器
        /// </summary>
        private bool _isConnected = false;

        /// <summary>
        /// 接收数据缓冲区大小64K
        /// </summary>
        public const int DefaultBufferSize = 1024;

        /// <summary>
        /// 报文解析器
        /// </summary>
        private DatagramResolver _resolver;

        /// <summary>
        /// 通讯格式编码解码器
        /// </summary>
        private Coder _coder;

        /// <summary>
        /// 接收数据缓冲区
        /// </summary>
        private byte[] _recvDataBuffer = new byte[DefaultBufferSize];

        #endregion

        #region 事件定义

        /// <summary>
        /// 已经连接服务器事件
        /// </summary>
        public event NetEvent ConnectedServer;

        /// <summary>
        /// 接收到数据报文事件
        /// </summary>
        public event NetEvent ReceivedDatagram;

        /// <summary>
        /// 连接断开事件
        /// </summary>
        public event NetEvent DisConnectedServer;
        #endregion

        #region 属性

        /// <summary>
        /// 返回客户端与服务器之间的会话对象
        /// </summary>
        public Session ClientSession
        {
            get
            {
                return _session;
            }
        }

        /// <summary>
        /// 返回客户端与服务器之间的连接状态
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return _isConnected;
            }
        }

        /// <summary>
        /// 数据报文分析器
        /// </summary>
        public DatagramResolver Resovlver
        {
            get
            {
                return _resolver;
            }
            set
            {
                _resolver = value;
            }
        }

        /// <summary>
        /// 编码解码器
        /// </summary>
        public Coder ServerCoder
        {
            get
            {
                return _coder;
            }
        }

        #endregion

        #region 公有方法

        /// <summary>
        /// 默认构造函数,使用默认的编码格式
        /// </summary>
        public TcpCli()
        {
            _coder = new Coder(Coder.EncodingMothord.Default);
        }

        /// <summary>
        /// 构造函数,使用一个特定的编码器来初始化
        /// </summary>
        /// <param name="_coder">报文编码器</param>
        public TcpCli(Coder coder)
        {

            _coder = coder;
        }

        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <param name="ip">服务器IP地址</param>
        /// <param name="port">服务器端口</param>
        public virtual void Connect(string ip, int port)
        {
            /*
            if (IsConnected)
            {
                Debug.Assert(_session != null);
                Close();
            }
            */

            Socket newsock = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint iep = new IPEndPoint(IPAddress.Parse(ip), port);
            newsock.BeginConnect(iep, new AsyncCallback(Connected), newsock);

        }

        /// <summary>
        /// 发送数据报文
        /// </summary>
        /// <param name="datagram"></param>
        public virtual void Send(string datagram)
        {
            if (datagram.Length == 0)
            {
                return;
            }

            if (!_isConnected)
            {
                throw (new ApplicationException("没有连接服务器，不能发送数据"));
            }

            //获得报文的编码字节
            byte[] data = _coder.GetEncodingBytes(datagram);

            //byte[] data = new byte[] { 68, 04, 07, 00, 00, 00 };

            _session.ClientSocket.BeginSend(data, 0, data.Length, SocketFlags.None,
             new AsyncCallback(SendDataEnd), _session.ClientSocket);
        }

        /// <summary>
        /// 发送数据报文
        /// </summary>
        /// <param name="data"></param>
        public virtual void Send(byte[] data)
        {


            if (!_isConnected)
            {
                throw (new ApplicationException("没有连接服务器，不能发送数据"));
            }



            //byte[] data = new byte[] { 68, 04, 07, 00, 00, 00 };

            _session.ClientSocket.BeginSend(data, 0, data.Length, SocketFlags.None,
             new AsyncCallback(SendDataEnd), _session.ClientSocket);
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public virtual void Close()
        {
            if (!_isConnected)
            {
                return;
            }

            _session.Close();
            _session = null;
            _isConnected = false;
        }

        #endregion

        #region 受保护方法

        /// <summary>
        /// 数据发送完成处理函数
        /// </summary>
        /// <param name="iar"></param>
        protected virtual void SendDataEnd(IAsyncResult iar)
        {
            Socket remote = (Socket)iar.AsyncState;
            int sent = remote.EndSend(iar);
            Debug.Assert(sent != 0);
        }

        /// <summary>
        /// 建立Tcp连接后处理过程
        /// </summary>
        /// <param name="iar">异步Socket</param>
        protected virtual void Connected(IAsyncResult iar)
        {
            Socket socket = (Socket)iar.AsyncState;
            socket.EndConnect(iar);

            //创建新的会话
            _session = new Session(socket);
            _isConnected = true;

            //触发连接建立事件
            if (ConnectedServer != null)
            {
                ConnectedServer(this, new NetEventArgs(_session));
            }

            _session.ClientSocket.BeginReceive(_recvDataBuffer, 0,
             DefaultBufferSize, SocketFlags.None,
             new AsyncCallback(RecvData), socket);
        }

        /// <summary>
        /// 数据接收处理函数
        /// </summary>
        /// <param name="iar">异步Socket</param>
        protected virtual void RecvData(IAsyncResult iar)
        {
            Socket remote = (Socket)iar.AsyncState;

            try
            {
                int recv = remote.EndReceive(iar);

                if (recv == 0)
                {
                    _session.TypeOfExit = Session.ExitType.NormalExit;

                    if (DisConnectedServer != null)
                    {
                        DisConnectedServer(this, new NetEventArgs(_session));
                    }

                    return;
                }

                string receivedData = _coder.GetEncodingString(_recvDataBuffer, recv);

                //通过事件发布收到的报文
                //if (ReceivedDatagram != null)
                //{
                //    if (_resolver != null)
                //    {
                //        if (_session.Datagram != null && _session.Datagram.Length != 0)
                //        {
                //            receivedData = _session.Datagram + receivedData;
                //        }

                //        string[] recvDatagrams = _resolver.Resolve(ref receivedData);
                //        foreach (string newDatagram in recvDatagrams)
                //        {
                //            ICloneable copySession = (ICloneable)_session;
                //            Session clientSession = (Session)copySession.Clone();
                //            clientSession.Datagram = newDatagram;

                //            //发布一个报文消息
                //            ReceivedDatagram(this, new NetEventArgs(clientSession));
                //        }

                //        //剩余的代码片断,下次接收的时候使用
                //        _session.Datagram = receivedData;
                //    }
                //    else
                //    {
                //        ICloneable copySession = (ICloneable)_session;
                //        Session clientSession = (Session)copySession.Clone();
                //        clientSession.Datagram = receivedData;

                //        ReceivedDatagram(this, new NetEventArgs(clientSession));
                //    }

                //}//end of if(ReceivedDatagram != null)

                {
                    //omega 添加 2013年9月13日11:20:07
                    ICloneable copySession = (ICloneable)_session;
                    Session clientSession = (Session)copySession.Clone();
                    clientSession.Datagram = receivedData;
                    clientSession.RecvDataBuffer = _recvDataBuffer;

                    ReceivedDatagram(this, new NetEventArgs(clientSession));
                }


                //继续接收数据
                _session.ClientSocket.BeginReceive(_recvDataBuffer, 0, DefaultBufferSize, SocketFlags.None,
                 new AsyncCallback(RecvData), _session.ClientSocket);
            }
            catch (SocketException ex)
            {
                //客户端退出
                if (10054 == ex.ErrorCode)
                {
                    _session.TypeOfExit = Session.ExitType.ExceptionExit;

                    if (DisConnectedServer != null)
                    {
                        DisConnectedServer(this, new NetEventArgs(_session));
                    }
                }
                else
                {
                    throw (ex);
                }
            }
            catch (ObjectDisposedException ex)
            {
                if (ex != null)
                {
                    ex = null;
                    //DoNothing;
                }
            }
        }
        #endregion
    }
}
