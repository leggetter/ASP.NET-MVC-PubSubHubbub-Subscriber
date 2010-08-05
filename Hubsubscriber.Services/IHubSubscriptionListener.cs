using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HubSubscriber.Models;

namespace HubSubscriber.Services
{
    public interface IHubSubscriptionListener
    {
        void SubscriptionUpdateReceived(UserModel userModel, string update);
    }
}
