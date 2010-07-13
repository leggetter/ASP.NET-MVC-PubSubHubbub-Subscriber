using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace HubSubscriber.Kwwika
{
    public class SubscriptionModel
    {
        public SubscriptionModel()
        {
        }

        [Key]
        public int Id
        {
            get;
            set;
        }

        [Required]
        [DefaultValue("subscribe")]
        public string Mode = "subscribe";

        [Required]
        [DefaultValue("sync")]
        public string Verify = "sync";

        public string Callback
        {
            get;
            set;
        }

        [Required]
        [DataType(DataType.Url)]
        public string Topic
        {

            get;
            set;
        }

        [Required]
        public bool Digest = false;

        public bool PendingDeletion { get; set; }

        public bool Verified { get; set; }

        public DateTime? LastUpdated { get; set; }
    }
}