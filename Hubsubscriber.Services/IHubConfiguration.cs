using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HubSubscriber.Services
{
    public interface IHubConfiguration
    {        
        string HubUsername { get; }
        string HubPassword { get; }
        short HubMaximumSubscriptions { get; }
        Uri HubRoot { get; }
    }
}
