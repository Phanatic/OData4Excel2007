namespace ODataFeedClientTests
{
    using System;
    using System.Linq;
    using System.Threading;
    using Microsoft.Data.OData;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ODataFeedClient;
    using ODataFeedClient.Objects;

    /// <summary>
    ///This is a test class for ConnectedODataFeedClientTest and is intended
    ///to contain all ConnectedODataFeedClientTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ConnectedODataFeedClientTest
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for BeginDownload
        ///</summary>
        [TestMethod()]
        public void DownloadTest()
        {
            IODataFeedClient target = new ConnectedODataFeedClient();
            string requestUri = "http://odata.netflix.com/v2/Catalog/Titles?$top=1";
            AutoResetEvent resetEvent = new AutoResetEvent(false);
            ODataFeedDownloadArgs eventArgs = null;
            target.FeedDownloaded += (sender, args) =>
            {
                eventArgs = args;
                Assert.IsNull(args.Error, "No error expected");
                Assert.IsFalse(args.IsCancelled, "No error expected");
                Assert.IsNull(args.TotalCount, "No count expected");

                Assert.AreEqual(args.Entries.Count(), 1, "Only one entity expected");
            };

            IAsyncResult expected = target.BeginDownload(requestUri);
            expected.AsyncWaitHandle.WaitOne();
        }

        /// <summary>
        ///A test for BeginDownload
        ///</summary>
        [TestMethod()]
        public void TimeoutOnDownloadTests()
        {
            IODataFeedClient target = new ConnectedODataFeedClient();
            target.Timeout = 0.5 * 1000;
            string requestUri = "http://odata.netflix.com/v2/Catalog/Titles?$top=400&$expand=Genres";
            AutoResetEvent resetEvent = new AutoResetEvent(false);
            ODataFeedDownloadArgs eventArgs = null;
            target.FeedDownloaded += (sender, args) =>
            {
                eventArgs = args;
                Assert.IsTrue(args.IsTimedOut, "Timeout expected");
                Assert.IsNull(args.Error, "No error expected");
                Assert.IsNull(args.TotalCount, "No count expected");
            };

            IAsyncResult expected = target.BeginDownload(requestUri);
            expected.AsyncWaitHandle.WaitOne();
        }

        /// <summary>
        ///A test for BeginDownload
        ///</summary>
        [TestMethod()]
        public void CancelDownloadTest()
        {
            IODataFeedClient target = new ConnectedODataFeedClient();
            string requestUri = "http://odata.netflix.com/v2/Catalog/Titles?$top=100";
            AutoResetEvent resetEvent = new AutoResetEvent(false);

            target.FeedDownloaded += (sender, args) =>
            {
                try
                {
                    Assert.IsTrue(args.IsCancelled, "Expected Request to be cancelled");
                    Assert.IsNull(args.Error, "No error expected");
                    Assert.IsNull(args.TotalCount, "No count expected");
                }
                finally
                {
                    resetEvent.Set();
                }
            };

            IAsyncResult expected = target.BeginDownload(requestUri);
            target.CancelRequest();
            resetEvent.WaitOne();
        }

        /// <summary>
        ///A test for BeginDownload
        ///</summary>
        [TestMethod()]
        public void DownloadSingleEntry_ExpectFailure_Test()
        {
            IODataFeedClient target = new ConnectedODataFeedClient();
            string requestUri = "http://odata.netflix.com/v2/Catalog/Titles('1EGty')?$select=BoxArt,Genres/*&$expand=Genres";
            AutoResetEvent resetEvent = new AutoResetEvent(false);

            target.FeedDownloaded += (sender, args) =>
            {
                try
                {
                    Assert.IsNotNull(args.Error, "error expected");
                    Assert.IsInstanceOfType(args.Error, typeof(ODataException), "ODataException expected");
                    Assert.IsNull(args.Entries, "no entities expected");
                }
                finally
                {
                    resetEvent.Set();
                }
            };

            IAsyncResult expected = target.BeginDownload(requestUri);
            resetEvent.WaitOne();
        }

        /// <summary>
        ///A test for BeginDownload
        ///</summary>
        [TestMethod()]
        public void FlattenComplexTypes()
        {
            IODataFeedClient target = new ConnectedODataFeedClient();
            string requestUri = "http://odata.netflix.com/v2/Catalog/Titles?$top=1&$select=BoxArt";
            AutoResetEvent resetEvent = new AutoResetEvent(false);

            target.FeedDownloaded += (sender, args) =>
            {
                try
                {
                    Assert.IsNull(args.Error, "No error expected");
                    Assert.IsFalse(args.IsCancelled, "No error expected");
                    Assert.IsNull(args.TotalCount, "No count expected");

                    Assert.AreEqual(args.Entries.Count(), 1, "Only one entity expected");
                    ODataEntity entity = args.Entries.Single();
                    foreach (var propertyName in new[] { "BoxArt.SmallUrl", "BoxArt.MediumUrl", "BoxArt.LargeUrl", "BoxArt.HighDefinitionUrl" })
                    {
                        Assert.IsTrue(entity.Properties.ContainsKey(propertyName), "{0} property not flattened", propertyName);
                    }
                }
                finally
                {
                    resetEvent.Set();
                }
            };

            IAsyncResult expected = target.BeginDownload(requestUri);
            resetEvent.WaitOne();
        }
    }
}
