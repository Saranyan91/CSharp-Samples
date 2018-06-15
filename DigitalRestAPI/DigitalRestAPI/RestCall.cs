using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Web;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace DigitalRestAPI
{
    public enum enmRestMethod
    {
        /// <summary>
        /// Typically used for object update operations
        /// </summary>
        PUT,
        /// <summary>
        /// Typically used for object creation operations
        /// </summary>
        POST,
        /// <summary>
        /// Typically used for query operations
        /// </summary>
        GET,
        /// <summary>
        /// Typically used for object destruction operations
        /// </summary>
        DELETE,

        NotSet
    }

    /// <summary>
    /// A utility class to build a valid Uri for the REST call. It adds the necessary parameters,
    /// generates the signature, and encodes the data.
    /// </summary>
    public class RestUriBuilder
    {
        #region Attributes
        private string m_host = null;
        private byte[] m_secret = null;
        private enmRestMethod m_method = enmRestMethod.NotSet;
        private string m_operation = null;
        private SortedDictionary<string, string> m_parameters = new SortedDictionary<string, string>();
        private string m_body = null;
        #endregion

        /// <summary>
        /// The Domain URL
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
        /// Base 64 encoded string representing the secret used for the signature.
        /// </summary>
        public string SecretString
        {
            get
            {
                if (m_secret != null)
                {
                    try
                    {
                        return Convert.ToBase64String(m_secret);
                    }
                    catch (Exception ex)
                    {
                        throw new ArgumentException("Unable to convert secret from base64 encoding", ex);
                    }
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value != null)
                {
                    try
                    {
                        m_secret = Convert.FromBase64String(value);
                    }
                    catch (Exception ex)
                    {
                        throw new ArgumentException("Unable to convert string to base64 encoding", ex);
                    }
                }
                else
                {
                    m_secret = null;
                }
            }
        }

        /// <summary>
        /// The binary data of the secret.
        /// </summary>
        public byte[] Secret
        {
            get
            {
                return m_secret;
            }
            set
            {
                m_secret = value;
            }
        }

        /// <summary>
        /// The method used 
        /// </summary>
        public enmRestMethod Method
        {
            get
            {
                return m_method;
            }
            set
            {
                m_method = value;
            }
        }

        /// <summary>
        /// The relative path for the REST operation.
        /// </summary>
        public string RestOperation
        {
            get
            {
                return m_operation;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Operation cannot be null");
                }
                if (value == "")
                {
                    throw new ArgumentException("Operation cannot be empty");
                }

                if (!value.StartsWith("/"))
                {
                    m_operation = "/" + value;
                }
                else
                {
                    m_operation = value;
                }
            }
        }

        /// <summary>
        /// The set of parameters associated with the operation. The class will add the Timestamp and 
        /// SignatureVersion parameters if omitted. The Signature parameter will be recalculated if provided.
        /// </summary>
        public SortedDictionary<string, string> Parameters
        {
            get
            {
                return m_parameters;
            }
            set
            {
                if (value == null)
                {
                    m_parameters.Clear();
                }
                else
                {
                    m_parameters = value;
                }
            }
        }

        /// <summary>
        /// The body of the request. This is needed for the signature. Default to NULL if none.
        /// </summary>
        public string RequestBody
        {
            get
            {
                return m_body;
            }
            set
            {
                m_body = value;
            }
        }

        /// <summary>
        /// Compsoes the Uri associated with the data provided.
        /// </summary>
        public Uri ComposedUri
        {
            get
            {
                VerifySettings();

                EnsureCoreParameters();

                AddSignature();

                UriBuilder builder = new UriBuilder();
                builder.Host = m_host;
                builder.Path = m_operation;
                builder.Query = GenerateQueryString();
                builder.Scheme = "https";

                return builder.Uri;
            }
        }

        /// <summary>
        /// Ensure all the necessary properties are supplied
        /// </summary>
        private void VerifySettings()
        {
            if ((m_host == null) || (m_host == ""))
            {
                throw new InvalidOperationException("Host property not set");
            }
            if (m_secret == null)
            {
                throw new InvalidOperationException("Secret property not set");
            }
            if (m_method == enmRestMethod.NotSet)
            {
                throw new InvalidOperationException("Method property not set");
            }
            if ((m_operation == null) || (m_operation == ""))
            {
                throw new InvalidOperationException("Operation property not set");
            }
        }

        /// <summary>
        /// Traverses the parameter list, adding missing key parameters if needed or removing calculated values.
        /// </summary>
        private void EnsureCoreParameters()
        {
            // Add any missing keys
            if (!m_parameters.ContainsKey("Timestamp"))
            {
                m_parameters.Add("Timestamp", DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddThh:mm:ss"));
            }
            if (!m_parameters.ContainsKey("SignatureVersion"))
            {
                m_parameters.Add("SignatureVersion", "1");
            }

            // Remove calcuated keys
            if (m_parameters.ContainsKey("Signature"))
            {
                m_parameters.Remove("Signature");
            }
        }

        /// <summary>
        /// Returns the canonical query string based on the parameters supplied
        /// </summary>
        /// <returns></returns>
        private string GenerateCanonicalQueryString()
        {
            //
            // ASSERT: m_parameters keys are sorted. This is a property of the SortedDictionary.
            //

            string queryString = "";
            foreach (KeyValuePair<string, string> kvp in m_parameters)
            {
                queryString += "&" + UrlEncodeUpperCase(kvp.Key) + "=" + UrlEncodeUpperCase(kvp.Value);
            }

            return queryString.TrimStart('&').TrimEnd('\n');
        }

        /// <summary>
        /// URL Encodes the string provided. Percent encoded strings use upper case hexadecimal characters. This is needed to match the backend implementation.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string UrlEncodeUpperCase(string value)
        {
            value = HttpUtility.UrlEncode(value, System.Text.Encoding.UTF8);
            return Regex.Replace(value, "(%[0-9a-f][0-9a-f])", c => c.Value.ToUpper());
        }

        /// <summary>
        /// Calculate the signature and add it to the parameter list.
        /// </summary>
        private void AddSignature()
        {
            //
            // Sample string to sign:
            //
            // GETsandbox.digitalpaytech.com/REST/TokenAccount=Rest+User+1&SignatureVersion=1&Timestamp=2010-01-01T05%3A00%3A00

            string stringToSign =
                m_method.ToString() +
                m_host.ToString() +
                m_operation +
                GenerateCanonicalQueryString();

            if (m_body != null)
            {
                stringToSign += m_body;
            }

            HMACSHA256 hmac = new HMACSHA256(m_secret);

            byte[] signatureByte = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(stringToSign));
            string signature = Convert.ToBase64String(signatureByte);

            m_parameters.Add("Signature", signature);
        }

        /// <summary>
        /// Returns the parameter list conforming the URI query
        /// </summary>
        /// <returns></returns>
        private string GenerateQueryString()
        {
            string queryString = "";
            foreach (KeyValuePair<string, string> kvp in m_parameters)
            {
                queryString += UrlEncodeUpperCase(kvp.Key) + "=" + UrlEncodeUpperCase(kvp.Value) + "&";
            }

            return queryString.TrimEnd('&');
        }
    }
}
