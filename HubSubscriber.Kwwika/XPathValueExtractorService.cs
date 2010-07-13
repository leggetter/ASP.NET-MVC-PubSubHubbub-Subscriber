using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kwwika.Common.Logging;
using System.Xml;

namespace HubSubscriber.Kwwika
{
    public class XPathValueExtractorService
    {
        private System.Xml.XmlNamespaceManager nsMgr;
        private System.Xml.XmlDocument doc;
        ILoggingService _loggingService;

        public XPathValueExtractorService(System.Xml.XmlDocument doc, System.Xml.XmlNamespaceManager nsMgr, ILoggingService loggingService)
        {
            this.doc = doc;
            this.nsMgr = nsMgr;
            _loggingService = loggingService;
        }

        public string TryGetList(string xpath)
        {
            List<string> list = new List<string>();
            try
            {
                XmlNodeList nodeList = doc.SelectNodes(xpath, nsMgr);
                foreach (XmlNode node in nodeList)
                {
                    list.Add(node.Value);
                }
            }
            catch (Exception ex)
            {
                _loggingService.Warn("Could not get value for " + xpath + ": " + ex);
            }

            _loggingService.Trace("XPath: " + xpath + " returned nodelist count: \"" + list.Count + "\"");

            string[] items = list.ToArray();
            return (items.Length > 0 ? String.Join(",", items) : " ");
        }

        public string TryGetValue(string xpath)
        {
            string value = " ";
            try
            {
                XmlNode node = doc.SelectSingleNode(xpath, nsMgr);
                value = node != null ? node.Value : "";
            }
            catch (Exception ex)
            {
                _loggingService.Warn("Could not get value for " + xpath + ": " + ex);
            }

            _loggingService.Trace("XPath: " + xpath + " returned: \"" + value + "\"");

            return value;
        }
    }
}