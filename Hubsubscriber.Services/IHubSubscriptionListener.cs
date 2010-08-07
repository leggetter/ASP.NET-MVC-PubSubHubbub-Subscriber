using HubSubscriber.Models;

namespace HubSubscriber.Services
{
    public interface IHubSubscriptionListener
    {
        void SubscriptionUpdateReceived(SubscriptionModel subscriptionModel, string update);
    }
}
