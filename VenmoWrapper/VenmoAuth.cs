using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VenmoWrapper
{
    public class VenmoAuth
    {
        public VenmoAuth(string refreshToken, string userAccessToken, DateTime expireTime, VenmoUser currentUser)
        {
            this.refreshToken = refreshToken;
            this.userAccessToken = userAccessToken;
            this.expireTime = expireTime;
            this.currentUser = currentUser;
        }

        public void RefreshLogin(string refreshToken, string userAccessToken, DateTime expireTime)
        {
            this.refreshToken = refreshToken;
            this.userAccessToken = userAccessToken;
            this.expireTime = expireTime;
        }

        public string refreshToken { get; private set; }
        public string userAccessToken { get; private set; }
        public DateTime expireTime { get; private set; }
        public VenmoUser currentUser { get; set; }
    }
}
