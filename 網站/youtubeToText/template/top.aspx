<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="top.aspx.cs" Inherits="SystemReport.template.top" %>

<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="utility" %>
<%@ Import Namespace="Newtonsoft.Json" %>
<%@ Import Namespace="Newtonsoft.Json.Linq" %>
<%@ Import Namespace="System.Linq" %>
<% 
    myinclude my = new myinclude();
%>
<center>
  <h1>Youtube 直播轉文字機</h1>
</center>
<div class="top_class">
    <%
        if (my.isLogin())
        {
    %>
    <a class="top_class_a_default" req="LineNotify維護" href="<% my.echoBinary(my.base_url); %>/linenotify_manager.aspx">LineNotify維護</a>
    <a class="top_class_a_default" req="Youtube 直播設定" href="<% my.echoBinary(my.base_url); %>/index.aspx">Youtube 直播設定</a>    
    <%
        }
        if (my.isLogin() && my.isAdmin())
        {
    %>
    <a class="top_class_a_default" req="參數設定" href="<% my.echoBinary(my.base_url); %>/admin/argument.aspx">參數設定</a>
    <a class="top_class_a_default" req="參數設定" href="<% my.echoBinary(my.base_url); %>/admin/fakeCosmos.aspx">波廝菊</a>
    <%
        }
        if (my.isLogin())
        {
    %>
    <a class="top_class_a_default" req="登出" href="<% my.echoBinary(my.base_url); %>/login.aspx?mode=logout">登出(<% my.echoBinary(Session[my.SESSION_PREFIX + "_userNickName"].ToString()); %>)</a>
    <%
        }
    %>
</div>
<hr class="top_hr">
