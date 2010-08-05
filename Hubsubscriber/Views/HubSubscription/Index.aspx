<%@ Page Title="" Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage<IEnumerable<HubSubscriber.Models.SubscriptionModel>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Kwwika-SuperFeedr Demo
</asp:Content>

<asp:Content ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript" src="http://code.jquery.com/jquery-1.4.2.min.js"></script>
    <link href="../../Content/js/formValidator.1.7/css/validationEngine.jquery.css" rel="stylesheet" type="text/css" />
    <script src="../../Content/js/formValidator.1.7/js/jquery.validationEngine-en.js" type="text/javascript"></script>
    <script src="../../Content/js/formValidator.1.7/js/jquery.validationEngine.js" type="text/javascript"></script>

    <script src="http://api.kwwika.com/latest"></script>
    <script>
    kwwika.namespace("kwwika.superfeedr");

    kwwika.superfeedr.RealTimeSubscriber = function(realTimeTopic, feedItemsParentId)
    {
        this.realTimeTopic = realTimeTopic;
        this.feedItemsParentId = feedItemsParentId;
    };
    kwwika.superfeedr.RealTimeSubscriber.prototype.subscribe = function()
    {
        var rts = this;
        this.connection = kwwika.Service.connect();
        this.newsSubscription = this.connection.subscribe(rts.realTimeTopic,
        {
            topicUpdated: function (sub, update)
            {
                var newsItem = this.createNewsItem(update);
                newsItem.hide();
                $("#" + rts.feedItemsParentId).prepend(newsItem);
                newsItem.slideDown();
                //$("#feed_updated").html(update.feedUpdated);

                var newsItems = $("#" + rts.feedItemsParentId + " .topic-box");
                if(newsItems.size() > 50)
                {
                    newsItems.last().remove();
                }
            },
            topicError: function (sub, error)
            {
                alert("Error subscribing to " + sub.topicName + ": " + error);
            },
            createNewsItem: function(update)
            {
                var title = update.entryTitle;
                var content = update.entryContent;
                var link = update.entryLinkAlternate;
                var topic = update.topic || "";

                return $('<div class="topic-box">' +
                            '<div class="topic-title">' +
                                '<div class="topic-link">' +
                                    '<div class="link-arrow">' + link + '</div>' +
                                '</div>' +
                                '<div class="topic-tri" title="' + topic + '">' +
                                    (topic.length > 65? topic.substring(0, 55) + "...":topic) +
                                '</div>' +
                                 'Topic' +
                            '</div>' +
                            '<div class="topic-content">' + 
                                '<h3 class="topic-content-title">' + title + '</h3>' + 
                                '<div class="topic-content-body">' + content +'</div>' +
                            '</div>' +
                        '</div>');
            }
        });
    };

    kwwika.superfeedr.RealTimeSubscriber.prototype.unsubscribe = function()
    {
        this.connection.unsubscribe(this.newsSubscription);
    };
    
    $(window).load(function()
    {
        var user = {
            pushTopic: '<%= ((HubSubscriber.Models.UserInfoModel)ViewData["UserInfo"]).PushTopic %>',
            username: '<%= ((HubSubscriber.Models.UserInfoModel)ViewData["UserInfo"]).Username %>',
            status: '<%= ((HubSubscriber.Models.UserInfoModel)ViewData["UserInfo"]).Status %>'
            };

        var realTimeSubscriber;

        function clearSubscriptionsUI()
        {
            $("#leftcontent").html("");
            $("#no_feed_items_notice").hide();
        };

        function getUserRealTimeSubscriptions()
        {
            if(realTimeSubscriber)
            {
                realTimeSubscriber.unsubscribe();
            }
            realTimeSubscriber = new kwwika.superfeedr.RealTimeSubscriber(user.pushTopic, "feed_items");
            realTimeSubscriber.subscribe();
        };

        function getUsersHubSubscriptions()
        {
            var listUrl = '<%= Url.Action("List", "HubSubscription") %>';
            $.ajax({
                url: listUrl,
                type: "POST",
                dataType: 'json',
                success: function(data, status)
                            {
                                if(status == "success")
                                {
                                    if(data.length == 0)
                                    {
                                        $("#no_feed_items_notice").fadeIn();
                                    }
                                    else
                                    {
                                        $.each(data, function(i, sub)
                                        {
                                            addSubscriptionToUI(sub);
                                        });
                                    }
                                }
                            }
                    });
        };

        function addSubscriptionToUI(sub)
        {
            var subEl = createSubscriptionHtml(sub);
            subEl.hide();
            $("#leftcontent").append(subEl);
            subEl.fadeIn();
        };

        function deleteSubscription()
        {
            if(!confirm("Are you sure you wish to delete this subscription?"))return false;

            var img = $(this);
            var id = img.attr("id");
            var subIdToDelete = id.substring(id.indexOf("_")+1);
            
            var deleteUrl = '<%= Url.Action("Delete", "HubSubscription") %>';
            $.ajax({
                url: deleteUrl,
                data:{id:subIdToDelete},
                type: "POST",
                dataType: 'json',
                success: function(data, status)
                        {
                            if(status == "success")
                            {
//                                NotAuthorised,
//                                Error,
//                                Success,
//                                NotFound
                                if(data.Type != 2)
                                {
                                    alert(data.ErrorDescription);
                                }        
                                
                                if( (data.Type == 2 || data.Type == 3) &&
                                    data.Subscription )
                                {
                                    var img = $("#subscription_" + data.Subscription.Id);
                                    img.parents(".keyword").fadeOut(function()
                                    {
                                        $(this).remove();
                                        if($("#leftcontent .keyword").size() == 0)
                                        {
                                            $("#no_feed_items_notice").show();
                                        }
                                    });
                                }   
                            }
                        }
                });
        }

        function createSubscriptionHtml(sub)
        {
            var img = $('<img id="subscription_' + sub.Id + '" src="/Content/Images/delete.png" width="14" height="14" alt="Delete subscription" />');
            img.click(deleteSubscription);
            var keyword = $('<div class="keyword" title="' + sub.Topic + '">' + 
                                '<span>' +
                                    (sub.Topic.length > 35?sub.Topic.substring(0, 35) + '...':sub.Topic) +                             
                                '</span>' +
                            '</div>');
            keyword.find("span").append(img);
            return keyword;
        };

        function init()
        {
            clearSubscriptionsUI();
            getUsersHubSubscriptions();
            getUserRealTimeSubscriptions();
        };

        function submitLogin()
        {
            var loginUrl = '<%= Url.Action("Login", "HubSubscription") %>';

            var formValues = $("#loginform").serialize();
            $.ajax({
                url: loginUrl,
                type: "POST",
                data: formValues,
                dataType: 'json',
                success: function(data, status)
                            {
                                if(status == "success")
                                {
                                    user = data;
                                    if(user.Status == "LoggedIn")
                                    {
                                        $("#userinfo_wrapper .username").html(user.Username);
                                        $("#loginform_wrapper").fadeOut('slow',function()
                                        {
                                            $("#userinfo_wrapper").fadeIn('slow');
                                        });

                                        init();
                                    }
                                    else
                                    {
                                        alert("you have not been logged in");
                                    }
                                }
                                else
                                {
                                    alert("an error occurred whilst logging in.");
                                }
                            }
                    });

            return false;
        };

        function submitCreateSubscription()
        {
            var loginUrl = '<%= Url.Action("Create", "HubSubscription") %>';
            var track = $.trim($("#Track").val());

            if(track.match("^http:\/\/") == null)
            {
                track = "http://superfeedr.com/track/" + track;
            }

            var subscriptionModel = {
                    Topic: track
                };

            var subscriptions = $.ajax({
                url: loginUrl,
                type: "POST",
                data: subscriptionModel,
                dataType: 'json',
                success: function(data, status)
                         {
                            if(status == "success")
                            {
//                                NotAuthorised,
//                                Error,
//                                Success,
//                                NotFound
                                if(data.Type != 2)
                                {
                                    alert(data.ErrorDescription);
                                }        
                                
                                if(data.Subscription)
                                {
                                    $("#no_feed_items_notice").hide();

                                    addSubscriptionToUI(data.Subscription);                                    
                                }
                            }
                            else
                            {
                                alert("an error occurred whilst subscribing to " + subscriptionModel.Topic);
                            }
                         }
                    });

            return false;
        };        

        $("#logout").click(function()
        {
            var logoutUrl = '<%= Url.Action("Logout", "HubSubscription") %>';
            var subscriptions = $.ajax({
                url: logoutUrl,
                type: "POST",
                dataType: 'json',
                success: function(data, status)
                            {
                                if(status == "success")
                                {
                                    user = data;
                                    if(user.Status == "LoggedOut")
                                    {
                                        $("#userinfo_wrapper").fadeOut('slow',function()
                                        {
                                            $("#loginform_wrapper").fadeIn('slow');
                                        });

                                        init();
                                    }
                                    else
                                    {
                                        alert("you have not been logged out");
                                    }
                                }
                                else
                                {
                                    alert("an error occurred whilst logging out");
                                }
                            }
                    });

            return false;
        });

        $("#loginform").validationEngine({success:submitLogin, promptPosition:"bottomright",unbindEngine:false});
        $("#createsubscriptionform").validationEngine({
                success:submitCreateSubscription,
                promptPosition:"bottomright",
                unbindEngine:false
            });
        init();
    });   
    </script>
</asp:Content>

<asp:Content ContentPlaceHolderID="HeaderContent" runat="server">
    <div id="loginform_wrapper" <%= ((HubSubscriber.Models.UserInfoModel)ViewData["UserInfo"]).Status == "LoggedIn"?"class=\"hidden\"":"" %>>
        <% using (Html.BeginForm("Login", "HubSubscription", FormMethod.Post, new { id = "loginform" }))
           { %>	
                Username: <input type="text" id="Username" name="Username" class="validate[required]" /><br />
                Password: <input type="password" id="Password" name="Password" class="validate[required]" /><br />
                <input type="submit" value="Login" />
        <% } %>
    </div>
    <div id="userinfo_wrapper" <%= ((HubSubscriber.Models.UserInfoModel)ViewData["UserInfo"]).Status == "LoggedIn"?"":"class=\"hidden\"" %>>
        <p>Hello, <span class="username"><%= ((HubSubscriber.Models.UserInfoModel)ViewData["UserInfo"]).Username%></span> | <span id="logout">Logout</span></p>
    </div>
    
    <div id="create_subscription">
        <% using (Html.BeginForm("Create", "HubSubscription", FormMethod.Post, new { id = "createsubscriptionform" }))
           { %>	
                <input type="text" id="Track" name="Track" class="validate[required,length[5,256]]" />
                <input type="submit" value="Track" />
        <% } %>
    </div>	
</asp:Content>

<asp:Content ContentPlaceHolderID="LeftColumnContent" runat="server">
 	
    <div id="no_feed_items_notice" class="hidden">No subscriptions have been created</div> 
    <div id="leftcontent">
        
    </div>

</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    
    <div id="feed_items"></div>
		      
</asp:Content>