using System.Configuration;
using System;

namespace HubSubscriber.Kwwika
{
    internal static class Config
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
    }
}