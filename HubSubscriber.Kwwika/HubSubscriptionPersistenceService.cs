using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HubSubscriber.Services;
using HubSubscriber.Models;
using Kwwika.Common.Logging;

namespace HubSubscriber.Kwwika
{
    public class HubSubscriptionPersistenceService : IHubSubscriptionPersistenceService
    {
        private readonly object _dbAccessLock = new object();
        private ILoggingService _loggingService;
        private HubSubscriberEntities _entities;

        public HubSubscriptionPersistenceService(ILoggingService loggingService)
        {
            _loggingService = loggingService;
            _entities = new HubSubscriberEntities();
        }

        #region IHubSubscriptionPersistenceService Members

        public void StoreSubscription(SubscriptionModel model)
        {
            if (GetUser(model.PubSubHubUser) == null)
            {
                throw new InvalidOperationException("User does not exist.");
            }

            int maxSubsForUser = GetMaxSubscriptionsForUser(model.PubSubHubUser);
            int userSubCount = GetSubscriptionCountForUser(model.PubSubHubUser);

            if (userSubCount >= maxSubsForUser)
            {
                throw new InvalidOperationException("Storing this subscription would exceed the users subscription limit");
            }

            lock (_dbAccessLock)
            {
                Subscription newSubscription = _entities.CreateObject<Subscription>();
                newSubscription.Callback = model.Callback;
                newSubscription.Digest = model.Digest;
                newSubscription.Mode = model.Mode;
                newSubscription.Verify = model.Verify;
                newSubscription.Topic = model.Topic;
                newSubscription.LastUpdated = DateTime.Now;
                newSubscription.PubSubHubUser = model.PubSubHubUser;

                _loggingService.Info("Saving model to the database " + model);

                _entities.AddToSubscriptionsSet(newSubscription);
                _entities.SaveChanges();

                model.Id = newSubscription.Id;
            }
        }

        public IEnumerable<SubscriptionModel> GetSubscriptionsList(string username)
        {
            IList<SubscriptionModel> subscriptionModels = new List<SubscriptionModel>();
            lock (_dbAccessLock)
            {
                var subscriptions = from sub in _entities.SubscriptionsSet
                                    where sub.PubSubHubUser == username
                                    select sub;

                foreach (Subscription sub in subscriptions)
                {
                    subscriptionModels.Add(CreateSubscriptionModelFromSubscription(sub));
                }
            }
            return subscriptionModels;
        }

        public void DeleteSubscriptionById(int id)
        {
            lock (_dbAccessLock)
            {
                Subscription sub = _entities.SubscriptionsSet.First(m => m.Id == id);
                _entities.SubscriptionsSet.DeleteObject(sub);
                _entities.SaveChanges();
            }
        }

        public int GetSubscriptionCountById(int id)
        {
            lock (_dbAccessLock)
            {
                int count = _entities.SubscriptionsSet.Count<Subscription>(m => m.Id == id);
                return count;
            }
        }

        public SubscriptionModel GetSubscriptionById(int id)
        {
            lock (_dbAccessLock)
            {
                Subscription sub = _entities.SubscriptionsSet.First(m => m.Id == id);
                SubscriptionModel model = CreateSubscriptionModelFromSubscription(sub);
                return model;
            }
        }

        public void SaveChanges(SubscriptionModel model)
        {
            lock (_dbAccessLock)
            {
                Subscription sub = _entities.SubscriptionsSet.First(m => m.Id == model.Id);

                sub.Callback = model.Callback;
                sub.Digest = model.Digest;
                sub.LastUpdated = model.LastUpdated;
                sub.Mode = model.Mode;
                sub.PendingDeletion = model.PendingDeletion;
                sub.Topic = model.Topic;
                sub.Verified = model.Verified;
                sub.Verify = model.Verify;
                sub.PubSubHubUser = model.PubSubHubUser;

                _entities.SaveChanges();
            }
        }

        public int GetMaxSubscriptionsForUser(string user)
        {
            lock (_dbAccessLock)
            {
                return _entities.UsersSet.First(u => u.PubSubHubUser == user).MaxHubSubscriptions;
            }
        }

        public int GetSubscriptionCountForUser(string user)
        {
            lock (_dbAccessLock)
            {
                return _entities.SubscriptionsSet.Count(subs => subs.PubSubHubUser == user);
            }
        }

        public UserModel GetUser(string username)
        {
            lock (_dbAccessLock)
            {
                UserModel model = null;
                User user =  _entities.UsersSet.First(u => u.PubSubHubUser == username);
                if (user != null)
                {
                    model = new UserModel()
                    {
                        Username = user.PubSubHubUser,
                        PushTopic = user.KwwikaTopic,
                        MaxHubSubscriptions = user.MaxHubSubscriptions
                    };
                }
                return model;
            }
        }
        #endregion

        private SubscriptionModel CreateSubscriptionModelFromSubscription(Subscription sub)
        {
            var model = new SubscriptionModel()
            {
                Id = sub.Id,
                Callback = sub.Callback,
                Mode = sub.Mode,
                Digest = sub.Digest,
                Topic = sub.Topic,
                Verify = sub.Verify,
                PendingDeletion = sub.PendingDeletion,
                Verified = sub.Verified,
                LastUpdated = sub.LastUpdated,
                PubSubHubUser = sub.PubSubHubUser
            };

            return model;
        }
    }
}