using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Data;
using System.Timers;
using System.Data.OracleClient;
using System.Threading;
using System.IO;
using System.Xml;
using System.Windows.Forms;

namespace ZLCHSLisComm
{
    public class Init_COM
    {

        public SerialPort SpCom = new SerialPort();
        public string strInstrumentID;
        public string strSampleID;
        public string strError;
        public string status;//是否启动
        string port;                                       //端口号
        string speed;                                    //波特率
        string data_bits;                                //数据位
        string stop_bits;                                //停止位
        string check_mode;                           //校验方式
        string buffer_in;                                //传入缓冲
        string buffer_out;                              //传出缓冲
        string decode_mode;                        //单帧，多帧
        string data_type;                              //ascii,文本
        string data_begin;                            //数据开始位
        string data_end;                               //数据结束位
        string inbegin;                                 //接收数据开始应答
        string inend;                                    //接收数据结束应答
        string begin_bits;                             //多帧时开始位
        string end_bits;                                //多帧时结束位
        string ACK_all;                                  //全部应答
        string ACK_term;                              //条件应答
        string targetDevice;
        string strDeviceID;
        string RemarkContent;//设备备注
        string strRecive;
        string strBarCode;                           //条码号
        string strRack;
        string resolveType;                          //解析类型
        string CommProgramName;             //通讯程序名称 
        Boolean BSendSample = false;         //正在传输样本
        //string strSendSampleInfo;             //双向传输发送样本信息

        int SendSampleStep = 0;
        public string recvStr;
        string strComRetrieve;                     //串口接收数据
        string strDeviceName;
        string strResult;
        string strReserved;
        //解析后的标准数据
        static object mylock = new object();

        DataSetHandle dsHandle = new DataSetHandle();
        DataSet ds = new DataSet();
        Write_Log writelog = new Write_Log();
        GetResultString resString = new GetResultString();
        List<Thread> threadUpp = new List<Thread>();          //线程数组
        Thread thread;
        public object obj = null;                                             //定义反射对象
        private  IDataResolve IResolve;                            //定义数据解析接口

        /// <summary>
        /// 服务启动时调用
        /// </summary>
        /// <param name="DeviceId">仪器ID</param>
        public void Start(string DeviceId)
        {
            status = "open";
            recvStr = "";
            ds = dsHandle.GetDataSet(@"解析类型,通讯程序名,备注,Extractvalue(通讯参数,'/root/port') As Port, Extractvalue(通讯参数, '/root/speed') As Speed, 
                                     Extractvalue(通讯参数, '/root/data_bits') As Data_Bits,Extractvalue(通讯参数,'/root/stop_bits') As Stop_Bits, 
                                     Extractvalue(通讯参数, '/root/check_mode') As Check_Mode,Extractvalue(接收规则, '/root/buffer_in') As Buffer_In, 
                                     Extractvalue(接收规则, '/root/buffer_out') As Buffer_Out,名称", "检验仪器", "id='" + DeviceId + "'");

            port = ds.Tables[0].Rows[0]["port"].ToString();
            speed = ds.Tables[0].Rows[0]["speed"].ToString();
            data_bits = ds.Tables[0].Rows[0]["data_bits"].ToString();
            stop_bits = ds.Tables[0].Rows[0]["stop_bits"].ToString();
            check_mode = ds.Tables[0].Rows[0]["check_mode"].ToString();
            buffer_in = ds.Tables[0].Rows[0]["buffer_in"].ToString();
            buffer_out = ds.Tables[0].Rows[0]["buffer_out"].ToString();
            strDeviceName = ds.Tables[0].Rows[0]["名称"].ToString();
            resolveType = ds.Tables[0].Rows[0]["解析类型"].ToString();
            CommProgramName = ds.Tables[0].Rows[0]["通讯程序名"].ToString();
            RemarkContent = ds.Tables[0].Rows[0]["备注"].ToString();
            if (resolveType == "1")
            {
                IResolve = resString;
            }
            else
            {
                try
                {
                    obj = ObjectReflection.CreateObject(CommProgramName.Substring(0, CommProgramName.IndexOf(".dll")));
                    IResolve = obj as IDataResolve;
                    IResolve.GetRules(DeviceId);
                }
                catch (Exception exobj)
                {
                    writelog.Write(strDeviceName, exobj.Message, "log");
                    return;
                }

            }
            if (port == "")
            {
                strError = "检验数据接收或检验串口通讯未设置!";
                writelog.Write(strDeviceName, strError, "log");
                return;
            }
            /// <summary>
            /// 打开COM口
            /// </summary>
            try
            {
                SpCom.PortName = "COM" + port;                             //端口号
                SpCom.BaudRate = Convert.ToInt32(speed);                //波特率
                if (check_mode == "N") SpCom.Parity = Parity.None;    //校验位
                if (check_mode == "O") SpCom.Parity = Parity.Odd;
                if (check_mode == "E") SpCom.Parity = Parity.Even;
                if (check_mode == "S") SpCom.Parity = Parity.Space;
                if (check_mode == "M") SpCom.Parity = Parity.Mark;
                SpCom.DataBits = Convert.ToInt32(data_bits);               //数据位长度
                if (stop_bits == "1") SpCom.StopBits = StopBits.One;     //停止位
                if (stop_bits == "2") SpCom.StopBits = StopBits.Two;
                SpCom.Handshake = Handshake.None;

                SpCom.DtrEnable = true;
                SpCom.RtsEnable = true;
                if (string.IsNullOrEmpty(buffer_in) || string.IsNullOrEmpty(buffer_out))
                {
                    SpCom.ReadBufferSize = 2048;
                    SpCom.WriteBufferSize = 1024;
                }
                else
                {
                    SpCom.ReadBufferSize = Convert.ToInt32(buffer_in);
                    SpCom.WriteBufferSize = Convert.ToInt32(buffer_out);
                }
            }
            catch (Exception exsz)
            {
                writelog.Write(strDeviceName, "通讯参数设置错误：" + exsz.Message, "log");
            }

            try
            {
                SpCom.Open();
                writelog.Write(strDeviceName, "打开串口!", "log");
            }
            catch (Exception ex)
            {
                strError = "没有找到串口，请先打开串口!" + ex.Message;
                writelog.Write(strDeviceName, strError, "log");
                return;
            }

            ds = dsHandle.GetDataSet(@"Extractvalue(接收规则, '/root/data_type') As data_type, Extractvalue(接收规则, '/root/data_begin') As data_begin, 
                                       Extractvalue(接收规则, '/root/data_end') As data_end, Extractvalue(接收规则, '/root/start_cmd') As start_cmd, 
                                       Extractvalue(接收规则, '/root/end_cmd') As end_cmd, Extractvalue(接收规则, '/root/ack_all') As ack_all, 
                                       Extractvalue(接收规则, '/root/ack_term') As ack_term, Extractvalue(接收规则, '/root/decode_mode') As decode_mode, 
                                       Extractvalue(接收规则, '/root/begin_bits') As begin_bits, Extractvalue(接收规则, '/root/end_bits') As end_bits",
                                       "检验仪器", "id = '" + DeviceId + "'");
            if (ds.Tables[0].Rows.Count == 0)
            {
                strError = "未设置检验数据接收设置!";
                writelog.Write(strDeviceName, strError, "log");
            }
            data_type = ds.Tables[0].Rows[0]["data_type"].ToString();
            data_begin = ds.Tables[0].Rows[0]["data_begin"].ToString();
            data_end = ds.Tables[0].Rows[0]["data_end"].ToString();
            //inbegin = ds.Tables[0].Rows[0][5].ToString();
            //inend = ds.Tables[0].Rows[0][6].ToString();
            decode_mode = ds.Tables[0].Rows[0]["decode_mode"].ToString();
            begin_bits = ds.Tables[0].Rows[0]["begin_bits"].ToString();
            end_bits = ds.Tables[0].Rows[0]["end_bits"].ToString();
            ACK_all = ds.Tables[0].Rows[0]["ack_all"].ToString();
            ACK_term = ds.Tables[0].Rows[0]["ack_term"].ToString();

            resString.strDetype = decode_mode;
            resString.strInstrument_id = DeviceId;
            resString.strSubBegin = begin_bits;
            resString.strSubEnd = end_bits;
            resString.strDataBegin = data_begin;
            resString.strDataEnd = data_end;
            resString.strACK_all = ACK_all;
            resString.strACK_term = ACK_term;
            resString.GetRules(DeviceId);
            resString.listInputResult = new List<string>();

            //SpCom_DataReceived(null, null);//可以在ParseResult直接写死的字符串检查

            ParameterizedThreadStart ParStart = new ParameterizedThreadStart(ListenThread);
            thread = new Thread(ParStart);
            thread.Start();
            SpCom.DataReceived += new SerialDataReceivedEventHandler(SpCom_DataReceived);//委托，把SerialDataReceivedEventHandler(SpCom_DataReceived)委托给DataReceived
            SpCom.ReceivedBytesThreshold = 1;
        
        }
        /// <summary>
        /// 回收线程
        /// </summary>
        public void ListenThread(object s)
        {
            while (threadUpp.Count > 0)
            {
                for (int i = 0; i < threadUpp.Count; i++)
                {
                    if (threadUpp[i].IsAlive) threadUpp[i].Join();
                }
            }
        }
        /// <summary>
        /// 终止线程
        /// </summary>
        public void Stop()
        {
            if (thread != null)
                thread.Join();
            SpCom.Close();
        }


        /// <summary>
        /// 读取进制该仪器是好多多少位进制，或者未设置
        /// </summary>
        /// <param name="instrumentId">仪器ID</param>
        /// <returns>返回结果值该仪器接收的进制位数</returns>
        private string IsHex(string instrumentId)
        {
            try
            {
                string xmlStrFullPath = AppDomain.CurrentDomain.BaseDirectory + "DeviceSetting.config";
                if (!File.Exists(xmlStrFullPath)) //
                {
                    return "";
                }
                else
                {

                    XmlDocument doc = new XmlDocument();
                    doc.Load(xmlStrFullPath);

                    if (doc.SelectSingleNode(@"//InstrumentPort[@id='" + instrumentId + "']") == null)
                    {
                        return "";
                    }
                    else
                    {
                        return doc.SelectSingleNode(@"//InstrumentPort[@id='" + instrumentId + "']").InnerText.Trim();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }


        }

      
        private void SpCom_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
                      
            try
            {
                 IniFile ConfigIni = new IniFile("SOLVESET.INI");
                string encode = ConfigIni.IniReadValue("EQUIPMENT", "Encode"); 
                string strCmd = "";
                System.Threading.Thread.Sleep(20);//尿常规2秒               
                if (IsHex(strInstrumentID).Equals("16")) //以16进制的方式进行数据接收
                {
                    int n = SpCom.BytesToRead;
                    Byte[] b = new Byte[n];
                    for (int i = 0; i < n; i++)
                    {
                        strComRetrieve += Convert.ToString(SpCom.ReadByte(), 16).PadLeft(2, '0').ToUpper() + " ";
                    }
                }
                else if (string.IsNullOrEmpty(IsHex(strInstrumentID)))
                {
                    SpCom.Encoding = Encoding.GetEncoding(encode);
                    strComRetrieve = SpCom.ReadExisting();
                }

                else //以文本形式接收
                {
                    SpCom.Encoding = Encoding.GetEncoding(encode);
                    strComRetrieve = SpCom.ReadExisting();
                }            
                writelog.Write(strDeviceName, "接收" + strComRetrieve, "log");           
                recvStr += strComRetrieve;
                if (string.IsNullOrEmpty(recvStr))
                {
                    writelog.Write(strDeviceName, "未接收到新数据", "log");
                    return;
                }
                else
                {
                    strReserved = "";
                    IResolve.ParseResult(recvStr, ref strResult, ref strReserved, ref strCmd);
                    recvStr = "";
                }

                    if (!String.IsNullOrEmpty(strCmd)) SpCom.Write(strCmd);
                                
            }
            catch (Exception ex)
            {
                writelog.Write(strDeviceName, "系统错误：" + ex.ToString(), "log");
            }
        }
        
        /// <summary>
        /// 标本核收
        /// </summary>
        public void SampleHS()
        {
            SaveResult saveResult = new SaveResult();
            DataSet dsTestSpecimenList;
            DataSet dsTestApplyDetail;
            DataSet dsGUID;
            string strTestApplyDetailID = "";
            //strBarCode = "102021875157";
            //OracleParameter[] parameters ={
            //                        new OracleParameter("SPECIMEN_ID_IN",OracleType.VarChar,4000),
            //                        new OracleParameter("STATUS_IN",OracleType.Number,1),
            //                        new OracleParameter("OPERATOR_IN",OracleType.VarChar,36),
            //                        new OracleParameter("OPERATOR_TIME_IN",OracleType.DateTime),
            //                        new OracleParameter("LOT_IN",OracleType.VarChar,40),
            //                        new OracleParameter("deny_reason_in",OracleType.VarChar,36)};

            dsTestSpecimenList = dsHandle.GetDataSet("SPECIMEN_ID", "Test_Specimen_list", "Barcode='" + strBarCode + "'");
            foreach (DataRow dr in dsTestSpecimenList.Tables[0].Rows)
            {
                dsTestApplyDetail = dsHandle.GetDataSet("wm_concat(TEST_APPLY_DETAIL_ID) TEST_APPLY_DETAIL_ID", "TEST_APPLY_DETAIL", "SPECIMEN_ID='" + dr["SPECIMEN_ID"].ToString() + "'");
                strTestApplyDetailID = strTestApplyDetailID + "," + dsTestApplyDetail.Tables[0].Rows[0]["TEST_APPLY_DETAIL_ID"].ToString();

            };
            if (strTestApplyDetailID.Substring(0, 1) == ",")
            {
                strTestApplyDetailID = strTestApplyDetailID.Substring(1);
            }
            dsGUID = dsHandle.GetDataSet("sysdate", "dual", "");

            ConnectDB init = new ConnectDB();
            OracleCommand cmd = new OracleCommand();
            OracleTransaction transaction;
            init.DBConnect();
            cmd.Connection = ConnectDB.con;// init.con;
            transaction = ConnectDB.con.BeginTransaction();// init.con.BeginTransaction();
            cmd.Transaction = transaction;

            cmd.CommandText = "P_TEST_SPECIMEN_LIST_UP";//声明存储过程名
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("SPECIMEN_ID_IN", OracleType.VarChar, 200);
            cmd.Parameters.Add("STATUS_IN", OracleType.Number);
            cmd.Parameters.Add("OPERATOR_IN", OracleType.VarChar, 20);
            cmd.Parameters.Add("OPERATOR_TIME_IN", OracleType.DateTime);
            cmd.Parameters.Add("LOT_IN", OracleType.VarChar, 20);
            cmd.Parameters.Add("deny_reason_in", OracleType.VarChar, 36);

            cmd.Parameters["SPECIMEN_ID_IN"].Value = strTestApplyDetailID;
            cmd.Parameters["STATUS_IN"].Value = 6;
            cmd.Parameters["OPERATOR_IN"].Value = "操作员";
            cmd.Parameters["OPERATOR_TIME_IN"].Value = Convert.ToDateTime(dsGUID.Tables[0].Rows[0][0].ToString());
            cmd.Parameters["LOT_IN"].Value = string.Empty;
            cmd.Parameters["deny_reason_in"].Value = strInstrumentID;

            cmd.Parameters["SPECIMEN_ID_IN"].Direction = ParameterDirection.Input;
            cmd.Parameters["STATUS_IN"].Direction = ParameterDirection.Input;
            cmd.Parameters["OPERATOR_IN"].Direction = ParameterDirection.Input;
            cmd.Parameters["OPERATOR_TIME_IN"].Direction = ParameterDirection.Input;
            cmd.Parameters["LOT_IN"].Direction = ParameterDirection.Input;
            cmd.Parameters["deny_reason_in"].Direction = ParameterDirection.Input;

            try
            {
                cmd.ExecuteNonQuery();//执行存储过程
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                writelog.Write(strDeviceName, "核收时出错!" + ex.Message, "log");
            }
            //init.DisConnectDB(init.con);
            //transaction = null;
            //cmd = null;
            //init = null;
        }

        public void GetRaw(object obj)
        {
            string s = (string)obj;
            writelog.Write(strDeviceName, s, "raw");
            string strComRetrieve = "";
            string strBegin;
            string strEnd;

            //解析方式=ASCII
            if (data_type == "1")
            {
                strBegin = StrChange(data_begin);
                strEnd = StrChange(data_end);
                strComRetrieve = s;
                if (s.IndexOf(strBegin) > -1) strComRetrieve = s.Substring(s.IndexOf(strBegin) + strBegin.Length);//去除开始符
                if (s.IndexOf(strEnd) > -1) strComRetrieve = strComRetrieve.Substring(0, strComRetrieve.IndexOf(strEnd));//去除结束符
                //writelog.Write(strComRetrieve, "result");


                resString.listInputResult = new List<string>();
                resString.listInputResult.Add(strComRetrieve);
                // resString.ParseResult();

            }
            //解析方式为字符串
            else if (data_type == "2")
            {
                strComRetrieve = s;
                //writelog.Write(strComRetrieve, "result");
                resString.listInputResult = new List<string>();
                resString.listInputResult.Add(strComRetrieve);
                // resString.ParseResult();
            }
        }
        /// <summary>
        /// 根据条件应答符获取应答指令
        /// </summary>
        /// <param name="dataIn">传回数据</param>
        /// <param name="ack_term">条件应答串,格式：传回数据1:应答符1|传回数据2:应答符2|…</param>
        public string GetCmd(string dataIn, string ack_term)
        {
            foreach (string source in ack_term.Split('|'))
            {
                if (!String.IsNullOrEmpty(source))
                {
                    if (source.Contains(":"))
                    {
                        if (StrChange(source.Split(':')[0]) == StrChange(dataIn)) return StrChange(source.Split(':')[1]);
                    }
                    else return StrChange(source);
                }
            }
            return "";
        }
        /// <summary>
        /// 字符转换
        /// </summary>
        private string StrChange(string SourceData)
        {
            if (SourceData.Contains("<") && SourceData.Contains(">")) return ((char)int.Parse(SourceData.Replace("<", "").Replace(">", ""))).ToString();
            else return SourceData;
        }
    }
}
