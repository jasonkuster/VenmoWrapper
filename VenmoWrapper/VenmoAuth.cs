using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace VenmoWrapper
{
    /// <summary>
    /// Class containing all the items which are necessary to authenticate a user against Venmo.
    /// </summary>
    [DataContract]
    public class VenmoAuth
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="refreshToken">The refresh token provided by Venmo.</param>
        /// <param name="userAccessToken">The user access token provided by Venmo.</param>
        /// <param name="expireTime">The date/time that the user access token will expire.</param>
        public VenmoAuth(string refreshToken, string userAccessToken, DateTime expireTime, VenmoUser currentUser)
        {
            this.refreshToken = refreshToken;
            this.userAccessToken = userAccessToken;
            this.expireTime = expireTime;
            this.currentUser = currentUser;
        }

        /// <summary>
        /// Changes out the data upon an access token expiring.
        /// </summary>
        /// <param name="refreshToken">The refresh token provided by Venmo.</param>
        /// <param name="userAccessToken">The user access token provided by Venmo.</param>
        /// <param name="expireTime">The date/time that the user access token will expire.</param>
        public void RefreshLogin(string refreshToken, string userAccessToken, DateTime expireTime)
        {
            this.refreshToken = refreshToken;
            this.userAccessToken = userAccessToken;
            this.expireTime = expireTime;
        }

        [DataMember]
        public string refreshToken { get; set; }
        [DataMember]
        public string userAccessToken { get; set; }
        [DataMember]
        public VenmoUser currentUser { get; set; }
        [DataMember]
        public long utcDate { get; set; }
        [IgnoreDataMember]
        public DateTime expireTime {
            get
            {
                return DateTime.FromFileTimeUtc(utcDate).ToLocalTime();
            }
            set
            {
                utcDate = value.ToFileTimeUtc();
            }
        }
    }
}
