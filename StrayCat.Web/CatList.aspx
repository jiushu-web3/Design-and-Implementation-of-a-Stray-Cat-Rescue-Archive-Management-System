<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CatList.aspx.cs" Inherits="StrayCat.Web.CatListPage" MasterPageFile="~/Site.Master" %>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-head">
        <h2>猫咪档案</h2>
        <a class="primary-link" href="CatAdd.aspx">新增档案</a>
    </div>
    <div class="filters">
        <div>
            <label for="KeywordBox">编号或名字</label>
            <asp:TextBox ID="KeywordBox" runat="server" MaxLength="50" />
        </div>
        <div>
            <label for="StatusList">状态</label>
            <asp:DropDownList ID="StatusList" runat="server" />
        </div>
        <div>
            <label for="GenderList">性别</label>
            <asp:DropDownList ID="GenderList" runat="server" />
        </div>
        <div>
            <label for="FromBox">接收从</label>
            <asp:TextBox ID="FromBox" runat="server" TextMode="Date" />
        </div>
        <div>
            <label for="ToBox">接收到</label>
            <asp:TextBox ID="ToBox" runat="server" TextMode="Date" />
        </div>
        <asp:Button ID="SearchButton" runat="server" Text="查询" CssClass="primary" CausesValidation="false" OnClick="SearchButton_Click" />
    </div>
    <asp:Label ID="MessageLabel" runat="server" CssClass="form-message" />
    <asp:Label ID="EmptyLabel" runat="server" Text="暂无记录" Visible="false" CssClass="empty" />
    <asp:Repeater ID="ListRepeater" runat="server">
        <HeaderTemplate><div class="list"></HeaderTemplate>
        <ItemTemplate>
            <div class="list-row">
                <span class="col-no"><%#: Eval("CatNo") %></span>
                <span class="col-name"><%#: Eval("TempName") %></span>
                <span class="col-gender"><%#: Eval("GenderName") %></span>
                <span class="col-status"><span class="tag tag-<%#: Eval("Status") %>"><%#: Eval("StatusName") %></span></span>
                <span class="col-time"><%#: Eval("RescueTimeText") %></span>
                <a href='<%# "CatDetail.aspx?id=" + Eval("CatId") %>'>查看</a>
            </div>
        </ItemTemplate>
        <FooterTemplate></div></FooterTemplate>
    </asp:Repeater>
    <asp:Literal ID="PagerLiteral" runat="server" Mode="PassThrough" />
</asp:Content>
