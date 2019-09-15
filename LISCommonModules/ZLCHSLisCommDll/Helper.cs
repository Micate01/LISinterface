using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using System.Windows.Forms;

namespace ZLCHSLisComm
{
  public  class Helper
    {
        //样本编号转换为条码号
          //string returns = Helper.SampleNoToSampleBar(bbh);
          //                  if (!string.IsNullOrEmpty(returns))
          //                  {
          //                      strTestTime = returns.Split('|')[0];
          //                      strSampleNo = returns.Split('|')[1];
          //                      strBarCode = bbh;
          //                  }


        public static void  FileInit(string file)
        {

            if (System.IO.File.Exists(file))
            {

                Console.WriteLine("存在");

            }
            else
            {
                FileStream myFs = new FileStream(file, FileMode.Create);
                StreamWriter mySw = new StreamWriter(myFs);
                string dateTime = DateTime.Now.ToString("yyyy-MM-dd");
                string contentStart = dateTime;
                string contentInit = "0=" + dateTime;
                mySw.WriteLine(contentStart);
                mySw.WriteLine(contentInit);
                mySw.Close();
                myFs.Close();

            }
        }
        /// <summary>
        /// 2017-4-12
        /// 添加一个对比结果的功能
        /// </summary>
        /// <param name="cmdStr"></param>
        /// <returns>如果需要再次写入就是true ,否则是false</returns>
        public static bool CompareSampleNoAndTime(string file, string strSampleNo, string strTestTime,List<string> TestValues)
        {
            string strTestValues = "";
            foreach (string TestValues_item in TestValues)
            {
                strTestValues += TestValues_item+"|";
            }
            FileInit(file);
           
            string dateTime = DateTime.Now.ToString("yyyy-MM-dd");
            StreamReader srTemp = new StreamReader(file, System.Text.Encoding.GetEncoding("UTF-8"));
            string dateTemp = srTemp.ReadLine();
            srTemp.Close();
            if (dateTemp != dateTime)
            {
                File.Delete(file);
                FileInit(file);
            }
            List<string> list = new List<string>();
            StreamReader sr = new StreamReader(file, System.Text.Encoding.GetEncoding("UTF-8"));
            string s = "";
            while ((s = sr.ReadLine()) != null)
            {
                list.Add(s);
            }
            sr.Close();
            string item = strSampleNo + "=" + strTestTime + "|" + strTestValues;
            if (list.Contains(item))
            {
                return true;
            }
            else
            {
                FileStream myFs = new FileStream(file, FileMode.Append);
                StreamWriter mySw = new StreamWriter(myFs);
                mySw.WriteLine(item);
                mySw.Close();
                myFs.Close();
                return false;
            }
        }
        public static bool CompareSampleNoAndTime(string file, string strSampleNo, string strTestTime)
        {
            FileInit(file);
            string dateTime = DateTime.Now.ToString("yyyy-MM-dd");
            StreamReader srTemp = new StreamReader(file, System.Text.Encoding.GetEncoding("UTF-8"));
            string dateTemp = srTemp.ReadLine();
            srTemp.Close();
            if (dateTemp != dateTime)
            {
                File.Delete(file);
                FileInit(file);
            }

            List<string> list = new List<string>();
            StreamReader sr = new StreamReader(file, System.Text.Encoding.GetEncoding("UTF-8"));
            string s = "";
            while ((s = sr.ReadLine()) != null)
            {
                list.Add(s);
            }
            sr.Close();
            string item = strSampleNo + "=" + strTestTime;
            if (list.Contains(item))
            {
                return true;
            }
            else
            {
                FileStream myFs = new FileStream(file, FileMode.Append);
                StreamWriter mySw = new StreamWriter(myFs);
                mySw.WriteLine(item);
                mySw.Close();
                myFs.Close();
                return false;
            }

        }
        //添加行号作为区分
        public static bool CompareSampleNoAndTime(string file, string strSampleNo, string strTestTime,int rownum)
        {
            FileInit(file);
            string dateTime = DateTime.Now.ToString("yyyy-MM-dd");
            StreamReader srTemp = new StreamReader(file, System.Text.Encoding.GetEncoding("UTF-8"));
            string dateTemp = srTemp.ReadLine();
            srTemp.Close();
            if (dateTemp != dateTime)
            {
                File.Delete(file);
                FileInit(file);
            }

            List<string> list = new List<string>();
            StreamReader sr = new StreamReader(file, System.Text.Encoding.GetEncoding("UTF-8"));
            string s = "";
            while ((s = sr.ReadLine()) != null)
            {
                list.Add(s);
            }
            sr.Close();
            string item = strSampleNo + "=" + strTestTime + "=" + rownum;
            if (list.Contains(item))
            {
                return true;
            }
            else
            {
                FileStream myFs = new FileStream(file, FileMode.Append);
                StreamWriter mySw = new StreamWriter(myFs);
                mySw.WriteLine(item);
                mySw.Close();
                myFs.Close();
                return false;
            }

        }
        public static string SampleNoToSampleBar(string sampleNo)
        {
            string times="";
            string strSampleNo="";
            string strTestTime="";
            if (sampleNo.Length >= 8)
            {
                DataSetHandle dsHandle = new DataSetHandle();
                IniFile ConfigIni = new IniFile("SOLVESET.INI");
                string orgId = ConfigIni.IniReadValue("EQUIPMENT", "Agencies");
                string sql = @"to_char(申请时间,'yyyymmdd') as 核收时间,样本序号";
                string where = @" 机构id='" + orgId + "' and  样本条码='" + sampleNo + "'";
                DataSet ds = dsHandle.GetDataSet(sql, "检验记录", where);
                DataTable dt = ds.Tables[0];
                if (dt.Rows.Count > 0)
                {
                    times = ds.Tables[0].Rows[0]["核收时间"].ToString();//20160202145321
                    strSampleNo = ds.Tables[0].Rows[0]["样本序号"].ToString();
                    try
                    {
                        strTestTime = times.Substring(0, 4) + "-" + times.Substring(4, 2) + "-" + times.Substring(6, 2) ;
                    }
                    catch(Exception e)
                    {
                        MessageBox.Show("Helper error strTestTime :+\r\n" + e.ToString());
                    }
                        
                    return strTestTime + "|" + strSampleNo;
                }
            }
            return "";
            
        }
        public static List<string> getCurrentFiles(string dateFormat,string pattern,string dirPath)
        {
            if (string.IsNullOrEmpty(dateFormat))
            {
                string fileName = DateTime.Now.ToString("yyyy-MM-dd");
                StringBuilder sBuilder = new StringBuilder("");
                List<string> result = new List<string>();
                DirectoryInfo dirInfo = Directory.CreateDirectory(dirPath);
                FileInfo[] fileInfo = dirInfo.GetFiles(fileName+pattern);

                foreach (FileInfo info in fileInfo)
                {
                    result.Add(info.FullName.ToString());
                }
                return result;
            }
            else
            {
                StringBuilder sBuilder = new StringBuilder("");
                List<string> result = new List<string>();
                DirectoryInfo dirInfo = Directory.CreateDirectory(dirPath);
                FileInfo[] fileInfo = dirInfo.GetFiles(pattern);
                foreach (FileInfo info in fileInfo)
                {
                    result.Add(info.FullName.ToString());
                }
                return result;

            }
        }

    }
}
