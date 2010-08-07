kwwika.namespace("kwwika.hubsubscriber");

kwwika.hubsubscriber.HtmlGenerator = function()
{
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
    var topic = sub.Topic;
    var match = topic.match("(^http:\/\/superfeedr.com\/track\/)(.*)");
    if(match != null)
    {
        topic = match[2];
    }

    var keyword = $('<div id="subscription_' + sub.Id + '" class="keyword" title="' + topic + '">' + 
                            '<span>' +
                                (topic.length > 35?topic.substring(0, 35) + '...':topic) +                             
                            '</span>' +
                        '</div>');
    keyword.find("span").append(img);
    keyword.click(deleteClickHandler);
    return keyword;
};