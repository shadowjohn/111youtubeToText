<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="index.aspx.cs" Inherits="SystemReport.index" %>

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

    string GETS_STRING = "id";
    var GETS = my.getGET_POST(GETS_STRING, "GET");

    if (GETS["id"].ToString() != "")
    {
        GETS["id"] = Convert.ToInt32(GETS["id"].ToString());
    }
    var PA = new Dictionary<string, string>();
    string SQL = @"
    SELECT 
        [name],
        [room_id],
        [user_id],
        [del],
        [id]
    FROM
      [site]
    WHERE
      1=1
      AND [del]='0'
  ";
    if (Session[my.SESSION_PREFIX + "_userID"].ToString() == "1")
    {
    }
    else
    {
        SQL += @"
            ND [user_id] = @user_id
        ";
        //array_push($PA,$_SESSION["{$SESSION_PREFIX}_userID"]);
        PA["user_id"] = Session[my.SESSION_PREFIX + "_userID"].ToString();
    }
    SQL += @"
    ORDER BY [id] ASC
  ";
    string dt = my.time();
    my.linkToDB();
    var ra_site = my.selectSQL_SAFE(SQL, PA);
    SQL = "";

    PA = new Dictionary<string, string>();
    SQL = @"
                    SELECT 
                      [B].[name] AS [project_name],
                      [A].[kind],
                      [A].[site_id],
                      [A].[name],
                      [A].[URL],
                      [A].[POST],
                      [A].[FILE],
                      [A].[c_datetime],
                      [A].[loop_min],
                      [A].[last_getdata],
                      [A].[last_size],
                       CONVERT(varchar(256),[A].[last_datetime],120) AS [last_datetime],
                      [A].[last_status],
                      [A].[last_status_info],
                      [A].[ftp_ip],
                      [A].[ftp_port],
                      [A].[ftp_login],
                      [A].[ftp_pwd],
                      [A].[ftp_path],
                      [A].[PORT_IP],
                      [A].[PORT],
                      [A].[check_kind],
                      [A].[check_text],
                      [A].[del],
                      [A].[is_need_alert],
                      [A].[alert_kind],
                      [A].[fail_time],
                      [A].[fail_times_to_alert],
                      [A].[response_time],
                       CONVERT(varchar(256),[A].[ssl_expire_date],120) AS [ssl_expire_date],
                      [A].[is_need_alert_ssl],
                       CONVERT(varchar(256),[A].[domain_expire_date],120) as [domain_expire_date],
                      [A].[is_need_alert_domain],
                      [A].[id]
                    FROM
                      [site_item] AS [A],
                      [site] AS [B] 
                    WHERE
                      1 = 1
                      AND [A].[site_id]=[B].[id]
                  ";
    if (GETS["id"].ToString() != "" && GETS["id"].ToString() != "0")
    {

        SQL += @"
                          AND [A].[site_id]=@site_id
                        ";
        PA["site_id"] = GETS["id"].ToString();
    }
    if (Session[my.SESSION_PREFIX + "_userID"].ToString() != "1")
    {
        SQL += @"
                          AND [B].[user_id]=@user_id
                      ";
        //array_push($PA, $_SESSION["{$SESSION_PREFIX}_userID"]);
        PA["user_id"] = Session[my.SESSION_PREFIX + "_userID"].ToString();
    }
    SQL += @"              
        AND [A].[del]!= '2'
        ORDER BY [id] ASC
    ";
    var ra_items = my.selectSQL_SAFE(SQL, PA);
    ra_items.Columns.Add("下次掃描時間");
    ra_items.Columns.Add("網址或IP");
    ra_items.Columns.Add("options");
    ra_items.Columns.Add("fake_last_status_info");
    ra_items.Columns.Add("fake_last_datetime");
    ra_items.Columns.Add("fake_domain_expire_date");
    ra_items.Columns.Add("fake_last_status");
    for (int k = 0, max_k = my.count(ra_items); k < max_k; k++)
    {
        //下次執行時間
        ra_items.Rows[k]["下次掃描時間"] = (Convert.ToInt32(ra_items.Rows[k]["loop_min"].ToString()) * 60 - Convert.ToInt32(dt) % (Convert.ToInt32(ra_items.Rows[k]["loop_min"].ToString()) * 60)).ToString() + "秒";
        switch (ra_items.Rows[k]["kind"].ToString())
        {
            case "WEB":
                {
                    ra_items.Rows[k]["網址或IP"] = ra_items.Rows[k]["URL"].ToString();
                    ra_items.Rows[k]["網址或IP"] = my.htmlspecialchars(ra_items.Rows[k]["網址或IP"].ToString());
                }
                break;
            case "FTP":
                {
                    ra_items.Rows[k]["網址或IP"] = ra_items.Rows[k]["ftp_ip"].ToString();
                }
                break;
            case "PORT":
                {
                    ra_items.Rows[k]["網址或IP"] = ra_items.Rows[k]["PORT_IP"].ToString() + ":" + ra_items.Rows[k]["PORT"].ToString();
                }
                break;
        }
        switch (ra_items.Rows[k]["last_status"].ToString())
        {
            case "0":
                ra_items.Rows[k]["fake_last_status"] = "待命";
                break;
            case "1":
                ra_items.Rows[k]["fake_last_status"] = "掃描中";
                break;
            case "2":
                ra_items.Rows[k]["fake_last_status"] = "<img width='40' src='" + my.base_url + "/pic/status_ok.png' alt='正常' title='正常'>";
                break;
            case "3":
                ra_items.Rows[k]["fake_last_status"] = "<img width='40' src='" + my.base_url + "/pic/status_no.png' alt='異常' title='異常'>";
                break;
        }
        if (ra_items.Rows[k]["del"].ToString() == "1")
        {
            ra_items.Rows[k]["fake_last_status"] = "已停止掃描";
            ra_items.Rows[k]["下次掃描時間"] = "--";
        }
        ra_items.Rows[k]["options"] = @"
                        <a href = 'javascript:;' reqc = 'site_log_a' req_id = '" + ra_items.Rows[k]["id"].ToString() + @"'>掃描紀錄</a>
                        <a href = 'add_edit_site_items.aspx?mode=edit&site_id=" + ra_items.Rows[k]["site_id"].ToString() + "&id=" + ra_items.Rows[k]["id"].ToString() + @"'>編輯</a>
                      ";
        if (ra_items.Rows[k]["del"].ToString() == "0")
        {
            ra_items.Rows[k]["options"] += @"        
                              <a href = 'javascript:;' class='red' reqc='stop_btn' req_id='" + ra_items.Rows[k]["id"].ToString() + @"'>停用</a>
                         ";
        }
        else
        {
            ra_items.Rows[k]["options"] += @"        
          <a href = 'javascript:;' class='green' reqc='start_btn' req_id='" + ra_items.Rows[k]["id"].ToString() + @"'>啟用</a>
        ";
        }
        ra_items.Rows[k]["options"] += @"
        <a href = 'javascript:;' class='red' reqc='del_btn' req_id='" + ra_items.Rows[k]["id"].ToString() + @"'>刪除</a>
      ";

        ra_items.Rows[k]["fake_last_status_info"] = (ra_items.Rows[k]["last_status_info"].ToString() == "") ? "正常" : ra_items.Rows[k]["last_status_info"].ToString();
        ra_items.Rows[k]["fake_last_datetime"] = "<span title=\"" + ra_items.Rows[k]["last_datetime"].ToString() + "\">" + my.fb_date(ra_items.Rows[k]["last_datetime"].ToString()) + @"</span>";

        //domain 日期調整  2077-08-07 改  --                         
        ra_items.Rows[k]["fake_domain_expire_date"] = my.str_replace("2077-08-07 00:00:00", "--", ra_items.Rows[k]["domain_expire_date"].ToString());
        if (ra_items.Rows[k]["fake_domain_expire_date"].ToString() != "--" && ra_items.Rows[k]["fake_domain_expire_date"].ToString() != "")
        {
            ra_items.Rows[k]["fake_domain_expire_date"] = my.date("Y-m-d", my.strtotime(ra_items.Rows[k]["fake_domain_expire_date"].ToString()));
            if (ra_items.Rows[k]["fake_domain_expire_date"].ToString() == "1970-01-01")
            {
                ra_items.Rows[k]["fake_domain_expire_date"] = "";
            }
        }

        //ssl_expire_date
        if (ra_items.Rows[k]["ssl_expire_date"].ToString() != "")
        {
            ra_items.Rows[k]["ssl_expire_date"] = my.date("Y-m-d", my.strtotime(ra_items.Rows[k]["ssl_expire_date"].ToString())); //(time()-strtotime($ra_items[$k]['ssl_expire_date'])<2*30*24*60*60)? date("Y-m-d",strtotime($ra_items[$k]['ssl_expire_date'])):"還久...";
        }
    }
    my.closeDB();
    my.echoBinary(my.file_get_contents(my.base_dir + "\\template\\html.html"));
    my.echoBinary(my.b2s(my.file_get_contents(my.base_dir + "\\template\\head.html")).Replace("{base_url}", my.base_url));
%>
<style>
    .item_control {
        width: 200px;
        height: 35px;
        line-height: 35px;
        padding: 15px;
        text-align: right;
        margin-left: auto;
        margin-right: 15px;
    }

    .item_div {
        width: 99%;
        margin-left: auto;
        margin-right: auto;
        border: 1px solid #cdd;
        min-height: 800px;
    }

    .thetable {
        width: 100%;
        font-size: 13px;
        border: 1px solid #000;
    }

        .thetable tbody tr:hover {
            background-color: #eff;
        }

        .thetable td[field='id'] {
            text-align: center;
        }

        .thetable td[field='kind'] {
            text-align: center;
        }

        .thetable th[field='最後掃描時間'] {
            width: 120px;
        }

        .thetable th[field='狀況描述'] {
            width: 70px;
        }

        .thetable th[field='服務回應時間(ms)'] {
            width: 90px;
        }

        .thetable th[field='專案'] {
            width: 200px;
        }

        .thetable th[field='掃描類型'] {
            width: 70px;
        }

        .thetable th[field='標題'] {
            width: 200px;
        }

        .thetable th[field='Domain 到期時間'] {
            width: 120px;
        }

        .thetable th[field='SSL 憑證到期時間'] {
            width: 120px;
        }

        .thetable th[field='下次掃描時間'] {
            width: 80px;
        }

        .thetable th[field='掃描狀況說明'] {
            width: 150px;
        }

        .thetable th[field='功能'] {
            width: 140px;
        }

        .thetable td[field='fake_last_datetime'] {
            text-align: center;
        }

        .thetable td[field='fake_last_status'] {
            text-align: center;
        }

        .thetable td[field='fake_last_status_info'] {
            text-align: center;
        }

        .thetable td {
            word-break: break-all;
        }

            .thetable td[field='ssl_expire_date'] {
                text-align: center;
                text-wrap: normal;
                word-wrap: break-word;
                word-break: break-all;
                white-space: nowrap;
            }

            .thetable td[field='fake_domain_expire_date'] {
                text-align: center;
                text-wrap: normal;
                word-wrap: break-word;
                word-break: break-all;
                white-space: nowrap;
            }

            .thetable td[field='response_time'] {
                text-align: center;
            }

            .thetable td[field='下次掃描時間'] {
                text-align: center;
            }

            .thetable td[field='options'] {
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
<script>
    $(".top_class_a_default[req='網站服務偵測機']").addClass("top_class_a_active");
    $(document).ready(function () {
        //preset
        $("#site").val("<% my.echoBinary(my.jsAddSlashes(GETS["id"].ToString())); %>");

        $("#site").unbind("change");
        $("#site").change(function () {
            location.href = "?id=" + $("#site").val();
        });

        $("#new_site_btn").unbind("click").click(function () {
            //建立新專案
            var tmp = myAjax("app/site_add_edit.aspx?mode=add", "");
            dialogMyBoxOn(tmp, false, function () {

            });
        });
        $("#edit_site_name_btn").unbind("click").click(function () {
            //編輯專案
            var tmp = myAjax("app/site_add_edit.aspx?mode=edit&id=<% my.echoBinary(GETS["id"].ToString()); %>", "");
            dialogMyBoxOn(tmp, false, function () {

            });
        });
        $("#test_skype_btn").unbind("click").click(function () {
            //測試小桃子
            var tmp = myAjax("api.aspx?mode=test_skype&id=<% my.echoBinary(GETS["id"].ToString()); %>", "");
            smallComment("已發測試...", 5000, false, {});
        });
        $("#remove_site_btn").unbind("click").click(function () {
            //刪除專案
            if (confirm("您確定要刪除本專案? (刪除後若反悔可以請John恢復)") == true) {
                dialogMyBoxOn("請稍候...", false, function () {
                    var o = new Object();
                    o['site_id'] = "<% my.echoBinary(GETS["id"].ToString()); %>";
                    myAjax_async("api.aspx?mode=remove_project", o, function () {
                        smallComment("已刪除...網頁重載中...", 5000, false, {});
                        setTimeout(function () {
                            location.replace("?");
                        }, 3000);
                    });
                });
            }
        });
    });
</script>
    <div id="new_site">
  <div align="left">
    專案選擇：
    <select id="site">
      <option value="">--請選擇專案--</option>
      <%
          for (int i = 0, max_i = my.count(ra_site); i < max_i; i++)
          {
        %>
        <option value='<% my.echoBinary(ra_site.Rows[i]["id"].ToString()); %>'><% my.echoBinary(ra_site.Rows[i]["name"].ToString()); %></option>
        <%
            }
      %>
    </select>
    &nbsp;&nbsp;
    <%
        if (GETS["id"].ToString() != "")
        {
        %>
        <input type="button" value="編輯專案" id="edit_site_name_btn">
        <input type="button" value="測試小桃子" id="test_skype_btn">
        <input type="button" value="刪除專案" class="red" id="remove_site_btn">
        <%
            }
            else
            {
        %>
        <input type="button" id="new_site_btn" value="建立新專案">
        <%
            }
    %>
  </div>  
      <div class="item_control">
        <%
            if (GETS["id"].ToString() != "")
            {
          %>
          <input type = "button" id="add_item" value="新增偵測項目">
          <%
              }
        %>
      </div>
      <div class="item_div">
      <%
          my.echoBinary(my.print_table(ra_items,
            "id,project_name,kind,name,網址或IP,fake_domain_expire_date,ssl_expire_date,fake_last_datetime,fake_last_status,fake_last_status_info,response_time,下次掃描時間,options",
            "序號,專案,掃描類型,標題,網址或IP,Domain 到期時間,SSL 憑證到期時間,最後掃描時間,狀況描述,掃描狀況說明,回應時間(ms),下次掃描時間,功能",
            "thetable"
          ));
      %>
      </div>
      <script>
          $(document).ready(function () {
              //select 轉 combo
              select2combobox($("#site"));
              //如果現在時間比憑證時間大，就變紅色
              var table_dom = $("table[class='thetable']");
              var table_dom_tbody_td = table_dom.find("tbody td[field='ssl_expire_date'],tbody td[field='domain_expire_date']");
              for (var i = 0, max_i = table_dom_tbody_td.length; i < max_i; i++) {
                  var dt = table_dom_tbody_td.eq(i).text();
                  if (dt != "" && dt != "--" && time() - strtotime(dt) >= 0) {
                      table_dom_tbody_td.eq(i).addClass("red");
                  }
              }

              $("#add_item").unbind("click").click(function () {
                  //var tmp = myAjax("app/add_edit_site_items.aspx?mode=add","");
                  //dialogMyBoxOn(tmp,false,function(){          
                  //});
                  location.href = "add_edit_site_items.aspx?mode=add&site_id=<% my.echoBinary(GETS["id"].ToString());%>";
              });
              $(".thetable a[reqc='start_btn']").unbind("click").click(function () {
                  var id = $(this).attr('req_id');
                  location.href = "add_edit_site_items.aspx?mode=start_action&site_id=<% my.echoBinary(GETS["id"].ToString());%>&id=" + id;
              });
              $(".thetable a[reqc='stop_btn']").unbind("click").click(function () {
                  if (confirm("你確定要停用這筆掃描嗎?")) {
                      var id = $(this).attr('req_id');
                      location.href = "add_edit_site_items.aspx?mode=stop_action&site_id=<% my.echoBinary(GETS["id"].ToString());%>&id=" + id;
                  }
              });
              $("a[reqc='site_log_a']").unbind("click").click(function () {
                  var id = $(this).attr('req_id');
                  dialogMyBoxOn("請稍候...", true, function () {
                      var o = new Object();
                      o['id'] = id;
                      myAjax_async("api.aspx?mode=get_site_log", o, function (data) {
                          dialogMyBoxOn(data, true, function () { });
                      });
                  });
              });
              $("a[reqc='del_btn']").unbind("click").click(function () {
                  //刪除
                  var id = $(this).attr('req_id');
                  if (confirm("你確定要刪除嗎??") == true) {
                      var o = new Object();
                      o['id'] = id;
                      myAjax_async("add_edit_site_items.aspx?mode=del_site_item", o, function (data) {

                      });
                      $(this).closest("tr").fadeOut("slow");
                  }
              });
          });
      </script>


<!--內容結束-->
</center>
<%
    my.echoBinary(my.file_get_contents(my.base_dir + "\\template\\body.html"));
%>
</html>
