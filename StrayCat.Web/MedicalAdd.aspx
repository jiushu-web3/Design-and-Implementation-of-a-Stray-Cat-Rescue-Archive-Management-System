<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MedicalAdd.aspx.cs" Inherits="StrayCat.Web.MedicalAddPage" MasterPageFile="~/Site.Master" %>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <h2>新增医疗</h2>
    <asp:Label ID="MessageLabel" runat="server" CssClass="form-message" data-form-message="true" />
    <asp:PlaceHolder ID="FormHolder" runat="server">
        <div class="form-grid" data-validate="true">
            <label for="DateBox">医疗日期</label>
            <asp:TextBox ID="DateBox" runat="server" TextMode="Date" data-required="true" />
            <asp:RequiredFieldValidator ID="Val1" runat="server" ControlToValidate="DateBox" ErrorMessage="请填写必填项" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
            <label for="DiagnosisBox">诊断</label>
            <asp:TextBox ID="DiagnosisBox" runat="server" MaxLength="200" data-required="true" data-max="200" />
            <asp:RequiredFieldValidator ID="Val2" runat="server" ControlToValidate="DiagnosisBox" ErrorMessage="请填写必填项" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
            <label for="TreatmentBox">处理</label>
            <asp:TextBox ID="TreatmentBox" runat="server" TextMode="MultiLine" MaxLength="500" data-required="true" data-max="500" />
            <asp:RequiredFieldValidator ID="Val3" runat="server" ControlToValidate="TreatmentBox" ErrorMessage="请填写必填项" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
            <label for="MedicineBox">用药</label>
            <asp:TextBox ID="MedicineBox" runat="server" MaxLength="200" data-max="200" />
            <label for="RemarkBox">备注</label>
            <asp:TextBox ID="RemarkBox" runat="server" TextMode="MultiLine" MaxLength="500" data-max="500" />
            <div>
                <asp:Button ID="SaveButton" runat="server" Text="保存" CssClass="primary" ValidationGroup="save" OnClick="SaveButton_Click" />
                <asp:HyperLink ID="BackLink" runat="server" CssClass="secondary" Text="返回" />
            </div>
        </div>
    </asp:PlaceHolder>
</asp:Content>
