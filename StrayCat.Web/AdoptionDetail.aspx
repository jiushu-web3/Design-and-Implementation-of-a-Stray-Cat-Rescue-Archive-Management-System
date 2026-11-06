<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AdoptionDetail.aspx.cs" Inherits="StrayCat.Web.AdoptionDetailPage" MasterPageFile="~/Site.Master" %>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <h2>领养详情</h2>
    <asp:Label ID="MessageLabel" runat="server" CssClass="form-message" />
    <asp:PlaceHolder ID="DetailHolder" runat="server">
        <div class="detail"><asp:Literal ID="InfoLiteral" runat="server" Mode="PassThrough" /></div>
        <asp:HyperLink ID="EditLink" runat="server" CssClass="secondary" Text="修改或退养" />
        <asp:HyperLink ID="FollowLink" runat="server" CssClass="secondary" Text="回访记录" />
        <asp:HyperLink ID="AddFollowLink" runat="server" CssClass="secondary" Text="新增回访" />
        <asp:Button ID="DeleteButton" runat="server" Text="删除领养" CssClass="danger" CausesValidation="false" OnClick="DeleteButton_Click" OnClientClick="return confirm('确定删除这条领养记录吗？');" />
        <asp:HyperLink ID="BackLink" runat="server" CssClass="secondary" Text="返回档案" />
    </asp:PlaceHolder>
</asp:Content>
