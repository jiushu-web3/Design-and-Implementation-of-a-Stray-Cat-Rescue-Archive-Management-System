<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RescueDetail.aspx.cs" Inherits="StrayCat.Web.RescueDetailPage" MasterPageFile="~/Site.Master" %>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <h2>接收记录</h2>
    <asp:Label ID="MessageLabel" runat="server" CssClass="form-message" />
    <asp:PlaceHolder ID="DetailHolder" runat="server">
        <div class="detail"><asp:Literal ID="InfoLiteral" runat="server" Mode="PassThrough" /></div>
        <asp:HyperLink ID="EditLink" runat="server" CssClass="secondary" Text="修改接收记录" />
        <asp:HyperLink ID="BackLink" runat="server" CssClass="secondary" Text="返回档案" />
    </asp:PlaceHolder>
</asp:Content>
