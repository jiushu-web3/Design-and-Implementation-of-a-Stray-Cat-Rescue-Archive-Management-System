<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Stat.aspx.cs" Inherits="StrayCat.Web.StatPage" MasterPageFile="~/Site.Master" %>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <h2>数据统计</h2>
    <div class="filters">
        <div>
            <label for="YearList">年份</label>
            <asp:DropDownList ID="YearList" runat="server" AutoPostBack="true" OnSelectedIndexChanged="YearList_SelectedIndexChanged" />
        </div>
    </div>

    <%-- 汇总指标：本年度接收总量、领养总量、领养转化率 --%>
    <div class="cards">
        <div class="card">
            <span>本年接收</span><strong><asp:Literal ID="RescueTotalLiteral" runat="server" Text="0" /></strong>
        </div>
        <div class="card accent">
            <span>本年领养</span><strong><asp:Literal ID="AdoptionTotalLiteral" runat="server" Text="0" /></strong>
        </div>
        <div class="card accent">
            <span>领养率</span><strong><asp:Literal ID="RateLiteral" runat="server" Text="—" /></strong>
        </div>
    </div>

    <%-- 月度柱状图：由后台按数据生成内联 SVG，不依赖任何前端图表库 --%>
    <div class="chart-box">
        <div class="chart-head">
            <h3>月度接收 / 领养趋势</h3>
            <div class="chart-legend">
                <span><i class="rescue"></i>接收数</span>
                <span><i class="adopt"></i>领养数</span>
            </div>
        </div>
        <asp:Literal ID="ChartLiteral" runat="server" Mode="PassThrough" />
    </div>

    <h3>月度明细</h3>
    <asp:Repeater ID="MonthRepeater" runat="server">
        <HeaderTemplate>
            <div class="list">
                <div class="list-row head"><span>月份</span><span>接收数</span><span>领养数</span></div>
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
