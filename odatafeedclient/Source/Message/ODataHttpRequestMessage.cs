// -----------------------------------------------------------------------
// <copyright file="ODataHttpRequestMessage.cs" company="Self">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace ODataFeedClient.Messages
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using Microsoft.Data.OData;

    /// <summary>
    /// Http Request message implementation to send request to an OData service
    /// </summary>
    public class ODataHttpRequestMessage : IODataRequestMessage
    {
        /// <summary>
        /// the http web request used to make the current request.
        /// </summary>
        private HttpWebRequest underlyingWebRequest;

        /// <summary>
        /// The stream containing the request payload.
        /// </summary>
        private Stream underlyingRequestStream;

        /// <summary>
        /// Initializes a new instance of the ODataHttpRequestMessage class.
        /// </summary>
        /// <param name="webRequest">the http web request used to make the current request.</param>
        /// <param name="requestStream">The stream containing the request payload.</param>
        public ODataHttpRequestMessage(HttpWebRequest webRequest, Stream requestStream)
        {
            this.underlyingWebRequest = webRequest;
            this.underlyingRequestStream = requestStream;
        }

        /// <summary>
        /// Gets or sets the url to send the current request to.
        /// </summary>
        public Uri Url
        {
            get
            {
                return this.underlyingWebRequest.RequestUri;
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Gets the Http headers
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> Headers
        {
            get
            {
                return this.underlyingWebRequest.Headers.AllKeys.Select(key => new KeyValuePair<string, string>(key, this.underlyingWebRequest.Headers[key]));
            }
        }

        /// <summary>
        /// Gets or sets the Http method used to make this request.
        /// </summary>
        public string Method
        {
            get
            {
                return this.underlyingWebRequest.Method;
            }

            set
            {
                this.underlyingWebRequest.Method = value;
            }
        }

        /// <summary>
        /// Sets the value of the http header with the name <paramref name="headerName"/> to <param name="headerValue"/>
        /// </summary>
        /// <param name="headerName">Name of Http header</param>
        /// <param name="headerValue">value of Http header</param>
        public void SetHeader(string headerName, string headerValue)
        {
            if (headerName == "Content-Type")
            {
                this.underlyingWebRequest.ContentType = headerValue;
            }
        }

        /// <summary>
        /// Gets the value of the http header with the name <paramref name="headerName"/>
        /// </summary>
        /// <param name="headerName">Name of Http header</param>
        /// <returns>the value of the http header with the name <paramref name="headerName"/></returns>
        public string GetHeader(string headerName)
        {
            return this.underlyingWebRequest.Headers[headerName];
        }

        /// <summary>
        /// Gets the stream representing the OData payload to the be read.
        /// </summary>
        /// <returns>The stream representing the OData payload to the be read.</returns>
        public Stream GetStream()
        {
            return this.underlyingRequestStream;
        }
    }
}