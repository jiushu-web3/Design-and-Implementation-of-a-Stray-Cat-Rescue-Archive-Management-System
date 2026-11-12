<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="FollowUpList.aspx.cs" Inherits="StrayCat.Web.FollowUpListPage" MasterPageFile="~/Site.Master" %>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <h2><asp:Literal ID="TitleLiteral" runat="server" Text="回访记录" /></h2>
    <asp:Label ID="MessageLabel" runat="server" CssClass="form-message" />
    <asp:Label ID="HintLabel" runat="server" CssClass="hint" />
    <asp:PlaceHolder ID="ListHolder" runat="server">
        <asp:HyperLink ID="AddLink" runat="server" CssClass="primary-link" Text="新增回访" />
        <asp:Label ID="EmptyLabel" runat="server" Text="暂无记录" Visible="false" CssClass="empty" />
        <asp:Repeater ID="ListRepeater" runat="server">
            <HeaderTemplate>
                <div class="list">
                <div class="list-row head"><span>档案</span><span>日期</span><span>方式</span><span>情况</span><span></span></div>
            </HeaderTemplate>
            <ItemTemplate>
                <div class="list-row">
                    <span><%#: Eval("CatNo") %> <%#: Eval("TempName") %></span>
                    <span><%#: Eval("FollowDateText") %></span>
                    <span><%#: Eval("FollowWay") %></span>
                    <span><%#: Eval("CatCondition") %></span>
                    <a href='<%# "FollowUpDetail.aspx?id=" + Eval("FollowUpId") %>'>查看</a>
                </div>
            </ItemTemplate>
            <FooterTemplate></div></FooterTemplate>
        </asp:Repeater>
        <p><asp:HyperLink ID="BackLink" runat="server" Text="返回领养" /></p>
    </asp:PlaceHolder>
</asp:Content>
