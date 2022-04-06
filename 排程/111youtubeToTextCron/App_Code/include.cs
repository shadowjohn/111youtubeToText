using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Cache;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using GFLib.Database;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vosk;
using _stts;
//using System.Reflection;
/*
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="webservice.aspx.cs" Inherits="WaterRegion.Search.webservice.webservice"  %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="utility" %>
<%@ Import Namespace="Newtonsoft.Json" %>
<%@ import Namespace="Newtonsoft.Json.Linq" %>
<%@ Import Namespace="System.Linq" %>
*/

namespace utility
{
    public class myinclude
    {
        private Random rnd = new Random(DateTime.Now.Millisecond);
        private string connString = "";
        Model voskModel = null;
        public myinclude()
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            connString = System.Configuration.ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            //Console.WriteLine("Model: " + pwd() + "\\binary\\vosk\\model\\");
            Vosk.Vosk.SetLogLevel(-1);
            voskModel = new Model("./binary/vosk/model");// pwd() + "\\binary\\vosk\\model");

        }
        public DataTable selectSQL_SAFE(string SQL)
        {
            return selectSQL_SAFE(SQL, new Dictionary<string, string>());
        }
        public DataTable selectSQL_SAFE(string SQL, Dictionary<string, string> m)
        {
            MsSql PDO = new MsSql(connString);
            var pa = new ArrayList();
            List<string> fields = new List<string>();
            List<string> Q_fields = new List<string>();
            foreach (string n in m.Keys)
            {
                fields.Add(n);
                Q_fields.Add("@" + n);
                pa.Add(new SqlParameter { ParameterName = "@" + n, SqlDbType = SqlDbType.NVarChar, Value = m[n] });
            }
            var ra = PDO.Select(SQL, pa);
            PDO.Dispose();
            return ra;
        }
        public List<Dictionary<string, string>> datatable2dictinyary(DataTable ra)
        {
            List<Dictionary<string, string>> o = new List<Dictionary<string, string>>();
            for (int i = 0, max_i = ra.Rows.Count; i < max_i; i++)
            {
                var d = new Dictionary<string, string>();
                foreach (var kk in ra.Columns)
                {
                    string k = kk.ToString();
                    d[k] = ra.Rows[i][k].ToString();
                }
                o.Add(d);
            }
            return o;
        }



        public string gzdecode(string data)
        {
            return data;
        }
        public void execSQL_SAFE(string SQL, Dictionary<string, string> m)
        {
            MsSql PDO = new MsSql(connString);
            var pa = new ArrayList();
            List<string> fields = new List<string>();
            List<string> Q_fields = new List<string>();
            foreach (string n in m.Keys)
            {
                fields.Add(n);
                Q_fields.Add("@" + n);
                pa.Add(new SqlParameter { ParameterName = "@" + n, SqlDbType = SqlDbType.NVarChar, Value = m[n] });
            }
            PDO.Execute(SQL, pa);
            PDO.Dispose();
        }
        public void deleteSQL_SAFE(string tableName, string whereSQL, Dictionary<string, string> m)
        {
            MsSql PDO = new MsSql(connString);
            var pa = new ArrayList();
            List<string> fields = new List<string>();
            List<string> Q_fields = new List<string>();
            foreach (string n in m.Keys)
            {
                fields.Add(n);
                Q_fields.Add("@" + n);
                pa.Add(new SqlParameter { ParameterName = "@" + n, SqlDbType = SqlDbType.NVarChar, Value = m[n] });
            }
            string SQL = @"
                DELETE FROM [" + tableName + @"]
                WHERE
                    1 = 1
                    AND " + whereSQL + @";
            ";
            PDO.Execute(SQL, pa);
            PDO.Dispose();
        }
        public int insertSQL(string tableName, Dictionary<string, string> m)
        {
            MsSql PDO = new MsSql(connString);
            var pa = new ArrayList();
            List<string> fields = new List<string>();
            List<string> Q_fields = new List<string>();
            foreach (string n in m.Keys)
            {
                fields.Add(n);
                Q_fields.Add("@" + n);
                pa.Add(new SqlParameter { ParameterName = "@" + n, SqlDbType = SqlDbType.NVarChar, Value = m[n] });
            }
            string SQL = @"
                INSERT INTO [" + tableName + "]([" + implode("],[", fields) + "])VALUES(" + implode(",", Q_fields) + @")
            ";
            int LAST_ID = PDO.ExecuteReturnIdentity(SQL, pa);
            PDO.Dispose();
            return LAST_ID;
        }
        public void updateSQL_SAFE(string tableName, Dictionary<string, string> m, string WHERE_SQL, Dictionary<string, string> wpa)
        {
            MsSql PDO = new MsSql(connString);
            var pa = new ArrayList();
            List<string> fields = new List<string>();
            foreach (string n in m.Keys)
            {
                fields.Add("[" + n + "]=@" + n);
                pa.Add(new SqlParameter { ParameterName = "@" + n, SqlDbType = SqlDbType.NVarChar, Value = m[n] });
            }
            string SQL = @"
                UPDATE [" + tableName + @"]
                    SET " + implode(",", fields) + @"
                WHERE
                    1=1 AND " + WHERE_SQL + @"
            ";
            foreach (string n in wpa.Keys)
            {
                pa.Add(new SqlParameter { ParameterName = "@" + n, SqlDbType = SqlDbType.NVarChar, Value = wpa[n] });
            }
            PDO.Execute(SQL, pa);
            PDO.Dispose();
        }

        public string secondtodhis(long time)
        {
            //秒數轉成　天時分秒
            //Create by 羽山 
            // 2010-02-07
            string days = string.Format("%02d", time / (24 * 60 * 60));
            days = (Convert.ToInt32(days) >= 1) ? days + "天" : "";
            string hours = string.Format("%02d", (time % (60 * 60 * 24)) / (60 * 60));
            hours = (days == "" && hours == "0") ? "" : hours + "時";
            string mins = string.Format("%02d", (time % (60 * 60)) / (60));
            mins = (days == "" && hours == "" && mins == "0") ? "" : mins + "分";
            string seconds = string.Format("%02d", (time % 60)) + "秒";
            string output = string.Format("%s%s%s%s", days, hours, mins, seconds);
            return output;
        }
        public string fb_date(string datetime)
        {

            //類似 facebook的時間轉換方式
            //傳入日期　格式如 2011-01-19 04:12:12 
            //就會回傳 facebook 的幾秒、幾分鐘、幾小時的那種
            if (datetime == "") return datetime;
            var week_array = new List<string>();
            week_array.Add("星期一");
            week_array.Add("星期二");
            week_array.Add("星期三"); week_array.Add("星期四"); week_array.Add("星期五"); week_array.Add("星期六"); week_array.Add("星期日");
            string timestamp = strtotime(datetime);
            long distance = (Convert.ToInt64(time()) - Convert.ToInt64(timestamp));
            /*echo time();
            echo "<br>";
            echo timestamp;
            echo "<br>";  
            echo distance;
            echo "<br>";*/
            if (distance <= 59)
            {
                return string.Format("%d %s", distance, "秒前");
            }
            else if (distance >= 60 && distance < 59 * 60)
            {
                return string.Format("%d %s", Math.Floor(distance / 60.0), "分鐘前");
            }
            else if (distance >= 60 * 60 && distance < 60 * 60 * 24)
            {
                return string.Format("%d %s", Math.Floor(distance / 60.0 / 60.0), "小時前");
            }
            else if (distance >= 60 * 60 * 24 && distance < 59 * 60 * 24 * 7)
            {
                return string.Format("%s %s", week_array[Convert.ToInt32(date("N", timestamp)) - 1], date("H:i", timestamp));
            }
            else
            {
                return string.Format("%s", date("Y/m/d H:i", timestamp));
            }

        }
        public string str_replace(string r, string t, string data)
        {
            return data.Replace(r, t);
        }
        public string print_table(DataTable ra, string fields = null, string headers = null, string classname = null)
        {
            if (count(ra) == 0)
            {
                return "";
            }
            classname = (classname == null) ? "" : " class=\"" + classname + "\" ";
            if (fields == null || fields == "*")
            {

                string tmp = "<table " + classname + " border='1' cellspacing='0' cellpadding='0'>";
                tmp += "<thead><tr>";
                foreach (DataColumn kc in ra.Columns)
                {
                    string k = kc.ToString();
                    string v = strip_tags(ra.Columns[k].ToString());
                    v = trim(v);
                    tmp += "<th field='" + v + "'>" + k + "</th>";
                }
                tmp += "</tr></thead>";
                tmp += "<tbody>";
                for (int i = 0, max_i = count(ra); i < max_i; i++)
                {
                    tmp += "<tr>";
                    foreach (DataColumn kc in ra.Columns)
                    {
                        string k = kc.ToString();
                        string kk = trim(k);
                        string v = ra.Rows[i][kk].ToString();
                        tmp += "<td field='" + kk + "'>" + v + "</td>";
                    }
                    tmp += "</tr>";
                }
                tmp += "</tbody>";
                tmp += "</table>";
                return tmp;
            }
            else
            {
                string tmp = "<table " + classname + " border='1' cellspacing='0' cellpadding='0'>";
                tmp += "<thead><tr>";
                var m = explode(",", headers);
                for (int k = 0, max_k = m.Count(); k < max_k; k++)
                {
                    string v = m[k];
                    string field = strip_tags(v);
                    field = trim(field);
                    tmp += "<th field='" + field + "'>" + v + "</th>";
                }
                tmp += "</tr></thead>";
                tmp += "<tbody>";
                var m_fields = explode(",", fields);
                for (int i = 0, max_i = count(ra); i < max_i; i++)
                {
                    tmp += "<tr>";
                    foreach (string k in m_fields)
                    {
                        string kk = trim(k);
                        tmp += "<td field='" + k + "'>" + ra.Rows[i][kk].ToString() + "</td>";
                    }
                    tmp += "</tr>";
                }
                tmp += "</tbody>";
                tmp += "</table>";
                return tmp;
            }
        }
        public string print_csv(List<Dictionary<string, string>> ra, string fields = "", string headers = "", bool is_need_header = true)
        {
            if (ra.Count == 0) return "";
            string tmp = "";
            if (fields == "" || fields == "*")
            {
                tmp = "";
                var keys = new List<string>();
                foreach (string kc in ra[0].Keys)
                {
                    string k = kc.ToString();
                    keys.Add(k);
                }

                if (is_need_header)
                {
                    tmp += "\"" + implode("\",\"", keys) + "\"\r\n";
                }
                for (int i = 0, max_i = ra.Count; i < max_i; i++)
                {
                    var d = new List<string>();
                    foreach (string k in keys)
                    {
                        string v = ra[i][k];
                        v = str_replace("\n", " ", v);
                        v = str_replace(",", "，", v);
                        v = addslashes(v);
                        d.Add(v);
                    }
                    tmp += "\"" + implode("\",\"", d) + "\"";
                    if (i != max_i - 1)
                    {
                        tmp += "\r\n";
                    }
                }
                return tmp;
            }
            else
            {
                tmp = "";
                var mheaders = explode(",", headers);
                if (is_need_header)
                {
                    tmp += "\"" + implode("\",\"", mheaders) + "\"\r\n";
                }
                var m_fields = explode(",", fields);
                for (int i = 0, max_i = ra.Count; i < max_i; i++)
                {
                    var d = new List<string>();
                    foreach (string k in m_fields)
                    {

                        string v = str_replace("\n", " ", ra[i][k]);
                        v = str_replace(",", "，", v);
                        d.Add(addslashes(v));
                    }
                    tmp += "\"" + implode("\",\"", d) + "\"";
                    if (i != max_i - 1)
                    {
                        tmp += "\r\n";
                    }
                }
                return tmp;
            }
        }
        public string print_csv(DataTable ra, string fields = "", string headers = "", bool is_need_header = true)
        {
            return print_csv(datatable2dictinyary(ra), fields, headers, is_need_header);
        }
        public string strip_tags(string Txt)
        {
            return Regex.Replace(Txt, "<(.|\\n)*?>", string.Empty);
        }

        public int rand(int min, int max)
        {
            return rnd.Next(min, max);
        }
        public string pwd()
        {
            return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            //return dirname(System.Web.HttpContext.Current.Server.MapPath("~/"));
        }
        public bool is_dir(string path)
        {
            return Directory.Exists(path);
        }
        public bool is_file(string filepath)
        {
            return File.Exists(filepath);
        }
        public void myLog(String data)
        {
            string path = pwd() + "\\Log";
            if (!is_dir(path))
            {
                mkdir(path);
            }
            string filename = String.Format("{0}.txt", date("Y-m-d"));
            string fn = String.Format("{0}\\{1}", path, filename);
            if (!is_file(fn))
            {
                touch(fn);
            }
            file_append_contents(fn, string.Format("\r\n\r\n{0} -\r\n{1}", date("Y-m-d H:i:s"), data));
        }
        public void touch(string fileName)
        {
            FileStream myFileStream = File.Open(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            myFileStream.Close();
            myFileStream.Dispose();
            File.SetLastWriteTimeUtc(fileName, DateTime.UtcNow);
        }
        public void file_append_contents(string filename, string data)
        {
            StreamWriter w = File.AppendText(filename);
            w.Write(data);
            w.Close();
        }
        /*public string get_between(string data, string s_begin, string s_end)
        {
            //http://stackoverflow.com/questions/378415/how-do-i-extract-a-string-of-text-that-lies-between-two-parenthesis-using-net
            string s = data;
            int start = s.IndexOf(s_begin);
            int end = s.IndexOf(s_end);
            return s.Substring(start + s_begin.Length, end - (start + s_begin.Length));
        }*/
        public string get_between(string data, string s_begin, string s_end)
        {
            //http://stackoverflow.com/questions/378415/how-do-i-extract-a-string-of-text-that-lies-between-two-parenthesis-using-net
            //string a = "abcdefg";
            //MessageBox.Show(my.get_between(a, "cde", "g"));
            //return f;
            string s = data;
            int start = s.IndexOf(s_begin);
            string new_s = data.Substring(start + s_begin.Length);
            int end = new_s.IndexOf(s_end);
            return s.Substring(start + s_begin.Length, end);
        }
        public long find_string_appear_in_a_string_counts(string data, string find_string)
        {
            //尋找一個字出現了幾次
            return Regex.Matches(data, find_string).Count;
        }
        public bool is_string_like(string data, string find_string)
        {
            return (data.IndexOf(find_string) == -1) ? false : true;
        }
        public bool is_string_like_new(string data, string find_string)
        {
            /*
              is_string_like($data,$fine_string)

              $mystring = "Hi, this is good!";
              $searchthis = "%thi% goo%";

              $resp = string_like($mystring,$searchthis);


              if ($resp){
                 echo "milike = VERDADERO";
              } else{
                 echo "milike = FALSO";
              }

              Will print:
              milike = VERDADERO

              and so on...

              this is the function:
            */
            bool tieneini = false;
            if (find_string == "") return true;
            var vi = explode("%", find_string);
            int offset = 0;
            for (int n = 0, max_n = vi.Count(); n < max_n; n++)
            {
                if (vi[n] == "")
                {
                    if (vi[0] == "")
                    {
                        tieneini = true;
                    }
                }
                else
                {
                    //newoff =  strpos(data,vi[$n],offset);
                    int newoff = data.IndexOf(vi[n], offset);
                    if (newoff != -1)
                    {
                        if (!tieneini)
                        {
                            if (offset != newoff)
                            {
                                return false;
                            }
                        }
                        if (n == max_n - 1)
                        {
                            if (vi[n] != data.Substring(data.Length - vi[n].Length, vi[n].Length))
                            {
                                return false;
                            }

                        }
                        else
                        {
                            offset = newoff + vi[n].Length;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public bool is_istring_like(string data, string find_string)
        {
            return (data.ToUpper().IndexOf(find_string.ToUpper()) == -1) ? false : true;
        }

        public string getSystemKey(string keyindex)
        {
            return ConfigurationManager.AppSettings[keyindex];
        }
        public bool IsValidEmailAddress(string s)
        {
            if (string.IsNullOrEmpty(s))
                return false;
            else
            {
                var regex = new Regex(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");
                return regex.IsMatch(s) && !s.EndsWith(".");
            }
        }
        public String FilterMetaCharacters(String s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return "";
            }
            else
            {
                Regex re = new Regex("([^A-Za-z0-9@.' _-]+)");
                String filtered = re.Replace(s, "_");
                return filtered;
            }
        }
        //大小寫
        public string strtoupper(string input)
        {
            return input.ToUpper();
        }
        public string strtolower(string input)
        {
            return input.ToLower();
        }
        public string UTF8toBig5(string strInput)
        {
            byte[] strut8 = System.Text.Encoding.Unicode.GetBytes(strInput);
            byte[] strbig5 = System.Text.Encoding.Convert(System.Text.Encoding.Unicode, System.Text.Encoding.Default, strut8);
            return System.Text.Encoding.Default.GetString(strbig5);
        }
        public string Big5toUTF8(string strInput)
        {
            byte[] strut8 = System.Text.Encoding.Unicode.GetBytes(strInput);
            byte[] strbig5 = System.Text.Encoding.Convert(System.Text.Encoding.Default, System.Text.Encoding.Unicode, strut8);
            return System.Text.Encoding.Default.GetString(strbig5);
        }
        public string[] stringToStringArray(string input)
        {
            string[] o = new string[input.Length];
            for(int i=0,max_i=input.Length;i<max_i;i++)
            {
                o[i] = input.Substring(i, 1);
            }
            return o;
        }
        public string dePWD_string(string input, string thekey)
        {
            input = b2s(base64_decode(input));
            thekey = base64_encode(s2b(thekey));
            string xored = "";
            char[] input_arr = input.ToCharArray();
            char[] thekey_arr = thekey.ToCharArray();
            foreach (char ich in input_arr)
            {
                int a = (int)ich;
                for (int j = thekey_arr.Length - 1; j >= 0; j--)
                {
                    int k = (int)thekey_arr[j];
                    a = a ^ k;
                }
                xored = string.Format("{0}{1}", xored, Convert.ToChar(a));
            }
            xored = b2s(base64_decode(xored));
            return xored;
        }

        public string addslashes(string value)
        {
            return value.Replace("'", "\'").Replace("\"", "\\\"");

        }
        public string stripslashes(string value)
        {
            return value.Replace("\\'", "'").Replace("\\\"", "\"");
        }

        //UnixTimeToDateTime
        public DateTime UnixTimeToDateTime(string text)
        {
            System.DateTime dateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
            // Add the number of seconds in UNIX timestamp to be converted.            
            dateTime = dateTime.AddSeconds(Convert.ToDouble(text));
            return dateTime;
        }
        //仿php的date
        public string time()
        {
            return strtotime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }
        public string date()
        {
            return date("Y-m-d H:i:s", strtotime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")));
        }
        public string date(string format)
        {
            return date(format, strtotime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")));
        }
        public string date(string format, string unixtimestamp)
        {
            DateTime tmp = UnixTimeToDateTime(unixtimestamp);
            tmp = tmp.AddHours(+8);
            switch (format)
            {
                case "Y-m-d H:i:s":
                    return tmp.ToString("yyyy-MM-dd HH:mm:ss");
                case "Y/m/d":
                    return tmp.ToString("yyyy/MM/dd");
                case "Y/m/d H:i:s":
                    return tmp.ToString("yyyy/MM/dd HH:mm:ss");
                case "Y/m/d H:i:s.fff":
                    return tmp.ToString("yyyy/MM/dd HH:mm:ss.fff");
                case "Y-m-d_H_i_s":
                    return tmp.ToString("yyyy-MM-dd_HH_mm_ss");
                case "Y-m-d":
                    return tmp.ToString("yyyy-MM-dd");
                case "H:i:s":
                    return tmp.ToString("HH:mm:ss");
                case "H:i":
                    return tmp.ToString("HH:mm");
                case "Y-m-d H:i":
                    return tmp.ToString("yyyy-MM-dd HH:mm");
                case "Y_m_d_H_i_s":
                    return tmp.ToString("yyyy_MM_dd_HH_mm_ss");
                case "Y_m_d_H_i_s_fff":
                    return tmp.ToString("yyyy_MM_dd_HH_mm_ss_fff");
                case "w":
                    //回傳week, sun =0 , sat = 6, mon=1.....
                    return Convert.ToInt16(tmp.DayOfWeek).ToString();
                case "Y":
                    return tmp.ToString("yyyy");
                case "m":
                    return tmp.ToString("MM");
                case "md":
                    return tmp.ToString("MMdd");
                case "d":
                    return tmp.ToString("dd");
                case "H":
                    return tmp.ToString("HH");
                case "i":
                    return tmp.ToString("mm");
                case "s":
                    return tmp.ToString("ss");
                case "Y-m-d H:i:s.fff":
                    return tmp.ToString("yyyy-MM-dd HH:mm:ss.fff");
                case "Y-m-d H:i:s.ffffff":
                    return tmp.ToString("yyyy-MM-dd HH:mm:ss.ffffff");
                case "H:i:s.fff":
                    return tmp.ToString("HH:mm:ss.fff");
                case "H:i:s.ffffff":
                    return tmp.ToString("HH:mm:ss.ffffff");
                case "N":
                    //回傳星期1~星期日 (1~7)
                    Dictionary<string, string> w = new Dictionary<string, string>();
                    w["Monday"] = "1";
                    w["Tuesday"] = "2";
                    w["Wednesday"] = "3";
                    w["Thursday"] = "4";
                    w["Friday"] = "5";
                    w["Saturday"] = "6";
                    w["Sunday"] = "7";
                    return w[tmp.DayOfWeek.ToString()];
            }
            return "";
        }
        //strtotime 轉換成 Unix time
        public string strtotime(string value)
        {
            //create Timespan by subtracting the value provided from
            //the Unix Epoch
            TimeSpan span = (Convert.ToDateTime(value) - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());

            //return the total seconds (which is a UNIX timestamp)
            if (is_string_like(value, "."))
            {
                //有小數點               
                double sec = span.Ticks / (TimeSpan.TicksPerMillisecond / 1000.0) / 1000000.0;
                return sec.ToString();
            }
            else
            {
                return span.TotalSeconds.ToString();
            }
        }
        public string strtotime(DateTime value)
        {
            //create Timespan by subtracting the value provided from
            //the Unix Epoch
            TimeSpan span = (value - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());

            //return the total seconds (which is a UNIX timestamp)
            return span.TotalSeconds.ToString();
        }
        //javascript用的吐js資料
        public string jsAddSlashes(string value)
        {
            value = value.Replace("\\", "\\\\");
            value = value.Replace("\n", "\\n");
            value = value.Replace("\r", "\\r");
            value = value.Replace("\"", "\\\"");
            value = value.Replace("&", "\\x26");
            value = value.Replace("<", "\\x3C");
            value = value.Replace(">", "\\x3E");
            return value;
        }
        public string print_r(object obj)
        {
            return print_r(obj, 0);
        }
        public string pre_print_r(object obj)
        {
            return "<pre>" + print_r(obj) + "</pre>";
        }
        public bool in_array(string find_key, List<string> arr)
        {
            return arr.Contains(find_key);
        }
        public bool in_array(string find_key, string[] arr)
        {
            return arr.Contains(find_key);
        }
        public bool in_array(string find_key, char[] arr)
        {
            string[] o = new string[arr.Count()];
            for (int i = 0; i < arr.Count(); i++)
            {
                o[i] = arr[i].ToString();
            }
            return in_array(find_key, o);
        }
        public bool in_array(string find_key, ArrayList arr)
        {
            return arr.Contains(find_key);
        }

        public bool is_numeric(object Expression)
        {
            if (Expression == null || Expression is DateTime)
                return false;
            if (Expression is Int16 || Expression is Int32 || Expression is Int64 || Expression is Decimal || Expression is Single || Expression is Double || Expression is Boolean)
                return true;
            try
            {
                if (Expression is string)
                    Double.Parse(Expression as string);
                else
                    Double.Parse(Expression.ToString());
                return true;
            }
            catch { } // just dismiss errors but return false
            return false;
        }

        public string print_r(object obj, int recursion)
        {
            StringBuilder result = new StringBuilder();

            // Protect the method against endless recursion
            if (recursion < 5)
            {
                // Determine object type
                Type t = obj.GetType();

                // Get array with properties for this object
                PropertyInfo[] properties = t.GetProperties();

                foreach (PropertyInfo property in properties)
                {
                    try
                    {
                        // Get the property value
                        object value = property.GetValue(obj, null);

                        // Create indenting string to put in front of properties of a deeper level
                        // We'll need this when we display the property name and value
                        string indent = String.Empty;
                        string spaces = "|   ";
                        string trail = "|...";

                        if (recursion > 0)
                        {
                            indent = new StringBuilder(trail).Insert(0, spaces, recursion - 1).ToString();
                        }

                        if (value != null)
                        {
                            // If the value is a string, add quotation marks
                            string displayValue = value.ToString();
                            if (value is string) displayValue = String.Concat('"', displayValue, '"');

                            // Add property name and value to return string
                            result.AppendFormat("{0}{1} = {2}\n", indent, property.Name, displayValue);

                            try
                            {
                                if (!(value is ICollection))
                                {
                                    // Call var_dump() again to list child properties
                                    // This throws an exception if the current property value
                                    // is of an unsupported type (eg. it has not properties)
                                    result.Append(print_r(value, recursion + 1));
                                }
                                else
                                {
                                    // 2009-07-29: added support for collections
                                    // The value is a collection (eg. it's an arraylist or generic list)
                                    // so loop through its elements and dump their properties
                                    int elementCount = 0;
                                    foreach (object element in ((ICollection)value))
                                    {
                                        string elementName = String.Format("{0}[{1}]", property.Name, elementCount);
                                        indent = new StringBuilder(trail).Insert(0, spaces, recursion).ToString();

                                        // Display the collection element name and type
                                        result.AppendFormat("{0}{1} = {2}\n", indent, elementName, element.ToString());

                                        // Display the child properties
                                        result.Append(print_r(element, recursion + 2));
                                        elementCount++;
                                    }

                                    result.Append(print_r(value, recursion + 1));
                                }
                            }
                            catch { }
                        }
                        else
                        {
                            // Add empty (null) property to return string
                            result.AppendFormat("{0}{1} = {2}\n", indent, property.Name, "null");
                        }
                    }
                    catch
                    {
                        // Some properties will throw an exception on property.GetValue()
                        // I don't know exactly why this happens, so for now i will ignore them...
                    }
                }
            }

            return result.ToString();
        }
        /// <summary>

        /// Will return the string contents of a
        /// regular file or the contents of a
        /// response from a URL
        /// </summary>
        /// <param name="fileName">The filename or URL</param>
        /// <returns></returns>
        public byte[] file_get_contents_retry(string url)
        {
            if (url.ToLower().IndexOf("http:") > -1)
            {
                // URL                 
                //http://social.msdn.microsoft.com/Forums/en-US/8050d80a-ca45-4b0c-82dc-81dd1eac496f/retry-catch
                bool redo = false;
                const int maxRetries = 10;
                int retries = 0;
                HttpWebRequest request = null;
                HttpWebResponse response = null;
                byte[] byteData = null;
                do
                {
                    try
                    {
                        request = (HttpWebRequest)WebRequest.Create(url);
                        request.Timeout = 30000;
                        request.Proxy = null;
                        request.UserAgent = "user_agent','Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.93 Safari/537.36";
                        //request.Referer = getSystemKey("HTTP_REFERER");
                        response = (HttpWebResponse)request.GetResponse();
                        Stream stream = response.GetResponseStream();
                        byteData = ReadStream(stream, 5000);
                        stream.Close();
                    }
                    catch (Exception e)
                    {
                        Console.Write(e.Message);
                        //Console.WriteLine(e.Message);
                        redo = true;
                        Thread.Sleep(5);
                        ++retries;
                        //myLog("retry..." + retries);
                    }

                } while (redo && retries < maxRetries);
                response.Close();
                return byteData;
            }
            else
            {
                System.IO.StreamReader sr = new System.IO.StreamReader(url);
                string sContents = sr.ReadToEnd();
                sr.Close();
                return s2b(sContents);
            }
        }
        public byte[] file_get_contents(string url)
        {
            if (url.ToLower().IndexOf("http:") > -1 || url.ToLower().IndexOf("https:") > -1)
            {
                // URL                 
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                HttpWebRequest request = null;
                HttpWebResponse response = null;
                byte[] byteData = null;

                request = (HttpWebRequest)WebRequest.Create(url);
                request.Timeout = 60000;
                request.Proxy = null;
                request.UserAgent = "user_agent','Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.93 Safari/537.36";
                //request.Referer = getSystemKey("HTTP_REFERER");
                response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                byteData = ReadStream(stream, 32765);
                response.Close();
                stream.Close();
                return byteData;
            }
            else
            {
                byte[] data;
                using (var fs = new FileStream(url, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    data = ReadStream(fs, 8192);
                    fs.Close();
                };
                return data;
            }
        }
        private void ExecuteWithRetry(Action action)
        {
            // Use a maximum count, we don't want to loop forever
            // Alternativly, you could use a time based limit (eg, try for up to 30 minutes)
            const int maxRetries = 5;

            bool done = false;
            int attempts = 0;

            while (!done)
            {
                attempts++;
                try
                {
                    action();
                    done = true;
                }
                catch (WebException ex)
                {
                    if (!IsRetryable(ex))
                    {
                        throw;
                    }

                    if (attempts >= maxRetries)
                    {
                        throw;
                    }

                    // Back-off and retry a bit later, don't just repeatedly hammer the connection
                    Thread.Sleep(SleepTime(attempts));
                }
            }
        }

        private int SleepTime(int retryCount)
        {
            // I just made these times up, chose correct values depending on your needs.
            // Progressivly increase the wait time as the number of attempts increase.
            switch (retryCount)
            {
                case 0: return 0;
                case 1: return 1000;
                case 2: return 5000;
                case 3: return 10000;
                default: return 30000;
            }
        }

        private bool IsRetryable(WebException ex)
        {
            return
                ex.Status == WebExceptionStatus.ReceiveFailure ||
                ex.Status == WebExceptionStatus.ConnectFailure ||
                ex.Status == WebExceptionStatus.KeepAliveFailure;
        }
        public void file_put_contents(string filepath, string input)
        {
            file_put_contents(filepath, s2b(input), false);
        }
        public void file_put_contents(string filepath, byte[] input)
        {
            file_put_contents(filepath, input, false);
        }
        public void file_put_contents(string filepath, string input, bool isFileAppend)
        {
            file_put_contents(filepath, s2b(input), isFileAppend);
        }
        public void file_put_contents(string filepath, byte[] input, bool isFileAppend)
        {

            switch (isFileAppend)
            {
                case true:
                    {
                        FileMode FM = new FileMode();
                        if (!is_file(filepath))
                        {
                            FM = FileMode.Create;
                            using (FileStream myFile = File.Open(@filepath, FM, FileAccess.Write, FileShare.Read))
                            {
                                myFile.Seek(myFile.Length, SeekOrigin.Begin);
                                myFile.Write(input, 0, input.Length);
                                myFile.Dispose();
                            }
                        }
                        else
                        {
                            FM = FileMode.Append;
                            using (FileStream myFile = File.Open(@filepath, FM, FileAccess.Write, FileShare.Read))
                            {
                                myFile.Seek(myFile.Length, SeekOrigin.Begin);
                                myFile.Write(input, 0, input.Length);
                                myFile.Dispose();
                            }
                        }
                    }
                    break;
                case false:
                    {
                        using (FileStream myFile = File.Open(@filepath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
                        {
                            myFile.Write(input, 0, input.Length);
                            myFile.Dispose();
                        };
                    }
                    break;
            }
        }

        public string b2s(byte[] input)
        {
            return System.Text.Encoding.UTF8.GetString(input);
        }
        public byte[] s2b(string input)
        {
            return System.Text.Encoding.UTF8.GetBytes(input);
        }
        private byte[] ReadStream(Stream stream, int initialLength)
        {
            if (initialLength < 1)
            {
                initialLength = 32768;
            }
            byte[] buffer = new byte[initialLength];
            int read = 0;
            int chunk;
            while ((chunk = stream.Read(buffer, read, buffer.Length - read)) > 0)
            {
                read += chunk;
                if (read == buffer.Length)
                {
                    int nextByte = stream.ReadByte();
                    if (nextByte == -1)
                    {
                        return buffer;
                    }
                    byte[] newBuffer = new byte[buffer.Length * 2];
                    Array.Copy(buffer, newBuffer, buffer.Length);
                    newBuffer[read] = (byte)nextByte;
                    buffer = newBuffer;
                    read++;
                }
            }
            byte[] bytes = new byte[read];
            Array.Copy(buffer, bytes, read);
            return bytes;
        }
        public string base64_encode(byte[] data)
        {
            //base64編碼
            return Convert.ToBase64String(data);

        }
        public byte[] base64_decode(string data)
        {
            //base64解碼

            return Convert.FromBase64String(data);
        }

        public byte[] file_get_contents_post(string url, string postData)
        {

            HttpWebRequest httpWReq =
            (HttpWebRequest)WebRequest.Create(url);

            //ASCIIEncoding encoding = new ASCIIEncoding();

            byte[] data = Encoding.UTF8.GetBytes(postData);

            httpWReq.Method = "POST";
            httpWReq.ContentType = "application/x-www-form-urlencoded";
            httpWReq.UserAgent = "user_agent','Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.93 Safari/537.36";
            httpWReq.Proxy = null;
            httpWReq.Timeout = 60000;
            //httpWReq.Referer = HttpContext.Current.Request.ServerVariables["SERVER_NAME"];
            //httpWReq.Referer = url;//getSystemKey("HTTP_REFERER");
            httpWReq.ContentLength = data.Length;

            using (Stream stream = httpWReq.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
                stream.Close();
            }

            HttpWebResponse response = (HttpWebResponse)httpWReq.GetResponse();

            Stream streamD = response.GetResponseStream();
            byte[] byteData = ReadStream(streamD, 32767);
            response.Close();
            streamD.Close();
            return byteData;
            //byte[] responseString = new StreamReader(response.GetResponseStream()).ToArray();

        }


        public Dictionary<string, string> convert_dictionary(Dictionary<string, object> input)
        {
            Dictionary<string, string> tmp = new Dictionary<string, string>();
            foreach (string k in input.Keys)
            {
                tmp[k] = input[k].ToString();
            }
            return tmp;
        }

        public string implode(string keyword, string[] arrays)
        {
            return string.Join(keyword, arrays);
        }
        public string implode(string keyword, List<string> arrays)
        {
            return string.Join<string>(keyword, arrays);
        }
        public string implode(string keyword, Dictionary<int, string> arrays)
        {
            string[] tmp = new String[arrays.Keys.Count];
            int i = 0;
            foreach (int k in arrays.Keys)
            {
                tmp[i++] = arrays[k];
            }
            return string.Join(keyword, tmp);
        }
        public string implode(string keyword, Dictionary<string, string> arrays)
        {
            string[] tmp = new String[arrays.Keys.Count];
            int i = 0;
            foreach (string k in arrays.Keys)
            {
                tmp[i++] = arrays[k];
            }
            return string.Join(keyword, tmp);
        }
        public string implode(string keyword, ArrayList arrays)
        {
            string[] tmp = new String[arrays.Count];
            for (int i = 0; i < arrays.Count; i++)
            {
                tmp[i] = arrays[i].ToString();
            }
            return string.Join(keyword, tmp);
        }
        public string[] explode(string keyword, string data)
        {
            return data.Split(new string[] { keyword }, StringSplitOptions.None);
        }
        public string[] explode(string keyword, object data)
        {
            return data.ToString().Split(new string[] { keyword }, StringSplitOptions.None);
        }
        public string[] explode(string[] keyword, string data)
        {
            return data.Split(keyword, StringSplitOptions.None);
        }
        public string selectarray2table(Dictionary<int, Dictionary<string, string>> data, string style)
        {
            //將SELECT 出來的內容吐出table畫面
            string tmp = "";
            style = style.ToLower();
            switch (style)
            {
                case "dot":
                    if (data.Count != 0)
                    {
                        foreach (string k in data[0].Keys)
                        {
                            tmp += k;
                            tmp += ",";
                        }
                        tmp = tmp.Substring(0, tmp.Length - 1);
                        tmp += "\n";
                        for (int i = 0; i < data.Count; i++)
                        {
                            tmp += implode(",", data[i]);
                            if (i != data.Count - 1)
                            {
                                tmp += "\n";
                            }
                        }
                    }
                    break;
                case "normal":
                    if (data.Count != 0)
                    {
                        tmp += "<table>";
                        tmp += "<tr>";
                        tmp += "<th>";
                        string tmp_add = " </th><th>";
                        foreach (string k in data[0].Keys)
                        {
                            tmp += k;
                            tmp += tmp_add;
                        }
                        tmp = tmp.Substring(0, tmp.Length - tmp_add.Length - 1);
                        tmp += " </th></tr>";
                        for (int i = 0; i < data.Count; i++)
                        {
                            tmp += "<tr><td>";
                            tmp += implode(" </td><td>", data[i]);
                            tmp += " </td></tr>";
                        }
                        tmp += "</table>";
                    }
                    break;
                case "normal_center":
                    if (data.Count != 0)
                    {
                        tmp += "<table>";
                        tmp += "<tr>";
                        tmp += "<th>";
                        string tmp_add = " </th><th align=\"center\">";
                        foreach (string k in data[0].Keys)
                        {
                            tmp += k;
                            tmp += tmp_add;
                        }
                        tmp = tmp.Substring(0, tmp.Length - tmp_add.Length - 1);
                        tmp += " </th></tr>";
                        for (int i = 0; i < data.Count; i++)
                        {
                            tmp += "<tr><td align=\"center\">";
                            tmp += implode(" </td><td align=\"center\">", data[i]);
                            tmp += " </td></tr>";
                        }
                        tmp += "</table>";
                    }
                    break;
                case "normal_center_style":
                    if (data.Count != 0)
                    {
                        tmp += "<table cellpadding=\"5\" cellspacing=\"0\" class=\"table_3wa\">";
                        tmp += "<tr>";
                        tmp += "<th>";
                        string tmp_add = " </th><th align=\"center\">";
                        foreach (string k in data[0].Keys)
                        {
                            tmp += k;
                            tmp += tmp_add;
                        }
                        tmp = tmp.Substring(0, tmp.Length - tmp_add.Length - 1);
                        tmp += " </th></tr>";
                        for (int i = 0; i < data.Count; i++)
                        {
                            tmp += "<tr><td align=\"center\">";
                            tmp += implode(" </td><td>", data[i]);
                            tmp += " </td></tr>";
                        }
                        tmp += "</table>";
                    }
                    break;
            }
            return tmp;
        }


        public string size_hum_read_v2(string _size)
        {
            return size_hum_read_v2(Convert.ToInt64(_size));
        }
        public string size_hum_read_v2(long _size)
        {
            if (_size != 0)
            {
                List<string> unit = new List<string>();
                unit.Add("B");
                unit.Add("KB");
                unit.Add("MB");
                unit.Add("GB");
                unit.Add("TB");
                unit.Add("PB");
                int i = Convert.ToInt32(Math.Floor(Math.Log(_size, 1024)));
                return string.Format("{0:0.00}", Math.Round(Convert.ToDouble(_size) / Convert.ToDouble(Math.Pow(1024, i)), 2)) + " " + unit[i];
            }
            else
            {
                return "0 B";
            }
        }

        public string EscapeUnicode(string input)
        {
            StringBuilder sb = new StringBuilder(input.Length);
            foreach (char ch in input)
            {
                if (ch <= 0x7f)
                    sb.Append(ch);
                else
                    sb.AppendFormat(CultureInfo.InvariantCulture, "\\u{0:x4}", (int)ch);
            }
            return sb.ToString();
        }
        public string unEscapeUnicode(string input)
        {
            return Regex.Unescape(input);
        }
        public string json_encode(object input)
        {
            return EscapeUnicode(JsonConvert.SerializeObject(input, Formatting.None));
        }
        public string json_format(string input)
        {
            JArray jdod = json_decode(input);
            return EscapeUnicode(JsonConvert.SerializeObject(jdod, Formatting.Indented));
        }
        public string json_format_utf8(string input)
        {
            JArray jdod = json_decode(input);
            return JsonConvert.SerializeObject(jdod, Formatting.Indented);
        }
        public string trim(string input)
        {
            return input.Trim();
        }
        public Dictionary<string, object> json_decode_output_dictionary(string input)
        {
            return jobjToDictionary(json_decode(input));
        }
        public Dictionary<string, object> jobjToDictionary(JToken obj, string name = null)
        {
            name = name ?? "obj";
            if (obj is JObject)
            {
                var asBag =
                    from prop in (obj as JObject).Properties()
                    let propName = prop.Name
                    let propValue = prop.Value is JValue
                        ? new Dictionary<string, object>()
                        {
                            {prop.Name, prop.Value}
                        }
                        : jobjToDictionary(prop.Name)
                    select new KeyValuePair<string, object>(propName, propValue);
                return asBag.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
            if (obj is JArray)
            {
                var vals = (obj as JArray).Values();
                var alldicts = vals
                    .SelectMany(val => jobjToDictionary(name))
                    .Select(x => x.Value)
                    .ToArray();
                return new Dictionary<string, object>()
            {
                {name, (object)alldicts}
            };
            }
            if (obj is JValue)
            {
                return new Dictionary<string, object>()
            {
                {name, (obj as JValue)}
            };
            }
            return new Dictionary<string, object>()
            {
                {name, null}
            };
        }
        public JArray json_decode(string input)
        {
            input = trim(input);
            if (input.Length != 0)
            {
                if (input.Substring(1, 1) != "[")
                {
                    input = "[" + input + "]";
                    return (JArray)JsonConvert.DeserializeObject<JArray>(input);
                }
                else
                {
                    return (JArray)JsonConvert.DeserializeObject<JArray>(input);
                }
            }
            else
            {
                return null;
            }
        }
        public string nl2br(string input)
        {
            return input.Replace("\n", "<br />");
        }
        public List<string> natsort(List<string> data)
        {
            //自然排序法
            return natsort(data.ToArray()).ToList();
        }
        public string[] natsort(string[] data)
        {
            //自然排序法
            Func<string, object> convert = str =>
            {
                try { return int.Parse(str); }
                catch { return str; }
            };
            var sorted = data.OrderBy(
                str => Regex.Split(str.Replace(" ", ""), "([0-9]+)").Select(convert),
                new EnumerableComparer<object>()).OrderBy(
                   x => x.Length
                );
            return sorted.ToArray();
        }
        public string firstWordUpper(string data)
        {
            //首字大寫
            data = strtolower(data);
            if (data.Length > 0)
            {
                data = data.Substring(0, 1).ToUpper() + data.Substring(1, data.Length - 1);
            }
            return data;
        }
        public string basename(string path)
        {
            return Path.GetFileName(path);
        }
        public string mainname(string path)
        {
            return Path.GetFileNameWithoutExtension(path);
        }
        public string subname(string path)
        {
            return Path.GetExtension(path);
        }
        public long filesize(string path)
        {
            FileInfo f = new FileInfo(path);
            return f.Length;
        }
        public long filemtime(string filename)
        {
            if (!is_file(filename))
            {
                return -1;
            }
            DateTime dt = File.GetLastWriteTime(filename);
            return Convert.ToInt64(strtotime(dt.ToString("yyyy-MM-dd HH:mm:ss")));
        }
        public string size_hum_read(long bytes)
        {
            string[] Suffix = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            int i = 0;
            double dblSByte = Convert.ToDouble(bytes);
            if (bytes > 1024)
                for (i = 0; (bytes / 1024) > 0; i++, bytes /= 1024)
                    dblSByte = bytes / 1024.0;
            return String.Format("{0:0.##} {1}", dblSByte, Suffix[i]);
        }
        public string[] glob(string path)
        {
            //string[] test = my.glob("c:\\tmp");
            //my.echo(my.pre_print_r(test));
            return Directory.GetFiles(path);
        }
        public string[] glob(string path, string patten)
        {
            //string[] test = my.glob("c:\\tmp");
            //my.echo(my.pre_print_r(test));
            return Directory.GetFiles(path, patten);
        }
        public void mkdir(string path)
        {
            Directory.CreateDirectory(path);
        }
        public void copy(string sourceFile, string destFile)
        {
            System.IO.File.Copy(sourceFile, destFile, true);
        }
        public string dirname(string path)
        {
            return Directory.GetParent(path).FullName;
        }


        public string system(string command)
        {
            StringBuilder sb = new StringBuilder();
            string version = System.Environment.OSVersion.VersionString;//读取操作系统版本  
            if (version.Contains("Windows"))
            {
                using (Process p = new Process())
                {
                    p.StartInfo.FileName = "cmd.exe";

                    p.StartInfo.UseShellExecute = false;//是否指定操作系统外壳进程启动程序  
                    p.StartInfo.RedirectStandardInput = true;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.CreateNoWindow = true;//不显示dos命令行窗口  

                    p.Start();//启动cmd.exe  
                    p.StandardInput.WriteLine(command);//输入命令  
                    p.StandardInput.WriteLine("exit");//退出cmd.exe  
                    p.WaitForExit();//等待执行完了，退出cmd.exe  

                    using (StreamReader reader = p.StandardOutput)//截取输出流  
                    {
                        string line = reader.ReadLine();//每次读取一行  
                        while (!reader.EndOfStream)
                        {
                            sb.Append(line).Append("<br />");//在Web中使用<br />换行  
                            line = reader.ReadLine();
                        }
                        p.WaitForExit();//等待程序执行完退出进程  
                        p.Close();//关闭进程  
                        reader.Close();//关闭流  
                    }
                }
            }
            return sb.ToString();
        }

        public string microtime()
        {
            System.DateTime dt = DateTime.Now;
            System.DateTime UnixEpoch = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan span = dt - UnixEpoch;
            return string.Format("{0:0.0000}", Convert.ToDouble((span.Ticks / (TimeSpan.TicksPerMillisecond / 1000))) / 1000000.0);
        }
        public ConcurrentDictionary<string, object> curl_getPost_INIT(string URL, string posts, ConcurrentDictionary<string, object> options = null)
        {
            var N = HttpUtility.ParseQueryString(posts);
            return curl_getPost_INIT(URL, N, null, options);
        }
        public ConcurrentDictionary<string, object> curl_getPost_INIT(string URL, Dictionary<string, string> posts, ConcurrentDictionary<string, object> options = null)
        {
            ConcurrentDictionary<string, string> p = new ConcurrentDictionary<string, string>();
            foreach (string k in posts.Keys)
            {
                p[k] = posts[k];
            }
            return curl_getPost_INIT(URL, p, options);
        }
        public ConcurrentDictionary<string, object> curl_getPost_INIT(string URL, ConcurrentDictionary<string, string> posts, ConcurrentDictionary<string, object> options = null)
        {
            //NameValueCollection postParameters = new NameValueCollection();
            List<string> mPostData = new List<string>();
            int step = 0;
            List<Dictionary<string, string>> upfiles = new List<Dictionary<string, string>>();
            if (posts != null)
            {
                foreach (string k in posts.Keys)
                {
                    //postParameters.Add(k, posts[k]);                
                    if (posts[k].Length > 0 && posts[k].Substring(0, 1) == "@" && is_file(posts[k].Substring(1, posts[k].Length - 1)))
                    {
                        //file_put_contents("C:\\temp\\a.txt", posts[k].Substring(1, posts[k].Length - 1));
                        //這是檔案
                        var d = new Dictionary<string, string>();
                        d[post_encode_string(k)] = posts[k].Substring(1, posts[k].Length - 1);
                        upfiles.Add(d);
                    }
                    else
                    {
                        mPostData.Add(post_encode_string(k) + "=" + post_encode_string(posts[k]));
                    }
                    step++;

                }
            }
            upfiles = (upfiles.Count == 0) ? null : upfiles;
            return curl_getPost_INIT(URL, implode("&", mPostData), upfiles, options);
        }

        public string implode(string keyword, ConcurrentDictionary<int, string> arrays)
        {
            string[] tmp = new String[arrays.Keys.Count];
            int i = 0;
            foreach (int k in arrays.Keys)
            {
                tmp[i++] = arrays[k];
            }
            return string.Join(keyword, tmp);
        }

        public string implode(string keyword, ConcurrentDictionary<string, string> arrays)
        {
            string[] tmp = new String[arrays.Keys.Count];
            int i = 0;
            foreach (string k in arrays.Keys)
            {
                tmp[i++] = arrays[k];
            }
            return string.Join(keyword, tmp);
        }

        private string post_encode_string(string value)
        {
            int limit = 2000;

            StringBuilder sb = new StringBuilder();
            int loops = value.Length / limit;

            for (int i = 0; i <= loops; i++)
            {
                if (i < loops)
                {
                    sb.Append(Uri.EscapeDataString(value.Substring(limit * i, limit)));
                }
                else
                {
                    sb.Append(Uri.EscapeDataString(value.Substring(limit * i)));
                }
            }
            //Uri.EscapeDataString()
            return sb.ToString();

            //return Uri.EscapeDataString(value);
        }
        public string post_decode_string(string value)
        {
            return Uri.UnescapeDataString(value);
        }
        public byte[] byteCombine(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }
        public ConcurrentDictionary<string, object> curl_getPost_INIT(string URL, NameValueCollection posts, NameValueCollection files = null, ConcurrentDictionary<string, object> options = null)
        {
            List<string> mPostData = new List<string>();
            if (posts != null)
            {
                foreach (string k in posts.Keys)
                {
                    string d = post_encode_string(k) + "=" + post_encode_string(posts[k]);
                    mPostData.Add(d);
                }
            }
            List<Dictionary<string, string>> mfiles = new List<Dictionary<string, string>>();
            if (files != null)
            {
                foreach (string k in files.Keys)
                {
                    Dictionary<string, string> d = new Dictionary<string, string>();
                    d[post_encode_string(k)] = post_encode_string(files[k]);
                    mfiles.Add(d);
                }
            }
            return curl_getPost_INIT(URL, implode("&", mPostData), mfiles, options);
        }
        public string htmlspecialchars_decode(string input)
        {
            return HttpUtility.HtmlDecode(input);
        }
        public string htmlspecialchars(string input)
        {
            return HttpUtility.HtmlEncode(input);
        }
        private ConcurrentDictionary<string, object> curl_getPost_INIT(string URL, string postData, List<Dictionary<string, string>> upfiles, ConcurrentDictionary<string, object> options = null)
        {
            //file_put_contents("C:\\temp\\a.txt", postData);
            //From : https://stackoverflow.com/questions/2972643/how-to-use-cookies-with-httpwebrequest
            ConcurrentDictionary<string, object> output = new ConcurrentDictionary<string, object>();
            output["cookies"] = new CookieContainer();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            HttpRequestCachePolicy noCachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
            request.CachePolicy = noCachePolicy;
            request.CookieContainer = (CookieContainer)output["cookies"];
            //request.UserAgent = "user_agent','Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.75 Safari/537.36";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.75 Safari/537.36";
            try
            {


                if (options != null)
                {
                    if (options.ContainsKey("login_id") && options.ContainsKey("login_pd"))
                    {
                        if (options["login_id"].ToString() != "")
                        {
                            CredentialCache mycache = new CredentialCache();
                            Uri uri = new Uri(URL);
                            mycache.Add(uri, "Basic", new NetworkCredential(options["login_id"].ToString(), options["login_pd"].ToString()));
                            //加入另一種 Digest 驗證
                            mycache.Add(
                              new Uri(uri.GetLeftPart(UriPartial.Authority)), // request url's host
                              "Digest",  // authentication type 
                              new NetworkCredential(options["login_id"].ToString(), options["login_pd"].ToString()) // credentials 
                            );
                            request.Credentials = mycache;
                        }
                    }
                    if (options.ContainsKey("timeout"))
                    {
                        request.Timeout = Convert.ToInt32(options["timeout"]);
                    }
                    if (options.ContainsKey("cookie"))
                    {
                        //request.Headers.Add("Cookie", options["cookie"].ToString());
                        //request.CookieContainer.Add( = options["cookie"].ToString();      
                        request.CookieContainer = new CookieContainer();
                        Uri uri = new Uri(URL);
                        request.CookieContainer.SetCookies(uri, options["cookie"].ToString());
                    }
                    if (options.ContainsKey("user_agent"))
                    {
                        request.UserAgent = options["user_agent"].ToString();
                    }
                    if (options.ContainsKey("headers") && options["headers"] != null)
                    {
                        foreach (string k in ((ConcurrentDictionary<string, string>)options["headers"]).Keys)
                        {
                            request.Headers[k] = ((ConcurrentDictionary<string, string>)options["headers"])[k];
                        }
                    }
                }
                request.Proxy = null;




                HttpWebResponse response = null;
                if (postData == null || postData == "")
                {
                    //GET         
                    request.Method = "GET";
                    response = (HttpWebResponse)request.GetResponse();
                    Stream stream = response.GetResponseStream();
                    output["data"] = ReadStream(stream, 32765);
                    stream.Close();
                }
                else
                {
                    //Post


                    string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");
                    // The first boundary
                    byte[] firstBoundaryBytes = System.Text.Encoding.UTF8.GetBytes("--" + boundary + "\r\n");
                    byte[] boundaryBytes = System.Text.Encoding.UTF8.GetBytes("\r\n--" + boundary + "\r\n");
                    // The last boundary
                    byte[] trailer = System.Text.Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");

                    request.Method = "POST";
                    request.ContentType = "multipart/form-data; boundary=" + boundary + "";

                    // Get request stream
                    Stream requestStream = request.GetRequestStream();
                    var m = explode("&", postData);
                    for (int i = 0, max_i = m.Count(); i < max_i; i++)
                    {
                        var d = explode("=", m[i]);
                        if (d.Length < 2) continue;
                        // Write item to stream
                        string key = post_decode_string(d[0]);
                        string keyvalue = post_decode_string(d[1]);
                        byte[] formItemBytes = System.Text.Encoding.UTF8.GetBytes(string.Format("Content-Disposition: form-data; name={0};\r\n\r\n{1}",
                            key, keyvalue));
                        if (i == 0)
                        {
                            requestStream.Write(firstBoundaryBytes, 0, firstBoundaryBytes.Length);
                            Array.Clear(firstBoundaryBytes, 0, firstBoundaryBytes.Length);
                            firstBoundaryBytes = null;
                        }
                        else
                        {
                            requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);
                        }
                        requestStream.Write(formItemBytes, 0, formItemBytes.Length);
                        Array.Clear(formItemBytes, 0, formItemBytes.Length);
                        formItemBytes = null;
                    }

                    if (upfiles != null && upfiles.Count > 0)
                    {
                        foreach (Dictionary<string, string> d in upfiles)
                        {
                            foreach (string keyname in d.Keys)
                            {
                                //keyname = string.Join("", d.Keys);
                                string filename = post_decode_string(d[keyname]);
                                string bn = basename(filename);
                                string dekeyname = post_decode_string(keyname);
                                if (File.Exists(filename))
                                {
                                    int bytesRead = 0;
                                    byte[] buffer = new byte[8192];
                                    byte[] formItemBytes = System.Text.Encoding.UTF8.GetBytes(string.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: application/octet-stream\r\n\r\n", dekeyname, filename));
                                    requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);
                                    requestStream.Write(formItemBytes, 0, formItemBytes.Length);
                                    Array.Clear(formItemBytes, 0, formItemBytes.Length);
                                    formItemBytes = null;
                                    using (FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
                                    {
                                        while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                                        {
                                            // Write file content to stream, byte by byte
                                            requestStream.Write(buffer, 0, bytesRead);
                                        }

                                        fileStream.Close();
                                    }
                                    Array.Clear(buffer, 0, buffer.Length);
                                    buffer = null;
                                }
                            }
                        }
                    }

                    // Write trailer and close stream
                    requestStream.Write(trailer, 0, trailer.Length);
                    requestStream.Close();

                    Array.Clear(boundaryBytes, 0, boundaryBytes.Length);
                    Array.Clear(trailer, 0, trailer.Length);
                    boundaryBytes = null;
                    trailer = null;

                    response = (HttpWebResponse)request.GetResponse();
                    Stream streamD = response.GetResponseStream();
                    output["data"] = ReadStream(streamD, 32767);
                    streamD.Close();
                }
                output["headers"] = new Dictionary<string, string>();
                foreach (var k in response.Headers)
                {
                    ((Dictionary<string, string>)output["headers"])[k.ToString()] = response.Headers[k.ToString()].ToString();
                }
                output["realCookie"] = response.Headers[HttpResponseHeader.SetCookie];
                output["size"] = response.ContentLength.ToString();
                response.Close();
                output["reason"] = "";
                output["status"] = "OK";
                return output;
            }
            catch (Exception ex)
            {
                output["status"] = "NO";
                output["data"] = new byte[0];
                output["reason"] = ex.Message + "\n\r" + ex.StackTrace;
                return output;
            }
        }
        public string wavToText(string wavFile)
        {
            if (is_file(wavFile))
            {
                string data = VoskWavToText(voskModel, wavFile);
                var jd = json_decode(data)[0];
                return _stts.stts.stts_gb2big5(jd["text"].ToString().Replace(" ", ""));
            }
            return "";
        }
        private string VoskWavToText(Model model, string wavFile)
        {
            // Demo byte buffer
            VoskRecognizer rec = new VoskRecognizer(model, 8000.0f);
            rec.SetMaxAlternatives(0);
            rec.SetWords(true);
            using (Stream source = File.OpenRead(wavFile))
            {
                byte[] buffer = new byte[8000];
                int bytesRead;
                while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                {
                    if (rec.AcceptWaveform(buffer, bytesRead))
                    {
                        //Console.WriteLine(rec.Result());
                    }
                    else
                    {
                        //Console.WriteLine(rec.PartialResult());
                    }
                }
            }
            //Console.WriteLine(rec.FinalResult());
            return rec.FinalResult();
        }
        public void unlink(string filepath)
        {
            try
            {
                if (is_file(filepath))
                {
                    File.Delete(filepath);
                }
            }
            catch { }
        }
        public Dictionary<string, string> jObjectToDictionary(JObject obj)
        {
            var d = new Dictionary<string, string>();
            foreach (var k in obj)
            {
                d[k.Key] = obj[k.Key].ToString();
            }
            return d;
        }
        public bool is_PortOpen(string host, int port, int timeout_ms)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    var result = client.BeginConnect(host, port, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(timeout_ms);
                    client.EndConnect(result);
                    return success;
                }
            }
            catch
            {
                return false;
            }
        }
        public string UrlEncode(string str)
        {
            StringBuilder sb = new StringBuilder();
            byte[] byStr = System.Text.Encoding.UTF8.GetBytes(str); //預設是System.Text.Encoding.Default.GetBytes(str)
            for (int i = 0; i < byStr.Length; i++)
            {
                sb.Append(@"%" + Convert.ToString(byStr[i], 16));
            }

            return (sb.ToString());
        }
        public ConcurrentDictionary<string, object> curl_getPost_continue(ConcurrentDictionary<string, object> C, string URL, string postData, ConcurrentDictionary<string, object> options = null)
        {
            //無上傳資料時
            return curl_getPost_continue(C, URL, postData, null, options);
        }
        public ConcurrentDictionary<string, object> curl_getPost_continue(ConcurrentDictionary<string, object> C, string URL, string postData, List<Dictionary<string, string>> upfiles, ConcurrentDictionary<string, object> options = null)
        {
            ConcurrentDictionary<string, object> output = new ConcurrentDictionary<string, object>();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            HttpRequestCachePolicy noCachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
            request.CachePolicy = noCachePolicy;
            request.Proxy = null;
            //request.UserAgent = "user_agent','Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.75 Safari/537.36";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.75 Safari/537.36";
            request.CookieContainer = (CookieContainer)C["cookies"];
            C["cookies"] = request.CookieContainer;
            try
            {
                if (options != null)
                {
                    if (options.ContainsKey("login_id") && options.ContainsKey("login_pd"))
                    {
                        if (options["login_id"].ToString() != "")
                        {
                            CredentialCache mycache = new CredentialCache();
                            Uri uri = new Uri(URL);
                            mycache.Add(uri, "Basic", new NetworkCredential(options["login_id"].ToString(), options["login_pd"].ToString()));
                            //加入另一種 Digest 驗證
                            mycache.Add(
                              new Uri(uri.GetLeftPart(UriPartial.Authority)), // request url's host
                              "Digest",  // authentication type 
                              new NetworkCredential(options["login_id"].ToString(), options["login_pd"].ToString()) // credentials 
                            );
                            request.Credentials = mycache;
                        }
                    }
                    if (options.ContainsKey("timeout"))
                    {
                        request.Timeout = Convert.ToInt32(options["timeout"]);
                    }
                    if (options.ContainsKey("user_agent"))
                    {
                        request.UserAgent = options["user_agent"].ToString();
                    }

                    //file_put_contents(pwd() + "\\log\\cookie.txt", json_encode(C["cookies"]));
                    if (options.ContainsKey("cookie"))
                    {
                        //request.Headers.Add("Cookie", options["cookie"].ToString());
                        //request.CookieContainer.Add( = options["cookie"].ToString();      
                        //request.CookieContainer = new CookieContainer();
                        /*
                        List<Cookie> LC = cookieStrToCookie(options["cookie"].ToString());
                        for (int i = 0, max_i = LC.Count; i < max_i; i++)
                        {
                            request.CookieContainer.Add(LC[i]);
                        }
                        */
                        Uri uri = new Uri(URL);
                        request.CookieContainer.SetCookies(uri, options["cookie"].ToString());
                    }
                }
                HttpWebResponse response = null;
                if (postData == "")
                {
                    //GET         
                    request.Method = "GET";
                    response = (HttpWebResponse)request.GetResponse();
                    Stream stream = response.GetResponseStream();
                    output["data"] = ReadStream(stream, 32765);
                    stream.Close();
                }
                else
                {
                    //Post


                    string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");
                    // The first boundary
                    byte[] firstBoundaryBytes = System.Text.Encoding.UTF8.GetBytes("--" + boundary + "\r\n");
                    byte[] boundaryBytes = System.Text.Encoding.UTF8.GetBytes("\r\n--" + boundary + "\r\n");
                    // The last boundary
                    byte[] trailer = System.Text.Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");

                    request.Method = "POST";
                    request.ContentType = "multipart/form-data; boundary=" + boundary + "";

                    // Get request stream
                    Stream requestStream = request.GetRequestStream();
                    var m = explode("&", postData);
                    for (int i = 0, max_i = m.Count(); i < max_i; i++)
                    {
                        var d = explode("=", m[i]);
                        if (d.Length < 2) continue;
                        // Write item to stream
                        string key = post_decode_string(d[0]);
                        string keyvalue = post_decode_string(d[1]);
                        byte[] formItemBytes = System.Text.Encoding.UTF8.GetBytes(string.Format("Content-Disposition: form-data; name={0};\r\n\r\n{1}",
                            key, keyvalue));
                        if (i == 0)
                        {
                            requestStream.Write(firstBoundaryBytes, 0, firstBoundaryBytes.Length);
                            Array.Clear(firstBoundaryBytes, 0, firstBoundaryBytes.Length);
                            firstBoundaryBytes = null;
                        }
                        else
                        {
                            requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);
                        }
                        requestStream.Write(formItemBytes, 0, formItemBytes.Length);
                        Array.Clear(formItemBytes, 0, formItemBytes.Length);
                        formItemBytes = null;
                    }

                    if (upfiles != null && upfiles.Count > 0)
                    {
                        foreach (Dictionary<string, string> d in upfiles)
                        {
                            foreach (string keyname in d.Keys)
                            {
                                //keyname = string.Join("", d.Keys);
                                string filename = post_decode_string(d[keyname]);
                                string bn = basename(filename);
                                string dekeyname = post_decode_string(keyname);
                                if (File.Exists(filename))
                                {
                                    int bytesRead = 0;
                                    byte[] buffer = new byte[8192];
                                    byte[] formItemBytes = System.Text.Encoding.UTF8.GetBytes(string.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: application/octet-stream\r\n\r\n", dekeyname, filename));
                                    requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);
                                    requestStream.Write(formItemBytes, 0, formItemBytes.Length);
                                    Array.Clear(formItemBytes, 0, formItemBytes.Length);
                                    formItemBytes = null;
                                    using (FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
                                    {
                                        while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                                        {
                                            // Write file content to stream, byte by byte
                                            requestStream.Write(buffer, 0, bytesRead);
                                        }

                                        fileStream.Close();
                                    }
                                    Array.Clear(buffer, 0, buffer.Length);
                                    buffer = null;
                                }
                            }
                        }
                    }

                    // Write trailer and close stream
                    requestStream.Write(trailer, 0, trailer.Length);
                    requestStream.Close();

                    Array.Clear(boundaryBytes, 0, boundaryBytes.Length);
                    Array.Clear(trailer, 0, trailer.Length);
                    boundaryBytes = null;
                    trailer = null;

                    response = (HttpWebResponse)request.GetResponse();
                    Stream streamD = response.GetResponseStream();
                    output["data"] = ReadStream(streamD, 32767);
                    streamD.Close();
                }
                output["headers"] = new Dictionary<string, string>();
                foreach (var k in response.Headers)
                {
                    ((Dictionary<string, string>)output["headers"])[k.ToString()] = response.Headers[k.ToString()].ToString();
                }
                output["status"] = "OK";
                output["realCookie"] = response.Headers[HttpResponseHeader.SetCookie];
                output["size"] = response.ContentLength.ToString();
                response.Close();
                output["reason"] = "";

                return output;
            }
            catch (Exception ex)
            {
                output["status"] = "NO";
                output["data"] = new byte[0];
                output["reason"] = ex.Message + "\n\r" + ex.StackTrace;
                return output;
            }
        }
        public string get_ssl_expire_datetime(string url)
        {
            //取得 https ssl 憑證過期時間，失敗回傳 ""
            try
            {
                var uri = new Uri(url);
                if (!is_string_like_new(url, "https://%")) return "";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://" + uri.Host);
                X509Certificate cert2 = null;
                HttpWebResponse response = null;

                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                    X509Certificate cert = request.ServicePoint.Certificate;
                    cert2 = new X509Certificate2(cert);
                }
                catch
                {
                    X509Certificate cert = request.ServicePoint.Certificate;

                    cert2 = new X509Certificate2(cert);

                }
                finally
                {
                    response.Close();
                }

                if (cert2 != null)
                {
                    string cedate = cert2.GetExpirationDateString();
                    //Console.WriteLine(cedate);
                    return date("Y-m-d H:i:s", strtotime(cedate));
                }
            }
            catch
            {
                return "";
            }
            return "";
        }
        public string get_host_expire_from_url(string url)
        {
            //透過 twnic 查詢網址可以用到何時
            try
            {
                var uri = new Uri(url);
                string host = uri.Host;
                List<string> o = new List<string>();
                if (is_string_like_new(host, "%.gov.tw%"))
                {
                    //政府機關的不管
                    return "2077-08-07";
                }
                TcpClient tcpClinetWhois = new TcpClient("whois.twnic.net.tw", 43);
                tcpClinetWhois.SendTimeout = 5 * 1000;
                tcpClinetWhois.ReceiveTimeout = 5 * 1000;
                NetworkStream networkStreamWhois = tcpClinetWhois.GetStream();
                BufferedStream bufferedStreamWhois = new BufferedStream(networkStreamWhois);
                StreamWriter streamWriter = new StreamWriter(bufferedStreamWhois);

                streamWriter.WriteLine(host);
                streamWriter.Flush();

                StreamReader streamReaderReceive = new StreamReader(bufferedStreamWhois);

                while (!streamReaderReceive.EndOfStream)
                {
                    string data = streamReaderReceive.ReadLine();
                    if (is_string_like_new(data, "%expires on%"))
                    {
                        string dt = strtotime(trim(get_between(data, "expires on ", "(")));
                        //Console.WriteLine(dt);

                        return date("Y-m-d H:i:s", dt);
                    }

                    //o.Add(data);
                }
            }
            catch
            {

            }
            return "";
        }
        public string get_url_last_modify_datetime(string url)
        {
            //TODO 取得 url 最後修改時間
            //取得某網路路徑最後修改的時間
            //取得網頁、圖片，最後更新時間
            try
            {
                HttpWebRequest request = null;
                HttpWebResponse response = null;

                request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "HEAD";
                request.Timeout = 30000;
                request.Proxy = null;
                request.UserAgent = "user_agent','Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.93 Safari/537.36";
                //request.Referer = getSystemKey("HTTP_REFERER");
                response = (HttpWebResponse)request.GetResponse();
                //Stream stream = response.GetResponseStream();
                //byte[] byteData = ReadStream(stream, 5000);
                //stream.Close();

                if (response.StatusCode == HttpStatusCode.OK && response.LastModified != null)
                {
                    return date("Y-m-d H:i:s", strtotime(response.LastModified));
                }
            }
            catch
            {
                return "-1";
            }
            return "-1";

        }
        public byte[] file_get_contents_post(string URL, ConcurrentDictionary<string, string> posts)
        {
            //NameValueCollection postParameters = new NameValueCollection();
            string[] mPostData = new string[posts.Keys.Count];
            int step = 0;
            foreach (string k in posts.Keys)
            {
                //postParameters.Add(k, posts[k]);
                mPostData[step] = post_encode_string(k) + "=" + post_encode_string(posts[k]);
                //file_put_contents("C:\\temp\\a.txt", mPostData[step], true);
                step++;

            }
            return file_get_contents_post(URL, implode("&", mPostData));
        }
        public byte[] file_get_contents_post(string URL, Dictionary<string, string> posts)
        {
            //NameValueCollection postParameters = new NameValueCollection();
            string[] mPostData = new string[posts.Keys.Count];
            int step = 0;
            foreach (string k in posts.Keys)
            {
                //postParameters.Add(k, posts[k]);
                mPostData[step] = post_encode_string(k) + "=" + post_encode_string(posts[k]);
                //file_put_contents("C:\\temp\\a.txt", mPostData[step], true);
                step++;

            }
            return file_get_contents_post(URL, implode("&", mPostData));
        }
        public bool isAllowChars(string data, string allowCharsString)
        {
            var m = allowCharsString.ToCharArray();
            //echo(json_encode(m));
            for (int i = 0, max_i = data.Length; i < max_i; i++)
            {
                if (!in_array(data[i].ToString(), m))
                {
                    return false;
                }
            }
            return true;
        }

        public int count(DataTable ra)
        {
            return ra.Rows.Count;
        }
        public int count(List<Dictionary<string, string>> ra)
        {
            return ra.Count;
        }
        public void sendMail(List<string> TO, List<string> CC, List<string> BCC, List<string> upfile, string title, string body)
        {
            //待開發
            return;
        }

        public int strlen(string data)
        {
            return data.Length;
        }
        private string getSystemInfo(JArray jd, string key)
        {
            foreach (string k in jd)
            {
                var v = jd[k];
                if (v["systemName"] != null && v["systemName"].ToString() == key)
                {
                    return v["systemData"].ToString();
                }
            }
            return null;
        }

        public List<Dictionary<string, string>> array_sort(List<Dictionary<string, string>> array, string on, string order = "SORT_DESC")
        {

            switch (order)
            {
                case "SORT_ASC":
                    //echo "ASC";
                    {
                        //Array.Sort(sortable_array);
                        return new List<Dictionary<string, string>>(array.OrderBy(dict => Convert.ToInt32(dict[on])));
                    }
                case "SORT_DESC":
                    {
                        //echo "DESC";
                        return new List<Dictionary<string, string>>(array.OrderByDescending(dict => Convert.ToInt32(dict[on])));
                    }
            }
            return array;
        }
        public List<Dictionary<string, string>> sortByArrayName(List<Dictionary<string, string>> arr, List<string> sort_arr, string field)
        {
            //可以讓一個陣列參考一個現成的陣列排序某個欄位
            var output = new List<Dictionary<string, string>>();
            //natrsort($arr);
            for (int i = 0, max_i = sort_arr.Count(); i < max_i; i++)
            {

                for (int j = arr.Count() - 1; j >= 0; j--)
                {

                    if (arr[j][field] == sort_arr[i])
                    {

                        output.Add(arr[j]);
                        //array_splice($arr,$j, 1);
                        arr.RemoveAt(j);
                    }
                }

                for (int j = arr.Count() - 1; j >= 0; j--)
                {
                    //alert(print_r(arr[j],true));
                    string finds = sort_arr[i];
                    if (arr[j][field].IndexOf(finds) == 0)
                    {
                        output.Add(arr[j]);
                        //array_splice($arr,$j, 1);
                        arr.RemoveAt(j);
                    }
                }
                //最後再把沒排序到的，全放回output
            }
            for (int j = 0, max_j = arr.Count(); j < max_j; j++)
            {
                output.Add(arr[j]);
            }
            return output;
        }
        public bool isFileLocked(string fileName)
        {
            try
            {
                FileStream fs = File.OpenWrite(fileName);
                fs.Close();
                return false;
            }

            catch (Exception) { return true; }
        }
        public ConcurrentDictionary<string, string> youtubeJsonParser(string json)
        {
            //把 youtube-dl.exe -j 解出來的 json 轉成 如:
            // 95 m3u8 path...
            // 96 m3u8 path...
            // 94 m3u8 path...
            ConcurrentDictionary<string, string> output = new ConcurrentDictionary<string, string>();
            try
            {
                var jd = json_decode(json);
                for (int i = 0, max_i = jd[0]["formats"].Count(); i < max_i; i++)
                {
                    output[jd[0]["formats"][i]["format_id"].ToString()] = jd[0]["formats"][i]["url"].ToString();
                }
                return output;
            }
            catch
            {
                return null;
            }
        }
        public List<ConcurrentDictionary<string, string>> m3u8Parser(string m3u8_data)
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

也有可能是
#EXTM3U
#EXT-X-VERSION:3
#EXT-X-TARGETDURATION:5
#EXT-X-MEDIA-SEQUENCE:4412192
#EXT-X-DISCONTINUITY-SEQUENCE:163498
#EXT-X-PROGRAM-DATE-TIME:2021-01-12T22:52:51.174+00:00
#EXTINF:5.0,
https://r8---sn-ipoxu-umbe.googlevideo.com/videoplayback/id/13C8jdbqQcI.1/itag/95/source/yt_live_broadcast/expire/1610524514/ei/AVP-X73rOr2Os8IPz6-YkAI/ip/211.20.175.252/requiressl/yes/ratebypass/yes/live/1/sgoap/gir%3Dyes%3Bitag%3D140/sgovp/gir%3Dyes%3Bitag%3D136/hls_chunk_host/r8---sn-ipoxu-umbe.googlevideo.com/playlist_duration/30/manifest_duration/30/vprv/1/playlist_type/DVR/initcwndbps/4980/mh/xU/mm/44/mn/sn-ipoxu-umbe/ms/lva/mv/m/mvi/8/pl/24/keepalive/yes/mt/1610502742/sparams/expire,ei,ip,id,itag,source,requiressl,ratebypass,live,sgoap,sgovp,playlist_duration,manifest_duration,vprv,playlist_type/sig/AOq0QJ8wRQIgGbFET1XacEOocCDQHomn2Jt5gsAYj-OEF4QwXk0brfsCIQDuQbYk7fTaOUVfrzLc5GY0otkQUgs5gaE6r6BqSwoqOw%3D%3D/lsparams/hls_chunk_host,initcwndbps,mh,mm,mn,ms,mv,mvi,pl/lsig/AG3C_xAwRQIhAN8fu3WE2E_-1V-kIftG54pk3dOg5Hp9JtPrMMXP_oJ1AiBwN_koX8vqDRiWHrPTtkcOmLffAA2SViGY7EWwRP12yA%3D%3D/playlist/index.m3u8/sq/4412192/goap/clen%3D81060%3Blmt%3D1610489584611668/govp/clen%3D639724%3Blmt%3D1610489584611666/dur/5.000/file/seg.ts
#EXTINF:5.0,
https://r8---sn-ipoxu-umbe.googlevideo.com/videoplayback/id/13C8jdbqQcI.1/itag/95/source/yt_live_broadcast/expire/1610524514/ei/AVP-X73rOr2Os8IPz6-YkAI/ip/211.20.175.252/requiressl/yes/ratebypass/yes/live/1/sgoap/gir%3Dyes%3Bitag%3D140/sgovp/gir%3Dyes%3Bitag%3D136/hls_chunk_host/r8---sn-ipoxu-umbe.googlevideo.com/playlist_duration/30/manifest_duration/30/vprv/1/playlist_type/DVR/initcwndbps/4980/mh/xU/mm/44/mn/sn-ipoxu-umbe/ms/lva/mv/m/mvi/8/pl/24/keepalive/yes/mt/1610502742/sparams/expire,ei,ip,id,itag,source,requiressl,ratebypass,live,sgoap,sgovp,playlist_duration,manifest_duration,vprv,playlist_type/sig/AOq0QJ8wRQIgGbFET1XacEOocCDQHomn2Jt5gsAYj-OEF4QwXk0brfsCIQDuQbYk7fTaOUVfrzLc5GY0otkQUgs5gaE6r6BqSwoqOw%3D%3D/lsparams/hls_chunk_host,initcwndbps,mh,mm,mn,ms,mv,mvi,pl/lsig/AG3C_xAwRQIhAN8fu3WE2E_-1V-kIftG54pk3dOg5Hp9JtPrMMXP_oJ1AiBwN_koX8vqDRiWHrPTtkcOmLffAA2SViGY7EWwRP12yA%3D%3D/playlist/index.m3u8/sq/4412193/goap/clen%3D81399%3Blmt%3D1610489584611677/govp/clen%3D734717%3Blmt%3D1610489584611675/dur/5.000/file/seg.ts
#EXTINF:4.999,
https://r8---sn-ipoxu-umbe.googlevideo.com/videoplayback/id/13C8jdbqQcI.1/itag/95/source/yt_live_broadcast/expire/1610524514/ei/AVP-X73rOr2Os8IPz6-YkAI/ip/211.20.175.252/requiressl/yes/ratebypass/yes/live/1/sgoap/gir%3Dyes%3Bitag%3D140/sgovp/gir%3Dyes%3Bitag%3D136/hls_chunk_host/r8---sn-ipoxu-umbe.googlevideo.com/playlist_duration/30/manifest_duration/30/vprv/1/playlist_type/DVR/initcwndbps/4980/mh/xU/mm/44/mn/sn-ipoxu-umbe/ms/lva/mv/m/mvi/8/pl/24/keepalive/yes/mt/1610502742/sparams/expire,ei,ip,id,itag,source,requiressl,ratebypass,live,sgoap,sgovp,playlist_duration,manifest_duration,vprv,playlist_type/sig/AOq0QJ8wRQIgGbFET1XacEOocCDQHomn2Jt5gsAYj-OEF4QwXk0brfsCIQDuQbYk7fTaOUVfrzLc5GY0otkQUgs5gaE6r6BqSwoqOw%3D%3D/lsparams/hls_chunk_host,initcwndbps,mh,mm,mn,ms,mv,mvi,pl/lsig/AG3C_xAwRQIhAN8fu3WE2E_-1V-kIftG54pk3dOg5Hp9JtPrMMXP_oJ1AiBwN_koX8vqDRiWHrPTtkcOmLffAA2SViGY7EWwRP12yA%3D%3D/playlist/index.m3u8/sq/4412194/goap/clen%3D81060%3Blmt%3D1610489584611686/govp/clen%3D789313%3Blmt%3D1610489584611684/dur/4.999/file/seg.ts
#EXTINF:5.0,
https://r8---sn-ipoxu-umbe.googlevideo.com/videoplayback/id/13C8jdbqQcI.1/itag/95/source/yt_live_broadcast/expire/1610524514/ei/AVP-X73rOr2Os8IPz6-YkAI/ip/211.20.175.252/requiressl/yes/ratebypass/yes/live/1/sgoap/gir%3Dyes%3Bitag%3D140/sgovp/gir%3Dyes%3Bitag%3D136/hls_chunk_host/r8---sn-ipoxu-umbe.googlevideo.com/playlist_duration/30/manifest_duration/30/vprv/1/playlist_type/DVR/initcwndbps/4980/mh/xU/mm/44/mn/sn-ipoxu-umbe/ms/lva/mv/m/mvi/8/pl/24/keepalive/yes/mt/1610502742/sparams/expire,ei,ip,id,itag,source,requiressl,ratebypass,live,sgoap,sgovp,playlist_duration,manifest_duration,vprv,playlist_type/sig/AOq0QJ8wRQIgGbFET1XacEOocCDQHomn2Jt5gsAYj-OEF4QwXk0brfsCIQDuQbYk7fTaOUVfrzLc5GY0otkQUgs5gaE6r6BqSwoqOw%3D%3D/lsparams/hls_chunk_host,initcwndbps,mh,mm,mn,ms,mv,mvi,pl/lsig/AG3C_xAwRQIhAN8fu3WE2E_-1V-kIftG54pk3dOg5Hp9JtPrMMXP_oJ1AiBwN_koX8vqDRiWHrPTtkcOmLffAA2SViGY7EWwRP12yA%3D%3D/playlist/index.m3u8/sq/4412195/goap/clen%3D81060%3Blmt%3D1610489584611695/govp/clen%3D615546%3Blmt%3D1610489584611693/dur/5.000/file/seg.ts
#EXTINF:5.0,
https://r8---sn-ipoxu-umbe.googlevideo.com/videoplayback/id/13C8jdbqQcI.1/itag/95/source/yt_live_broadcast/expire/1610524514/ei/AVP-X73rOr2Os8IPz6-YkAI/ip/211.20.175.252/requiressl/yes/ratebypass/yes/live/1/sgoap/gir%3Dyes%3Bitag%3D140/sgovp/gir%3Dyes%3Bitag%3D136/hls_chunk_host/r8---sn-ipoxu-umbe.googlevideo.com/playlist_duration/30/manifest_duration/30/vprv/1/playlist_type/DVR/initcwndbps/4980/mh/xU/mm/44/mn/sn-ipoxu-umbe/ms/lva/mv/m/mvi/8/pl/24/keepalive/yes/mt/1610502742/sparams/expire,ei,ip,id,itag,source,requiressl,ratebypass,live,sgoap,sgovp,playlist_duration,manifest_duration,vprv,playlist_type/sig/AOq0QJ8wRQIgGbFET1XacEOocCDQHomn2Jt5gsAYj-OEF4QwXk0brfsCIQDuQbYk7fTaOUVfrzLc5GY0otkQUgs5gaE6r6BqSwoqOw%3D%3D/lsparams/hls_chunk_host,initcwndbps,mh,mm,mn,ms,mv,mvi,pl/lsig/AG3C_xAwRQIhAN8fu3WE2E_-1V-kIftG54pk3dOg5Hp9JtPrMMXP_oJ1AiBwN_koX8vqDRiWHrPTtkcOmLffAA2SViGY7EWwRP12yA%3D%3D/playlist/index.m3u8/sq/4412196/goap/clen%3D81399%3Blmt%3D1610489584611704/govp/clen%3D580395%3Blmt%3D1610489584611702/dur/5.000/file/seg.ts
#EXT-X-DISCONTINUITY
#EXT-X-PROGRAM-DATE-TIME:2021-01-13T01:54:48.222+00:00
#EXTINF:4.96,
https://r8---sn-ipoxu-umbe.googlevideo.com/videoplayback/id/13C8jdbqQcI.1/itag/95/source/yt_live_broadcast/expire/1610524514/ei/AVP-X73rOr2Os8IPz6-YkAI/ip/211.20.175.252/requiressl/yes/ratebypass/yes/live/1/sgoap/gir%3Dyes%3Bitag%3D140/sgovp/gir%3Dyes%3Bitag%3D136/hls_chunk_host/r8---sn-ipoxu-umbe.googlevideo.com/playlist_duration/30/manifest_duration/30/vprv/1/playlist_type/DVR/initcwndbps/4980/mh/xU/mm/44/mn/sn-ipoxu-umbe/ms/lva/mv/m/mvi/8/pl/24/keepalive/yes/mt/1610502742/sparams/expire,ei,ip,id,itag,source,requiressl,ratebypass,live,sgoap,sgovp,playlist_duration,manifest_duration,vprv,playlist_type/sig/AOq0QJ8wRQIgGbFET1XacEOocCDQHomn2Jt5gsAYj-OEF4QwXk0brfsCIQDuQbYk7fTaOUVfrzLc5GY0otkQUgs5gaE6r6BqSwoqOw%3D%3D/lsparams/hls_chunk_host,initcwndbps,mh,mm,mn,ms,mv,mvi,pl/lsig/AG3C_xAwRQIhAN8fu3WE2E_-1V-kIftG54pk3dOg5Hp9JtPrMMXP_oJ1AiBwN_koX8vqDRiWHrPTtkcOmLffAA2SViGY7EWwRP12yA%3D%3D/playlist/index.m3u8/sq/4412197/goap/clen%3D80721%3Blmt%3D1610502895950629/govp/clen%3D45831%3Blmt%3D1610502895950627/dur/4.960/file/seg.ts
#EXTINF:5.0,
https://r8---sn-ipoxu-umbe.googlevideo.com/videoplayback/id/13C8jdbqQcI.1/itag/95/source/yt_live_broadcast/expire/1610524514/ei/AVP-X73rOr2Os8IPz6-YkAI/ip/211.20.175.252/requiressl/yes/ratebypass/yes/live/1/sgoap/gir%3Dyes%3Bitag%3D140/sgovp/gir%3Dyes%3Bitag%3D136/hls_chunk_host/r8---sn-ipoxu-umbe.googlevideo.com/playlist_duration/30/manifest_duration/30/vprv/1/playlist_type/DVR/initcwndbps/4980/mh/xU/mm/44/mn/sn-ipoxu-umbe/ms/lva/mv/m/mvi/8/pl/24/keepalive/yes/mt/1610502742/sparams/expire,ei,ip,id,itag,source,requiressl,ratebypass,live,sgoap,sgovp,playlist_duration,manifest_duration,vprv,playlist_type/sig/AOq0QJ8wRQIgGbFET1XacEOocCDQHomn2Jt5gsAYj-OEF4QwXk0brfsCIQDuQbYk7fTaOUVfrzLc5GY0otkQUgs5gaE6r6BqSwoqOw%3D%3D/lsparams/hls_chunk_host,initcwndbps,mh,mm,mn,ms,mv,mvi,pl/lsig/AG3C_xAwRQIhAN8fu3WE2E_-1V-kIftG54pk3dOg5Hp9JtPrMMXP_oJ1AiBwN_koX8vqDRiWHrPTtkcOmLffAA2SViGY7EWwRP12yA%3D%3D/playlist/index.m3u8/sq/4412198/goap/clen%3D81060%3Blmt%3D1610502895950638/govp/clen%3D51119%3Blmt%3D1610502895950636/dur/5.000/file/seg.ts

            List<ConcurrentDictionary<string, string>> o = [
                {
                    "DATE" : "2021-01-06"
                    "START_DT": 2021-01-06T07:37:39.467+00:00 + 8小時，轉 timestamp ，如 1609921344.471
                    "TS_PATH": "",
                    "TS_MD5" : "",
                    "DURATION"
                }
            ]
            */

            List<ConcurrentDictionary<string, string>> output = new List<ConcurrentDictionary<string, string>>();
            try
            {
                m3u8_data = m3u8_data.Trim();
                m3u8_data = m3u8_data.Replace("\r", "");
                //用這個切
                var m = explode("#EXT-X-PROGRAM-DATE-TIME:", m3u8_data);
                if (m.Count() < 2)
                {
                    //格式不對~_~
                    return null;
                }
                m = explode("\n", m[1]);
                if (m.Count() < 1) return null;
                //第 0 行是日期 2021-01-06T07:37:39.467+00:00
                double dt = Convert.ToDouble(strtotime(m[0].Trim().Replace("+00:00", "").Replace("T", " "))) + 8 * 60 * 60; //+8小時
                //從 1 行開始是 
                //#EXTINF:5.0,
                //https://r2---sn-ipoxu-umb6.googlevideo.com/videoplayback/id/tWdI0YfY93Y.1/itag/95/source/yt_live_broadcast/expire/1609939060/ei/FGT1X7qPA7uss8IPgauLgA4/ip/211.20.175.252/requiressl/yes/ratebypass/yes/live/1/sgoap/gir%3Dyes%3Bitag%3D140/sgovp/gir%3Dyes%3Bitag%3D136/hls_chunk_host/r2---sn-ipoxu-umb6.googlevideo.com/playlist_duration/30/manifest_duration/30/vprv/1/playlist_type/DVR/initcwndbps/5450/mh/GI/mm/44/mn/sn-ipoxu-umb6/ms/lva/mv/m/mvi/2/pl/24/keepalive/yes/beids/9466588/mt/1609917155/sparams/expire,ei,ip,id,itag,source,requiressl,ratebypass,live,sgoap,sgovp,playlist_duration,manifest_duration,vprv,playlist_type/sig/AOq0QJ8wRgIhALkfwKjgWCXSXlAWlVAKN8nyLIGlEzW_WTzEDfhpsXZ-AiEAiYwxxEWDzniXW9BS-0Tl8v-UGA04r8S6YBlQCBNMHEo%3D/lsparams/hls_chunk_host,initcwndbps,mh,mm,mn,ms,mv,mvi,pl/lsig/AG3C_xAwRAIgOnyJ8sB5MnepvzFKh3h5h_PR5TQxe0e14gVTkP3nTeACIBDXmtNN7Hm97ZWLF0sMVdH_Zo9wnR-q4MtXnxVE9mU5/playlist/index.m3u8/sq/4886891/goap/clen%3D81126%3Blmt%3D1609659626117291/govp/clen%3D222556%3Blmt%3D1609659626117288/dur/5.000/file/seg.ts
                //#EXTINF:5.0,
                //https://r2---sn-ipoxu-umb6.googlevideo.com/videoplayback/id/tWdI0YfY93Y.1/itag/95/source/yt_live_broadcast/expire/1609939060/ei/FGT1X7qPA7uss8IPgauLgA4/ip/211.20.175.252/requiressl/yes/ratebypass/yes/live/1/sgoap/gir%3Dyes%3Bitag%3D140/sgovp/gir%3Dyes%3Bitag%3D136/hls_chunk_host/r2---sn-ipoxu-umb6.googlevideo.com/playlist_duration/30/manifest_duration/30/vprv/1/playlist_type/DVR/initcwndbps/5450/mh/GI/mm/44/mn/sn-ipoxu-umb6/ms/lva/mv/m/mvi/2/pl/24/keepalive/yes/beids/9466588/mt/1609917155/sparams/expire,ei,ip,id,itag,source,requiressl,ratebypass,live,sgoap,sgovp,playlist_duration,manifest_duration,vprv,playlist_type/sig/AOq0QJ8wRgIhALkfwKjgWCXSXlAWlVAKN8nyLIGlEzW_WTzEDfhpsXZ-AiEAiYwxxEWDzniXW9BS-0Tl8v-UGA04r8S6YBlQCBNMHEo%3D/lsparams/hls_chunk_host,initcwndbps,mh,mm,mn,ms,mv,mvi,pl/lsig/AG3C_xAwRAIgOnyJ8sB5MnepvzFKh3h5h_PR5TQxe0e14gVTkP3nTeACIBDXmtNN7Hm97ZWLF0sMVdH_Zo9wnR-q4MtXnxVE9mU5/playlist/index.m3u8/sq/4886892/goap/clen%3D81126%3Blmt%3D1609659626117301/govp/clen%3D153810%3Blmt%3D1609659626117298/dur/5.000/file/seg.ts
                //#EXTINF:5.0,
                double sum = 0;
                for (int i = 1; i < m.Count() - 1; i += 2)
                {
                    if (!is_string_like(m[i], "EXTINF"))
                    {
                        continue;
                    }
                    ConcurrentDictionary<string, string> d = new ConcurrentDictionary<string, string>();
                    d["DURATION"] = m[i].Replace("#EXTINF:", "").Replace(",", "").Trim();
                    d["START_DT"] = (dt + sum).ToString();
                    sum += Convert.ToDouble(d["DURATION"]);
                    d["DATE"] = date("Y-m-d", d["START_DT"]);
                    d["TS_PATH"] = m[i + 1];
                    d["TS_MD5"] = md5(d["TS_PATH"]); //md5
                    output.Add(d);
                }
                //MessageBox.Show(dt.ToString()); //1609921344.471

                return output;
            }
            catch //(Exception ex)
            {
                //MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
                return null;
            }
        }
        public string md5(string str)
        {
            using (var cryptoMD5 = System.Security.Cryptography.MD5.Create())
            {
                //將字串編碼成 UTF8 位元組陣列
                var bytes = Encoding.UTF8.GetBytes(str);
                //取得雜湊值位元組陣列
                var hash = cryptoMD5.ComputeHash(bytes);
                //取得 MD5
                var md5 = BitConverter.ToString(hash)
                  .Replace("-", String.Empty)
                  .ToUpper();
                return md5;
            }
        }
        public void KillProcessAndChildrens(int pid)
        {
            ManagementObjectSearcher processSearcher = new ManagementObjectSearcher
              ("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection processCollection = processSearcher.Get();

            // We must kill child processes first!
            if (processCollection != null)
            {
                foreach (ManagementObject mo in processCollection)
                {
                    KillProcessAndChildrens(Convert.ToInt32(mo["ProcessID"])); //kill child processes(also kills childrens of childrens etc.)
                }
            }
            // Then kill parents.
            try
            {
                Process proc = Process.GetProcessById(pid);
                if (!proc.HasExited) proc.Kill();
            }
            catch (ArgumentException)
            {
                // Process already exited.
                string cmd = "taskkill /f /pid " + pid.ToString() + " || exit";
                system(cmd);
            }
        }
        public bool deltree(string target_dir)
        {
            //From : https://dotblogs.com.tw/grepu9/2013/03/20/98267
            try
            {
                bool result = false;
                string[] files = Directory.GetFiles(target_dir);
                string[] dirs = Directory.GetDirectories(target_dir);
                foreach (string file in files)
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }
                foreach (string dir in dirs)
                {
                    deltree(dir);
                }
                Directory.Delete(target_dir, false);
                return result;
            }
            catch
            {
                return false;
            }
        }
        public Dictionary<string, string> mergeFiles(List<string> inputFiles, string outputFile)
        {
            //用來合併檔案
            //From : https://stackoverflow.com/questions/3556755/how-to-merge-efficiently-gigantic-files-with-c-sharp
            Dictionary<string, string> o = new Dictionary<string, string>();
            o["status"] = "OK";
            o["error_log"] = "";
            try
            {
                using (var output = File.OpenWrite(outputFile))
                {
                    foreach (var inputFile in inputFiles)
                    {
                        using (var input = File.OpenRead(inputFile))
                        {
                            try
                            {
                                input.CopyTo(output);
                            }
                            catch
                            {
                                o["status"] = "NO";
                                o["error_log"] += "合併檔案失敗...：" + inputFile + "→" + outputFile + "\r\n";
                            }
                        }
                    }
                }
            }
            catch
            {

            }
            return o;
        }
        public string system_background(string command)
        {
            return system_background(command, 0);
        }

        public string system_background(string command, int timeout)
        {

            StringBuilder sb = new StringBuilder();
            string version = System.Environment.OSVersion.VersionString;//读取操作系统版本  
            if (version.Contains("Windows"))
            {
                using (Process p = new Process())
                {


                    p.StartInfo.FileName = "cmd.exe";

                    p.StartInfo.UseShellExecute = false;//是否指定操作系统外壳进程启动程序  
                    p.StartInfo.RedirectStandardInput = true;
                    p.StartInfo.RedirectStandardOutput = false;
                    p.StartInfo.CreateNoWindow = true;//不显示dos命令行窗口  

                    p.Start();//启动cmd.exe  
                    p.StandardInput.WriteLine(command);//输入命令                      
                    if (timeout == 0)
                    {
                        p.StandardInput.WriteLine("exit");//退出cmd.exe 
                        p.WaitForExit();//等待执行完了，退出cmd.exe  
                    }
                    else if (timeout == -1)
                    {
                        //wont wait
                        //wont close
                    }
                    else
                    {
                        p.StandardInput.WriteLine("exit");//退出cmd.exe 
                        p.WaitForExit(timeout);//等待执行完了，退出cmd.exe  
                    }
                }
            }
            return sb.ToString();
        }
        public DataTable ConvertHTMLTablesToDataTable(string HTML)
        {
            DataTable dt = null;
            DataRow dr = null;
            //DataColumn dc = null;
            string TableExpression = "<table[^>]*>(.*?)</table>";
            string HeaderExpression = "<th[^>]*>(.*?)</th>";
            string RowExpression = "<tr[^>]*>(.*?)</tr>";
            string ColumnExpression = "<td[^>]*>(.*?)</td>";
            bool HeadersExist = false;
            int iCurrentColumn = 0;
            int iCurrentRow = 0;

            // Get a match for all the tables in the HTML    
            MatchCollection Tables = Regex.Matches(HTML, TableExpression, RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);

            // Loop through each table element    
            foreach (Match Table in Tables)
            {

                // Reset the current row counter and the header flag    
                iCurrentRow = 0;
                HeadersExist = false;

                // Add a new table to the DataSet    
                dt = new DataTable();

                // Create the relevant amount of columns for this table (use the headers if they exist, otherwise use default names)    
                if (Table.Value.Contains("<th"))
                {
                    // Set the HeadersExist flag    
                    HeadersExist = true;

                    // Get a match for all the rows in the table    
                    MatchCollection Headers = Regex.Matches(Table.Value, HeaderExpression, RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);

                    // Loop through each header element    
                    foreach (Match Header in Headers)
                    {
                        //dt.Columns.Add(Header.Groups(1).ToString);  
                        dt.Columns.Add(Header.Groups[1].ToString());

                    }
                }
                else
                {
                    for (int iColumns = 1; iColumns <= Regex.Matches(Regex.Matches(Regex.Matches(Table.Value, TableExpression, RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase)[0].ToString(), RowExpression, RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase)[0].ToString(), ColumnExpression, RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase).Count; iColumns++)
                    {
                        dt.Columns.Add("Column " + iColumns);
                    }
                }

                // Get a match for all the rows in the table    
                MatchCollection Rows = Regex.Matches(Table.Value, RowExpression, RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);

                // Loop through each row element    
                foreach (Match Row in Rows)
                {

                    // Only loop through the row if it isn't a header row    
                    if (!(iCurrentRow == 0 & HeadersExist == true))
                    {

                        // Create a new row and reset the current column counter    
                        dr = dt.NewRow();
                        iCurrentColumn = 0;

                        // Get a match for all the columns in the row    
                        MatchCollection Columns = Regex.Matches(Row.Value, ColumnExpression, RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);

                        // Loop through each column element    
                        foreach (Match Column in Columns)
                        {

                            DataColumnCollection columns = dt.Columns;

                            if (!columns.Contains("Column " + iCurrentColumn))
                            {
                                //Add Columns  
                                dt.Columns.Add("Column " + iCurrentColumn);
                            }
                            // Add the value to the DataRow    
                            dr[iCurrentColumn] = Column.Groups[1].ToString();
                            // Increase the current column    
                            iCurrentColumn += 1;

                        }

                        // Add the DataRow to the DataTable    
                        dt.Rows.Add(dr);

                    }

                    // Increase the current row counter    
                    iCurrentRow += 1;
                }


            }

            return (dt);

        }
    }
    /// <summary>
    /// Compares two sequences.
    /// </summary>
    /// <typeparam name="T">Type of item in the sequences.</typeparam>
    /// <remarks>
    /// Compares elements from the two input sequences in turn. If we
    /// run out of list before finding unequal elements, then the shorter
    /// list is deemed to be the lesser list.
    /// </remarks>
    public class EnumerableComparer<T> : IComparer<IEnumerable<T>>
    {
        /// <summary>
        /// Create a sequence comparer using the default comparer for T.
        /// </summary>
        public EnumerableComparer()
        {
            comp = Comparer<T>.Default;
        }

        /// <summary>
        /// Create a sequence comparer, using the specified item comparer
        /// for T.
        /// </summary>
        /// <param name="comparer">Comparer for comparing each pair of
        /// items from the sequences.</param>
        public EnumerableComparer(IComparer<T> comparer)
        {
            comp = comparer;
        }

        /// <summary>
        /// Object used for comparing each element.
        /// </summary>
        private IComparer<T> comp;


        /// <summary>
        /// Compare two sequences of T.
        /// </summary>
        /// <param name="x">First sequence.</param>
        /// <param name="y">Second sequence.</param>
        public int Compare(IEnumerable<T> x, IEnumerable<T> y)
        {
            using (IEnumerator<T> leftIt = x.GetEnumerator())
            using (IEnumerator<T> rightIt = y.GetEnumerator())
            {
                while (true)
                {
                    bool left = leftIt.MoveNext();
                    bool right = rightIt.MoveNext();

                    if (!(left || right)) return 0;

                    if (!left) return -1;
                    if (!right) return 1;

                    int itemResult = comp.Compare(leftIt.Current, rightIt.Current);
                    if (itemResult != 0) return itemResult;
                }
            }
        }
    }
}