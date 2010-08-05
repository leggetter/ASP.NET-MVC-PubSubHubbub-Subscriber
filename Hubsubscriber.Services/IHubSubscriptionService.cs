using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HubSubscriber.Models;

namespace HubSubscriber.Services
{
    public interface IHubSubscriptionService
    {
        SubscriptionServiceResult Subscribe(IHubConfiguration configuration, SubscriptionModel model);

        SubscriptionServiceResult UnSubscribe(IHubConfiguration configuration, SubscriptionModel model);
    }
}
