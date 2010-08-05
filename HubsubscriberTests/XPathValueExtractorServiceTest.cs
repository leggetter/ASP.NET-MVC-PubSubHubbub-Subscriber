using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting.Web;
using System.Xml;
using Kwwika.Common.Logging;
using HubSubscriber.Kwwika;
using Rhino.Mocks;

namespace HubsubscriberTests
{
    
    
    /// <summary>
    ///This is a test class for XPathValueExtractorServiceTest and is intended
    ///to contain all XPathValueExtractorServiceTest Unit Tests
    ///</summary>
    [TestClass()]
    public class XPathValueExtractorServiceTest
    {


        private TestContext testContextInstance;
        private MockRepository _mocks;
        private ILoggingService _loggingService;

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
        [TestInitialize()]
        public void MyTestInitialize()
        {
            _mocks = new MockRepository();
            _loggingService = _mocks.Stub<ILoggingService>();
        }
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for XPathValueExtractorService Constructor
        ///</summary>
        // TODO: Ensure that the UrlToTest attribute specifies a URL to an ASP.NET page (for example,
        // http://.../Default.aspx). This is necessary for the unit test to be executed on the web server,
        // whether you are testing a page, web service, or a WCF service.
        [TestMethod()]
        public void XPathValueExtractorServiceConstructorTest()
        {
            XmlDocument doc = new XmlDocument();

            doc.LoadXml("<?xml version=\"1.0\"?> " +
                        "<feed xmlns:geo=\"http://www.georss.org/georss\" xmlns=\"http://www.w3.org/2005/Atom\" xmlns:as=\"http://activitystrea.ms/spec/1.0/\" xmlns:sf=\"http://superfeedr.com/xmpp-pubsub-ext\">" +
                        "   <updated>2010-07-01T02:04:42+00:00</updated>" +
                        "   <id>http://superfeedr.com/track/real-time-push</id>" +
                        "   <title></title>" +
                        "   <link type=\"application/atom+xml\" rel=\"self\" href=\"http://superfeedr.com/track/real-time-push\"/>" +
                        "   <entry xml:lang=\"en-US\">" +
                        "       <id>tag:typepad.com,2003:post-6a00e551f19dba883301348085952c970c</id>" +
                        "       <published>2010-05-07T02:56:46+00:00</published>" +
                        "       <title>Polling across the Pond</title>" +
                        "       <content type=\"html\">Some content</content>" +
                        "       <link title=\"Polling across the Pond\" type=\"text/html\" rel=\"alternate\" href=\"http://dcdiary.typepad.com/home/2010/05/polling-across-the-pond.html\"/>" +
                        "       <link title=\"Polling across the Pond\" type=\"text/html\" rel=\"replies\" href=\"http://dcdiary.typepad.com/home/2010/05/polling-across-the-pond.html\"/>" +
                        "       <category term=\"Current Affairs\"/>" +
                        "       <author>" +
                        "           <name>Emma Shercliff</name>" +
                        "           <uri/>" +
                        "           <email/>" +
                        "       </author>" +
                        "   </entry>" +
                        "</feed>");
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(doc.NameTable);
            nsMgr.AddNamespace("atom", "http://www.w3.org/2005/Atom");
            var extractor = new XPathValueExtractorService(doc, nsMgr, _loggingService);

            string feedUpdated = extractor.TryGetValue("//atom:feed/atom:updated/text()");
            string entryPublished = extractor.TryGetValue("//atom:feed/atom:entry/atom:published/text()");
            string entryId = extractor.TryGetValue("//atom:feed/atom:entry/atom:id/text()");
            string entryTitle = extractor.TryGetValue("//atom:feed/atom:entry/atom:title/text()");
            string entryContent = extractor.TryGetValue("//atom:feed/atom:entry/atom:content/text()");
            string entryAuthors = extractor.TryGetList("//atom:feed/atom:entry/atom:author/atom:name/text()");
            string entryLinkAlt = extractor.TryGetValue("//atom:feed/atom:entry/atom:link[@rel='alternate']/@href");
            string entryLinkReplies = extractor.TryGetValue("//atom:feed/atom:entry/atom:link[@rel='replies']/@href");
        }

        //[TestMethod()]
        //public void TryGetListTest()
        //{
        //    XmlDocument doc = null; // TODO: Initialize to an appropriate value
        //    XmlNamespaceManager nsMgr = null; // TODO: Initialize to an appropriate value
        //    XPathValueExtractorService target = new XPathValueExtractorService(doc, nsMgr, _loggingService); // TODO: Initialize to an appropriate value
        //    string xpath = string.Empty; // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    actual = target.TryGetList(xpath);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        //[TestMethod()]
        //public void TryGetValueTest()
        //{
        //    XmlDocument doc = null; // TODO: Initialize to an appropriate value
        //    XmlNamespaceManager nsMgr = null; // TODO: Initialize to an appropriate value
        //    XPathValueExtractorService target = new XPathValueExtractorService(doc, nsMgr, _loggingService); // TODO: Initialize to an appropriate value
        //    string xpath = string.Empty; // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    actual = target.TryGetValue(xpath);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}
    }
}
