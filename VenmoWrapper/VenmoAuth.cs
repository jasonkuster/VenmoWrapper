using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VenmoWrapper
{
    public class VenmoAuth
    {
        public string refreshToken;
        public string accessToken;
        public DateTime expireTime;
        public VenmoUser User;
    }
}
