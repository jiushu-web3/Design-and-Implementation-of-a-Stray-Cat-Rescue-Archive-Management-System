<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="UserList.aspx.cs" Inherits="StrayCat.Web.UserListPage" MasterPageFile="~/Site.Master" %>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <h2>用户管理</h2>
    <div class="filters">
        <div>
            <label for="KeywordBox">关键字</label>
            <asp:TextBox ID="KeywordBox" runat="server" MaxLength="50" />
        </div>
        <asp:Button ID="SearchButton" runat="server" Text="查询" CssClass="primary" CausesValidation="false" OnClick="SearchButton_Click" />
        <a class="primary-link" href="UserAdd.aspx">新建账号</a>
    </div>
    <asp:Label ID="MessageLabel" runat="server" CssClass="form-message" />
    <asp:Label ID="EmptyLabel" runat="server" Text="暂无记录" Visible="false" CssClass="empty" />
    <asp:Repeater ID="ListRepeater" runat="server" OnItemCommand="ListRepeater_ItemCommand">
        <HeaderTemplate><div class="list"></HeaderTemplate>
        <ItemTemplate>
            <div class="list-row">
                <span><%#: Eval("UserName") %></span>
                <span><%#: Eval("RealName") %></span>
                <span><%#: Eval("RoleName") %></span>
                <span><%#: Eval("Phone") %></span>
                <span><%#: Eval("EnabledText") %></span>
                <a href='<%# "UserEdit.aspx?id=" + Eval("UserId") %>'>编辑</a>
                <asp:LinkButton runat="server" CommandName="off" CommandArgument='<%# Eval("UserId") %>' Text="停用" CausesValidation="false" Visible='<%# (bool)Eval("IsEnabled") %>' />
                <asp:LinkButton runat="server" CommandName="on" CommandArgument='<%# Eval("UserId") %>' Text="启用" CausesValidation="false" Visible='<%# !(bool)Eval("IsEnabled") %>' />
            </div>
        </ItemTemplate>
        <FooterTemplate></div></FooterTemplate>
    </asp:Repeater>
    <asp:Literal ID="PagerLiteral" runat="server" Mode="PassThrough" />
</asp:Content>
