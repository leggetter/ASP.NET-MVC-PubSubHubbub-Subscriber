kwwika.namespace("kwwika.hubsubscriber");

kwwika.hubsubscriber.RealTimeSubscriber = function(realTimeTopic, feedItemsParentId, htmlGenerator)
{
    this.realTimeTopic = realTimeTopic;
    this.feedItemsParentId = feedItemsParentId;
    this.htmlGenerator = htmlGenerator;
    this.maxNewsItems = 20;
};
kwwika.hubsubscriber.RealTimeSubscriber.prototype.subscribe = function()
{
    this.connection = kwwika.Service.connect();
    this.newsSubscription = this.connection.subscribe(this.realTimeTopic, this);
};

kwwika.hubsubscriber.RealTimeSubscriber.prototype.topicUpdated = function (sub, update)
{
    var newsItem = this.createNewsItem(update);
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

kwwika.hubsubscriber.RealTimeSubscriber.prototype.topicError = function (sub, error)
{
    alert("Error subscribing to " + sub.topicName + ": " + error);
};

kwwika.hubsubscriber.RealTimeSubscriber.prototype.createNewsItem = function(update)
{
    return this.htmlGenerator.createItemElement(update);
};

kwwika.hubsubscriber.RealTimeSubscriber.prototype.unsubscribe = function()
{
    this.connection.unsubscribe(this.newsSubscription);
};