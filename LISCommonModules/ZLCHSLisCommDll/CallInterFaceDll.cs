using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OleDb;
using System.Runtime.InteropServices;
using System.Data;
using System.IO.Ports;
using System.Windows.Forms;


namespace ZLCHSLisComm
{
    public class CallInterFaceDll
    {
        string strCommType;                //通讯类型 1-串口，2-网络，3-数据库,4-文本
        string strError;                   //错误信息
        string strDeviceName;              //仪器名称
        public static Boolean boolRun = false;           //正在运行

        Write_Log writeLog;
       //xt Init_COM initDll_COM;
        List<Init_COM> list_initDll_COM = new List<Init_COM>();//xt
        Dictionary<string,Init_COM> dictionary_initDll_COM = new Dictionary<string,Init_COM>();
        Dictionary<string, Init_TCP> dictionary_initDll_TCP = new Dictionary<string, Init_TCP>();
        Dictionary<string, Init_DB> dictionary_initDll_DB = new Dictionary<string, Init_DB>();
        Init_TCP initDll_TCP;
        Init_TXT initDll_TXT;
        Init_DB initDll_DB;
        DataSetHandle dsHandle;
        DataSet ds;

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        //发送消息
        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpData;
        }
        const int WM_COPYDATA = 0x004A;

        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(int hWnd, int Msg, int wParam, ref COPYDATASTRUCT lParam);

        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        private static extern int FindWindow(string lpClassName, string lpWindowName);

        /// <summary>
        /// type 调用类型
        /// 0 - 初始化
        /// 1 - 连接
        /// 9 - 断开
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string CallDll(string para)
        {
            writeLog = new Write_Log();            
            dsHandle = new DataSetHandle();
            ds = new DataSet();
            
            string strInstrumentID = "";
            string strMicroPlateID = "";
            string strSampleNO = "";
            string strSampleID = "";
            string strCallSource = "";
            string strInstrumentKind = "";
            string strPara = "";
            string strType = "";
            string strReadBatch = "";
            string strDeviceId = "";

            strInstrumentID = "";
            strType = "";
            strMicroPlateID = "";
            strSampleID = "";
            strSampleNO = "";
            strCallSource = "";

            if (boolRun) return strError;            

            Dictionary<string, string> para_dic = new Dictionary<string, string>();
            string[] para_value = para.Split(';');
            foreach (string para_col in para_value)
            {
                if (string.IsNullOrEmpty(para_col)) break;
                string[] tmp = para_col.Split('=');
                para_dic.Add(tmp[0].ToUpper(), tmp[1]);
            }
            strPara = "";
            //仪器ID
            if (para.ToUpper().IndexOf("TEST_INSTRUMENT_ID=") >= 0)
            {
                strInstrumentID = para_dic["TEST_INSTRUMENT_ID"];
                strPara = strPara + "TEST_INSTRUMENT_ID=" + strInstrumentID + ";";
            }
            //类型
            if (para.ToUpper().IndexOf("TYPE=") >= 0)
            {
                strType = para_dic["TYPE"];
                strPara = strPara + "TYPE=" + strType + ";";
            }


            //检验标本ID
            if (para.ToUpper().IndexOf("TEST_SAMPLE_ID=") >= 0)
            {
                strSampleID = para_dic["TEST_SAMPLE_ID"];
                strPara = strPara + "TEST_SAMPLE_ID=" + strSampleID + ";";
            }
            //标本ID
            if (para.ToUpper().IndexOf("SAMPLE_NO=") >= 0)
            {
                strSampleNO = para_dic["SAMPLE_NO"];
                if (!string.IsNullOrEmpty(strSampleNO))
                {
                    strPara = strPara + "SAMPLE_NO=" + strSampleNO + ";";
                }
            }
            //调用来源
            if (para.ToUpper().IndexOf("CALL_SOURCE=") >= 0)
            {
                strCallSource = para_dic["CALL_SOURCE"];
                strPara = strPara + "CALL_SOURCE=" + strCallSource + ";";
            }
            //单个读取
            if (para.ToUpper().IndexOf("READ_BATCH=") >= 0)
            {
                strReadBatch = para_dic["READ_BATCH"];
                strPara = strPara + "READ_BATCH=" + strReadBatch + ";";
            }

            if (ConnectDB.con.State != ConnectionState.Open)
            {
                ConnectDB init = new ConnectDB();
                init.DBConnect();
            }
            //处理一台电脑解析多个数据
            for (int j = 0; j < strInstrumentID.Split(',').Length; j++)
            {
               
             
                strDeviceId = strInstrumentID.Split(',')[j];
                ds = dsHandle.GetDataSet("通讯类型,名称", "检验仪器", "ID = '" + strDeviceId + "'");
                if (ds == null || ds.Tables[0].Rows.Count==0)
                {
                    strError = "未找到Id[" + strDeviceId + "]的仪器";
                    writeLog.Write(strError, "log");
                    break;
                }
                strCommType = ds.Tables[0].Rows[0]["通讯类型"].ToString();
                strDeviceName = ds.Tables[0].Rows[0]["名称"].ToString();               

                //strCallSource = "1";
                //BH调用exe
                if (strCallSource == "1")
                {
                    int WINDOW_HANDLER = FindWindow(null, @"解析程序");
                    if (WINDOW_HANDLER==0)
                    //if (WINDOW_HANDLER.Equals(IntPtr.Zero))
                    {
                        strError = "解析程序未运行!";
                        MessageBox.Show(strError,"导入数据",MessageBoxButtons.OK,MessageBoxIcon.Information);
                        writeLog.Write( strDeviceName, strError, "log");
                        break;
                    }
                    else
                    {
                        byte[] sarr = System.Text.Encoding.Default.GetBytes(strPara);
                        int len = sarr.Length;

                        COPYDATASTRUCT cds;
                        cds.dwData = (IntPtr)1000;
                        cds.lpData = strPara;
                        cds.cbData = len + 1;
                        SendMessage(WINDOW_HANDLER, WM_COPYDATA, 0, ref cds);
                    }
                }
                else
                {
                    if (strCommType == "1")
                    {
                        #region 同时运行
                        if (strType != "9")
                        {
                            if (!dictionary_initDll_COM.ContainsKey(strDeviceName))
                            {
                                Init_COM initDll_COM = new Init_COM();
                              
                              
                                if (string.IsNullOrEmpty(initDll_COM.strSampleID) && !string.IsNullOrEmpty(strSampleID))
                                {
                                    initDll_COM.strSampleID = strSampleID;
                                }
                                if (string.IsNullOrEmpty(initDll_COM.strInstrumentID) && !string.IsNullOrEmpty(strInstrumentID))
                                {
                                    initDll_COM.strInstrumentID = strInstrumentID.Split(',')[j];
                                }
                                dictionary_initDll_COM.Add(strDeviceName, initDll_COM);
                            }

                        }
                        if (strType == "0")
                        {
                            //initDll_COM.Start(strDeviceId);
                            foreach (Init_COM item in dictionary_initDll_COM.Values)
                            {
                                if (string.IsNullOrEmpty(item.status))
                               item.Start(strDeviceId);
                            }
                           
                        }
                        else if (strType == "9")
                        {
                            //initDll_COM.Stop();
                            foreach (Init_COM item in dictionary_initDll_COM.Values)
                            {
                                
                                    item.Stop();
                                    strError = item.strError;
                                
                            }
                            dictionary_initDll_COM.Clear();
                        }
                        #endregion

                    }
                    else if (strCommType == "2")
                    {
                        if (initDll_TCP == null)
                        {
                            initDll_TCP = new Init_TCP();
                            initDll_TCP.strInstrumentID = strInstrumentID.Split(',')[j];
                        }
                        if (strType == "0")
                        {
                            initDll_TCP.Init();
                        }
                        else if (strType == "9")
                        {
                            initDll_TCP.Stop();
                        }
                        strError = initDll_TCP.strError;
                    }
                   //xt 数据库类型支持多个同时应用
                    else if (strCommType == "3")
                    {



                        if (initDll_DB == null)
                        {
                            initDll_DB = new Init_DB();
                            initDll_DB.strInstrumentID = strInstrumentID.Split(',')[j];
                            initDll_DB.strTestNO = strSampleNO;
                            initDll_DB.Init();

                            #region 同时运行
                            //if (strType != "9")
                            //{
                            //    if (!dictionary_initDll_DB.ContainsKey(strDeviceName))
                            //    {
                            //        initDll_DB = new Init_DB();


                            //        if (string.IsNullOrEmpty(initDll_DB.strTestNO) && !string.IsNullOrEmpty(strSampleID))
                            //        {
                            //            initDll_DB.strTestNO = strSampleID;
                            //        }
                            //        if (string.IsNullOrEmpty(initDll_DB.strInstrumentID) && !string.IsNullOrEmpty(strInstrumentID))
                            //        {
                            //            initDll_DB.strInstrumentID = strInstrumentID.Split(',')[j];
                            //        }
                            //        dictionary_initDll_DB.Add(strDeviceName, initDll_DB);
                            //    }

                            //}
                            //if (strType == "0")
                            //{
                            //    //initDll_COM.Start(strDeviceId);
                            //    foreach (Init_DB item in dictionary_initDll_DB.Values)
                            //    {
                            //        if (string.IsNullOrEmpty(item.status))
                            //            item.Init(strDeviceId);
                            //    }

                            //}
                            //else if (strType == "9")
                            //{
                            //    //initDll_COM.Stop();
                            //    foreach (Init_DB item in dictionary_initDll_DB.Values)
                            //    {

                            //        item.Stop();
                            //        strError = item.strError;

                            //    }
                            //    dictionary_initDll_DB.Clear();
                            //}
                            #endregion

                        }
                        strError = initDll_DB.strError;
                    }
                    else if (strCommType == "4")
                    {
                        if (initDll_TXT == null) initDll_TXT = new Init_TXT();
                        initDll_TXT.strInstrumentID = strInstrumentID.Split(',')[j];
                        initDll_TXT.bnReadBatch = (strReadBatch == "1" ? true : false);
                        initDll_TXT.Start();
                        strError = initDll_TXT.strError;
                    }
                    else
                    {
                        strError = "未设置仪器的通讯类型!";
                    }
                    if (strType == "9")
                    {
                        if (ConnectDB.con.State == ConnectionState.Open)
                        {
                            ConnectDB init = new ConnectDB();
                            init.DisConnectDB(ConnectDB.con);
                        }
                    }
                }
                if (!String.IsNullOrEmpty(strError)) writeLog.Write(strDeviceName, strError, "log");
            }
            return strError;
        }

        /// <summary>
        /// 选取本地图片
        /// </summary>
        /// <param name="IMG"></param>
        /// <returns></returns>
        public System.Drawing.Bitmap LocalIMG(string IMG)
        {
            System.IO.FileStream fs = new System.IO.FileStream(IMG, System.IO.FileMode.Open);
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(fs);
            fs.Close();
            return bmp;
        }
    }
}
