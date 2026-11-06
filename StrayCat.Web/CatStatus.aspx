<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CatStatus.aspx.cs" Inherits="StrayCat.Web.CatStatusPage" MasterPageFile="~/Site.Master" %>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <h2>变更状态</h2>
    <asp:Label ID="MessageLabel" runat="server" CssClass="form-message" data-form-message="true" />
    <asp:PlaceHolder ID="FormHolder" runat="server">
        <p>当前状态：<asp:Literal ID="StatusLiteral" runat="server" /></p>
        <asp:Panel ID="ToWaitingPanel" runat="server" Visible="false">
            <asp:Button ID="WaitingButton" runat="server" Text="改为待领养" CssClass="primary" CausesValidation="false" OnClick="WaitingButton_Click" />
        </asp:Panel>
        <asp:Panel ID="ToCarePanel" runat="server" Visible="false">
            <asp:Button ID="CareButton" runat="server" Text="改为在站照料" CssClass="primary" CausesValidation="false" OnClick="CareButton_Click" />
        </asp:Panel>
        <asp:Panel ID="ClosePanel" runat="server" Visible="false">
            <asp:Button ID="CloseButton" runat="server" Text="结案" CssClass="primary" CausesValidation="false" OnClick="CloseButton_Click" />
            <asp:Label ID="CloseHint" runat="server" CssClass="hint" />
        </asp:Panel>
        <asp:Panel ID="TerminatePanel" runat="server" Visible="false">
            <div class="form-grid" data-validate="true">
                <label for="ReasonBox">终止原因</label>
                <asp:TextBox ID="ReasonBox" runat="server" MaxLength="200" data-required="true" data-max="200" />
                <asp:RequiredFieldValidator ID="Val1" runat="server" ControlToValidate="ReasonBox" ErrorMessage="请填写终止原因" Display="Dynamic" CssClass="field-error" ValidationGroup="stop" />
                <asp:Button ID="TerminateButton" runat="server" Text="终止" CssClass="danger" ValidationGroup="stop" OnClick="TerminateButton_Click" />
            </div>
        </asp:Panel>
        <p><asp:HyperLink ID="BackLink" runat="server" Text="返回档案" /></p>
    </asp:PlaceHolder>
</asp:Content>
