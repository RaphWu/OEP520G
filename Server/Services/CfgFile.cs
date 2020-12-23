using System;
using System.IO;
using TcpipServer.Constants;
using TcpipServer.Models;

namespace TcpipServer.Services
{
    internal class CfgFile
    {
        internal static bool ReadCfgFile()
        {
            StreamReader sr;
            string line, line0 = "", line1 = "";
            char[] cArray;
            string[] lineSplit;

            sr = new StreamReader(FileName.McCfgFileName);

            while (true)
            {
                line = sr.ReadLine();
                if (line == null)
                {
                    break;
                }
                else
                {
                    cArray = line.ToCharArray();

                    if ((cArray[0] == '/') && (cArray[1] == '/'))
                    {

                    }
                    else
                    {
                        lineSplit = line.Split('=');

                        if (lineSplit.Length == 2)
                        {
                            line0 = lineSplit[0].Trim(' ');
                            line1 = lineSplit[1].Trim(' ');
                        }

                        if (String.Compare(line0, "IP") == 0)
                        {
                            SocketData.ip = line1;
                        }
                        else if (String.Compare(line0, "Port") == 0)
                        {
                            SocketData.port = Int32.Parse(line1);
                        }
                        else if (String.Compare(line0, "Width") == 0)
                        {
                            SocketData.width = Int32.Parse(line1);
                        }
                        else if (String.Compare(line0, "Height") == 0)
                        {
                            SocketData.height = Int32.Parse(line1);
                        }
                        else if (String.Compare(line0, "NumImgs") == 0)
                        {
                            SocketData.numImgs = Int32.Parse(line1);
                        }
                        else if (String.Compare(line0, "ClientPath") == 0)
                        {
                            SocketData.clientPath = line1;
                        }
                    }
                }

                if (sr.EndOfStream == true)
                    break;
            }

            sr.Close();
            return true;
        }
    }
}
