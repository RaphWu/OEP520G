using System;
using System.Collections.Generic;
using System.Text;

namespace OEP520G.Automatic
{
    public class AutoHead
    {
        public EHead Id { get; set; }
        public string Key { get; set; }
        public string Title { get; set; }
        public List<AutoAction> Actions { get; set; }

        public AutoHead()
        {
            Actions = new List<AutoAction>();
        }

        public string GetKey() => Key;

        public string GetTitle() => Title;
    }
}
