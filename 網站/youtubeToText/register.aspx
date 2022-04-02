<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="sample.aspx.cs" Inherits="SystemReport.sample" %>

<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="utility" %>
<%@ Import Namespace="Newtonsoft.Json" %>
<%@ Import Namespace="Newtonsoft.Json.Linq" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="System.Data.SqlClient" %>
<% 
    myinclude my = new myinclude();

    string GETS_STRING = "mode";
    var GETS = my.getGET_POST(GETS_STRING, "GET");
    switch (GETS["mode"].ToString())
    {
        case "register_action":
            {
                string POSTS_STRING = "login_id,pd,name,nickname,gdcode";
                var POSTS = my.getGET_POST(POSTS_STRING, "POST");
                if (POSTS["login_id"].ToString() == "")
                {
                    my.exit();
                }
                if (POSTS["pd"].ToString() == "")
                {
                    my.exit();
                }
                if (POSTS["name"].ToString() == "")
                {
                    my.exit();
                }
                if (POSTS["nickname"].ToString() == "")
                {
                    my.exit();
                }
                if (Session["GD_CODE"] != null && POSTS["gdcode"].ToString() != Session["GD_CODE"].ToString())
                {
                    my.alert("驗證碼錯誤...");
                    my.history_go();
                    my.exit();
                }
                //先查有沒有人註冊過
                my.linkToDB();
                string SQL = @"
                SELECT
                    COUNT(*) AS [COUNTS]
                FROM
                    [user]
                WHERE
                    [login_id]=@login_id
                ";
                Dictionary<string, string> mc = new Dictionary<string, string>();
                mc["login_id"] = POSTS["login_id"].ToString();
                var ra = my.selectSQL_SAFE(SQL, mc);
                if (ra.Rows[0]["COUNTS"].ToString() != "0")
                {
                    my.closeDB();
                    my.alert("此帳號已被註冊過...");
                    my.history_go();
                    my.exit();
                }
                Dictionary<string, string> m = new Dictionary<string, string>();
                m["login_id"] = POSTS["login_id"].ToString();
                m["pwd"] = POSTS["pd"].ToString();
                m["name"] = POSTS["name"].ToString();
                m["nickname"] = POSTS["nickname"].ToString();
                m["c_datetime"] = my.date("Y-m-d H:i:s");
                my.insertSQL("user", m);
                my.closeDB();
                my.alert("帳號註冊成功，請登入系統...");
                my.location_href("login.aspx");
                my.exit();
            }
            break;
    }


    my.echoBinary(my.file_get_contents(my.base_dir + "\\template\\html.html"));
    my.echoBinary(my.b2s(my.file_get_contents(my.base_dir + "\\template\\head.html")).Replace("{base_url}", my.base_url));
%>
<% 
    my.echoBinary(my.file_get_contents(my.base_dir + "\\template\\head_end.html"));
%>
<% 
    my.echoBinary(my.file_get_contents(my.base_dir + "\\template\\body.html"));
%>
<div reqc="topData">
    <script>myAjax_async("<% my.echoBinary(my.base_url + "/template/top.aspx");%>", "", function (data) { $("div[reqc='topData']").html(data); });</script>
</div>
<center>

<h2>網站服務偵測機</h2>

<br>

<!--內容開始-->
<center>
  <h2>註冊新帳號</h2>  
  <br>
  <script>
      function check() {
          var message = "";
          $("#login_id").val(trim($("#login_id").val()));
          $("#pd").val(trim($("#pd").val()));
          $("#pd_again").val(trim($("#pd_again").val()));
          $("#gdcode").val(trim($("#gdcode").val()));
          $("#name").val(trim($("#name").val()));
          $("#nickname").val(trim($("#nickname").val()));
          if ($("#login_id").val() == "" || ValidEmail($("#login_id").val()) == false) {
              message += "「帳號」請輸入正確的 Email 格式...\n";
          }
          if ($("#pd").val() == "") {
              message += "請輸入密碼...\n";
          }
          if ($("#pd").val() != $("#pd_again").val()) {
              message += "密碼二次輸入不一樣...\n";
          }
          if ($("#name").val() == "") {
              message += "請輸入姓名...\n";
          }
          if ($("#nickname").val() == "") {
              message += "請輸入匿稱...\n";
          }
          if ($("#gdcode").val() == "") {
              message += "請輸入驗證碼...\n";
          }
          if (message != "") {
              alert(message);
              return false;
          }
          else {
              return true;
          }
      }
  </script>
  <form action="?mode=register_action" method="post" onSubmit="return check();">
    <div class="login_class">
      帳　　號：<input type="text" id="login_id" name="login_id" placeholder="請輸入帳號，如：john@gis.tw"><br>
      密　　碼：<input type="password" id="pd" name="pd" placeholder="請輸入密碼..."><br>
      密碼驗證：<input type="password" id="pd_again" placeholder="請再次輸入密碼..."><br>
      姓　　名：<input type="text" id="name" name="name" placeholder="請輸入姓名..."><br>
      匿　　稱：<input type="text" id="nickname" name="nickname" placeholder="請輸入匿稱..."><br>
      驗 證 碼：<img src="gd.aspx">&nbsp;<input type="text" id="gdcode" name="gdcode" maxlength="4" style="width:50px;">
      <br>      
      <br>
      <center>                
        <a href="javascript:;" reqc="login_a" onClick="$('#submit_btn').trigger('click');">建立帳號</a>
        <input type="submit" style="display:none;" id="submit_btn" value="建立帳號">
        &nbsp;&nbsp;&nbsp;
        <a href='login.aspx'>取消</a>
      </center>
    </div>
  </form> 


<!--內容結束-->
</center>
<%
    my.echoBinary(my.file_get_contents(my.base_dir + "\\template\\body.html"));
%>
</html>
