using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OEP520G.Dispensing.Models
{
    public class DispensingParameters
    {
        internal static DispenserDefine Dispenser { get; set; }
        internal static List<DispensingShapeDefine> Shape { get; set; }
        internal static List<DispensingSequenceDefine> Sequence { get; set; }
        internal static List<DispensingGroupDefine> Group { get; set; }
    }
}
