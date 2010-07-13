﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HubSubscriber.Services;
using HubSubscriber.Kwwika;
using Kwwika.Common.Logging;

namespace HubSubscriber.Kwwika
{
    public class HubSubscriptionPersistenceService: IHubSubscriptionPersistenceService
    {
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
            Subscription newSubscription = _entities.CreateObject<Subscription>();
            newSubscription.Callback = model.Callback;
            newSubscription.Digest = model.Digest;
            newSubscription.Mode = model.Mode;
            newSubscription.Verify = model.Verify;
            newSubscription.Topic = model.Topic;
            newSubscription.LastUpdated = DateTime.Now;

            _loggingService.Info("Saving model to the database " + model);

            _entities.AddToSubscriptionsSet(newSubscription);
            _entities.SaveChanges();

            model.Id = newSubscription.Id;
        }

        public IEnumerable<SubscriptionModel> GetSubscriptionsList()
        {
            IList<SubscriptionModel> subscriptionModels = new List<SubscriptionModel>();
            foreach (Subscription sub in _entities.SubscriptionsSet.ToList())
            {
                subscriptionModels.Add(CreateSubscriptionModelFromSubscription(sub));
            }
            return subscriptionModels;
        }

        public void MarkSubscriptionPendingDeletionById(int id)
        {
            Subscription sub = _entities.SubscriptionsSet.First(m => m.Id == id);
            sub.PendingDeletion = true;
            sub.LastUpdated = DateTime.Now;
            _entities.SaveChanges();
        }

        public void DeleteSubscriptionById(int id)
        {
            Subscription sub = _entities.SubscriptionsSet.First(m => m.Id == id);
            _entities.SubscriptionsSet.DeleteObject(sub);
            _entities.SaveChanges();
        }

        public int GetSubscriptionCountById(int id)
        {
            return _entities.SubscriptionsSet.Count<Subscription>(m => m.Id == id);
        }

        public SubscriptionModel GetSubscriptionById(int id)
        {
            Subscription sub = _entities.SubscriptionsSet.First(m => m.Id == id);
            SubscriptionModel model = CreateSubscriptionModelFromSubscription(sub);
            return model;
        }

        public void SaveChanges(SubscriptionModel model)
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

            _entities.SaveChanges();
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
                        LastUpdated = sub.LastUpdated
                    };

            return model;
        }
    }
}