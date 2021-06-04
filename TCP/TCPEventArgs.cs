using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GoBang.TCP
{
    class TCPEventArgs:EventArgs
    {
        private TcpClient handle;
        private string v;

        public TCPEventArgs(string descrip, TcpClient handle)
        {
            this.handle = handle;
           
            IsHandled = false;
        }

        public TCPEventArgs(string v)
        {
            C = v;
            IsHandled = false;
        }

        public TCPEventArgs(TcpClient handle)
        {
            this.handle = handle;
            IsHandled = false;
        }
        public String C { get; set; }
        public bool IsHandled { get; set; }

    }
}
