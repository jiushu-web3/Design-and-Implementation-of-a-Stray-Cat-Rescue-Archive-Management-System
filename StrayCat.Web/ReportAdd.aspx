<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ReportAdd.aspx.cs" Inherits="StrayCat.Web.ReportAddPage" MasterPageFile="~/Site.Master" %>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <h2>上报流浪猫</h2>
    <p class="hint">填写发现信息，管理员审核后会联系你。</p>
    <div class="form-grid" data-validate="true">
        <asp:Label ID="MessageLabel" runat="server" CssClass="form-message" data-form-message="true" />
        <label for="PlaceBox">发现地点</label>
        <asp:TextBox ID="PlaceBox" runat="server" MaxLength="200" data-required="true" data-max="200" />
        <asp:RequiredFieldValidator ID="Val1" runat="server" ControlToValidate="PlaceBox" ErrorMessage="请填写必填项" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
        <label for="DateBox">发现日期</label>
        <asp:TextBox ID="DateBox" runat="server" TextMode="Date" data-required="true" />
        <asp:RequiredFieldValidator ID="Val2" runat="server" ControlToValidate="DateBox" ErrorMessage="请填写必填项" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
        <label for="DescBox">情况描述</label>
        <asp:TextBox ID="DescBox" runat="server" TextMode="MultiLine" MaxLength="500" data-max="500" />
        <label for="PhoneBox">联系电话</label>
        <asp:TextBox ID="PhoneBox" runat="server" MaxLength="11" data-required="true" data-phone="true" />
        <asp:RequiredFieldValidator ID="Val3" runat="server" ControlToValidate="PhoneBox" ErrorMessage="请填写必填项" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
        <asp:RegularExpressionValidator ID="Val4" runat="server" ControlToValidate="PhoneBox" ValidationExpression="^1[3-9]\d{9}$" ErrorMessage="请填写正确的手机号" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
        <label for="PhotoFile">照片</label>
        <asp:FileUpload ID="PhotoFile" runat="server" accept=".jpg,.jpeg,.png" />
        <div>
            <asp:Button ID="SaveButton" runat="server" Text="提交上报" CssClass="primary" ValidationGroup="save" OnClick="SaveButton_Click" />
            <a class="secondary" href="MyCenter.aspx">返回</a>
        </div>
    </div>
</asp:Content>
