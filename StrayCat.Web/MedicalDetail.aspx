<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MedicalDetail.aspx.cs" Inherits="StrayCat.Web.MedicalDetailPage" MasterPageFile="~/Site.Master" %>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <h2>医疗详情</h2>
    <asp:Label ID="MessageLabel" runat="server" CssClass="form-message" />
    <asp:PlaceHolder ID="DetailHolder" runat="server">
        <div class="detail"><asp:Literal ID="InfoLiteral" runat="server" Mode="PassThrough" /></div>
        <asp:HyperLink ID="EditLink" runat="server" CssClass="secondary" Text="修改" />
        <asp:Button ID="DeleteButton" runat="server" Text="删除" CssClass="danger" CausesValidation="false" OnClick="DeleteButton_Click" OnClientClick="return confirm('确定删除这条医疗记录吗？');" />
        <asp:HyperLink ID="BackLink" runat="server" CssClass="secondary" Text="返回列表" />
    </asp:PlaceHolder>
</asp:Content>
