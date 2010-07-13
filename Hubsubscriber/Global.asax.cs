using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Kwwika.Common.Logging;
using Kwwika.Common.Logging.NLog;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using Castle.Core.Resource;

namespace HubSubscriber
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static ILoggingService LoggingService;
        public static IWindsorContainer Container;

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("elmah.axd");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "HubSubscription", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            LoggingService = new LoggingService("HubSubscriber");

            Container = new WindsorContainer( new XmlInterpreter(new ConfigResource("castle") ) );

            Error += new EventHandler(MvcApplication_Error);

            AreaRegistration.RegisterAllAreas();

            RegisterRoutes(RouteTable.Routes);
        }

        void MvcApplication_Error(object sender, EventArgs e)
        {
            LoggingService.Error("Application level error: " + HttpContext.Current.Server.GetLastError());
        }
    }
}