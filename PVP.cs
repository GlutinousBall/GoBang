using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GoBang
{
    public class PVP
    {
        /*******************************************************************************/

        int[,] chess = new int[9, 12];//棋盘
        TcpClient tcpclient = null;
        private NetworkStream stream;
        private byte[] _recvBuffer;
        private byte[] _sendBuffer;
        private String message = null;
        public static bool isconeect = false;
        public static String Oppenonet_name = null;
        private static String Name;



        /*******************************************************************************/

        /*********************************************************
         * 构造函数
         * 初始化客户端和数据流
         * *******************************************************/
        public PVP(String name)
        {
            Connnect_sever(name);
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 12; j++)
                {
                    chess[i, j] = 0;
                }
        }
        private async void Connnect_sever(String name)
        {
            await Task.Run(async () =>
            {
                tcpclient = new TcpClient("172.25.32.1", 6800);
                isconeect = true;

                stream = tcpclient.GetStream();
                _recvBuffer = new byte[1024];
                await SendData("name"+name);
                Name = name;
            });

        }
        public bool get_connet() { return isconeect; }
        /**********************************************************
         * 发送一个匹配信息
         * *******************************************************/
        public async void send_Match_Data()
        {
            await SendData(":2");
            await Matching_Oppenonet();
        }
        public async Task SendData(string msg)
        {
            await Task.Factory.StartNew(() =>
            {
                Console.WriteLine("尝试发送:"+msg);
                byte[] data = new byte[1024];
                data = Encoding.Default.GetBytes(msg);
                try
                {
                    stream.Write(data, 0, data.Length);
                    stream.Flush();
                }
                catch (Exception)
                {
                    //TODO 处理异常
                }
            });
        }
        /**********************************************************
         * 设置自身棋盘
         * 异步发送发送数据出去
         * ********************************************************/
        public void set_Chessboard(String tag)
        {
            String str, str1;
            int a, b;
            str = tag.Substring(0, 1);
            str1 = tag.Substring(2);
            a = int.Parse(str);
            b = int.Parse(str1);
            chess[a, b] = 1;
            SendData(tag);  //异步发送数据
        }

        /**********************************************************
         * 异步设置对手棋盘
         * 需要在form中对棋子进行无效化
         * 同步等待client接收到数据
         * 如果为空，代表断开连接（写出相应事件）
         * ********************************************************/
        public async Task<int> set_OPponent_Chess()
        {
           return await Task.Run(async () =>
            {
                var s= await RecevieData();
                String tag = s.ToString();
                String str, str1;
                int a, b;
                str = tag.Substring(0, 1);
                str1 = tag.Substring(2);
                a = int.Parse(str);
                b = int.Parse(str1);
                chess[a, b] = 2;
                return a * 100 + b;
            });

        }
        /**********************************************************
         * 一个等待客户端发来数据的函数
         * 与主线程同步，因为没有接收到数据是不能进行下一步的
         * ********************************************************/
        public async Task<String> RecevieData()
        {

            int len = 0;
            String s = await Task<String>.Run(() =>
            {
                if (tcpclient.Connected)
                {
                    try
                    {
                        _recvBuffer=new byte[1024];
                        len = stream.Read(_recvBuffer, 0, _recvBuffer.Length);
                        //len = tcpclient.ReceiveBufferSize;        //获取数据长度
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                    if (len == 0)
                    {
                        //the client has disconnected from server   || or no data?
                        return null;
                    }
                    /*
                     * 假设与netstream一样，没有流的时候会阻塞
                     * 而一次发送的数据正好读完
                     */
                    message = Encoding.Default.GetString(_recvBuffer);
                    message = message.Substring(0, len);
                    Console.WriteLine("从服务端来的数据为  " + message);
                    _recvBuffer = null;
                    if (message.Contains("chat"))
                    {
                        Form1.chat_text = message;
                    }
                    return message;
                }
                return "";
            });
            return s;
        }
        /**************************************      匹配      *************************************/
        private async Task Matching_Oppenonet()
        {
            String rec;
            await Task.Run(async () => {
                while (tcpclient.Connected)
                {
                    var re = await RecevieData();
                        rec = re.ToString();
                        if (rec.Contains("::"))
                        {
                            Oppenonet_name = rec.Substring(2);
                            break;
                        }
                        await SendData(Name + " " + rec);//无情转发
                }
            });
        }
        public bool IsWin(int value)
        {
            if (Hor_win(value) || Ver_win(value) || Bev_Win(value))
                return true;
            else
                return false;
        }
        private bool Hor_win(int value)
        {
            int k = 0;
            for (int i = 0; i < 9; i++) //横着的五子
            {
                for (int j = 0; j < 12; j++)
                {
                    if (chess[i, j] == value)
                    {
                        k++;
                        if (k == 5)
                            return true;
                    }
                    else
                        k = 0;
                }
                k = 0;

            }
            return false;
        }
        private bool Ver_win(int value)
        {
            int k = 0;
            for (int i = 0; i < 12; i++) //竖着的五子
            {
                for (int j = 0; j < 9; j++)
                {
                    if (chess[j, i] == value)
                    {
                        k++;
                        if (k == 5)
                            return true;
                    }
                    else
                        k = 0;
                }
                k = 0;
            }
            return false;
        }
        private bool Bev_Win(int value)
        {
            int k = 0;
            int c = 0;
            for (int i = 0; i < 5; i++)//斜着五子
            {
                c = i;
                for (int j = 0; j < 9 - i; j++)
                {
                    if (chess[c, j] == value)
                    {
                        k++;
                        if (k == 5)
                            return true;
                    }
                    else
                        k = 0;
                    c++;
                }
                k = 0;
            }
            for (int i = 0; i < 5; i++)//斜着五子
            {
                c = i;
                for (int j = 11; j > (2 + i); j--)
                {
                    if (chess[c, j] == value)
                    {
                        k++;
                        if (k == 5)
                            return true;
                    }
                    else
                        k = 0;
                    c++;
                }
                k = 0;
            }
            for (int i = 10; i > 3; i--)//斜着五子
            {
                int x = i;
                if (i >= 8)
                    c = 0;
                else
                    c++;
                for (int j = 0; j < 9 - c; j++)
                {
                    if (chess[j, x] == value)
                    {
                        k++;
                        if (k == 5)
                            return true;
                    }
                    else
                        k = 0;
                    x--;
                }
                k = 0;
            }
            for (int i = 1; i < 8; i++)//斜着五子
            {
                int x = i;
                if (i <= 3)
                    c = 0;
                else
                    c++;
                for (int j = 0; j < 9 - c; j++)
                {
                    if (chess[j, x] == value)
                    {
                        k++;
                        if (k == 5)
                            return true;
                    }
                    else
                        k = 0;
                    x++;
                }
                k = 0;
            }
            return false;
        }

        public void clear_chess()
        {
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 12; j++)
                {
                    chess[i, j] = 0;
                }
        }
    }
}
