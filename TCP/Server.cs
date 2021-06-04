using GoBang.TCP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GoBang
{
    
    class Server
    {
        /*********************************************************************************/

        
        private int max_Client = 50;  //服务器接入的最大设备数

        private int client_Count; //当前接入的设备数

        private TcpListener server = null;

        private List<Client_Attributes> clients_list;//当前接入设备的会话端列表//1在线 2匹配 3 以有匹配对象，4在游戏中

        private Dictionary<Client_Attributes, Client_Attributes> gaming_list; //存储已匹配的客户端

        private bool disposed = false;

        private bool IsRunning = false;//服务器是否在运行；

        private BinaryReader rs;

        private BinaryWriter ws;


        /************************************************************************************/

        public Server()
        {
            //本机ip地址与未注册port
            server = new TcpListener(IPAddress.Parse("172.28.48.1"), 6800);

            clients_list = new List<Client_Attributes>();

            gaming_list = new Dictionary<Client_Attributes, Client_Attributes>();

            Start_Server();
        }
        public void Start_Server()
        {
            if (!IsRunning)
            {
                server.Start();
                IsRunning = true;
            }
            Recieve();

        }
        private async void Recieve()
        {
            await Task.Run(() => {
                while (true)
                {
                    if (!server.Pending())
                    {
                    }
                    else
                    {
                        TcpClient client = server.AcceptTcpClient();
                        accept(client);
                    }
                }

            });
        }
        /******************************************************
         * 
         * 通过AsyncCallBack()调用的异步函数，处理接入客户端的client
         * 该client会调用
         * 
         * *****************************************************/

        private void accept(TcpClient client)
        {
            Console.WriteLine("连接成功！");
            //调用发送方的一个赋值函数
            if (client_Count >= max_Client)
            {
                //不再接入
            }
            else
            {
                client_Count++;
                Client_Attributes ca = new Client_Attributes(null, 1, false, client);
                clients_list.Add(ca);
                Severs_Data_process(ca);
            }

        }
        /**********************************************************
         * 服务器循环接收与此client的数据
         * *********************************************************/

        private async void Severs_Data_process(Client_Attributes ca)
        {
            await Task.Run(() =>
            {
                rs = new BinaryReader(ca.get_client().GetStream());
                byte[] _recvBuffer;
                int len = 0;
                while (//Is_connect
                       ca.get_client().Connected)
                {
                    try
                    {
                        _recvBuffer = new byte[1024];
                        len = rs.Read(_recvBuffer, 0, _recvBuffer.Length);
                        //len = tcpclient.ReceiveBufferSize;        //获取数据长度
                    }
                    catch (Exception)
                    {
                        break;
                    }
                    if (len == 0)
                    {
                        //the client has disconnected from server   || or no data?
                        break;
                    }
                    /*
                     * 假设与netstream一样，没有流的时候会阻塞
                     * 而一次发送的数据正好读完
                     */
                    String message = Encoding.Default.GetString(_recvBuffer);
                    message = message.Substring(0, len);
                    _recvBuffer = null;
                    if (message == null)
                        Console.WriteLine("数据为空");
                    Console.WriteLine(message);
                    if(!ca.get_In_game())
                        Match_data(ca, message);
                    else
                    {
                        Send(message, gaming_list[ca].get_client());
                    }
                        //发送游戏数据
                }
            });
           
        }
        public void set_NorS(String name,int status,TcpClient tcp)
        {
            foreach(Client_Attributes ca in clients_list)
            {
                if(tcp == ca.get_client())
                {
                    if (name!=null)
                        ca.set_Nmae(name);
                    if (status!=0)
                        ca.set_Status(status);
                }
            }
        }
        /******************************************************
        * 
        * 一个服务器异步发送的函数
        * 
        * *****************************************************/

        //发送函数
        public async void Send(string msg, TcpClient client)
        {
             await Task.Run(() =>
            {
                ws = new BinaryWriter(client.GetStream());
                byte[] data = Encoding.Default.GetBytes(msg);
                try
                {
                    ws.Write(data, 0, data.Length);
                    ws.Flush();
                }
                catch (Exception)
                {
                    //TODO 处理异常
                }
            });
        }
        /**********************************************************
         * 匹配函数
         * 同步函数，对发来的数据进行处理
         * 不需要异步，没处理完不能再接收数据客户端也不会发
         * ********************************************************/
        public  void Match_data(Client_Attributes ca,String message)
        {

            switch (ca.get_Status()) {
                case 1:
                    if (message.Contains(":2")){
                        set_NorS(null, 2, ca.get_client());
                        Match_oppoent(ca);
                    }
                    else
                    {
                        Console.WriteLine("收到名字");
                        set_NorS(message, 0, ca.get_client());
                        Send(message, ca.get_client());
                    }
                        
                    break;
                case 2:
                    if (message.Contains("Oppent:"))
                    {
                        ca.set_Status(3);
                        Ack_opponent(message.Substring(message.IndexOf(":") + 1),true,ca);
                        //建立连接,回复确认
                        ca.set_In_game(true);
                        ca.set_Status(4);

                    }break;
                case 3:
                    if(message.Contains("Oppent")){
                        //拒绝并回复
                        Ack_opponent(message.Substring(message.IndexOf(":") + 1), false,ca);
                    }//晚了一步
                    if (message.Equals("Ido:"+ca.get_Nmae()))
                        //收到回复并且名字是自己
                    {
                        Send("::" + gaming_list[ca].get_Nmae(), ca.get_client()); //获得对方的名字
                        ca.set_In_game(true);
                        ca.set_Status(4);
                        //建立连接
                    }
                    else if(message.Equals("Sorry:" + ca.get_Nmae()))
                    {
                        //连接建立失败拒绝

                        set_NorS(null, 2, ca.get_client());
                        gaming_list.Remove(ca);
                        Match_oppoent(ca);
                    }
                    break;
            }

        }

        private void Ack_opponent(String name,bool confirm,Client_Attributes cal)
        {
            foreach (Client_Attributes ca in clients_list)
            {
                if (ca.get_Nmae().Equals(name))
                {
                    if (confirm)
                    {
                        Send("Ido:" + name, ca.get_client());
                        Send("::" + ca.get_Nmae(), cal.get_client());
                        gaming_list.Add(cal, ca); //添加对象
                        Send(ca.get_Nmae(), cal.get_client());//确认连接之后获取对方的名字
                    }
                    else
                    Send("Sorry:" + name, ca.get_client());
                    break;
                }
            }
        }
        private async void Match_oppoent(Client_Attributes ca)
        {
            //遍历list获取同为2的client
            await Task.Run(()=>{
                while(ca.get_Status()!=3)
                foreach(Client_Attributes cal in clients_list)
                {
                        if (cal.get_Status() == 2 && !cal.get_client().Equals(ca.get_client()) && ca.get_Status() != 3)
                        {
                            //状态为3，表示以有匹配对象
                            ca.set_Status(3);
                            Send("Oppent:" + cal.get_Nmae(), cal.get_client());
                            gaming_list.Add(ca, cal); //添加对象
                            break;
                        }
                        else if (ca.get_Status() == 3)
                            break;
                }
            });
        }
        /************************************************************************
         * 定义事件
         * client连接与断开
         * 比如，调用RaiseClientConnected()时，实际上是触发了ClientConnected事件
         * 而此事件有一个对应的处理函数
         * 目前觉得事件没有什么好处，由其是自定义事件
         * 在此仅把他们作为“函数声明”
         * **********************************************************************/

        public event EventHandler<TCPEventArgs> ClientConnected;
        public event EventHandler<TCPEventArgs> ClientDisconnected;

        private void RaiseClientConnected(TcpClient handle)
        {
            if (ClientConnected != null)
            {
                ClientConnected(this, new TCPEventArgs(handle));
            }
        }
        private void RaiseClientDisconnected(Socket client)
        {
            if (ClientDisconnected != null)
            {
                ClientDisconnected(this, new TCPEventArgs("连接断开"));
            }
        }

        public event EventHandler<TCPEventArgs> DataReceived;

        private void RaiseDataReceived(TcpClient handle)
        {
            if (DataReceived != null)
            {
                DataReceived(this, new TCPEventArgs(handle));
            }
        }

        public event EventHandler<TCPEventArgs> CompletedSend;

        private void RaiseCompletedSend(TcpClient handle)
        {
            if (CompletedSend != null)
            {
                CompletedSend(this, new TCPEventArgs(handle));
            }
        }

        public event EventHandler<TCPEventArgs> NetError;
        private void RaiseNetError(TcpClient handle)
        {
            if (NetError != null)
            {
                NetError(this, new TCPEventArgs(handle));
            }
        }
        public event EventHandler<TCPEventArgs> OtherException;
        /// <summary>
        /// 触发异常事件
        /// </summary>
        /// <param name="state"></param>
        private void RaiseOtherException(TcpClient handle, string descrip)
        {
            if (OtherException != null)
            {
                OtherException(this, new TCPEventArgs(descrip, handle));
            }
        }
        private void RaiseOtherException(TcpClient handle)
        {
            RaiseOtherException(handle, "");
        }

        /***********************************************
         * 关闭客户端连接
         * *********************************************/

        public void Close( Client_Attributes kv)
        {
                clients_list.Remove(kv);
                kv.get_client().Dispose();
                client_Count--;
                RaiseClientConnected(kv.get_client());
                //TODO 触发关闭事件
        }
        /***********************************************
           * 关闭所有客户端连接
           * *********************************************/
        public void CloseAllClient()
        {
            foreach (Client_Attributes kv in clients_list)
            {
                Close(kv);
            }
            client_Count = 0;
            clients_list.Clear();
        }
        /***********************************************
         * 清空所使用的资源
         * *********************************************/
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /***********************************************
          * 关闭服务器
          * *********************************************/
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    try
                    {
                        Stop();
                        if (server != null)
                        {
                            server = null;
                        }
                    }
                    catch (SocketException)
                    {
                        //TODO 异常
                    }
                }
                disposed = true;
            }
        }
        public void Stop()
        {
            if (IsRunning)
            {
                IsRunning = false;
                server.Stop();
                CloseAllClient();
                //TODO 关闭对所有客户端的连接
            }
        }
    }
}
