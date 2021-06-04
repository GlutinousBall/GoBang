using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GoBang.TCP
{
    class Client_Attributes
    {
        private String name;//唯一，用于匹配
        private int status;//表示状态
        private bool in_game;
        TcpClient tcpclient;

        public Client_Attributes(String n,int s,bool g,TcpClient tcp)
        {
            name = n;
            status = s;
            in_game = g;
            tcpclient = tcp;
        }
        public bool get_In_game() { return in_game; }
        public String get_Nmae() { return name; }
        public int get_Status() { return status; }
        public void set_In_game(bool g ) { in_game = g; }
        public void set_Nmae(String N) { name = N; }
        public void set_Status(int S) {  status=S; }
        public TcpClient get_client() { return tcpclient; }
    }
}
