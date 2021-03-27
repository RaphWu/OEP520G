using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OEP520G.Dispensing.Constants
{
    public class UVPosition
    {
        public const int None = 0;
        public const int Stage = 1;
        public const int Tray = 2;
        
        public static implicit operator int(UVPosition pos) => pos;
        public static implicit operator UVPosition(int pos) => pos;

        public static string GetName(int pos) => Names[pos];

        private static readonly string[] Names = new string[] { "None", "Stage", "Tray" };
    }
}
