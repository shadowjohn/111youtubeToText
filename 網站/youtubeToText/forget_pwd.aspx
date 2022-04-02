<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="forget_pwd.aspx.cs" Inherits="SystemReport.forget_pwd" %>

<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="utility" %>
<%@ Import Namespace="Newtonsoft.Json" %>
<%@ Import Namespace="Newtonsoft.Json.Linq" %>
<%@ Import Namespace="System.Linq" %>
<% 
    myinclude my = new myinclude();    
%>
<style>
    .forget_pwd_class {
        width: 600px;
        height: 500px;
        border: 3px solid #000;
        border-radius: 15px;
    }

        .forget_pwd_class img[reqc='close_btn'] {
            position: absolute;
            right: 0px;
            cursor: pointer;
        }
</style>
<div class="forget_pwd_class">
    <img src="pic/x_close.png" reqc="close_btn" width="32">
    <h2>忘記密碼</h2>
    <h3>系統將會發送密碼至您申請的 Email 信箱</h3>
    <form action="?mode=forget_pd_action" method="post" onsubmit="return check_forget_div();">
        帳　號：<input type="text" reqc='login_id' name="login_id">
        <input type='submit' value="送出" reqc="run_btn">
        &nbsp;&nbsp;&nbsp;
    <input type='button' value="取消" onclick="dialogMyBoxOff();">
    </form>
</div>
<script>
    $("img[reqc='close_btn']").unbind("click").click(function () {
        dialogMyBoxOff();
    });
    function check_forget_div() {
        $(".forget_pwd_class input[reqc='login_id']").val(trim($(".forget_pwd_class input[reqc='login_id']").val()));
        if ($(".forget_pwd_class input[reqc='login_id']").val() == "") {
            alert("請輸入 帳號...\n");
            return false;
        }
        else {
            return true;
        }
    }
</script>
