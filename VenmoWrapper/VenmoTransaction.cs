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

        //TODO: Return "userpay", "usercharge", "otherpay", "othercharge" instead.
        public string typeImage
        {
            get
            {
                if (!target_user_id.Equals(VenmoHelper.currentUser.id.ToString()))
                {
                    return action == "pay" ? "/Assets/AppBar/minus.png" : "/Toolkit.Content/ApplicationBar.Add.png";
                }
                else
                {
                    return action == "pay" ? "/Toolkit.Content/ApplicationBar.Add.png" : "/Assets/AppBar/minus.png";
                }
            }
        }

        public string viewText
        {
            get
            {
                if (target_user != null)
                {
                    if (!target_user_id.Equals(VenmoHelper.currentUser.id.ToString()))
                    {
                        return action == "pay" ? "You paid " + target_user.display_name + " $" + amount + "." : "You charged " + target_user.display_name + " $" + amount + ".";
                    }
                    else
                    {
                        return action == "pay" ? "Someone paid you $" + amount + "." : "Someone charged you $" + amount + ".";
                    }
                }
                else
                {
                    return action == "pay" ? "You paid " + target_user_id + " $" + amount + "." : "You charged " + target_user_id + " $" + amount + ".";
                }
            }
        }
    }
}
