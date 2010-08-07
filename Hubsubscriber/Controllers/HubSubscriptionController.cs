using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Mvc;
using HubSubscriber.Models;
using HubSubscriber.Services;
using Kwwika.Common.Logging;

namespace HubSubscriber.Controllers
{
    public class HubSubscriptionController : ApplicationController
    {
        private const int LOGIN_TEST_ID = 0;
        private const string LOGIN_TEST_TOPIC = "http://superfeedr.com/dummy.xml";

        private ILoggingService _loggingService;        
        private IHubSubscriptionListener _hubSubscriptionListener;
        private IHubSubscriptionPersistenceService _subscriptionPersistenceService;
        private IHubSubscriptionService _subscriptionService;
        private HubConfiguration _hubConfiguration;

        #region Constructors
        public HubSubscriptionController():
            this(MvcApplication.LoggingService)
        {
        }

        public HubSubscriptionController(ILoggingService loggingService)
        {
            _hubConfiguration = new HubConfiguration()
            {
                HubMaximumSubscriptions = 10,
                HubUsername = Config.HubUsername,
                HubPassword = Config.HubPassword,
                HubRoot = new Uri(Config.HubRootUrl)
            };

            _loggingService = loggingService;
            _hubSubscriptionListener = MvcApplication.Container.GetService<IHubSubscriptionListener>();
            _subscriptionPersistenceService = MvcApplication.Container.GetService<IHubSubscriptionPersistenceService>();
            _subscriptionService = MvcApplication.Container.GetService<IHubSubscriptionService>();
        }
        #endregion

        #region Properties
        internal ILoggingService LoggingService
        {
            get
            {
                return _loggingService;
            }
        }

        internal IHubConfiguration HubConfiguration
        {
            get
            {
                if (SubscriptionUser.IsLoggedIn)
                {
                    _hubConfiguration.HubUsername = SubscriptionUser.Username;
                    _hubConfiguration.HubPassword = SubscriptionUser.Password;
                    _hubConfiguration.HubMaximumSubscriptions = SubscriptionUser.MaxHubSubscriptions;
                }

                return _hubConfiguration;
            }
        }

        internal IHubSubscriptionListener HubSubscriptionListener
        {
            get
            {
                return _hubSubscriptionListener;
            }
        }

        internal IHubSubscriptionPersistenceService HubSubscriptionPersistenceService
        {
            get
            {
                return _subscriptionPersistenceService;
            }
        }

        internal IHubSubscriptionService HubSubscriptionService
        {
            get
            {
                return _subscriptionService;
            }
        }
        #endregion

        #region Application invoked methods

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Login(LoginModel loginModel)
        {
            ViewData.Model = loginModel;
            if (ModelState.IsValid)
            {
                SubscriptionModel modelForLogin = CreateSubscriptionModelForTestLogin();
                HubConfiguration testLoginHubConfig = CreateHubConfigurationForTestLogin(loginModel);
                UserModel userModel = _subscriptionPersistenceService.GetUser(testLoginHubConfig.HubUsername);

                if (userModel == null)
                {
                    string msg = "User \"" + testLoginHubConfig.HubUsername + "\" does not exist.";
                    ViewData.ModelState.AddModelError("_FORM", msg);
                }
                else
                {
                    var result = _subscriptionService.Subscribe(testLoginHubConfig, modelForLogin);

                    if (result.Type != SubscriptionResponseResultType.NotAuthorised)
                    {
                        modelForLogin.Mode = "unsubscribe";
                        _subscriptionService.UnSubscribe(testLoginHubConfig, modelForLogin);

                        userModel.Password = loginModel.Password;
                        userModel.IsLoggedIn = true;

                        SubscriptionUser = userModel;
                    }
                    else
                    {
                        ViewData.ModelState.AddModelError("_FORM", result.ErrorDescription);
                    }
                }
            }

            return Json(CreateUserInfo(SubscriptionUser));
        }

        public ActionResult Logout()
        {
            SubscriptionUser = CreateDefaultUserModel();
            return Json(CreateUserInfo(SubscriptionUser));       
        }

        //
        // GET: /HubSubscription/

        public ActionResult List()
        {
            var subscriptions = (IEnumerable<SubscriptionModel>)_subscriptionPersistenceService.GetSubscriptionsList(HubConfiguration.HubUsername);

            return Json(subscriptions);
        }

        //
        // POST: /HubSubscription/Create

        [HttpPost]
        public ActionResult Create([Bind(Exclude = "Id")] SubscriptionModel model)
        {
            JsonResult jsonResult = null;
            try
            {
                model.Callback = model.Callback ?? Request.Url.GetLeftPart(UriPartial.Authority) + GetAppPath() + Url.Action("HubUpdate", "HubSubscription");
                model.Mode = model.Mode ?? "subscribe";
                model.Verify = model.Verify ?? "sync";
                model.PubSubHubUser = HubConfiguration.HubUsername;

                _loggingService.Info("Creating subscription for " + model + "\nModel valid: " + ViewData.ModelState.IsValid);

                ViewData.Model = model;
                if (!ViewData.ModelState.IsValid)
                {
                    jsonResult = Json(new SubscriptionServiceResult()
                    {
                        Type = SubscriptionResponseResultType.Error,
                        ErrorDescription = "Model is not valid"
                    });
                }
                else
                {
                    int maxSubsForUser = _subscriptionPersistenceService.GetMaxSubscriptionsForUser(model.PubSubHubUser);
                    int userSubCount = _subscriptionPersistenceService.GetSubscriptionCountForUser(model.PubSubHubUser);

                    if (userSubCount >= maxSubsForUser)
                    {
                        string msg = 
                            string.Format("Maximum number of subscriptions reaced for user. Subscriptions in use {0}. Maximum subscriptions {1}.",
                                userSubCount, maxSubsForUser);
                        
                        jsonResult = Json(new SubscriptionServiceResult()
                        {
                            Type = SubscriptionResponseResultType.Error,
                            ErrorDescription = msg
                        });
                    }
                    else
                    {
                        _subscriptionPersistenceService.StoreSubscription(model);

                        SubscriptionServiceResult result = _subscriptionService.Subscribe(HubConfiguration, model);

                        jsonResult = Json(result);
                    }
                }
            }
            catch(Exception ex)
            {
                string msg = "An exception occurred in Create method: " + ex.ToString();
                _loggingService.Error(msg);
                
                jsonResult = Json(new SubscriptionServiceResult()
                {
                    Type = SubscriptionResponseResultType.Error,
                    ErrorDescription = msg
                });
            }

            return jsonResult;
        }

        //
        // GET: /HubSubscription/Delete/5
 
        public ActionResult Delete(int id)
        {
            JsonResult jsonResult = null;

            _loggingService.Info("Deleting subscription Id: " + id);

            try
            {                
                SubscriptionModel model = _subscriptionPersistenceService.GetSubscriptionById(id);
                model.PendingDeletion = true;
                model.LastUpdated = DateTime.Now;
                model.Mode = "unsubscribe";
                _subscriptionPersistenceService.SaveChanges(model);

                SubscriptionServiceResult result = _subscriptionService.UnSubscribe(HubConfiguration, model);

                if (result.Type == SubscriptionResponseResultType.NotFound)
                {
                    _subscriptionPersistenceService.DeleteSubscriptionById(id);

                    string msg = "The subscription could not be found in the subscription service. Deleted anyway. " +
                        result.ErrorDescription;
                    _loggingService.Error(msg);
                }
                jsonResult = Json(result);
            }            
            catch (Exception ex)
            {
                string msg = "An exception in Delete method: " + ex.ToString();
                _loggingService.Error(msg);

                jsonResult = Json(new SubscriptionServiceResult()
                {
                    ErrorDescription = msg,
                    Type = SubscriptionResponseResultType.Error
                });
            }

            return jsonResult;
        }
        #endregion

        #region Hub callback methods

        public ActionResult HubLoginTest()
        {
            ViewData["hub.challenge"] = Request["hub.challenge"];
            return View("EchoChallenge");
        }

        public ActionResult HubUpdate(int id)
        {
            _loggingService.Info("Update received for subscription Id: " + id);
            ActionResult view = View();

            try
            {
                if (Request["hub.mode"] == "unsubscribe")
                {
                    _loggingService.Info("Update received with hub.mode=\"unsubscribe\" for subscription id: " + id);
                    view = HubDelete(id);
                }
                else if (_subscriptionPersistenceService.GetSubscriptionCountById(id) != 1)
                {
                    _loggingService.Error("Error finding subscription for id: " + id + ". Ignoring.");
                    Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
                else
                {
                    SubscriptionModel sub = _subscriptionPersistenceService.GetSubscriptionById(id);
                    if (sub.Verified == false)
                    {
                        _loggingService.Info("Verifying subscription with Id: " + id);
                        sub.Verified = true;

                        ViewData["hub.challenge"] = Request["hub.challenge"];
                        Response.StatusCode = (int)HttpStatusCode.OK;
                    }
                    else
                    {
                        _loggingService.Info("Received subscription update for ID: " + id);

                        if (string.IsNullOrEmpty(sub.PushTopic) == true)
                        {
                            // Use the default user push topic
                            UserModel model = _subscriptionPersistenceService.GetUser(sub.PubSubHubUser);
                            sub.PushTopic = model.PushTopic;
                        }
                        string documentContents = GetDocumentContents(Request);
                        _hubSubscriptionListener.SubscriptionUpdateReceived(sub, documentContents);
                        
                        Response.StatusCode = (int)HttpStatusCode.OK;
                        view = View();
                    }
                    sub.LastUpdated = DateTime.Now;

                    _subscriptionPersistenceService.SaveChanges(sub);

                    view = View("EchoChallenge");
                }
            }
            catch (Exception ex)
            {
                string msg = "An exception in Update method: " + ex.ToString();
                _loggingService.Error(msg);

                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

            return view;
        }

        private ActionResult HubDelete(int id)
        {
             _loggingService.Info("HubDelete of subscription Id: " + id);

             try
             {
                 if (_subscriptionPersistenceService.GetSubscriptionCountById(id) != 1)
                 {
                     _loggingService.Error("Error finding subscription for id: " + id + ". Simply echoing challenge to confirm the deletion");
                 }
                 else
                 {
                     _subscriptionPersistenceService.DeleteSubscriptionById(id);

                     _loggingService.Info("Deleted subscription Id: " + id);

                     string updateContent = GetUpdateContent(Request);
                     _loggingService.Trace("Content received:" + Environment.NewLine + updateContent);
                     Response.StatusCode = (int)HttpStatusCode.OK;
                 }
                 ViewData["hub.challenge"] = Request["hub.challenge"];                 
             }
             catch (Exception ex)
             {
                 string msg = "An exception in ConfirmDelete method: " + ex.ToString();
                 _loggingService.Error(msg);

                 Response.StatusCode = (int)HttpStatusCode.InternalServerError;
             }

             _loggingService.Trace("Loading EchoChallange view with Status: " + Response.StatusDescription + " Challange: " + ViewData["hub.challenge"]);

             return View("EchoChallenge");
        }
        #endregion

        #region Helper Methods

        internal string GetAppPath()
        {
            string appPath = Url.Content("~");
            if (appPath.Length > 0)
            {
                appPath = appPath.Substring(1);
            }
            return appPath;
        }

        private Controllers.HubConfiguration CreateHubConfigurationForTestLogin(LoginModel loginModel)
        {
            return new HubConfiguration()
            {
                HubMaximumSubscriptions = 10,
                HubUsername = loginModel.Username,
                HubPassword = loginModel.Password,
                HubRoot = new Uri(Config.HubRootUrl)
            };
        }

        private SubscriptionModel CreateSubscriptionModelForTestLogin()
        {
            return new SubscriptionModel()
            {
                Id = LOGIN_TEST_ID,
                Topic = LOGIN_TEST_TOPIC,
                Callback = Request.Url.GetLeftPart(UriPartial.Authority) + GetAppPath() + Url.Action("HubLoginTest", "HubSubscription"),
                Mode = "subscribe",
                Verify = "sync"
            };
        }

        private string GetDocumentContents(System.Web.HttpRequestBase Request)
        {
            string documentContents;
            using (Stream receiveStream = Request.InputStream)
            {
                using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                {
                    documentContents = readStream.ReadToEnd();
                }
            }
            return documentContents;
        } 

        private string GetUpdateContent(System.Web.HttpRequestBase Request)
        {
            string updateContent = "Request parameters received:" + Environment.NewLine;
            foreach (string key in Request.Params.Keys)
            {
                updateContent += key + " = " + Request.Params[key] + Environment.NewLine;
            }

            using (Stream receiveStream = Request.InputStream)
            {
                using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                {
                    updateContent += "Text received: " + readStream.ReadToEnd() + Environment.NewLine;
                }
            }
            return updateContent;
        }

        #endregion
    }
}
