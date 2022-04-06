using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _youtubeToTextCron;
using System.Collections.Concurrent;
using System.Threading;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace theapp
{
    public class youtube_app
    {
        private DataTable tasks = null; //所有工作        
        private ConcurrentDictionary<string, Thread> t_array = new ConcurrentDictionary<string, Thread>(); //工人進行中的工作
        //定義 M3U8 記憶體空間
        //id , M3U8 PATH
        ConcurrentDictionary<string, string> M3U8_MEMORY = new ConcurrentDictionary<string, string>();

        public string lastMinute = ""; //用來判斷此分鐘是否執行過了
        public string last30Second = ""; //用來判斷此30秒排程是否執行過了
        public string last40Second = ""; //用來判斷此40秒是否執行過了
        public string last2Hour = ""; //用來判斷此2小時是否執行過了
        public string last6Hour = ""; //用來判斷此6小時是否執行過了
        public bool isNeedStop = false; //是否要離開了        
        public youtube_app()
        {

        }
        public bool isRun1MinRunning = false;
        public bool isRun10MinRunning = false;
        private bool getRun1TasksIsDone()
        {
            if (tasks == null) return false;
            bool isDone = true;
            for (int i = 0; i < tasks.Rows.Count; i++)
            {
                if (tasks.Rows[i]["ThreadStatus"].ToString() != "2")
                {
                    isDone = false;
                }
            }
            return isDone;
        }
        public void run_2hour()
        {
            //每2小時更新所有 youtube 的 m3u8
            //每分鐘更新youtube下載路徑 gridview
            try
            {

                if (isNeedStop) //是否要離開 thread
                {
                    return;
                }
                if (last2Hour == "")
                {
                    last2Hour = Program.my.date("H");
                }
                else
                {
                    if ((
                            Program.my.date("H") != "00" &&
                            Program.my.date("H") != "02" &&
                            Program.my.date("H") != "04" &&
                            Program.my.date("H") != "06" &&
                            Program.my.date("H") != "08" &&
                            Program.my.date("H") != "10" &&
                            Program.my.date("H") != "12" &&
                            Program.my.date("H") != "14" &&
                            Program.my.date("H") != "16" &&
                            Program.my.date("H") != "18" &&
                            Program.my.date("H") != "20" &&
                            Program.my.date("H") != "22"
                        )
                        || last2Hour == Program.my.date("H")) //最後執行的「小時」與目前時間的「小時」如果是一樣，就再休息1分鐘，1 分鐘後再跑一次
                    {
                        try
                        {
                            Thread.Sleep(60 * 1000);
                        }
                        catch
                        {
                        }
                        Program.threads["MAIN_YT_RUN_2_HOUR"] = new Thread(() => run_2hour());
                        Program.threads["MAIN_YT_RUN_2_HOUR"].Start();
                        return;
                    }
                    else
                    {
                        last2Hour = Program.my.date("H");
                    }
                }
                //開始了
                Program.log("Prepare Run updateM3U8...");
                updateM3U8(false);

                //都作完了，等下一次工作
                try
                {
                    Thread.Sleep(60 * 1000);
                }
                catch
                {
                }

                Program.threads["MAIN_YT_RUN_2_HOUR"] = new Thread(() => run_2hour());
                Program.threads["MAIN_YT_RUN_2_HOUR"].Start();

                return;
            }
            catch (Exception ex)
            {
                Program.logError("Error...:" + ex.Message + "\r\n" + ex.StackTrace);
                //GC.Collect(); //回收 ram                
                Thread.Sleep(5 * 1000); //休息5秒
                Program.threads["MAIN_YT_RUN_2_HOUR"] = new Thread(() => run_2hour());
                Program.threads["MAIN_YT_RUN_2_HOUR"].Start();
            }
        }
        public void updateM3U8(bool onlyRunEmpty)
        {
            //如果 onlyRunEmpty 是 true 表示要針對 VALID='1' 而且 M3U8 是空的處理



            //每二小時更新所有 youtube 的 m3u8
            //每分鐘更新youtube下載路徑 gridview
            string SQL = @"
                    SELECT 
                        [id],
                        [youtube_url],
                        CONVERT(varchar(256), [last_m3u8_datetime], 120) AS [last_m3u8_datetime]
                    FROM
                        [site]
                    WHERE
                        1=1
                        AND [del]='0'                      
            ";
            if (onlyRunEmpty)
            {
                SQL += @" AND ISNULL([m3u8_url],'') = '' ";
            }
            SQL += @"
                    ORDER BY 
                        [id] ASC
                ";

            DataTable ra = Program.my.selectSQL_SAFE(SQL);
            Program.log("updateM3U8: (onlyRunEmpty)" + "( " + onlyRunEmpty.ToString() + " ) " + Program.my.json_format_utf8(Program.my.json_encode(ra)));
            string Y_DL_EXE = Program.PWD + "\\binary\\youtube-dl.exe";
            string op = Program.GLOBAL_tmpPath;
            if (!Program.my.is_dir(op))
            {
                Program.my.mkdir(op);
            }
            for (int i = 0, max_i = ra.Rows.Count; i < max_i; i++)
            {
                string id = ra.Rows[i]["id"].ToString();
                string YT_PATH = ra.Rows[i]["youtube_url"].ToString().Trim();
                YT_PATH = YT_PATH.Replace("\"", ""); // prevent command injection

                //check json_ files 
                Program.log("op: " + op);
                var fp = Program.my.glob(op, "json_*.txt");
                if (fp.Count() != 0)
                {
                    if (Convert.ToInt64(Program.my.time()) - Convert.ToInt64(Program.my.strtotime(ra.Rows[i]["last_m3u8_datetime"].ToString())) < 3600)
                    {
                        continue;
                    }
                }

                string opfile = op + "\\json_" + Program.my.time() + ".txt";
                string CMD = "\"" + Y_DL_EXE + "\" -s -j \"" + YT_PATH + "\" > \"" + opfile + "\" && exit";
                Program.log("Use youtube-dl.exe download ... : " + CMD);
                Program.logError("執行 youtube-dl 下載：site_id : " + id + "\r\n" + CMD);
                Program.my.system_background(CMD, 0);
                string json_str = "";

                if (!Program.my.is_file(opfile))
                {

                    //發生問題，沒有 m3u8
                    Program.logError("M3U8 沒有 m3u8：site_id：" + id);
                    var pa = new Dictionary<string, string>();
                    pa["m3u8_url"] = "";
                    var mpa = new Dictionary<string, string>();
                    mpa["id"] = id;
                    Program.my.updateSQL_SAFE("site", pa, "[id]=@id", mpa);
                    continue;
                }
                json_str = Program.my.b2s(Program.my.file_get_contents(opfile));
                try
                {
                    //remove tmp file
                    Program.my.unlink(opfile);
                }
                catch //(Exception ex)
                {
                    //Program.logError("無法刪除 opfile... : " + opfile + "\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                }
                //MessageBox.Show(json_str);
                var formats_list = Program.my.youtubeJsonParser(json_str);
                if (formats_list == null)
                {
                    //m3u8 解析不到
                    Program.logError("M3U8 無法解晰：site_id：" + id);
                    var pa = new Dictionary<string, string>();
                    pa["m3u8_url"] = "";
                    var mpa = new Dictionary<string, string>();
                    mpa["id"] = id;
                    Program.my.updateSQL_SAFE("site", pa, "[id]=@id", mpa);
                    continue;
                }

                string downloadQuality = Program.my.getSystemKey("downloadQuality");
                downloadQuality = downloadQuality.Trim().Replace(";", ",");
                var m = Program.my.explode(",", downloadQuality);

                List<string> allowID = new List<string>();
                //allowID.Add("95"); //95 - 1280x720 (HLS)
                //allowID.Add("94"); //94 - 854x480 (HLS)
                //allowID.Add("93"); //93 - 640x360 (HLS)
                //allowID.Add("92"); //92 - 426x240 (HLS)
                //allowID.Add("91"); //91 - 256x144 (HLS)
                for (int _mi = 0, _maxmi = m.Count(); _mi < _maxmi; _mi++)
                {
                    allowID.Add(m[_mi]);
                }

                string format_id = "";
                foreach (var k in allowID)
                {
                    if (Program.my.in_array(k, formats_list.Keys.ToArray()))
                    {
                        format_id = k;
                        break;
                    }
                }
                if (format_id == "")
                {
                    continue; //找不到可以用的格式
                              //MessageBox.Show(format_id);
                              //MessageBox.Show(formats_list[format_id]);
                              //把新的 m3u8 路徑 寫回 DB
                }
                else
                {
                    var pa = new Dictionary<string, string>();
                    pa["m3u8_url"] = formats_list[format_id];
                    pa["last_m3u8_datetime"] = Program.my.date("Y-m-d H:i:s");

                    var mpa = new Dictionary<string, string>();
                    mpa["id"] = id;
                    Program.my.updateSQL_SAFE("site", pa, "[id]=@id", mpa);
                }
            }
        }
        public void run_30second()
        {
            //每30秒跑一次下載 M3u8 裡的 ts 檔

            try
            {

                if (isNeedStop) //是否要離開 thread
                {
                    return;
                }
                if (last30Second == "")
                {
                    //第一次都跑
                    last30Second = Program.my.date("s");
                }
                else
                {
                    if ((Program.my.date("s") != "00" && Program.my.date("s") != "30") || last30Second == Program.my.date("s")) //最後執行的「秒」與目前時間的「秒」如果是一樣，就再休息1秒，1秒後再跑一次
                    {
                        try
                        {
                            Thread.Sleep(500);
                        }
                        catch
                        {
                        }
                        Program.threads["MAIN_YT_RUN_30_SECOND"] = new Thread(() => run_30second());
                        Program.threads["MAIN_YT_RUN_30_SECOND"].Start();
                        return;
                    }
                    else
                    {
                        last30Second = Program.my.date("s");
                    }
                }
                //開始了
                //每30秒跑下載 M3u8 、解析，並抓裡面的 ts 檔
                //直接跑 M3U8_MEMORY 即可
                //迴圈各自跑各自的thread下載
                foreach (string site_id in M3U8_MEMORY.Keys.ToArray())
                {
                    new Thread(() =>
                        download_ts(site_id, M3U8_MEMORY[site_id])
                    ).Start();
                }
                //寫一個時間文字檔案到 mp4Path
                try
                {
                    Program.my.file_put_contents(Program.GLOBAL_mp4Path + "\\system_time.txt", Program.my.date("Y-m-d H:i:s"));
                }
                catch (Exception ex)
                {
                    Program.logError("無法寫出 system_time.txt\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                }
                Thread.Sleep(500); //休息500ms
                Program.threads["MAIN_YT_RUN_30_SECOND"] = new Thread(() => run_30second());
                Program.threads["MAIN_YT_RUN_30_SECOND"].Start();
                return;
            }
            catch (Exception ex)
            {
                Program.logError("Error...:" + ex.Message + "\r\n" + ex.StackTrace);
                //GC.Collect(); //回收 ram                
                Thread.Sleep(500); //休息1秒
                Program.threads["MAIN_YT_RUN_30_SECOND"] = new Thread(() => run_30second());
                Program.threads["MAIN_YT_RUN_30_SECOND"].Start();
            }
        }

        private void download_ts(string site_id, string m3u8_url)
        {

            //解析 M3U8 內容
            /*
             出格式
#EXTM3U
#EXT-X-VERSION:3
#EXT-X-TARGETDURATION:5
#EXT-X-MEDIA-SEQUENCE:4886891
#EXT-X-DISCONTINUITY-SEQUENCE:219417
#EXT-X-PROGRAM-DATE-TIME:2021-01-06T07:37:39.467+00:00
#EXTINF:5.0,
https://r2---sn-ipoxu-umb6.googlevideo.com/videoplayback/id/tWdI0YfY93Y.1/itag/95/source/yt_live_broadcast/expire/1609939060/ei/FGT1X7qPA7uss8IPgauLgA4/ip/211.20.175.252/requiressl/yes/ratebypass/yes/live/1/sgoap/gir%3Dyes%3Bitag%3D140/sgovp/gir%3Dyes%3Bitag%3D136/hls_chunk_host/r2---sn-ipoxu-umb6.googlevideo.com/playlist_duration/30/manifest_duration/30/vprv/1/playlist_type/DVR/initcwndbps/5450/mh/GI/mm/44/mn/sn-ipoxu-umb6/ms/lva/mv/m/mvi/2/pl/24/keepalive/yes/beids/9466588/mt/1609917155/sparams/expire,ei,ip,id,itag,source,requiressl,ratebypass,live,sgoap,sgovp,playlist_duration,manifest_duration,vprv,playlist_type/sig/AOq0QJ8wRgIhALkfwKjgWCXSXlAWlVAKN8nyLIGlEzW_WTzEDfhpsXZ-AiEAiYwxxEWDzniXW9BS-0Tl8v-UGA04r8S6YBlQCBNMHEo%3D/lsparams/hls_chunk_host,initcwndbps,mh,mm,mn,ms,mv,mvi,pl/lsig/AG3C_xAwRAIgOnyJ8sB5MnepvzFKh3h5h_PR5TQxe0e14gVTkP3nTeACIBDXmtNN7Hm97ZWLF0sMVdH_Zo9wnR-q4MtXnxVE9mU5/playlist/index.m3u8/sq/4886891/goap/clen%3D81126%3Blmt%3D1609659626117291/govp/clen%3D222556%3Blmt%3D1609659626117288/dur/5.000/file/seg.ts
#EXTINF:5.0,
https://r2---sn-ipoxu-umb6.googlevideo.com/videoplayback/id/tWdI0YfY93Y.1/itag/95/source/yt_live_broadcast/expire/1609939060/ei/FGT1X7qPA7uss8IPgauLgA4/ip/211.20.175.252/requiressl/yes/ratebypass/yes/live/1/sgoap/gir%3Dyes%3Bitag%3D140/sgovp/gir%3Dyes%3Bitag%3D136/hls_chunk_host/r2---sn-ipoxu-umb6.googlevideo.com/playlist_duration/30/manifest_duration/30/vprv/1/playlist_type/DVR/initcwndbps/5450/mh/GI/mm/44/mn/sn-ipoxu-umb6/ms/lva/mv/m/mvi/2/pl/24/keepalive/yes/beids/9466588/mt/1609917155/sparams/expire,ei,ip,id,itag,source,requiressl,ratebypass,live,sgoap,sgovp,playlist_duration,manifest_duration,vprv,playlist_type/sig/AOq0QJ8wRgIhALkfwKjgWCXSXlAWlVAKN8nyLIGlEzW_WTzEDfhpsXZ-AiEAiYwxxEWDzniXW9BS-0Tl8v-UGA04r8S6YBlQCBNMHEo%3D/lsparams/hls_chunk_host,initcwndbps,mh,mm,mn,ms,mv,mvi,pl/lsig/AG3C_xAwRAIgOnyJ8sB5MnepvzFKh3h5h_PR5TQxe0e14gVTkP3nTeACIBDXmtNN7Hm97ZWLF0sMVdH_Zo9wnR-q4MtXnxVE9mU5/playlist/index.m3u8/sq/4886892/goap/clen%3D81126%3Blmt%3D1609659626117301/govp/clen%3D153810%3Blmt%3D1609659626117298/dur/5.000/file/seg.ts
#EXTINF:5.0,
https://r2---sn-ipoxu-umb6.googlevideo.com/videoplayback/id/tWdI0YfY93Y.1/itag/95/source/yt_live_broadcast/expire/1609939060/ei/FGT1X7qPA7uss8IPgauLgA4/ip/211.20.175.252/requiressl/yes/ratebypass/yes/live/1/sgoap/gir%3Dyes%3Bitag%3D140/sgovp/gir%3Dyes%3Bitag%3D136/hls_chunk_host/r2---sn-ipoxu-umb6.googlevideo.com/playlist_duration/30/manifest_duration/30/vprv/1/playlist_type/DVR/initcwndbps/5450/mh/GI/mm/44/mn/sn-ipoxu-umb6/ms/lva/mv/m/mvi/2/pl/24/keepalive/yes/beids/9466588/mt/1609917155/sparams/expire,ei,ip,id,itag,source,requiressl,ratebypass,live,sgoap,sgovp,playlist_duration,manifest_duration,vprv,playlist_type/sig/AOq0QJ8wRgIhALkfwKjgWCXSXlAWlVAKN8nyLIGlEzW_WTzEDfhpsXZ-AiEAiYwxxEWDzniXW9BS-0Tl8v-UGA04r8S6YBlQCBNMHEo%3D/lsparams/hls_chunk_host,initcwndbps,mh,mm,mn,ms,mv,mvi,pl/lsig/AG3C_xAwRAIgOnyJ8sB5MnepvzFKh3h5h_PR5TQxe0e14gVTkP3nTeACIBDXmtNN7Hm97ZWLF0sMVdH_Zo9wnR-q4MtXnxVE9mU5/playlist/index.m3u8/sq/4886893/goap/clen%3D81465%3Blmt%3D1609659626117311/govp/clen%3D158286%3Blmt%3D1609659626117308/dur/5.000/file/seg.ts
#EXTINF:5.0,
https://r2---sn-ipoxu-umb6.googlevideo.com/videoplayback/id/tWdI0YfY93Y.1/itag/95/source/yt_live_broadcast/expire/1609939060/ei/FGT1X7qPA7uss8IPgauLgA4/ip/211.20.175.252/requiressl/yes/ratebypass/yes/live/1/sgoap/gir%3Dyes%3Bitag%3D140/sgovp/gir%3Dyes%3Bitag%3D136/hls_chunk_host/r2---sn-ipoxu-umb6.googlevideo.com/playlist_duration/30/manifest_duration/30/vprv/1/playlist_type/DVR/initcwndbps/5450/mh/GI/mm/44/mn/sn-ipoxu-umb6/ms/lva/mv/m/mvi/2/pl/24/keepalive/yes/beids/9466588/mt/1609917155/sparams/expire,ei,ip,id,itag,source,requiressl,ratebypass,live,sgoap,sgovp,playlist_duration,manifest_duration,vprv,playlist_type/sig/AOq0QJ8wRgIhALkfwKjgWCXSXlAWlVAKN8nyLIGlEzW_WTzEDfhpsXZ-AiEAiYwxxEWDzniXW9BS-0Tl8v-UGA04r8S6YBlQCBNMHEo%3D/lsparams/hls_chunk_host,initcwndbps,mh,mm,mn,ms,mv,mvi,pl/lsig/AG3C_xAwRAIgOnyJ8sB5MnepvzFKh3h5h_PR5TQxe0e14gVTkP3nTeACIBDXmtNN7Hm97ZWLF0sMVdH_Zo9wnR-q4MtXnxVE9mU5/playlist/index.m3u8/sq/4886894/goap/clen%3D81126%3Blmt%3D1609659626117321/govp/clen%3D192092%3Blmt%3D1609659626117318/dur/5.000/file/seg.ts
#EXTINF:5.0,
https://r2---sn-ipoxu-umb6.googlevideo.com/videoplayback/id/tWdI0YfY93Y.1/itag/95/source/yt_live_broadcast/expire/1609939060/ei/FGT1X7qPA7uss8IPgauLgA4/ip/211.20.175.252/requiressl/yes/ratebypass/yes/live/1/sgoap/gir%3Dyes%3Bitag%3D140/sgovp/gir%3Dyes%3Bitag%3D136/hls_chunk_host/r2---sn-ipoxu-umb6.googlevideo.com/playlist_duration/30/manifest_duration/30/vprv/1/playlist_type/DVR/initcwndbps/5450/mh/GI/mm/44/mn/sn-ipoxu-umb6/ms/lva/mv/m/mvi/2/pl/24/keepalive/yes/beids/9466588/mt/1609917155/sparams/expire,ei,ip,id,itag,source,requiressl,ratebypass,live,sgoap,sgovp,playlist_duration,manifest_duration,vprv,playlist_type/sig/AOq0QJ8wRgIhALkfwKjgWCXSXlAWlVAKN8nyLIGlEzW_WTzEDfhpsXZ-AiEAiYwxxEWDzniXW9BS-0Tl8v-UGA04r8S6YBlQCBNMHEo%3D/lsparams/hls_chunk_host,initcwndbps,mh,mm,mn,ms,mv,mvi,pl/lsig/AG3C_xAwRAIgOnyJ8sB5MnepvzFKh3h5h_PR5TQxe0e14gVTkP3nTeACIBDXmtNN7Hm97ZWLF0sMVdH_Zo9wnR-q4MtXnxVE9mU5/playlist/index.m3u8/sq/4886895/goap/clen%3D81127%3Blmt%3D1609659626117331/govp/clen%3D187266%3Blmt%3D1609659626117328/dur/5.000/file/seg.ts
#EXTINF:5.0,
https://r2---sn-ipoxu-umb6.googlevideo.com/videoplayback/id/tWdI0YfY93Y.1/itag/95/source/yt_live_broadcast/expire/1609939060/ei/FGT1X7qPA7uss8IPgauLgA4/ip/211.20.175.252/requiressl/yes/ratebypass/yes/live/1/sgoap/gir%3Dyes%3Bitag%3D140/sgovp/gir%3Dyes%3Bitag%3D136/hls_chunk_host/r2---sn-ipoxu-umb6.googlevideo.com/playlist_duration/30/manifest_duration/30/vprv/1/playlist_type/DVR/initcwndbps/5450/mh/GI/mm/44/mn/sn-ipoxu-umb6/ms/lva/mv/m/mvi/2/pl/24/keepalive/yes/beids/9466588/mt/1609917155/sparams/expire,ei,ip,id,itag,source,requiressl,ratebypass,live,sgoap,sgovp,playlist_duration,manifest_duration,vprv,playlist_type/sig/AOq0QJ8wRgIhALkfwKjgWCXSXlAWlVAKN8nyLIGlEzW_WTzEDfhpsXZ-AiEAiYwxxEWDzniXW9BS-0Tl8v-UGA04r8S6YBlQCBNMHEo%3D/lsparams/hls_chunk_host,initcwndbps,mh,mm,mn,ms,mv,mvi,pl/lsig/AG3C_xAwRAIgOnyJ8sB5MnepvzFKh3h5h_PR5TQxe0e14gVTkP3nTeACIBDXmtNN7Hm97ZWLF0sMVdH_Zo9wnR-q4MtXnxVE9mU5/playlist/index.m3u8/sq/4886896/goap/clen%3D81464%3Blmt%3D1609659626117341/govp/clen%3D219428%3Blmt%3D1609659626117338/dur/5.000/file/seg.ts


            List<ConcurrentDictionary<string, string>> o = [
                {
                    "START_DT": 2021-01-06T07:37:39.467+00:00 + 8小時，轉 timestamp
                    "TS_PATH": "",
                    "TS_MD5" : "",
                    "DURATION"
                }
            ]
            */
            string data = "";
            List<ConcurrentDictionary<string, string>> m3 = null;

            try
            {
                data = Program.my.b2s(Program.my.file_get_contents(m3u8_url));
                m3 = Program.my.m3u8Parser(data);
            }
            catch (Exception ex)
            {
                Program.logError("無法下載與解析 m3u8 : " + site_id + "\r\n" + m3u8_url + "\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                m3 = null;
            }
            //MessageBox.Show(Program.my.json_encode(m3));
            if (m3 == null)
            {
                //有錯
                var pa = new Dictionary<string, string>();
                pa["P_VALID"] = "0";
                var mpa = new Dictionary<string, string>();
                mpa["id"] = site_id;
                Program.my.updateSQL_SAFE("site", pa, "[id]=@id", mpa);
                return;
            }
            //循續下載 ts 檔，並寫入 YOUTUBE_RECORD_TS_ITEMS

            bool check = true;
            for (int i = 0, max_i = m3.Count(); i < max_i; i++)
            {
                try
                {
                    //建立目錄
                    string Y = Program.my.date("Y", Program.my.strtotime(m3[i]["DATE"]));
                    string md = Program.my.date("md", Program.my.strtotime(m3[i]["DATE"]));
                    string OUTPUT_PATH = Program.GLOBAL_mp4Path + "\\" + site_id + "\\" + Y + "\\" + md;
                    string OUTPUT_FILENAME = m3[i]["TS_MD5"] + ".ts";//md5 + .ts
                    //string OUTPUT_MP4_FILENAME = m3[i]["TS_MD5"] + ".mp4";//md5 + .mp4
                    string DOWNLOAD_PATH = m3[i]["TS_PATH"]; //要下載的來源
                    string DURATION = m3[i]["DURATION"];
                    string START_DT = m3[i]["START_DT"];
                    string DATE = Program.my.date("Y-m-d", Program.my.strtotime(m3[i]["DATE"]));
                    string op = OUTPUT_PATH + "\\" + OUTPUT_FILENAME;
                    //string op_mp4 = OUTPUT_PATH + "\\" + OUTPUT_MP4_FILENAME;

                    if (!Program.my.is_dir(OUTPUT_PATH))
                    {
                        Program.my.mkdir(OUTPUT_PATH);
                    }
                    if (Program.my.is_file(op))
                    {
                        continue;
                    }
                    //不存在才要下載
                    //string CMD = Program.PWD + "\\binary\\wget.exe \"" + DOWNLOAD_PATH + "\" --timeout=20 -t 2 -c -O \"" + op + "\" && exit";
                    //抓吧
                    //Program.log(CMD);
                    //Program.my.system_background(CMD, 0);
                    //下載 ts
                    byte[] tsFile_ByteArray = Program.my.file_get_contents(DOWNLOAD_PATH);
                    Program.my.file_put_contents(op, tsFile_ByteArray);
                    Array.Clear(tsFile_ByteArray, 0, tsFile_ByteArray.Length);
                    tsFile_ByteArray = null;

                    //////////////////////////////////////////////////////////////////////////////////////// 轉 wav
                    string FFMPEG_BIN = Program.PWD + "\\binary\\ffmpeg.exe";
                    string FFPROBE_BIN = Program.PWD + "\\binary\\ffprobe.exe";
                    if (Program.my.is_file(op))
                    {
                        string WORKING_PATH = Program.GLOBAL_mp4Path + "\\" + site_id + "\\" + Y + "\\" + md;
                        //tsFiles[i] = WORKING_PATH + "\\" + tsFiles[i]; //full path
                        string t = Program.my.time();
                        //string OUTPUT_MP4_FILENAME = t + ".mp4"; //timestamp 輸出的 mp4 檔名
                        string OUTPUT_TS_FILENAME = t + ".ts";
                        string OUTPUT_WAV = t + ".wav"; //timestamp 輸出的 wav 檔名                        
                        string OUTPUT_DURATION_TXT = t + ".txt"; //只紀錄 DURATION                                        //rename ts
                        Program.my.rename(op, WORKING_PATH + "\\" + OUTPUT_TS_FILENAME);
                        //-vcodec copy 
                        //-vf scale=-2:480
                        //string CMD = "cd /d \"" + WORKING_PATH + "\" && \"" + FFMPEG_BIN + "\" -y -i \"" + tsFiles[i] + "\" -vcodec copy \"" + OUTPUT_MP4_FILENAME + "\" && exit";
                        string CMD = "cd /d \"" + WORKING_PATH + "\" && \"" + FFMPEG_BIN + "\" -y -i \"" + OUTPUT_TS_FILENAME + "\" -ac 1 -ar 8000 \"" + OUTPUT_WAV + "\" && exit";
                        //Program.log(CMD);
                        Program.my.system_background(CMD, 0);

                        //在此語音轉文字
                        string txt = Program.my.wavToText(WORKING_PATH + "\\" + OUTPUT_WAV);
                        Program.log("\r\n文字：" + txt);

                        //如果有產出 OUTPUT_MP4_FILENAME，就刪除用掉的 tsFiles，並寫入 DB
                        //連 ts 的 txt 也刪
                        if (Program.my.is_file(WORKING_PATH + "\\" + OUTPUT_WAV))
                        {

                            //string mn = Program.my.mainname(OUTPUT_WAV);
                            //string d_txt_file = WORKING_PATH + "\\" + mn + ".txt";
                            //string d_ts_file = WORKING_PATH + "\\" + mn + ".ts";


                            if (Program.my.is_file(WORKING_PATH + "\\" + OUTPUT_DURATION_TXT)) //刪 durion .txt 
                            {
                                Program.my.unlink(WORKING_PATH + "\\" + OUTPUT_DURATION_TXT);

                            }
                            //這時不能刪 ts 檔
                            //if (Program.my.is_file(d_ts_file)) //刪 ts 檔 .ts
                            //{
                            //Program.my.unlink(d_ts_file);
                            //}


                            //取得 ts 的影片during播放時間，然後寫入 DB
                            // $CMD = "/usr/bin/ffprobe -v quiet -show_streams -select_streams v:0 -of json {$OUTPUT_FILENAME_MP4} > 倒出 txt 檔"; 
                            CMD = "cd /d \"" + WORKING_PATH + "\" && \"" + FFPROBE_BIN + "\" -v quiet -show_streams -select_streams v:0 -of json \"" + OUTPUT_TS_FILENAME + "\" > \"" + OUTPUT_DURATION_TXT + "\" && exit";
                            Program.my.system_background(CMD, 0);


                            //針對第一個影格作縮圖 (先不要)                
                            //CMD = "cd /d \"" + WORKING_PATH + "\" && \"" + FFMPEG_BIN + "\" -y -i \"" + OUTPUT_MP4_FILENAME + "\"  -f image2 -ss 1 -vframes 1 -s 853x480 -an \"" + OUTPUT_PNG_FILENAME + "\" && exit";
                            //Program.my.system_background(CMD, 0);
                            /*
                             * {
                "streams": [
                    {
                        "index": 0,
                        "codec_name": "h264",
                        "codec_long_name": "H.264 / AVC / MPEG-4 AVC / MPEG-4 part 10",
                        "profile": "High",
                        "codec_type": "video",
                        "codec_time_base": "1/36",
                        "codec_tag_string": "avc1",
                        "codec_tag": "0x31637661",
                        "width": 640,
                        "height": 480,
                        "coded_width": 640,
                        "coded_height": 480,
                        "has_b_frames": 2,
                        "sample_aspect_ratio": "1:1",
                        "display_aspect_ratio": "4:3",
                        "pix_fmt": "yuvj420p",
                        "level": 30,
                        "color_range": "pc",
                        "chroma_location": "left",
                        "refs": 1,
                        "is_avc": "true",
                        "nal_length_size": "4",
                        "r_frame_rate": "18/1",
                        "avg_frame_rate": "18/1",
                        "time_base": "1/18432",
                        "start_pts": 0,
                        "start_time": "0.000000",
                        "duration_ts": 9264144,
                        "duration": "502.611979",
                        "bit_rate": "70218",
                        "bits_per_raw_sample": "8",
                        "nb_frames": "9047",
                        "disposition": {
                            "default": 1,
                            "dub": 0,
                            "original": 0,
                            "comment": 0,
                            "lyrics": 0,
                            "karaoke": 0,
                            "forced": 0,
                            "hearing_impaired": 0,
                            "visual_impaired": 0,
                            "clean_effects": 0,
                            "attached_pic": 0,
                            "timed_thumbnails": 0
                        },
                        "tags": {
                            "language": "und",
                            "handler_name": "VideoHandler"
                        }
                    }
                ]
            }

                            */
                            //如果有 txt 檔，格式長這樣，要抓 duration，然後寫回「YOUTUBE_RECORD_MP4」

                            if (Program.my.is_file(WORKING_PATH + "\\" + OUTPUT_DURATION_TXT) &&
                                Program.my.is_file(WORKING_PATH + "\\" + OUTPUT_WAV) &&
                                Program.my.is_file(WORKING_PATH + "\\" + OUTPUT_TS_FILENAME)
                                )
                            {
                                string Ymd = Program.my.date("Y-m-d", Program.my.strtotime(Y + "-" + md.Substring(0, 2) + "-" + md.Substring(2, 2)));
                                long TS_FILESIZE = Program.my.filesize(WORKING_PATH + "\\" + OUTPUT_TS_FILENAME);


                                var pa = new Dictionary<string, string>();
                                pa["site_id"] = site_id;
                                pa["TS_DOWNLOAD_URL"] = DOWNLOAD_PATH;
                                pa["DATE"] = Ymd;
                                pa["START_DT"] = Program.my.date("Y-m-d H:i:s.fff", START_DT);
                                pa["DURATION"] = DURATION;
                                pa["TS_FILENAME"] = OUTPUT_TS_FILENAME;
                                pa["TS_FILESIZE"] = TS_FILESIZE.ToString();
                                pa["CREATE_DATETIME"] = Program.my.date("Y-m-d H:i:s");
                                pa["contents"] = txt;
                                Program.log(Program.my.json_format_utf8(Program.my.json_encode(pa)));
                                Program.my.insertSQL("site_item", pa);
                            }
                        }

                        //然後把 DURATION 也寫出
                        Program.my.file_put_contents(OUTPUT_DURATION_TXT, DURATION);
                    }


                }
                catch
                {
                    check = false;
                }
            }

            var _pa = new Dictionary<string, string>();
            _pa["P_VALID"] = (check) ? "1" : "0";
            var _mpa = new Dictionary<string, string>();
            _mpa["id"] = site_id;
            Program.my.updateSQL_SAFE("site", _pa, "[id]=@id", _mpa);
        }
        public void run_1min()
        {
            //每分鐘更新youtube下載路徑 gridview
            try
            {

                if (isNeedStop) //是否要離開 thread
                {
                    return;
                }

                Program.killMoreThan120minProcess();


                if (lastMinute == "")
                {
                    lastMinute = Program.my.date("i");
                }
                else
                {
                    if (lastMinute == Program.my.date("i")) //最後執行的「分鐘」與目前時間的「分鐘」如果是一樣，就再休息5秒，5秒後再跑一次
                    {
                        try
                        {
                            Thread.Sleep(5000);
                        }
                        catch //(Exception ex)
                        {

                        }
                        Program.threads["MAIN_YT_RUN_1_MIN"] = new Thread(() => run_1min());
                        Program.threads["MAIN_YT_RUN_1_MIN"].Start();
                        return;
                    }
                    else
                    {
                        lastMinute = Program.my.date("i");
                    }
                }
                //開始了                
                //強制更新 空的 M3U8 
                updateM3U8(true);
                //不該刪全部的，要啟動前才刪就好
                //killAllThreads();
                var dt = loadYRToGrid(); //載入 YOUTUBE_RECORD 資料列表
                if (dt != null)
                {
                    List<string> M3U8_MEMORY_SNS = new List<string>();
                    List<string> M3U8_DT_SNS = new List<string>();
                    //先移掉不該存在的
                    foreach (string k in M3U8_MEMORY.Keys.ToArray())
                    {
                        M3U8_MEMORY_SNS.Add(k);
                    }
                    for (int i = 0, max_i = dt.Rows.Count; i < max_i; i++)
                    {
                        string id = dt.Rows[i]["id"].ToString();
                        M3U8_DT_SNS.Add(id);
                        //string M3U8_PATH = dt.Rows[i]["M3U8_PATH"].ToString();                        
                    }
                    //如果現在記憶體空間有的 id 不在 DT 裡，代表已下架，不用再抓 ts 了
                    foreach (string k in M3U8_MEMORY.Keys.ToArray())
                    {
                        if (!Program.my.in_array(k, M3U8_DT_SNS))
                        {
                            //移除 現在記憶體的
                            string a;
                            M3U8_MEMORY.TryRemove(k, out a);
                        }
                    }
                    //如果 DT 裡的 id 不在 現在記憶體中，就要新增
                    for (int i = 0, max_i = dt.Rows.Count; i < max_i; i++)
                    {
                        string id = dt.Rows[i]["id"].ToString();
                        string m3u8_url = dt.Rows[i]["m3u8_url"].ToString().Trim();
                        //if (!Program.my.in_array(id, M3U8_MEMORY.Keys.ToArray()))
                        //{
                        //不管怎樣都把新查到的寫入
                        M3U8_MEMORY[id] = m3u8_url;
                        //}
                    }
                }
                Thread.Sleep(3 * 1000);
                Program.threads["MAIN_YT_RUN_1_MIN"] = new Thread(() => run_1min());
                Program.threads["MAIN_YT_RUN_1_MIN"].Start();
                return;
            }
            catch (Exception ex)
            {
                Program.logError("Error...:" + ex.Message + "\r\n" + ex.StackTrace);
                //GC.Collect(); //回收 ram                
                Thread.Sleep(5 * 1000); //休息5秒
                Program.threads["MAIN_YT_RUN_1_MIN"] = new Thread(() => run_1min());
                Program.threads["MAIN_YT_RUN_1_MIN"].Start();
            }
        }

        public void run_40sec()
        {
            //每10分鐘合併 ts -> mp4
            try
            {
                //每10分鐘，移除超時120分鐘的工作
                try
                {
                    Program.killMoreThan120minProcess();
                }
                catch // (Exception ex)
                {
                    //Program.log("移除超時 40 秒的工作 執行失敗...:\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                }

                if (isNeedStop) //是否要離開 thread
                {
                    return;
                }
                if (last40Second == "")
                {
                    last40Second = Program.my.date("s");
                }
                else
                {
                    if (
                         Convert.ToInt32(Program.my.date("s")) % 40 == 0 ||
                         last40Second == Program.my.date("s") //最後執行的「40秒」與目前時間的「40秒」如果是一樣，就再休息1秒，1秒後再跑一次
                         )
                    {
                        try
                        {
                            Thread.Sleep(1 * 1000);
                        }
                        catch
                        {
                        }
                        Program.threads["MAIN_YT_RUN_40_SEC"] = new Thread(() => run_40sec());
                        Program.threads["MAIN_YT_RUN_40_SEC"].Start();
                        return;
                    }
                    else
                    {
                        last40Second = Program.my.date("i");
                    }
                }
                //開始了
                //合併的方式參考：https://superuser.com/questions/692990/use-ffmpeg-copy-codec-to-combine-ts-files-into-a-single-mp4
                //至少合併二日的資料，昨日與今日，如果昨日還有 ts 就直接全部合併成一個 mp4
                //今日有 ts ，就依 howManyTsMerge 來決定多少個合併成一個mp4，mp4的檔名就取timestamp.mp4

                //都作完了，等下一次工作
                try
                {
                    //Thread.Sleep(60 * 1000);
                    //迴圈跑所有的 M3U8_MEMORY
                    foreach (string site_id in M3U8_MEMORY.Keys.ToArray())
                    {
                        try
                        {
                            //檢查的目錄為「GLOBAL_mp4Path\\site_id\\YEAR\\md\\*.ts
                            List<string> check_dates = new List<string>();
                            for (long i = Convert.ToInt64(Program.my.strtotime(Program.my.date("Y-m-d"))) - 24 * 60 * 60; i <= Convert.ToInt64(Program.my.strtotime(Program.my.date("Y-m-d"))); i += 24 * 60 * 60)
                            {
                                //要檢查的 site_id
                                string Y = Program.my.date("Y", i.ToString()); //要檢查的年
                                string md = Program.my.date("md", i.ToString()); //要檢查的月日
                                //如果目錄不存在，就跳過吧
                                string CHECK_PATH = Program.GLOBAL_mp4Path + "\\" + site_id + "\\" + Y + "\\" + md;
                                if (!Program.my.is_dir(CHECK_PATH))
                                {
                                    continue; //跳過
                                }
                                //如果目錄在，就跑 ts 檔
                                //排序建檔順序範例：https://stackoverflow.com/questions/4765789/getting-files-by-creation-date-in-net
                                DirectoryInfo info = new DirectoryInfo(CHECK_PATH);
                                FileInfo[] files = info.GetFiles("*.ts");
                                // Sort by creation-time ascending 
                                Array.Sort(files, delegate (FileInfo f1, FileInfo f2)
                                {
                                    return f1.CreationTime.CompareTo(f2.CreationTime);
                                });
                                //如果沒有 ts 檔，就跳下一筆
                                if (files.Count() == 0)
                                {
                                    continue;
                                }
                                //如果 ts 檔的數量大於 howManyTsMerge，就跑迴圈合併，一次最多合併 howManyTsMerge 這個數量
                                //如果日期不是今天，就全組一組吧
                                List<string> f = new List<string>();
                                if (Y + md != Program.my.date("Ymd"))
                                {
                                    //不是今天
                                    //把所有的 ts 檔組合起來
                                    for (int _i = 0; _i < files.Count(); _i++)
                                    {
                                        string bn = Program.my.basename(files[_i].FullName);
                                        f.Add(bn); //這裡取到的是完整路徑，所以要轉 basename
                                    }
                                }

                                //開始轉檔
                                //MessageBox.Show(Program.my.json_encode(f));
                                //丟thread轉好了
                                new Thread(() => ts_to_wav(site_id, Y, md, new List<string>(f))).Start();
                            }
                        }
                        catch (Exception ex)
                        {
                            Program.logError("無法轉MP4...: " + site_id + "\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Program.logError("無法轉MP4...OUTTER : " + "\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                }
                Program.threads["MAIN_YT_RUN_40_SEC"] = new Thread(() => run_40sec());
                Program.threads["MAIN_YT_RUN_40_SEC"].Start();
                return;
            }
            catch (Exception ex)
            {
                Program.logError("Error...:" + ex.Message + "\r\n" + ex.StackTrace);
                //GC.Collect(); //回收 ram                
                Thread.Sleep(5 * 1000); //休息5秒
                Program.threads["MAIN_YT_RUN_40_SEC"] = new Thread(() => run_40sec());
                Program.threads["MAIN_YT_RUN_40_SEC"].Start();
            }
        }
        private void ts_to_wav(string site_id, string Y, string md, List<string> tsFiles)
        {
            return;
            //轉檔的語法在這：
            //https://superuser.com/questions/692990/use-ffmpeg-copy-codec-to-combine-ts-files-into-a-single-mp4
            //copy /b segment1_0_av.ts+segment2_0_av.ts+segment3_0_av.ts all.ts
            //ffmpeg -i all.ts -acodec copy -vcodec copy all.mp4
            //工作目錄            
            string WORKING_PATH = Program.GLOBAL_mp4Path + "\\" + site_id + "\\" + Y + "\\" + md;
            //輸出 mp4 的檔名


            string FFMPEG_BIN = Program.PWD + "\\binary\\ffmpeg.exe";
            string FFPROBE_BIN = Program.PWD + "\\binary\\ffprobe.exe";
            for (int i = 0, max_i = tsFiles.Count(); i < max_i; i++)
            {
                //tsFiles[i] = WORKING_PATH + "\\" + tsFiles[i]; //full path
                string t = Program.my.time();
                //string OUTPUT_MP4_FILENAME = t + ".mp4"; //timestamp 輸出的 mp4 檔名
                string OUTPUT_TS_FILENAME = t + ".ts";
                string OUTPUT_WAV = t + ".wav"; //timestamp 輸出的 wav 檔名
                string OUTPUT_JSON_FILENAME = t + ".txt"; //timestamp 輸出 mp4 的 infomation
                //string OUTPUT_PNG_FILENAME = t + ".png"; //timestamp 輸出的 mp4 第一張影格的圖片

                //rename ts
                Program.my.rename(tsFiles[i], WORKING_PATH + "\\" + OUTPUT_TS_FILENAME);
                //-vcodec copy 
                //-vf scale=-2:480
                //string CMD = "cd /d \"" + WORKING_PATH + "\" && \"" + FFMPEG_BIN + "\" -y -i \"" + tsFiles[i] + "\" -vcodec copy \"" + OUTPUT_MP4_FILENAME + "\" && exit";
                string CMD = "cd /d \"" + WORKING_PATH + "\" && \"" + FFMPEG_BIN + "\" -y -i \"" + OUTPUT_TS_FILENAME + "\" -ac 1 -ar 8000 \"" + OUTPUT_WAV + "\" && exit";
                //Program.log(CMD);
                Program.my.system_background(CMD, 0);

                //在此語音轉文字
                string txt = Program.my.wavToText(WORKING_PATH + "\\" + OUTPUT_WAV);
                Program.log("\r\n文字：" + txt);

                //如果有產出 OUTPUT_MP4_FILENAME，就刪除用掉的 tsFiles，並寫入 DB
                //連 ts 的 txt 也刪
                if (Program.my.is_file(WORKING_PATH + "\\" + OUTPUT_WAV))
                {

                    //string mn = Program.my.mainname(OUTPUT_WAV);
                    //string d_txt_file = WORKING_PATH + "\\" + mn + ".txt";
                    //string d_ts_file = WORKING_PATH + "\\" + mn + ".ts";


                    if (Program.my.is_file(WORKING_PATH + "\\" + OUTPUT_JSON_FILENAME)) //刪 durion .txt 
                    {
                        Program.my.unlink(WORKING_PATH + "\\" + OUTPUT_JSON_FILENAME);

                    }
                    //這時不能刪 ts 檔
                    //if (Program.my.is_file(d_ts_file)) //刪 ts 檔 .ts
                    //{
                    //Program.my.unlink(d_ts_file);
                    //}


                    //取得 ts 的影片during播放時間，然後寫入 DB
                    // $CMD = "/usr/bin/ffprobe -v quiet -show_streams -select_streams v:0 -of json {$OUTPUT_FILENAME_MP4} > 倒出 txt 檔"; 
                    CMD = "cd /d \"" + WORKING_PATH + "\" && \"" + FFPROBE_BIN + "\" -v quiet -show_streams -select_streams v:0 -of json \"" + OUTPUT_TS_FILENAME + "\" > \"" + OUTPUT_JSON_FILENAME + "\" && exit";
                    Program.my.system_background(CMD, 0);


                    //針對第一個影格作縮圖 (先不要)                
                    //CMD = "cd /d \"" + WORKING_PATH + "\" && \"" + FFMPEG_BIN + "\" -y -i \"" + OUTPUT_MP4_FILENAME + "\"  -f image2 -ss 1 -vframes 1 -s 853x480 -an \"" + OUTPUT_PNG_FILENAME + "\" && exit";
                    //Program.my.system_background(CMD, 0);
                    /*
                     * {
        "streams": [
            {
                "index": 0,
                "codec_name": "h264",
                "codec_long_name": "H.264 / AVC / MPEG-4 AVC / MPEG-4 part 10",
                "profile": "High",
                "codec_type": "video",
                "codec_time_base": "1/36",
                "codec_tag_string": "avc1",
                "codec_tag": "0x31637661",
                "width": 640,
                "height": 480,
                "coded_width": 640,
                "coded_height": 480,
                "has_b_frames": 2,
                "sample_aspect_ratio": "1:1",
                "display_aspect_ratio": "4:3",
                "pix_fmt": "yuvj420p",
                "level": 30,
                "color_range": "pc",
                "chroma_location": "left",
                "refs": 1,
                "is_avc": "true",
                "nal_length_size": "4",
                "r_frame_rate": "18/1",
                "avg_frame_rate": "18/1",
                "time_base": "1/18432",
                "start_pts": 0,
                "start_time": "0.000000",
                "duration_ts": 9264144,
                "duration": "502.611979",
                "bit_rate": "70218",
                "bits_per_raw_sample": "8",
                "nb_frames": "9047",
                "disposition": {
                    "default": 1,
                    "dub": 0,
                    "original": 0,
                    "comment": 0,
                    "lyrics": 0,
                    "karaoke": 0,
                    "forced": 0,
                    "hearing_impaired": 0,
                    "visual_impaired": 0,
                    "clean_effects": 0,
                    "attached_pic": 0,
                    "timed_thumbnails": 0
                },
                "tags": {
                    "language": "und",
                    "handler_name": "VideoHandler"
                }
            }
        ]
    }

                    */
                    //如果有 txt 檔，格式長這樣，要抓 duration，然後寫回「YOUTUBE_RECORD_MP4」

                    if (Program.my.is_file(WORKING_PATH + "\\" + OUTPUT_JSON_FILENAME) &&
                        Program.my.is_file(WORKING_PATH + "\\" + OUTPUT_WAV) &&
                        Program.my.is_file(WORKING_PATH + "\\" + OUTPUT_TS_FILENAME)
                        )
                    {
                        string Ymd = Program.my.date("Y-m-d", Program.my.strtotime(Y + "-" + md.Substring(0, 2) + "-" + md.Substring(2, 2)));
                        long TS_FILESIZE = Program.my.filesize(WORKING_PATH + "\\" + OUTPUT_TS_FILENAME);
                        string data = Program.my.b2s(Program.my.file_get_contents(WORKING_PATH + "\\" + OUTPUT_JSON_FILENAME));
                        var jd = Program.my.json_decode(data);
                        string DURATION = jd[0]["streams"][0]["duration"].ToString();

                        var pa = new Dictionary<string, string>();
                        pa["site_id"] = site_id;
                        pa["DATE"] = Ymd;
                        pa["DURATION"] = DURATION;
                        pa["TS_FILENAME"] = OUTPUT_TS_FILENAME;
                        pa["TS_FILESIZE"] = TS_FILESIZE.ToString();
                        pa["CREATE_DATETIME"] = Program.my.date("Y-m-d H:i:s");
                        //pa["START_DT"] = start
                        pa["contents"] = txt;
                        Program.my.insertSQL("site_item", pa);

                    }
                    else
                    {
                        Program.logError("無法產出 TS、WAV... : site_id : " + site_id + "\r\n找不到 TS、WAV\r\nTS :" + WORKING_PATH + "\\" + OUTPUT_TS_FILENAME + "\r\nWAV : " + WORKING_PATH + "\\" + OUTPUT_WAV);
                    }
                }
                else
                {
                    Program.logError("無法產出 WAV... : site_id : " + site_id + "\r\nCMD : " + CMD);
                }
            }
        }
        public void run_6hour()
        {
            //每6小時刪除 mp4Path 目錄下 所有站台超過 keepMp4Day 天的資料
            try
            {

                if (isNeedStop) //是否要離開 thread
                {
                    return;
                }
                if (last6Hour == "")
                {
                    last6Hour = Program.my.date("H");
                }
                else
                {
                    if ((
                            Program.my.date("H") != "00" &&
                            Program.my.date("H") != "06" &&
                            Program.my.date("H") != "12" &&
                            Program.my.date("H") != "18"
                        )
                        || last6Hour == Program.my.date("H")) //最後執行的「小時」與目前時間的「小時」如果是一樣，就再休息1分鐘，1 分鐘後再跑一次
                    {
                        try
                        {
                            Thread.Sleep(60 * 1000);
                        }
                        catch
                        {
                        }
                        Program.threads["MAIN_YT_RUN_6_HOUR"] = new Thread(() => run_6hour());
                        Program.threads["MAIN_YT_RUN_6_HOUR"].Start();
                        return;
                    }
                    else
                    {
                        last6Hour = Program.my.date("H");
                    }
                }
                //開始了
                //產生安全列表，如 20210102,20210103,20210104,20210105,20210106,20210107,20210108
                List<string> allowDate = new List<string>();
                for (long dt = Convert.ToInt64(Program.my.strtotime(Program.my.date("Y-m-d"))) - Program.GLOBAL_keepMp4Day * 24 * 60 * 60; dt <= Convert.ToInt64(Program.my.strtotime(Program.my.date("Y-m-d"))); dt += 24 * 60 * 60)
                {
                    allowDate.Add(Program.my.date("Ymd", dt.ToString()));
                }
                //從 M3U8_MEMORY 找要處理哪些 site_id
                foreach (string site_sn in M3U8_MEMORY.Keys.ToArray())
                {
                    //年
                    //List<string> years = new List<string>();
                    string cp = Program.GLOBAL_mp4Path + "\\" + site_sn;
                    if (!Program.my.is_dir(cp))
                    {
                        Program.my.mkdir(cp);
                    }
                    string[] years = Directory.GetDirectories(cp);
                    //取到的是完整路徑，如 E:\YoutubeToText\output_mp4\1\2021
                    //迴圈跑所有的路徑，找所有的日期
                    for (int i = 0; i < years.Count(); i++)
                    {
                        string Ymn = Program.my.mainname(years[i]);
                        //MessageBox.Show(mn);
                        //如果 mn 是 202x 再繼續
                        if (!Program.my.is_string_like(Ymn, "202") || Ymn.Length != 4) continue;
                        string[] md = Directory.GetDirectories(years[i]);
                        for (int j = 0; j < md.Count(); j++)
                        {
                            string mdmn = Program.my.mainname(md[i]); //如 0108 代表 1月8日
                                                                      //如果 mdmn 不是4碼，也跳過
                            if (mdmn.Length != 4) continue;
                            //合併 日期
                            string checkYmd = Ymn + mdmn;
                            if (!Program.my.in_array(checkYmd, allowDate))
                            {
                                //不在保留日，這些要刪除
                                try
                                {
                                    Program.my.deltree(md[i]);
                                    Program.logError("已刪除超過 " + Program.GLOBAL_keepMp4Day.ToString() + " 的資料：" + md[i]);
                                }
                                catch
                                {

                                }
                            }
                        }
                    }
                }
                //都作完了，等下一次工作
                try
                {
                    Thread.Sleep(60 * 1000);
                }
                catch
                {
                }
                Program.threads["MAIN_YT_RUN_6_HOUR"] = new Thread(() => run_6hour());
                Program.threads["MAIN_YT_RUN_6_HOUR"].Start();
                return;
            }
            catch (Exception ex)
            {
                Program.logError("Error...:" + ex.Message + "\r\n" + ex.StackTrace);
                //GC.Collect(); //回收 ram                
                Thread.Sleep(5 * 1000); //休息5秒
                Program.threads["MAIN_YT_RUN_6_HOUR"] = new Thread(() => run_6hour());
                Program.threads["MAIN_YT_RUN_6_HOUR"].Start();
            }
        }
        private DataTable loadYRToGrid()
        {
            string SQL = @"
                SELECT 
                    [id],                    
                    [youtube_url],   
                    [m3u8_url],
                    [del]
                FROM
                    [site]
                WHERE
                    1=1
                    AND [del]='0'                    
                ORDER BY 
                    [id] ASC
            ";
            return Program.my.selectSQL_SAFE(SQL);
        }

    }
}
