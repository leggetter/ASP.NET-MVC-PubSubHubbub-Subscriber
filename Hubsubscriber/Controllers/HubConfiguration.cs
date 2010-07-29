using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HubSubscriber.Services;

namespace HubSubscriber.Controllers
{
    class HubConfiguration: IHubConfiguration
    {
        #region IHubConfiguration Members

        public string HubUsername
        {
            get;
            set;
        }

        public string HubPassword
        {
            get;
            set;
        }

        public short HubMaximumSubscriptions
        {
            get;
            set;
        }

        public Uri HubRoot
        {
            get;
            set;
        }

        #endregion
    }
}
