using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace VenmoWrapper
{
    [DataContract]
    public class VenmoAuth
    {
        public VenmoAuth(string refreshToken, string userAccessToken, DateTime expireTime)
        {
            this.refreshToken = refreshToken;
            this.userAccessToken = userAccessToken;
            this.expireTime = expireTime;
        }

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
