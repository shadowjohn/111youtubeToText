<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="fakeCosmos.aspx.cs" Inherits="SystemReport.admin.fakeCosmos" %>

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
    if (!my.isAdmin())
    {
        my.exit();
    }
    my.echoBinary(my.file_get_contents(my.base_dir + "\\template\\html.html"));
    my.echoBinary(my.b2s(my.file_get_contents(my.base_dir + "\\template\\head.html")).Replace("{base_url}", my.base_url));
%>
<style>
    html, body {
        font-size: 14px;
    }

    #output {
        font-size: 12px;
    }

    .myWHover {
        background-color: #fee;
    }

    table th {        
        background-color: orange;
    }

    table td {
        text-overflow: ellipsis;
    }

    .red {
        color: red;
    }

    #event_logs table th[field='DateTime'] {
        width: 160px;
    }

    #event_logs table th, #event_logs table td {
        padding: 5px;
        font-size: 16px;
    }

    #HDD_div_info table th, #HDD_div_info table td {
        padding: 15px;
        font-size: 16px;
    }
</style>
<script>
    window['SQL_STEP'] = 0;
    window['SQL'] = new Array();
    function newW(jd, t) {
        for (var i = 0, max_i = count(jd); i < max_i; i++) {
            for (var k in jd[i]) {
                console.log(jd[i][k]);
                if (jd[i][k] != null && strlen(jd[i][k]) > 25) {
                    jd[i][k] = "<span reqc=\"spanShow\" title=\"" + addslashes(htmlspecialchars(jd[i][k])) + "\">" + substr(jd[i][k], 0, 25) + "...(詳細資料)</span>";
                }
            }
        }
        var data = print_table(jd);
        var fields = new Array();
        if (count(jd) >= 1) {
            for (var k in jd[0]) {
                fields.push(k);
            }
        }
        var mfields = new Array();;
        for (var i in fields) {
            var d = "<option value=\"" + fields[i] + "\" selected> " + fields[i] + "</option>";
            mfields.push(d);
        }
        var d = "";
        d += "<p reqc=\"handle\" style=\"border: 1px solid gray; background-color: yellow; cursor: pointer; text-align: center\">可拖曳</p>";
        d += "<img reqc='myw_close' width='32' req_t=\"" + t + "\" src=\"images/x_close.png\" align=\"right\">";
        d += "<select reqc='selects' multiple=\"multiple\" size=\"5\">" + implode("", mfields) + "</select>";
        d += "<input type='button' reqc='selectsBtn' value='Show Table'>";
        d += "<div style=\"padding:0px;width:100%;height:100%;overflow:auto;\">";
        d += data;
        d += "</div>";
        var mID = myW(d, function (mID) {
            $("#" + mID).css({
                'width': 'auto',
                'height': 'auto',
                'inset': 'auto auto auto auto'
            });

            //show source code
            $("#" + mID + " span[reqc='spanShow']").unbind("dblclick").dblclick(function () {
                var data = $(this).attr('title');
                data = stripslashes(data);
                dialogMyBoxOn("<pre>" + data + "</pre>", true, function () {

                });
            });

            $("#" + mID + " input[reqc='selectsBtn']").unbind("click").click({ mID: mID }, function (e) {
                var vall = $("#" + e.data.mID + " select[reqc='selects']").val();

                $("#" + e.data.mID + " table th").hide();
                $("#" + e.data.mID + " table td").hide();
                for (var k in vall) {
                    $("#" + e.data.mID + " table th[field='" + htmlspecialchars(vall[k]) + "']").show();
                    $("#" + e.data.mID + " table td[field='" + htmlspecialchars(vall[k]) + "']").show();
                }

                $("#" + e.data.mID).css({
                    'width': 'auto',
                    'height': 'auto',
                    'inset': 'auto auto auto auto'
                });
                $("#" + mID).center();

            });
            $("#" + mID).hover(function () {
                $(this).css("background-color", "#eff");
            }, function () {
                $(this).css("background-color", "#fff");
            });
            $("#" + mID).draggable({
                handle: "p[reqc='handle']",
                start: function (event, ui) {
                    $(this).css({
                        "z-index": time()
                    });
                }
            });
            $("#" + mID).resizable();
            $("#" + mID).find("img[reqc='myw_close']").unbind("click").click(function () {
                $(this).closest("div[id^='myW_']").hide("slow").delay(500, function () {
                    $(this).remove();
                });
            });
            //table scroll
            //myTableScroll("#" + mID + " table", true, { 'max-width': '500px', 'max-height': '400px' });

            //$("#" + mID + " div").css({ 'height': 'auto' });
            $("#" + mID).css({ 'max-width': '800px', 'max-height': '900px', 'overflow': 'auto' });
            $("#" + mID + " table").css({ 'max-width': '500px', 'max-height': '400px' });
        });
    }
    $(document).ready(function () {
        //快速
        $("#showDatabasesBtn").unbind("click").click(function () {
            $("textarea[reqc='data']").val(
                `
SELECT name, database_id, create_date
FROM sys.databases
ORDER BY name ASC
`
            );
        });
        $("#showTablesBtn").unbind("click").click(function () {
            $("textarea[reqc='data']").val(
                `
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_CATALOG='SystemReport'
ORDER BY TABLE_NAME ASC
`
            );
        });
        $("#slowSQLBtn").unbind("click").click(function () {
            $("textarea[reqc='data']").val(
                `
SELECT TOP 20 * FROM
tblSQLLog
ORDER BY id DESC
`
            );
        });
        mytabs($("#tabs"), {
            head_li_focus_css: {
                'background-color': '#77ff77',
                'font-weight': 'bold'
            },
            head_li_css: {
                'background-color': '#eeeeee',
                'font-weight': 'normal'
            },
            head_a_css: {
                color: '#000'
            }, content_css: {
                border: '1px solid #000'
            },
            show: "#SQL_div"
        });
        $("input[reqc='loginBtn']").unbind("click").click(function () {
            var reason = doLogin();
            if (reason != "") {
                alert(reason);
                return;
            }
            $("span[reqc='isLoginSpan']").html("<span class='red'>已登入...</span>");
        });
        //取Log
        $("#reload_event_log_bin").unbind("click").click(function () {
            $("#event_logs").html("載入中...");
            myAjax_async("../api.aspx?mode=runCosmosEvent", "", function (data) {
                var jd = json_decode(base64_decode(data), true);
                var table_data = print_table(jd["data"], "DateTime,Index,Category,Message", "DateTime,Index,Category,Message");
                $("#event_logs").html(table_data);
            });
        });
        $("#reload_event_log_bin").trigger("click");
        //取硬碟空間
        $("#reload_hdd_log_bin").unbind("click").click(function () {
            $("#HDD_div_info").html("載入中...");
            myAjax_async("../api.aspx?mode=runCosmosHDD", "", function (data) {
                var jd = json_decode(base64_decode(data), true);
                for (var i = 0, max_i = jd["data"].length; i < max_i; i++) {
                    jd["data"][i]["fake_TotalSize"] = size_hum_read(jd["data"][i]["TotalSize"]);
                    jd["data"][i]["fake_FreeSpace"] = size_hum_read(jd["data"][i]["FreeSpace"]);
                    jd["data"][i]["UsedSize"] = jd["data"][i]["TotalSize"] - jd["data"][i]["FreeSpace"];
                    jd["data"][i]["fake_UsedSize"] = size_hum_read(jd["data"][i]["UsedSize"]);
                    jd["data"][i]["fake_Percent"] = sprintf("%.2f %%", (jd["data"][i]["UsedSize"] / jd["data"][i]["TotalSize"]) * 100.0);
                }
                $("#HDD_div_info").html(print_table(jd["data"], "Name,fake_FreeSpace,fake_UsedSize,fake_TotalSize,fake_Percent", "磁碟名稱,剩餘空間,已使用空間,總空間,使用率"));
            });
        });
        $("#reload_hdd_log_bin").trigger("click");

        $("#runBtn").unbind("click").click(function () {
            dialogMyBoxOn("請稍候...", true, function () {
                var o = new Object();
                o['Cosmos'] = base64_encode($("textarea[reqc='data']").val());
                myAjax_async("../api.aspx?mode=runCosmos", o, function (data) {
                    dialogMyBoxOff();
                    data = base64_decode(data);
                    var jd = json_decode(data, true);
                    if (jd["STATUS"] != "OK") {
                        alert(jd["REASON"]);
                        return;
                    }
                    /*if (count(jd['DATA']) < 1000) {
                        //var thetable = print_table(jd['DATA']);
                        //$("div[reqc='output']").html(thetable);
                        newW(jd['DATA']);
                    }
                    else {
                        //$("div[reqc='output']").html("<pre>"+print_r(data,true)+"</pre>");
                        //newW("<pre>" + print_r(data, true) + "</pre>");
                        var a = "<a href='#SQL_STEP_" + window['SQL_STEP'] + "'>SQL " + (window['SQL_STEP'] + 1) + "</a>";
                        var div = "<div id='SQL_STEP_" + window['SQL_STEP'] + "'></div>";
                        $("div[reqc='output'] ul li").append(a);
                        $("div[reqc='output']").append(div);
                        $("#SQL_STEP_" + window['SQL_STEP']).html(print_r(data, true));
                    }*/
                    var a = "<li><a href='#SQL_STEP_" + window['SQL_STEP'] + "'>SQL " + (window['SQL_STEP'] + 1) + "</a></li>";
                    var div = "<div id='SQL_STEP_" + window['SQL_STEP'] + "'></div>";
                    $("div[reqc='output'] ul").append(a);
                    $("div[reqc='output']").append(div);
                    var sql = $("textarea[reqc='data']").val();
                    window['SQL'].push(sql);
                    var sql_div = "<textarea reqc='sql_text' style='width:600px;height:100px;'></textarea>";

                    var thetable = sql_div + "<br>" + print_table(jd['DATA']);
                    $("#SQL_STEP_" + window['SQL_STEP']).html(thetable);
                    $("#SQL_STEP_" + window['SQL_STEP'] + " table td").css({
                        'white-space': 'pre',
                        'font-family': 'monospace',
                        'word-wrap': 'break-word',
                        'padding': '5px'
                    });
                    $("#SQL_STEP_" + window['SQL_STEP'] + " textarea[reqc='sql_text']").val(window['SQL'][window['SQL_STEP']]);
                    mytabs($("div[reqc='output']"), {
                        head_li_focus_css: {
                            'background-color': '#77ff77',
                            'font-weight': 'bold'
                        },
                        head_li_css: {
                            'background-color': '#eeeeee',
                            'font-weight': 'normal'
                        },
                        head_a_css: {
                            color: '#000'
                        }, content_css: {
                            border: '1px solid #000'
                        },
                        show: "#SQL_STEP_" + window['SQL_STEP']
                    });
                    window['SQL_STEP']++;
                });
            });
        });
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

<h2>波廝菊</h2>

<br>

<!--內容開始-->
<div id="tabs">
        <ul>
            <li><a href="#SQL_div">SQL 操作</a></li>
            <li><a href="#EVENT_LOG_div">Event Logs</a></li>
            <li><a href="#HDD_div">硬碟空間</a></li>
        </ul>
        <div id="SQL_div">
            <table>
                <tr>
                    <td valign="top">
                        SQL:
                        <input type="button" value="Show databases" id="showDatabasesBtn" />
                        <input type="button" value="Show tables" id="showTablesBtn" />
                        <input type="button" value="Slow SQL" id="slowSQLBtn" />
                        <br>
                        <textarea reqc="data" style="width:800px;height:450px;"></textarea>
                        <br>
                        <input type="button" id="runBtn" value="Run">

                    </td>
                    <td valign="top">
                        <div reqc="output">
                            <ul></ul>
                        </div>
                    </td>
                </tr>
            </table>
        </div>
        <div id="EVENT_LOG_div">
            <input type="button" id="reload_event_log_bin" value="重新整理" />
            <br /><br />
            <div id="event_logs"></div>
        </div>
        <div id="HDD_div">
            <input type="button" id="reload_hdd_log_bin" value="重新整理" />
            <br /><br />
            <div id="HDD_div_info">
            </div>
        </div>
    </div>


<!--內容結束-->
</center>
<%
    my.echoBinary(my.file_get_contents(my.base_dir + "\\template\\body.html"));
%>
</html>
