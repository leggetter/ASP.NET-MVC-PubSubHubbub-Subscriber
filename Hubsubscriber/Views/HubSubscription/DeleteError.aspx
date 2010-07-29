<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="main" runat="server">

    <h2>DeleteError</h2>
    <div>
        <%= ViewData["ErrorDescription"] %>
    </div>

    <%:Html.ActionLink("Back to list", "Index") %>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="server">
</asp:Content>
