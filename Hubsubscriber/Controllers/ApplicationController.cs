using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HubSubscriber.Models;

namespace HubSubscriber.Controllers
{
    public abstract class ApplicationController : Controller
    {
        private UserModel _user = null;
        public ApplicationController()
        {           
        }

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            _user = (requestContext.HttpContext.Session["User"] != null ? (UserModel)requestContext.HttpContext.Session["User"] : CreateDefaultUserModel() );
            ViewData["User"] = _user;
            
            base.Initialize(requestContext);
        }

        protected UserModel CreateDefaultUserModel()
        {
            return new UserModel() { PushTopic = "/KWWIKA/SANDBOX" };
        }

        public UserModel SubscriptionUser
        {
            get
            {
                if (_user == null)
                {
                    _user = CreateDefaultUserModel();
                }
                return _user;
            }
            set
            {
                _user = value;
                Session["User"] = _user;
                ViewData["User"] = _user;
            }
        }

    }
}
