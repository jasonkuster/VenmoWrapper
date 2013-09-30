using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VenmoWrapper
{
    /// <summary>
    /// This class contains all of the data Venmo provides about a just-completed transaction.
    /// </summary>
    public class PaymentResult
    {
        public string status { get; set; }
        public int target_user_id { get; set; }
        public string action { get; set; }
        public string message { get; set; }
        public string balance { get; set; }
        public string id { get; set; }
    }
}
