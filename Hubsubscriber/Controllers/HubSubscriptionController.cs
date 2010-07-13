using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;
using HubSubscriber.Kwwika;
using HubSubscriber.Services;
using Kwwika.Common.Logging;
using System.Collections;
using System.Collections.Generic;

namespace HubSubscriber.Kwwika
{
    public class HubSubscriptionController : Controller
    {
        private ILoggingService _loggingService;        
        private IHubSubscriptionListener _hubSubscriptionListener;
        private IHubSubscriptionPersistenceService _subscriptionPersistenceService;

        public HubSubscriptionController():
            this(MvcApplication.LoggingService)
        {
        }

        public HubSubscriptionController(ILoggingService loggingService)
        {
            _loggingService = loggingService;
            _hubSubscriptionListener = MvcApplication.Container.GetService<IHubSubscriptionListener>();
            _subscriptionPersistenceService = MvcApplication.Container.GetService<IHubSubscriptionPersistenceService>();
        }

        //
        // GET: /HubSubscription/

        public ActionResult Index()
        {
            ViewData.Model = (IEnumerable<SubscriptionModel>)_subscriptionPersistenceService.GetSubscriptionsList();

            return View();
        }

        //
        // GET: /HubSubscription/Details/5

        public ActionResult Details(int id)
        {
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
            ActionResult view = null;
            try
            {
                string appPath = Url.Content("~").Substring(1);
                model.Callback = model.Callback ?? Request.Url.GetLeftPart(UriPartial.Authority) + appPath + Url.Action("HubUpdate", "HubSubscription");
                model.Mode = model.Mode ?? "subscribe";
                model.Verify = model.Verify ?? "sync";

                _loggingService.Info("Creating subscription for " + model + "\nModel valid: " + ViewData.ModelState.IsValid);

                ViewData.Model = model;
                if (!ViewData.ModelState.IsValid)
                {
                    view = View();
                }
                else
                {
                    _subscriptionPersistenceService.StoreSubscription(model);                    

                    HttpWebResponse response = GetSubscriptionResponse(model);

                    _loggingService.Info("Received response for create request: StatusCode: " + response.StatusDescription);

                    view = HandlePubSubHubResponse(response);
                    
                }
            }
            catch(Exception ex)
            {
                string msg = "An exception occurred in Create method: " + ex.ToString();
                _loggingService.Error(msg);
                ViewData["ErrorDescription"] = msg;
                view = View();
            }
            return view;
        }

        //
        // GET: /HubSubscription/Delete/5
 
        public ActionResult Delete(int id)
        {
            ActionResult view = View("Error");

            _loggingService.Info("Deleting subscription Id: " + id);

            Subscription sub = null;
            try
            {                
                _subscriptionPersistenceService.MarkSubscriptionPendingDeletionById(id);

                var model = new SubscriptionModel();
                model.Id = sub.Id;
                model.Topic = sub.Topic;
                string appPath = Url.Content("~").Substring(1);
                model.Callback = model.Callback ?? Request.Url.GetLeftPart(UriPartial.Authority) + appPath + Url.Action("HubUpdate", "HubSubscription");
                model.Mode = "unsubscribe";
                model.Verify = "sync";
                var response = GetSubscriptionResponse(model);

                _loggingService.Info("Received response for delete request: StatusCode: " + response.StatusDescription);

                return HandlePubSubHubResponse(response);
            }
            catch (WebException we)
            {
                if ((int)((HttpWebResponse)we.Response).StatusCode == 422)
                {
                    _subscriptionPersistenceService.DeleteSubscriptionById(id);

                    string msg = "An exception in Delete method since the hub did not believe the subscription to exist. Deleted anyway.";
                    _loggingService.Error(msg);
                    ViewData["ErrorDescription"] = msg;
                }
                else
                {
                    string msg = "An exception in Delete method: " + we.ToString();
                    _loggingService.Error(msg);
                    ViewData["ErrorDescription"] = msg;
                }
            }
            catch (Exception ex)
            {
                string msg = "An exception in Delete method: " + ex.ToString();
                _loggingService.Error(msg);
                ViewData["ErrorDescription"] = msg;
            }

            return RedirectToAction("Index");
        }

        // deprecated
        public ActionResult Update(int id)
        {
            return HubUpdate(id);
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
                            using (StreamReader  readStream = new StreamReader(receiveStream, Encoding.UTF8))
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
                     //Response.StatusCode = (int)HttpStatusCode.NotFound;
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
                 }
                 ViewData["hub.challenge"] = Request["hub.challenge"];
                 Response.StatusCode = (int)HttpStatusCode.OK;
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

        private ActionResult HandlePubSubHubResponse(HttpWebResponse response)
        {
            ActionResult view = RedirectToAction("Index");
            _loggingService.Info("Response received: " + response.StatusCode);

            if (response.StatusCode != HttpStatusCode.NoContent)
            {
                ViewData["ErrorDescription"] = "The pubsubhub returned " + response.StatusDescription;
            }
            else
            {
                using (Stream receiveStream = response.GetResponseStream())
                {
                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    {
                        string hubResponse = readStream.ReadToEnd();

                        _loggingService.Info("Response text received: " + hubResponse);
                    }
                }
            }
            return view;
        }

        private HttpWebResponse GetSubscriptionResponse(SubscriptionModel model)
        {
            UriBuilder builder = new UriBuilder("http://superfeedr.com/hubbub");

            string query = "hub.mode=" + model.Mode + "&";
            query += "hub.verify=" + model.Verify + "&";
            query += "hub.callback=" + model.Callback + "/" + model.Id + "&";
            query += "hub.topic=" + model.Topic;
            builder.Query = query;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(builder.Uri);
            byte[] authBytes = Encoding.UTF8.GetBytes("leggetter:cResweF7".ToCharArray());
            request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(authBytes);

            request.Method = "POST";
            request.PreAuthenticate = true;

            _loggingService.Info("Making request to hub for subscription: " + request.RequestUri.ToString());

            return (HttpWebResponse)request.GetResponse();
        }
    }
}
