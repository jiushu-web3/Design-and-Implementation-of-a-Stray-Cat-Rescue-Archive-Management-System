<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ChangePassword.aspx.cs" Inherits="StrayCat.Web.ChangePasswordPage" MasterPageFile="~/Site.Master" %>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <h2>修改密码</h2>
    <div class="form-grid" data-validate="true">
        <asp:Label ID="MessageLabel" runat="server" CssClass="form-message" data-form-message="true" />
        <label for="OldBox">原密码</label>
        <asp:TextBox ID="OldBox" runat="server" TextMode="Password" MaxLength="20" data-required="true" />
        <asp:RequiredFieldValidator ID="Val1" runat="server" ControlToValidate="OldBox" ErrorMessage="请填写必填项" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
        <label for="NewBox">新密码</label>
        <asp:TextBox ID="NewBox" runat="server" TextMode="Password" MaxLength="20" data-required="true" data-rule="password" data-min="6" data-max="20" />
        <asp:RequiredFieldValidator ID="Val2" runat="server" ControlToValidate="NewBox" ErrorMessage="请填写必填项" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
        <asp:RegularExpressionValidator ID="Val3" runat="server" ControlToValidate="NewBox" ValidationExpression="^[\s\S]{6,20}$" ErrorMessage="密码须为 6 到 20 位" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
        <label for="ConfirmBox">确认新密码</label>
        <asp:TextBox ID="ConfirmBox" runat="server" TextMode="Password" MaxLength="20" data-required="true" />
        <asp:RequiredFieldValidator ID="Val4" runat="server" ControlToValidate="ConfirmBox" ErrorMessage="请填写必填项" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
        <asp:CompareValidator ID="Val5" runat="server" ControlToValidate="ConfirmBox" ControlToCompare="NewBox" ErrorMessage="两次输入的密码不一致" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
        <asp:Button ID="SaveButton" runat="server" Text="保存" CssClass="primary" ValidationGroup="save" OnClick="SaveButton_Click" />
    </div>
</asp:Content>
