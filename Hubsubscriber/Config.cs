using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace HubSubscriber
{
    public class Config
    {
        public static string HubUsername
        {
            get
            {
                string value = ConfigurationManager.AppSettings.Get("HubUsername");
                return value;
            }
        }

        public static string HubPassword
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