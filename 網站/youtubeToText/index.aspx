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
    string GETS_STRING = "mode";
    var GETS = my.getGET_POST(GETS_STRING, "GET");
    switch (GETS["mode"].ToString())
    {
        case "getDetail":
            {
                string POSTS_STRING = "id";
                var POSTS = my.getGET_POST(POSTS_STRING, "POST");
                POSTS["id"] = Convert.ToInt32(POSTS["id"].ToString());
                my.linkToDB();
                string _SQL = @"
                    SELECT * FROM [site]
                    WHERE [id]=@id
                ";
                var PA = new Dictionary<string, string>();
                PA["id"] = POSTS["id"].ToString();
                var _ra = my.selectSQL_SAFE(_SQL, PA);
                my.closeDB();
                var OUTPUT = new Dictionary<string, object>();
                OUTPUT["status"] = "OK";
                OUTPUT["data"] = _ra;
                my.echoBinary(my.json_encode(OUTPUT));
                my.exit();
            }
            break;
        case "add_edit_action":
            {
                my.linkToDB();
                string POSTS_STRING = "id,title,status,youtube_url,crawler_url,cdatetime,last_datetime,linenotify_send_ids,keyword";
                var POSTS = my.getGET_POST(POSTS_STRING, "POST");
                if (POSTS["id"].ToString() != "")
                {
                    POSTS["id"] = Convert.ToInt32(POSTS["id"].ToString());
                }
                GETS_STRING = "mode,domode";
                GETS = my.getGET_POST(GETS_STRING, "GET");
                var o = new Dictionary<string, string>();
                o["title"] = POSTS["title"].ToString();
                o["crawler_url"] = POSTS["crawler_url"].ToString();
                o["youtube_url"] = POSTS["youtube_url"].ToString();
                o["status"] = POSTS["status"].ToString();
                o["cdatetime"] = my.date("Y-m-d H:i:s");
                o["linenotify_send_ids"] = POSTS["linenotify_send_ids"].ToString();
                o["keyword"] = POSTS["keyword"].ToString();
                switch (GETS["domode"].ToString())
                {
                    case "add":
                        {

                            my.insertSQL("site", o);
                        }
                        break;
                    case "edit":
                        {
                            var mpa = new Dictionary<string, string>();
                            mpa["id"] = POSTS["id"].ToString();
                            my.updateSQL_SAFE("site", o, "[id]=@id", mpa);
                        }
                        break;
                }
                my.closeDB();
                var OUTPUT = new Dictionary<string, object>();
                OUTPUT["status"] = "OK";
                my.echoBinary(my.json_encode(OUTPUT));
                my.exit();
            }
            break;
        case "statusChange":
            {
                string POSTS_STRING = "id,status";
                var POSTS = my.getGET_POST(POSTS_STRING, "POST");
                POSTS["id"] = Convert.ToInt32(POSTS["id"].ToString());
                POSTS["status"] = Convert.ToInt32(POSTS["status"].ToString());
                my.linkToDB();
                var m = new Dictionary<string, string>();
                m["status"] = POSTS["status"].ToString();
                var mpa = new Dictionary<string, string>();
                mpa["id"] = POSTS["id"].ToString();
                my.updateSQL_SAFE("site", m, "id=@id", mpa);
                my.closeDB();
                var OUTPUT = new Dictionary<string, object>();
                OUTPUT["status"] = "OK";
                my.echoBinary(my.json_encode(OUTPUT));
                my.exit();
            }
            break;
        case "del_action":
            {
                string POSTS_STRING = "id";
                var POSTS = my.getGET_POST(POSTS_STRING, "POST");
                POSTS["id"] = Convert.ToInt32(POSTS["id"].ToString());
                my.linkToDB();
                var m = new Dictionary<string, string>();
                m["del"] = "1";
                var mpa = new Dictionary<string, string>();
                mpa["id"] = POSTS["id"].ToString();
                my.updateSQL_SAFE("site", m, "id=@id", mpa);
                //my.deleteSQL_SAFE("site", "[id]=@id", mpa);
                my.closeDB();
                var OUTPUT = new Dictionary<string, object>();
                OUTPUT["status"] = "OK";
                my.echoBinary(my.json_encode(OUTPUT));
                my.exit();
            }
            break;
    }
    my.linkToDB();
    string SQL = @"
        SELECT 
            [id],
            [title],           
            [status],
            [youtube_url],
            [m3u8_url],
            [crawler_url],
            CONVERT(varchar(256), [cdatetime],120) AS [cdatetime],
            CONVERT(varchar(256), [last_datetime],120) AS [last_datetime],
            [linenotify_send_ids],
            [keyword],
            ISNULL([error_log],'') AS [error_log]
        FROM 
            [site]
        WHERE
            [del]='0'
    ";
    var rra = my.selectSQL_SAFE(SQL);
    var ra = my.datatable2dictinyary(rra);
    my.closeDB();
    //增加編輯與刪除    
    for (int i = 0, max_i = ra.Count; i < max_i; i++)
    {
        ra[i]["option"] = "";
        switch (ra[i]["status"])
        {
            case "0":
                {
                    ra[i]["option"] += "<input type='button' value='啟動' req_value='0' reqc='statusBtn' req_id='" + ra[i]["id"] + "'>";
                }
                break;
            case "1":
                {
                    ra[i]["option"] += "<input type='button' value='暫停' req_value='1' reqc='statusBtn' req_id='" + ra[i]["id"] + "'>";
                }
                break;
        }
        ra[i]["option"] += "&nbsp;&nbsp;&nbsp;";
        ra[i]["option"] += "<input type='button' value='編輯' reqc='editBtn' req_id='" + ra[i]["id"] + "'>";
        ra[i]["option"] += "&nbsp;&nbsp;&nbsp;";
        ra[i]["option"] += "<input type='button' value='刪除' class='red' reqc='delBtn' req_id='" + ra[i]["id"] + "'>";
        ra[i]["fake_status"] = (ra[i]["status"] == "0") ? "<span class='red'>暫停</span>" : "<span class='green'>執行中</span>";
    }
    string table = my.print_table(ra,
        "id,title,youtube_url,keyword,last_datetime,fake_status,error_log,option",
        "序號,Youtube 標題,Youtube URL,關鍵字,最後更新時間,狀態,異常紀錄,功能", "thetable");
    my.echoBinary(my.file_get_contents(my.base_dir + "\\template\\html.html"));
    my.echoBinary(my.b2s(my.file_get_contents(my.base_dir + "\\template\\head.html")).Replace("{base_url}", my.base_url));
%>
<style>
    .thetable tbody td[field='fake_status'] {
        text-align: center;
    }

        .thetable tbody td[field='fake_status'] img {
            width: 40px;
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

<h2>直播設定表</h2>

<br>

<!--內容開始-->
    <div align="right">
        <input type="button" reqc="add_btn" value="新增直播連結" /><br />        
    </div>

    <% my.echoBinary(table); %>
    <script>
        //啟動暫停按鈕
        $("input[reqc='statusBtn']").unbind("click").click(function () {
            var o = new Object();
            o['status'] = (parseInt($(this).attr('req_value')) + 1) % 2;
            o['id'] = $(this).attr('req_id');
            myAjax_async("?mode=statusChange", o, function (data) {
                var jd = json_decode(data, true);
                if (jd['status'] == 'OK') {
                    smallComment("設定完成", 3000, false, {});
                    setTimeout(function () {
                        location.replace("?");
                    }, 1000);
                }
            });
        });
        //編輯
        $("input[reqc='editBtn']").unbind("click").click(function () {
            var id = $(this).attr('req_id');
            var o = new Object();
            o["id"] = $(this).attr('req_id');
            myAjax_async("app/site_add_edit.html?_t=" + time(), "", function (data) {
                data = str_replace("{kind_title}", "編輯", data);
                data = str_replace("{id}", id, data);

                dialogMyBoxOn(data, true, function () {

                    myAjax_async("?mode=getDetail", o, function (data) {
                        var jd = json_decode(data);
                        //get 通報 LineNotify
                        var jd_linenotify_room_ids = json_decode(myAjax("api.aspx?mode=getLineNotifyRoomIds", ""), true);
                        var _tmp = new Array();
                        for (var i = 0, max_i = jd_linenotify_room_ids['data'].length; i < max_i; i++) {
                            var d = "<option value='" + jd_linenotify_room_ids['data'][i]["id"] + "'>" + jd_linenotify_room_ids['data'][i]["title"] + "</option>";
                            _tmp.push(d);
                        }
                        $("div[id^='mybox_div'] select[reqc='form_linenotify_send_ids']").html(implode("", _tmp));
                        //preset
                        for (var k in jd["data"][0]) {
                            $("div[id^='mybox_div'] input[reqc='form_" + k + "']").val(jd["data"][0][k]);
                            $("div[id^='mybox_div'] select[reqc='form_" + k + "']").val(jd["data"][0][k]);
                            $("div[id^='mybox_div'] textarea[reqc='form_" + k + "']").val(jd["data"][0][k]);
                        }
                    });

                    //儲存、取消
                    $("div[id^='mybox_div'] input[reqc='cancelBtn']").unbind("click").click(function () { //取消
                        if (confirm("取消嗎?")) {
                            dialogMyBoxOff();
                        }
                    });
                    $("div[id^='mybox_div'] input[reqc='saveBtn']").unbind("click").click(function () { //儲存(編輯)
                        var o = new Object();
                        o['id'] = $(this).attr('req_id');
                        o['title'] = $("div[id^='mybox_div'] input[reqc='form_title']").val().trim();
                        o['crawler_url'] = $("div[id^='mybox_div'] input[reqc='form_crawler_url']").val().trim();
                        o['youtube_url'] = $("div[id^='mybox_div'] input[reqc='form_youtube_url']").val().trim();
                        o['keyword'] = $("div[id^='mybox_div'] textarea[reqc='form_keyword']").val().trim();
                        o['keyword'] = str_replace("，", ",", o['keyword']);
                        o['keyword'] = str_replace(";", ",", o['keyword']);
                        o['linenotify_send_ids'] = $("div[id^='mybox_div'] select[reqc='form_linenotify_send_ids']").val().join(",");
                        o['status'] = $("div[id^='mybox_div'] select[reqc='form_status']").val().trim();
                        var message = form_check_empty(o, [
                            { "key": "title", "value": "Youtube 標題" },
                            { "key": "youtube_url", "value": "Youtube URL" },
                            { "key": "linenotify_send_ids", "value": "通報 LineNotify" },
                            { "key": "keyword", "value": "關鍵字" }
                        ]);
                        if (message != "") {
                            alert(message);
                            return;
                        }
                        myAjax_async("?mode=add_edit_action&domode=edit", o, function (data) {
                            var jd = json_decode(data);
                            if (jd['status'] == 'OK') {
                                smallComment("編輯完成", 3000, false, {});
                                setTimeout(function () {
                                    location.replace("?");
                                }, 1000);
                            }
                        });
                    });
                });
            });
        });
        //刪除鈕
        $("input[reqc='delBtn']").unbind("click").click(function () {
            if (confirm("刪除嗎?")) {
                var o = new Object();
                o["id"] = $(this).attr('req_id');
                myAjax_async("?mode=del_action", o, function (data) {
                    var jd = json_decode(data);
                    if (jd['status'] == 'OK') {
                        smallComment("刪除完成", 3000, false, {});
                        setTimeout(function () {
                            location.replace("?");
                        }, 1000);
                    }
                });
            }
        });
        $("input[reqc='add_btn']").unbind("click").click(function () {
            //新增通報群組
            myAjax_async("app/site_add_edit.html?_t=" + time(), "", function (data) {
                data = str_replace("{kind_title}", "新增", data);
                dialogMyBoxOn(data, true, function () {
                    //get 通報 LineNotify
                    var jd_linenotify_room_ids = json_decode(myAjax("api.aspx?mode=getLineNotifyRoomIds", ""), true);
                    var _tmp = new Array();
                    for (var i = 0, max_i = jd_linenotify_room_ids['data'].length; i < max_i; i++) {
                        var d = "<option value='" + jd_linenotify_room_ids['data'][i]["id"] + "'>" + jd_linenotify_room_ids['data'][i]["title"] + "</option>";
                        _tmp.push(d);
                    }
                    $("div[id^='mybox_div'] select[reqc='form_linenotify_send_ids']").html(implode("", _tmp));
                    //儲存、取消
                    $("div[id^='mybox_div'] input[reqc='cancelBtn']").unbind("click").click(function () { //取消
                        if (confirm("取消嗎?")) {
                            dialogMyBoxOff();
                        }
                    });
                    $("div[id^='mybox_div'] input[reqc='saveBtn']").unbind("click").click(function () { //儲存
                        var o = new Object();
                        o['title'] = $("div[id^='mybox_div'] input[reqc='form_title']").val().trim();
                        o['crawler_url'] = $("div[id^='mybox_div'] input[reqc='form_crawler_url']").val().trim();
                        o['youtube_url'] = $("div[id^='mybox_div'] input[reqc='form_youtube_url']").val().trim();
                        o['keyword'] = $("div[id^='mybox_div'] textarea[reqc='form_keyword']").val().trim();
                        o['keyword'] = str_replace("，", ",", o['keyword']);
                        o['keyword'] = str_replace(";", ",", o['keyword']);
                        o['linenotify_send_ids'] = $("div[id^='mybox_div'] select[reqc='form_linenotify_send_ids']").val().join(",");
                        o['status'] = $("div[id^='mybox_div'] select[reqc='form_status']").val().trim();
                        var message = form_check_empty(o, [
                            { "key": "title", "value": "Youtube 標題" },
                            { "key": "youtube_url", "value": "Youtube URL" },
                            { "key": "linenotify_send_ids", "value": "通報 LineNotify" },
                            { "key": "keyword", "value": "關鍵字" }
                        ]);
                        if (message != "") {
                            alert(message);
                            return;
                        }
                        myAjax_async("?mode=add_edit_action&domode=add", o, function (data) {
                            var jd = json_decode(data);
                            if (jd['status'] == 'OK') {
                                smallComment("新增完成", 3000, false, {});
                                setTimeout(function () {
                                    location.replace("?");
                                }, 1000);
                            }
                        });
                    });
                });
            })
        });
    </script>


<!--內容結束-->
</center>
<%
    my.echoBinary(my.file_get_contents(my.base_dir + "\\template\\body.html"));
%>
</html>
