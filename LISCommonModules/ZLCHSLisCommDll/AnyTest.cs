using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ZLCHSLisComm
{
    public class AnyTest
    {
        public string filePath, fileName;
        public string dataRow, dataEnd;
        public string strInstrumentID;
        public Boolean bnReadBatch;               //读取单个文件

        List<string> strFolder = new List<string>();
        public List<string> resultArray = new List<string>();

        List<string> strHBcAbArray = new List<string>();
        List<string> strHBeAbArray = new List<string>();
        List<string> strHBeAgArray = new List<string>();
        List<string> strHBsAbArray = new List<string>();
        List<string> strHBsAgArray = new List<string>();
        List<string> strSampleInfo = new List<string>();
        List<string> TestGraph = new List<string>(); //图像列表
        string strHBcAbID, strHBeAbID, strHBeAgID, strHBsAbID, strHBsAgID;

        long lngStep = 0;                   //读取文件个数
        string strResult;
        string strFileName;
        DataSetHandle dsHandle;
        System.Data.DataSet dsProject;
        DateTime dtCreatDate;
        FileInfo fi;
        Write_Log writelog = new Write_Log();

        public void Start()
        {
            strFolder.Add("HBcAb");
            strFolder.Add("HBeAb");
            strFolder.Add("HBeAg");
            strFolder.Add("HBsAb");
            strFolder.Add("HBsAg");

            dsHandle = new DataSetHandle();
            dsProject = dsHandle.GetDataSet("TEST_ITEM_ID,CHANNEL_NO", "TEST_INSTRUMENT_ITEM_CHANNEL", "INSTRUMENT_ID='" + strInstrumentID + "'");

            foreach (System.Data.DataRow dr in dsProject.Tables[0].Rows)
            {
                if (dr["CHANNEL_NO"].ToString().ToUpper() == "HBCAB") strHBcAbID = dr["TEST_ITEM_ID"].ToString();
                else if (dr["CHANNEL_NO"].ToString().ToUpper() == "HBEAB") strHBeAbID = dr["TEST_ITEM_ID"].ToString();
                else if (dr["CHANNEL_NO"].ToString().ToUpper() == "HBEAG") strHBeAgID = dr["TEST_ITEM_ID"].ToString();
                else if (dr["CHANNEL_NO"].ToString().ToUpper() == "HBSAB") strHBsAbID = dr["TEST_ITEM_ID"].ToString();
                else if (dr["CHANNEL_NO"].ToString().ToUpper() == "HBSAG") strHBsAgID = dr["TEST_ITEM_ID"].ToString();

            };
            //通过线程去提取数据
            System.Threading.ParameterizedThreadStart ParStart = new System.Threading.ParameterizedThreadStart(GetResult);
            System.Threading.Thread threadSocket = new System.Threading.Thread(ParStart);
            object socketListen = "";
            threadSocket.Start(socketListen);

            //2010-05-11 22:16:49|22^^0||标本||aa31c32d-b539-4cfc-8dea-a0e6f85f33d7^3.9|
            //SaveResult saveResult = new SaveResult();

            //filePath = @"D:\新建文件夹\二院仪器数据\AnyTest\0Aux";
            fileName = "*.*";
            //fileName = "*.out";

            //writelog.Write(filePath,"log");

        }

        public void GetResult(object s)
        {
            //writelog.Write("1 " + bnReadBatch, "log");
            //bnReadBatch = true;
            if (bnReadBatch)
            {
                //writelog.Write("2", "log");
                lngStep = 1;
                string[] strFiles = Directory.GetFiles(filePath, fileName);

                strFiles = FilterOutoffHiddenFiles(strFiles);
                strFileName = null;
                foreach (string name in strFiles)
                {
                    if (string.IsNullOrEmpty(name)) continue;
                    FileInfo fi = new FileInfo(name);
                    if (fi.LastWriteTime < System.DateTime.Today) continue;
                    strFileName = name;

                    strFileName = strFileName.Replace(filePath + "\\", "");
                    //writelog.Write(strFileName, "log");
                    ReadFile();
                }
            }
            else
            {
                //writelog.Write("3", "log");
                lngStep = 1;
                //foreach (string strFolderName in strFolder)
                //{
                    //FileSystemWatcher
                    //lngStep += 1;

                    string[] strFiles = Directory.GetFiles(filePath, fileName);

                    strFiles = FilterOutoffHiddenFiles(strFiles);
                    strFileName = null;
                    foreach (string name in strFiles)
                    {
                        //writelog.Write("name" + name, "log");
                        if (string.IsNullOrEmpty(name)) continue;
                        fi = new FileInfo(name);
                        if (fi.LastWriteTime < System.DateTime.Today) continue;

                        if (string.IsNullOrEmpty(strFileName))
                        {
                            fi = new FileInfo(name);
                            dtCreatDate = fi.LastWriteTime;
                            strFileName = name;
                            fi = null;
                        }
                        else
                        {
                            fi = new FileInfo(name);
                            if (fi.LastWriteTime > dtCreatDate)
                            {
                                strFileName = name;
                                dtCreatDate = fi.LastWriteTime;
                            }
                            fi = null;
                        }

                        //if (Convert.ToInt32(name.Substring(name.LastIndexOf('\\') + 1).Substring(0, name.Substring(name.LastIndexOf('\\') + 1).IndexOf("."))) >
                        //    Convert.ToInt32(strFileName.Substring(strFileName.LastIndexOf('\\') + 1).Substring(0, strFileName.Substring(strFileName.LastIndexOf('\\') + 1).IndexOf("."))))
                        //{
                        //    strFileName = name;
                        //}
                    }
                    strFileName = strFileName.Replace(filePath + "\\", "");
                    //writelog.Write("1"+strFileName, "log");
                    ReadFile();
                //}

                //writelog.Write("" + strSampleInfo.Count, "log");
                //for (int i = 0; i < strSampleInfo.Count; i++)
                //{
                //    SaveResult saveResult = new SaveResult();
                //    saveResult.SaveTextResult(strInstrumentID, strSampleInfo[i] + "|" + strHBcAbArray[i] + "|" + strHBeAbArray[i] + "|" + strHBeAgArray[i] + "|" + strHBsAbArray[i] + "|" + strHBsAgArray[i] + "|", TestGraph, null);
                //    saveResult.UpdateData();
                //    saveResult = null;
                //    //writelog.Write("" + i, "log");
                //}
            }
        }
        /// <summary>
        /// 读取文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="fileName">文件名称</param>
        /// <param name="readType">打开方式</param>
        /// <param name="fileType">文件类型</param>
        private void ReadFile()
        {
            String inputStr;
            Boolean lbResult = false;

            FileStream ifs = new FileStream(filePath + "\\" + strFileName, FileMode.Open, FileAccess.Read);
            StreamReader SrReadLin = new StreamReader(ifs, System.Text.Encoding.Default);
            while (true)
            {
                inputStr = SrReadLin.ReadLine();
                //writelog.Write(inputStr, "log");
                if (lbResult == false)
                {
                    //if (inputStr.IndexOf(" SEQ  PAT             COUNTS     CONC  %cvCONC     UNIT     CODE") < 0) continue;
                    if (inputStr.IndexOf(" SEQ  PAT             COUNTS     CONC  %cvCONC     UNIT     CODE") < 0) continue;
                    else lbResult = true;
                    continue;
                }

                if (string.IsNullOrEmpty(inputStr)) break;

                SaveResult saveResult = new SaveResult();

                inputStr = inputStr + fileName.Substring(0, fileName.IndexOf("."));

                if (lngStep == 1) strSampleInfo.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "|" + inputStr.Substring(57, 7).Trim() + "^^0||标本|");
                strResult = inputStr.Substring(28, 10).Replace(">", " ").Replace("<", " ").Trim();
                //try
                //{
                //    strResult = Convert.ToDecimal(strResult).ToString();
                //}
                //catch (Exception ex)
                //{
                //}
                if (strFileName.ToUpper().IndexOf("HBCAB") >= 0) saveResult.SaveTextResult(strInstrumentID, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "|" + inputStr.Substring(57, 7).Trim() + "^^0||标本|" + "|" + strHBcAbID + "^" + Convert.ToDouble(strResult).ToString("f2") + "|", TestGraph, null);
                else if (strFileName.ToUpper().IndexOf("HBEAB") >= 0) saveResult.SaveTextResult(strInstrumentID, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "|" + inputStr.Substring(57, 7).Trim() + "^^0||标本|" + "|" + strHBeAbID + "^" + Convert.ToDouble(strResult).ToString("f2") + "|", TestGraph, null);
                else if (strFileName.ToUpper().IndexOf("HBEAG") >= 0) saveResult.SaveTextResult(strInstrumentID, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "|" + inputStr.Substring(57, 7).Trim() + "^^0||标本|" + "|" + strHBeAgID + "^" + Convert.ToDouble(strResult).ToString("f3") + "|", TestGraph, null);
                else if (strFileName.ToUpper().IndexOf("HBSAB") >= 0) saveResult.SaveTextResult(strInstrumentID, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "|" + inputStr.Substring(57, 7).Trim() + "^^0||标本|" + "|" + strHBsAbID + "^" + Convert.ToDouble(strResult).ToString("f1") + "|", TestGraph, null);
                else if (strFileName.ToUpper().IndexOf("HBSAG") >= 0) saveResult.SaveTextResult(strInstrumentID, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "|" + inputStr.Substring(57, 7).Trim() + "^^0||标本|" + "|" + strHBsAgID + "^" + Convert.ToDouble(strResult).ToString("f2") + "|", TestGraph, null);

                //writelog.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "|" + inputStr.Substring(57, 7).Trim() + "^^0||标本|" + "|" + strHBsAgID + "^" + Convert.ToDecimal(inputStr.Substring(28, 10).Trim()).ToString("f2") + "|", "log");
                saveResult.UpdateData();
                saveResult = null;
            }
            SrReadLin.Close();
            ifs.Close();
        }

        /// <summary>
        /// 去除隐藏文件
        /// </summary>
        /// <param name="filenames">符合条件的文件名称数组</param>
        private string[] FilterOutoffHiddenFiles(string[] filenames)
        {
            string[] filenames1 = new string[filenames.Length];
            int i = 0;
            foreach (string str in filenames)
            {
                FileInfo fi = new FileInfo(str);
                if ((fi.Attributes & FileAttributes.Hidden) == 0)
                {
                    filenames1[i++] = str;
                }
            }
            return filenames1;
        }
    }
}
