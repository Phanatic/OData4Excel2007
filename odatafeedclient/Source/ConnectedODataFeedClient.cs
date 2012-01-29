// -----------------------------------------------------------------------
// <copyright file="ConnectedODataFeedClient.cs" company="Self">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace ODataFeedClient
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using Microsoft.Data.OData;
    using ODataFeedClient.Messages;
    using ODataFeedClient.Objects;
    using System.Threading;

    /// <summary>
    /// This type provides OData feed download support for an Http Url
    /// </summary>
    public class ConnectedODataFeedClient : IODataFeedClient
    {
        /// <summary>
        /// internal flag indicating whether the current request is marked for cancellation.
        /// </summary>
        private bool requestCancelled;

        /// <summary>
        /// internal flag indicating whether the current request has timed out.
        /// </summary>
        private bool requestTimedOut;

        /// <summary>
        /// Current HttpWebRequest object being used to download OData feeds.
        /// </summary>
        private HttpWebRequest currentRequest;

        /// <summary>
        /// This event is raised when the download completes.
        /// </summary>
        public event EventHandler<ODataFeedDownloadArgs> FeedDownloaded;

        /// <summary>
        /// Gets or sets the timeout,in milliseconds, before the current request times out
        /// </summary>
        public double? Timeout { get; set; }

        /// <summary>
        /// Begins downloading the OData feed located at <paramref name="requestUri"/>
        /// </summary>
        /// <param name="requestUriString">Location of OData feed</param>
        /// <returns>An AsyncResult token that the client of this implementation can wait on</returns>
        public IAsyncResult BeginDownload(string requestUriString)
        {
            Uri requestUri = new Uri(requestUriString, UriKind.RelativeOrAbsolute);
            Debug.Assert(requestUri.IsAbsoluteUri, "Request uri should be absolute");
            return this.BeginDownloadResults(requestUri, ODataFormat.Atom);
        }

        /// <summary>
        /// Cancels the current running download request.
        /// </summary>
        public void CancelRequest()
        {
            if (this.currentRequest != null)
            {
                this.requestCancelled = true;
                this.currentRequest.Abort();
            }
        }

        /// <summary>
        /// internal method to begin downloading OData feed.
        /// </summary>
        /// <param name="requestUri">Location of OData feed</param>
        /// <param name="format">The OData response format to make the request.</param>
        /// <returns>An AsyncResult token that the client of this implementation can wait on</returns>
        private IAsyncResult BeginDownloadResults(Uri requestUri, ODataFormat format)
        {
            this.requestCancelled = false;
            this.requestTimedOut = false;
            this.currentRequest = (HttpWebRequest)System.Net.HttpWebRequest.Create(requestUri);
            if (format == ODataFormat.Atom)
            {
                this.currentRequest.Accept = "application/atom+xml";
            }
            else if (format == ODataFormat.Json)
            {
                this.currentRequest.Accept = "application/json";
            }

            IAsyncResult result = this.currentRequest.BeginGetResponse(this.RequestCallback, this.currentRequest);
            if (this.Timeout != null)
            {
                ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, new WaitOrTimerCallback(TimeoutCallback), currentRequest, TimeSpan.FromMilliseconds(this.Timeout.Value), true);
            }

            return result;
        }

        private void TimeoutCallback(object state, bool timedOut)
        {
            if (timedOut)
            {
                HttpWebRequest request = state as HttpWebRequest;
                if (request != null)
                {
                    this.requestTimedOut = true;
                    request.Abort();
                }
            }
        }

        /// <summary>
        /// Callback function to return from the async begin download call.
        /// </summary>
        /// <param name="asyncResult">AsyncResult token indicating the current request.</param>
        private void RequestCallback(IAsyncResult asyncResult)
        {
            try
            {
                HttpWebRequest runningRequest = asyncResult.AsyncState as HttpWebRequest;
                HttpWebResponse response = (HttpWebResponse)runningRequest.EndGetResponse(asyncResult);
                this.ReadResponse(response);
            }
            catch (Exception requestException)
            {
                if (this.requestCancelled || this.requestTimedOut)
                {
                    this.RaiseFeedDownloaded(Enumerable.Empty<ODataEntity>(), null, null, null);
                }
                else
                {
                    this.RaiseFeedDownloaded(Enumerable.Empty<ODataEntity>(), null, null, requestException);
                }
            }
            finally
            {
                this.requestCancelled = false;
            }
        }

        /// <summary>
        /// Read the current OData feed download response.
        /// </summary>
        /// <param name="response">OData feed download response</param>
        private void ReadResponse(HttpWebResponse response)
        {
            ODataHttpResponseMessage odataResponseMessage = new ODataHttpResponseMessage(response);
            ODataMessageReader odataReader = new ODataMessageReader((IODataResponseMessage)odataResponseMessage, new ODataMessageReaderSettings());
            this.MaterializeResponse(odataReader);
        }

        /// <summary>
        /// Use the ODataReader to read results and convert them feed into usable CLR instances.
        /// </summary>
        /// <param name="odataReader">OData Reader representing current OData feed download.</param>
        private void MaterializeResponse(ODataMessageReader odataReader)
        {
            var feedReader = this.CreateReader(odataReader);
            List<ODataEntity> odataEntities = new List<ODataEntity>();
            Exception readingError = null;
            Uri nextPageLink = null;
            long? totalCount = null;

            try
            {
                ODataEntity currentEntity = new ODataEntity();

                while (feedReader.Read())
                {
                    // we never expand in this application, so there is no chance of getting back an expanded link
                    switch (feedReader.State)
                    {
                        case ODataReaderState.EntryStart:
                            currentEntity = new ODataEntity();
                            odataEntities.Add(currentEntity);
                            break;

                        case ODataReaderState.NavigationLinkEnd:
                            ODataNavigationLink navigationLink = (ODataNavigationLink)feedReader.Item;
                            currentEntity.NavigationProperties[navigationLink.Name] = navigationLink.Url;
                            break;

                        case ODataReaderState.EntryEnd:
                            ODataEntry entry = (ODataEntry)feedReader.Item;
                            currentEntity.TypeName = entry.TypeName;
                            currentEntity.EditLink = entry.EditLink;
                            currentEntity.ReadLink = entry.ReadLink;
                            if (entry.MediaResource != null)
                            {
                                currentEntity.MediaUri = entry.MediaResource.ReadLink.OriginalString;
                                currentEntity.MediaType = entry.MediaResource.ContentType;
                            }

                            foreach (var property in entry.Properties)
                            {
                                this.MaterializeProperty(currentEntity, property, property.Name);
                            }

                            break;
                        case ODataReaderState.FeedEnd:
                            ODataFeed feed = (ODataFeed)feedReader.Item;
                            nextPageLink = feed.NextPageLink;
                            totalCount = feed.Count;
                            break;
                    }
                }
            }
            catch (Exception deserializationError)
            {
                readingError = deserializationError;
            }

            this.RaiseFeedDownloaded(odataEntities, nextPageLink, totalCount, readingError);
        }

        /// <summary>
        /// Read instance property value and add to Property dictionary on ODataEntity instance.
        /// </summary>
        /// <param name="currentEntity">ODataEntity instance being initialized</param>
        /// <param name="property">The instance property value being read.</param>
        /// <param name="propertyName">The name of the instance property being read.</param>
        private void MaterializeProperty(ODataEntity currentEntity, ODataProperty property, string propertyName)
        {
            if (property.Value is ODataComplexValue)
            {
                // for a given complex value , we will flatten it out so that
                // Excel can show these values in a tabular format instead of having to nest rows.
                // e.g.: if the property value is BoxArt{ SmallUrl ="", LargeUrl =""}
                // we will convert it into .
                // instance[BoxArt.SmallUrl]  = "";
                // instance[BoxArt.LargeUrl]  = "";
                ODataComplexValue complexPropertyValue = (ODataComplexValue)property.Value;
                foreach (var primitiveProperty in complexPropertyValue.Properties)
                {
                    string flattenedPropertyName = string.Join(".", propertyName, primitiveProperty.Name);
                    this.MaterializeProperty(currentEntity, primitiveProperty, flattenedPropertyName);
                }
            }
            else if (property.Value is ODataCollectionValue)
            {
                // we don't support collections here yet.
            }
            else
            {
                // this is a primitive property, assign results to dictionary.
                currentEntity[propertyName] = property.Value;
            }
        }

        /// <summary>
        /// Creates an OData reader to read OData feed download response.
        /// </summary>
        /// <param name="reader">The message reader containing current OData feed download response.</param>
        /// <returns>an OData reader to read OData feed download response</returns>
        private ODataReader CreateReader(ODataMessageReader reader)
        {
            ODataReader resultReader = null;
            resultReader = reader.CreateODataFeedReader();
            return resultReader;
        }

        /// <summary>
        /// Raises the "FeedDownloadComplete" event.
        /// </summary>
        /// <param name="odataEntities">Entities downloaded.</param>
        /// <param name="nextPageLink">NexLink uri to the next page of results.</param>
        /// <param name="totalCount">Total count of entities available at the request Uri.</param>
        /// <param name="readingError">Any error that occured during downloading/reading of OData Feed.</param>
        private void RaiseFeedDownloaded(IEnumerable<ODataEntity> odataEntities, Uri nextPageLink, long? totalCount, Exception readingError)
        {
            if (this.FeedDownloaded != null)
            {
                this.FeedDownloaded(this, new ODataFeedDownloadArgs(odataEntities, totalCount, nextPageLink, readingError, this.requestCancelled, this.requestTimedOut));
            }
        }
    }
}
