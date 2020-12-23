using System.Collections.Generic;
using TcpipServer.Models;

namespace TcpipServer.Contracts
{
    public interface ITcpipServer
    {
        string IP { get; }
        int Port { get; }

        bool ConnectServer();
        void CloseServer();
        //bool SendMessage(string message);
        bool SendImage(List<ImageInfo> dataToSend);
    }
}
