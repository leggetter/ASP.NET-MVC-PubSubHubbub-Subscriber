using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HubSubscriber.Services;
using System.Net;
using Kwwika.Common.Logging;
using System.IO;
using HubSubscriber.Models;

namespace HubSubscriber.Controllers
{
    public class HubSubscriptionService: IHubSubscriptionService
    {

        private ILoggingService _loggingService;
        public HubSubscriptionService(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        #region IHubSubscriptionService Members

        public SubscriptionServiceResult Subscribe(IHubConfiguration configuration, SubscriptionModel model)
        {
            return CallSubscriber(configuration, model);
        }

        public SubscriptionServiceResult UnSubscribe(IHubConfiguration configuration, SubscriptionModel model)
        {
            return CallSubscriber(configuration, model);
        }
        #endregion

        private SubscriptionServiceResult CallSubscriber(IHubConfiguration configuration, SubscriptionModel model)
        {
            SubscriptionServiceResult result = new SubscriptionServiceResult()
            {
                Type = SubscriptionResponseResultType.Success,
                Subscription = model
            };

            var request = CreateRequest(configuration, model);

            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                _loggingService.Info("Response received: " + response.StatusCode);

                if (response.StatusCode != HttpStatusCode.NoContent)
                {
                    result.Type = SubscriptionResponseResultType.Error;
                    result.ErrorDescription = "The pubsubhub returned " + response.StatusDescription;
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
            }
            catch (WebException we)
            {
                if (((HttpWebResponse)we.Response).StatusCode == HttpStatusCode.Unauthorized)
                {
                    result.Type = SubscriptionResponseResultType.NotAuthorised;
                    result.ErrorDescription = "The credentials provided were not accepted by the subscription hub";
                }
                else if ((int)((HttpWebResponse)we.Response).StatusCode == 422)
                {
                    result.Type = SubscriptionResponseResultType.NotFound;
                    result.ErrorDescription = model.Mode + " error. The hub did not believe the subscription to exist or the sync callback failed.";
                }
                else
                {
                    string msg = model.Mode + " error. " + we.ToString();
                    _loggingService.Error(msg);
                    result.Type = SubscriptionResponseResultType.Error;
                    result.ErrorDescription = msg;
                }
            }

            return result;
        }        

        private HttpWebRequest CreateRequest(IHubConfiguration configuration, SubscriptionModel model)
        {
            UriBuilder builder = new UriBuilder(configuration.HubRoot);

            string query = "hub.mode=" + model.Mode + "&";
            query += "hub.verify=" + model.Verify + "&";
            query += "hub.callback=" + Uri.EscapeDataString(model.Callback) + "/" + model.Id + "&";
            query += "hub.topic=" + Uri.EscapeDataString(model.Topic);
            builder.Query = query;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(builder.Uri);
            byte[] authBytes = Encoding.UTF8.GetBytes((configuration.HubUsername + ":" + configuration.HubPassword).ToCharArray());
            request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(authBytes);

            request.Method = "POST";
            request.PreAuthenticate = true;

            _loggingService.Info("Making request to hub for subscription: " + request.RequestUri.ToString());

            return request;
        }

    }
}
