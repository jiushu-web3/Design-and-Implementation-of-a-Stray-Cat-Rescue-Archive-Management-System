<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ApplicationList.aspx.cs" Inherits="StrayCat.Web.ApplicationListPage" MasterPageFile="~/Site.Master" %>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <h2>领养申请审核</h2>
    <div class="filters">
        <div>
            <label for="StatusList">状态</label>
            <asp:DropDownList ID="StatusList" runat="server" />
        </div>
        <asp:Button ID="SearchButton" runat="server" Text="查询" CssClass="primary" CausesValidation="false" OnClick="SearchButton_Click" />
    </div>
    <asp:Label ID="MessageLabel" runat="server" CssClass="form-message" />
    <asp:Label ID="EmptyLabel" runat="server" Text="暂无记录" Visible="false" CssClass="empty" />
    <asp:Repeater ID="ApplicationRepeater" runat="server" OnItemCommand="ApplicationRepeater_ItemCommand">
        <HeaderTemplate>
            <div class="list">
                <div class="list-row head">
                    <span class="col-name">申请人</span>
                    <span class="col-time">联系方式</span>
                    <span class="col-name">领养地址</span>
                    <span class="col-name">申请理由</span>
                    <span class="col-name">猫咪</span>
                    <span class="col-time">申请时间</span>
                    <span class="col-status">状态</span>
                    <span>操作</span>
                </div>
        </HeaderTemplate>
        <ItemTemplate>
            <div class="list-row">
                <span class="col-name"><%#: Eval("ApplicantName") %></span>
                <span class="col-time"><%#: Eval("Phone") %></span>
                <span class="col-name" title='<%#: Eval("Address") %>'><%#: Eval("Address") %></span>
                <span class="col-name" title='<%#: ReasonText(Eval("Reason")) %>'><%#: ReasonText(Eval("Reason")) %></span>
                <span class="col-name"><%#: CatText(Eval("CatNo"), Eval("TempName")) %></span>
                <span class="col-time"><%#: TimeText(Eval("CreateTime")) %></span>
                <span class="col-status"><span class='tag <%# StatusTag(Eval("Status")) %>'><%#: StatusName(Eval("Status")) %></span></span>
                <span>
                    <asp:LinkButton ID="ApproveButton" runat="server" CommandName="approve" CommandArgument='<%# Eval("ApplicationId") %>' Visible='<%# IsPending(Eval("Status")) %>' Text="通过" CausesValidation="false" OnClientClick="return confirm('通过后将为该猫建立领养记录，确定通过吗？');" />
                    <asp:LinkButton ID="RejectButton" runat="server" CommandName="reject" CommandArgument='<%# Eval("ApplicationId") %>' Visible='<%# IsPending(Eval("Status")) %>' Text="驳回" CausesValidation="false" />
                </span>
            </div>
        </ItemTemplate>
        <FooterTemplate></div></FooterTemplate>
    </asp:Repeater>
    <asp:Literal ID="PagerLiteral" runat="server" Mode="PassThrough" />
    <asp:Panel ID="RejectPanel" runat="server" Visible="false" CssClass="form-grid">
        <asp:HiddenField ID="RejectId" runat="server" />
        <p class="hint">填写驳回理由后确认，理由会显示在用户的申请记录中。</p>
        <label for="ReasonBox">驳回理由</label>
        <asp:TextBox ID="ReasonBox" runat="server" MaxLength="200" data-max="200" />
        <div>
            <asp:Button ID="ConfirmRejectButton" runat="server" Text="确认驳回" CssClass="primary" CausesValidation="false" OnClick="ConfirmRejectButton_Click" />
            <asp:Button ID="CancelRejectButton" runat="server" Text="取消" CssClass="secondary" CausesValidation="false" OnClick="CancelRejectButton_Click" />
        </div>
    </asp:Panel>
</asp:Content>
