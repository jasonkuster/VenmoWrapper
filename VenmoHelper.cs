﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    /// This is the main Venmo helper class. It is initialized by providing the client's ID
    /// and secret, with the option to provide a VenmoAuth in the case where the user has already
    /// authenticated this particular client.
    /// </summary>
    public static class VenmoHelper
    {

        #region Class Vars

        //The current payment types Venmo supports, plus an option for invalid type.
        public enum USER_TYPE { USER_ID, PHONE, EMAIL, INVALID };

        //The client-specific ID and secret. These can be found on your app's page on Venmo.
        public static int clientID { get; private set; }
        public static string clientSecret { get; private set; }

        //A simple accessor which appends the correct query string to the user access token.
        public static string userAccessTokenQueryString
        {
            get
            {
                if (loggedIn)
                {
                    return "?access_token=" + currentAuth.userAccessToken;
                }
                return null;
            }
        }

        //The VenmoAuth specific to the current user.
        private static VenmoAuth _currentAuth;
        public static VenmoAuth currentAuth
        {
            get
            {
                if (loggedIn)
                {
                    return _currentAuth;
                }
                return null;
            }
            private set
            {
                _currentAuth = value;
            }
        }

        public static bool loggedIn { get; private set; }

        private static string friendQueryString;
        private static long friendID;
        private static string transactionQueryString;

        #endregion

        #region Constants

        //The various URLs for the different Venmo endpoints.
        const string venmoAuthUrl = "https://api.venmo.com/v1/oauth/access_token";
        const string venmoPaymentUrl = "https://api.venmo.com/v1/payments";
        const string venmoIndividualPaymentUrl = "https://api.venmo.com/v1/payments/{0}";
        const string venmoUserUrl = "https://api.venmo.com/v1/users/{0}";
        const string venmoFriendsUrl = "https://api.venmo.com/v1/users/{0}/friends";
        const string venmoMeUrl = "https://api.venmo.com/v1/me";

        const string notLoggedInError = "You are not currently logged in. Please log in now.";

        #endregion

        /// <summary>
        /// This is the setup method for a client which does not have a logged-in user.
        /// </summary>
        /// <param name="clientID">The client-specific ID, can be found on the Venmo app page.</param>
        /// <param name="clientSecret">The client-specific secret, can be found on the Venmo app page.</param>
        public static void SetUp(int clientID, string clientSecret)
        {
            VenmoHelper.clientID = clientID;
            VenmoHelper.clientSecret = clientSecret;
            VenmoHelper.loggedIn = false;
        }

        /// <summary>
        /// This is the setup method for a client which has the VenmoAuth for a logged-in user.
        /// </summary>
        /// <param name="clientID">The client-specific ID, can be found on the Venmo app page.</param>
        /// <param name="clientSecret">The client-specific secret, can be found on the Venmo app page.</param>
        /// <param name="auth">The VenmoAuth object, containing the user access token, refresh token, and refresh time.</param>
        public static void SetUp(int clientID, string clientSecret, VenmoAuth auth)
        {
            VenmoHelper.clientID = clientID;
            VenmoHelper.clientSecret = clientSecret;
            VenmoHelper.currentAuth = auth;
            VenmoHelper.loggedIn = true;
        }

        #region Tasks

        /// <summary>
        /// Logs the user into Venmo given an access code.
        /// </summary>
        /// <param name="accessCode">The access code retrieved from Venmo's OAuth login page.</param>
        /// <returns>Returns a VenmoAuth object corresponding to the current user.
        /// Returns null if already logged in.
        /// </returns>
        public static async Task<VenmoAuth> LogUserIn(string accessCode)
        {
            if (loggedIn)
            {
                return null;
            }

            string postData = "client_id=" + clientID + "&client_secret=" + clientSecret + "&code=" + accessCode;
            string venmoResponse = await VenmoPost(venmoAuthUrl, postData);

            Dictionary<string, object> results = JsonConvert.DeserializeObject<Dictionary<string, object>>(venmoResponse);

            string refresh_token = (string)results["refresh_token"];
            string user_access_token = (string)results["access_token"];
            long expire_time = (long)results["expires_in"];
            DateTime expires_in = DateTime.Now.AddSeconds(expire_time);
            VenmoUser user = JsonConvert.DeserializeObject<VenmoUser>(results["user"].ToString());
            if (results["balance"] != null)
            {
                user.balance = double.Parse(results["balance"].ToString());
            }
            else
            {
                user.balance = -1;
            }

            VenmoHelper.currentAuth = new VenmoAuth(refresh_token, user_access_token, expires_in, user);
            loggedIn = true;

            return VenmoHelper.currentAuth;
        }

        /// <summary>
        /// Negotiates a transaction with Venmo.
        /// </summary>
        /// <param name="usertype">The type of user ID which will be provided.</param>
        /// <param name="recipient">The user's ID, phone number, or email as indicated by usertype.</param>
        /// <param name="note">The note to accompany the transaction.</param>
        /// <param name="sendAmount">The <code>double</code> amount of the transaction.</param>
        /// <param name="audience">The audience ("public", "friends", or "private") to which this transaction should be visible. Defaults to public.</param>
        /// <exception cref="VenmoWrapper.NotLoggedInException">Throws a NotLoggedInException if the user is not logged in.</exception>
        /// <returns>VenmoTransaction object corresponding to the just-negotiated transaction.</returns>
        public static async Task<VenmoTransaction> PostVenmoTransaction(USER_TYPE usertype, string recipient, string note, double sendAmount, string audience = "public")
        {
            CheckLoginStatus();

            string type = usertype == USER_TYPE.EMAIL ? "email=" : usertype == USER_TYPE.PHONE ? "phone=" : "user_id=";

            string postData = "access_token=" + currentAuth.userAccessToken + "&" + type + recipient + "&note=" + note + "&amount=" + sendAmount + "&audience=" + audience;
            string venmoResponse = await VenmoPost(venmoPaymentUrl, postData);

            string paymentData = JsonConvert.DeserializeObject<Dictionary<string, object>>(venmoResponse)["data"].ToString();
            Dictionary<string, object> transData = JsonConvert.DeserializeObject <Dictionary<string, object>>(paymentData);

            if (transData["balance"] != null)
            {
                VenmoHelper.currentAuth.currentUser.balance = double.Parse(transData["balance"].ToString());
            }
            else
            {
                VenmoHelper.currentAuth.currentUser.balance = -1;
            }

            VenmoTransaction transaction = JsonConvert.DeserializeObject<VenmoTransaction>(transData["payment"].ToString());

            return transaction;
        }

        /// <summary>
        /// Returns the current user.
        /// </summary>
        /// <exception cref="VenmoWrapper.NotLoggedInException">Throws a NotLoggedInException if the user is not logged in.</exception>
        /// <returns>VenmoUser object corresponding to the current user.</returns>
        public static async Task<VenmoUser> GetMe()
        {
            CheckLoginStatus();

            string result = await VenmoGet(venmoMeUrl, userAccessTokenQueryString);
            string resultData = JsonConvert.DeserializeObject<Dictionary<string, object>>(result)["data"].ToString();

            Dictionary<string, object> userData = JsonConvert.DeserializeObject<Dictionary<string, object>>(resultData);

            VenmoUser currentUser = JsonConvert.DeserializeObject<VenmoUser>(userData["user"].ToString());
            if (userData["balance"] != null)
            {
                currentUser.balance = double.Parse(userData["balance"].ToString());
            }
            else
            {
                currentUser.balance = -1;
            }
            VenmoHelper.currentAuth.currentUser = currentUser;
            return currentUser;
        }

        /// <summary>
        /// Gets a user by user ID from Venmo.
        /// </summary>
        /// <param name="userID">The desired user's ID</param>
        /// <exception cref="VenmoWrapper.NotLoggedInException">Throws a NotLoggedInException if the current user is not logged in.</exception>
        /// <returns>VenmoUser object with the desired user's info.</returns>
        public static async Task<VenmoUser> GetUser(long userID)
        {
            CheckLoginStatus();

            string userUrl = String.Format(venmoUserUrl, userID);
            string userJson = await VenmoGet(userUrl, userAccessTokenQueryString);
            Dictionary<string, object> userData = JsonConvert.DeserializeObject<Dictionary<string, object>>(userJson);
            return JsonConvert.DeserializeObject<VenmoUser>(userData["data"].ToString());
        }

        /// <summary>
        /// Gets the user's friend list.
        /// </summary>
        /// <param name="userID">The user ID of the user whose friends list is being queried.</param>
        /// <param name="limit">The number of users to return. Providing a number &lt;=0 or no number will retrieve all friends.</param>
        /// <exception cref="VenmoWrapper.NotLoggedInException">Throws a NotLoggedInException if the user is not logged in.</exception>
        /// <remarks>Venmo paginates this data, but this method retrieves the entire friends list by default.</remarks>
        /// <returns>Returns a List of VenmoUsers.</returns>
        public static async Task<List<VenmoUser>> GetFriendsList(long userID, int limit = 0)
        {
            CheckLoginStatus();

            bool downloadAll = false;
            if (limit <= 0)
            {
                downloadAll = true;
                limit = 50;
            }

            friendID = userID;
            friendQueryString = userAccessTokenQueryString + "&limit=" + limit;
            List<VenmoUser> friendsList = new List<VenmoUser>();

            do
            {
                friendsList.AddRange(await GetMoreFriends());
            } while (friendQueryString != "" && downloadAll);

            return friendsList;
        }

        /// <summary>
        /// Method which continues pagination initiated by GetFriendsList.
        /// </summary>
        /// <returns>A list of VenmoUsers (may be empty if no more pages).</returns>
        public static async Task<List<VenmoUser>> GetMoreFriends()
        {
            CheckLoginStatus();

            if (String.IsNullOrEmpty(friendQueryString))
            {
                return new List<VenmoUser>();
            }

            string friendsUrl = String.Format(venmoFriendsUrl, friendID);

            string result = await VenmoGet(friendsUrl, friendQueryString);
            Dictionary<string, object> friendsData = JsonConvert.DeserializeObject<Dictionary<string, object>>(result);
            List<VenmoUser> friendsList = JsonConvert.DeserializeObject<List<VenmoUser>>(friendsData["data"].ToString());

            JObject pagData = (JObject)friendsData["pagination"];
            friendQueryString = pagData.HasValues ? userAccessTokenQueryString + "&" + pagData["next"].ToString().Split('?')[1] : "";
            return friendsList;
        }

        /// <summary>
        /// Given a transaction ID, returns the VenmoTransaction corresponding to that transaction.
        /// </summary>
        /// <param name="transactionID">The id of the transaction to be returned.</param>
        /// <exception cref="VenmoWrapper.NotLoggedInException">Throws a NotLoggedInException if the user is not logged in.</exception>
        /// <returns>Returns the VenmoTransaction requested</returns>
        public static async Task<VenmoTransaction> GetTransaction(long transactionID)
        {
            CheckLoginStatus();

            string paymentUrl = String.Format(venmoIndividualPaymentUrl, transactionID);
            string venmoResponse = await VenmoGet(paymentUrl, userAccessTokenQueryString);

            Dictionary<string, object> transactionData = JsonConvert.DeserializeObject<Dictionary<string, object>>(venmoResponse);
            VenmoTransaction trans = JsonConvert.DeserializeObject<VenmoTransaction>(transactionData["data"].ToString());
            return trans;
        }

        /// <summary>
        /// Retrieves recent transactions in which the current user has been involved.
        /// </summary>
        /// <param name="limit">The number of transactions to retrieve. When left out, defaults to 25.</param>
        /// <exception cref="VenmoWrapper.NotLoggedInException">Throws a NotLoggedInException if the user is not logged in.</exception>
        /// <returns>List of VenmoTransactions</returns>
        public static async Task<List<VenmoTransaction>> GetRecentTransactions(int limit = 25)
        {
            CheckLoginStatus();

            if (limit <= 0)
            {
                return new List<VenmoTransaction>();
            }

            transactionQueryString = userAccessTokenQueryString + "&limit=" + limit;
            return await GetMoreTransactions();
        }

        /// <summary>
        /// Method which continues pagination initiated by GetRecentTransactions.
        /// </summary>
        /// <returns>A list of VenmoTransactions (may be empty if no more pages).</returns>
        public static async Task<List<VenmoTransaction>> GetMoreTransactions()
        {
            CheckLoginStatus();

            if (String.IsNullOrEmpty(transactionQueryString))
            {
                return new List<VenmoTransaction>();
            }

            string venmoResponse = await VenmoGet(venmoPaymentUrl, transactionQueryString);

            Dictionary<string, object> transactionData = JsonConvert.DeserializeObject<Dictionary<string, object>>(venmoResponse);
            List<VenmoTransaction> recentTransactions = JsonConvert.DeserializeObject<List<VenmoTransaction>>(transactionData["data"].ToString());
            JObject pagData = (JObject)transactionData["pagination"];
            transactionQueryString = pagData.HasValues ? userAccessTokenQueryString + "&" + pagData["next"].ToString().Split('?')[1] : "";

            return recentTransactions;
        }

        #endregion

        #region Helper Functions

        //Because of Venmo's change to how authentication works, auth tokens now only last for 60 days.
        //Once the user's token has expired, this method will be called and the login will be refreshed.
        private static async void RefreshLogin()
        {
            string postData = "client_id=" + clientID + "&client_secret=" + clientSecret + "&refresh_token=" + VenmoHelper.currentAuth.refreshToken;
            string venmoResponse = await VenmoPost(venmoAuthUrl, postData);

            Dictionary<string, object> results = JsonConvert.DeserializeObject<Dictionary<string, object>>(venmoResponse);
            string refresh_token = (string)results["refresh_token"];
            string user_access_token = (string)results["access_token"];
            long expire_time = (long)results["expires_in"];
            DateTime expires_in = DateTime.Now.AddSeconds(expire_time);
            currentAuth.RefreshLogin(refresh_token, user_access_token, expires_in);
        }

        //Executes a GET against a specific Venmo endpoint with a specified query string.
        private static async Task<string> VenmoGet(string url, string queryString)
        {
            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(url + queryString);
            webRequest.Method = "GET";
            webRequest.Headers[HttpRequestHeader.IfModifiedSince] = DateTime.Now.ToString();
            var webResponse = (HttpWebResponse)(await Task<WebResponse>.Factory.FromAsync(webRequest.BeginGetResponse, (Func<IAsyncResult, WebResponse>)webRequest.BetterEndGetResponse, null));

            string responseCode = webResponse.StatusCode.ToString();
            string response = GetContentFromWebResponse(webResponse);

            errorCheck(responseCode, response);

            return response;
        }

        //Executes a POST against a specific Venmo endpoint with specified data for the body of the POST.
        private static async Task<string> VenmoPost(string url, string postData)
        {
            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            webRequest.Method = "POST";
            webRequest.ContentType = "application/x-www-form-urlencoded";

            using (Stream reqStream = await Task<Stream>.Factory.FromAsync(webRequest.BeginGetRequestStream, webRequest.EndGetRequestStream, null))
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                reqStream.Write(byteArray, 0, byteArray.Length);
            }

            var webResponse = (HttpWebResponse)(await Task<WebResponse>.Factory.FromAsync(webRequest.BeginGetResponse, (Func<IAsyncResult, WebResponse>)webRequest.BetterEndGetResponse, null));

            string responseCode = webResponse.StatusCode.ToString();
            string response = GetContentFromWebResponse(webResponse);

            errorCheck(responseCode, response);

            return response;
        }

        #endregion

        #region Utilities

        //Checks a response from Venmo for errors and throws the appropriate exception if an error has occurred.
        public static void errorCheck(string responseCode, string response)
        {
            if (responseCode != "OK" && response != "")
            {
                var definition = new { message = "", code = 0 };
                Dictionary<string, object> message = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);
                var error = JsonConvert.DeserializeAnonymousType(message["error"].ToString(), definition);
                //TODO Handle all the errors Venmo could give instead of just revoked token.
                if (error.code == 262)
                {
                    logOut();
                    throw new NotLoggedInException(notLoggedInError);
                }
                else
                {
                    throw new VenmoException(error.message);
                }
            }
            else if (response == "")
            {
                throw new VenmoException("Your internet connection appears to be having some problems. Check it out and try again!");
            }
        }

        //Gets the content from a given webresponse, or returns an empty string if there has been an error.
        public static string GetContentFromWebResponse(HttpWebResponse response)
        {
            if (response == null)
            {
                return "";
            }
            using (StreamReader httpWebStreamReader = new StreamReader(response.GetResponseStream()))
            {
                string result = httpWebStreamReader.ReadToEnd();
                return result;
            }
        }

        //Ensures the user is logged in and that their login has not expired.
        private static void CheckLoginStatus()
        {
            if (!loggedIn)
            {
                throw new NotLoggedInException(notLoggedInError);
            }
            if (currentAuth != null && currentAuth.expireTime <= DateTime.Now)
            {
                RefreshLogin();
            }
        }

        //Removes all user-specific information and resets the VenmoHelper to the not-logged-in state.
        public static void logOut()
        {
            VenmoHelper.loggedIn = false;
            VenmoHelper.currentAuth = null;
        }

        #endregion

    }

    //Exception to be thrown if the user is not logged in.
    public class NotLoggedInException : System.Exception
    {
        public NotLoggedInException() : base() { }
        public NotLoggedInException(string message) : base(message) { }
        public NotLoggedInException(string message, System.Exception inner) : base(message, inner) { }
    }

    //Catch-all exception class for any errors returned from Venmo.
    public class VenmoException : System.Exception
    {
        public VenmoException() : base() { }
        public VenmoException(string message) : base(message) { }
        public VenmoException(string message, System.Exception inner) : base(message, inner) { }
    }

    //-----------------------------------------------------------------------
    //
    //     Copyright (c) 2011 Garrett Serack. All rights reserved.
    //
    //
    //     The software is licensed under the Apache 2.0 License (the "License")
    //     You may not use the software except in compliance with the License.
    //
    //-----------------------------------------------------------------------
    public static class WebRequestExtension
    {
        public static WebResponse BetterEndGetResponse(this WebRequest request, IAsyncResult asyncResult)
        {
            try
            {
                return request.EndGetResponse(asyncResult);
            }
            catch (WebException wex)
            {
                if (wex.Response != null)
                {
                    return wex.Response;
                }
                throw;
            }
        }
    }
}
