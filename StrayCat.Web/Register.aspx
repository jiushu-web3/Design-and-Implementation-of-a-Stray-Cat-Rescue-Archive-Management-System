<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="StrayCat.Web.RegisterPage" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <meta charset="utf-8" />
    <title>注册 - 流浪猫救助档案管理系统</title>
    <link rel="stylesheet" href="Content/css/site.css" />
</head>
<body class="gate">
    <form id="RegisterForm" runat="server" data-validate="true">
        <div class="gate-card">
            <p class="brand-mark">档案</p>
            <h1>注册工作人员账号</h1>
            <p class="gate-note">注册后使用“用户登录”。管理员账号由站内管理员创建。</p>
            <asp:Label ID="MessageLabel" runat="server" CssClass="form-message" data-form-message="true" />
            <label for="UserNameBox">用户名</label>
            <asp:TextBox ID="UserNameBox" runat="server" MaxLength="20" data-required="true" data-rule="username" data-min="4" data-max="20" />
            <asp:RequiredFieldValidator ID="Val1" runat="server" ControlToValidate="UserNameBox" ErrorMessage="请填写必填项" Display="Dynamic" CssClass="field-error" />
            <asp:RegularExpressionValidator ID="Val2" runat="server" ControlToValidate="UserNameBox" ValidationExpression="^[A-Za-z0-9]{4,20}$" ErrorMessage="用户名须为 4 到 20 位字母或数字" Display="Dynamic" CssClass="field-error" />
            <label for="PasswordBox">密码</label>
            <asp:TextBox ID="PasswordBox" runat="server" TextMode="Password" MaxLength="20" data-required="true" data-rule="password" data-min="6" data-max="20" />
            <asp:RequiredFieldValidator ID="Val3" runat="server" ControlToValidate="PasswordBox" ErrorMessage="请填写必填项" Display="Dynamic" CssClass="field-error" />
            <asp:RegularExpressionValidator ID="Val4" runat="server" ControlToValidate="PasswordBox" ValidationExpression="^[\s\S]{6,20}$" ErrorMessage="密码须为 6 到 20 位" Display="Dynamic" CssClass="field-error" />
            <label for="ConfirmBox">确认密码</label>
            <asp:TextBox ID="ConfirmBox" runat="server" TextMode="Password" MaxLength="20" data-required="true" data-compare="PasswordBox" />
            <asp:RequiredFieldValidator ID="Val5" runat="server" ControlToValidate="ConfirmBox" ErrorMessage="请填写必填项" Display="Dynamic" CssClass="field-error" />
            <asp:CompareValidator ID="Val6" runat="server" ControlToValidate="ConfirmBox" ControlToCompare="PasswordBox" ErrorMessage="两次输入的密码不一致" Display="Dynamic" CssClass="field-error" />
            <label for="RealNameBox">姓名</label>
            <asp:TextBox ID="RealNameBox" runat="server" MaxLength="50" data-required="true" data-max="50" />
            <asp:RequiredFieldValidator ID="Val7" runat="server" ControlToValidate="RealNameBox" ErrorMessage="请填写必填项" Display="Dynamic" CssClass="field-error" />
            <label for="PhoneBox">电话</label>
            <asp:TextBox ID="PhoneBox" runat="server" MaxLength="11" data-phone="true" />
            <asp:RegularExpressionValidator ID="Val8" runat="server" ControlToValidate="PhoneBox" ValidationExpression="^1[3-9]\d{9}$" ErrorMessage="请填写正确的手机号" Display="Dynamic" CssClass="field-error" />
            <label for="CaptchaBox">验证码</label>
            <div class="captcha-row">
                <asp:TextBox ID="CaptchaBox" runat="server" MaxLength="4" autocomplete="off" data-required="true" data-rule="captcha" data-min="4" data-max="4" />
                <asp:Image ID="CaptchaImage" runat="server" AlternateText="验证码" CssClass="captcha-image" />
                <a href="Captcha.aspx?scope=register" data-captcha-refresh="CaptchaImage">换一张</a>
            </div>
            <p class="hint">4 位数字或字母，区分大小写。点击图片或“换一张”可更换。</p>
            <asp:RequiredFieldValidator ID="Val9" runat="server" ControlToValidate="CaptchaBox" ErrorMessage="请填写验证码" Display="Dynamic" CssClass="field-error" />
            <asp:CustomValidator ID="Val10" runat="server" ControlToValidate="CaptchaBox" ErrorMessage="验证码错误" OnServerValidate="CaptchaCheck" Display="Dynamic" CssClass="field-error" EnableClientScript="false" />
            <asp:Button ID="RegisterButton" runat="server" Text="注册" CssClass="primary" OnClick="RegisterButton_Click" />
            <p class="gate-links"><a href="Login.aspx">已有账号？去登录</a></p>
        </div>
    </form>
    <script src="Content/js/site.js"></script>
</body>
</html>
