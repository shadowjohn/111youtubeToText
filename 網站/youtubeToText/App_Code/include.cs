
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
using System.Net;
using System.Net.Cache;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.UI.WebControls;
using GFLib.Database;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GFLib.Mail;
using System.IO.Compression;
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
    public class myinclude : System.Web.Services.WebService
    {

        public Dictionary<string, string> GLOBAL_filter = new Dictionary<string, string>();
        private Random rnd = new Random(DateTime.Now.Millisecond);
        public string base_url = System.Web.HttpContext.Current.Request.Url.Scheme + "://" + System.Web.HttpContext.Current.Request.Url.Authority + System.Web.HttpContext.Current.Request.ApplicationPath.TrimEnd('/');
        public string base_real_url = "https://map.gis.tw/SystemReport";
        public string base_dir = Directory.GetParent(System.Web.HttpContext.Current.Server.MapPath("~/")).FullName;
        public string SESSION_PREFIX = "SystemReport";
        public myinclude()
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            GLOBAL_filter["firewall"] = "name,ApplicationName,ServiceName,Enabled,Protocol,AllowDeny,InOut,LocalPorts,RemotePorts,LocalAddresses,RemoteAddresses";
            GLOBAL_filter["task"] = "name,basename,fullpathname,run_way";
            GLOBAL_filter["schedule"] = "name,cmd";
            GLOBAL_filter["service"] = "name,contents,cmd";
            GLOBAL_filter["installed_software"] = "DisplayName,OSBit,DisplayVersion";
        }
        public GFLib.Database.MsSql PDO = null;
        public bool isLinkToDB()
        {
            return (PDO != null);
        }
        public bool isLogin()
        {
            if (Session[SESSION_PREFIX + "_isLogin"] != null && Session[SESSION_PREFIX + "_isLogin"].ToString() != "Y")
            {
                Session[SESSION_PREFIX + "_isLogin"] = "";
            }
            return (Session[SESSION_PREFIX + "_isLogin"] != null && Session[SESSION_PREFIX + "_isLogin"].ToString() == "Y");
        }
        public bool isAdmin()
        {
            return (Session[SESSION_PREFIX + "_isAdmin"] != null && Session[SESSION_PREFIX + "_isAdmin"].ToString() == "Y");
        }
        public string getArgument(string title)
        {
            linkToDB();
            string SQL = "SELECT TOP 1 [value] FROM [argument] WHERE [title]=@title";
            var pa = new Dictionary<string, string>();
            pa["title"] = title;
            var ra = selectSQL_SAFE(SQL, pa);
            if (count(ra) == 0)
            {
                return "";
            }
            else
            {
                return htmlspecialchars_decode(ra.Rows[0]["value"].ToString());
            }
        }
        public DataTable selectSQL_SAFE(string SQL, Dictionary<string, string> m)
        {
            var pa = new ArrayList();
            List<string> fields = new List<string>();
            List<string> Q_fields = new List<string>();
            foreach (string n in m.Keys)
            {
                fields.Add(n);
                Q_fields.Add("@" + n);
                pa.Add(new SqlParameter { ParameterName = "@" + n, SqlDbType = SqlDbType.NVarChar, Value = m[n] });
            }
            return PDO.Select(SQL, pa);
        }
        private int word_counts(string orin_string, string pattern)
        {
            //計算 pattern 在 orin_string 出現的次數
            int count = 0;
            int a = 0;
            while ((a = orin_string.IndexOf(pattern, a)) != -1)
            {
                a += pattern.Length;
                count++;
            }
            return count;
        }
        public string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        /// <summary>
        /// 可以回傳 Error 的 select
        /// </summary>
        /// <param name="strSQL"></param>
        /// <param name="parameters"></param>
        /// <param name="Timeout">逾時時間(秒)，預設為30秒</param>        
        /// <returns></returns>
        public Dictionary<string, object> selectE(string strSQL, SqlParameter[] parameters, int Timeout = 30)
        {
            Dictionary<string, object> OUTPUT = new Dictionary<string, object>();
            try
            {
                Dictionary<string, object> _param = new Dictionary<string, object>();
                //如果strSQL 出現問號的數量，等同 parameters 的數量，把 ? 取代成 @DATA_0 @DATA_1 @DATA_2 ...
                int question_counts = word_counts(strSQL, "?");
                if (parameters != null && question_counts == parameters.Length)
                {

                    for (int i = 0; i < question_counts; i++)
                    {
                        strSQL = ReplaceFirst(strSQL, "?", "@DATA_" + i.ToString());
                        parameters[i].ParameterName = "@DATA_" + i.ToString();
                    }
                }

                ArrayList pa = new ArrayList();
                if (parameters != null)
                {
                    for (int i = 0, max_i = parameters.Length; i < max_i; i++)
                    {
                        pa.Add(parameters[i]);
                        _param[parameters[i].ParameterName] = parameters[i].Value;
                    }
                }


                //2021-03-05 增加觀察 select 時間，超過3秒，就寫到 tblSQLLog
                myinclude my = new myinclude();
                Int64 st = Convert.ToInt64(my.microtime());
                var ds = PDO.Select(strSQL, pa, Timeout);
                Int64 et = Convert.ToInt64(my.microtime());
                if (et - st >= 3000000) // 3000ms
                {
                    string _SQL = @"
                        INSERT INTO [tblSQLLog]([SQLString],[keyvalues],[Createtime],[Exectime])VALUES(@SQLString,@keyvalues,@Createtime,@Exectime)
                    ";
                    var _pa = new ArrayList();
                    _pa.Add(new SqlParameter("@SQLString", strSQL));
                    _pa.Add(new SqlParameter("@keyvalues", JsonConvert.SerializeObject(_param, Formatting.Indented)));
                    _pa.Add(new SqlParameter("@Createtime", my.date("Y-m-d H:i:s")));
                    _pa.Add(new SqlParameter("@Exectime", ((et - st) / 1000).ToString()));
                    PDO.Execute(_SQL.ToString(), _pa);
                }

                OUTPUT["status"] = "OK";
                OUTPUT["data"] = ds;
                return OUTPUT;
            }
            catch (Exception e)
            {
                myLog(strSQL);
                myLog("Parms:" + string.Join(",", parameters.Select(x => x.Value).ToArray()));
                myLog(e.Message.ToString());
                myLog(e.StackTrace.ToString());
                OUTPUT["status"] = "NO";
                OUTPUT["data"] = e.Message + "\r\n" + e.StackTrace;
                return OUTPUT;
            }
        }
        public Dictionary<string, object> selectE(string strSQL)
        {
            //多載，不用重寫
            return selectE(strSQL, null);
        }
        public string pdo_get_field_from_id(string table, string id, string fieldname)
        {
            string SQL = @"
                SELECT 
                    TOP 1
                    [" + fieldname + @"] 
                FROM 
                    [" + table + @"] 
                WHERE
                    [id]=@id
                ";
            var PA = new Dictionary<string, string>();
            PA["id"] = id;
            var ra = selectSQL_SAFE(SQL, PA);
            if (count(ra) != 0)
            {
                return ra.Rows[0][fieldname].ToString();
            }
            else
            {
                return "";
            }
        }
        public DataTable pdo_get_data_from_table(string table, string where, Dictionary<string, string> pa)
        {
            string SQL = @"
              SELECT                
                *
              FROM  
                [" + table + @"]
              WHERE
                1=1
                AND " + where + @"
            ";
            return selectSQL_SAFE(SQL, pa);
        }
        public string gzdecode(string data)
        {
            return data;
        }
        public void execSQL_SAFE(string SQL, Dictionary<string, string> m)
        {
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
        }
        public void deleteSQL_SAFE(string tableName, string whereSQL, Dictionary<string, string> m)
        {
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

        }
        public int insertSQL(string tableName, Dictionary<string, string> m)
        {
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
            return PDO.ExecuteReturnIdentity(SQL, pa);
        }
        public void updateSQL_SAFE(string tableName, Dictionary<string, string> m, string WHERE_SQL, Dictionary<string, string> wpa)
        {
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
        }
        public void download_file(string filePath, string displayName)
        {
            displayName = basename(displayName);
            HttpContext.Current.Response.Headers.Clear();
            HttpContext.Current.Response.AppendHeader("ContentType", "application/octet-stream");
            HttpContext.Current.Response.AppendHeader("Content-Disposition", "attachment;filename=\"" + displayName + "\"");
            HttpContext.Current.Response.AppendHeader("Content-Length", filesize(filePath).ToString());
            HttpContext.Current.Response.TransmitFile(filePath);
        }
        public byte[] Unzip(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    //gs.CopyTo(mso);
                    CopyTo(gs, mso);
                }

                return mso.ToArray();
            }
        }
        private void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }
        public string secondtodhis(long time)
        {
            //秒數轉成　天時分秒
            //Create by 羽山 
            // 2010-02-07
            string days = string.Format("{0}", time / (24 * 60 * 60));
            days = (Convert.ToInt32(days) >= 1) ? days + "天" : "";
            string hours = string.Format("{0}", (time % (60 * 60 * 24)) / (60 * 60));
            hours = (days == "" && hours == "0") ? "" : hours + "時";
            string mins = string.Format("{0}", (time % (60 * 60)) / (60));
            mins = (days == "" && hours == "" && mins == "0") ? "" : mins + "分";
            string seconds = string.Format("{0}", (time % 60)) + "秒";
            string output = string.Format("{0}{1}{2}{3}", days, hours, mins, seconds);
            return output;
        }
        public string fb_date(string datetime)
        {

            //類似 facebook的時間轉換方式
            //傳入日期　格式如 2011-01-19 04:12:12 
            //就會回傳 facebook 的幾秒、幾分鐘、幾小時的那種
            if (datetime == "") return datetime;
            var week_array = new List<string> { "星期一", "星期二", "星期三", "星期四", "星期五", "星期六", "星期日" };
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
                return string.Format("{0} {1}", distance, "秒前");
            }
            else if (distance >= 60 && distance < 59 * 60)
            {
                return string.Format("{0} {1}", Math.Floor(distance / 60.0), "分鐘前");
            }
            else if (distance >= 60 * 60 && distance < 60 * 60 * 24)
            {
                return string.Format("{0} {1}", Math.Floor(distance / 60.0 / 60.0), "小時前");
            }
            else if (distance >= 60 * 60 * 24 && distance < 59 * 60 * 24 * 7)
            {
                return string.Format("{0} {1}", week_array[Convert.ToInt32(date("N", timestamp)) - 1], date("H:i", timestamp));
            }
            else
            {
                return string.Format("{0}", date("Y/m/d H:i", timestamp));
            }

        }
        public string str_replace(string r, string t, string data)
        {
            return data.Replace(r, t);
        }
        public string print_table(DataTable ra, string fields = null, string headers = null, string classname = null)
        {
            return print_table(datatable2dictinyary(ra), fields, headers, classname);
        }
        public string print_table(List<Dictionary<string, string>> ra, string fields = null, string headers = null, string classname = null)
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
                foreach (string kc in ra[0].Keys)
                {
                    string k = kc.ToString();
                    string v = strip_tags(k);
                    v = trim(v);
                    tmp += "<th field='" + v + "'>" + k + "</th>";
                }
                tmp += "</tr></thead>";
                tmp += "<tbody>";
                for (int i = 0, max_i = count(ra); i < max_i; i++)
                {
                    tmp += "<tr>";
                    foreach (string kc in ra[0].Keys)
                    {
                        string k = kc.ToString();
                        string kk = trim(k);
                        string v = ra[i][kk].ToString();
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
                        tmp += "<td field='" + k + "'>" + ra[i][kk].ToString() + "</td>";
                    }
                    tmp += "</tr>";
                }
                tmp += "</tbody>";
                tmp += "</table>";
                return tmp;
            }
        }
        public string print_csv(DataTable ra, string fields = "", string headers = "", bool is_need_header = true)
        {
            string tmp = "";
            if (fields == "" || fields == "*")
            {
                tmp = "";
                var keys = new List<string>();
                foreach (DataColumn kc in ra.Columns)
                {
                    string k = kc.ToString();
                    keys.Add(k);
                }

                if (is_need_header)
                {
                    tmp += "\"" + implode("\",\"", keys) + "\"\r\n";
                }
                for (int i = 0, max_i = ra.Rows.Count; i < max_i; i++)
                {
                    var d = new List<string>();
                    foreach (string k in keys)
                    {
                        string v = ra.Rows[i][k].ToString();
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
                for (int i = 0, max_i = ra.Rows.Count; i < max_i; i++)
                {
                    var d = new List<string>();
                    foreach (string k in m_fields)
                    {

                        string v = str_replace("\n", " ", ra.Rows[i][k].ToString());
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
        public string strip_tags(string Txt)
        {
            return Regex.Replace(Txt, "<(.|\\n)*?>", string.Empty);
        }
        public bool linkToDB()
        {
            try
            {
                if (!isLinkToDB())
                {
                    string connString = System.Configuration.ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                    //connString = System.Configuration.ConfigurationManager.AppSettings["DBConnection"];
                    //connString = ConfigurationManager.ConnectionStrings["DBConnection"].ToString();
                    PDO = new GFLib.Database.MsSql(connString);
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                return false;
            }
            return true;
        }
        public int rand(int min, int max)
        {
            return rnd.Next(min, max);
        }
        public string pwd()
        {
            //return dirname(System.Web.HttpContext.Current.Request.PhysicalPath);
            return dirname(System.Web.HttpContext.Current.Server.MapPath("~/"));
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
        public bool is_string_like(string data, string find_string)
        {
            return (data.IndexOf(find_string) == -1) ? false : true;
        }
        public bool is_istring_like(string data, string find_string)
        {
            return (data.ToUpper().IndexOf(find_string.ToUpper()) == -1) ? false : true;
        }
        public void include(string filename)
        {
            echo(b2s(file_get_contents(filename)));
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
        string BIG5toUTF8(string strUtf)
        {
            Encoding utf81 = Encoding.GetEncoding("utf-8");
            Encoding big51 = Encoding.GetEncoding("big5");
            System.Web.HttpContext.Current.Response.ContentEncoding = utf81;
            byte[] strBig51 = big51.GetBytes(strUtf.Trim());
            byte[] strUtf81 = Encoding.Convert(big51, utf81, strBig51);

            char[] utf8Chars1 = new char[utf81.GetCharCount(strUtf81, 0, strUtf81.Length)];
            utf81.GetChars(strUtf81, 0, strUtf81.Length, utf8Chars1, 0);
            string tempString1 = new string(utf8Chars1);
            return tempString1;
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
        public string urlencode(string value)
        {
            return Server.UrlEncode(value);
        }
        public string urldecode(string value)
        {
            return Server.UrlDecode(value);
        }
        public string addslashes(string value)
        {
            return value.Replace("'", "\'").Replace("\"", "\\\"");

        }
        public string stripslashes(string value)
        {
            return value.Replace("\\'", "'").Replace("\\\"", "\"");
        }
        public void alert(string value)
        {
            value = jsAddSlashes(value);
            echo("<script language='javascript'>alert('" + value + "');</script>");
        }
        public void alert(int value)
        {
            alert(value.ToString());
        }
        public void alert(double value)
        {
            alert(value.ToString());
        }
        public void echo(double value)
        {
            echo(value.ToString());
        }
        public void echo(long value)
        {
            echo(value.ToString());
        }
        public void echo(int value)
        {
            echo(value.ToString());
        }
        public void echo(byte[] value)
        {
            echo(value.ToString());
        }
        public void echo(string value)
        {
            //System.Web.HttpContext.Current.Response.Write(value);
            HttpContext.Current.Response.Write(value);
        }
        public void echoBinary(string value)
        {
            HttpContext.Current.Response.BinaryWrite(s2b(value));
        }
        public void echoBinary(byte[] value)
        {
            HttpContext.Current.Response.BinaryWrite(value);
        }
        public void downloadHeader(string filename)
        {
            //HttpContext.Current.Response.Headers.Clear();
            HttpContext.Current.Response.AppendHeader("ContentType", "application/octet-stream");
            HttpContext.Current.Response.AppendHeader("Content-Disposition", "attachment;filename=\"" + filename + "\"");
        }

        public void allowAjaxHeader()
        {
            HttpContext.Current.Response.AppendHeader("Access-Control-Allow-Origin", "*");
            HttpContext.Current.Response.AppendHeader("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
            if (HttpContext.Current.Request.HttpMethod == "OPTIONS")
            {
                //These headers are handling the "pre-flight" OPTIONS call sent by the browser
                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE");
                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept");
                HttpContext.Current.Response.AddHeader("Access-Control-Max-Age", "1728000");
                HttpContext.Current.Response.End();
            }
        }
        public void setOutputHeader(string data)
        {
            // text/xml
            // text/plain
            // ...
            HttpContext.Current.Response.AppendHeader("Content-Type", data);
        }
        //取得使用者IP
        public string ip()
        {
            string ip = "";
            if (System.Web.HttpContext.Current.Request.ServerVariables["HTTP_VIA"] != null)
            {
                if (System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] == null)
                {
                    if (System.Web.HttpContext.Current.Request.ServerVariables["HTTP_CLIENT_IP"] != null)
                    {
                        ip = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_CLIENT_IP"].ToString();
                    }
                    else
                    {
                        if (System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"] != null)
                        {
                            ip = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"].ToString();
                        }
                    }
                }
                else
                {
                    ip = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString();
                }
            }
            else if (System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"] != null)
            {
                ip = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"].ToString();
            }
            return ip;
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
                case "Y/m/d H:i":
                    return tmp.ToString("yyyy/MM/dd HH:mm");
                case "Y/m/d H:i:s.fff":
                    return tmp.ToString("yyyy/MM/dd HH:mm:ss.fff");
                case "Y-m-d_H_i_s":
                    return tmp.ToString("yyyy-MM-dd_HH_mm_ss");
                case "Y-m-d":
                    return tmp.ToString("yyyy-MM-dd");
                case "Ymd":
                    return tmp.ToString("yyyyMMdd");
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
            if (value == "") return "0";
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
        public string htmlspecialchars(string input)
        {
            return HttpContext.Current.Server.HtmlEncode(input);
        }
        public string htmlspecialchars_decode(string input)
        {
            return HttpContext.Current.Server.HtmlDecode(input);
        }
        public Dictionary<string, object> getGET_POST(string inputs, string method)
        {
            /*
                string GETS_STRING="mode,id";
                Dictionary<string, object> get = x.getGET_POST(GETS_STRING, "GET");
                string vv=x.print_r(get,0);
                foreach (string a in get.Keys)
                {
                    Response.Write(a+":"+get[a]+"<br>");
                }
               sample:
                string GETS_STRING="mode,id";
                Dictionary<string, object> get = x.getGET_POST(GETS_STRING, "GET");

                string POSTS_STRING ="abc,b,s_a,s_b[],ddd";

                Dictionary<string, object> post = x.getGET_POST(POSTS_STRING, "POST");
                string q = x.print_r(get, 0);
                string p = x.print_r(post, 0);
                Response.Write("<pre>" + q + "<br>" + p + "</pre>");
                Response.Write("aaaaaaa->" + post["s_a"]+"<br>");
                Response.Write("aaaaaab->" + post["s_b[]"] + "<br>");             
             * 
            */
            method = method.ToUpper();
            Dictionary<string, object> get_post = new Dictionary<string, object>();
            switch (method)
            {
                case "GET":
                    foreach (string k in inputs.Split(','))
                    {
                        if (this.Context.Request.QueryString[k] != null)
                        {
                            get_post[k] = this.Context.Request.QueryString[k];
                        }
                        else
                        {
                            get_post[k] = "";
                        }

                    }
                    break;
                case "POST":
                    foreach (string k in inputs.Split(','))
                    {
                        if (this.Context.Request.Form[k] != null)
                        {
                            if (this.Context.Request.Form.GetValues(k).Length != 1)
                            {
                                //暫時先這樣，以後再修= =
                                //alert(this.Context.Request.Form.GetValues(k).Length.ToString());
                                get_post[k] = implode("┃", this.Context.Request.Form.GetValues(k));
                            }
                            else
                            {
                                get_post[k] = this.Context.Request.Form[k];
                            }
                        }
                        else
                        {
                            get_post[k] = "";
                        }
                    }
                    break;
            }
            return get_post;
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
        public byte[] WMS_file_get_contents(string url)
        {
            if (url.ToLower().IndexOf("http:") > -1)
            {
                // URL                 
                System.Net.WebRequest.DefaultWebProxy = null;
                HttpWebRequest hwr = (HttpWebRequest)(WebRequest.Create(url));

                hwr.Method = "GET";
                hwr.Timeout = 50000;
                hwr.Proxy = null;

                HttpWebResponse hwro = (HttpWebResponse)(hwr.GetResponse());
                System.Web.HttpContext.Current.Response.ContentType = hwro.ContentType;
                using (var memoryStream = new MemoryStream())
                {
                    byte[] buffer = new byte[16 * 1024]; // Fairly arbitrary size
                    int bytesRead;

                    while ((bytesRead = hwro.GetResponseStream().Read(buffer, 0, buffer.Length)) > 0)
                    {
                        memoryStream.Write(buffer, 0, bytesRead);
                    }
                    hwro.Close();
                    return memoryStream.ToArray();
                }
            }
            else
            {
                System.IO.StreamReader sr = new System.IO.StreamReader(url);
                string sContents = sr.ReadToEnd();
                sr.Close();
                return s2b(sContents);
            }
        }
        /*public long insertSQL(string table, Dictionary<string, string> datas, bool isNeedLastID = false)
        {
            string SQL = "";
            string[] field = new string[datas.Keys.Count];
            string[] data = new string[datas.Keys.Count];
            string[] question_marks = new string[datas.Keys.Count];
            int i = 0;
            foreach (string k in datas.Keys)
            {
                field[i] = k;
                data[i] = datas[k].ToString();
                question_marks[i] = "@" + k;
                i++;
            }
            SQL = string.Format(@"
                    INSERT INTO {0}({1})VALUES({2});",
                                                           table,
                                                           implode(",", field),
                                                           implode(",", question_marks));

            SqlCommand cmd = new SqlCommand(SQL, conn);
            List<SqlParameter> param_now = new List<SqlParameter>();
            param_now.Clear();
            for (i = 0; i < data.Length; i++)
            {
                cmd.Parameters.AddWithValue(question_marks[i], data[i]);
            }
            cmd.ExecuteNonQuery();

            if (isNeedLastID)
            {
                Dictionary<int, Dictionary<string, string>> ro = selectSQL("SELECT @@IDENTITY AS LastID");
                int last_id = Convert.ToInt32(ro[0]["LastID"]);
                return last_id;
            }
            else
            {
                return -1;
            }
        }
        public void updateSQL(string table, Dictionary<string, string> datas, string WHERESQL)
        {
            string SQL = "";
            string[] SQL_DATA = new string[datas.Keys.Count];
            string[] field = new string[datas.Keys.Count];
            string[] data = new string[datas.Keys.Count];
            string[] question_marks = new string[datas.Keys.Count];
            int i = 0;
            foreach (string k in datas.Keys)
            {
                field[i] = k;
                data[i] = datas[k].ToString();
                question_marks[i] = "@" + k;
                SQL_DATA[i] = String.Format(@" ""{0}"" = {1} ", field[i], question_marks[i]);
                i++;

            }
            SQL = string.Format(@"
                    UPDATE ""{0}"" SET  {1} WHERE {2} ;",
                                                           table,
                                                           implode(",", SQL_DATA),
                                                           WHERESQL);
            SqlCommand cmd = new SqlCommand(SQL, conn);

            List<SqlParameter> param_now = new List<SqlParameter>();
            param_now.Clear();
            for (i = 0; i < data.Length; i++)
            {
                cmd.Parameters.AddWithValue(question_marks[i], data[i]);
            }

            cmd.ExecuteNonQuery();
        }
        */
        public void closeDB()
        {
            if (PDO != null)
            {
                PDO.Dispose();
                PDO = null;
            }
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

        public void location_href(string value)
        {
            echo("<script language='javascript'>location.href=\"" + value + "\";</script>");
        }
        public void location_replace(string value)
        {
            echo("<script language='javascript'>location.replace(\"" + value + "\");</script>");
        }
        public void history_go()
        {
            echo("<script language='javascript'>history.go(-1);</script>");
        }
        public void history_back()
        {
            echo("<script language='javascript'>history.back();</script>");
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
        public void exit()
        {
            System.Web.HttpContext.Current.Response.Flush(); //強制輸出緩衝區資料
            System.Web.HttpContext.Current.Response.Clear(); //清除緩衝區的資料
            System.Web.HttpContext.Current.Response.End(); //結束資料輸出
                                                           //System.Web.HttpContext.Current.Response.StatusCode = 200;
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
        public string basedir()
        {
            //取得專案的起始位置
            return System.Web.HttpContext.Current.Request.PhysicalApplicationPath;
        }
        /// <summary>    
        /// 利用外部指令，將 SHP 轉換坐標系統    
        /// </summary>    
        /// <param name="shp_source">來源 SHP 完整路徑</param>            
        public bool shx_generator(string shp_source)
        {
            try
            {
                string SHX_FIXER = string.Format(@"{0}\\lib\\shpfile_fixer\\shapechk.exe", basedir());
                string SHP_TMP_DIR = string.Format(@"{0}\\{1}", getSystemKey("SHP_TMP_DIR"), date("Y-m-d"));
                string WORK_DISK = explode(":", SHP_TMP_DIR)[0];

                string cmd = string.Format(@"{0}: && cd {1} && {2} {3} /auto ",
                                        WORK_DISK,
                                        SHP_TMP_DIR,
                                        SHX_FIXER, shp_source
                );

                system(cmd);
                if (File.Exists(string.Format(@"{0}\\{1}.shx", SHP_TMP_DIR, mainname(shp_source))))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                return false;
            }
        }
        /// <summary>    
        /// 利用外部指令，將 SHP 轉換坐標系統    
        /// </summary>    
        /// <param name="shp_source">來源 SHP 完整路徑</param>    
        /// <param name="shp_output">目地 SHP 完整路徑</param>            
        /// <param name="s_srs">來源 SHP 坐標系統</param>            
        /// <param name="t_srs">目地 SHP 坐標系統</param>            
        /// <returns>true or false</returns>    
        public bool shp_projection_change(string shp_source, string shp_output, string s_srs, string t_srs)
        {
            try
            {
                string SHP_TMP_DIR = string.Format(@"{0}\\{1}", getSystemKey("SHP_TMP_DIR"), date("Y-m-d"));
                string WORK_DISK = explode(":", SHP_TMP_DIR)[0];
                string OGR2OGR_PATH = string.Format(@"{0}\\lib\\GDAL\\ogr2ogr.exe", basedir());
                string cmd = string.Format(@"{0}: && cd {1} && {2} -s_srs ""{3}"" -t_srs ""{4}"" ""{5}"" ""{6}""  ",
                                                    WORK_DISK,
                                                    SHP_TMP_DIR,
                                                    OGR2OGR_PATH, s_srs, t_srs, shp_output, shp_source
                );
                system(cmd);

                if (File.Exists(string.Format(@"{0}\\{1}", SHP_TMP_DIR, shp_output)))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                return false;
            }
        }
        /// <summary>    
        /// 利用外部指令，將 SHP 轉成 JSON    
        /// </summary>    
        /// <param name="shp_source">來源 SHP 完整路徑</param>    
        /// <param name="json_output">目地 JSON 完整路徑</param>            
        /// <returns>true or false</returns>    
        public bool shp_to_json(string shp_source, string json_output)
        {
            try
            {
                string SHP_TMP_DIR = string.Format(@"{0}\\{1}", getSystemKey("SHP_TMP_DIR"), date("Y-m-d"));
                string WORK_DISK = explode(":", SHP_TMP_DIR)[0];
                string GDAL_PATH = string.Format(@"{0}\\lib\\GDAL\\ogr2ogr.exe", basedir());

                string cmd = string.Format(@"{0}: && cd {1} && {2} -f GeoJSON ""{3}"" ""{4}"" ",
                                                WORK_DISK,
                                                SHP_TMP_DIR,
                                                GDAL_PATH, json_output, shp_source);
                //file_put_contents("c:\\LOGS\\NGIS\\a.txt", cmd);
                system(cmd);

                if (File.Exists(string.Format(@"{0}\\{1}", SHP_TMP_DIR, json_output)))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                //file_put_contents("c:\\LOGS\\NGIS\\b.txt", cmd+"\n"+ex.Message);
                return false;
            }
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
            long microseconds = span.Ticks / (TimeSpan.TicksPerMillisecond / 1000);
            return microseconds.ToString();
        }

        public ConcurrentDictionary<string, object> curl_getPost_INIT(string URL, ConcurrentDictionary<string, string> posts, ConcurrentDictionary<string, object> options = null)
        {
            //NameValueCollection postParameters = new NameValueCollection();
            List<string> mPostData = new List<string>();
            int step = 0;
            List<Dictionary<string, string>> upfiles = new List<Dictionary<string, string>>();
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
            /*int limit = 2000;

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
            */
            return Uri.EscapeDataString(value);
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
            foreach (string k in posts.Keys)
            {
                string d = post_encode_string(k) + "=" + post_encode_string(posts[k]);
                mPostData.Add(d);
            }
            List<Dictionary<string, string>> mfiles = new List<Dictionary<string, string>>();
            foreach (string k in files.Keys)
            {
                Dictionary<string, string> d = new Dictionary<string, string>();
                d[post_encode_string(k)] = post_encode_string(files[k]);
                mfiles.Add(d);
            }
            return curl_getPost_INIT(URL, implode("&", mPostData), mfiles, options);
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
                output["realCookie"] = response.Headers[HttpResponseHeader.SetCookie];
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
        private void PaintInterLine(Graphics g, int num, int width, int height)
        {
            Random r = new Random();
            int startX, startY, endX, endY;
            for (int i = 0; i < num; i++)
            {
                startX = r.Next(0, width);
                startY = r.Next(0, height);
                endX = r.Next(0, width);
                endY = r.Next(0, height);
                g.DrawLine(new Pen(Brushes.Red), startX, startY, endX, endY);
            }
        }
        public void checkpassword()
        {
            if (isLogin() == false)
            {
                alert("請先登入系統...");
                location_href(base_url + "/login.aspx");
                exit();
            }
        }
        public int count(List<Dictionary<string, string>> ra)
        {
            return ra.Count;
        }
        public int count(DataTable ra)
        {
            return ra.Rows.Count;
        }
        public void gd_show(string key, string id)
        {
            // From https://ithelp.ithome.com.tw/articles/10208039
            Session["GD_CODE" + id] = key;
            byte[] data = null;
            string code = key;
            //定義一個畫板
            MemoryStream ms = new MemoryStream();
            using (Bitmap map = new Bitmap(100, 40))
            {
                //畫筆,在指定畫板畫板上畫圖
                //g.Dispose();
                using (Graphics g = Graphics.FromImage(map))
                {
                    g.Clear(Color.White);
                    g.DrawString(code, new Font("黑體", 18.0F), Brushes.Blue, new Point(10, 8));
                    //繪製干擾線(數字代表幾條)
                    PaintInterLine(g, 10, map.Width, map.Height);
                }
                map.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            data = ms.GetBuffer();
            setOutputHeader("image/png");
            echoBinary(data);
        }
        public int strlen(string data)
        {
            return data.Length;
        }
        private string getSystemInfo(JArray jd, string key)
        {
            //foreach (var kk in jd.)
            for (int k = 0, max_k = jd.Count; k < max_k; k++)
            {
                var v = jd[k];
                if (v["systemName"] != null && v["systemName"].ToString() == key)
                {
                    return v["systemData"].ToString();
                }
            }
            return null;
        }
        public void insertSYSTEM_INFO(JArray jd, string INDEX, string computers_id)
        {
            Dictionary<string, string> m = new Dictionary<string, string>();
            m["computers_id"] = computers_id;
            m["INDEX"] = INDEX;
            m["datetime"] = date("Y-m-d H:i:s");
            m["CPUID"] = getSystemInfo(jd, "CPUID");
            m["CPU規格"] = getSystemInfo(jd, "CPU規格");
            m["CPU核心數"] = getSystemInfo(jd, "CPU核心數");
            m["RAM大小"] = getSystemInfo(jd, "RAM大小");
            m["位元"] = getSystemInfo(jd, "位元");
            m["作業系統"] = getSystemInfo(jd, "作業系統");
            m["IP_1"] = getSystemInfo(jd, "IP_1");
            m["IP_2"] = @getSystemInfo(jd, "IP_2");
            m["IP_3"] = @getSystemInfo(jd, "IP_3");
            m["Gateway"] = getSystemInfo(jd, "Gateway");
            m["UsedRam"] = getSystemInfo(jd, "UsedRam");
            m["UsedCPU"] = getSystemInfo(jd, "UsedCPU");
            m["回報系統名稱"] = getSystemInfo(jd, "回報系統名稱");
            m["電腦名稱"] = getSystemInfo(jd, "電腦名稱");
            m["使用者名稱"] = getSystemInfo(jd, "使用者名稱");
            m["WindowsUpdateDate"] = getSystemInfo(jd, "WindowsUpdateDate");
            m["WindowDefender_AMProductVersion"] = getSystemInfo(jd, "WindowDefender_AMProductVersion");
            m["WindowDefender_AMEngineVersion"] = getSystemInfo(jd, "WindowDefender_AMEngineVersion");
            m["WindowDefender_AntispywareSignatureVersion"] = getSystemInfo(jd, "WindowDefender_AntispywareSignatureVersion");
            m["WindowDefender_AntivirusSignatureVersion"] = getSystemInfo(jd, "WindowDefender_AntivirusSignatureVersion");
            m["Framework版本"] = getSystemInfo(jd, "Framework版本");
            m["ping8888"] = getSystemInfo(jd, "ping8888");
            m["外網IP"] = ip();
            m["網域名稱"] = getSystemInfo(jd, "網域名稱");
            m["baseboard"] = getSystemInfo(jd, "BASEBOARD");
            m["TOOL_VERSION"] = getSystemInfo(jd, "TOOL_VERSION");
            insertSQL("system_info_log", m);
        }
        public void insertEVENTS_INFO(JArray jd, string INDEX, string computers_id)
        {
            //寫入事件 
            //事件比較特別，已寫過的不要寫了
            string SQL = @"
              SELECT
                TOP 300
                [events_index]
              FROM
                [events_log]
              WHERE
                1 = 1
                AND [computers_id]=@computers_id
              ORDER BY
                [id] DESC              
            ";
            var PA = new Dictionary<string, string>();
            PA["computers_id"] = computers_id;
            var _ra = selectSQL_SAFE(SQL, PA);
            var FILTER_M = new Dictionary<string, string>();
            for (int k = 0, max_k = _ra.Rows.Count; k < max_k; k++)
            {
                var v = _ra.Rows[k];
                FILTER_M[v["events_index"].ToString()] = "";
            }

            for (int k = 0, max_k = jd.Count; k < max_k; k++)
            {
                var v = jd[k];
                if (FILTER_M.ContainsKey((v["eventsIndex"].ToString()))) continue;
                Dictionary<string, string> m = new Dictionary<string, string>();
                m["computers_id"] = computers_id;
                m["INDEX"] = INDEX;
                m["datetime"] = date("Y-m-d H:i:s");
                m["events_kind"] = v["eventsKind"].ToString(); //事件類型
                m["events_datetime"] = v["eventsDateTime"].ToString();
                m["events_index"] = v["eventsIndex"].ToString();
                m["events_category"] = v["eventsCategory"].ToString();
                m["events_message"] = v["eventsMessage"].ToString();
                insertSQL("events_log", m);
            }
        }
        public void insertFIREWALL_INFO(JArray jd, string INDEX, string computers_id)
        {
            //寫入防火牆
            for (int k = 0, max_k = jd.Count; k < max_k; k++)
            {
                var v = jd[k];
                Dictionary<string, string> m = new Dictionary<string, string>();
                m["computers_id"] = computers_id;
                m["INDEX"] = INDEX;
                m["datetime"] = date("Y-m-d H:i:s");
                m["name"] = v["firewallName"].ToString();
                m["ApplicationName"] = v["firewallApplicationName"].ToString();
                m["ServiceName"] = v["firewallServiceName"].ToString();
                m["Enabled"] = v["firewallEnabled"].ToString();
                m["Protocol"] = v["firewallProtocol"].ToString();
                m["AllowDeny"] = v["firewallAllowBlock"].ToString();
                m["InOut"] = v["firewallDirectionInOut"].ToString();
                m["LocalPorts"] = v["firewallLocalPorts"].ToString();
                m["RemotePorts"] = v["firewallRemotePorts"].ToString();
                m["LocalAddresses"] = v["firewallLocalAddresses"].ToString();
                m["RemoteAddresses"] = v["firewallRemoteAddresses"].ToString();

                insertSQL("firewall_log", m);
            }
        }
        public void insertHDD_INFO(JArray jd, string INDEX, string computers_id)
        {
            //寫入硬碟 
            for (int k = 0, max_k = jd.Count; k < max_k; k++)
            {
                var v = jd[k];
                Dictionary<string, string> m = new Dictionary<string, string>();
                m["computers_id"] = computers_id;
                m["INDEX"] = INDEX;
                m["datetime"] = date("Y-m-d H:i:s");
                m["disk"] = v["hddID"].ToString();
                m["total_space"] = v["hddTotalSpace"].ToString();
                m["free_space"] = v["hddFreeSpace"].ToString();
                m["brand"] = v["hddModel"].ToString();
                m["use_hour"] = v["hddUsageHour"].ToString();
                m["temperature"] = v["hddTemperature"].ToString();
                m["bad_track"] = v["hddBadSectors"].ToString();
                m["boot_times"] = v["hddOnOffTimes"].ToString();
                m["file_system"] = v["hddFormatType"].ToString();

                insertSQL("hdd_log", m);
            }
        }
        public void insertSCHEDULE_INFO(JArray jd, string INDEX, string computers_id)
        {
            //寫入排程
            for (int k = 0, max_k = jd.Count; k < max_k; k++)
            {
                var v = jd[k];
                Dictionary<string, string> m = new Dictionary<string, string>();
                m["computers_id"] = computers_id;
                m["INDEX"] = INDEX;
                m["datetime"] = date("Y-m-d H:i:s");
                m["name"] = v["scheduleName"].ToString();
                m["is_runable"] = v["scheduleEnable"].ToString();
                m["cmd"] = v["schedulePath"].ToString();
                m["prev_run_datetime"] = v["schedulePrevDateTime"].ToString();
                if (v["scheduleNextDateTime"].ToString() != "--")
                {
                    m["next_run_datetime"] = v["scheduleNextDateTime"].ToString();
                }
                try
                {
                    insertSQL("schedule_log", m);
                }
                catch (Exception ex)
                {
                    myLog("insert schedule_log error...：\r\n" + json_format_utf8(json_encode(m)));
                }
            }
        }
        public void insertSYSTEM_SERVICE_INFO(JArray jd, string INDEX, string computers_id)
        {
            //服務
            for (int k = 0, max_k = jd.Count; k < max_k; k++)
            {
                var v = jd[k];
                Dictionary<string, string> m = new Dictionary<string, string>();
                m["computers_id"] = computers_id;
                m["INDEX"] = INDEX;
                m["datetime"] = date("Y-m-d H:i:s");
                m["name"] = v["system_serviceName"].ToString();
                m["contents"] = v["system_serviceDescription"].ToString();
                m["status"] = v["system_serviceStatus"].ToString();
                m["start_kind"] = v["system_serviceStartupStatus"].ToString();
                m["cmd"] = v["system_servicePathName"].ToString();
                insertSQL("service_log", m);
            }
        }
        public void insertTASK_INFO(JArray jd, string INDEX, string computers_id)
        {
            //執行緒
            for (int k = 0, max_k = jd.Count; k < max_k; k++)
            {
                var v = jd[k];
                Dictionary<string, string> m = new Dictionary<string, string>();
                m["computers_id"] = computers_id;
                m["INDEX"] = INDEX;
                m["datetime"] = date("Y-m-d H:i:s");
                m["pid"] = v["running_programPID"].ToString();
                m["name"] = v["running_programName"].ToString();
                m["basename"] = v["running_programBaseName"].ToString();
                m["fullpathname"] = v["running_programPath"].ToString();
                m["run_way"] = v["running_programCommandLine"].ToString();
                m["start_datetime"] = v["running_programStart_datetime"].ToString();
                if (m["start_datetime"] == "")
                {
                    //unset(m["start_datetime']);
                    m.Remove("start_datetime");
                }
                m["run_times"] = v["running_programRun_times"].ToString();
                if (m["run_times"] == "")
                {
                    //unset(m["run_times]);
                    m.Remove("run_times");
                }
                m["run_user"] = v["running_programRun_user"].ToString();
                insertSQL("task_log", m);
            }
        }
        public void insertIIS_INFO(JArray jd, string INDEX, string computers_id)
        {
            //執行緒
            for (int k = 0, max_k = jd.Count; k < max_k; k++)
            {
                var v = jd[k];
                Dictionary<string, string> m = new Dictionary<string, string>();
                m["computers_id"] = computers_id;
                m["INDEX"] = INDEX;
                m["datetime"] = date("Y-m-d H:i:s");
                m["site_name"] = v["iis_site_name"].ToString();
                m["PhysicalPath"] = v["iis_PhysicalPath"].ToString();
                m["Path"] = v["iis_Path"].ToString();
                m["IsWebconfigEncrypt"] = v["iis_IsWebconfigEncrypt"].ToString();
                m["customErrors"] = v["iis_customErrors"].ToString();
                m["sessionTimeout"] = v["iis_sessionTimeout"].ToString();
                m["mimeMap"] = v["iis_mimeMap"].ToString();
                m["defaultDocument"] = v["iis_defaultDocument"].ToString();
                m["ApplicationPoolName"] = v["iis_ApplicationPoolName"].ToString();
                insertSQL("iis_log", m);
            }
        }
        public void insertINSTALLED_SOFTWARE_INFO(JArray jd, string INDEX, string computers_id)
        {
            //已安裝的程式
            for (int k = 0, max_k = jd.Count; k < max_k; k++)
            {
                var v = jd[k];
                Dictionary<string, string> m = new Dictionary<string, string>();
                m["computers_id"] = computers_id;
                m["INDEX"] = INDEX;
                m["datetime"] = date("Y-m-d H:i:s");
                m["DisplayName"] = v["installed_software_DisplayName"].ToString();
                m["OSBit"] = v["installed_software_OSBit"].ToString();
                m["DisplayVersion"] = v["installed_software_DisplayVersion"].ToString();
                if (m["DisplayVersion"] == "")
                {
                    //unset(m["DisplayVersion"]);
                    m.Remove("DisplayVersion");
                }
                m["Publisher"] = v["installed_software_Publisher"].ToString();
                if (m["Publisher"] == "")
                {
                    //unset(m["Publisher']);
                    m.Remove("Publisher");
                }
                m["InstallDate"] = v["installed_software_InstallDate"].ToString();
                if (m["InstallDate"] == "")
                {
                    //unset(m["InstallDate']);
                    m.Remove("InstallDate");
                }

                m["InstallLocation"] = v["installed_software_InstallLocation"].ToString();
                if (m["InstallLocation"] == "")
                {
                    //unset(m["InstallLocation']);
                    m.Remove("InstallLocation");
                }
                //echoBinary(json_encode(m));
                //exit();
                insertSQL("installed_software_log", m);
            }
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
        public void sendMail(List<string> To, string Subject, string Body, List<string> upFile = null)
        {
            string SMTP_USERNAME = getArgument("smtp_username");
            string SMTP_PD = getArgument("smtp_password");
            string SMTP_FROMNAME = getArgument("smtp_fromname");
            GMailReposity G = new GMailReposity(SMTP_USERNAME, SMTP_PD);
            if (upFile == null)
            {
                G.SendMail(SMTP_FROMNAME, To, Subject, Body);
            }
            else
            {
                G.SendMail(SMTP_FROMNAME, To, Subject, Body, upFile);
            }
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
        public bool computerUpdateStatus(JArray jd, int report_size)
        {

            string LAST_ID = "";
            string INDEX = "0";

            string SQL = @"
                    SELECT
                        TOP 1
                      [id],
                      [del],
                      [INDEX]
                    FROM
                      [computers]
                    WHERE
                      1 = 1
                      AND [name]=@name        
                  ";
            var PA = new Dictionary<string, string>();
            PA["name"] = jd[0]["NAME"].ToString();
            var ra = selectSQL_SAFE(SQL, PA);
            var m = new Dictionary<string, string>();
            m["name"] = jd[0]["NAME"].ToString();
            m["last_report_datetime"] = date("Y-m-d H:i:s");

            m["report_size"] = report_size.ToString();


            if (count(ra) == 0)
            {
                m["INDEX"] = "0";
                LAST_ID = insertSQL("computers", m).ToString();
                INDEX = "0";
            }
            else
            {
                if (ra.Rows[0]["del"].ToString() != "0")
                {
                    closeDB();
                    return false;// my.exit();
                }
                LAST_ID = ra.Rows[0]["id"].ToString();
                INDEX = (Convert.ToInt32(ra.Rows[0]["INDEX"].ToString()) + 1).ToString();
                var mpa = new Dictionary<string, string>();
                mpa["id"] = LAST_ID;
                updateSQL_SAFE("computers", m, "[id]=@id", mpa);
            }
            //寫入資料
            //補充資料
            //if(!((JObject)jd[0]["SYSTEM_INFO"]).ContainsKey("外網IP"))
            //{
            //    ((JObject)jd[0]["SYSTEM_INFO"]).Add("外網IP", ip());
            //}
            //jd[0]["SYSTEM_INFO"]["外網IP"] = ip();


            insertSYSTEM_INFO((JArray)jd[0]["SYSTEM_INFO"], INDEX, LAST_ID);
            insertEVENTS_INFO((JArray)jd[0]["EVENTS_INFO"], INDEX, LAST_ID);
            insertFIREWALL_INFO((JArray)jd[0]["FIREWALL_INFO"], INDEX, LAST_ID);
            insertHDD_INFO((JArray)jd[0]["HDD_INFO"], INDEX, LAST_ID);
            insertSCHEDULE_INFO((JArray)jd[0]["SCHEDULE_INFO"], INDEX, LAST_ID);
            insertSYSTEM_SERVICE_INFO((JArray)jd[0]["SYSTEM_SERVICE_INFO"], INDEX, LAST_ID);
            insertTASK_INFO((JArray)jd[0]["TASK_INFO"], INDEX, LAST_ID);
            insertIIS_INFO((JArray)jd[0]["IIS_INFO"], INDEX, LAST_ID);
            insertINSTALLED_SOFTWARE_INFO((JArray)jd[0]["INSTALLED_SOFTWARE_INFO"], INDEX, LAST_ID);

            var mm = new Dictionary<string, string>();
            mm["id"] = LAST_ID;
            m["INDEX"] = INDEX; //update computers when finish
            updateSQL_SAFE("computers", m, "[id]=@id", mm);
            return true;
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