<%@ Page Title="" Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage<IEnumerable<HubSubscriber.Models.SubscriptionModel>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Kwwika-SuperFeedr Demo
</asp:Content>

<asp:Content ContentPlaceHolderID="HeadContent" runat="server">
    <script>
    var user = {
            PushTopic: '<%= ((HubSubscriber.Models.UserInfoModel)ViewData["UserInfo"]).PushTopic %>',
            Username: '<%= ((HubSubscriber.Models.UserInfoModel)ViewData["UserInfo"]).Username %>',
            Status: '<%= ((HubSubscriber.Models.UserInfoModel)ViewData["UserInfo"]).Status %>'
        };
        
    var service = {
            listUrl: '<%= Url.Action("List", "HubSubscription") %>',
            deleteUrl: '<%= Url.Action("Delete", "HubSubscription") %>',
            loginUrl: '<%= Url.Action("Login", "HubSubscription") %>',
            createUrl: '<%= Url.Action("Create", "HubSubscription") %>',
            logoutUrl: '<%= Url.Action("Logout", "HubSubscription") %>'
        }   
    </script>

    <script type="text/javascript" src="http://code.jquery.com/jquery-1.4.2.min.js"></script>
    <script src="http://d1eqzjbvoh1rux.cloudfront.net/json2.min.js"></script>

    <link href="../../Content/js/formValidator.1.7/css/validationEngine.jquery.css" rel="stylesheet" type="text/css" />
    <script src="../../Content/js/formValidator.1.7/js/jquery.validationEngine-en.js" type="text/javascript"></script>
    <script src="../../Content/js/formValidator.1.7/js/jquery.validationEngine.js" type="text/javascript"></script>

    <script src="http://api.kwwika.com/latest"></script>
    <script src="../../Content/js/kwwika/hubsubscriber/RealTimeSubscriber.js" type="text/javascript"></script>
    <script src="../../Content/js/kwwika/hubsubscriber/HtmlGenerator.js" type="text/javascript"></script>
    <script src="../../Content/js/kwwika-hubsubscriber.js" type="text/javascript"></script>
</asp:Content>

<asp:Content ContentPlaceHolderID="MessageContent" runat="server">
    <div id="shared_account_warning" <%= ((HubSubscriber.Models.UserInfoModel)ViewData["UserInfo"]).Status == "LoggedIn"?"class=\"hidden\"":"" %>>
        <p>You are using a demo account which means that you are restricted to tracking 10 Keywords or RSS feeds.
        Keywords and RSS feeds can be created
        and deleted by any user. Want your own dedicated demo?
        <a href="http://wiki.kwwika.com/demos/kwwika-superfeedr-demo#TOC-How-to-get-your-own-dedicated-demo">Here's how to get one</a>.</p>
    </div>

    <div id="loading">Loading...</div>    
</asp:Content>

<asp:Content ContentPlaceHolderID="HeaderContent" runat="server">

    <div id="header_content">

        <div id="create_subscription">
            <p>Track your favourite RSS feeds, or track using Keywords</p>
            <% using (Html.BeginForm("Create", "HubSubscription", FormMethod.Post, new { id = "createsubscriptionform" }))
               { %>	
                    <input type="text" id="Track" name="Track" class="validate[required,length[5,256]]" />
                    <input type="submit" value="Track" />
            <% } %>
        </div>

        <div id="login_wrapper">

            <div id="login_area" <%= ((HubSubscriber.Models.UserInfoModel)ViewData["UserInfo"]).Status == "LoggedIn"?"class=\"hidden\"":"" %>>
                <div class="superfeedr">
                    <span>Already have a Superfeedr and Kwwika account?</span> <span>Login with your Superfeedr details.</span>
                </div>
                <% using (Html.BeginForm("Login", "HubSubscription", FormMethod.Post, new { id = "loginform" }))
                   { %>	
                        <span class="label">Username:</span><input type="text" id="Username" name="Username" class="validate[required]" /><br />
                        <span class="label">Password:</span><input type="password" id="Password" name="Password" class="validate[required]" /><br />
                        <input type="submit" value="Login" />
                <% } %>                
            </div>
            <div id="userinfo_wrapper" <%= ((HubSubscriber.Models.UserInfoModel)ViewData["UserInfo"]).Status == "LoggedIn"?"":"class=\"hidden\"" %>>
                Welcome, <h3 class="username"><%= ((HubSubscriber.Models.UserInfoModel)ViewData["UserInfo"]).Username%></h3><div id="logout">Logout</div>
            </div>
            <p><a href="#how_built">How was this built?</a></p>
        </div>
        

    </div>
    	
</asp:Content>

<asp:Content ContentPlaceHolderID="LeftColumnContent" runat="server">
    <div id="info_message"></div>

 	<h2>Tracking</h2>
    <div id="no_feed_items_notice" class="hidden">No Keywords or RSS feeds are being tracked.</div> 
    <div id="leftcontent">
        
    </div>

</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">

    <h2>Live Results</h2>
    <div id="feed_items"></div>
		      
</asp:Content>

<asp:Content ContentPlaceHolderID="FooterContent" runat="server">

<div id="left_footer">
    <a name="how_built"></a>
    <h3>How was this demo built</h3>
    <p>The demo relies on two key real-time web services; <a href="http://superfeedr.com">Superfeedr</a> and <a href="http://kwwika.com">Kwwika</a>.</p>
    <p>The demo uses Superfeedr to subscribe to both RSS feeds and track the contents of thousands of RSS feeds for keywords. When Superfeedr receives
    an update to either an RSS feed or finds a keyword that the demo is interested in it instantly pushes it to the web server that this
    demo is being served from. When the web server receives an update we instantly push that update through the Kwwika service so that it can
    push that update to all the client users (web browsers) interested in that update.</p>
    <p>For more information see the <a href="http://wiki.kwwika.com/demos/kwwika-superfeedr-demo">Kwwika-Superfeedr Wiki page</a>.</p>

    <h3>Get the source code</h3>
    <p>If you'd like to build your own demo, or better yet, your own application based on this really cool technology then you
    can download the source code from <a href="http://github.com/kwwika/ASP.NET-MVC-PubSubHubbub-Subscriber/tree/Kwwika-Superfeedr-Demo">GitHub</a>.</p>
    <p>Once you have the source code you can follow <a href="http://wiki.kwwika.com/demos/kwwika-superfeedr-demo">these instructions</a> to help you get
    set up running the demo application and building your own Kwwika &amp; Superfeedr powered Real-Time Web application.</p>
</div>

<div id="right_footer">
    <div id="dedicated_demo">
        <h3>How to get your own dedicated demo</h3>
        To get your own dedicated demo you'll need to have a <a href="http://superfeedr.com/subscriber">Superfeedr Subscriber</a>
        account and have <a href="http://kwwika.com/#getbeta">registered with Kwwika</a>. The process is simple. For more details
        check out <a href="http://wiki.kwwika.com/demos/kwwika-superfeedr-demo#TOC-How-to-get-your-own-dedicated-demo">the Kwwika wiki</a>.
    </div>
    <div id="social">
        <h3>Links</h3>
        <div id="social_twitter">
            <h4>Kwwika</h4>
            <ul>
                <li><a href="http://kwwika.com">Kwwika</a></li>
                <li><a class="twitter-link" href="http://twitter.com/Kwwika">Kwwika on Twitter</a></li>
                <li><a href="http://blog.kwwika.com">Kwwika Blog</a></li>
            </ul>
        </div>
        <div id="social_superfeedr">
            <h4>Superfeedr</h4>
            <ul>
                <li><a href="http://superfeedr.com">Superfeedr</a></li>
                <li><a class="twitter-link" href="http://twitter.com/Superfeedr">Superfeedr on Twitter</a></li>
                <li><a href="http://blog.superfeedr.com">Superfeedr Blog</a></li>
            </ul>
        </div>
    </div>
</div>

</asp:Content>