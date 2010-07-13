using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HubSubscriber.Services;
using Kwwika.QueueComponents;
using Kwwika.Common.Logging;
using System.Xml;
using System.Text.RegularExpressions;

namespace HubSubscriber.Kwwika
{
    public class HubSubscriptionListener : IHubSubscriptionListener
    {
        private MessageQueueWriter _queueWriter;
        private ILoggingService _loggingService;

        public HubSubscriptionListener(ILoggingService loggingService)
        {
            _loggingService = loggingService;
            _queueWriter = new MessageQueueWriter(Config.MessageWriterQueue, _loggingService);
        }

        #region IHubSubscriptionListener Members

        public void SubscriptionUpdateReceived(string update)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(update);
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(doc.NameTable);
            nsMgr.AddNamespace("atom", "http://www.w3.org/2005/Atom");
            var extractor = new XPathValueExtractorService(doc, nsMgr, _loggingService);

            string feedUpdated = extractor.TryGetValue("//atom:feed/atom:updated/text()");
            string entryPublished = extractor.TryGetValue("//atom:feed/atom:entry/atom:published/text()");
            string entryId = extractor.TryGetValue("//atom:feed/atom:entry/atom:id/text()");
            string entryTitle = extractor.TryGetValue("//atom:feed/atom:entry/atom:title/text()");
            string entryContent = extractor.TryGetValue("//atom:feed/atom:entry/atom:content/text()");
            entryContent = Regex.Replace(entryContent, @"((<[\s\/]*script\b[^>]*>)([^>]*)(<\/script>))", "", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (entryContent.Length > 25000)
            {
                entryContent = entryContent.Substring(0, Config.TrimEntryContentLength);

                _loggingService.Warn("Trimming message content for \"" + entryTitle + "\" due to large content size");
            }

            string entryAuthors = extractor.TryGetList("//atom:feed/atom:entry/atom:author/atom:name/text()");
            string entryLinkAlt = extractor.TryGetValue("//atom:feed/atom:entry/atom:link[@rel='alternate']/@href");
            string entryLinkReplies = extractor.TryGetValue("//atom:feed/atom:entry/atom:link[@rel='replies']/@href");


            var publishMessage = new PublishMessage("/KWWIKA/SANDBOX");
            publishMessage.Values.Add("feedUpdated", feedUpdated);
            publishMessage.Values.Add("entryPublished", entryPublished);
            publishMessage.Values.Add("entryId", entryId);
            publishMessage.Values.Add("entryTitle", entryTitle);
            publishMessage.Values.Add("entryContent", entryContent);
            publishMessage.Values.Add("entryAuthors", entryAuthors);
            publishMessage.Values.Add("entryLinkAlternate", entryLinkAlt);
            publishMessage.Values.Add("entryLinkReplies", entryLinkReplies);

            _queueWriter.Write(publishMessage);
        }

        #endregion
    }
}
