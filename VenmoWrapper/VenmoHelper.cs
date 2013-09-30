﻿using Newtonsoft.Json;
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
        /// <returns>Returns a dictionary with 3 keys:\n
        /// "accessToken" is the perpetual use access token for the user.\n
        /// "user" is the json data containing the info about the user.\n
        /// "responseCode" is the TCP response code retrieved from the transaction.
        /// </returns>
        public Task<Dictionary<string, string>> LogUserIn(string accessCode)
        {
            if (!loggedIn)
            {
                return Task<Dictionary<string, string>>.Factory.StartNew(() => LogIn(accessCode));
            }
            return null;
        }

        /// <summary>
        /// Asynchronously negotiates a transaction with Venmo.
        /// </summary>
        /// <param name="usertype">The type of user ID you will be </param>
        /// <param name="recipient">The user's ID, phone number, or email as indicated by usertype.</param>
        /// <param name="note">The note to accompany the transaction.</param>
        /// <param name="sendAmount">The <code>double</code> amount of the transaction.</param>
        /// <returns></returns>
        public Task<Dictionary<string, string>> PostVenmoTransaction(USER_TYPE usertype, string recipient, string note, double sendAmount)
        {
            if (!loggedIn)
            {
                return null;
            }
            return Task<Dictionary<string, string>>.Factory.StartNew(() => PostTransaction(recipient, note, sendAmount));
        }

        /// <summary>
        /// Asynchrously returns the current user.
        /// </summary>
        /// <returns>VenmoUser object corresponding to the current user.</returns>
        public async Task<VenmoUser> GetMe()
        {
            if (!loggedIn)
            {
                return null;
            }
            string result = (await GetSomething(venmoMeUrl, userAccessTokenQueryString))["response"];
            return JsonConvert.DeserializeObject<Dictionary<string, VenmoUser>>(result)["data"];
        }

        /// <summary>
        /// Asynchronously gets a user from Venmo.
        /// </summary>
        /// <param name="userID">The desired user's ID</param>
        /// <returns>VenmoUser object with the desired user's info.</returns>
        public async Task<VenmoUser> GetUser(int userID)
        {
            if (!loggedIn)
            {
                return null;
            }
            string userUrl = String.Format(venmoUserUrl, userID);
            string userJson = (await GetSomething(userUrl, userAccessTokenQueryString))["response"];
            Dictionary<string, object> userData = JsonConvert.DeserializeObject<Dictionary<string, object>>(userJson);
            return JsonConvert.DeserializeObject<VenmoUser>(userData["data"].ToString());
        }

        /// <summary>
        /// Asynchronously gets the user's friend list.
        /// </summary>
        /// <param name="userID">The user ID of the user whose friends list is being queried.</param>
        /// <returns>Returns an ObservableCollection of VenmoUsers.</returns>
        public async Task<ObservableCollection<VenmoUser>> GetFriendsList(int userID)
        {
            if (!loggedIn)
            {
                return null;
            }
            string friendsUrl = String.Format(venmoFriendsUrl, userID);
            string result = (await GetSomething(friendsUrl, userAccessTokenQueryString))["response"];
            Dictionary<string, object> friendsData = JsonConvert.DeserializeObject<Dictionary<string, object>>(result);
            return JsonConvert.DeserializeObject<ObservableCollection<VenmoUser>>(friendsData["data"].ToString());
        }

        /// <summary>
        /// Returns a tuple 
        /// </summary>
        /// <returns></returns>
        public async Task<Tuple<bool, string>> GetRecentTransactions()
        {
            if (!loggedIn)
            {
                return null;
            }
            Dictionary<string, string> resultDict = await GetSomething(venmoPaymentUrl, userAccessTokenQueryString);
            string result = resultDict["response"];
            bool error = errorPresent(resultDict) ? true : false;

            return new Tuple<bool, string>(error, result);
        }

        #endregion

        #region Helper Functions

        private Dictionary<string, string> LogIn(string accessCode)
        {
            string postData = "client_id=" + clientID + "&client_secret=" + clientSecret + "&code=" + accessCode;
            operationComplete = false;
            BeginPOSTRequest(venmoAuthUrl, postData);

            while (!operationComplete)
            {
                Thread.Sleep(250);
            }

            Dictionary<string, object> results = JsonConvert.DeserializeObject<Dictionary<string, object>>(venmoResponse);
            userAccessToken = (string)results["access_token"];
            string userInfo = results["user"].ToString();

            Dictionary<string, string> loginInfo = new Dictionary<string, string>();
            loginInfo["accessToken"] = userAccessToken;
            loginInfo["user"] = userInfo;
            loginInfo["responseCode"] = resultCode.ToString();
            loggedIn = true;

            cleanupData();
            return loginInfo;
        }

        private Dictionary<string, string> PostTransaction(string recipient, string note, double sendAmount)
        {
            string postData = "access_token=" + userAccessToken + "&" + recipient + "&note=" + note + "&amount=" + sendAmount;
            operationComplete = false;
            BeginPOSTRequest(venmoPaymentUrl, postData);

            while (!operationComplete)
            {
                Thread.Sleep(250);
            }

            Dictionary<string, string> paymentInfo = new Dictionary<string, string>();
            paymentInfo["transactionResult"] = venmoResponse;
            paymentInfo["responseCode"] = resultCode.ToString();

            cleanupData();
            return paymentInfo;
        }

        private async Task<Dictionary<string, string>> GetSomething(string url, string queryString)
        {
            WebClient c = new WebClient();
            string response = "";
            string responseCode = "OK";
            try
            {
                response = await c.DownloadStringTaskAsync(url + queryString);
            }
            catch (WebException e)
            {
                using (WebResponse webResponse = e.Response)
                {
                    HttpWebResponse hwr = (HttpWebResponse)webResponse;
                    response = GetContentFromWebResponse(hwr);
                    responseCode = hwr.StatusCode.ToString();
                }
            }

            Dictionary<string, string> responseDict = new Dictionary<string, string>
            {
                { "responseCode", responseCode },
                { "response", response }
            };

            return responseDict;
        }

        #endregion

        #region POSTCode

        bool operationComplete = false;
        string postData = "";
        HttpStatusCode resultCode = HttpStatusCode.Unused;
        string venmoResponse = "";

        private void BeginPOSTRequest(string url, string postData)
        {
            this.postData = postData;
            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            webRequest.Method = "POST";
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.BeginGetRequestStream(new AsyncCallback(PostDataStep), webRequest);
        }

        private void PostDataStep(IAsyncResult ar)
        {
            HttpWebRequest webRequest = (HttpWebRequest)ar.AsyncState;

            // End the stream request operation
            Stream postStream = webRequest.EndGetRequestStream(ar);

            // Create the post data
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            // Add the post data to the web request
            postStream.Write(byteArray, 0, byteArray.Length);
            postStream.Close();

            // Start the web request
            webRequest.BeginGetResponse(new AsyncCallback(ReturnFromVenmo), webRequest);
        }

        private void ReturnFromVenmo(IAsyncResult ar)
        {
            HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
            HttpWebResponse response = null;

            try
            {
                response = (HttpWebResponse)request.EndGetResponse(ar);
            }
            catch (WebException e)
            {
                using (WebResponse webResponse = e.Response)
                {
                    response = (HttpWebResponse)webResponse;
                }
            }

            resultCode = response.StatusCode;
            venmoResponse = GetContentFromWebResponse(response);
            operationComplete = true;
        }

        #endregion

        #region Utilities

        public bool errorPresent(Dictionary<string, string> result)
        {
            return result["responseCode"] != "OK" ? true : false;
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
            cleanupData();
        }

        private void cleanupData()
        {
            operationComplete = false;
            postData = "";
            resultCode = HttpStatusCode.Unused;
            venmoResponse = "";
        }

        #endregion

    }
}
