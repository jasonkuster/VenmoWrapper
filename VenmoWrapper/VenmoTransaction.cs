using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VenmoWrapper
{
    /// <summary>
    /// An object containing all the data provided by Venmo for a transaction.
    /// </summary>
    public class VenmoTransaction
    {
        public string id { get; set; }
        public string status { get; set; }
        public string note { get; set; }
        public double amount { get; set; }
        public string action { get; set; }
        public string date_created { get; set; }
        public string date_completed { get; set; }
        public string audience { get; set; }
        public VenmoTarget target { get; set; }
        public VenmoUser actor { get; set; }
        public object fee { get; set; }
        public double refund { get; set; }
        public string medium { get; set; }

        public string paymentType
        {
            get
            {
                bool userInitiated = actor.id == VenmoHelper.currentAuth.currentUser.id ? true : false;
                bool payment = "pay".Equals(action);
                return userInitiated ? payment ? "userpay" : "usercharge" : payment ? "otherpay" : "othercharge";

                //The hilarious line above is functionally identical to the below.
                /*
                if (userInitiated)
                {
                    if (payment)
                    {
                        return "userpay";
                    }
                    else
                    {
                        return "usercharge";
                    }
                }
                else
                {
                    if (payment)
                    {
                        return "otherpay";
                    }
                    else
                    {
                        return "othercharge";
                    }
                }
                 * */
            }
        }
    }
}
