using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace VenmoWrapper
{
    /// <summary>
    /// A class containing all the data Venmo provides about a user,
    /// including the current user. Check if balance, phone number, or email
    /// are null to determine if this is the current user or not.
    /// </summary>
    [DataContract]
    public class VenmoUser : IComparable
    {
        [DataMember]
        public int id { get; set; }
        [DataMember]
        public string first_name { get; set; }
        [IgnoreDataMember]
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
        [DataMember]
        public string last_name { get; set; }
        [IgnoreDataMember]
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
        [DataMember]
        public string display_name { get; set; }
        [IgnoreDataMember]
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
        [DataMember]
        public string username { get; set; }
        [DataMember]
        public string date_joined { get; set; }
        [DataMember]
        public string profile_picture_url { get; set; }
        [IgnoreDataMember]
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
        [DataMember]
        public string about { get; set; }
        [DataMember]
        public double balance { get; set; }
        [IgnoreDataMember]
        public string formattedBalance
        {
            get
            {
                return balance.ToString("N2");
            }
        }
        [DataMember]
        public string phone { get; set; }
        [DataMember]
        public string email { get; set; }

        /// <summary>
        /// Standard CompareTo method. Compares on first name, then last name.
        /// </summary>
        /// <param name="obj">The VenmoUser to which to compare this user.</param>
        /// <returns>1 if the provided user comes after this user, 0 if they are the same,
        /// and -1 if this user comes after the provided user.</returns>
        public int CompareTo(object obj)
        {
            VenmoUser user = obj as VenmoUser;
            if (user == null)
            {
                throw new ArgumentException("Object is not of type VenmoUser");
            }
            int same = this.first_name.CompareTo(user.first_name);
            return same == 0 ? this.last_name.CompareTo(user.last_name) : same;
        }
    }
}
