<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="UserAdd.aspx.cs" Inherits="StrayCat.Web.UserAddPage" MasterPageFile="~/Site.Master" %>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <h2>新建账号</h2>
    <div class="form-grid" data-validate="true">
        <asp:Label ID="MessageLabel" runat="server" CssClass="form-message" data-form-message="true" />
        <label for="UserNameBox">用户名</label>
        <asp:TextBox ID="UserNameBox" runat="server" MaxLength="20" data-required="true" data-rule="username" data-min="4" data-max="20" />
        <asp:RequiredFieldValidator ID="Val1" runat="server" ControlToValidate="UserNameBox" ErrorMessage="请填写必填项" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
        <asp:RegularExpressionValidator ID="Val2" runat="server" ControlToValidate="UserNameBox" ValidationExpression="^[A-Za-z0-9]{4,20}$" ErrorMessage="用户名须为 4 到 20 位字母或数字" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
        <label for="PasswordBox">密码</label>
        <asp:TextBox ID="PasswordBox" runat="server" TextMode="Password" MaxLength="20" data-required="true" data-rule="password" data-min="6" data-max="20" />
        <asp:RequiredFieldValidator ID="Val3" runat="server" ControlToValidate="PasswordBox" ErrorMessage="请填写必填项" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
        <asp:RegularExpressionValidator ID="Val4" runat="server" ControlToValidate="PasswordBox" ValidationExpression="^[\s\S]{6,20}$" ErrorMessage="密码须为 6 到 20 位" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
        <label for="RealNameBox">姓名</label>
        <asp:TextBox ID="RealNameBox" runat="server" MaxLength="50" data-required="true" data-max="50" />
        <asp:RequiredFieldValidator ID="Val5" runat="server" ControlToValidate="RealNameBox" ErrorMessage="请填写必填项" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
        <label for="PhoneBox">电话</label>
        <asp:TextBox ID="PhoneBox" runat="server" MaxLength="11" data-phone="true" />
        <asp:RegularExpressionValidator ID="Val6" runat="server" ControlToValidate="PhoneBox" ValidationExpression="^1[3-9]\d{9}$" ErrorMessage="请填写正确的手机号" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
        <label for="RoleList">角色</label>
        <asp:DropDownList ID="RoleList" runat="server" />
        <asp:CheckBox ID="EnabledBox" runat="server" Text="启用" Checked="true" />
        <div>
            <asp:Button ID="SaveButton" runat="server" Text="保存" CssClass="primary" ValidationGroup="save" OnClick="SaveButton_Click" />
            <a class="secondary" href="UserList.aspx">返回</a>
        </div>
    </div>
</asp:Content>
