<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AdoptionList.aspx.cs" Inherits="StrayCat.Web.AdoptionListPage" MasterPageFile="~/Site.Master" %>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <h2>领养记录</h2>
    <asp:Label ID="MessageLabel" runat="server" CssClass="form-message" />
    <asp:PlaceHolder ID="ListHolder" runat="server">
        <asp:HyperLink ID="AddLink" runat="server" CssClass="primary-link" Text="登记领养" />
        <asp:Label ID="EmptyLabel" runat="server" Text="暂无记录" Visible="false" CssClass="empty" />
        <asp:Repeater ID="ListRepeater" runat="server">
            <HeaderTemplate><div class="list"></HeaderTemplate>
            <ItemTemplate>
                <div class="list-row">
                    <span><%#: Eval("AdoptDateText") %></span>
                    <span><%#: Eval("AdopterName") %></span>
                    <span><%#: Eval("AdopterPhone") %></span>
                    <span><%#: Eval("StatusName") %></span>
                    <a href='<%# "AdoptionDetail.aspx?id=" + Eval("AdoptionId") %>'>查看</a>
                </div>
            </ItemTemplate>
            <FooterTemplate></div></FooterTemplate>
        </asp:Repeater>
        <p><asp:HyperLink ID="BackLink" runat="server" Text="返回档案" /></p>
    </asp:PlaceHolder>
</asp:Content>
