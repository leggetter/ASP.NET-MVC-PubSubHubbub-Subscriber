using HubSubscriber.Models;
using System.Collections.Generic;

namespace HubSubscriber.Services
{
    public interface IHubSubscriptionPersistenceService
    {
        void StoreSubscription(SubscriptionModel model);

        IEnumerable<SubscriptionModel> GetSubscriptionsList(string username);

        void DeleteSubscriptionById(int id);

        int GetSubscriptionCountById(int id);

        SubscriptionModel GetSubscriptionById(int id);

        void SaveChanges(SubscriptionModel sub);

        int GetMaxSubscriptionsForUser(string user);

        int GetSubscriptionCountForUser(string user);

        UserModel GetUser(string username);
    }
}
