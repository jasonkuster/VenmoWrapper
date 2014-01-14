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
        public double balance { get; set; }
        public VenmoTransaction payment { get; set; }
    }
}
