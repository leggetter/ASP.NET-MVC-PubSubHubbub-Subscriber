using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace HubSubscriber.Models
{
    public class UserModel: LoginModel
    {
        public string PushTopic { get; set; }

        public short MaxHubSubscriptions { get; set; }

        public bool IsLoggedIn { get; set; }
    }
}
