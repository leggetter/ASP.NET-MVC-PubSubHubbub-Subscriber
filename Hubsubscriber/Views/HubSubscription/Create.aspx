<%@ Page Title="" Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage<HubSubscriber.Models.SubscriptionModel>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="main" runat="server">
    <% using (Html.BeginForm()) {%>
        <%: Html.ValidationSummary(true) %>
        <% if (ViewData["ErrorDescription"] != null)
           {%>
           <h2>Error:</h2>
           <div class="general-error"><%: ViewData["ErrorDescription"]%></div>
        <%} %>
        <fieldset>
            <legend>Fields</legend>
            
            <div class="editor-label">
                <%: Html.LabelFor(model => model.Topic) %>
            </div>
            <div class="editor-field">
                <%: Html.TextBoxFor(model => model.Topic) %>
                <%: Html.ValidationMessageFor(model => model.Topic) %>
            </div>
            
            <p>
                <input type="submit" value="Create" />
            </p>
        </fieldset>

    <% } %>

    <div>
        <%: Html.ActionLink("Back to List", "Index") %>
    </div>
</asp:Content>