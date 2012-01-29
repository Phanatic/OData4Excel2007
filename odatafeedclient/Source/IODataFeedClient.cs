// -----------------------------------------------------------------------
// <copyright file="IODataFeedClient.cs" company="Welf">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace ODataFeedClient
{
    using System;

    /// <summary>
    /// Contract to represent an OData feed downloader.
    /// </summary>
    public interface IODataFeedClient
    {
        /// <summary>
        /// This event is raised when the download completes.
        /// </summary>
        event EventHandler<ODataFeedDownloadArgs> FeedDownloaded;

        /// <summary>
        /// Begins downloading the OData feed located at <paramref name="requestUri"/>
        /// </summary>
        /// <param name="requestUri">Location of OData feed</param>
        /// <returns>An AsyncResult token that the client of this implementation can wait on</returns>
        IAsyncResult BeginDownload(string requestUri);

        /// <summary>
        /// Cancels the current running download request.
        /// </summary>
        void CancelRequest();
    }
}
