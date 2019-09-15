using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ZLCHSLisComm
{
    public class Write_Log
    {
        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="log"></param>
        /// <param name="type"></param>
        public void Write(string DeviceName,string log, string type)
        {
            string path;
            DateTime sysdate;
            StreamWriter sw;
            //path = System.Windows.Forms.Application.StartupPath;
            //path = System.Reflection.Assembly.GetExecutingAssembly().Location.ToString();
            path = Directory.GetCurrentDirectory();
            sysdate = System.DateTime.Now;

            if (type == "raw")
            {
                path = path + "\\" + DeviceName + "\\raw";
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            }
            else if (type == "result")
            {
                path = path + "\\" + DeviceName + "\\result";
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            }
            else if (type == "log")
            {
                path = path + "\\" + DeviceName + "\\log";
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            }
            path = path + "\\" + sysdate.ToString("yyyyMMdd") + ".txt";
            if (!File.Exists(path)) //如果文件存在,则创建File.AppendText对象
            {
                sw = File.CreateText(path);
                sw.Close();
            }

            log = sysdate.ToLongTimeString() + "\r\n" + log;
            sw = new StreamWriter(path, true, System.Text.Encoding.Default);
            sw.WriteLine(log);
            sw.Close();
        }

        public void Write( string log, string type)
        {
            string path;
            DateTime sysdate;
            StreamWriter sw;
            path = Directory.GetCurrentDirectory();
            sysdate = System.DateTime.Now;

            if (type == "raw")
            {
                path = path  + "\\raw";
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            }
            else if (type == "result")
            {
                path = path  + "\\result";
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            }
            else if (type == "log")
            {
                path = path + "\\log";
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            }
            path = path + "\\" + sysdate.ToString("yyyyMMdd") + ".txt";
            if (!File.Exists(path)) //如果文件存在,则创建File.AppendText对象
            {
                sw = File.CreateText(path);
                sw.Close();
            }

            log = sysdate.ToLongTimeString() + "\r\n" + log;
            sw = new StreamWriter(path, true, System.Text.Encoding.Default);
            sw.WriteLine(log);
            sw.Close();
        }
    }
}
