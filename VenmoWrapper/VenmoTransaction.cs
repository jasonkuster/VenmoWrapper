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
        public string status { get; set; }
        public string completed { get; set; }
        public string id { get; set; }
        public object fee { get; set; }
        public string created { get; set; }
        public double amount { get; set; }
        public string target_user_id { get; set; }
        public VenmoUser target_user { get; set; }
        public string note { get; set; }
        public string audience { get; set; }
        public string action { get; set; }
        public string target_user_type { get; set; }

        //Get rid of these for final version, these should be handled by the user
        //to allow for more flexibility.
        public string typeImage
        {
            get
            {
                return action == "pay" ? "/Assets/AppBar/minus.png" : "/Toolkit.Content/ApplicationBar.Add.png";
            }
        }

        public string viewText
        {
            get
            {
                if (target_user != null)
                {
                    return action == "pay" ? "You paid " + target_user.display_name + " $" + amount : target_user.display_name + " paid you $" + amount;
                }
                else
                {
                    return target_user_id;
                }
            }
        }
    }
}
