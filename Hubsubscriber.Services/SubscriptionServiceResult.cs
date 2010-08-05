using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HubSubscriber.Models;

namespace HubSubscriber.Services
{
    public class SubscriptionServiceResult
    {
        public SubscriptionResponseResultType Type { get; set; }

        public string ErrorDescription { get; set; }

        public SubscriptionModel Subscription { get; set; }
    }
}
