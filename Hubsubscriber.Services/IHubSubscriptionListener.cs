using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HubSubscriber.Services
{
    public interface IHubSubscriptionListener
    {
        void SubscriptionUpdateReceived(string update);
    }
}
