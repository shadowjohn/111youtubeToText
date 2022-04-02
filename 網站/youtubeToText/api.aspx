<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="api.aspx.cs" Inherits="SystemReport.api" %>

<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="System.Collections.Concurrent" %>
<%@ Import Namespace="utility" %>
<%@ Import Namespace="Newtonsoft.Json" %>
<%@ Import Namespace="Newtonsoft.Json.Linq" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="System.Data.SqlClient" %>
<%@ Import Namespace="System.Diagnostics" %>
<%@ Import Namespace="System.Threading" %>
<% 
    myinclude my = new myinclude();
    my.allowAjaxHeader();
    string GETS_STRING = "mode";
    var GETS = my.getGET_POST(GETS_STRING, "GET");
    switch (GETS["mode"].ToString())
    {
        case "getGDCode":
            {
                if (Session["GD_CODE"] != null)
                {
                    my.echoBinary(Session["GD_CODE"].ToString());
                }
            }
            my.exit();
            break;
        case "t":
            {
                int mdt = Convert.ToInt32(Math.Floor((Convert.ToDouble(my.time()) / 60.0)));
                my.echo(mdt.ToString());
                my.exit();
                long st = Convert.ToInt64(my.microtime());
                Thread.Sleep(3000);
                long et = Convert.ToInt64(my.microtime());
                my.echo((et - st).ToString());
                //my.echo(my.size_hum_read_v2(21341242134));
                //my.echo(my.date("N",(Convert.ToInt64(my.time())+7*24*60*60).ToString()));
            }
            my.exit();
            break;
        case "runCosmosEvent":
            {
                my.checkpassword();
                if (!my.isAdmin())
                {
                    my.exit();
                }
                //From : https://rollbar.com/guides/where-are-dotnet-errors-logged/
                //事件類型：Application(應用程式)、Security(安全性)、Setup(Setup)、System(系統)、ForwardedEvents(Forwarded Events)
                var mkinds = new List<string> { "Application", "System" };
                //"Security" ,"Setup"
                Dictionary<string, object> OUTPUT = new Dictionary<string, object>();
                List<Dictionary<string, string>> Logs = new List<Dictionary<string, string>>();
                for (int k = 0, max_k = mkinds.Count(); k < max_k; k++)
                {
                    string eventLogName = mkinds[k]; // "Application";
                    EventLog eventLog = new EventLog();
                    eventLog.Log = eventLogName;
                    int step = 0;
                    for (int i = eventLog.Entries.Count - 1; i >= 0; i--)
                    {
                        EventLogEntry log = eventLog.Entries[i];
                        Dictionary<string, string> d = new Dictionary<string, string>();
                        d["events_kind"] = eventLogName;
                        d["Index"] = log.Index.ToString();
                        d["Category"] = log.Category;
                        d["Message"] = log.Message;
                        d["DateTime"] = log.TimeGenerated.ToString("yyyy-MM-dd HH:mm:ss");
                        Logs.Add(d);
                        //my.echo(log.Message + "\n");
                        step++;
                        if (step >= 50)
                        {
                            break;
                        }
                    }

                }
                OUTPUT["status"] = "OK";
                OUTPUT["data"] = Logs;
                //OUTPUT["kinds"] = my.json_encode(EventLog.GetEventLogs());
                my.echoBinary(my.s2b(my.base64_encode(my.s2b(my.json_encode(OUTPUT)))));
            }
            my.exit();
            break;
        case "runCosmosHDD":
            {
                my.checkpassword();
                if (!my.isAdmin())
                {
                    my.exit();
                }
                Dictionary<string, object> OUTPUT = new Dictionary<string, object>();
                DriveInfo[] drives = DriveInfo.GetDrives();
                List<Dictionary<string, string>> HDDS = new List<Dictionary<string, string>>();
                foreach (DriveInfo drive in drives)
                {
                    //Console.WriteLine(drive.Name);
                    if (!drive.IsReady) continue;
                    // Console.WriteLine(drive.TotalSize);
                    Dictionary<string, string> d = new Dictionary<string, string>();
                    d["Name"] = drive.Name.ToString();
                    d["TotalSize"] = drive.TotalSize.ToString();
                    d["FreeSpace"] = drive.AvailableFreeSpace.ToString();
                    HDDS.Add(d);
                }
                OUTPUT["status"] = "OK";
                OUTPUT["data"] = HDDS;
                my.echoBinary(my.s2b(my.base64_encode(my.s2b(my.json_encode(OUTPUT)))));
            }
            my.exit();
            break;
        case "runCosmos":
            {
                my.checkpassword();
                if (!my.isAdmin())
                {
                    my.exit();
                }
                my.linkToDB();
                string POSTS_STRING = "Cosmos";
                var POSTS = my.getGET_POST(POSTS_STRING, "POST");
                POSTS["Cosmos"] = my.b2s(my.base64_decode(POSTS["Cosmos"].ToString()));
                string SQL = POSTS["Cosmos"].ToString();
                Dictionary<string, object> OUTPUT = new Dictionary<string, object>();
                if (my.is_string_like(SQL, "DROP"))
                {
                    OUTPUT["STATUS"] = "NO";
                    OUTPUT["REASON"] = "不允許 DROP...";
                    my.echoBinary(my.s2b(my.base64_encode(my.s2b(my.json_encode(OUTPUT)))));
                    my.closeDB();
                    my.exit();
                }
                if (my.is_string_like(SQL, "OUTFILE"))
                {
                    OUTPUT["STATUS"] = "NO";
                    OUTPUT["REASON"] = "不允許 OUTFILE...";
                    my.echoBinary(my.s2b(my.base64_encode(my.s2b(my.json_encode(OUTPUT)))));
                    my.closeDB();
                    my.exit();
                }
                OUTPUT["STATUS"] = "OK";
                try
                {
                    Dictionary<string, object> jd = my.selectE(SQL);
                    if (jd["status"].ToString() == "OK")
                    {
                        var dt = (DataTable)jd["data"];
                        for (int i = dt.Rows.Count - 1; i >= 1000; i--)
                        {
                            dt.Rows.RemoveAt(i);
                        }
                        OUTPUT["DATA"] = dt;
                        my.echoBinary(my.s2b(my.base64_encode(my.s2b(my.json_encode(OUTPUT)))));
                    }
                    else
                    {
                        var m = new List<Dictionary<string, string>>();
                        var d = new Dictionary<string, string>();
                        d["error"] = jd["data"].ToString();
                        m.Add(d);
                        OUTPUT["DATA"] = m;
                        my.echoBinary(my.s2b(my.base64_encode(my.s2b(my.json_encode(OUTPUT)))));
                    }
                }
                catch (Exception ex)
                {
                    var m = new List<Dictionary<string, string>>();
                    var d = new Dictionary<string, string>();
                    d["error"] = ex.Message + "<br>" + ex.StackTrace;
                    m.Add(d);
                    OUTPUT["DATA"] = m;
                    my.echoBinary(my.s2b(my.base64_encode(my.s2b(my.json_encode(OUTPUT)))));
                }
                //寫入 log
                string LOG_DIR = my.base_dir + "\\log\\runCosmos";
                if (!my.is_dir(LOG_DIR))
                {
                    my.mkdir(LOG_DIR);
                }
                string LOG_FILE = LOG_DIR + "\\" + my.date("Y-m-d") + ".txt";
                if (!my.is_file(LOG_FILE))
                {
                    my.file_put_contents(LOG_FILE, "");
                }
                string data = string.Format("\r\n\r\n時間：{0} , IP：{1}：\r\n{2}", my.date("Y-m-d H:i:s"), my.ip(), SQL);
                my.file_put_contents(LOG_FILE, data, true);
            }
            my.closeDB();
            my.exit();
            break;
        /*case "t":
            {
                Response.Write("<br/>1. " + HttpContext.Current.Request.Url.Scheme);
                Response.Write("<br/>2. " + HttpContext.Current.Request.Url.OriginalString);
                Response.Write("<br/>3. " + HttpContext.Current.Request.RawUrl);
                Response.Write("<br/>4. " + HttpContext.Current.Request.Url.Host);
                Response.Write("<br/>5. " + HttpContext.Current.Request.Url.Authority);
                Response.Write("<br/>6. " + HttpContext.Current.Request.Url.Port);
                Response.Write("<br/>7. " + HttpContext.Current.Request.Url.AbsolutePath);
                Response.Write("<br/>8. " + HttpContext.Current.Request.ApplicationPath);
                Response.Write("<br/>9. " + HttpContext.Current.Request.Url.AbsoluteUri);
                Response.Write("<br/>10. " + HttpContext.Current.Request.Url.PathAndQuery);
                Response.Write("<br/>11. " + HttpContext.Current.Request.Url.LocalPath);
                Response.Write("<br/>12. " + HttpContext.Current.Request.ApplicationPath);
                Response.Write("<br/>13. " + Directory.GetParent(System.Web.HttpContext.Current.Request.PhysicalPath).FullName);


                string baseURL = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority + HttpContext.Current.Request.ApplicationPath;
            }
            my.exit();
            break;
            */
        case "tt":
            {
                my.linkToDB();
                string SQL = @"
                    select * FROM [argument]
                ";
                var ra = my.selectSQL_SAFE(SQL, new Dictionary<string, string>());
                string table = my.print_table(ra, "title,name", "標題,名稱", "thetable");
                my.echoBinary(table);
                my.closeDB();
            }
            my.exit();
            break;
        case "checkGDCODE":
            {
                //檢查驗證碼對不對的
                //正確會回應 OK
                string POSTS_STRING = "GD_CODE";
                var POSTS = my.getGET_POST(POSTS_STRING, "POST");
                if (POSTS["GD_CODE"].ToString() == Session["GD_CODE"].ToString())
                {
                    my.echoBinary("OK");
                }
            }
            my.exit();
            break;
        case "test":
            {
                my.echoBinary("HelloWorld!");
            }
            my.exit();
            break;
        case "apiLists":
            {
                my.echoBinary(my.b2s(my.file_get_contents(my.base_dir + "/apiLists.json")));
            }
            my.exit();
            break;
        case "isLink":
            {
                my.echoBinary("OK");
            }
            my.exit();
            break;
        case "getTime":
            {
                my.echoBinary(my.date("Y-m-d H:i:s"));
            }
            my.exit();
            break;
        case "add_GET":
            {
                GETS_STRING = "A,B";
                GETS = my.getGET_POST(GETS_STRING, "GET");
                my.echoBinary((Convert.ToDouble(GETS["A"].ToString()) + Convert.ToDouble(GETS["B"].ToString())).ToString());
            }
            my.exit();
            break;
        case "add_POST":
            {
                string POSTS_STRING = "A,B";
                var POSTS = my.getGET_POST(POSTS_STRING, "POST");
                my.echoBinary((Convert.ToDouble(POSTS["A"].ToString()) + Convert.ToDouble(POSTS["B"].ToString())).ToString());
            }
            my.exit();
            break;
        case "json_decode":
            {
                string POSTS_STRING = "inputdata";
                var POSTS = my.getGET_POST(POSTS_STRING, "POST");
                POSTS["inputdata"] = my.htmlspecialchars_decode(POSTS["inputdata"].ToString());
                POSTS["inputdata"] = my.stripslashes(POSTS["inputdata"].ToString());
                var o = my.json_decode(POSTS["inputdata"].ToString());
                //pre_print_r(o);
                my.exit();
            }
            break;
        case "get_room_name":
            {
                //設定房間名稱，來自：https://3wa.tw/demo/php/gis_skype/callback.php
                my.linkToDB();
                string SQL = @"
                    SELECT
                      [roomName],
                      [roomId]
                    FROM
                      [skype_chat_rooms]
                    ORDER BY 
                      [id] DESC
                  ";
                var ra = my.selectSQL_SAFE(SQL, new Dictionary<string, string>());
                my.closeDB();
                my.echoBinary(my.print_csv(ra, "roomName,roomId", "房間名稱,房間編號", false));
            }
            my.exit();
            break;
        case "set_room_name":
            {
                //設定房間名稱，來自：https://3wa.tw/demo/php/gis_skype/callback.php
                string POSTS_STRING = "room_id,room_name";
                var POSTS = my.getGET_POST(POSTS_STRING, "POST");
                my.linkToDB();
                //寫入房間編號
                string SQL = @"
                SELECT
                TOP 1
                  [id]
                FROM [skype_chat_rooms]
                WHERE
                  1 = 1
                  AND [roomId]=@roomId
                ORDER BY
                  [id] DESC                
              ";
                var pa = new Dictionary<string, string>();
                pa["roomId"] = POSTS["room_id"].ToString();
                var ra = my.selectSQL_SAFE(SQL, pa);
                my.closeDB();
                if (my.count(ra) == 0)
                {
                    var m_skypeRooms = new Dictionary<string, string>();
                    m_skypeRooms["roomId"] = POSTS["room_id"].ToString();
                    m_skypeRooms["roomName"] = POSTS["room_name"].ToString();
                    my.insertSQL("skype_chat_rooms", m_skypeRooms);
                }
                else
                {
                    var m_skypeRooms = new Dictionary<string, string>();
                    m_skypeRooms["roomId"] = POSTS["room_id"].ToString();
                    m_skypeRooms["roomName"] = POSTS["room_name"].ToString();
                    Dictionary<string, string> mpa = new Dictionary<string, string>();
                    mpa["id"] = ra.Rows[0]["id"].ToString();
                    my.updateSQL_SAFE("skype_chat_rooms", m_skypeRooms, "[id]=@id", mpa);
                }
                my.echoBinary("OK");
            }
            my.exit();
            break;
        case "skype_send":

            {
                //發 skype 功能
                string POSTS_STRING = "room_ids,say_word";
                var POSTS = my.getGET_POST(POSTS_STRING, "POST");
                POSTS["room_ids"] = my.stripslashes(my.htmlspecialchars_decode(POSTS["room_ids"].ToString()));
                POSTS["say_word"] = my.stripslashes(my.htmlspecialchars_decode(POSTS["say_word"].ToString()));
                var _m = my.explode("\n", my.trim(POSTS["room_ids"].ToString()));
                foreach (string room_id in _m)
                {
                    var o = new ConcurrentDictionary<string, string>();
                    o["room_id"] = my.trim(room_id);
                    o["say"] = POSTS["say_word"].ToString();
                    string SKYPE_URL = my.getArgument("skypebot_url"); //"http://3wa.tw/skype_bot/api.php?say=";
                                                                       //SKYPE_URL .= urlencode(SAY_WORD);
                                                                       //file_get_contents_post(SKYPE_URL,o);
                    my.curl_getPost_INIT(SKYPE_URL, o, null);
                    var x = new Dictionary<string, string>();
                    x["room_id"] = o["room_id"];
                    x["say"] = o["say"];
                    x["SKYPE_URL"] = SKYPE_URL;
                    my.echoBinary(my.json_encode(x));
                }
            }
            my.exit();
            break;
        case "update_status":
            {
                //更新檢測結果
                string POSTS_STRING = "id,response_time,last_status,last_size,last_getdata,last_status_info,ssl_expire_date,domain_expire_date,error_log";
                var POSTS = my.getGET_POST(POSTS_STRING, "POST");

                if (!my.is_dir(my.pwd() + "\\log"))
                {
                    my.mkdir(my.pwd() + "\\log");
                }
                //my.file_put_contents(my.pwd() + "\\log\\" + my.date("Ymd") + ".txt", my.json_encode(POSTS) + "\r\n", true);

                if (POSTS["id"].ToString() != "")
                {
                    POSTS["id"] = Convert.ToInt32(POSTS["id"].ToString());
                }
                if (POSTS["response_time"].ToString() != "")
                {
                    POSTS["response_time"] = string.Format("{0:0}", Convert.ToDouble(POSTS["response_time"].ToString()));
                }
                if (POSTS["last_status"].ToString() != "")
                {
                    POSTS["last_status"] = Convert.ToInt32(POSTS["last_status"].ToString());
                }
                if (POSTS["last_size"].ToString() != "")
                {
                    POSTS["last_size"] = Convert.ToInt64(POSTS["last_size"].ToString());
                }


                var m = new Dictionary<string, string>();
                m["response_time"] = POSTS["response_time"].ToString();
                m["last_status"] = POSTS["last_status"].ToString();
                m["last_size"] = POSTS["last_size"].ToString();
                m["last_status_info"] = POSTS["last_status_info"].ToString();
                m["last_datetime"] = my.date("Y-m-d H:i:s");
                m["last_getdata"] = my.stripslashes(my.htmlspecialchars_decode(POSTS["last_getdata"].ToString()));
                m["error_log"] = POSTS["error_log"].ToString();
                if (POSTS["ssl_expire_date"].ToString() != "")
                {
                    m["ssl_expire_date"] = POSTS["ssl_expire_date"].ToString();
                }
                if (POSTS["domain_expire_date"].ToString() != "")
                {
                    //domain 到期時間?
                    m["domain_expire_date"] = POSTS["domain_expire_date"].ToString();
                }
                if (m["last_status"] == "2")
                {
                    //正常
                    //當發生正常時，失敗次數就歸零
                    m["fail_time"] = "0";
                }
                var mpa = new Dictionary<string, string>();
                mpa["id"] = POSTS["id"].ToString();
                my.linkToDB();

                //my.file_put_contents(my.pwd()+"\\tmp\\log.txt", my.json_format(my.json_encode(m)), true);
                my.updateSQL_SAFE("site_item", m, "[id]=@id", mpa);


                //2021-09-01 新加，如果憑證時間減現在時間，小於7日，且要發警告，就發
                if (POSTS["ssl_expire_date"].ToString() == "")
                {
                    POSTS["ssl_expire_date"] = "";
                }
                long dt_time = Convert.ToInt64(my.strtotime(POSTS["ssl_expire_date"].ToString())) - Convert.ToInt64(my.time());
                //憑證整點通知一次就好
                if (POSTS["ssl_expire_date"].ToString() != "" && dt_time <= 7 * 24 * 60 * 60 && (Convert.ToInt32(my.date("i")) >= 55 || Convert.ToInt32(my.date("i")) <= 5))
                {
                    string SQL = @"
                                  SELECT
                                    TOP 1
                                      [si].*,
                                      [s].[room_id] 
                                    FROM
                                      [site_item] AS [si],
                                      [site] AS [s] 
                                    WHERE
                                      1 = 1
                                      AND [si].[site_id]=[s].[id]
                                      AND [si].[id]=@id
                                      AND [si].[is_need_alert]= '1'
                                      AND [si].[is_need_alert_ssl]= '1'
                                      AND [si].[kind]= 'WEB'
                                      AND [si].[del]= '0'                                   
                                ";
                    var pa = new Dictionary<string, string>();
                    pa["id"] = POSTS["id"].ToString();
                    var ra = my.selectSQL_SAFE(SQL, pa);
                    if (my.count(ra) != 0)
                    {
                        string _title = "SSL 憑證已過期";
                        if (dt_time > 0)
                        {
                            _title = "SSL 憑證即將過期";
                        }
                        string SAY_WORD = _title + "：" + ra.Rows[0]["name"].ToString() + "\n網站：" + ra.Rows[0]["URL"].ToString() + "\n過期日期：" + ra.Rows[0]["ssl_expire_date"].ToString() + "\n\n詳見：" + my.base_real_url;
                        var _m = my.explode("\n", my.trim(ra.Rows[0]["room_id"].ToString()));
                        foreach (string room_id in _m)
                        {
                            var o = new ConcurrentDictionary<string, string>();
                            o["room_id"] = my.trim(room_id);

                            o["say"] = SAY_WORD;

                            string SKYPE_URL = my.getArgument("skypebot_url"); //"http://3wa.tw/skype_bot/api.php?say=";
                                                                               //SKYPE_URL .= urlencode(SAY_WORD);
                                                                               //file_get_contents_post(SKYPE_URL,o);
                            my.curl_getPost_INIT(SKYPE_URL, o, null);
                        }
                    }
                }

                //domain name
                //2021-09-01 新加，如果domain name expire時間減現在時間，小於7日，且要發警告，就發
                if (POSTS["domain_expire_date"].ToString() == "")
                {
                    POSTS["domain_expire_date"] = "";
                }
                dt_time = Convert.ToInt64(my.strtotime(POSTS["domain_expire_date"].ToString())) - Convert.ToInt64(my.time());
                if (POSTS["domain_expire_date"].ToString() != "" && dt_time <= 7 * 24 * 60 * 60 && (
                        Convert.ToInt32(my.date("i")) >= 55 || Convert.ToInt32(my.date("i")) <= 5))
                {
                    string SQL = @"
                                  SELECT
                                      TOP 1
                                      [si].*,
                                      [s].[room_id]
                                    FROM
                                      [site_item] AS [si],
                                      [site] AS [s]
                                    WHERE
                                      1 = 1
                                      AND [si].[site_id]=[s].[id]
                                      AND [si].[id]=@id
                                      AND [si].[is_need_alert]= '1'
                                      AND [si].[is_need_alert_domain]= '1'
                                      AND [si].[kind]= 'WEB'
                                      AND [si].[del]= '0'                                    
                                ";
                    var pa = new Dictionary<string, string>();
                    pa["id"] = POSTS["id"].ToString();
                    var ra = my.selectSQL_SAFE(SQL, pa);
                    if (my.count(ra) != 0)
                    {
                        string _title = "Domain name 已過期";
                        if (dt_time > 0)
                        {
                            _title = "Domain name 即將過期";
                        }
                        string SAY_WORD = _title + "：" + ra.Rows[0]["name"].ToString() + "\n網站：" + ra.Rows[0]["URL"].ToString() + "\n過期日期：" + ra.Rows[0]["domain_expire_date"].ToString() + "\n\n詳見：" + my.base_real_url;
                        var _m = my.explode("\n", my.trim(ra.Rows[0]["room_id"].ToString()));
                        foreach (string room_id in _m)
                        {
                            var o = new ConcurrentDictionary<string, string>();
                            o["room_id"] = my.trim(room_id);
                            o["say"] = SAY_WORD;
                            string SKYPE_URL = my.getArgument("skypebot_url"); //"http://3wa.tw/skype_bot/api.php?say=";
                                                                               //SKYPE_URL .= urlencode(SAY_WORD);
                                                                               //file_get_contents_post(SKYPE_URL,o);
                            my.curl_getPost_INIT(SKYPE_URL, o, null);
                        }
                    }
                }


                if (m["last_status"] == "3")
                {
                    //異常        
                    string SQL = "UPDATE [site_item] SET [fail_time]=[fail_time]+1 WHERE [id]=@id";
                    var pa = new Dictionary<string, string>();
                    pa["id"] = POSTS["id"].ToString();
                    my.execSQL_SAFE(SQL, pa);
                    //如果異常次數，大於資料庫定義次數
                    //發警報
                    SQL = @"
                                  SELECT
                                    TOP 1
                                    [si].*,
                                    [s].[room_id]
                                  FROM
                                    [site_item] AS [si],
                                    [site] AS [s]
                                  WHERE
                                    1 = 1
                                    AND [si].[site_id] =[s].[id]
                                    AND [si].[id] = @id
                                    AND [si].[fail_time] != '0'
                                    AND [si].[fail_time] % [si].[fail_times_to_alert] = 0
                                    AND [si].[is_need_alert] = '1'
                                    AND [si].[del] = '0'
                                ";

                    pa = new Dictionary<string, string>();
                    pa["id"] = POSTS["id"].ToString();
                    var ra = my.selectSQL_SAFE(SQL, pa);
                    if (my.count(ra) >= 1)
                    {
                        string SAY_APPEND = "";
                        switch (ra.Rows[0]["kind"].ToString())
                        {
                            case "WEB":
                                {
                                    SAY_APPEND = "網址：" + ra.Rows[0]["URL"].ToString();
                                }
                                break;
                            case "FTP":
                                {
                                    SAY_APPEND = "FTP：" + ra.Rows[0]["ftp_ip"].ToString() + " , " + ra.Rows[0]["ftp_port"].ToString();
                                }
                                break;
                            case "PORT":
                                {
                                    SAY_APPEND = "PORT：" + ra.Rows[0]["PORT_IP"].ToString() + " , " + ra.Rows[0]["PORT"].ToString();
                                }
                                break;
                        }
                        string SAY_WORD = "服務異常：【" + ra.Rows[0]["kind"].ToString() + "】" + ra.Rows[0]["name"].ToString() + "\n" + SAY_APPEND + "\n詳見：" + my.base_real_url;
                        var _m = my.explode("\n", my.trim(ra.Rows[0]["room_id"].ToString()));
                        foreach (string room_id in _m)
                        {
                            var o = new ConcurrentDictionary<string, string>();
                            o["room_id"] = my.trim(room_id);
                            o["say"] = SAY_WORD;
                            string SKYPE_URL = my.getArgument("skypebot_url"); //"http://3wa.tw/skype_bot/api.php?say=";
                                                                               //SKYPE_URL .= urlencode(SAY_WORD);
                                                                               //file_get_contents_post(SKYPE_URL,o);
                            my.curl_getPost_INIT(SKYPE_URL, o, null);
                        }
                    }
                }
                var mm = new Dictionary<string, string>();
                mm["site_item_id"] = POSTS["id"].ToString();
                mm["datetime"] = my.date("Y-m-d H:i:s");
                mm["dt"] = my.time();
                mm["response_time"] = POSTS["response_time"].ToString();
                mm["status"] = (m["last_status"] == "2") ? "正常" : "異常";
                mm["reason"] = POSTS["last_status_info"].ToString();

                mm["IP"] = my.ip();
                if (m["last_status"] != "2")
                {
                    //異常時，記下當初的錯誤資料
                    mm["site_data"] = POSTS["last_getdata"].ToString();
                }
                my.insertSQL("site_log", mm);
                mpa = new Dictionary<string, string>();
                mpa["dt"] = (Convert.ToInt64(my.time()) - 365 * 24 * 60 * 60).ToString();
                my.deleteSQL_SAFE("site_log", " [dt] < @dt ", mpa); //超過一年不要了      
                                                                    //deleteSQL_SAFE('site_log'," DATE([datetime]) < CURDATE() - INTERVAL 365 DAY ",ARRAY()); //先改一年      
                my.closeDB();
            }
            my.exit();
            break;
        case "test_skype":
            {
                GETS_STRING = "id";
                GETS = my.getGET_POST(GETS_STRING, "GET");
                GETS["id"] = Convert.ToInt32(GETS["id"].ToString());

                my.linkToDB();
                string SQL = "SELECT TOP 1 [room_id] FROM [site] WHERE [id]=@id";
                var pa = new Dictionary<string, string>();
                pa["id"] = GETS["id"].ToString();
                var ra = my.selectSQL_SAFE(SQL, pa);
                my.closeDB();
                if (my.count(ra) == 0)
                {
                    my.echo("No data...");
                    my.exit();
                }
                var _m = my.explode("\n", my.trim(ra.Rows[0]["room_id"].ToString()));
                foreach (string room_id in _m)
                {
                    var o = new ConcurrentDictionary<string, string>();
                    o["room_id"] = my.trim(room_id);
                    o["say"] = "這是小桃子的測試~~~" + my.date("Y-m-d H:i:s");
                    string SKYPE_URL = my.getArgument("skypebot_url"); //"http://3wa.tw/skype_bot/api.php?say=";
                                                                       //SKYPE_URL .= urlencode(SAY_WORD);
                                                                       //file_get_contents_post(SKYPE_URL,o);
                    my.curl_getPost_INIT(SKYPE_URL, o, null);
                    var x = new Dictionary<string, string>();
                    x["room_id"] = o["room_id"];
                    x["say"] = o["say"];
                    x["SKYPE_URL"] = SKYPE_URL;
                    my.echoBinary(my.json_encode(x));
                }
            }
            my.exit();
            break;
        case "remove_project":
            {
                //刪除專案
                string POSTS_STRING = "site_id";
                var POSTS = my.getGET_POST(POSTS_STRING, "POST");
                POSTS["site_id"] = Convert.ToInt32(POSTS["site_id"].ToString());
                //移除子項
                var m = new Dictionary<string, string>();
                m["del"] = "1";
                var mpa = new Dictionary<string, string>();
                mpa["site_id"] = POSTS["site_id"].ToString();

                my.linkToDB();
                my.updateSQL_SAFE("site_item", m, "[site_id]=@site_id", mpa);
                //移除大項
                m = new Dictionary<string, string>();
                m["del"] = "1";

                mpa = new Dictionary<string, string>();
                mpa["id"] = POSTS["site_id"].ToString();

                my.updateSQL_SAFE("site", m, "[id]=@id", mpa);
                my.closeDB();
                my.echoBinary("OK");
            }
            my.exit();
            break;
        case "get_site_log":
            {
                string POSTS_STRING = "id,sdate,edate";
                var POSTS = my.getGET_POST(POSTS_STRING, "POST");
                POSTS["id"] = Convert.ToInt32(POSTS["id"].ToString());
                my.linkToDB();
                //成功與失敗的次數
                var PA = new Dictionary<string, string>();
                string SQL = @"
                              SELECT
                                '成功' AS [label],
                                COUNT([id]) AS [data],
                                (SELECT SUM([keep_second])
                                  FROM
                                    [site_log_sum]
                                  WHERE
                                    [site_item_id] = @site_item_id AND [status] = '正常'
                                  ";
                PA["site_item_id"] = POSTS["id"].ToString();

                if (POSTS["sdate"] != null && POSTS["sdate"].ToString() != "" && POSTS["edate"] != null && POSTS["edate"].ToString() != "")
                {
                    SQL += @" AND [dt] BETWEEN @sdate AND @edate ";
                    PA["sdate"] = my.strtotime(POSTS["sdate"].ToString());
                    PA["edate"] = my.strtotime(POSTS["edate"].ToString());
                }
                SQL += @"              
                                    ) AS [keep_second]
                                  FROM
                                    [site_log]
                                  WHERE
                                    1 = 1
                                    AND [site_item_id] = @site_item_id
                            ";
                PA["site_item_id"] = POSTS["id"].ToString();
                if (POSTS["sdate"] != null && POSTS["sdate"].ToString() != "" && POSTS["edate"] != null && POSTS["edate"].ToString() != "")
                {
                    SQL += @" AND [dt] BETWEEN @sdate AND @edate ";
                    PA["sdate"] = my.strtotime(POSTS["sdate"].ToString());
                    PA["edate"] = my.strtotime(POSTS["edate"].ToString());
                }
                SQL += @"
                                AND [status] = '正常'
                                  UNION
                                  SELECT
                                    '異常' AS [label],   
                                    COUNT([id]) AS [data],
                                    (SELECT SUM([keep_second]) FROM [site_log_sum] WHERE [site_item_id] = @site_item_id AND [status] = '異常'

                                ";
                PA["site_item_id"] = POSTS["id"].ToString();
                if (POSTS["sdate"] != null && POSTS["sdate"].ToString() != "" && POSTS["edate"] != null && POSTS["edate"].ToString() != "")
                {
                    SQL += @" AND [dt] BETWEEN @sdate AND @edate ";
                    PA["sdate"] = my.strtotime(POSTS["sdate"].ToString());
                    PA["edate"] = my.strtotime(POSTS["edate"].ToString());
                }
                SQL += @"                        
                                    ) 
                                  FROM
                                    [site_log]
                                  WHERE
                                    1 = 1
                                    AND [site_item_id] = @site_item_id
                                    AND [status] = '異常'
                              ";
                PA["site_item_id"] = POSTS["id"].ToString();
                //如果有時間條件
                if (POSTS["sdate"] != null && POSTS["sdate"].ToString() != "" && POSTS["edate"] != null && POSTS["edate"].ToString() != "")
                {
                    SQL += " AND [dt] BETWEEN @sdate AND @edate ";
                    PA["sdate"] = my.strtotime(POSTS["sdate"].ToString());
                    PA["edate"] = my.strtotime(POSTS["edate"].ToString());
                }
                //SQL.=" ORDER BY [site_log].[id] ASC ";

                //pre_print_r(SQL);
                //pre_print_r(PA);
                //my.exit();
                //my.echo(SQL);
                //my.exit();
                var ra_pie = my.selectSQL_SAFE(SQL, PA);

                var sum = 0;
                var sum_s = 0;
                var sum_f = 0;
                var sum_time = 0;
                var sum_time_s = 0;
                var sum_time_f = 0;

                for (int i = 0, max_i = my.count(ra_pie); i < max_i; i++)
                {
                    ra_pie.Rows[i]["data"] = Convert.ToInt32(ra_pie.Rows[i]["data"].ToString());
                    sum += Convert.ToInt32(ra_pie.Rows[i]["data"].ToString());



                    try
                    {
                        sum_time += Convert.ToInt32(ra_pie.Rows[i]["keep_second"].ToString());
                    }
                    catch
                    {
                        sum_time += 0;
                    }
                    switch (ra_pie.Rows[i]["label"].ToString())
                    {
                        case "成功":
                            sum_s += Convert.ToInt32(ra_pie.Rows[i]["data"].ToString());
                            try
                            {
                                sum_time_s += Convert.ToInt32(ra_pie.Rows[i]["keep_second"].ToString());
                            }
                            catch
                            {
                                sum_time_s += 0;
                            }
                            break;
                        default:
                            sum_f += Convert.ToInt32(ra_pie.Rows[i]["data"].ToString());
                            try
                            {
                                sum_time_f += Convert.ToInt32(ra_pie.Rows[i]["keep_second"].ToString());
                            }
                            catch
                            {
                                sum_time_f += 0;
                            }
                            break;
                    }
                }



                PA = new Dictionary<string, string>();
                SQL = @"
                                        SELECT
                                        TOP 300
                                          [A].*,
                                          [C].[name] AS [project_name],
                                          [B].[name]
                                        FROM
                                            [site_log] AS [A],
                                            [site_item] AS [B],
                                            [site] AS [C],
                                            [user] AS [D]
                                        WHERE
                                          1=1 
                                          AND [A].[site_item_id]=[B].[id]
                                          AND [B].[site_id]=[C].[id]
                                          AND [C].[user_id]=[D].[id]
                                          AND [B].[id]=@id
                                      ";
                //-- AND [C].[user_id]=?
                //array_push(PA,Session["{SESSION_PREFIX}_userID"]);
                PA["id"] = POSTS["id"].ToString();
                if (POSTS["sdate"] != null && POSTS["sdate"].ToString() != "" && POSTS["edate"] != null && POSTS["edate"].ToString() != "")
                {
                    SQL += " AND [A].[dt] BETWEEN @sdate AND @edate ";
                    PA["sdate"] = my.strtotime(POSTS["sdate"].ToString());
                    PA["edate"] = my.strtotime(POSTS["edate"].ToString());
                }
                SQL += @"
                                ORDER BY
                                  [A].[id]
                                    DESC
                                
                              ";
                var ra = my.selectSQL_SAFE(SQL, PA);
                if (ra.Rows.Count == 0)
                {
                    my.echo("尚無資料");
                    my.closeDB();
                    my.exit();
                }
                string title = "【" + ra.Rows[0]["project_name"].ToString() + "】" + ra.Rows[0]["name"].ToString();
                string table = my.print_table(ra, "datetime,status,response_time,reason", "掃描時間,狀況,回應時間,問題說明", "thetable_list");
                //歷史資料 - 圖表
                //-- 
                PA = new Dictionary<string, string>();
                SQL = @"
                    SELECT * FROM (
                              SELECT
                                  ROW_NUMBER() OVER (ORDER BY [id] ASC) AS [_num],
                                  [B].[_COUNTS],
                                  [id],       
      	                          [A].[site_item_id],
                                  ([A].[dt]*1000) AS [dt],
                                  [A].[status]
                                    FROM
                                    [site_log] AS [A],(
                                    SELECT
                                        COUNT([id])/2000 AS [_COUNTS] FROM [site_log]
                                    WHERE
            	                        1=1
                                      AND [site_item_id] = @site_item_id
                                      ";
                PA["site_item_id"] = POSTS["id"].ToString();


                if (POSTS["sdate"] != null && POSTS["sdate"].ToString() != "" && POSTS["edate"] != null && POSTS["edate"].ToString() != "")
                {
                    SQL += " AND [dt] BETWEEN @sdate AND @edate ";
                    PA["sdate"] = my.strtotime(POSTS["sdate"].ToString());
                    PA["edate"] = my.strtotime(POSTS["edate"].ToString());
                }
                SQL += @"
                              ) AS [B]
                              WHERE

                                  1 = 1
                                AND [A].[site_item_id] = @site_item_id
                              ";


                PA["site_item_id"] = POSTS["id"].ToString();
                //如果有時間條件
                if (POSTS["sdate"] != null && POSTS["sdate"].ToString() != "" && POSTS["edate"] != null && POSTS["edate"].ToString() != "")
                {
                    SQL += " AND [dt] BETWEEN @sdate AND @edate ";
                    PA["sdate"] = my.strtotime(POSTS["sdate"].ToString());
                    PA["edate"] = my.strtotime(POSTS["edate"].ToString());
                }
                SQL += @" 
                          ) AS [CC]
                           WHERE     FLOOR([CC].[_num] % case when [CC].[_COUNTS]=0 then 1 when [CC].[_COUNTS]!=0 then [CC].[_COUNTS] end ) = 0
                                OR [CC].[status] = '異常'
                            ";
                //如果 POSTS["edate'] 是今天，就包含最後30分鐘就很迷人了 
                if (POSTS["sdate"] != null && POSTS["sdate"].ToString() != "" && POSTS["edate"] != null && POSTS["edate"].ToString() != "")
                {
                    /*if(date('Y-m-d',strtotime(POSTS["edate'])) == date('Y-m-d'))
                    {
                      SQL.=' OR [dt] >= ? ';
                      array_push(PA,strtotime(date('Y-m-d'))); 
                    }
                    */
                }
                else
                {
                    SQL += @" OR [dt] >= @ss ";
                    PA["ss"] = ((Convert.ToInt64(my.time()) - 30 * 60) * 1000).ToString();  //包含最後30分鐘就很迷人了 
                }
                //my.echoBinaryPOSTS["edate"].ToString();
                //pre_print_r(SQL);
                //pre_print_r(PA);
                //my.exit();

                SQL += @" ORDER BY [id] ASC ";

                var ra_month = my.selectSQL_SAFE(SQL, PA);

                var raM = new Dictionary<string, object>();
                raM["label"] = ra.Rows[0]["project_name"].ToString() + " 系統狀況";
                raM["data"] = new List<object>();

                //頭尾時間
                SQL = @"
                              SELECT
                                min([dt]) AS [min_dt],
                                max([dt]) AS [max_dt]
                              FROM
                                [site_log]
                              WHERE
                                [site_item_id] = @site_item_id
                            ";
                PA = new Dictionary<string, string>();
                PA["site_item_id"] = POSTS["id"].ToString();
                if (POSTS["sdate"] != null && POSTS["sdate"].ToString() != "" && POSTS["edate"] != null && POSTS["edate"].ToString() != "")
                {
                    SQL += " AND [dt] BETWEEN @sdate AND @edate ";
                    PA["sdate"] = my.strtotime(POSTS["sdate"].ToString());
                    PA["edate"] = my.strtotime(POSTS["edate"].ToString());
                }
                var ra_se = my.selectSQL_SAFE(SQL, PA);
                //my.echo(my.json_encode(ra_se));
                //my.exit();
                ra_se.Columns.Add("fake_min_dt");
                ra_se.Columns.Add("fake_max_dt");
                for (int i = 0, max_i = my.count(ra_se); i < max_i; i++)
                {
                    ra_se.Rows[i]["fake_min_dt"] = my.date("Y-m-d H:i:s", ra_se.Rows[i]["min_dt"].ToString());
                    ra_se.Rows[i]["fake_max_dt"] = my.date("Y-m-d H:i:s", ra_se.Rows[i]["max_dt"].ToString());
                }
                for (int i = 0, max_i = my.count(ra_month); i < max_i; i++)
                {
                    string status = (ra_month.Rows[i]["status"].ToString() == "正常") ? "1" : "0";
                    //ra_month[i]['datetime']
                    var d = new List<object>();
                    d.Add(Convert.ToInt64(ra_month.Rows[i]["dt"].ToString()));
                    d.Add(status);
                    ((List<object>)raM["data"]).Add(d);
                    //array_push(raM['data'], ARRAY(ra_month[i]['dt'], status));
                }
                string SDATE = my.strtotime(ra_se.Rows[0]["fake_min_dt"].ToString());

                string EDATE = my.strtotime(ra_se.Rows[0]["fake_max_dt"].ToString());

                if (POSTS["sdate"] != null && POSTS["sdate"].ToString() != "" && POSTS["edate"] != null && POSTS["edate"].ToString() != "")
                {
                    SDATE = my.strtotime(POSTS["sdate"].ToString());
                    EDATE = my.strtotime(POSTS["edate"].ToString());
                }
%>
<center>
    <h3><% my.echoBinary(title);%> 掃站紀錄 </h3>
</center>
開始時間：<input type='text' reqc='sdate' value="<% my.echoBinary(my.date("Y-m-d", SDATE));%>">
&nbsp;
結束時間：<input type='text' reqc='edate' value="<% my.echoBinary(my.date("Y-m-d", EDATE));%>">

<script>
    $("input[reqc='sdate'],input[reqc='edate']").datepicker({
        'dateFormat': 'yy-mm-dd',
        'showButtonPanel': true,
        changeMonth: true,
        changeYear: true,
        yearRange: '<% my.echoBinary(my.date("Y", my.strtotime(ra_se.Rows[0]["fake_min_dt"].ToString())));%>:<% my.echoBinary(my.date("Y", my.strtotime(ra_se.Rows[0]["fake_max_dt"].ToString())));%>'
    });
</script>
<input type='button' reqc="queryBtn" value='查詢'>


<script>
    $("input[type='button'][reqc='queryBtn']").unbind("click").click(function () {
        var o = new Object();
        o["id"] = "<% my.echoBinary(POSTS["id"].ToString());%>";
        o["sdate"] = $("input[reqc='sdate']").val() + " 00:00:00";
        o["edate"] = $("input[reqc='edate']").val() + " 23:59:59";
        dialogMyBoxOn("請稍候...", true, function () {
            myAjax_async("api.aspx?mode=get_site_log", o, function (data) {
                dialogMyBoxOn(data, true, function () {
                });
            });
        });
    });
</script>
<table border="1" style="width: 100%;">
    <thead>
        <tr>
            <th>歷史紀錄 </th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>
                <div id="list" style="width: 1280px; height: 250px;"></div>
                <script>
                    var ddata = <% my.echoBinary(my.json_encode(raM));%>;
                    $("<div id='tooltip'></div>").css({
                        position: "absolute",
                        display: "none",
                        border: "1px solid #fdd",
                        padding: "2px",
                        "background-color": "#fee",
                        opacity: 0.80,
                        'z-index': 9999999999
                    }).appendTo("body");
                    $("#list").bind("plothover", function (event, pos, item) {
                        if (!pos.x || !pos.y) {
                            return;
                        }

                        if (item) {
                            var x = item.datapoint[0],
                                y = item.datapoint[1];
                            var fake_y = (y == 1) ? '正常' : '異常';
                            $("#tooltip").html(date('Y-m-d H:i:s', x / 1000) + " = " + fake_y)
                                .css({ top: item.pageY + 5, left: item.pageX + 5 })
                                .show();
                        }
                        else {
                            $("#tooltip").hide();
                        }
                    });
                    $.plot('#list', [ddata], {
                        series:
                        {
                            lines:
                            {
                                show: true

                            },
                            points:
                            {
                                show: true

                            }
                        },
                        grid:
                        {
                            hoverable: true,
                            clickable: true

                        },
                        yaxis:
                        {
                            min: -2,
                            max: 2,
                            tickSize: 1

                        },
                        xaxis:
                        {
                            mode: "time",
                            timeformat: "%Y-%m-%d" /* %H:%M:%S*/
                        },
                        zoom:
                        {
                            interactive: true

                        },
                        pan:
                        {
                            interactive: true,
                            enableTouch: true

                        }
                    });
                </script>
            </td>
        </tr>
        <tr>
            <td align="center">檢查時間範圍：<% my.echoBinary(ra_se.Rows[0]["min_dt"].ToString());%>～<% my.echoBinary(ra_se.Rows[0]["max_dt"].ToString());%><br>
                <style>
                    .t_class th {
                        text-align: right;
                    }

                    .t_class td {
                        text-align: right;
                    }
                </style>
                <table border='1' cellpadding='0' cellspacing='0' class='t_class'>
                    <tr>
                        <th>總嘗試次數：</th>
                        <td><% my.echoBinary(sum.ToString());%></td>
                        <th>總時間(分鐘)：</th>
                        <td><% my.echoBinary(sum_time.ToString());%> ( <% my.echoBinary(my.secondtodhis(Convert.ToInt64(sum_time.ToString())));%> )</td>
                    </tr>
                    <tr>
                        <th>正常次數：</th>
                        <td><% my.echoBinary(sum_s.ToString());%></td>
                        <th>正常時間(分鐘)：</th>
                        <td><% my.echoBinary(sum_time_s.ToString());%> ( <% my.echoBinary(my.secondtodhis(Convert.ToInt64(sum_time_s.ToString())));%> )</td>
                    </tr>
                    <tr>
                        <th>異常次數：</th>
                        <td><span class="red"><% my.echoBinary(sum_f.ToString());%></span></td>
                        <th>異常時間(分鐘)：</th>
                        <td><span class="red"><% my.echoBinary(sum_time_f.ToString());%> ( <% my.echoBinary(my.secondtodhis(Convert.ToInt64(sum_time_f.ToString())));%> )</span></td>
                    </tr>
                </table>
            </td>
        </tr>
    </tbody>
</table>
<h2 align="center">近期資料 </h2>
<div style='width: 1280px; max-height: 350px; overflow: auto;'>
    <style>
        .thetable_list {
            width: 100%;
        }
    </style>
    <%
        my.echoBinary(table);
    %>
</div>
<%
            }
            my.closeDB();
            my.exit();
            break;
        case "getNewestSystemCheckVersion":
            {
                //取得最新版本
                my.linkToDB();
                string SQL = @"
                    SELECT
                    TOP 1
                      *
                    FROM
                      [system_check_version]
                    WHERE
                      [status] = '1'
                    ORDER BY
                      [version] DESC
                    
                  ";
                var ra = my.selectSQL_SAFE(SQL, new Dictionary<string, string>());
                my.closeDB();
                var output = new Dictionary<string, string>();
                if (my.count(ra) == 0)
                {
                    output["status"] = "NO";
                    output["reason"] = "無法取得最新版...";
                    my.echoBinary(my.json_encode(output));
                    my.exit();
                }
                output["status"] = "OK";
                output["version"] = ra.Rows[0]["version"].ToString();
                output["notes"] = my.htmlspecialchars(ra.Rows[0]["notes"].ToString());

                output["downloadPath"] = my.base_url + "/api.aspx?mode=getNewestSystemCheckFile";
                my.echoBinary(my.json_encode(output));
            }
            my.exit();
            break;
        case "getNewestSystemCheckFile":
            {
                my.linkToDB();
                //抓最新版本檔案
                string SQL = @"
                TOP 1
                    SELECT
                      *
                    FROM
                      [system_check_version]
                    WHERE
                      [status] = '1'
                    ORDER BY
                      [version] DESC
        
                  ";
                var ra = my.selectSQL_SAFE(SQL, new Dictionary<string, string>());
                string id = ra.Rows[0]["id"].ToString();

                var output = new Dictionary<string, string>();
                if (my.count(ra) == 0)
                {
                    my.closeDB();
                    my.exit();
                }
                var PA = new Dictionary<string, string>();
                SQL = @"
                    UPDATE
                       [system_check_version]
                    SET
                      [counts] =[counts] + 1
                    WHERE
                      [id] = @id
                  ";
                PA["id"] = id;
                my.execSQL_SAFE(SQL, PA);
                my.closeDB();
                my.download_file(my.base_dir + "\\uploads\\" + id + ".exe", "system_check.exe");
            }
            my.exit();
            break;
        case "uploadSystemStatusVersion":
            {
                //上傳新版本
                //require "{base_dir}/inc/checkpassword.aspx";
                my.checkpassword();
                my.linkToDB();
                string POSTS_STRING = "version,notes";
                var POSTS = my.getGET_POST(POSTS_STRING, "POST");
                var m = new Dictionary<string, string>();
                m["version"] = string.Format("%.2f", POSTS["version"].ToString());

                m["notes"] = POSTS["notes"].ToString();
                m["datetime"] = my.date("Y-m-d H:i:s");
                string SQL = @"
                    SELECT
                    TOP 1
                      [id]
                    FROM
                      [system_check_version]
                    WHERE
                      1 = 1
                      AND [version] = @version
                   ORDER BY
                     [id] DESC
                  ";
                var pa = new Dictionary<string, string>();
                pa["version"] = m["version"];
                var ra = my.selectSQL_SAFE(SQL, pa);
                string LAST_ID = "";
                if (my.count(ra) == 0)
                {
                    LAST_ID = my.insertSQL("system_check_version", m).ToString();
                }
                else
                {
                    LAST_ID = ra.Rows[0]["id"].ToString();
                    var mpa = new Dictionary<string, string>();
                    mpa["id"] = ra.Rows[0]["id"].ToString();
                    my.updateSQL_SAFE("system_check_version", m, "[id]=@id", mpa);
                }
                HttpPostedFile file = Request.Files["tmp_name"];
                file.SaveAs(my.base_dir + "\\uploads\\" + LAST_ID + ".exe");
                my.closeDB();
                my.echoBinary("OK");
            }
            my.exit();
            break;
        case "cleanTransDB":
            {
                //定期清除DB交易紀錄
                //設定簡單模式無法使用，再觀察看看
                string SQL = @"            
                    USE SystemReport;
                    DECLARE @LogicalName nvarchar(128);
                    DECLARE @DataBaseName nvarchar(128);

                    SELECT @LogicalName =  f.name, @DataBaseName = d.name
                    FROM sys.master_files f
                    INNER JOIN sys.databases d ON d.database_id = f.database_id
                    where d.name = DB_NAME();

                    ALTER DATABASE CURRENT SET RECOVERY SIMPLE WITH NO_WAIT;
                    DBCC SHRINKFILE(@LogicalName, 1);
                    ALTER DATABASE CURRENT SET RECOVERY FULL WITH NO_WAIT;
                ";
                my.linkToDB();
                my.PDO.Execute(SQL);
                my.closeDB();
                my.echoBinary("OK");
            }
            my.exit();
            break;
        case "mail_test":
            {
                my.linkToDB();
                List<string> To = new List<string>();
                To.Add("john@gis.tw");
                To.Add("linainverseshadow@gmail.com");
                my.sendMail(To, "這是測試", "測看看能不能發信", null);
                my.closeDB();
            }
            my.exit();
            break;
    }
%>