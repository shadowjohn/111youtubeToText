<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="linenotify_manager.aspx.cs" Inherits="youtubeToText.linenotify_manager" %>

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
                    SELECT * FROM [linenotify_send]
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
                string POSTS_STRING = "id,title,linenotify_room_token";
                var POSTS = my.getGET_POST(POSTS_STRING, "POST");
                if (POSTS["id"] != "")
                {
                    POSTS["id"] = Convert.ToInt32(POSTS["id"].ToString());
                }
                GETS_STRING = "mode,domode";
                GETS = my.getGET_POST(GETS_STRING, "GET");
                var o = new Dictionary<string, string>();
                o["title"] = POSTS["title"].ToString();
                o["linenotify_room_token"] = POSTS["linenotify_room_token"].ToString();
                switch (GETS["domode"].ToString())
                {
                    case "add":
                        {

                            my.insertSQL("linenotify_send", o);
                        }
                        break;
                    case "edit":
                        {
                            var mpa = new Dictionary<string, string>();
                            mpa["id"] = POSTS["id"].ToString();
                            my.updateSQL_SAFE("linenotify_send", o, "[id]=@id", mpa);
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
        case "del_action":
            {
                string POSTS_STRING = "id";
                var POSTS = my.getGET_POST(POSTS_STRING, "POST");
                POSTS["id"] = Convert.ToInt32(POSTS["id"].ToString());
                my.linkToDB();
                var mpa = new Dictionary<string, string>();
                mpa["id"] = POSTS["id"].ToString();
                my.deleteSQL_SAFE("linenotify_send", "[id]=@id", mpa);
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
            [linenotify_room_token]
        FROM 
            [linenotify_send]
        WHERE
            [del]='0'
    ";
    var rra = my.selectSQL_SAFE(SQL);
    var ra = my.datatable2dictinyary(rra);
    my.closeDB();
    //增加編輯與刪除
    for (int i = 0, max_i = ra.Count; i < max_i; i++)
    {
        ra[i]["option"] = "<input type='button' value='編輯' reqc='editBtn' req_id='" + ra[i]["id"] + "'>";
        ra[i]["option"] += "&nbsp;&nbsp;&nbsp;";
        ra[i]["option"] += "<input type='button' value='刪除' class='red' reqc='delBtn' req_id='" + ra[i]["id"] + "'>";
    }
    string table = my.print_table(ra, "id,title,linenotify_room_token,option", "序號,房間名稱,Line Token,功能", "thetable");
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

<h2>LineNotify維護</h2>

<br>

<!--內容開始-->
    <div align="right">
        <input type="button" reqc="add_btn" value="新增通報群組" /><br />        
    </div>

    <% my.echoBinary(table); %>
    <script>
        //編輯
        $("input[reqc='editBtn']").unbind("click").click(function () {
            var id = $(this).attr('req_id');
            var o = new Object();
            o["id"] = $(this).attr('req_id');
            myAjax_async("app/linenotify_manager_add_edit.html?_t=" + time(), "", function (data) {
                data = str_replace("{kind_title}", "編輯", data);
                data = str_replace("{id}", id, data);

                dialogMyBoxOn(data, true, function () {

                    myAjax_async("?mode=getDetail", o, function (data) {
                        var jd = json_decode(data);
                        //preset
                        for (var k in jd["data"][0]) {
                            $("div[id^='mybox_div'] input[reqc='form_" + k + "']").val(jd["data"][0][k]);
                        }
                    });

                    //儲存、取消
                    $("div[id^='mybox_div'] input[reqc='cancelBtn']").unbind("click").click(function () { //取消
                        if (confirm("取消嗎?")) {
                            dialogMyBoxOff();
                        }
                    });
                    $("div[id^='mybox_div'] input[reqc='saveBtn']").unbind("click").click(function () { //儲存
                        var o = new Object();
                        o['id'] = $(this).attr('req_id');
                        o['title'] = $("div[id^='mybox_div'] input[reqc='form_title']").val().trim();
                        o['linenotify_room_token'] = $("div[id^='mybox_div'] input[reqc='form_linenotify_room_token']").val().trim();
                        var message = form_check_empty(o, [{ "key": "title", "value": "房間名稱" }, { "key": "linetoken", "value": "Line Token" }]);
                        if (message != "") {
                            alert(message);
                            return;
                        }
                        myAjax_async("linenotify_manager.aspx?mode=add_edit_action&domode=edit", o, function (data) {
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
            myAjax_async("app/linenotify_manager_add_edit.html?_t=" + time(), "", function (data) {
                data = str_replace("{kind_title}", "新增", data);
                dialogMyBoxOn(data, true, function () {
                    //儲存、取消
                    $("div[id^='mybox_div'] input[reqc='cancelBtn']").unbind("click").click(function () { //取消
                        if (confirm("取消嗎?")) {
                            dialogMyBoxOff();
                        }
                    });
                    $("div[id^='mybox_div'] input[reqc='saveBtn']").unbind("click").click(function () { //儲存
                        var o = new Object();
                        o['title'] = $("div[id^='mybox_div'] input[reqc='form_title']").val().trim();
                        o['linenotify_room_token'] = $("div[id^='mybox_div'] input[reqc='form_linenotify_room_token']").val().trim();
                        var message = form_check_empty(o, [{ "key": "title", "value": "房間名稱" }, { "key": "linetoken", "value": "Line Token" }]);
                        if (message != "") {
                            alert(message);
                            return;
                        }
                        myAjax_async("linenotify_manager.aspx?mode=add_edit_action&domode=add", o, function (data) {
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
