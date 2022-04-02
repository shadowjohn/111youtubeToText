<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="login.aspx.cs" Inherits="SystemReport.login" %>

<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="utility" %>
<%@ Import Namespace="Newtonsoft.Json" %>
<%@ Import Namespace="Newtonsoft.Json.Linq" %>
<%@ Import Namespace="System.Linq" %>
<% 
    myinclude my = new myinclude();
    string GETS_STRING = "mode";
    var GETS = my.getGET_POST(GETS_STRING, "GET");

    switch (GETS["mode"].ToString())
    {
        case "logout":
            {
                Session[my.SESSION_PREFIX + "_isLogin"] = "";
                Session[my.SESSION_PREFIX + "_isAdmin"] = "";
                Session[my.SESSION_PREFIX + "_userID"] = "";
                Session[my.SESSION_PREFIX + "_userName"] = "";
                Session[my.SESSION_PREFIX + "_userNickName"] = "";
                my.alert("登出成功");
                my.location_href("login.aspx");
                my.exit();
            }
            break;
        case "login_action":
            {
                string POSTS_STRING = "login_id,pd,gdcode";
                var POSTS = my.getGET_POST(POSTS_STRING, "POST");
                if (POSTS["login_id"].ToString() == "")
                {
                    my.exit();
                }
                if (POSTS["pd"].ToString() == "")
                {
                    my.exit();
                }
                //if (POSTS["login_id"].ToString() != "john@gis.tw")
                {
                    if (Session["GD_CODE"] != null && POSTS["gdcode"].ToString() != Session["GD_CODE"].ToString())
                    {
                        my.alert("驗證碼錯誤...");
                        my.history_go();
                        my.exit();
                    }
                }
                Session[my.SESSION_PREFIX + "_isLogin"] = "";
                Session[my.SESSION_PREFIX + "_isAdmin"] = "";
                Session[my.SESSION_PREFIX + "_userID"] = "";
                Session[my.SESSION_PREFIX + "_userName"] = "";
                Session[my.SESSION_PREFIX + "_userNickName"] = "";
                my.linkToDB();
                string SQL = @"
                  SELECT
                    TOP 1
                    [id],
                    [is_admin],
                    [name],
                    [nickname]
                  FROM 
                    [user]
                  WHERE
                    1 = 1
                    AND [login_id]=@login_id
                  AND [pwd]=@pd
                  AND [del]= '0'
                  ORDER BY [id] DESC                
                ";
                Dictionary<string, string> pa = new Dictionary<string, string>();
                pa["login_id"] = POSTS["login_id"].ToString();
                pa["pd"] = POSTS["pd"].ToString();
                var ra = my.selectSQL_SAFE(SQL, pa);
                my.closeDB();
                if (ra.Rows.Count != 1)
                {
                    my.alert("尚未註冊，或密碼錯誤...");
                    my.history_go();
                }
                else
                {

                    Session[my.SESSION_PREFIX + "_isLogin"] = "Y";
                    Session[my.SESSION_PREFIX + "_isAdmin"] = (ra.Rows[0]["is_admin"].ToString() == "1") ? "Y" : "";
                    Session[my.SESSION_PREFIX + "_userID"] = ra.Rows[0]["id"].ToString();
                    Session[my.SESSION_PREFIX + "_userName"] = ra.Rows[0]["name"].ToString();
                    Session[my.SESSION_PREFIX + "_userNickName"] = ra.Rows[0]["nickname"].ToString();
                    my.location_href("index.aspx");
                }
                my.exit();
            }
            break;
        case "forget_pd_action":
            {
                my.linkToDB();
                string POSTS_STRING = "login_id";
                var POSTS = my.getGET_POST(POSTS_STRING, "POST");
                string SQL = @"
        SELECT
        TOP 1
          [login_id],
          [pwd]
        FROM
          [user]
        WHERE
          [login_id]=@login_id
        ";
                Dictionary<string, string> pa = new Dictionary<string, string>();
                pa["login_id"] = POSTS["login_id"].ToString();
                var ra = my.selectSQL_SAFE(SQL, pa);
                my.closeDB();
                if (ra.Rows.Count != 0)
                {
                    string fake_pwd = ra.Rows[0]["pwd"].ToString();
                    string[] mfake_pwd = new string[] { fake_pwd };
                    for (int i = 0; i < mfake_pwd.Count() - 1; i += 2)
                    {
                        mfake_pwd[i] = "_";
                    }
                    fake_pwd = my.implode("", mfake_pwd);
                    List<string> TO = new List<string>();
                    TO.Add(ra.Rows[0]["login_id"].ToString());
                    my.sendMail(
                        TO,
                          "【神奇的網站測試機】忘記密碼...",
                          @"
            Dear " + ra.Rows[0]["login_id"].ToString() + @",<br><br>
            你原本的密碼可能是…「" + fake_pwd + @"」<br><br>
            請盡全力回想，如果還是記不起來，再問 John 吧<br> <br>
            Regards,<br>
            John");
                    my.alert("已發信至您的 Email...");
                    my.location_href(my.base_url);
                }
                else
                {
                    my.alert("查無此人...");
                    my.location_href(my.base_url);
                }
                my.exit();
            }
            break;
    }
    my.echoBinary(my.file_get_contents(my.base_dir + "\\template\\html.html"));
    my.echoBinary(my.b2s(my.file_get_contents(my.base_dir + "\\template\\head.html")).Replace("{base_url}", my.base_url));
%>
<script>
    function check() {
        $("#login_id").val(trim($("#login_id").val()));
        $("#pd").val(trim($("#pd").val()));
        $("#gdcode").val(trim($("#gdcode").val()));
        var message = "";
        if ($("#login_id").val() == "" || ValidEmail($("#login_id").val()) == false) {
            message += "「帳號」請輸入正確的 Email 格式...\n";
        }
        if ($("#pd").val() == "") {
            message += "「密碼」未輸入...\n";
        }
        if ($("#gdcode").val() == "") {
            //message += "「驗證碼」未輸入...\n";
        }
        if (message != "") {
            alert(message);
            return false;
        }
        else {
            return true;
        }
    }
    $(document).ready(function () {
        //focus
        $("#login_id").focus();
        //忘記密碼
        $("a[reqc='forget_pd']").unbind("click");
        $("a[reqc='forget_pd']").click(function () {
            var tmp = myAjax("forget_pwd.aspx", "");
            dialogMyBoxOn(tmp, false, function () {

            });
        });
        //註冊
        $("a[reqc='register_a']").unbind("click");
        $("a[reqc='register_a']").click(function () {
            location.href = "register.aspx";
        });
        $("#gdcode").keyup(function () {
            $(this).val($(this).val().toUpperCase());
        });
        <%
    if (my.is_string_like_new(my.base_url, "%localhost%") || my.is_string_like_new(my.base_url, "%127.0.0.1%"))
    {
        %>
        $("#gdcode").val(myAjax('api.aspx?mode=getGDCode',''));
<%
    }
        %>
    });
</script>
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
  <h3>系統登入</h3>  
  <br>
  <!--內容開始-->
  
  <form action="?mode=login_action" method="post" onSubmit="return check();">
    <div class="login_class" style="text-align:left;width:500px;">
      <p>帳　號：<input type="text" id="login_id" name="login_id" size="50" placeholder="請輸入帳號，如：ooxx@3wa.tw"></p>
      <p>密　碼：<input type="password" id="pd" name="pd" size="50" placeholder="請輸入密碼..."></p>
      <p>驗證碼：<img src="gd.aspx">&nbsp;<input type="text" id="gdcode" name="gdcode" maxlength="4" style="text-align:center;width:50px;"></p>
      <br>      
      <br>
      <center>
        <!--a href="javascript:;" reqc="register_a">註冊新帳號</a-->
        &nbsp;&nbsp;&nbsp;
        <a href="javascript:;" reqc="login_a" onClick="$('#submit_btn').trigger('click');">登入</a>
        <input type="submit" style="display:none;" id="submit_btn" value="登入">
        &nbsp;&nbsp;&nbsp;
        <a href="javascript:;" reqc="forget_pd">忘記密碼</a>
      </center>
    </div>
  </form>  
<!--內容結束-->
</center>
<%
    my.echoBinary(my.file_get_contents(my.base_dir + "\\template\\body.html"));
%>
</html>