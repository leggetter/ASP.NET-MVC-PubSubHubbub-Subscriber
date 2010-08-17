$(window).load(function ()
{
    var realTimeSubscriber;
    var htmlGenerator = new kwwika.hubsubscriber.HtmlGenerator( "feed_items", "leftcontent", deleteSubscription);

    function clearSubscriptionsUI()
    {
        $("#leftcontent").html("");
        $("#no_feed_items_notice").hide();
    };

    function showUserInfo()
    {
        $("#shared_account_warning").slideUp();
        $("#login_area").fadeOut('slow', function ()
        {
            $("#userinfo_wrapper").fadeIn('slow');
        });
    };

    function hideUserInfo()
    {
        $("#userinfo_wrapper").fadeOut('slow', function ()
        {
            $("#login_area").fadeIn();
            $("#shared_account_warning").slideDown();
        });
    };

    function getUserRealTimeSubscriptions()
    {
        if (realTimeSubscriber)
        {
            realTimeSubscriber.unsubscribe();
        }
        realTimeSubscriber = new kwwika.hubsubscriber.RealTimeSubscriber(user.PushTopic, htmlGenerator);
        realTimeSubscriber.subscribe();
    };

    function getUsersHubSubscriptions()
    {
        var listUrl = service.listUrl;
        $.ajax({
            url: listUrl,
            type: "POST",
            dataType: 'json',
            success: function (data, status)
            {
                if (status == "success")
                {
                    if (data.length == 0)
                    {
                        $("#no_feed_items_notice").fadeIn();
                    }
                    else
                    {
                        $.each(data, function (i, sub)
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
        htmlGenerator.addSubscription(sub, deleteSubscription);
    };

    function deleteSubscription()
    {
        if (!confirm("Are you sure you wish to delete this subscription?")) return false;

        var span = $(this);
        var id = span.attr("id");
        var subIdToDelete = id.substring(id.indexOf("_") + 1);

        var deleteUrl = service.deleteUrl;
        $.ajax({
            url: deleteUrl,
            data: { id: subIdToDelete },
            type: "POST",
            dataType: 'json',
            success: function (data, status)
            {
                if (status == "success")
                {
                    //                                NotAuthorised,
                    //                                Error,
                    //                                Success,
                    //                                NotFound
                    if (data.Type != 2)
                    {
                        alert(data.ErrorDescription);
                    }

                    if ((data.Type == 2 || data.Type == 3) &&
                                data.Subscription)
                    {
                        htmlGenerator.removeSubscription(data.Subscription);
                    }
                }
            }
        });
    }

    function init()
    {
        clearSubscriptionsUI();
        getUsersHubSubscriptions();
        getUserRealTimeSubscriptions();
    };

    function disableFormInputs(formId)
    {
        $("#" + formId + " input").attr("disabled", "disabled");
    };

    function enableFormInputs(formId)
    {
        $("#" + formId + " input").removeAttr("disabled");
    };

    function clearInputValuesOnSuccess(formId, status)
    {
        enableFormInputs(formId);
        if(status == "success")
        {
            $("#" + formId + " input[type=text],#" + formId + " input[type=password]").val("");
        }
    };

    function submitLogin()
    {
        var loginUrl = service.loginUrl;

        var formValues = $("#loginform").serialize();
        disableFormInputs("loginform");
        $.ajax({
            url: loginUrl,
            type: "POST",
            data: formValues,
            dataType: 'json',
            complete:function(req, status){clearInputValuesOnSuccess("loginform", status);},
            success: function (data, status)
            {
                if (status == "success")
                {
                    user = data;
                    if (user.Status == "LoggedIn")
                    {
                        $("#userinfo_wrapper .username").html(user.Username);

                        showUserInfo();

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
        var createUrl = service.createUrl;
        var track = $.trim($("#Track").val());

        if (track.match("^http:\/\/") == null)
        {
            track = "http://superfeedr.com/track/" + track.replace(" ", "&");
        }

        var subscriptionModel = {
            Topic: track
        };

        disableFormInputs("createsubscriptionform");
        var subscriptions = $.ajax({
            url: createUrl,
            type: "POST",
            data: subscriptionModel,
            dataType: 'json',
            complete:function(req, status){clearInputValuesOnSuccess("createsubscriptionform", status);},
            success: function (data, status)
            {
                if (status == "success")
                {
                    //                                NotAuthorised,
                    //                                Error,
                    //                                Success,
                    //                                NotFound
                    if (data.Type != 2)
                    {
                        alert(data.ErrorDescription);
                    }

                    if (data.Subscription)
                    {
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

    $("#logout").click(function ()
    {
        var logoutUrl = service.logoutUrl;
        var subscriptions = $.ajax({
            url: logoutUrl,
            type: "POST",
            dataType: 'json',
            success: function (data, status)
            {
                if (status == "success")
                {
                    user = data;
                    if (user.Status == "LoggedOut")
                    {
                        hideUserInfo();

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

    $("#loginform").validationEngine({
        success: submitLogin,
        unbindEngine: false,
        validationEventTriggers: "submit"
    });
    $("#createsubscriptionform").validationEngine({
        success: submitCreateSubscription,
        unbindEngine: false,
        validationEventTriggers: "submit"
    });

    $("a[href=#how_built]").toggle(
        function ()
        {
            $("#information_content").slideDown();
            $(this).addClass("opened");
        },
        function ()
        {
            $("#information_content").slideUp();
            $(this).removeClass("opened");
        }
    );

    /*$('a[href*=#]').click(function ()
    {
        if (location.pathname.replace(/^\//, '') == this.pathname.replace(/^\//, '')
        && location.hostname == this.hostname)
        {
            var $target = $(this.hash);
            $target = $target.length && $target || $('[name=' + this.hash.slice(1) + ']');
            if ($target.length)
            {
                var targetOffset = $target.offset().top;
                $('html,body') .animate({ scrollTop: targetOffset }, 1000);
                return false;
            }
        }
    });*/

    $("#loading").bind("ajaxSend", function(){
           $(this).show();
     }).bind("ajaxComplete", function(){
           $(this).hide();
     });

    init();
}); 