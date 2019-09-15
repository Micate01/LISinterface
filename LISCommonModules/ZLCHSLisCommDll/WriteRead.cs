using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ZLCHSLisComm
{
   public class WriteRead
    {
        FileStream fs;
        StreamWriter sw;//写文件
        StreamReader sr;//读文件
        string resultpath;
        string ResultSource = "";
        public string ReadData(string DeviceName)
        {
            resultpath = Directory.GetCurrentDirectory();
            resultpath = resultpath + "\\" + DeviceName + "\\ResultName\\" + System.DateTime.Now.ToString("yyyyMMdd") + ".txt";
            if (File.Exists(resultpath))
            {
                fs = new FileStream(resultpath, FileMode.Open, FileAccess.Read);
                sr = new StreamReader(fs);
                sr.BaseStream.Seek(0, SeekOrigin.Begin);
                ResultSource = sr.ReadLine();
                fs.Flush();
                sr.Close();
                fs.Close();
            }
            return ResultSource;
        }

        public string Read(string DeviceName)
        {
            resultpath = Directory.GetCurrentDirectory();
            resultpath = resultpath + "\\" + DeviceName + "\\ResultName\\" + System.DateTime.Now.ToString("yyyyMMdd") + ".txt";
            if (File.Exists(resultpath))
            {
                sr = new StreamReader(resultpath, System.Text.Encoding.Default, true);
                ResultSource = sr.ReadLine();
                sr.Close();
                File.Delete(resultpath);
            }
            return ResultSource;
        }

        public void Write(string DeviceName, string content)
        {
            string path = Directory.GetCurrentDirectory();
            path = path + "\\" + DeviceName + "\\ResultName";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            path = path + "\\" + System.DateTime.Now.ToString("yyyyMMdd") + ".txt";
            if (!File.Exists(path)) //如果文件存在,则创建File.AppendText对象
            {
                sw = File.CreateText(path);
                sw.Close();
            }
            sw = new StreamWriter(path, true, System.Text.Encoding.Default);
            sw.Flush();
            sw.Write(content);
            sw.Close();
        }
    }
}

