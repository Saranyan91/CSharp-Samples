using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;
using System.Web;
using System.Xml;

namespace DigitalRestAPI
{
    /// <summary>
    /// This class contains the sample code for each operation. Some functions can be used verbatum like RetrieveSessionToken and 
    /// RetrieveRegions. Others like InsertNewPermit, need to be tweeked to convert your application's data to XML.
    /// </summary>
    class DigitalAPIOperations
    {
        #region Attributes
        private string m_sessionToken = "";
        private string m_host = "";
        private string m_account = "";
        private string m_secret = "";
        #endregion

        #region Properties
        /// <summary>
        /// The current active session token returned from the server.
        /// </summary>
        public string SessionToken
        {
            get
            {
                return m_sessionToken;
            }
        }

        /// <summary>
        /// The EMS server to send/recieve messages.
        /// </summary>
        public string Host
        {
            get
            {
                return m_host;
            }
            set
            {
                m_host = value;
            }
        }

        /// <summary>
        /// The account name used to send/recieve messages.
        /// </summary>
        public string AccountName
        {
            get
            {
                return m_account;
            }
            set
            {
                m_account = value;
            }
        }

        /// <summary>
        /// The shared secret used in each message.
        /// </summary>
        public string Secret
        {
            set
            {
                m_secret = value;
            }
        }
        #endregion

        #region Operations

        #region Session Tokens
        /// <summary>
        /// Retrieve a session token from the server. The token will be used in future REST requests until it is released or expired.
        /// </summary>
        /// <returns></returns>
        public HttpStatusCode RetrieveSessionToken()
        {
            RestUriBuilder uri = new RestUriBuilder();

            uri.Host = m_host;
            uri.SecretString = m_secret;

            uri.Method = enmRestMethod.GET;
            uri.RestOperation = "/REST/Token";
            uri.Parameters.Add("Account", m_account);
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(uri.ComposedUri);
            webRequest.Method = "GET";

            XmlDocument xmlResponse = new XmlDocument();

            HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
            xmlResponse.Load(webResponse.GetResponseStream());

            m_sessionToken = xmlResponse.InnerText;

            return webResponse.StatusCode;
        }

        /// <summary>
        /// Release the assigned session token.
        /// </summary>
        /// <returns></returns>
        public HttpStatusCode ReleaseToken()
        {
            RestUriBuilder uri = new RestUriBuilder();

            uri.Host = m_host;
            uri.SecretString = m_secret;

            //
            // Release token
            //
            uri.Method = enmRestMethod.PUT;
            uri.Parameters.Clear();
            uri.RestOperation = "/REST/Token";
            uri.Parameters.Add("Token", m_sessionToken);
            uri.RequestBody = null;
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(uri.ComposedUri);
            webRequest.Method = "PUT";

            HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();

            m_sessionToken = "";

            return webResponse.StatusCode;
        }
        #endregion

        #region Region Operations
        /// <summary>
        /// Returns a list of regions associated with the token.
        /// </summary>
        /// <returns>An XML list of regions associated with the account.</returns>
        public XmlDocument RetrieveRegions()
        {
            RestUriBuilder uri = new RestUriBuilder();

            uri.Host = m_host;
            uri.SecretString = m_secret;

            uri.Method = enmRestMethod.GET;
            uri.Parameters.Clear();
            uri.RestOperation = "/REST/Region";
            uri.Parameters.Add("Token", m_sessionToken);
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(uri.ComposedUri);
            webRequest.Method = "GET";

            XmlDocument xmlResponse = new XmlDocument();
            xmlResponse.Load(webRequest.GetResponse().GetResponseStream());

            return xmlResponse;
        }
        #endregion

        #region Permit Operations
        /// <summary>
        /// Creates a new permit.
        /// </summary>
        /// <param name="permitData"></param>
        public HttpStatusCode InsertNewPermit(XmlDocument permitData)
        {
            RestUriBuilder uri = new RestUriBuilder();

            uri.Host = m_host;
            uri.SecretString = m_secret;

            string requestBody = permitData.OuterXml;
            byte[] requestBodyData = System.Text.Encoding.UTF8.GetBytes(requestBody);

            uri.Method = enmRestMethod.POST;
            uri.RestOperation = "/REST/Permit";
            uri.Parameters.Add("Token", m_sessionToken);
            uri.RequestBody = requestBody;
            HttpWebRequest permitRequest = (HttpWebRequest)WebRequest.Create(uri.ComposedUri);
            permitRequest.Method = "POST";
            permitRequest.ContentType = "application/xml";
            permitRequest.ContentLength = requestBodyData.Length;

            Stream requestStream = permitRequest.GetRequestStream();
            requestStream.Write(requestBodyData, 0, requestBodyData.Length);
            requestStream.Close();

            HttpWebResponse permitResponse = (HttpWebResponse)permitRequest.GetResponse();
            return permitResponse.StatusCode;
        }

        /// <summary>
        /// Returns the permit associated with the permit number supplied.
        /// </summary>
        /// <param name="permitNumber"></param>
        /// <returns></returns>
        public XmlDocument RetrievePermit(int permitNumber)
        {
            RestUriBuilder uri = new RestUriBuilder();

            uri.Host = m_host;
            uri.SecretString = m_secret;

            uri.Method = enmRestMethod.GET;
            uri.Parameters.Clear();
            uri.RestOperation = "/REST/Permit";
            uri.Parameters.Add("Token", m_sessionToken);
            uri.Parameters.Add("PermitNumber", permitNumber.ToString());
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(uri.ComposedUri);
            webRequest.Method = "GET";

            HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();

            XmlDocument permitXml = new XmlDocument();
            permitXml.Load(webResponse.GetResponseStream());

            return permitXml;
        }

        /// <summary>
        /// Sends the updated permit information to the server.
        /// </summary>
        /// <param name="updatePermit"></param>
        public HttpStatusCode UpdatePermit(XmlDocument updatePermit)
        {
            RestUriBuilder uri = new RestUriBuilder();

            uri.Host = m_host;
            uri.SecretString = m_secret;

            string requestBody = updatePermit.OuterXml;
            byte[] requestBodyData = System.Text.Encoding.UTF8.GetBytes(requestBody);

            uri.Method = enmRestMethod.PUT;
            uri.Parameters.Clear();
            uri.RestOperation = "/REST/Permit";
            uri.Parameters.Add("Token", m_sessionToken);
            uri.RequestBody = requestBody;
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(uri.ComposedUri);
            webRequest.Method = "PUT";
            webRequest.ContentType = "application/xml";
            webRequest.ContentLength = requestBodyData.Length;

            Stream responseStream = webRequest.GetRequestStream();
            responseStream.Write(requestBodyData, 0, requestBodyData.Length);
            responseStream.Close();

            HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
            if ((webResponse.StatusCode != HttpStatusCode.Continue) && // The permit was updated only
                (webResponse.StatusCode != HttpStatusCode.Created))    // The update resulted in new data being created in the system.
            {
                throw new WebException("Unexpected status code", null, WebExceptionStatus.Success, webResponse);
            }

            return webResponse.StatusCode;
        }
        #endregion

        #region Coupon Operations
        /// <summary>
        /// Creates a new coupon.
        /// </summary>
        /// <param name="couponCode">the coupon number of add</param>
        /// <param name="couponData">The XML document containing the coupon data.</param>
        /// <returns></returns>
        public HttpStatusCode InsertCoupon(string couponCode, XmlDocument couponData)
        {
            RestUriBuilder uri = new RestUriBuilder();

            uri.Host = m_host;
            uri.SecretString = m_secret;

            string requestBody = couponData.OuterXml;
            byte[] requestBodyData = System.Text.Encoding.UTF8.GetBytes(requestBody);

            uri.Method = enmRestMethod.POST;
            uri.RestOperation = "/REST/Coupons/" + couponCode;
            uri.Parameters.Add("Token", m_sessionToken);
            uri.RequestBody = requestBody;

            HttpWebRequest restRequest = (HttpWebRequest)WebRequest.Create(uri.ComposedUri);
            restRequest.Method = "POST";
            restRequest.ContentType = "application/xml";
            restRequest.ContentLength = requestBodyData.Length;

            Stream requestStream = restRequest.GetRequestStream();
            requestStream.Write(requestBodyData, 0, requestBodyData.Length);
            requestStream.Close();

            HttpWebResponse restResponse = (HttpWebResponse)restRequest.GetResponse();

            return restResponse.StatusCode;
        }

        /// <summary>
        /// Returns the coupon associated with the coupon code supplied.
        /// </summary>
        /// <param name="couponCode">the coupon to retrieve</param>
        /// <param name="couponXml">An XML document containing the xml</param>
        /// <returns></returns>
        public HttpStatusCode RetrieveCoupon(string couponCode, out XmlDocument couponXml)
        {
            RestUriBuilder uri = new RestUriBuilder();

            uri.Host = m_host;
            uri.SecretString = m_secret;

            uri.Method = enmRestMethod.GET;
            uri.Parameters.Clear();
            uri.RestOperation = "/REST/Coupons/" + couponCode;
            uri.Parameters.Add("Token", m_sessionToken);
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(uri.ComposedUri);
            webRequest.Method = "GET";

            HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();

            if (webResponse.StatusCode == HttpStatusCode.OK)
            {
                couponXml = new XmlDocument();
                couponXml.Load(webResponse.GetResponseStream());
            }
            else
            {
                couponXml = null;
            }

            return webResponse.StatusCode;
        }

        /// <summary>
        /// Removes a coupon from EMS
        /// </summary>
        /// <param name="couponCode">The coupon code to remove.</param>
        /// <param name="deletedCoupon">The coupon code that was removed</param>
        /// <returns></returns>
        public HttpStatusCode DeleteCoupon(string couponCode, out XmlDocument deletedCoupon)
        {
            RestUriBuilder uri = new RestUriBuilder();

            uri.Host = m_host;
            uri.SecretString = m_secret;

            uri.Method = enmRestMethod.DELETE;
            uri.RestOperation = "/REST/Coupons/" + couponCode;
            uri.Parameters.Add("Token", m_sessionToken);
            HttpWebRequest restRequest = (HttpWebRequest)WebRequest.Create(uri.ComposedUri);
            restRequest.Method = "DELETE";

            HttpWebResponse restResponse = (HttpWebResponse)restRequest.GetResponse();

            if (restResponse.StatusCode == HttpStatusCode.OK)
            {
                deletedCoupon = new XmlDocument();
            }
            else
            {
                deletedCoupon = null;
            }

            return restResponse.StatusCode;
        }

        /// <summary>
        /// Updates a coupon in EMS.
        /// </summary>
        /// <param name="couponCode">The coupon code to update.</param>
        /// <param name="couponData">An XML document containing the coupon data to update.</param>
        /// <returns></returns>
        public HttpStatusCode UpdateCoupon(string couponCode, XmlDocument couponData)
        {
            RestUriBuilder uri = new RestUriBuilder();

            uri.Host = m_host;
            uri.SecretString = m_secret;

            string requestBody = couponData.OuterXml;
            byte[] requestBodyData = System.Text.Encoding.UTF8.GetBytes(requestBody);

            uri.Method = enmRestMethod.PUT;
            uri.RestOperation = "/REST/Coupons/" + couponCode;
            uri.Parameters.Add("Token", m_sessionToken);
            uri.RequestBody = requestBody;
            HttpWebRequest restRequest = (HttpWebRequest)WebRequest.Create(uri.ComposedUri);
            restRequest.Method = "PUT";
            restRequest.ContentType = "application/xml";
            restRequest.ContentLength = requestBodyData.Length;

            Stream requestStream = restRequest.GetRequestStream();
            requestStream.Write(requestBodyData, 0, requestBodyData.Length);
            requestStream.Close();

            HttpWebResponse restResponse = (HttpWebResponse)restRequest.GetResponse();
            return restResponse.StatusCode;
        }

        /// <summary>
        /// Creates a collection of new coupons.
        /// </summary>
        /// <param name="couponData">The XML documentat containing the coupons to create.</param>
        /// <param name="responseCodes">The XML document contianing the response to each coupon created.</param>
        /// <returns></returns>
        public HttpStatusCode InsertCoupons(XmlDocument couponData, out XmlDocument responseCodes)
        {
            RestUriBuilder uri = new RestUriBuilder();

            uri.Host = m_host;
            uri.SecretString = m_secret;

            string requestBody = couponData.OuterXml;
            byte[] requestBodyData = System.Text.Encoding.UTF8.GetBytes(requestBody);

            uri.Method = enmRestMethod.POST;
            uri.RestOperation = "/REST/Coupons";
            uri.Parameters.Add("Token", m_sessionToken);
            uri.RequestBody = requestBody;
            HttpWebRequest restRequest = (HttpWebRequest)WebRequest.Create(uri.ComposedUri);
            restRequest.Method = "POST";
            restRequest.ContentType = "application/xml";
            restRequest.ContentLength = requestBodyData.Length;

            Stream requestStream = restRequest.GetRequestStream();
            requestStream.Write(requestBodyData, 0, requestBodyData.Length);
            requestStream.Close();

            HttpWebResponse restResponse = (HttpWebResponse)restRequest.GetResponse();

            if (restResponse.StatusCode == HttpStatusCode.OK)
            {
                responseCodes = new XmlDocument();
                responseCodes.Load(restResponse.GetResponseStream());
            }
            else
            {
                responseCodes = null;
            }

            return restResponse.StatusCode;
        }

        /// <summary>
        /// Returns the coupon associated with the coupon code supplied.
        /// </summary>
        /// <param name="couponCodes">A comma separated list of coupon codes to retrieve.</param>
        /// <param name="coupons">An XML document containing all the coupons retrieved</param>
        /// <returns></returns>
        public HttpStatusCode RetrieveCoupons(string couponCodes, out XmlDocument coupons)
        {
            RestUriBuilder uri = new RestUriBuilder();

            uri.Host = m_host;
            uri.SecretString = m_secret;

            uri.Method = enmRestMethod.GET;
            uri.Parameters.Clear();
            uri.RestOperation = "/REST/Coupons";
            uri.Parameters.Add("Token", m_sessionToken);
            uri.Parameters.Add("CouponCodes", couponCodes);
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(uri.ComposedUri);
            webRequest.Method = "GET";

            HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();

            coupons = new XmlDocument();
            coupons.Load(webResponse.GetResponseStream());

            return webResponse.StatusCode;
        }

        /// <summary>
        /// Removes a list of coupons from EMS.
        /// </summary>
        /// <param name="couponCodes">A list of coupon codes to delete.</param>
        /// <param name="responseCodes">The XML document contianing the response to each coupon deleted.</param>
        /// <returns></returns>
        public HttpStatusCode DeleteCoupons(XmlDocument couponCodes, out XmlDocument deletedCoupons)
        {
            RestUriBuilder uri = new RestUriBuilder();

            uri.Host = m_host;
            uri.SecretString = m_secret;

            string requestBody = couponCodes.OuterXml;
            byte[] requestBodyData = System.Text.Encoding.UTF8.GetBytes(requestBody);

            uri.Method = enmRestMethod.DELETE;
            uri.RestOperation = "/REST/Coupons";
            uri.Parameters.Add("Token", m_sessionToken);
            uri.RequestBody = requestBody;
            HttpWebRequest restRequest = (HttpWebRequest)WebRequest.Create(uri.ComposedUri);
            restRequest.Method = "DELETE";
            restRequest.ContentType = "application/xml";
            restRequest.ContentLength = requestBodyData.Length;

            Stream requestStream = restRequest.GetRequestStream();
            requestStream.Write(requestBodyData, 0, requestBodyData.Length);
            requestStream.Close();

            HttpWebResponse restResponse = (HttpWebResponse)restRequest.GetResponse();

            if (restResponse.StatusCode == HttpStatusCode.OK)
            {
                deletedCoupons = new XmlDocument();
                deletedCoupons.Load(restResponse.GetResponseStream());
            }
            else
            {
                deletedCoupons = null;
            }

            return restResponse.StatusCode;
        }

        /// <summary>
        /// Update a collection of coupons at one time.
        /// </summary>
        /// <param name="coupons">The XML document detailing the coupons to update.</param>
        /// <param name="responseCodes">The XML document contianing the response to each coupon updated.</param>
        /// <returns></returns>
        public HttpStatusCode UpdateCoupons(XmlDocument coupons, out XmlDocument responseCodes)
        {
            RestUriBuilder uri = new RestUriBuilder();

            uri.Host = m_host;
            uri.SecretString = m_secret;

            string requestBody = coupons.OuterXml;
            byte[] requestBodyData = System.Text.Encoding.UTF8.GetBytes(requestBody);

            uri.Method = enmRestMethod.PUT;
            uri.RestOperation = "/REST/Coupons";
            uri.Parameters.Add("Token", m_sessionToken);
            uri.RequestBody = requestBody;
            HttpWebRequest restRequest = (HttpWebRequest)WebRequest.Create(uri.ComposedUri);
            restRequest.Method = "PUT";
            restRequest.ContentType = "application/xml";
            restRequest.ContentLength = requestBodyData.Length;

            Stream requestStream = restRequest.GetRequestStream();
            requestStream.Write(requestBodyData, 0, requestBodyData.Length);
            requestStream.Close();

            HttpWebResponse restResponse = (HttpWebResponse)restRequest.GetResponse();

            if (restResponse.StatusCode == HttpStatusCode.OK)
            {
                responseCodes = new XmlDocument();
                responseCodes.Load(restResponse.GetResponseStream());
            }
            else
            {
                responseCodes = null;
            }

            return restResponse.StatusCode;
        }

        #endregion

        #endregion
    }
}
