using HubSubscriber.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting.Web;
using Kwwika.Common.Logging;
using System.Web.Mvc;
using HubSubscriber.Models;
using System.Net;
using Rhino.Mocks;
using HubSubscriber;
using Castle.Windsor;
using Castle.MicroKernel.Registration;
using HubSubscriber.Services;
using Rhino.Mocks.Constraints;
using System.Web.Routing;
using System.Web;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HubsubscriberTests
{
    [TestClass()]
    public class HubSubscriptionControllerTest
    {
        private TestContext testContextInstance;
        private MockRepository _mocks;
        private ILoggingService _loggingService;
        private IHubSubscriptionListener _subscriptionListener;
        private IHubSubscriptionService _subscriptionService;
        private IHubSubscriptionPersistenceService _subscriptionPersistenceService;
        private HubSubscriptionController _controller;
        private HttpRequestBase _request;
        private HttpResponseBase _response;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        // Use TestInitialize to run code before running each test
        [TestInitialize()]
        public void MyTestInitialize()
        {
            _mocks = new MockRepository();
            _loggingService = _mocks.Stub<ILoggingService>();
            _subscriptionListener = _mocks.StrictMock<IHubSubscriptionListener>();
            _subscriptionService = _mocks.StrictMock<IHubSubscriptionService>();
            _subscriptionPersistenceService = _mocks.StrictMock<IHubSubscriptionPersistenceService>();

            MvcApplication.LoggingService = _loggingService;

            MvcApplication.Container = new WindsorContainer();
            MvcApplication.Container.Kernel.AddComponentInstance<ILoggingService>(_loggingService);
            MvcApplication.Container.Kernel.AddComponentInstance<IHubSubscriptionListener>(_subscriptionListener);
            MvcApplication.Container.Kernel.AddComponentInstance<IHubSubscriptionService>(_subscriptionService);
            MvcApplication.Container.Kernel.AddComponentInstance<IHubSubscriptionPersistenceService>(_subscriptionPersistenceService);

            var routes = new RouteCollection();
            MvcApplication.RegisterRoutes(routes);

            _request = _mocks.Stub<HttpRequestBase>();
            SetupResult.For(_request.ApplicationPath).Return("/");
            SetupResult.For(_request.Url).Return(new Uri("http://localhost/a", UriKind.Absolute));
            SetupResult.For(_request.ServerVariables).Return(new System.Collections.Specialized.NameValueCollection());
            SetupResult.For(_request.Params).Return(new System.Collections.Specialized.NameValueCollection());

            _response = _mocks.Stub<HttpResponseBase>();
            SetupResult.For(_response.ApplyAppPathModifier("/post1")).Return("http://localhost/post1");

            var context = _mocks.Stub<HttpContextBase>();
            var session = _mocks.Stub<HttpSessionStateBase>();
            SetupResult.For(context.Request).Return(_request);
            SetupResult.For(context.Response).Return(_response);
            SetupResult.For(context.Session).Return(session);

            _controller = new HubSubscriptionController(_loggingService);
            _controller.ControllerContext = new ControllerContext(context, new RouteData(), _controller);
            _controller.Url = new UrlHelper(new RequestContext(context, new RouteData()), routes);
        }

        //
        //Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
        }
        
        #endregion


        [TestMethod()]
        public void HubSubscriptionController_constructor_initialises_requried_services_test()
        {
            AssertServicesAreNotNull(_controller);
            Assert.IsNotNull(_controller.HubConfiguration);
        }        

        [TestMethod()]
        public void HubSubscriptionController_default_constructor_test()
        {
            AssertServicesAreNotNull(_controller);
            Assert.IsNotNull(_controller.HubConfiguration);
        }

        private void AssertServicesAreNotNull(HubSubscriptionController target)
        {
            Assert.IsNotNull(target.LoggingService);
            Assert.IsNotNull(target.HubSubscriptionListener);
            Assert.IsNotNull(target.HubSubscriptionPersistenceService);
            Assert.IsNotNull(target.HubSubscriptionService);
        }

        [TestMethod()]
        public void CreateTest()
        {
            HubSubscriptionController target = new HubSubscriptionController();
            ActionResult actual = target.Create();

            Assert.IsNotNull(actual);
        }

        [TestMethod()]
        public void Create_stores_subscription_and_makes_subscription_request_to_subscription_service()
        {
            SubscriptionModel model = new SubscriptionModel();
            ActionResult actual = null;

            With.Mocks(_mocks).Expecting(delegate
            {
                _subscriptionPersistenceService.Stub(x => x.GetMaxSubscriptionsForUser(_controller.HubConfiguration.HubUsername)).Return(10);
                _subscriptionPersistenceService.Stub(x => x.GetSubscriptionCountForUser(_controller.HubConfiguration.HubUsername)).Return(1);

                _subscriptionPersistenceService.Expect(x => x.StoreSubscription(model));

                _subscriptionService.Expect(x => x.Subscribe(_controller.HubConfiguration, model))
                    .Return(new SubscriptionServiceResult() { Type = SubscriptionResponseResultType.Success });

            }).Verify(delegate
            {
                actual = _controller.Create(model);
            });
            Assert.IsNotNull(actual);
        }

        [TestMethod()]
        public void Create_sets_error_if_MaxSubscriptions_has_been_reached_test()
        {
            SubscriptionModel model = new SubscriptionModel();
            ActionResult actual = null;

            With.Mocks(_mocks).Expecting(delegate
            {
                _subscriptionPersistenceService.Expect(x => x.GetMaxSubscriptionsForUser(_controller.HubConfiguration.HubUsername)).Return(10);
                _subscriptionPersistenceService.Expect(x => x.GetSubscriptionCountForUser(_controller.HubConfiguration.HubUsername)).Return(10);

            }).Verify(delegate
            {
                actual = _controller.Create(model);
            });
            Assert.IsNotNull(actual);
            Assert.IsNotNull(_controller.ViewData["ErrorDescription"]);
        }

        [TestMethod()]
        public void Create_sets_ViewData_ErrorDescription_when_result_has_Error_type_test()
        {
            SubscriptionModel model = new SubscriptionModel();
            ActionResult actual = null;
            string errorDescription = "an error";

            With.Mocks(_mocks).Expecting(delegate
            {
                _subscriptionPersistenceService.Stub(x => x.GetMaxSubscriptionsForUser(_controller.HubConfiguration.HubUsername)).Return(10);
                _subscriptionPersistenceService.Stub(x => x.GetSubscriptionCountForUser(_controller.HubConfiguration.HubUsername)).Return(1);

                _subscriptionPersistenceService.Expect(x => x.StoreSubscription(model));

                _subscriptionService.Expect(x => x.Subscribe(_controller.HubConfiguration, model))
                    .Return(new SubscriptionServiceResult()
                    {
                        Type = SubscriptionResponseResultType.Error,
                        ErrorDescription = errorDescription
                    });

            }).Verify(delegate
            {
                actual = _controller.Create(model);
            });
            Assert.IsNotNull(actual);
            Assert.AreEqual(errorDescription, (string)(_controller.ViewData["ErrorDescription"]));
        }

        [TestMethod()]
        public void Delete_stores_subscription_and_makes_subscription_request_to_subscription_service()
        {
            ActionResult actual = null;
            int deleteId = 1;
            SubscriptionModel model = new SubscriptionModel();

            With.Mocks(_mocks).Expecting(delegate
            {
                _subscriptionPersistenceService.Stub(x => x.GetMaxSubscriptionsForUser(_controller.HubConfiguration.HubUsername)).Return(10);
                _subscriptionPersistenceService.Stub(x => x.GetSubscriptionCountForUser(_controller.HubConfiguration.HubUsername)).Return(1);

                _subscriptionPersistenceService.Expect(x => x.GetSubscriptionById(deleteId)).Return(model);
                
                _subscriptionPersistenceService.Expect(x => x.SaveChanges(model));

                _subscriptionService.Expect(x => x.UnSubscribe(_controller.HubConfiguration, model))
                    .Return(new SubscriptionServiceResult() { Type = SubscriptionResponseResultType.Success });

            }).Verify(delegate
            {
                actual = _controller.Delete(deleteId);
            });
            Assert.IsNotNull(actual);
        }

        [TestMethod()]
        public void Delete_subscription_NotFound_in_subscription_service_sets_ErrorDescription()
        {
            ActionResult actual = null;
            int deleteId = 1;
            SubscriptionModel model = new SubscriptionModel();

            With.Mocks(_mocks).Expecting(delegate
            {
                _subscriptionPersistenceService.Expect(x => x.GetSubscriptionById(deleteId)).Return(model);

                _subscriptionPersistenceService.Expect(x => x.SaveChanges(model));

                _subscriptionService.Expect(x => x.UnSubscribe(_controller.HubConfiguration, model))
                    .Return(new SubscriptionServiceResult() { Type = SubscriptionResponseResultType.NotFound });

                _subscriptionPersistenceService.Expect(x => x.DeleteSubscriptionById(deleteId));

            }).Verify(delegate
            {
                actual = _controller.Delete(deleteId);
            });
            Assert.IsNotNull(actual);
            Assert.IsNotNull(_controller.ViewData["ErrorDescription"]);
        } 

        [TestMethod()]
        public void HubUpdate_for_unverified_subscription_Test()
        {
            ActionResult actual = null;
            int detailsId = 1;
            SubscriptionModel model = new SubscriptionModel()
            {
                Verified = false
            };

            With.Mocks(_mocks).Expecting(delegate
            {
                _subscriptionPersistenceService.Expect(x => x.GetSubscriptionById(detailsId)).Return(model);
                _subscriptionPersistenceService.Expect(x => x.GetSubscriptionCountById(detailsId)).Return(1);

                _subscriptionPersistenceService.Expect(x => x.SaveChanges(model));

            }).Verify(delegate
            {
                actual = _controller.HubUpdate(detailsId);
            });
            Assert.IsNotNull(actual);
            Assert.IsTrue(model.Verified);
            Assert.AreEqual((int)HttpStatusCode.OK, _response.StatusCode);
        }

        [TestMethod()]
        public void HubUpdate_for_feed_update_Test()
        {
            ActionResult actual = null;
            int detailsId = 1;
            string updateContents = "some document contents";
            SetupResult.For(_request.InputStream).Return(new MemoryStream(new UTF8Encoding().GetBytes(updateContents)));
            SubscriptionModel model = new SubscriptionModel()
            {
                Verified = true,
                PubSubHubUser = "user"
            };
            UserModel userModel = new UserModel();

            With.Mocks(_mocks).Expecting(delegate
            {
                _subscriptionPersistenceService.Expect(x => x.GetSubscriptionById(detailsId)).Return(model);
                _subscriptionPersistenceService.Expect(x => x.GetSubscriptionCountById(detailsId)).Return(1);
                _subscriptionPersistenceService.Expect(x => x.GetUser(model.PubSubHubUser)).Return(userModel);
                _subscriptionListener.Expect(x => x.SubscriptionUpdateReceived(userModel, updateContents));
                _subscriptionPersistenceService.Expect(x => x.SaveChanges(model));

            }).Verify(delegate
            {
                actual = _controller.HubUpdate(detailsId);
            });
            Assert.IsNotNull(actual);
            Assert.AreEqual((int)HttpStatusCode.OK, _response.StatusCode);
        }

        [TestMethod()]
        public void HubUpdate_for_feed_update_NotFound_response_Test()
        {
            ActionResult actual = null;
            int detailsId = 1;
            SubscriptionModel detailsSub = new SubscriptionModel()
            {
                Verified = true
            };

            With.Mocks(_mocks).Expecting(delegate
            {
                _subscriptionPersistenceService.Expect(x => x.GetSubscriptionCountById(detailsId)).Return(0);

            }).Verify(delegate
            {
                actual = _controller.HubUpdate(detailsId);
            });
            Assert.IsNotNull(actual);
            Assert.AreEqual((int)HttpStatusCode.NotFound, _response.StatusCode);
        }

        [TestMethod()]
        public void HubUpdate_for_feed_unsubscribe_Test()
        {
            ActionResult actual = null;
            int detailsId = 1;
            string updateContents = "some document contents";
            string hubChallenge = new Guid().ToString();
            SetupResult.For(_request.InputStream).Return(new MemoryStream(new UTF8Encoding().GetBytes(updateContents)));
            SetupResult.For(_request["hub.mode"]).Return("unsubscribe");
            SetupResult.For(_request["hub.challenge"]).Return(hubChallenge);
            SubscriptionModel detailsSub = new SubscriptionModel()
            {
                Verified = true
            };

            With.Mocks(_mocks).Expecting(delegate
            {
                _subscriptionPersistenceService.Expect(x => x.GetSubscriptionCountById(detailsId)).Return(1);
                _subscriptionPersistenceService.Expect(x => x.DeleteSubscriptionById(detailsId));

            }).Verify(delegate
            {
                actual = _controller.HubUpdate(detailsId);
            });
            Assert.IsNotNull(actual);
            Assert.AreEqual(hubChallenge, _controller.ViewData["hub.challenge"]);
            Assert.AreEqual((int)HttpStatusCode.OK, _response.StatusCode);
        }

        [TestMethod()]
        public void HubUpdate_for_feed_NotFound_Test()
        {
            SubscriptionModel model = new SubscriptionModel();
            ActionResult actual = null;
            int detailsId = 1;
            string hubChallenge = new Guid().ToString();
            SetupResult.For(_request["hub.mode"]).Return("unsubscribe");
            SetupResult.For(_request["hub.challenge"]).Return(hubChallenge);

            With.Mocks(_mocks).Expecting(delegate
            {
                _subscriptionPersistenceService.Expect(x => x.GetSubscriptionCountById(detailsId)).Return(0);

            }).Verify(delegate
            {
                actual = _controller.HubUpdate(detailsId);
            });
            Assert.IsNotNull(actual);
            Assert.AreEqual(hubChallenge, _controller.ViewData["hub.challenge"]);
            Assert.AreEqual((int)HttpStatusCode.NotFound, _response.StatusCode);
        }

        [TestMethod()]
        public void IndexTest()
        {
            ActionResult actual = null;
            var subsList = new List<SubscriptionModel>();

            With.Mocks(_mocks).Expecting(delegate
            {
                _subscriptionPersistenceService.Expect(x => x.GetSubscriptionsList(Config.HubUsername)).Return(subsList);

            }).Verify(delegate
            {
                actual = _controller.Index();
            });
            Assert.IsNotNull(actual);
            Assert.AreEqual(subsList, _controller.ViewData.Model);
        }

        [TestMethod()]
        public void Login_checks_user_has_been_added_to_the_database_and_Error_is_set_when_user_does_not_exist_Test()
        {
            ActionResult actual = null;
            SubscriptionModel model = new SubscriptionModel();
            UserModel userModel = new UserModel()
            {
                Username = "user",
                Password = "pass"
            };

            With.Mocks(_mocks).Expecting(delegate
            {
                _subscriptionPersistenceService.Expect(x => x.GetUser(userModel.Username))
                    .Return(null);

            }).Verify(delegate
            {
                actual = _controller.Login(userModel);
            });

            Assert.IsNotNull(_controller.ViewData.ModelState["_FORM"]);
        }

        [TestMethod()]
        public void Login_attempts_a_Subscribe_and_Unsubscribe_on_subscription_service_Test()
        {
            ActionResult actual = null;
            SubscriptionModel model = new SubscriptionModel();
            UserModel userModel = new UserModel()
                {
                    Username = "user",
                    Password = "pass"
                };

            With.Mocks(_mocks).Expecting(delegate
            {
                _subscriptionPersistenceService.Stub(x => x.GetUser(userModel.Username))
                    .Return(userModel);

                _subscriptionService.Expect(x => x.Subscribe(
                        Arg<IHubConfiguration>.Matches(
                            config => config.HubUsername == userModel.Username &&
                            config.HubPassword == userModel.Password),
                        Arg<SubscriptionModel>.Is.Anything))
                    .Return(new SubscriptionServiceResult() { Type = SubscriptionResponseResultType.Success });

                _subscriptionService.Expect(x => x.UnSubscribe(
                        Arg<IHubConfiguration>.Matches(
                            config => config.HubUsername == userModel.Username &&
                            config.HubPassword == userModel.Password),
                        Arg<SubscriptionModel>.Is.Anything))
                    .Return(new SubscriptionServiceResult() { Type = SubscriptionResponseResultType.Success });

            }).Verify(delegate
            {
                actual = _controller.Login(userModel);
            });

            Assert.IsNotNull(actual);
            Assert.IsNull(_controller.ViewData["ErrorDescription"]);
        }

        [TestMethod()]
        public void Login_attempts_a_Subscribe_and_NotAuthorisedResponse_leads_to_ErrorDescription_being_set_Test()
        {
            ActionResult actual = null;
            SubscriptionModel model = new SubscriptionModel();
            UserModel userModel = new UserModel()
            {
                Username = "user",
                Password = "pass"
            };

            With.Mocks(_mocks).Expecting(delegate
            {
                _subscriptionPersistenceService.Stub(x => x.GetUser(userModel.Username))
                    .Return(userModel);

                _subscriptionService.Expect(x => x.Subscribe(
                        Arg<IHubConfiguration>.Matches(
                            config => config.HubUsername == userModel.Username &&
                            config.HubPassword == userModel.Password),
                        Arg<SubscriptionModel>.Is.Anything))
                    .Return(new SubscriptionServiceResult() { Type = SubscriptionResponseResultType.NotAuthorised });

            }).Verify(delegate
            {
                actual = _controller.Login(userModel);
            });

            Assert.IsNotNull(actual);
            Assert.IsNull(_controller.ViewData["ErrorDescription"]);
        }

        [TestMethod()]
        public void Logout_does_not_return_null_Test()
        {
            ActionResult actual = null;
            With.Mocks(_mocks).Expecting(delegate
            {

            }).Verify(delegate
            {
                actual = _controller.Logout();
            });

            Assert.IsNotNull(actual);
        }

        [TestMethod()]
        public void HubLoginTest_echos_hub_challenge_Test()
        {
            string hubChallenge = new Guid().ToString();
            SetupResult.For(_request["hub.challenge"]).Return(hubChallenge);

            ActionResult actual = null;

            With.Mocks(_mocks).Expecting(delegate
            {

            }).Verify(delegate
            {
                actual = _controller.HubLoginTest();
            }); 

            Assert.IsNotNull(actual);
            Assert.AreEqual(hubChallenge, (string)_controller.ViewData["hub.challenge"]);
        }
    }
}
