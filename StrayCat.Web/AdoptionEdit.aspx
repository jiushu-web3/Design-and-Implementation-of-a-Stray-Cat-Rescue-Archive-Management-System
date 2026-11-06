<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AdoptionEdit.aspx.cs" Inherits="StrayCat.Web.AdoptionEditPage" MasterPageFile="~/Site.Master" %>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <h2>领养办理</h2>
    <asp:Label ID="MessageLabel" runat="server" CssClass="form-message" data-form-message="true" />
    <asp:PlaceHolder ID="FormHolder" runat="server">
        <div class="form-grid" data-validate="true">
            <p>领养日期：<asp:Literal ID="DateLiteral" runat="server" /></p>
            <label for="NameBox">领养人姓名</label>
            <asp:TextBox ID="NameBox" runat="server" MaxLength="50" data-required="true" data-max="50" />
            <asp:RequiredFieldValidator ID="Val1" runat="server" ControlToValidate="NameBox" ErrorMessage="请填写必填项" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
            <label for="PhoneBox">手机号</label>
            <asp:TextBox ID="PhoneBox" runat="server" MaxLength="11" data-required="true" data-phone="true" />
            <asp:RequiredFieldValidator ID="Val2" runat="server" ControlToValidate="PhoneBox" ErrorMessage="请填写必填项" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
            <asp:RegularExpressionValidator ID="Val3" runat="server" ControlToValidate="PhoneBox" ValidationExpression="^1[3-9]\d{9}$" ErrorMessage="请填写正确的手机号" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
            <label for="AddressBox">住址</label>
            <asp:TextBox ID="AddressBox" runat="server" MaxLength="200" data-required="true" data-max="200" />
            <asp:RequiredFieldValidator ID="Val4" runat="server" ControlToValidate="AddressBox" ErrorMessage="请填写必填项" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
            <label for="RemarkBox">备注</label>
            <asp:TextBox ID="RemarkBox" runat="server" TextMode="MultiLine" MaxLength="500" data-max="500" />
            <asp:Button ID="SaveButton" runat="server" Text="保存联系信息" CssClass="primary" ValidationGroup="save" OnClick="SaveButton_Click" />
        </div>
        <asp:Panel ID="ReturnPanel" runat="server" Visible="false">
            <div class="form-grid" data-validate="true">
                <h3>办理退养</h3>
                <label for="ReturnDateBox">退养日期</label>
                <asp:TextBox ID="ReturnDateBox" runat="server" TextMode="Date" data-required="true" />
                <asp:RequiredFieldValidator ID="Val5" runat="server" ControlToValidate="ReturnDateBox" ErrorMessage="请填写必填项" Display="Dynamic" CssClass="field-error" ValidationGroup="back" />
                <label for="ReturnReasonBox">退养原因</label>
                <asp:TextBox ID="ReturnReasonBox" runat="server" MaxLength="200" data-required="true" data-max="200" />
                <asp:RequiredFieldValidator ID="Val6" runat="server" ControlToValidate="ReturnReasonBox" ErrorMessage="请填写必填项" Display="Dynamic" CssClass="field-error" ValidationGroup="back" />
                <asp:Button ID="ReturnButton" runat="server" Text="退养" CssClass="danger" ValidationGroup="back" OnClick="ReturnButton_Click" />
            </div>
        </asp:Panel>
        <p><asp:HyperLink ID="BackLink" runat="server" Text="返回" /></p>
    </asp:PlaceHolder>
</asp:Content>
