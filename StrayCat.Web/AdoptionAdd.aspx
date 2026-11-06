<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AdoptionAdd.aspx.cs" Inherits="StrayCat.Web.AdoptionAddPage" MasterPageFile="~/Site.Master" %>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <h2>登记领养</h2>
    <asp:Label ID="MessageLabel" runat="server" CssClass="form-message" data-form-message="true" />
    <asp:PlaceHolder ID="FormHolder" runat="server">
        <div class="form-grid" data-validate="true">
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
            <label for="DateBox">领养日期</label>
            <asp:TextBox ID="DateBox" runat="server" TextMode="Date" data-required="true" />
            <asp:RequiredFieldValidator ID="Val5" runat="server" ControlToValidate="DateBox" ErrorMessage="请填写必填项" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
            <label for="RemarkBox">备注</label>
            <asp:TextBox ID="RemarkBox" runat="server" TextMode="MultiLine" MaxLength="500" data-max="500" />
            <div>
                <asp:Button ID="SaveButton" runat="server" Text="保存" CssClass="primary" ValidationGroup="save" OnClick="SaveButton_Click" />
                <asp:HyperLink ID="BackLink" runat="server" CssClass="secondary" Text="返回" />
            </div>
        </div>
    </asp:PlaceHolder>
</asp:Content>
