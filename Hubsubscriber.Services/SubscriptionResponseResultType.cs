using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HubSubscriber.Services
{
    public enum SubscriptionResponseResultType
    {
        NotAuthorised,
        Error,
        Success,
        NotFound
    }
}