using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VenmoWrapper
{
    public class VenmoTarget
    {
        public string type { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public VenmoUser user { get; set; }
    }
}
