<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Stat.aspx.cs" Inherits="StrayCat.Web.StatPage" MasterPageFile="~/Site.Master" %>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <h2>数据统计</h2>
    <div class="filters">
        <div>
            <label for="YearList">年份</label>
            <asp:DropDownList ID="YearList" runat="server" AutoPostBack="true" OnSelectedIndexChanged="YearList_SelectedIndexChanged" />
        </div>
    </div>
    <asp:Repeater ID="MonthRepeater" runat="server">
        <HeaderTemplate>
            <div class="list">
                <div class="list-row"><span>月份</span><span>接收数</span><span>领养数</span></div>
        </HeaderTemplate>
        <ItemTemplate>
            <div class="list-row">
                <span><%# Eval("Month") %>月</span>
                <span><%# Eval("RescueCount") %></span>
                <span><%# Eval("AdoptionCount") %></span>
            </div>
        </ItemTemplate>
        <FooterTemplate></div></FooterTemplate>
    </asp:Repeater>
</asp:Content>
