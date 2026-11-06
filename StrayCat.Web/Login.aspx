<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="StrayCat.Web.LoginPage" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <meta charset="utf-8" />
    <title>登录 - 流浪猫救助档案管理系统</title>
    <link rel="stylesheet" href="Content/css/site.css" />
</head>
<body class="gate">
    <form id="LoginForm" runat="server" data-validate="true">
        <div class="gate-card">
            <p class="brand-mark">档案</p>
            <h1>流浪猫救助档案管理系统</h1>
            <p class="gate-note">救助站内部使用</p>
            <asp:Label ID="MessageLabel" runat="server" CssClass="form-message" data-form-message="true" />
            <div class="gate-choice">
                <span><asp:RadioButton ID="UserChoice" runat="server" GroupName="LoginRole" Text="用户登录" /></span>
                <span><asp:RadioButton ID="AdminChoice" runat="server" GroupName="LoginRole" Text="管理员登录" /></span>
            </div>
            <label for="UserNameBox">用户名</label>
            <asp:TextBox ID="UserNameBox" runat="server" MaxLength="20" data-required="true" />
            <asp:RequiredFieldValidator ID="Val1" runat="server" ControlToValidate="UserNameBox" ErrorMessage="请填写用户名" Display="Dynamic" CssClass="field-error" />
            <label for="PasswordBox">密码</label>
            <asp:TextBox ID="PasswordBox" runat="server" TextMode="Password" MaxLength="20" data-required="true" />
            <asp:RequiredFieldValidator ID="Val2" runat="server" ControlToValidate="PasswordBox" ErrorMessage="请填写密码" Display="Dynamic" CssClass="field-error" />
            <asp:Button ID="LoginButton" runat="server" Text="登录" CssClass="primary" OnClick="LoginButton_Click" />
        </div>
    </form>
    <script src="Content/js/site.js"></script>
</body>
</html>
