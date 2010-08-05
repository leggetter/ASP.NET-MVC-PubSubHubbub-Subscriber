<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<HubSubscriber.Models.UserModel>" %>
<div class="user">
    <% if(Model.IsLoggedIn == false) { %>
        <p>You are using the default account which is shared between all users of the demo. With this account only 10 subscriptions can be
        active at any one time and other users can delete your subscriptions and add their own.</p>

        <% using (Html.BeginForm("Login", "HubSubscription")) {%>
            <%: Html.ValidationSummary(false) %>

            <fieldset>
                <legend>Login</legend>
            
                <div class="editor-label">
                    <%: Html.LabelFor(model => model.Username) %>
                </div>
                <div class="editor-field">
                    <%: Html.TextBoxFor(model => model.Username) %>
                    <%: Html.ValidationMessageFor(model => model.Username) %>
                </div>
            
                <div class="editor-label">
                    <%: Html.LabelFor(model => model.Password) %>
                </div>
                <div class="editor-field">
                    <%: Html.PasswordFor(model => model.Password) %>
                    <%: Html.ValidationMessageFor(model => model.Password) %>
                </div>
            
                <p>
                    <input type="submit" value="Login" />
                </p>
            </fieldset>

        <% } %>
    <% } else { %>
    Hello <span><%= Model.Username %></span> | <%: Html.ActionLink("Logout", "Logout") %>
    <% } %>
</div>