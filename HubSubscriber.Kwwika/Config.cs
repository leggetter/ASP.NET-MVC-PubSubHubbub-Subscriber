using System.Configuration;
using System;

namespace HubSubscriber.Kwwika
{
    public static class Config
    {
        public static int TrimEntryContentLength
        {
            get
            {
                int trimLengthTo = 25000;
                string value = ConfigurationManager.AppSettings.Get("TrimEntryContentLength");
                int convertedValue = 0;
                if (int.TryParse(value, out convertedValue))
                {
                    trimLengthTo = convertedValue;
                }
                return trimLengthTo;
            }
        }

        public static string MessageWriterQueue
        {
            get
            {
                return @".\Private$\KwwikaPublishQueue";
            }
        }

        public static string HubUsername
        {
            get
            {
                string value = ConfigurationManager.AppSettings.Get("HubUsername");
                return value;
            }
        }

        public static object HubPassword
        {
            get
            {
                string value = ConfigurationManager.AppSettings.Get("HubPassword");
                return value;
            }
        }

        public static string HubRootUrl
        {
            get
            {
                string value = ConfigurationManager.AppSettings.Get("HubRootUrl");
                return value;
            }
        }
    }
}