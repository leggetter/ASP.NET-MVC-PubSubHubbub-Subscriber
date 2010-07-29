using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HubSubscriber.Kwwika;

namespace HubSubscriber.Services
{
    public interface IHubSubscriptionService
    {
        SubscriptionServiceResult Subscribe(IHubConfiguration configuration, SubscriptionModel model);

        SubscriptionServiceResult UnSubscribe(IHubConfiguration configuration, SubscriptionModel model);
    }
}
