using HubSubscriber.Kwwika;
using System.Collections.Generic;

namespace HubSubscriber.Services
{
    public interface IHubSubscriptionPersistenceService
    {
        void StoreSubscription(SubscriptionModel model);

        IEnumerable<SubscriptionModel> GetSubscriptionsList();

        void MarkSubscriptionPendingDeletionById(int id);

        void DeleteSubscriptionById(int id);

        int GetSubscriptionCountById(int id);

        SubscriptionModel GetSubscriptionById(int id);

        void SaveChanges(SubscriptionModel sub);
    }
}
