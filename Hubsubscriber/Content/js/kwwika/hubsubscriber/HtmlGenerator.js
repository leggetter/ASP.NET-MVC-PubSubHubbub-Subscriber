kwwika.namespace("kwwika.hubsubscriber");

kwwika.hubsubscriber.HtmlGenerator = function(feedItemsParentId, subscriptionListId, deleteCallback)
{
    this.feedItemsParentId = feedItemsParentId;
    this.subscriptionListId = subscriptionListId;
    this.deleteCallback = deleteCallback;
    this.maxNewsItems = 20;

    this.showingInfoMessage = false;
    this.queuedInfoMessages = [];
};

kwwika.hubsubscriber.HtmlGenerator.prototype.createItemElement = function(update)
{
    var title = update.entryTitle;
    var content = update.entryContent;
    var link = update.entryLinkAlternate;
    var topic = update.subscriptionTopic || "";

    var hasLink = (link && link != "=");

    var html = '<div class="topic-box">' +
                    '<div class="topic-title">';
                    if(hasLink)
                    {
                        html += 
                        '<div class="topic-link">' +                                    
                            '<div class="link-arrow">' + 
                                '<a target="_blank" href="' + link + '" title="' + link + '">' + 
                                    (link.length > 27? link.substring(0, 27) + "...":link) + 
                                '</a>' + 
                            '</div>' +
                        '</div>';                            
                    }
                    html +=
	                    '<span class="topic-tri">Tracking:</span>' +
                        '<span class="topic-tracking" title="' + topic + '">' +
                            (topic.length > 65? topic.substring(0, 65) + "...":topic) +
                        '</span>' +
                    '</div>' +
                    '<div class="topic-content">' + 
                        '<h3 class="topic-content-title">';
                        if(hasLink)
                        {
                            html +=
                            '<a target="_blank" href="' + link + '">' + 
                                title +
                            '</a>'; 
                        }
                        html +=
                        '</h3>' + 
                        '<div class="topic-content-body">' + content +'</div>' +
                    '</div>' +
                '</div>';
    return $(html);
};

kwwika.hubsubscriber.HtmlGenerator.prototype.createDeleteButtonElement  = function(sub)
{
    return $('<img src="/Content/Images/delete.png" width="14" height="14" alt="Delete subscription" />');
};

kwwika.hubsubscriber.HtmlGenerator.prototype.createKeywordElement = function(sub, deleteClickHandler)
{
    var img = this.createDeleteButtonElement(sub);    
    var topic = this.getUrlOrKeyword(sub.Topic);    

    var keyword = $('<div id="subscription_' + sub.Id + '" class="keyword" title="' + topic + '">' + 
                            '<span>' +
                                (topic.length > 35?topic.substring(0, 35) + '...':topic) +                             
                            '</span>' +
                        '</div>');
    keyword.find("span").append(img);
    keyword.click(deleteClickHandler);
    return keyword;
};

kwwika.hubsubscriber.HtmlGenerator.prototype.getUrlOrKeyword = function(topic)
{
    var match = topic.match("(^http:\/\/superfeedr.com\/track\/)(.*)");
    if(match != null)
    {
        topic = match[2];
    }
    return topic;
};

kwwika.hubsubscriber.HtmlGenerator.prototype.updateReceived  = function(update)
{
    var newsItem = this.createItemElement(update);
    newsItem.hide();
    $("#" + this.feedItemsParentId).prepend(newsItem);
    newsItem.slideDown();
    //$("#feed_updated").html(update.feedUpdated);

    var newsItems = $("#" + this.feedItemsParentId + " .topic-box");
    if(newsItems.size() > this.maxNewsItems)
    {
        newsItems.last().remove();
    }
};

kwwika.hubsubscriber.HtmlGenerator.prototype.addSubscription  = function(sub)
{
    if( $("#subscription_" + sub.Id).size() == 0 )
    {
        var subEl = this.createKeywordElement(sub, this.deleteCallback);
        subEl.hide();
        $("#" + this.subscriptionListId).append(subEl);
        subEl.fadeIn();
        
        $("#no_feed_items_notice").hide();
        
        this.showInfoMessage("Subscription added for \"" + this.getUrlOrKeyword(sub.Topic) + "\"");
    }
};

kwwika.hubsubscriber.HtmlGenerator.prototype.removeSubscription  = function(sub)
{
    var els = $("#subscription_" + sub.Id);
    var htmlGenerator = this;
    if( els.size() == 1 )
    {        
        els.fadeOut(function ()
        {
            $(this).remove();
            if ($("#" + htmlGenerator.subscriptionListId + " .keyword").size() == 0)
            {
                $("#no_feed_items_notice").show();
            }
        });
        this.showInfoMessage("Subscription removed for \"" + this.getUrlOrKeyword(sub.Topic) + "\"");
    }
};

kwwika.hubsubscriber.HtmlGenerator.prototype.showInfoMessage = function(message)
{
    this.queuedInfoMessages.push(message);

    this.showNextInfoMessage();
};

kwwika.hubsubscriber.HtmlGenerator.prototype.showNextInfoMessage = function()
{
    if(!this.showingInfoMessage &&
        this.queuedInfoMessages.length > 0)
    {
        this.showingInfoMessage = true;
        
        var message = this.queuedInfoMessages.shift();
        var _this = this;

        var el = $("#info_message");
        el.text(message)
        el.fadeIn().delay(1000).fadeOut(function()
        {
            _this.showingInfoMessage = false;
            _this.showNextInfoMessage();
        });
    }
};