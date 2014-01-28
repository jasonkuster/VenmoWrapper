using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VenmoWrapper
{
    /// <summary>
    /// A class to handle the target{} field in every transaction object.
    /// </summary>
    public class VenmoTarget
    {
        public string type { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public VenmoUser user { get; set; }
    }
}
