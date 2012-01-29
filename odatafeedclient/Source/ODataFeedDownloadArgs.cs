// -----------------------------------------------------------------------
// <copyright file="ODataFeedDownloadArgs.cs" company="Self">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace ODataFeedClient
{
    using System;
    using System.Collections.Generic;
    using ODataFeedClient.Objects;

    /// <summary>
    /// Feed Download event args to carry information about feed download completing.
    /// </summary>
    public class ODataFeedDownloadArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the ODataFeedDownloadArgs class.
        /// </summary>
        /// <param name="entries">Entries downloaded</param>
        /// <param name="totalCount">Total count of entities downloaded</param>
        /// <param name="nextLinkUri">Next Link Uri to next page of results.</param>
        /// <param name="error">Any erorr which occured during download of OData payload.</param>
        /// <param name="isCancelled">A boolean flag indicating whether the request was cancelled by user.</param>
        /// <param name="isTimedOut">A boolean flag indicating whether the request timed out.</param>
        public ODataFeedDownloadArgs(IEnumerable<ODataEntity> entries, long? totalCount, Uri nextLinkUri, Exception error, bool isCancelled, bool isTimedOut)
        {
            this.Entries = entries;
            this.TotalCount = totalCount;
            this.NextLinkUri = nextLinkUri;
            this.Error = error;
            this.IsCancelled = isCancelled;
            this.IsTimedOut = isTimedOut;
        }

        /// <summary>
        /// Gets the Entries downloaded
        /// </summary>
        public IEnumerable<ODataEntity> Entries { get; private set; }

        /// <summary>
        /// Gets Total count of entities downloaded
        /// </summary>
        public long? TotalCount { get; private set; }

        /// <summary>
        /// Gets NextLink Uri to next page of results.
        /// </summary>
        public Uri NextLinkUri { get; private set; }

        /// <summary>
        /// Gets any erorr which occured during download of OData payload.
        /// </summary>
        public Exception Error { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the request was cancelled by user.
        /// </summary>
        public bool IsCancelled { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the request was timed out.
        /// </summary>
        public bool IsTimedOut { get; private set; }
    }
}
