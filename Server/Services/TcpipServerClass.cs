using Prism.Events;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using TcpipServer.Contracts;
using TcpipServer.Models;

namespace TcpipServer.Services
{
    public class TcpipServerClass : ITcpipServer
    {
        private int dataLen;
        private Socket masterSocket;
        private Socket auxSocket;
        private Thread acceptConnectionThread;
        private bool connectionFlag;
        private Process process;

        private IEventAggregator _ea;

        public TcpipServerClass(IEventAggregator ea)
        {
            _ea = ea;

            CfgFile.ReadCfgFile();

            dataLen = 16 * 1024;
            connectionFlag = false;
        }

        public string IP => SocketData.ip;
        public int Port => SocketData.port;

        public bool ConnectServer()
        {
            InitServer();
            //process = Process.Start(new ProcessStartInfo
            //{
            //    WorkingDirectory = SocketData.clientPath,
            //    FileName = $@"{SocketData.clientPath}\Client.exe"
            //});

            return process != null;
            //return true;
        }

        public void CloseServer()
        {
            if (process != null)
            {
                process.Close();
                process.Dispose();
            }

            if (connectionFlag)
            {
                connectionFlag = true;

                auxSocket.Close();
                auxSocket.Dispose();
            }

            masterSocket.Close();
            masterSocket.Dispose();
        }

        //public bool SendMessage(string messageSend)
        //{
        //    byte[] dataSend = Encoding.ASCII.GetBytes(messageSend);

        //    try
        //    {
        //        auxSocket.Send(dataSend);
        //        return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        public bool SendImage(List<ImageInfo> dataToSend)
        {
            byte[] byteToSend = ListToByte(dataToSend);

            try
            {
                auxSocket.Send(byteToSend);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /********************
         * 
         ********************/
        internal void InitServer()
        {
            masterSocket = new Socket(AddressFamily.InterNetwork,
                                      SocketType.Stream,
                                      ProtocolType.Tcp);
            masterSocket.Bind(new IPEndPoint(IPAddress.Parse(SocketData.ip),
                                             SocketData.port));
            masterSocket.Listen(10);

            WaitAccept();
        }

        internal void WaitAccept()
        {
            acceptConnectionThread = new Thread(AcceptConnectionThreadProc)
            {
                IsBackground = true
            };
            acceptConnectionThread.Start();
        }

        internal void AcceptConnectionThreadProc()
        {
            try
            {
                auxSocket = masterSocket.Accept();

                if (auxSocket != null)
                    connectionFlag = true;

                WaitAccept();
                //_ea.GetEvent<PublishStatus>().Publish("Client connected!");

                long IntAcceptData;
                byte[] clientData = new byte[dataLen];
                string RvMessage;

                while (true)
                {
                    clientData = Enumerable.Repeat((byte)0, dataLen).ToArray();
                    IntAcceptData = auxSocket.Receive(clientData);

                    if (IntAcceptData <= 0)
                        throw new SocketException();

                    RvMessage = Encoding.Default.GetString(clientData);
                    //if (RvMessage.Substring(0, 2) == "[{")
                    _ea.GetEvent<PublishSolve>().Publish(RvMessage);

                    //_ea.GetEvent<PublishMessage>().Publish(RvMessage);
                    //_ea.GetEvent<PublishStatus>().Publish("Message received!");
                }
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.Interrupted)
                {
                    //_ea.GetEvent<PublishStatus>().Publish("Server stopped");
                }

                if (e.SocketErrorCode == SocketError.Success)
                {
                    //_ea.GetEvent<PublishStatus>().Publish("Client disconnected");
                }

                //_ea.GetEvent<PublishMessage>().Publish("");
            }
        }

        private byte[] ListToByte(List<ImageInfo> dataToSend)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, dataToSend);

            return stream.ToArray();
        }
    }
}
