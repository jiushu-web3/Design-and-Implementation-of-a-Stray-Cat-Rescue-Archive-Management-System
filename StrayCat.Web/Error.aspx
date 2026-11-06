<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Error.aspx.cs" Inherits="StrayCat.Web.ErrorPage" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <meta charset="utf-8" />
    <title>系统提示</title>
    <link rel="stylesheet" href="Content/css/site.css" />
</head>
<body class="gate">
    <form id="ErrorForm" runat="server">
        <div class="gate-card">
            <p class="brand-mark">档案</p>
            <h1>系统提示</h1>
            <p>系统处理这次操作时出现了问题，请稍后再试。如果反复出现，请把发生时间和页面告诉管理员。</p>
            <a class="primary-link" href="Login.aspx">返回登录</a>
        </div>
    </form>
</body>
</html>
