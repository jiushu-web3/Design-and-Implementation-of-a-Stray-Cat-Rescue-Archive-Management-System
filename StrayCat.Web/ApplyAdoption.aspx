<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ApplyAdoption.aspx.cs" Inherits="StrayCat.Web.ApplyAdoptionPage" MasterPageFile="~/Site.Master" %>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-head">
        <h2>申请领养</h2>
        <a class="primary-link" href="Adoptable.aspx">返回可领养猫</a>
    </div>
    <p class="hint">申请猫咪：<asp:Literal ID="CatLiteral" runat="server" Mode="PassThrough" /></p>
    <div class="form-grid" data-validate="true">
        <asp:Label ID="MessageLabel" runat="server" CssClass="form-message" data-form-message="true" />
        <label for="NameBox">姓名</label>
        <asp:TextBox ID="NameBox" runat="server" MaxLength="50" data-required="true" data-max="50" />
        <asp:RequiredFieldValidator ID="Val1" runat="server" ControlToValidate="NameBox" ErrorMessage="请填写必填项" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
        <label for="PhoneBox">联系电话</label>
        <asp:TextBox ID="PhoneBox" runat="server" MaxLength="11" data-required="true" data-phone="true" />
        <asp:RequiredFieldValidator ID="Val2" runat="server" ControlToValidate="PhoneBox" ErrorMessage="请填写必填项" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
        <asp:RegularExpressionValidator ID="Val3" runat="server" ControlToValidate="PhoneBox" ValidationExpression="^1[3-9]\d{9}$" ErrorMessage="请填写正确的手机号" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
        <label for="AddressBox">联系地址</label>
        <asp:TextBox ID="AddressBox" runat="server" MaxLength="200" data-required="true" data-max="200" />
        <asp:RequiredFieldValidator ID="Val4" runat="server" ControlToValidate="AddressBox" ErrorMessage="请填写必填项" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
        <label for="ReasonBox">申请理由</label>
        <asp:TextBox ID="ReasonBox" runat="server" TextMode="MultiLine" MaxLength="500" data-max="500" />
        <div>
            <asp:Button ID="SaveButton" runat="server" Text="提交申请" CssClass="primary" ValidationGroup="save" OnClick="SaveButton_Click" />
            <a class="secondary" href="Adoptable.aspx">返回</a>
        </div>
    </div>
</asp:Content>
