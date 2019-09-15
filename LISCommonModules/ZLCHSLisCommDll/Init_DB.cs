/****
 * ===1===start
 * 修改时间:2017-09-12
 * 修改人:谢天
 * 修改内容:支持多线程,能够同时开启多个数据库类型进行同时处理
 * ===1===end
 * 
 * */
using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OleDb;
using System.Data;
using System.Data.Odbc;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

namespace ZLCHSLisComm
{
    public class Init_DB
    {
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        //DATABASE
        public string strInstrumentID;
        public string strError;
        public string strTestNO;
        public Boolean bnAve763;
        public Boolean bnHPV;
        public Boolean bnSLAN;
        public string status;//是否启动
        string databasetype;
        string connectstr;
        string servername;
        string databasename;
        string username;
        string password;
        string parastr;
        string sqlstr;

        string dataType;
        string databegin;
        string dataend;
        string inBegin;
        string inEnd;
        string detype;
        string SubBegin;
        string SubEnd;
        string ConACK;
        string DataACK;

        string strChannel;
        string resultstring;       
        string conStr = null;
        Boolean boolSlan;
        System.Data.DataSet dsGuid;
        string strTestItemID;
        string strSampleNO;
        string strResult;
        SaveResult saveResult;

        OleDbConnection conDB = new OleDbConnection();
        GetResultString resString = new GetResultString();
        DataSetHandle dsHandle = new DataSetHandle();
        DataTable tItemChannel = new DataTable();          //项目通道
        List<string> TestGraph = new List<string>();        //图像列表
        DataSet ds = new DataSet();
        DataSet dsResult;
        Write_Log writeLog = new Write_Log();
        System.Timers.Timer aTimer = new System.Timers.Timer();
        System.Timers.Timer aTimer1 = new System.Timers.Timer();
        public object obj = null;                                             //定义反射对象
        public static IDataResolve IResolve;                            //定义数据解析接口
        string resolveType;                                            //解析类型
        string CommProgramName;                               //通讯程序名
        string strDevice;
        //DATABASE
        /// <summary>
        /// 初始化执行
        /// </summary>
        public void Init( )
        {
            status = "open";
            ds = dsHandle.GetDataSet(@"解析类型,通讯程序名,备注,
                                                        Extractvalue(通讯参数, '/root/db_type') as db_type,
                                                        Extractvalue(通讯参数, '/root/db_name') as db_name,
                                                        Extractvalue(通讯参数, '/root/user_name') as user_name,
                                                        Extractvalue(通讯参数, '/root/password') as password,
                                                        Extractvalue(通讯参数, '/root/server_name') as server_name,
                                                        Extractvalue(通讯参数, '/root/parastr') as parastr,
                                                        Extractvalue(通讯参数, '/root/selectstr') as selectstr", "检验仪器", "id = '" + strInstrumentID + "'");
            databasetype = ds.Tables[0].Rows[0]["db_type"].ToString();           
            servername = ds.Tables[0].Rows[0]["server_name"].ToString();
            databasename = ds.Tables[0].Rows[0]["db_name"].ToString();
            username = ds.Tables[0].Rows[0]["user_name"].ToString();
            password = ds.Tables[0].Rows[0]["password"].ToString();
            parastr = ds.Tables[0].Rows[0]["parastr"].ToString();
            sqlstr = ds.Tables[0].Rows[0]["selectstr"].ToString();
            CommProgramName = ds.Tables[0].Rows[0]["通讯程序名"].ToString();
            resolveType = ds.Tables[0].Rows[0]["解析类型"].ToString();         
            if (sqlstr.IndexOf("[SAMPLE_NO]") > 0)
            {
                sqlstr = sqlstr.Replace("[SAMPLE_NO]", strTestNO);
            }            
            ds = dsHandle.GetDataSet(@"Extractvalue(接收规则, '/root/buffer_in') As Buffer_In, Extractvalue(接收规则, '/root/buffer_out') As Buffer_out, 
                                                       Extractvalue(接收规则, '/root/data_type') As data_type, Extractvalue(接收规则, '/root/data_begin') As data_begin, 
                                                       Extractvalue(接收规则, '/root/data_end') As data_end, Extractvalue(接收规则, '/root/start_cmd') As start_cmd, 
                                                       Extractvalue(接收规则, '/root/end_cmd') As end_cmd, Extractvalue(接收规则, '/root/Ack_all') As Ack_all, 
                                                       Extractvalue(接收规则, '/root/ack_term') As ack_term, Extractvalue(接收规则, '/root/decode_mode') As decode_mode, 
                                                       Extractvalue(接收规则, '/root/begin_bits') As begin_bits, Extractvalue(接收规则, '/root/end_bits') As end_bits",
                                                    "检验仪器", "id = '" + strInstrumentID + "'");
            dataType = ds.Tables[0].Rows[0]["data_type"].ToString();
            databegin = ds.Tables[0].Rows[0]["data_begin"].ToString();
            dataend = ds.Tables[0].Rows[0]["data_end"].ToString();
           
            if (databasetype == "1")
            {                
                if (string.IsNullOrEmpty(password))
                    conStr = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + parastr + databasename + ";Persist Security Info=True";
                else
                    conStr = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + parastr + databasename + ";Persist Security Info=True; Jet OLEDB:Database Password=" + password;
            }
            else if (databasetype == "2")
            {
                conStr = @"Provider=SQLOLEDB.1;Persist Security Info=False;User ID=" + username + ";Password=" + password + ";Initial Catalog=" + databasename + ";Data Source=" + servername;              

            }
            else if (databasetype == "3")//Excel
            {
                conStr = @"Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + databasename + ";Extended Properties='Excel 8.0;HDR=Yes;IMEX=1;'";
            }
            else if (databasetype == "4")//mySQL
            {
                conStr = @"Data Source=" + databasename + ";Password=" + password + ";User ID=" + username + ";Location=" + servername + "";
            }
            conDB.ConnectionString = conStr;


            resString.strDetype = detype;
            resString.strInstrument_id = strInstrumentID;
            resString.strSubBegin = SubBegin;
            resString.strSubEnd = SubEnd;
            resString.listInputResult = new List<string>();
            resString.ImmediatelyUpdate = true;
            ds = dsHandle.GetDataSet("名称", "检验仪器", "id= '" + strInstrumentID + "'");

            strDevice = ds.Tables[0].Rows[0]["名称"].ToString();
            if (resolveType == "1")
            {
                IResolve = resString;
                Start();
            }
            else
            {
                try
                {
                    BH_Init_DB bhdb = new BH_Init_DB();
                    //反射动态链接库执行
                    Double I = Convert.ToDouble(bhdb.ReadRuntime());
                    obj = ObjectReflection.CreateObject(CommProgramName.Substring(0, CommProgramName.IndexOf(".dll")));
                    IResolve = obj as IDataResolve;
                    IResolve.GetRules(strInstrumentID);
                    AutoExecute(null, null);
                    aTimer1.Elapsed += new ElapsedEventHandler(AutoExecute);
                    aTimer1.Interval = I == 0 ? 10000 : I;
                    aTimer1.AutoReset = true;
                    aTimer1.Enabled = true;  

                }
                catch (Exception)
                {

                    throw;
                }
            }
        

        }

        /// <summary>
        /// 提取数据
        /// </summary>
        public void Start()
        {
            try
            {
                conDB.Open();
            }
            catch (Exception e)
            {              
                writeLog.Write("数据库连接失败！" + e.Message, "log");
                strError = "数据库连接失败！" + e.Message;
                return;
            }
            if (conDB.State != ConnectionState.Open) return;

            DataTable dtSample = OracleHelper.GetDataTable(@"select   wm_concat(样本序号) 样本序号
                                                                                          from 检验记录
                                                                                          where id in (select 记录id from 检验普通结果 where 结果标志 is not null)
                                                                                                   and 仪器id = '" + strInstrumentID + @"'
                                                                                                   and to_char(核收时间, 'yyyy-mm-dd') = to_char(sysdate, 'yyyy-mm-dd')");
            string _sampleNo = dtSample.Rows[0]["样本序号"].ToString();
            if (string.IsNullOrEmpty(sqlstr))
                sqlstr = @"SELECT dDate, nSid, sItem, fConc, sConc
                                  FROM Result
                                 where Format(dDate, \""yyyy-mm-dd\"") = Format(date() - 1, \""yyyy-mm-dd\"")
                                   and nSid not in (parameter)
                                 order by nsid";
            sqlstr = sqlstr.Replace("parameter", _sampleNo == "" ? "-1" : _sampleNo);//替换样本号
            dsResult = new DataSet();
            try
            {
                OleDbDataAdapter ada = new OleDbDataAdapter(sqlstr, conDB);
                ada.Fill(dsResult, "Table");
            }
            catch (System.Data.OleDb.OleDbException sqlex)
            {
                strError = sqlex.Message;
                writeLog.Write(strError, "log");
                return;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                writeLog.Write(strError, "log");
                return;
            }
            finally
            {
                conDB.Close();
            }           
            object socketListen = strError;          
            GetResult(strError, null);            
            aTimer.Elapsed += new ElapsedEventHandler(GetResult);
            aTimer.Interval = 10000;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        /// <summary>
        /// 断开数据库连接
        /// </summary>
        public void Stop()
        {
            conDB.Close();
        } 
        /// <summary>
        /// 反射动态执行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void AutoExecute(object sender, EventArgs e)
        {          
               Write_Log wl = new Write_Log();
                try
                {
                    string strResult = "";
                    string strReserved = "";
                    string strCmd = "";
                    string strSource = sqlstr + "|" + conStr + "|" + databasetype + "|" + parastr; //SQL语句+连接串+数据库类型+其他参数
                    IResolve.ParseResult(strSource, ref strResult, ref strReserved, ref strCmd);                   
                } 
                catch (Exception ex)
                {
                    wl.Write(ex.Message.ToString(), "log");
                }               
        }

        /// <summary>
        /// 判断是否有重复
        /// </summary>
        public void IsUpdate()
        {
            ds = dsHandle.GetDataSet(@"解析类型,通讯程序名,
                                                        Extractvalue(通讯参数, '/root/db_type') as db_type,
                                                        Extractvalue(通讯参数, '/root/db_name') as db_name,
                                                        Extractvalue(通讯参数, '/root/user_name') as user_name,
                                                        Extractvalue(通讯参数, '/root/password') as password,
                                                        Extractvalue(通讯参数, '/root/server_name') as server_name,
                                                        Extractvalue(通讯参数, '/root/parastr') as parastr,
                                                        Extractvalue(通讯参数, '/root/selectstr') as selectstr", "检验仪器", "id = '" + strInstrumentID + "'");
            sqlstr = ds.Tables[0].Rows[0]["selectstr"].ToString();
            if (sqlstr.IndexOf("[SAMPLE_NO]") > 0)
            {
                sqlstr = sqlstr.Replace("[SAMPLE_NO]", strTestNO);
            }
            try
            {
                conDB.Open();
            }
            catch (Exception e)
            {
                //conDB.Close();
                writeLog.Write("数据库连接失败！" + e.Message, "log");
                strError = "数据库连接失败！" + e.Message;
                return;
            }
            if (conDB.State != ConnectionState.Open) return;

            DataTable dtSample = OracleHelper.GetDataTable(@"select   wm_concat(样本序号) 样本序号
                                                                                          from 检验记录
                                                                                          where id in (select 记录id from 检验普通结果 where 结果标志 is not null)
                                                                                                   and 仪器id = '" + strInstrumentID + @"'
                                                                                                   and to_char(核收时间, 'yyyy-mm-dd') = to_char(sysdate, 'yyyy-mm-dd')");
            string _sampleNo = dtSample.Rows[0]["样本序号"].ToString();
            if (string.IsNullOrEmpty(sqlstr))
                sqlstr = @"SELECT dDate, nSid, sItem, fConc, sConc
                                  FROM Result
                                 where Format(dDate, \""yyyy-mm-dd\"") = Format(date() - 1, \""yyyy-mm-dd\"")
                                   and nSid not in (parameter)
                                 order by nsid";
            sqlstr = sqlstr.Replace("parameter", _sampleNo == "" ? "-1" : _sampleNo);//替换样本号
            dsResult = new DataSet();
            try
            {
                OleDbDataAdapter ada = new OleDbDataAdapter(sqlstr, conDB);
                ada.Fill(dsResult, "Table");
            }
            catch
            {
                dsResult = null;
            }
            finally
            {
                conDB.Close();
            }
        }

        /// <summary>
        /// 默认解析方式
        /// </summary>
        /// <param name="a"></param>
        /// <param name="e"></param>
        public void GetResult(object a, EventArgs e)
        {
            try
            {
                string strTestTime;              //检验时间
                string strSampleNo;            //标本号
                string strBarCode = "";        //条码
                string strOperator = "";       //检验医师
                string strSampleType = "";   //检验类型
                string StrSpecimen = "";      //标本类型
                string ChannelType;             //0-普通结果;1-直方图;2-散点图;3-直方图界标;4-散点图界标;5-BASE64
                string testItemID = "";          //通道项目ID
                DataRow[] FindRow;             //解析设置
                TestGraph = new List<string>();
                saveResult = new SaveResult();
                IsUpdate();//重新检查是否有新数据
                tItemChannel = OracleHelper.GetDataTable(@"Select 通道编码, m.项目id, Nvl(小数位数, 2) As 小数位数, Nvl(换算比, 0) As 换算比, Nvl(加算值, 0) As 加算值, j.结果类型
                                                                                From 仪器检测项目 m, 检验项目 j
                                                                                Where m.项目id = j.项目id and m.仪器Id='" + strInstrumentID + "'");
                if (dsResult.Tables[0].Rows.Count == 0)//判断是否有新标本
                {
                    writeLog.Write(DateTime.Now.ToString() + "未检测到新数据！", "log");
                    return;
                }

                DataTable _dsResult = SelectDistinct(dsResult.Tables[0], "nSid");
                foreach (DataRow dr in _dsResult.Rows) //循环标本号
                {
                    strTestTime = DateTime.Parse(dr["dDate"].ToString()).ToString("yyyy-MM-dd") + " " + DateTime.Now.ToString("HH:mm:ss"); ;
                    strSampleNo = dr["nSid"].ToString();
                    string TestResultValue = "";
                    foreach (DataRow dr1 in dsResult.Tables[0].Select("nSid='" + dr["nSid"].ToString() + "'"))//循环标本号里面的检验项目
                    {
                        string _channelNo = dr1["sItem"].ToString();
                        FindRow = tItemChannel.Select("通道编码='" + _channelNo.Trim() + "'");
                        if (FindRow.Length == 0) //无普通结果则查找图像能道，无图像通道则更新通道类型为空
                        {
                            ChannelType = null;
                            writeLog.Write(strDevice, "未设置通道：" + _channelNo, "log");

                        }
                        else
                        {
                            testItemID = FindRow[0]["项目id"].ToString();
                            ChannelType = "0"; //普通结果
                            TestResultValue = TestResultValue + testItemID + "^" + dr1["Result"].ToString() + "|";
                        }
                    }
                    TestResultValue = strTestTime + "|" + strSampleNo + "^" + strSampleType + "^" + strBarCode + "|" + strOperator + "|" + StrSpecimen + "|" + "|" + TestResultValue;
                    try
                    {
                        saveResult.SaveTextResult(strInstrumentID, TestResultValue, TestGraph, null);
                        saveResult.UpdateData();
                        writeLog.Write(strDevice, "解析结果： " + TestResultValue, "result");
                    }
                    catch (Exception ex)
                    {
                        writeLog.Write(strDevice, "保存失败： " + ex.ToString(), "log");
                    }
                    //System.Windows.Forms.MessageBox.Show(TestResultValue);
                }
            }
            catch (Exception exp1)
            {
                writeLog.Write(strDevice, "处理失败： " + exp1.ToString(), "log");
            }
        }

        /// <summary>
        /// 筛选DataTable不重复的列
        /// </summary>
        /// <param name="SourceTable">源数据DataTable</param>
        /// <param name="keyFields">筛选不重复字段</param>
        /// <returns></returns>t
        public static DataTable SelectDistinct(DataTable SourceTable, string keyFields)
        {
            DataTable dtRet = SourceTable.Clone();//定义返回记录表
            StringBuilder sRet = new StringBuilder();//定义比较对象
            //获得参照列列表
            string[] sFields = keyFields.Split(',');//获得参照列列表
            if (sFields.Length == 0)
                throw new ArgumentNullException("无参照列");
            int result = 0;//定义循环变量
            string sLastValue = "";//定义对照值
            SourceTable.Select("", keyFields);//按参照列排序
            foreach (DataRow row in SourceTable.Rows)//开始比对
            {
                sRet.Length = 0;
                for (result = 0; result < sFields.Length; result++)//将参照列依序组合到字符串中，以','分割，作为比较对象
                    sRet.Append(row[sFields[result]]).Append(",");
                result = string.Compare(sRet.ToString(), sLastValue, true);//进行比较并判断比较结果
                switch (result)
                {
                    case 0://相同则放弃
                        break;
                    case -1://不同则加入，并将当前比较字符串赋给对照值
                    case 1:
                        dtRet.Rows.Add(row.ItemArray);
                        sLastValue = sRet.ToString();
                        break;
                }
            }
            return dtRet;
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

        /// <summary>
        /// MSH|检验日期|样本号^条码号^样本类型<13>
        /// OBX|通道号^结果值<13>
        /// </summary>
        public void GetResult1(object a)
        {
            //writeLog.Write("2" + detype + " " + ds.Tables[0].Rows.Count.ToString(), "log");
            CallInterFaceDll.boolRun = true;
            try
            {
                if (Convert.ToInt32(ds.Tables[0].Rows.Count.ToString()) == 0) return;
                if (bnHPV)
                {
                    FileInfo fileInfo;
                    foreach (DataRow drRow in ds.Tables[0].Select("检测日期='" + DateTime.Now.ToString("yyyy/M/dd") + "'"))
                    {
                        strSampleNO = drRow[0].ToString().Substring(6, 3);
                        dsGuid = dsHandle.GetDataSet("to_char(sysdate,'HH24:mi:ss') time", "dual", "");
                        resultstring = Convert.ToDateTime(drRow[11].ToString()).ToString("yyyy-MM-dd") + " " + dsGuid.Tables[0].Rows[0][0].ToString() + "|";
                        resultstring += Convert.ToInt32(strSampleNO).ToString() + "^^" + "|";
                        resultstring += "|";
                        resultstring += "|";
                        resultstring += "|";

                        dsGuid = dsHandle.GetDataSet("test_item_id,channel_no", "test_instrument_item_channel", "instrument_id = '" + strInstrumentID + "'");
                        strResult = drRow[14].ToString();
                        foreach (DataRow dr in dsGuid.Tables[0].Rows)
                        {
                            strTestItemID = dr[0].ToString();
                            strChannel = dr[1].ToString().ToUpper();
                            if (strResult.IndexOf(strChannel + " ") >= 0) resultstring += (strTestItemID + "^+" + "|");
                            else resultstring += (strTestItemID + "^-" + "|");
                        }
                        //c:Program Files\基因芯片图像分析软件\image
                        //c:Program Files\基因芯片图像分析软件\temp
                        TestGraph = new List<string>();
                        //fileInfo = new FileInfo(@"c:Program Files\基因芯片图像分析软件\temp\" + strSampleNO + ".bmp");
                        //if (fileInfo.Exists)
                        //{   //文件存在
                        //    TestGraph.Add(fileInfo.FullName);
                        //    //writeLog.Write("log", fileInfo.FullName);
                        //}
                        //else
                        //{
                        fileInfo = new FileInfo(@"c:Program Files\基因芯片图像分析软件\image\" + Convert.ToInt32(DateTime.Now.ToString("yyMMdd")).ToString() + strSampleNO + ".bmp");
                        if (fileInfo.Exists)
                        {   //文件存在
                            TestGraph.Add(fileInfo.FullName);
                            //writeLog.Write("log", fileInfo.FullName);
                        }
                        //}

                        saveResult.SaveTextResult(strInstrumentID, resultstring, TestGraph, null);
                        saveResult.UpdateData();
                    }
                }
                else if (bnSLAN)
                {
                    foreach (DataRow drRow in ds.Tables[0].Rows)
                    {
                        strChannel = drRow[1].ToString();
                        strResult = drRow[20].ToString();

                        dsGuid = dsHandle.GetDataSet("to_char(sysdate,'HH24:mi:ss') time", "dual", "");
                        resultstring = Convert.ToDateTime(drRow[19].ToString()).ToString("yyyy-MM-dd") + " " + dsGuid.Tables[0].Rows[0][0].ToString() + "|";
                        resultstring += Convert.ToInt32(drRow[2].ToString()).ToString() + "^^" + "|";
                        resultstring += "|";
                        resultstring += "|";
                        resultstring += "|";

                        dsGuid = dsHandle.GetDataSet("test_item_id,channel_no", "test_instrument_item_channel", "channel_no = '" + strChannel + "' and instrument_id = '" + strInstrumentID + "'");
                        if (dsGuid.Tables[0].Rows.Count == 0)
                        {
                            writeLog.Write("未设置通道：" + strChannel, "log");
                            continue;
                        }
                        else
                        {
                            strTestItemID = dsGuid.Tables[0].Rows[0]["test_item_id"].ToString();
                        }
                        resultstring = resultstring + strTestItemID + "^" + strResult + "|";

                        saveResult.SaveTextResult(strInstrumentID, resultstring, TestGraph, null);
                        saveResult.UpdateData();
                    }
                }
                else
                {
                    if (detype == "1")
                    {
                        for (int i = 0; i < Convert.ToInt32(ds.Tables[0].Rows.Count.ToString()); i++)
                        {
                            if (boolSlan)
                            {
                                strSampleNO = ds.Tables[0].Rows[i][2].ToString().Substring(7);
                                //已经审核的标本不用处理
                                //dsGuid = dsHandle.GetDataSet("status", "test_sample", "instrument_id='" + strInstrumentID + "' and sample_no = '" + strSampleNO + "'");
                                //if (Convert.ToInt32(dsGuid.Tables[0].Rows[0]["status"].ToString()) > 7) continue;

                                strChannel = ds.Tables[0].Rows[i][1].ToString();
                                dsGuid = dsHandle.GetDataSet("test_item_id", "test_instrument_item_channel", "channel_no = '" + strChannel + "' and instrument_id = '" + strInstrumentID + "'");
                                if (dsGuid.Tables[0].Rows.Count == 0)
                                {
                                    writeLog.Write("未设置通道：" + strChannel, "log");
                                    continue;
                                }
                                else
                                {
                                    strTestItemID = dsGuid.Tables[0].Rows[0]["test_item_id"].ToString();
                                }

                                strResult = ds.Tables[0].Rows[i][20].ToString();
                                try
                                {
                                    strResult = double.Parse(strResult).ToString();
                                }
                                catch (Exception ex)
                                {
                                    //
                                }

                                strResult = strTestItemID + "^" + strResult + "|";
                                resultstring = Convert.ToDateTime(ds.Tables[0].Rows[i][19].ToString()).ToString("yyyy-MM-dd HH:mm:ss") + "|" + strSampleNO + "^^|" + "|" + ds.Tables[0].Rows[i][10].ToString() + "|" + "|" + strResult;
                                saveResult.SaveTextResult(strInstrumentID, resultstring, TestGraph, null);
                            }
                            else if (bnAve763)
                            {
                                string strSampleNo;
                                resultstring = ds.Tables[0].Rows[i][0].ToString();
                                strSampleNo = resultstring.Split('|')[1].Trim().PadLeft(4, '0');
                                //已经审核或者发布的标本不再提取数据
                                dsGuid = dsHandle.GetDataSet("min(sample_status)", "test_sample", "income_time >= trunc(sysdate) and instrument_id = '" + strInstrumentID + "' and sample_no = '" + strSampleNo + "'");
                                if (dsGuid.Tables[0].Rows.Count > 0)
                                {
                                    if (dsGuid.Tables[0].Rows[0][0].ToString() == "8" || dsGuid.Tables[0].Rows[0][0].ToString() == "9") continue;
                                }
                                StringBuilder objStrBd = new StringBuilder(256);
                                string inifile = "SOLVESET.INI";
                                string FileName;
                                string strImagePosition = "";

                                //resString = new GetResultString();
                                resString.listInputResult = new List<string>();
                                resString.ListImagePosition = new List<string>();
                                //resString.listInputResult.Add(resultstring);
                                resString.ImmediatelyUpdate = true;
                                //inifile = System.AppDomain.CurrentDomain.BaseDirectory.ToString() + inifile;

                                FileInfo fileInfo = new FileInfo(inifile);
                                if ((!fileInfo.Exists))
                                {   //文件不存在
                                    return;
                                }
                                //必须是完全路径，不能是相对路径
                                FileName = fileInfo.FullName;
                                long OpStation = GetPrivateProfileString("EQUIPMENT", "IMAGE", "", objStrBd, 256, FileName);
                                if (OpStation > 0)
                                {
                                    FileName = objStrBd.ToString();
                                    long lngNo = 0;
                                    //判断H0001.jpg是否存在
                                    fileInfo = new FileInfo(FileName + @"\" + DateTime.Parse(resultstring.Split('|')[0].ToString()).ToString("yyyyMMdd") + @"\" + strSampleNo + @"\H0002.jpg");
                                    if (fileInfo.Exists)
                                    {   //文件存在
                                        strImagePosition = strImagePosition + ";" + fileInfo.FullName;
                                        lngNo += 1;
                                    }
                                    //判断H0002.jpg是否存在
                                    fileInfo = new FileInfo(FileName + @"\" + DateTime.Parse(resultstring.Split('|')[0].ToString()).ToString("yyyyMMdd") + @"\" + strSampleNo + @"\H0003.jpg");
                                    if (fileInfo.Exists)
                                    {   //文件存在
                                        strImagePosition = strImagePosition + ";" + fileInfo.FullName;
                                        resString.ListImagePosition.Add(strImagePosition.Substring(1));
                                        lngNo += 1;
                                    }
                                    //if (lngNo < 2)
                                    //{
                                    //    string[] strFiles = Directory.GetFiles(FileName + @"\" + DateTime.Parse(resultstring.Split('|')[0].ToString()).ToString("yyyyMMdd") + @"\" + strSampleNo, "*.jpg");
                                    //    foreach (string name in strFiles)
                                    //    {
                                    //        if (string.IsNullOrEmpty(name)) continue;
                                    //        lngNo += 1;
                                    //        if (lngNo <= 2)
                                    //        {
                                    //            strImagePosition = strImagePosition + ";" + name;
                                    //            if (lngNo == 2)
                                    //            {
                                    //                resString.ListImagePosition.Add(strImagePosition.Substring(1));
                                    //                break;
                                    //            }
                                    //        }
                                    //    }
                                    //    //if (lngNo < 2)
                                    //    //{
                                    //    //    resString.ListImagePosition.Add("");
                                    //    //    resString.ListImagePosition.Add("");
                                    //    //}
                                    //}
                                }
                                resString.listInputResult.Add(resultstring);
                                resString.ParseResult();
                                //resString = null;
                            }
                            else
                            {
                                resultstring = ds.Tables[0].Rows[i][0].ToString();
                                //writeLog.Write(resultstring, "log");
                                resString.listInputResult.Add(resultstring);
                            }
                        }
                    }
                    else if (detype == "2")
                    {
                        string sample_old = "***";
                        string sample_new;
                        for (int i = 0; i < Convert.ToInt32(ds.Tables[0].Rows.Count.ToString()); i++)
                        {
                            sample_new = ds.Tables[0].Rows[i][1].ToString().Trim();
                            //新的样本号或者结束
                            if (sample_new != sample_old)
                            {
                                if (!string.IsNullOrEmpty(resultstring))
                                {
                                    resString.listInputResult.Add(resultstring);
                                    resString.ParseResult();
                                    resultstring = "";
                                }

                                if (ds.Tables[0].Rows[i][0] != null) resultstring = "MSH|" + (ds.Tables[0].Rows[i][0].ToString().Substring(0, 6).IndexOf('-') > 0 ? ds.Tables[0].Rows[i][0].ToString().Substring(2) : ds.Tables[0].Rows[i][0].ToString()) + "|";         //检验日期
                                if (ds.Tables[0].Rows[i][1] != null) resultstring = resultstring + Convert.ToInt32(ds.Tables[0].Rows[i][1].ToString()) + "^";   //样本号
                                if (ds.Tables[0].Rows[i][2] != null) resultstring = resultstring + ds.Tables[0].Rows[i][2].ToString() + "^";   //条码号
                                if (ds.Tables[0].Rows[i][3] != null) resultstring = resultstring + ds.Tables[0].Rows[i][3].ToString() + "|";   //样本类型
                                resultstring = resultstring + (char)13;
                                resultstring = resultstring + "OBX|";
                                if (ds.Tables[0].Rows[i][6] != null) resultstring = resultstring + ds.Tables[0].Rows[i][6].ToString() + "^";   //通道号
                                if (ds.Tables[0].Rows[i][7] != null) resultstring = resultstring + ds.Tables[0].Rows[i][7].ToString() + "|";   //结果值
                                resultstring = resultstring + (char)13;

                                sample_old = sample_new;
                            }
                            else
                            {
                                resultstring = resultstring + "OBX|";
                                if (ds.Tables[0].Rows[i][6] != null) resultstring = resultstring + ds.Tables[0].Rows[i][6].ToString() + "^";   //通道号
                                if (ds.Tables[0].Rows[i][7] != null) resultstring = resultstring + ds.Tables[0].Rows[i][7].ToString() + "|";   //结果值
                                resultstring = resultstring + (char)13;
                            }
                            if (i + 1 == Convert.ToInt32(ds.Tables[0].Rows.Count.ToString()) && !string.IsNullOrEmpty(resultstring))
                            {
                                resString.listInputResult.Add(resultstring);
                                resultstring = "";
                            }
                        }
                    }
                }
                if (boolSlan)
                {
                    saveResult.UpdateData();
                    saveResult = null;
                }
                else if (!bnAve763 && !bnHPV)
                {

                }
                else
                {
                    resString.ParseResult();
                }
                resString = null;
                //conDB.Close();
                TestGraph = null;
            }
            catch (Exception ex)
            {
                writeLog.Write(ex.Message, "log");
            }
            finally
            {
                CallInterFaceDll.boolRun = false;
            }
            conDB.Close();
        }
    }
}
