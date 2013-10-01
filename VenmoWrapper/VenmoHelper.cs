using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VenmoWrapper
{
    /// <summary>
    /// This is the main Venmo helper class. It is instantiated by providing the client's ID
    /// and secret, with the option to provide a userAccessToken in case the user has already
    /// authenticated this particular client.
    /// </summary>
    public class VenmoHelper
    {

        #region Class Vars

        public enum USER_TYPE { USER_ID, PHONE, EMAIL };
        public int clientID { get; private set; }
        public string clientSecret { get; private set; }
        public string userAccessToken { get; private set; }
        public string userAccessTokenQueryString
        {
            get
            {
                return "?access_token=" + userAccessToken;
            }
        }
        public bool loggedIn { get; private set; }

        #endregion

        #region Constants

        string venmoAuthUrl = "https://api.venmo.com/oauth/access_token";
        string venmoPaymentUrl = "https://api.venmo.com/payments";
        string venmoUserUrl = "https://api.venmo.com/users/{0}";
        string venmoFriendsUrl = "https://api.venmo.com/users/{0}/friends";
        string venmoMeUrl = "https://api.venmo.com/me";

        #endregion

        #region Constructors

        public VenmoHelper(int clientID, string clientSecret)
        {
            this.clientID = clientID;
            this.clientSecret = clientSecret;
            loggedIn = false;
        }

        public VenmoHelper(int clientID, string clientSecret, string userAccessToken)
        {
            this.clientID = clientID;
            this.clientSecret = clientSecret;
            this.userAccessToken = userAccessToken;
            loggedIn = true;
        }

        #endregion

        #region Tasks

        /// <summary>
        /// Asynchrously logs the user into Venmo given an access code.
        /// </summary>
        /// <param name="accessCode">The access code retrieved from Venmo's OAuth login page.</param>
        /// <returns>Returns a dictionary with 3 keys:
        /// "accessToken" is the perpetual use access token for the user.
        /// "user" is the json data containing the info about the user.
        /// "responseCode" is the TCP response code retrieved from the transaction.
        /// Returns null if already logged in.
        /// </returns>
        public async Task<VenmoUser> LogUserIn(string accessCode)
        {
            if (loggedIn)
            {
                return null;
            }

            string venmoResponse = await LogIn(accessCode);
            VenmoUser currentUser = JsonConvert.DeserializeObject<VenmoUser>(venmoResponse);
            return currentUser;
        }

        /// <summary>
        /// Asynchronously negotiates a transaction with Venmo.
        /// </summary>
        /// <param name="usertype">The type of user ID you will be </param>
        /// <param name="recipient">The user's ID, phone number, or email as indicated by usertype.</param>
        /// <param name="note">The note to accompany the transaction.</param>
        /// <param name="sendAmount">The <code>double</code> amount of the transaction.</param>
        /// <exception cref="VenmoWrapper.NotLoggedInException">Throws a NotLoggedInException if the user is not logged in.</exception>
        /// <returns></returns>
        public async Task<PaymentResult> PostVenmoTransaction(USER_TYPE usertype, string recipient, string note, double sendAmount)
        {
            if (!loggedIn)
            {
                throw new NotLoggedInException();
            }

            string venmoResponse = await PostTransaction(recipient, note, sendAmount);
            PaymentResult pr = JsonConvert.DeserializeObject<PaymentResult>(venmoResponse);
            return pr;
        }

        /// <summary>
        /// Asynchrously returns the current user.
        /// </summary>
        /// <exception cref="VenmoWrapper.NotLoggedInException">Throws a NotLoggedInException if the user is not logged in.</exception>
        /// <returns>VenmoUser object corresponding to the current user.</returns>
        public async Task<VenmoUser> GetMe()
        {
            if (!loggedIn)
            {
                throw new NotLoggedInException();
            }

            string result = await VenmoGet(venmoMeUrl, userAccessTokenQueryString);
            VenmoUser currentUser = JsonConvert.DeserializeObject<Dictionary<string, VenmoUser>>(result)["data"];
            return currentUser;
        }

        /// <summary>
        /// Asynchronously gets a user from Venmo.
        /// </summary>
        /// <param name="userID">The desired user's ID</param>
        /// <exception cref="VenmoWrapper.NotLoggedInException">Throws a NotLoggedInException if the user is not logged in.</exception>
        /// <returns>VenmoUser object with the desired user's info.</returns>
        public async Task<VenmoUser> GetUser(int userID)
        {
            if (!loggedIn)
            {
                throw new NotLoggedInException();
            }
            string userUrl = String.Format(venmoUserUrl, userID);
            string userJson = await VenmoGet(userUrl, userAccessTokenQueryString);
            Dictionary<string, object> userData = JsonConvert.DeserializeObject<Dictionary<string, object>>(userJson);
            return JsonConvert.DeserializeObject<VenmoUser>(userData["data"].ToString());
        }

        /// <summary>
        /// Asynchronously gets the user's friend list.
        /// </summary>
        /// <param name="userID">The user ID of the user whose friends list is being queried.</param>
        /// <exception cref="VenmoWrapper.NotLoggedInException">Throws a NotLoggedInException if the user is not logged in.</exception>
        /// <returns>Returns an ObservableCollection of VenmoUsers.</returns>
        public async Task<List<VenmoUser>> GetFriendsList(int userID)
        {
            if (!loggedIn)
            {
                throw new NotLoggedInException();
            }
            string friendsUrl = String.Format(venmoFriendsUrl, userID);
            string result = await VenmoGet(friendsUrl, userAccessTokenQueryString);
            Dictionary<string, object> friendsData = JsonConvert.DeserializeObject<Dictionary<string, object>>(result);
            return JsonConvert.DeserializeObject<List<VenmoUser>>(friendsData["data"].ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="VenmoWrapper.NotLoggedInException">Throws a NotLoggedInException if the user is not logged in.</exception>
        /// <returns></returns>
        public async Task<List<VenmoTransaction>> GetRecentTransactions()
        {
            if (!loggedIn)
            {
                throw new NotLoggedInException();
            }
            string venmoResponse = await VenmoGet(venmoPaymentUrl, userAccessTokenQueryString);

            Dictionary<string, object> transactionData = JsonConvert.DeserializeObject<Dictionary<string, object>>(venmoResponse);
            List<VenmoTransaction> recentTransactions = JsonConvert.DeserializeObject<List<VenmoTransaction>>(transactionData["data"].ToString());
            foreach (VenmoTransaction trans in recentTransactions)
            {
                if (trans.target_user_type == "user_id")
                {
                    trans.target_user = await GetUser(int.Parse(trans.target_user_id));
                }
            }

            return recentTransactions;
        }

        #endregion

        #region Helper Functions

        private async Task<string> LogIn(string accessCode)
        {
            string postData = "client_id=" + clientID + "&client_secret=" + clientSecret + "&code=" + accessCode;
            string venmoResponse = await VenmoPost(venmoAuthUrl, postData);

            Dictionary<string, object> results = JsonConvert.DeserializeObject<Dictionary<string, object>>(venmoResponse);
            userAccessToken = (string)results["access_token"];
            loggedIn = true;

            return results["user"].ToString(); ;
        }

        private async Task<string> PostTransaction(string recipient, string note, double sendAmount)
        {
            string postData = "access_token=" + userAccessToken + "&" + recipient + "&note=" + note + "&amount=" + sendAmount;
            return await VenmoPost(venmoPaymentUrl, postData);
        }

        private async Task<string> VenmoGet(string url, string queryString)
        {
            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(url + queryString);
            webRequest.Method = "GET";
            var webResponse = (HttpWebResponse)(await Task<WebResponse>.Factory.FromAsync(webRequest.BeginGetResponse, webRequest.EndGetResponse, null));
            //TODO: Error Checking/Handling Here

            string responseCode = webResponse.StatusCode.ToString();
            string response = GetContentFromWebResponse(webResponse);

            errorCheck(responseCode, response);

            return response;
        }

        private async Task<string> VenmoPost(string url, string postData)
        {
            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            webRequest.Method = "POST";
            webRequest.ContentType = "application/x-www-form-urlencoded";

            var reqStream = await Task<Stream>.Factory.FromAsync(webRequest.BeginGetRequestStream, webRequest.EndGetRequestStream, null);
            //TODO: Error Checking/Handling Here

            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            reqStream.Write(byteArray, 0, byteArray.Length);

            var webResponse = (HttpWebResponse)(await Task<WebResponse>.Factory.FromAsync(webRequest.BeginGetResponse, webRequest.EndGetResponse, null));
            //TODO: Error Checking/Handling Here

            string responseCode = webResponse.StatusCode.ToString();
            string response = GetContentFromWebResponse(webResponse);

            errorCheck(responseCode, response);

            return response;
        }

        #endregion

        #region Utilities

        public void errorCheck(string responseCode, string response)
        {
            if (responseCode != "OK")
            {
                var definition = new { message = "", code = 0 };
                Dictionary<string, object> message = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);
                var error = JsonConvert.DeserializeAnonymousType(message["error"].ToString(), definition);
                throw new VenmoException(error.message);
            }
        }

        public string GetContentFromWebResponse(HttpWebResponse response)
        {
            using (StreamReader httpWebStreamReader = new StreamReader(response.GetResponseStream()))
            {
                string result = httpWebStreamReader.ReadToEnd();
                return result;
            }
        }

        public void logOut()
        {
            userAccessToken = "";
            loggedIn = false;
        }

        #endregion

    }

    public class NotLoggedInException : System.Exception
    {
        public NotLoggedInException() : base() { }
        public NotLoggedInException(string message) : base(message) { }
        public NotLoggedInException(string message, System.Exception inner) : base(message, inner) { }
    }

    public class VenmoException : System.Exception
    {
        public VenmoException() : base() { }
        public VenmoException(string message) : base(message) { }
        public VenmoException(string message, System.Exception inner) : base(message, inner) { }
    }
}
