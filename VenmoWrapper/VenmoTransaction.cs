using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VenmoWrapper
{
    /// <summary>
    /// An object containing all the data provided by Venmo for a transaction.
    /// N.B: fee and refund are of type Nullable&lt;double&gt;, not just double.
    /// </summary>
    public class VenmoTransaction
    {
        public string id { get; set; }
        public string status { get; set; }
        public string note { get; set; }
        public double amount { get; set; }
        public string formattedAmount
        {
            get
            {
                return amount.ToString("N2");
            }
        }
        public string action { get; set; }
        public string date_created { get; set; }
        public string date_completed { get; set; }
        public string audience { get; set; }
        public VenmoTarget target { get; set; }
        public VenmoUser actor { get; set; }
        public double? fee { get; set; }
        public double? refund { get; set; }
        public string medium { get; set; }

        /// <summary>
        /// Accessor which returns the payment type of this particular transaction.
        /// </summary>
        public string paymentType
        {
            get
            {
                bool userInitiated = actor.id.Equals(VenmoHelper.currentAuth.currentUser.id) ? true : false;
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

        /// <summary>
        /// Accessor which provides a human-readable string describing what took place in this transaction.
        /// </summary>
        public string transactionString
        {
            get
            {
                string actorname = actor.display_name;
                string targetname;
                switch (target.type)
                {
                    case "phone":
                        targetname = target.phone;
                        break;
                    case "email":
                        targetname = target.email;
                        break;
                    case "user":
                        targetname = target.user.display_name;
                        break;
                    default:
                        targetname = "someone";
                        break;
                }

                switch (paymentType)
                {
                    case "userpay":
                        return "You paid " + targetname + ".";
                    case "usercharge":
                        return "You charged " + targetname + ".";
                    case "otherpay":
                        return actorname + " paid you.";
                    case "othercharge":
                        return actorname + " charged you.";
                    default:
                        return "Something Happened.";
                }
            }
        }

        public string amountString
        {
            get
            {
                switch (paymentType)
                {
                    case "userpay":
                    case "othercharge":
                        return "-$" + formattedAmount;
                    case "usercharge":
                    case "otherpay":
                        return "+$" + formattedAmount;
                    default:
                        return formattedAmount;
                }
            }
        }
    }
}
