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
            <p class="gate-note">救助站工作人员与领养人入口</p>
            <asp:Label ID="MessageLabel" runat="server" CssClass="form-message" data-form-message="true" />
            <div class="gate-choice">
                <span><asp:RadioButton ID="UserChoice" runat="server" GroupName="LoginRole" Text="普通用户" /></span>
                <span><asp:RadioButton ID="AdminChoice" runat="server" GroupName="LoginRole" Text="管理员" /></span>
            </div>
            <label for="UserNameBox">用户名</label>
            <asp:TextBox ID="UserNameBox" runat="server" MaxLength="20" data-required="true" />
            <asp:RequiredFieldValidator ID="Val1" runat="server" ControlToValidate="UserNameBox" ErrorMessage="请填写用户名" Display="Dynamic" CssClass="field-error" />
            <label for="PasswordBox">密码</label>
            <asp:TextBox ID="PasswordBox" runat="server" TextMode="Password" MaxLength="20" data-required="true" />
            <asp:RequiredFieldValidator ID="Val2" runat="server" ControlToValidate="PasswordBox" ErrorMessage="请填写密码" Display="Dynamic" CssClass="field-error" />
            <label for="CaptchaBox">验证码</label>
            <div class="captcha-row">
                <asp:TextBox ID="CaptchaBox" runat="server" MaxLength="4" autocomplete="off" data-required="true" data-rule="captcha" data-min="4" data-max="4" />
                <asp:Image ID="CaptchaImage" runat="server" AlternateText="验证码" CssClass="captcha-image" />
                <a href="Captcha.aspx?scope=login" data-captcha-refresh="CaptchaImage">换一张</a>
            </div>
            <p class="hint">4 位数字或字母，区分大小写。点击图片或“换一张”可更换。</p>
            <asp:RequiredFieldValidator ID="Val3" runat="server" ControlToValidate="CaptchaBox" ErrorMessage="请填写验证码" Display="Dynamic" CssClass="field-error" />
            <asp:CustomValidator ID="Val4" runat="server" ControlToValidate="CaptchaBox" ErrorMessage="验证码错误" OnServerValidate="CaptchaCheck" Display="Dynamic" CssClass="field-error" EnableClientScript="false" />
            <asp:Button ID="LoginButton" runat="server" Text="登录" CssClass="primary" OnClick="LoginButton_Click" />
            <p class="gate-links"><a href="Register.aspx">没有账号？注册</a> · <a href="Adoptable.aspx">浏览待领养猫咪</a></p>
        </div>
    </form>
    <script src="Content/js/site.js"></script>
</body>
</html>
