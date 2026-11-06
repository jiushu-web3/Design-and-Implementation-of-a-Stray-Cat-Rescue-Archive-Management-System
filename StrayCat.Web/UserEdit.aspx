<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="UserEdit.aspx.cs" Inherits="StrayCat.Web.UserEditPage" MasterPageFile="~/Site.Master" %>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <h2>编辑账号</h2>
    <asp:Label ID="MessageLabel" runat="server" CssClass="form-message" data-form-message="true" />
    <asp:PlaceHolder ID="FormHolder" runat="server">
        <div class="form-grid" data-validate="true">
            <label for="UserNameBox">用户名</label>
            <asp:TextBox ID="UserNameBox" runat="server" ReadOnly="true" />
            <label for="RealNameBox">姓名</label>
            <asp:TextBox ID="RealNameBox" runat="server" MaxLength="50" data-required="true" data-max="50" />
            <asp:RequiredFieldValidator ID="Val1" runat="server" ControlToValidate="RealNameBox" ErrorMessage="请填写必填项" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
            <label for="PhoneBox">电话</label>
            <asp:TextBox ID="PhoneBox" runat="server" MaxLength="11" data-phone="true" />
            <asp:RegularExpressionValidator ID="Val2" runat="server" ControlToValidate="PhoneBox" ValidationExpression="^1[3-9]\d{9}$" ErrorMessage="请填写正确的手机号" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
            <label for="RoleList">角色</label>
            <asp:DropDownList ID="RoleList" runat="server" />
            <asp:CheckBox ID="EnabledBox" runat="server" Text="启用" />
            <label for="PasswordBox">重置密码</label>
            <asp:TextBox ID="PasswordBox" runat="server" TextMode="Password" MaxLength="20" data-rule="password" data-min="6" data-max="20" />
            <asp:RegularExpressionValidator ID="Val3" runat="server" ControlToValidate="PasswordBox" ValidationExpression="^[\s\S]{6,20}$" ErrorMessage="密码须为 6 到 20 位" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
            <label for="ConfirmBox">确认密码</label>
            <asp:TextBox ID="ConfirmBox" runat="server" TextMode="Password" MaxLength="20" />
            <asp:CompareValidator ID="Val4" runat="server" ControlToValidate="ConfirmBox" ControlToCompare="PasswordBox" ErrorMessage="两次输入的密码不一致" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
            <div>
                <asp:Button ID="SaveButton" runat="server" Text="保存" CssClass="primary" ValidationGroup="save" OnClick="SaveButton_Click" />
                <a class="secondary" href="UserList.aspx">返回</a>
            </div>
        </div>
    </asp:PlaceHolder>
</asp:Content>
