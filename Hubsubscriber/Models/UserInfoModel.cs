using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HubSubscriber.Models
{
    public class UserInfoModel
    {
        public string Username { get; set; }
        public string Status { get; set; }
        public string PushTopic { get; set; }
    }
}