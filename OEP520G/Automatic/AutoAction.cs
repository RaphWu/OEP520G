using System;
using System.Collections.Generic;
using System.Text;

namespace OEP520G.Automatic
{
    public class AutoAction
    {
        public EAction Id { get; set; }
        public string Key { get; set; }
        public string Title { get; set; }
        public List<AutoTarget> Targets { get; set; }

        public AutoAction()
        {
            Targets = new List<AutoTarget>();
        }
    }
}
