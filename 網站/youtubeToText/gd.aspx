<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="gd.aspx.cs" Inherits="SystemReport.gd" %>

<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="utility" %>
<%@ Import Namespace="Newtonsoft.Json" %>
<%@ Import Namespace="Newtonsoft.Json.Linq" %>
<%@ Import Namespace="System.Linq" %>
<%
    myinclude my = new myinclude();
    string AZ19 = "ABCDEFGHKMNPQRSTUVWXY23456789";
    string key = my.strtoupper(
        AZ19.Substring(my.rand(0, my.strlen(AZ19) - 1), 1) +
        AZ19.Substring(my.rand(0, my.strlen(AZ19) - 1), 1) +
        AZ19.Substring(my.rand(0, my.strlen(AZ19) - 1), 1) +
        AZ19.Substring(my.rand(0, my.strlen(AZ19) - 1), 1)
    );    
    my.gd_show(key, "");
%>