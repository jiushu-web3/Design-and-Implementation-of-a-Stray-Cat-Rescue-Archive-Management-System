<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CatDetail.aspx.cs" Inherits="StrayCat.Web.CatDetailPage" MasterPageFile="~/Site.Master" %>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <h2>档案详情</h2>
    <asp:Label ID="MessageLabel" runat="server" CssClass="form-message" />
    <asp:PlaceHolder ID="DetailHolder" runat="server">
        <%-- 照片在左、信息在右；无照片时 Image 不渲染，右侧自动占满 --%>
        <div class="split detail-split">
            <asp:Image ID="PhotoImage" runat="server" CssClass="photo" Visible="false" AlternateText="猫咪照片" />
            <div class="main-col">
                <div class="detail">
                    <asp:Literal ID="InfoLiteral" runat="server" Mode="PassThrough" />
                </div>
            </div>
        </div>

        <%-- 操作区：首个可见按钮自动成为主操作，删除单独靠右 --%>
        <div class="action-bar">
            <asp:HyperLink ID="EditLink" runat="server" CssClass="secondary" Text="修改档案" />
            <asp:HyperLink ID="RescueLink" runat="server" CssClass="secondary" Text="接收记录" />
            <asp:HyperLink ID="StatusLink" runat="server" CssClass="secondary" Text="变更状态" />
            <asp:HyperLink ID="MedicalLink" runat="server" CssClass="secondary" Text="医疗记录" />
            <asp:HyperLink ID="AddMedicalLink" runat="server" CssClass="secondary" Text="新增医疗" />
            <asp:HyperLink ID="AdoptionLink" runat="server" CssClass="secondary" Text="领养记录" />
            <asp:HyperLink ID="AddAdoptionLink" runat="server" CssClass="secondary" Text="登记领养" />
            <asp:Button ID="DeleteButton" runat="server" Text="删除档案" CssClass="danger" CausesValidation="false" OnClick="DeleteButton_Click" OnClientClick="return confirm('确定删除这条档案吗？');" />
        </div>

        <h3>最近医疗</h3>
        <asp:Label ID="MedicalEmptyLabel" runat="server" Text="暂无记录" Visible="false" CssClass="empty" />
        <asp:Repeater ID="MedicalRepeater" runat="server">
            <HeaderTemplate><div class="list"></HeaderTemplate>
            <ItemTemplate>
                <div class="list-row">
                    <span class="col-time"><%#: Eval("MedicalDateText") %></span>
                    <span class="col-name"><%#: Eval("Diagnosis") %></span>
                    <a href='<%# "MedicalDetail.aspx?id=" + Eval("MedicalId") %>'>查看</a>
                </div>
            </ItemTemplate>
            <FooterTemplate></div></FooterTemplate>
        </asp:Repeater>
        <h3>领养历史</h3>
        <asp:Label ID="AdoptionEmptyLabel" runat="server" Text="暂无记录" Visible="false" CssClass="empty" />
        <asp:Repeater ID="AdoptionRepeater" runat="server">
            <HeaderTemplate><div class="list"></HeaderTemplate>
            <ItemTemplate>
                <div class="list-row">
                    <span class="col-time"><%#: Eval("AdoptDateText") %></span>
                    <span class="col-name"><%#: Eval("AdopterName") %></span>
                    <span class="col-gender"><%#: Eval("StatusName") %></span>
                    <a href='<%# "AdoptionDetail.aspx?id=" + Eval("AdoptionId") %>'>查看</a>
                </div>
            </ItemTemplate>
            <FooterTemplate></div></FooterTemplate>
        </asp:Repeater>
    </asp:PlaceHolder>
</asp:Content>
