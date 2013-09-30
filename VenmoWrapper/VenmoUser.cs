using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VenmoWrapper
{
    /// <summary>
    /// A class containing all the data Venmo provides about a user,
    /// including the current user. Check if balance, phone number, or email
    /// are null to determine if this is the current user or not.
    /// </summary>
    public class VenmoUser
    {
        public int id { get; set; }
        public string first_name { get; set; }
        public string firstname
        {
            get
            {
                return first_name;
            }
            set
            {
                first_name = value;
            }
        }
        public string last_name { get; set; }
        public string lastname
        {
            get
            {
                return last_name;
            }
            set
            {
                last_name = value;
            }
        }
        public string display_name { get; set; }
        public string name
        {
            get
            {
                return display_name;
            }
            set
            {
                display_name = value;
            }
        }
        public string username { get; set; }
        public string date_joined { get; set; }
        public string profile_picture_url { get; set; }
        public string picture
        {
            get
            {
                return profile_picture_url;
            }
            set
            {
                profile_picture_url = value;
            }
        }
        public string about { get; set; }
        public double balance { get; set; }
        public string formattedBalance
        {
            get
            {
                if (balance != null)
                {
                    return balance.ToString("N2");
                }
                return "";
            }
        }
        public string phone { get; set; }
        public string email { get; set; }
    }
}
