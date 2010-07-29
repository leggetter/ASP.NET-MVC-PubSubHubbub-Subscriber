using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Mvc;
using HubSubscriber.Kwwika;
using HubSubscriber.Services;
using Kwwika.Common.Logging;

namespace HubSubscriber.Controllers
{
    public class HubSubscriptionController : Controller
    {
        private ILoggingService _loggingService;        
        private IHubSubscriptionListener _hubSubscriptionListener;
        private IHubSubscriptionPersistenceService _subscriptionPersistenceService;
        private IHubSubscriptionService _subscriptionService;
        private HubConfiguration _hubConfiguration;

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

        //
        // GET: /HubSubscription/

        public ActionResult Index()
        {
            ViewData.Model = (IEnumerable<SubscriptionModel>)_subscriptionPersistenceService.GetSubscriptionsList();

            return View();
        }

        //
        // GET: /HubSubscription/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /HubSubscription/Create

        [HttpPost]
        public ActionResult Create([Bind(Exclude = "Id")] SubscriptionModel model)
        {
            ActionResult view = View();
            try
            {
                string appPath = Url.Content("~");
                if(appPath.Length > 0)
                {
                    appPath = appPath.Substring(1);
                }
                model.Callback = model.Callback ?? Request.Url.GetLeftPart(UriPartial.Authority) + appPath + Url.Action("HubUpdate", "HubSubscription");
                model.Mode = model.Mode ?? "subscribe";
                model.Verify = model.Verify ?? "sync";

                _loggingService.Info("Creating subscription for " + model + "\nModel valid: " + ViewData.ModelState.IsValid);

                ViewData.Model = model;
                if (ViewData.ModelState.IsValid)
                {
                    _subscriptionPersistenceService.StoreSubscription(model);

                    SubscriptionServiceResult result = _subscriptionService.Subscribe(_hubConfiguration, model);

                    if (result.Type == SubscriptionResponseResultType.Error)
                    {
                        ViewData["ErrorDescription"] = result.ErrorDescription;
                    }
                    view = RedirectToAction("Index");
                }
            }
            catch(Exception ex)
            {
                string msg = "An exception occurred in Create method: " + ex.ToString();
                _loggingService.Error(msg);
                ViewData["ErrorDescription"] = msg;
            }
            return view;
        }

        //
        // GET: /HubSubscription/Delete/5
 
        public ActionResult Delete(int id)
        {
            ActionResult view = View("DeleteError");

            _loggingService.Info("Deleting subscription Id: " + id);

            try
            {                
                SubscriptionModel model = _subscriptionPersistenceService.GetSubscriptionById(id);
                model.PendingDeletion = true;
                model.LastUpdated = DateTime.Now;
                model.Mode = "unsubscribe";
                _subscriptionPersistenceService.SaveChanges(model);

                SubscriptionServiceResult result = _subscriptionService.UnSubscribe(_hubConfiguration, model);

                if (result.Type == SubscriptionResponseResultType.Error)
                {
                    ViewData["ErrorDescription"] = result.ErrorDescription;
                }
                else if (result.Type == SubscriptionResponseResultType.NotFound)
                {
                    _subscriptionPersistenceService.DeleteSubscriptionById(id);

                    string msg = "The subscription could not be found in the subscription service. Deleted anyway. " +
                        result.ErrorDescription;
                    _loggingService.Error(msg);
                    ViewData["ErrorDescription"] = msg;
                }
                else
                {
                    view = RedirectToAction("Index");
                }
            }            
            catch (Exception ex)
            {
                string msg = "An exception in Delete method: " + ex.ToString();
                _loggingService.Error(msg);
                ViewData["ErrorDescription"] = msg;
            }

            return view;
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

                        string documentContents;
                        using (Stream receiveStream = Request.InputStream)
                        {
                            using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                            {
                                documentContents = readStream.ReadToEnd();                                  
                            }
                        }

                        _hubSubscriptionListener.SubscriptionUpdateReceived(documentContents);
                        
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
                     Response.StatusCode = (int)HttpStatusCode.NotFound;
                 }
                 else
                 {
                     _subscriptionPersistenceService.DeleteSubscriptionById(id);

                     _loggingService.Info("Deleted subscription Id: " + id);

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
    }
}
