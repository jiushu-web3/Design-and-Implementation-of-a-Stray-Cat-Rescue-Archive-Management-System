<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="StrayCat.Web.DefaultPage" MasterPageFile="~/Site.Master" %>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Label ID="NoticeLabel" runat="server" CssClass="form-message" />
    <h2>工作台</h2>
    <div class="cards">
        <div class="card"><span>在站照料</span><strong><asp:Literal ID="InCareLiteral" runat="server" Text="0" /></strong></div>
        <div class="card"><span>待领养</span><strong><asp:Literal ID="WaitingLiteral" runat="server" Text="0" /></strong></div>
        <div class="card"><span>已领养未回访</span><strong><asp:Literal ID="FollowLiteral" runat="server" Text="0" /></strong></div>
        <asp:PlaceHolder ID="UserCountHolder" runat="server" Visible="false">
            <div class="card"><span>启用账号</span><strong><asp:Literal ID="UserCountLiteral" runat="server" Text="0" /></strong></div>
        </asp:PlaceHolder>
    </div>
    <h3>最近档案</h3>
    <asp:Label ID="EmptyLabel" runat="server" Text="暂无记录" Visible="false" CssClass="empty" />
    <asp:Repeater ID="RecentRepeater" runat="server">
        <HeaderTemplate><div class="list"></HeaderTemplate>
        <ItemTemplate>
            <div class="list-row">
                <span class="col-no"><%#: Eval("CatNo") %></span>
                <span class="col-name"><%#: Eval("TempName") %></span>
                <span class="col-status"><span class="tag tag-<%#: Eval("Status") %>"><%#: Eval("StatusName") %></span></span>
                <span class="col-time"><%#: Eval("RescueTimeText") %></span>
            </div>
        </ItemTemplate>
        <FooterTemplate></div></FooterTemplate>
    </asp:Repeater>
</asp:Content>
