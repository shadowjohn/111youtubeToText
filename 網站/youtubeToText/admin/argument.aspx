<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="argument.aspx.cs" Inherits="SystemReport.admin.argument" %>

<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="utility" %>
<%@ Import Namespace="Newtonsoft.Json" %>
<%@ Import Namespace="Newtonsoft.Json.Linq" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="System.Data.SqlClient" %>
<% 
    myinclude my = new myinclude();
    my.checkpassword();

    //找出所有的參數值
    //改成用 group 來排序
    my.linkToDB();
    string SQL = "SELECT * FROM [argument] ORDER BY [group]";
    var ra = my.selectSQL_SAFE(SQL, new Dictionary<string, string>());

    //找出有幾種group  
    List<string> distinct_group = new List<string>();
    for (int i = 0; i < my.count(ra); i++)
    {
        if (!my.in_array(ra.Rows[i]["group"].ToString(), distinct_group))
        {
            distinct_group.Add(ra.Rows[i]["group"].ToString());
        }
    }

    string GETS_STRING = "mode";
    var GETS = my.getGET_POST(GETS_STRING, "GET");
    switch (GETS["mode"].ToString())
    {
        case "setting_action":
            var mlist = new List<string>();
            for (int i = 0; i < my.count(ra); i++)
            {
                mlist.Add(ra.Rows[i]["title"].ToString());
            }
            string POSTS_STRING = my.implode(",", mlist);
            var POSTS = my.getGET_POST(POSTS_STRING, "POST");
            for (int i = 0; i < my.count(ra); i++)
            {
                //不能為空值
                if (POSTS[ra.Rows[i]["title"].ToString()].ToString() == "")
                {
                    my.alert(ra.Rows[i]["name"].ToString() + " 不能是空值...");
                    my.history_go();
                    my.exit();
                }
                if (ra.Rows[i]["isNumber"].ToString() == "1")
                {
                    if (!my.is_numeric(POSTS[ra.Rows[i]["title"].ToString()].ToString()))
                    {
                        my.alert(ra.Rows[i]["name"].ToString() + " 必需是數值...");
                        my.history_go();
                        my.exit();
                    }
                }

                //以上皆成立
                Dictionary<string, string> m = new Dictionary<string, string>();
                m["value"] = POSTS[ra.Rows[i]["title"].ToString()].ToString();
                var mpa = new Dictionary<string, string>();
                mpa["title"] = ra.Rows[i]["title"].ToString();
                my.updateSQL_SAFE("argument", m, "[title]=@title", mpa);
            }
            my.closeDB();
            my.alert("Done!");
            my.location_href("?");
            my.exit();
            break;
    }
    my.closeDB();
    my.echoBinary(my.file_get_contents(my.base_dir + "\\template\\html.html"));
    my.echoBinary(my.b2s(my.file_get_contents(my.base_dir + "\\template\\head.html")).Replace("{base_url}", my.base_url));
%>
<script language="javascript">
    $(".top_class_a_default[req='參數設定']").addClass("top_class_a_active");
    $(document).ready(function () {
        $(".top_class a[req='參數設定']").removeClass('top_class_a_default');
        $(".top_class a[req='參數設定']").addClass('top_class_a_active');
    });
</script>
<style>
    #argument_tab ul {
        height: 50px;
    }

    /*-----------------*/
    .func {
        width: 90%;
        margin-left: auto;
        margin-right: auto;
    }

    .tab {
        margin: -10px -20px;
        margin-bottom: 10px;
        background: #333;
    }

        .tab a {
            display: inline-block;
            padding: 10px 20px;
            color: #999;
        }

            .tab a.active {
                border-bottom: 1px solid #fff;
                background: #f3f397;
                color: #5a5a18;
                font-weight: bold;
            }

    .setting_table tr td {
        word-wrap: break-word;
        /*word-break: normal;*/
    }

    .btnBlock {
        padding: 15px;
        text-align: center;
    }
</style>
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

<script language="javascript">
    $(document).ready(function () {
        $("#argument_tab").tabs();
        $("[id^=fragment_]:not(:eq(0))").hide();
        $(".tab a:eq(0)").addClass("active");

        $(".tab a").click(function () {
            var n = $(this).index() + 1;
            $(".tab a").removeClass("active");
            $(this).addClass("active");
            $("[id^=fragment_]").hide();
            $("#fragment_" + n).show();
        });
    });
  </script>
<script language="javascript">
    function check() {
        var message = "";
    <%
    for (int i = 0; i < my.count(ra); i++)
    {
      %>
        if (my.trim($("#<% my.echoBinary(ra.Rows[i]["title"].ToString());%>").val()) == "") {
            message +="<% my.echoBinary(ra.Rows[i]["name"].ToString()); %> 不能是空值...\n";
        }
      <%
    if (ra.Rows[i]["isNumber"].ToString() == "1")
    {
      %>
        else if (isNaN(parseInt(trim($("#<% my.echoBinary(ra.Rows[i]["title"].ToString()); %>").val())))) {
            message +="<% my.echoBinary(ra.Rows[i]["name"].ToString()); %> 必需為數值...\n";
        }
      <%
        }
    }
    %>
        if (message != "") {
            alert(message);
            return false;
        }
        else {
            return true;
        }
    }
    function showHTML(dom_id, kind) {
        switch (kind) {
            case 'preset':
                //查看的按鈕
                $("#mycolorbox").colorbox({
                    open: true,
                    html: sprintf("<div style='background-color:black;'>%s</div> \
            <br><br> \
            <center><input type='button' value='關閉' onClick=\"$('#mycolorbox').colorbox.close();\"></center> \
            ", $("#" + dom_id).html()),
                    overlayClose: false,
                    close: ""
                }, function () {
                    $("#mycolorbox").colorbox.resize();
                });
                break;
            case 'value':
                //查看的按鈕
                $("#mycolorbox").colorbox({
                    open: true,
                    html: sprintf("<div style='background-color:black;'>%s</div> \
            <br><br> \
            <center><input type='button' value='關閉' onClick=\"$('#mycolorbox').colorbox.close();\"></center> \
            ", $("#" + dom_id).val()),
                    overlayClose: false,
                    close: ""
                }, function () {
                    $("#mycolorbox").colorbox.resize();
                });
                break;
        }
    }
    function editHTML(dom_id) {
        //編輯的按鈕
        var tmp_dom = sprintf("dom_%d", time());
        $("#mycolorbox").colorbox({
            open: true,
            html: sprintf(" \
      <div style='background-color:white;'> \
        <textarea id='%s'>%s</textarea> \
      </div> \
      <br> \
      <center> \
        <input type='button' value='確定' onClick=\"$('#%s').val(CKEDITOR.instances.%s.getData());$('#mycolorbox').colorbox.close();\"> \
        &nbsp;&nbsp;&nbsp;&nbsp; \
        <input type='button' value='取消' onClick=\"$('#mycolorbox').colorbox.close();\"> \
      </center>", tmp_dom, $("#" + dom_id).val(), dom_id, tmp_dom),
            overlayClose: false,
            width: 800,
            height: 500,
            close: ""
        }, function () {
            CKEDITOR.replace(tmp_dom,
                {
                    filebrowserBrowseUrl: 'inc/javascript/ckeditor/ckfinder/ckfinder.html',
                    filebrowserImageBrowseUrl: 'inc/javascript/ckeditor/ckfinder/ckfinder.html?Type=Images',
                    filebrowserFlashBrowseUrl: 'inc/javascript/ckeditor/ckfinder/ckfinder.html?Type=Flash',
                    filebrowserUploadUrl: 'inc/javascript/ckeditor/ckfinder/core/connector/php/connector.php?command=QuickUpload&type=Files',
                    filebrowserImageUploadUrl: 'inc/javascript/ckeditor/ckfinder/core/connector/php/connector.php?command=QuickUpload&type=Images',
                    filebrowserFlashUploadUrl: 'inc/javascript/ckeditor/ckfinder/core/connector/php/connector.php?command=QuickUpload&type=Flash'
                });
        });


    }
</script>
<script type="text/javascript" src="inc/javascript/ckeditor/ckeditor.js"></script>
<script src="inc/javascript/ckeditor/_samples/sample.js" type="text/javascript"></script>
<link href="inc/javascript/ckeditor/_samples/sample.css" rel="stylesheet" type="text/css" />    

<center>
  <div class="header">
    <h2>系統參數設定</h2>
  </div>
</center>
<div class="content" style="padding-top: 20px;">

<form action="?mode=setting_action" method="POST" onSubmit="return check();">

<div class="func">
<div class="tab">

    <%
        for (int i = 0; i < distinct_group.Count; i++)
        {
        %>
        <a href="#fragment_<% my.echoBinary((i + 1).ToString()); %>"><% my.echoBinary(distinct_group[i]); %></a>
        <%
            }
    %>
</div><!-- tab -->
說明：此網頁為網站相關參數之設定。


<%
    for (int i = 0; i < distinct_group.Count(); i++)
    {
%>
  <div id="fragment_<% my.echoBinary((i + 1).ToString()); %>">
  <table class="setting_table" border="1" align="center" width="100%">
    <tr>
      <th align="center">參數項目</th>
      <th align="center">參數說明</th>
      <th align="center">建議預設值</th>
      <th align="center">設定值</th>
      <th align="center">使用設定值</th>
    </tr>
  <% 
      for (int j = 0; j < my.count(ra); j++)
      {
          if (ra.Rows[j]["group"].ToString() == distinct_group[i])
          {
      %>
      <tr>
        <td align="left"><% my.echoBinary(ra.Rows[j]["name"].ToString()); %></td>
        <td align="left"><% my.echoBinary(ra.Rows[j]["comment"].ToString()); %></td>
        <td align="center">
          <span id="for_show_preset_btn_<% my.echoBinary(ra.Rows[j]["title"].ToString()); %>" style="display:none;">
          <input type="button" onClick="showHTML('for_show_preset_<% my.echoBinary(ra.Rows[j]["title"].ToString()); %>', 'preset');" value="查看">
          </span>
          <span id="for_show_preset_<% my.echoBinary(ra.Rows[j]["title"].ToString()); %>">
          <% 
              string fake_preset = my.htmlspecialchars_decode(ra.Rows[j]["preset"].ToString());
              if (my.strlen(fake_preset) >= 30)
              {
                  my.echoBinary(fake_preset.Substring(0, 30) + "...");
              }
              else
              {
                  my.echoBinary(fake_preset);
              }
          %>
          </span>
          <% 
              if (ra.Rows[j]["isHtmlEditor"] == "1")
              {
            %>
            <script language="javascript">
                $("#for_show_preset_<% my.echoBinary(ra.Rows[j]["title"].ToString()); %>").hide();
                $("#for_show_preset_btn_<% my.echoBinary(ra.Rows[j]["title"].ToString()); %>").show();
            </script>
            <%
                }
          %>
        </td>
        <td align="left">
          <span id="for_show_value_btn_<% my.echoBinary(ra.Rows[j]["title"].ToString()); %>" style="display:none;">
            <input type="button" onClick="showHTML('<% my.echoBinary(ra.Rows[j]["title"].ToString()); %>', 'value');" value="查看">
            <input type="button" onClick="editHTML('<% my.echoBinary(ra.Rows[j]["title"].ToString()); %>');" value="修改">            
          </span> 
          <span id="for_show_value_<% my.echoBinary(ra.Rows[j]["title"].ToString()); %>">
          <%
              if (ra.Rows[j]["isPassword"].ToString() != "1")
              {
          %>                          
            <input type="text" id="<% my.echoBinary(ra.Rows[j]["title"].ToString()); %>" name="<% my.echoBinary(ra.Rows[j]["title"].ToString()); %>">
          <%
              }
              else
              {
          %>
            <input type="password" id="<% my.echoBinary(ra.Rows[j]["title"].ToString()); %>" name="<% my.echoBinary(ra.Rows[j]["title"].ToString()); %>">
          <%
              }
          %>              
          </span>
          <script language="javascript">
          <%                                
              if (ra.Rows[j]["isHtmlEditor"].ToString() == "1")
              {
            %>
              $("#for_show_value_<% my.echoBinary(ra.Rows[j]["title"].ToString()); %>").hide();
              $("#for_show_value_btn_<% my.echoBinary(ra.Rows[j]["title"].ToString()); %>").show();
            <%
              }
          %>
              $("#<% my.echoBinary(ra.Rows[j]["title"].ToString()); %>").val("<% my.echoBinary(my.jsAddSlashes(my.htmlspecialchars_decode(ra.Rows[j]["value"].ToString()))); %>");
          </script>          
        </td>
        <td align="center">
          <input type="button" onClick='if (confirm("你確定要還原為系統預設值嗎?") == true) {
        $("#<% my.echoBinary(ra.Rows[j]["title"].ToString()); %>").val("<% my.echoBinary(my.jsAddSlashes(my.htmlspecialchars_decode(ra.Rows[j]["preset"].ToString()))); %>");}' value="預設值">
        </td>
      </tr>
      <%
              } //if
          }       //for $j
  %>
    </table>
    </div><!-- fragment_ -->
<%
    }      //for $i
%>


<div class="btnBlock">
  <input type="submit" value="儲存" style="width: 26em;">
  <input type="button" onClick="location.reload();" value="重讀">
</div><!-- btn-block -->
</div><!-- func -->
</form>

</div>

<!--內容結束-->
</center>
<%
    my.echoBinary(my.file_get_contents(my.base_dir + "\\template\\body.html"));
%>
</html>
