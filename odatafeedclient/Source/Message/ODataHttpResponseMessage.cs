// -----------------------------------------------------------------------
// <copyright file="ODataHttpResponseMessage.cs" company="Self">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace ODataFeedClient.Messages
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using Microsoft.Data.OData;

    /// <summary>
    /// Http Request message implementation to read response from an OData service
    /// </summary>
    public class ODataHttpResponseMessage : IODataResponseMessage
    {
        /// <summary>
        /// The web response containing the response stream.
        /// </summary>
        private HttpWebResponse underlyingWebResponse;

        /// <summary>
        /// Initializes a new instance of the ODataHttpResponseMessage class
        /// </summary>
        /// <param name="webResponse">The web response containing the response stream.</param>
        public ODataHttpResponseMessage(HttpWebResponse webResponse)
        {
            this.underlyingWebResponse = webResponse;
        }

        /// <summary>
        /// Gets the Http headers
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> Headers
        {
            get
            {
                return this.underlyingWebResponse.Headers.AllKeys.Select(key => new KeyValuePair<string, string>(key, this.underlyingWebResponse.Headers[key]));
            }
        }

        /// <summary>
        /// Gets or sets the Http Status code of the current response.
        /// </summary>
        public int StatusCode
        {
            get
            {
                return (int)this.underlyingWebResponse.StatusCode;
            }

            set
            {
                throw new System.NotSupportedException();
            }
        }

        /// <summary>
        /// Gets the value of the http header with the name <paramref name="headerName"/>
        /// </summary>
        /// <param name="headerName">Name of Http header</param>
        /// <returns>the value of the http header with the name <paramref name="headerName"/></returns>
        public string GetHeader(string headerName)
        {
            return this.underlyingWebResponse.Headers[headerName];
        }

        /// <summary>
        /// Gets the stream representing the OData payload to the be read.
        /// </summary>
        /// <returns>The stream representing the OData payload to the be read.</returns>
        public Stream GetStream()
        {
            return this.underlyingWebResponse.GetResponseStream();
        }

        /// <summary>
        /// Sets the value of the http header with the name <paramref name="headerName"/> to <param name="headerValue"/>
        /// </summary>
        /// <param name="headerName">Name of Http header</param>
        /// <param name="headerValue">value of Http header</param>
        public void SetHeader(string headerName, string headerValue)
        {
            this.underlyingWebResponse.Headers[headerName] = headerValue;
        }
    }
}
