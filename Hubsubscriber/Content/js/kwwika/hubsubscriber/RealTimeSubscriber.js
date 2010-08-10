kwwika.namespace("kwwika.hubsubscriber");

kwwika.hubsubscriber.RealTimeSubscriber = function(realTimeTopic, htmlGenerator)
{
    this.realTimeTopic = realTimeTopic;   
    this.htmlGenerator = htmlGenerator;
};
kwwika.hubsubscriber.RealTimeSubscriber.prototype.subscribe = function()
{
    this.connection = kwwika.Service.connect();
    this.newsSubscription = this.connection.subscribe(this.realTimeTopic, this);
};

kwwika.hubsubscriber.RealTimeSubscriber.prototype.topicUpdated = function (sub, update)
{
    if( this.isValidSubscription(update.subscriptionCreated) )
    {
        var model = JSON.parse(update.subscriptionCreated);
        this.htmlGenerator.addSubscription(model);
    }
    else if( this.isValidSubscription(update.subscriptionDeleted) )
    {
        var model = JSON.parse(update.subscriptionDeleted);
        this.htmlGenerator.removeSubscription(model);
    }
    
    if(update.entryTitle)
    {
        this.htmlGenerator.updateReceived(update);
    }
};

kwwika.hubsubscriber.RealTimeSubscriber.prototype.topicError = function (sub, error)
{
    alert("Error subscribing to " + sub.topicName + ": " + error);
};

kwwika.hubsubscriber.RealTimeSubscriber.prototype.unsubscribe = function()
{
    this.connection.unsubscribe(this.newsSubscription);
};

kwwika.hubsubscriber.RealTimeSubscriber.prototype.isValidSubscription = function(sub)
{
    return sub && sub != "null";
};