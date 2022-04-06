using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using utility;
using theapp;
using System.IO;
using System.Data;
using System.Collections.Concurrent;
using System.Threading;
using System.Diagnostics;

namespace _youtubeToTextCron
{
    public static class Program
    {
        public static ConcurrentDictionary<string, Thread> threads = new ConcurrentDictionary<string, Thread>();
        public static youtube_app yt = new youtube_app();
        public static myinclude my = new myinclude();
        public static string GLOBAL_mp4Path = "";
        public static string GLOBAL_tmpPath = "";
        public static int GLOBAL_keepMp4Day = 7; //保留幾天資料
        public static string base_url = "http://localhost/youtubeToText";
        public static string base_real_url = "https://map.gis.tw/youtubeToText";
        public static string base_tmp = my.pwd() + "\\tmp";
        public static string PWD = my.pwd();
        public static string LOCK_FILE = my.pwd() + "\\lock.txt";

        static FileStream s2 = null;
        public static void log(string data)
        {
            Console.WriteLine(data);
        }
        static public void logError(string data)
        {
            Console.WriteLine(data);
            try
            {
                if (!my.is_dir(PWD + "\\log"))
                {
                    my.mkdir(PWD + "\\log");
                }
                my.file_put_contents(PWD + "\\log\\" + my.date("Y-m-d") + ".txt", my.date("Y-m-d H:i:s") + ":\r\n" + data + "\r\n", true);
            }
            catch
            {
                Thread.Sleep(500);
                logError(data);
            }
        }
        static void Main(string[] args)
        {
            //log(_stts.stts.stts_big52gb("許蓋功，你是個好人嗎?"));
            //log(my.wavToText(PWD + "\\testData\\test.wav"));
            //return;
            GLOBAL_mp4Path = my.getSystemKey("mp4Path");
            GLOBAL_tmpPath = my.getSystemKey("tmpPath");
            GLOBAL_keepMp4Day = Convert.ToInt32(my.getSystemKey("keepMp4Day"));

            if (!my.is_dir(GLOBAL_tmpPath))
            {
                my.mkdir(GLOBAL_tmpPath);
            }
            if (!my.is_dir(GLOBAL_mp4Path))
            {
                my.mkdir(GLOBAL_mp4Path);
            }
            if (my.isFileLocked(LOCK_FILE))
            {
                log("Another process running...");
                return;
            }
            s2 = new FileStream(LOCK_FILE, FileMode.Open, FileAccess.Read, FileShare.None);
            //嘗試當掉就中斷離開
            //AppDomain currentDomain = AppDomain.CurrentDomain;
            //currentDomain.UnhandledException += new UnhandledExceptionEventHandler(myCrash);
            if (!my.is_dir(base_tmp))
            {
                my.mkdir(base_tmp);
            }
            int dt = Convert.ToInt32(my.date("i"));

            //每2小時更新 m3u8
            //每次重啟，優先更新 m3u8
            threads["MAIN_YT_RUN_2_HOUR"] = new Thread(() => yt.run_2hour());
            threads["MAIN_YT_RUN_2_HOUR"].Start();
            Thread.Sleep(1 * 1000); //休息1秒            

            //每分鐘 loop 新的資料到畫面
            threads["MAIN_YT_RUN_1_MIN"] = new Thread(() => yt.run_1min());
            threads["MAIN_YT_RUN_1_MIN"].Start();

            Thread.Sleep(3 * 1000); //休息3秒
            //每30秒抓新的m3u8、解析內容，下載裡面所有的 ts 檔
            threads["MAIN_YT_RUN_30_SECOND"] = new Thread(() => yt.run_30second());
            threads["MAIN_YT_RUN_30_SECOND"].Start();

            Thread.Sleep(3 * 1000); //休息3秒

            //每40秒檢查資料夾有多少個 ts 檔，超過 howManyTsMerge 個 ts 檔就作合併的工作
            //threads["MAIN_YT_RUN_40_SEC"] = new Thread(() => yt.run_40sec());
            //threads["MAIN_YT_RUN_40_SEC"].Start();
            //Thread.Sleep(3 * 1000); //休息3秒

            //每六小時，刪除超過七天的 MP4
            threads["MAIN_YT_RUN_6_HOUR"] = new Thread(() => yt.run_6hour());
            //threads["MAIN_YT_RUN_6_HOUR"].Start();
            while (true)
            {
                //lock process
                try
                {
                    Thread.Sleep(10 * 1000);
                }
                catch
                {

                }
            }
        }
        static private void myCrash(object sender, UnhandledExceptionEventArgs args)
        {
            //yt.isNeedStop = true;
            killAllThreads("ALL");
            System.Environment.Exit(0);
        }
        static private void killAllThreads(string killSingleTime)
        {
            //killSingleTime 可以是 thread index name
            //killSingleTime 可以是 SingleTime
            //killSingleTime 可以是 ALL
            //remove all threads
            //From : http://godleon.blogspot.com/2011/06/linq.html            
            string[] aryKeys = threads.Keys.ToArray();
            foreach (string index in aryKeys)
            {

                //Say 88
                //時間會放在最後 _ 如果 killSingleTime = "ALL" 就全刪
                //平常就是看目前的秒 % killSingleTime
                switch (killSingleTime.ToUpper())
                {
                    case "ALL":
                        {
                            threads[index].Abort();
                            threads[index] = null;
                        }
                        break;
                    default:
                        {
                            //如果是直接指定 index 就刪直接刪
                            if (index == killSingleTime)
                            {
                                threads[index].Abort();
                                threads[index] = null;
                            }
                            else
                            {
                                var d = my.explode("_", index);
                                string need_kill_single_time = d[d.Count() - 1];
                                string minute = my.date("i");
                                if (Convert.ToInt32(minute) % Convert.ToInt32(need_kill_single_time) == 0)
                                {
                                    threads[index].Abort();
                                    threads[index] = null;
                                }
                            }
                        }
                        break;
                }
            }

        }
        static public void killMoreThan120minProcess()
        {
            //刪除超時排程
            try
            {
                List<string> filters = new List<string>();
                filters.Add("ffmpeg");
                //filters.Add("cmd");
                //filters.Add("conhost");
                Process[] processlist = Process.GetProcesses();
                foreach (Process theprocess in processlist)
                {

                    try
                    {
                        string pName = theprocess.ProcessName.ToString();
                        int pPid = theprocess.Id;
                        string pStartTime = theprocess.StartTime.ToString("yyyy-MM-dd HH:mm:ss");
                        if (my.in_array(pName, filters))
                        {
                            Int64 t = Convert.ToInt64(my.time());
                            Int64 pt = Convert.ToInt64(my.strtotime(pStartTime));
                            if (t - pt >= 120 * 60) // kill more than 120 minute...
                            {
                                logError("Kill Process...Success: (" + pPid + ") " + pName + ": " + pStartTime);
                                my.KillProcessAndChildrens(pPid);
                                //theprocess.Kill();
                            }
                        }
                    }
                    catch
                    {
                        //theform.logError("Kill Process...Failure: \r\n" + ex.Message + "\r\n" + ex.StackTrace);
                        //string cmd = "C:\\Windows\\System32\\taskkill.exe /f /pid " + pPid.ToString() + " || exit";
                        //theform.my.system(cmd);
                    }
                }
            }
            catch
            {
                //刪不掉算了
                //log("無法移除超過 120 分鐘的工作執行緒");
            }
        }
    }
}
