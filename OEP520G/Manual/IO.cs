using EPCIO;
using EPCIO.IoSystem;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace OEP520G.Manual
{
    /// <summary>
    /// Local Io擴充(DataGrid顯示用)
    /// </summary>
    public class LocalIo_No : LocalIo
    {
        internal int No { get; set; } // 序號

        /// <summary>
        /// 建構函式
        /// </summary>
        public LocalIo_No(int no, string name, string ioCode, bool inversion)
            : base(name, ioCode, inversion)
        {
            No = no;
        }
    }

    /// <summary>
    /// Remote Io擴充(DataGrid顯示用)
    /// </summary>
    public class RemoteIo_No : RemoteIo
    {
        internal int No { get; set; } // 序號

        /// <summary>
        /// 建構函式
        /// </summary>
        public RemoteIo_No(int no, string name, char type, bool inversion, ushort set, ushort port, ushort bit)
            : base(name, type, inversion, set, port, bit)
        {
            No = no;
        }
    }

    public class IO
    {
        private readonly Epcio epcio = Epcio.Instance;

        internal List<LocalIo_No> LocalIoList { get; set; }
        internal List<RemoteIo_No> RemoteIoInputList { get; set; }
        internal List<RemoteIo_No> RemoteIoOutputList { get; set; }

        /// <summary>
        /// 建構函式
        /// </summary>
        public IO()
        {
            LocalIoList = new List<LocalIo_No>();
            RemoteIoInputList = new List<RemoteIo_No>();
            RemoteIoOutputList = new List<RemoteIo_No>();

            int LocalIoNo = 1;
            int RemoteIoInputNo = 1;
            int RemoteIoOutputNo = 1;

            /********************
             * Local IO
             *******************/
            AddLocalList(epcio.ServoX.Home);
            AddLocalList(epcio.ServoY.Home);
            AddLocalList(epcio.ServoZ.Home);
            AddLocalList(epcio.ServoR.Home);
            AddLocalList(epcio.ServoTray.Home);
            AddLocalList(epcio.ServoClamp.Home);
            AddLocalList(epcio.ServoX.LsPositive);
            AddLocalList(epcio.ServoY.LsPositive);
            AddLocalList(epcio.ServoY.LsNegative);
            AddLocalList(epcio.ServoZ.LsPositive);
            AddLocalList(epcio.ServoZ.LsNegative);

            void AddLocalList(LocalIo li)
            {
                LocalIo_No lin = new LocalIo_No(
                    name: li.Name,
                    ioCode: li.IoCode,
                    inversion: li.Inversion,
                    no: LocalIoNo++
                )
                { Value = li.Value };

                LocalIoList.Add(lin);
            }

            /********************
             * Remote IO
             *******************/
            foreach (var io in epcio.RioInputList)
                AddToRemoteIoInputList(io);

            foreach (var io in epcio.RioOutputList)
                AddToRemoteIoOutputList(io);

            void AddToRemoteIoInputList(RemoteIo ri)
            {
                RemoteIo_No rin = new RemoteIo_No(
                    name: ri.Name,
                    type: 'X',
                    inversion: ri.Inversion,
                    set: ri.Set,
                    port: ri.Port,
                    bit: ri.Bit,
                    no: RemoteIoInputNo++
                )
                { Value = ri.Value };

                RemoteIoInputList.Add(rin);
            }

            void AddToRemoteIoOutputList(RemoteIo ri)
            {
                RemoteIo_No rin = new RemoteIo_No(
                    name: ri.Name,
                    type: 'Y',
                    inversion: ri.Inversion,
                    set: ri.Set,
                    port: ri.Port,
                    bit: ri.Bit,
                    no: RemoteIoOutputNo++
                )
                { Value = ri.Value };

                RemoteIoOutputList.Add(rin);
            }
        }

        /********************
         * DataGrid
         *******************/
        internal void LioOutput(Servo servo, EIoType it)
        {
            LocalIo ls = it switch
            {
                EIoType.Home => servo.Home,
                EIoType.LimitSwitch => servo.LsPositive,
                EIoType.LS_Positive => servo.LsPositive,
                EIoType.LS_Negative => servo.LsNegative,
                _ => null,
            };

            if (ls != null)
            {
                int index = LocalIoList.FindIndex(x => x.IoCode == ls.IoCode);
                if (index >= 0)
                    LocalIoList[index].Value = ls.Value;
            }
        }

        internal void RioInput(RemoteIo ri)
            => RemoteIoInputList.Find(x => x.Name == ri.Name).Value = ri.Value;

        internal void RioOutputChanged(RemoteIo ri)
            => RemoteIoOutputList.Find(x => x.Name == ri.Name).Value = ri.Value;

        internal void UpdateList()
        {

        }
    }
}