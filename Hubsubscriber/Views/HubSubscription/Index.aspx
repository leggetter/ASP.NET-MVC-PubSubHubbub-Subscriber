<%@ Page Title="" Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage<IEnumerable<HubSubscriber.Models.SubscriptionModel>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="server">
    Subscription List
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="main" runat="server">
 <% if (ViewData["ErrorDescription"] != null)
           {%>
           <h2>Error:</h2>
           <div class="general-error"><%: ViewData["ErrorDescription"]%></div>
        <%} %>
        <h2>Subscription List</h2>
<table>
        <tr>
            <th></th>
            <th>
                Id
            </th>
            <th>
                Callback
            </th>
            <th>
                Topic
            </th>
            <th>
                Last Updated
            </th>
        </tr>

    <% foreach (var item in Model) { %>
    
        <tr>
            <td>
                <%: Html.ActionLink("Delete", "Delete", new { id = item.Id })%>
            </td>
            <td>
                <%: item.Id %>
            </td>
            <td>
                <%: item.Callback %>
            </td>
            <td>
                <%: item.Topic %>
            </td>
            <td>
                <%: item.LastUpdated.HasValue?item.LastUpdated.Value.ToString("MM/dd/yyyy hh:mm tt"):"N/A" %>
            </td>
        </tr>
    
    <% } %>

    </table>

    <p>
        <%: Html.ActionLink("Create New", "Create") %>
    </p>
</asp:Content>