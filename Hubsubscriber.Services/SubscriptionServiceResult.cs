using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HubSubscriber.Services
{
    public class SubscriptionServiceResult
    {
        public SubscriptionResponseResultType Type { get; set; }

        public string ErrorDescription { get; set; }
    }
}
