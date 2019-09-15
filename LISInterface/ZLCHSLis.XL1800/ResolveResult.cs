using System;
using System.Collections.Generic;
using System.Text;
using ZLCHSLisComm;
using System.Data;
using System.Data.OleDb;
using System.Data.OracleClient;

namespace ZLCHSLis.XL1800
{
    
    public class ResolveResult : IDataResolve
    {
        #region 定义变量

        public string strInstrument_id;
        public string strSubBegin;  //多帧开始位
        public string strSubEnd;    //多帧结束位
        public string strDetype;    //解析方式 
        public string strDataBegin;  //数据开始位
        public string strDataEnd;                 //数据结束位
        //public string strBeginBits;              //多帧时开始位
        //public string strEndBits;                 //多帧时结束位
        public string strACK_all;                  //全部应答
        public string strACK_term;                 //条件应答
        public List<string> listInputResult = new List<string>();
        public List<string> ListImagePosition = new List<string>();              //图像存放位置
        public Boolean ImmediatelyUpdate = false;                               //立即更新

        DataSetHandle dsHandle = new DataSetHandle();
        Write_Log writelog = new Write_Log();
        SaveResult saveResult;

        string TestResultValue;      //解析后的通用字符串
        string strDevice;
        List<string> TestGraph; //图像列表

        DataRow DrTestTimeSignField;         //检验时间标识
        DataRow DrTestTimeField;             //检验时间
        DataRow DrSampleNoSignField;         //常规样本号标识
        DataRow DrSampleNoField;             //常规样本号
        DataRow DrBarCodeSignField;             //条码号标识
        DataRow DrBarCodeField;              //条码号
        DataRow DrSampleTypeSignField;       //样本类型标识
        DataRow DrSampleTypeField;           //样本类型
        DataRow DrOperatorSignField;       //检验人标识
        DataRow DrOperatorField;           //检验人
        DataRow DrSpecimenSignField;       //标本标识
        DataRow DrSpecimenField;           //标本
        DataRow DrResultSignField;           //结果标识
        DataRow DrResultInfoField;           //结果信息
        DataRow DrResultCountField;          //结果数
        DataRow DrSingleResultField;         //单个结果
        DataRow DrChannelField;              //通道号
        DataRow DrResultField;               //结果值
        DataRow DrQCSampleField;             //质控样本号
        DataTable VDT = new DataTable(); //参数集合
        int begin;
        int end;

        string strTestTime; //检验时间
        string strSampleNo;//标本号
        string strBarCode;  //条码
        string strOperator; //检验医师
        string strSampleType; //检验类型
        string StrSpecimen;   //标本类型
        string FilePath = "";
        Boolean ResultFlag;                 //结果开始标志
        string subString;                    //临时存放解析字符串
        DataRow[] FindRow;                  //解析设置
        DataSet ds_ItemChannel = new DataSet();
        DataTable tItemChannel = new DataTable();
        Dictionary<String, String> _DIC = new Dictionary<String, String>();
        string sqlstr;
        string strError;
        string conStr = null;
        DataSet ds = new DataSet();
        DataSet dsResult;
        OleDbConnection conDB = new OleDbConnection();

        #endregion
        public void ParseResult()
        {
        }

        /// <summary>
        ///     '设备标准接口
        //'功能：解析数据
        //'参数：
        //'strSource：要解析的原始串
        //'strResult：返回的检验结果(各仪器解析程序必须按以下标准组织结果)
        //'   每组检验结果以||分隔,元素之间以|分隔
        //'   第0个元素：检验时间
        //'   第1个元素：样本序号(标本号^条码号^样本类型)
        //'   第2个元素：检验人
        //'   第3个元素：标本
        //'   第4个元素：预留
        //'   从第5个元素开始为检验结果，每2个元素表示一个检验项目。
        //'       如：第5i个元素为检验项目，第5i+1个元素为检验结果
        //'strReserved：最后不能完成解析的原始串，需要返回作后续处理
        //'strCmd：如果需要，可返回向设备发送的命令
        //'补充图像的方式：
        //'                   1.图像数据跟随指标数据后，使用回车换行符来分隔。
        //'                   2.有多个图像数据时使用"^"来分隔
        //'                   3.单个图像数据格式: 图像画法 0=直方图  1=散点图
        //'                     a) 直方图: 图像名称;图像画法(0=直方图  1=散点图);X1;X2;X3;X4;X5...
        //'                     b) 散点图: 图像名称;图像画法(0=直方图  1=散点图):
        //'                        例:00000100001000010000100010;00000100001000010000100010;
        //'                        说明:1.散点图以点阵方式保存每一行使用分号来分隔.
        //'                             2.有多少个分号就有多少行
        //'                             3.每一行有多少个点由每一行的长度来确定
        //'                             3.画图的方向是从最上边向下画，如有65*65的图就是从65行开始画(最上边开始画)
        /// 检验日期|
        /// </summary>
        /// <param name="ResultString"></param>
        public void ParseResult(string strSource, ref string strResult, ref string strReserved, ref string strCmd)
        {
            string channel = "0";
            // string testItemID = "";
            string result = "";
            decimal CONVERSION_RATIO = 0;
            decimal DECIMAL_LEVEL = 2;
            string ParString = "";
            string SingleResult = "";
            string ResultString = "";
            string UnknownResult = ""; //未知项目
            string TempCmd = "";

            DataSet ds_GraphChannel = new DataSet();
            DataSet dsGUID = new DataSet();
            DataSet dsTestItem = new DataSet();

            string TestResultInfo = "";
            strReserved = "";
            strResult = "";
            strCmd = "";

            init();//初始化

            try
            {
                string strTestTime;             //检验时间
                string strSampleNo;          //标本号
                string strBarCode = "";        //条码
                string strOperator = "";       //检验医师
                string strSampleType = "";   //检验类型
                string StrSpecimen = "";      //标本类型
                string ChannelType;          //0-普通结果;1-直方图;2-散点图;3-直方图界标;4-散点图界标;5-BASE64
                string testItemID = "";       //通道项目ID
                DataRow[] FindRow;        //解析设置
                TestGraph = new List<string>();
                saveResult = new SaveResult();
                IsUpdate(strSource);//重新检查是否有新数据

                //获取样本序号:oracle
                string strSql = "select trim(wm_concat(样本序号)) 样本序号 from 检验记录 where 仪器ID = '" + strInstrument_id + "' and to_char(检验时间,'yyyy-mm-dd') = to_char(sysdate,'yyyy-mm-dd')";
                DataTable sampleNo = OracleHelper.GetDataTable(strSql);

                //判断是否有新的数据产生
                if (dsResult.Tables[0].Rows.Count == 0)
                {
                    writelog.Write(strDevice, DateTime.Now.ToShortDateString() + "未检测到任何检验数据！", "log");
                    return;
                }
                else
                {
                    for (int i = 0; i < dsResult.Tables[0].Rows.Count; i++)
                    {
                        strSampleNo = dsResult.Tables[0].Rows[i]["SAMPLE_ID"].ToString();
                        string _channelNo = "";
                        if (i == dsResult.Tables[0].Rows.Count - 1)
                        {
                            strTestTime = dsResult.Tables[0].Rows[i]["TIME"].ToString();
                            _channelNo = dsResult.Tables[0].Rows[i]["ITEM"].ToString();
                            FindRow = tItemChannel.Select("通道编码='" + _channelNo.Trim() + "'");
                            if (FindRow.Length == 0) //无普通结果则查找图像能道，无图像通道则更新通道类型为空
                            {
                                ChannelType = null;
                                writelog.Write(strDevice, "未设置通道：" + _channelNo, "log");
                            }
                            else
                            {
                                testItemID = FindRow[0]["项目id"].ToString();
                                ChannelType = "0"; //普通结果
                                TestResultValue = TestResultValue + testItemID + "^" + dsResult.Tables[0].Rows[i]["RESULT"].ToString() + "|";
                            }
                            TestResultValue = strTestTime + "|" + strSampleNo + "^" + strSampleType + "^" + strBarCode + "|" + strOperator + "|" + StrSpecimen + "|" + "|" + TestResultValue;
                            try
                            {
                                saveResult.SaveTextResult(strInstrument_id, TestResultValue, TestGraph, null);
                                saveResult.UpdateData();
                                writelog.Write(strDevice, "解析结果： " + TestResultValue, "result");
                                TestResultValue = "";
                            }
                            catch (Exception ex)
                            {
                                writelog.Write(strDevice, "保存失败： " + ex.ToString(), "log");
                            }
                        }
                        else
                        {
                            if (dsResult.Tables[0].Rows[i + 1]["SAMPLE_ID"].ToString().Equals(dsResult.Tables[0].Rows[i]["SAMPLE_ID"].ToString()))
                            {
                                _channelNo = dsResult.Tables[0].Rows[i]["ITEM"].ToString();
                                FindRow = tItemChannel.Select("通道编码='" + _channelNo.Trim() + "'");
                                if (FindRow.Length == 0) //无普通结果则查找图像能道，无图像通道则更新通道类型为空
                                {
                                    ChannelType = null;
                                    writelog.Write(strDevice, "未设置通道：" + _channelNo, "log");
                                }
                                else
                                {
                                    testItemID = FindRow[0]["项目id"].ToString();
                                    ChannelType = "0"; //普通结果
                                    TestResultValue = TestResultValue + testItemID + "^" + dsResult.Tables[0].Rows[i]["RESULT"].ToString() + "|";
                                }
                                //间接胆红素
                                if (dsResult.Tables[0].Rows[i]["ITEM"].ToString().Contains("DBIL"))
                                {
                                    FindRow = tItemChannel.Select("通道编码='IBILI'");
                                    if (FindRow.Length == 0) //无普通结果则查找图像能道，无图像通道则更新通道类型为空
                                    {
                                        ChannelType = null;
                                        writelog.Write(strDevice, "未设置通道：" + _channelNo, "log");
                                    }
                                    else
                                    {
                                        testItemID = FindRow[0]["项目id"].ToString();
                                        ChannelType = "0"; //普通结果                                    
                                        result = (Convert.ToDouble(dsResult.Tables[0].Rows[i - 1]["RESULT"].ToString()) - Convert.ToDouble(dsResult.Tables[0].Rows[i]["RESULT"].ToString())).ToString();
                                        TestResultValue = TestResultValue + testItemID + "^" + result + "|";
                                    }
                                }
                                //球蛋白
                                if (dsResult.Tables[0].Rows[i]["ITEM"].ToString().Contains("ALB"))
                                {
                                    FindRow = tItemChannel.Select("通道编码='GLB'");
                                    if (FindRow.Length == 0) //无普通结果则查找图像能道，无图像通道则更新通道类型为空
                                    {
                                        ChannelType = null;
                                        writelog.Write(strDevice, "未设置通道：GLB", "log");
                                    }
                                    else
                                    {
                                        testItemID = FindRow[0]["项目id"].ToString();
                                        ChannelType = "0"; //普通结果
                                        result = (Convert.ToDouble(dsResult.Tables[0].Rows[i - 1]["RESULT"].ToString()) - Convert.ToDouble(dsResult.Tables[0].Rows[i]["RESULT"].ToString())).ToString();
                                        TestResultValue = TestResultValue + testItemID + "^" + result + "|";
                                    }
                                    #region 白球比
                                    FindRow = tItemChannel.Select("通道编码='A/G'");
                                    if (FindRow.Length == 0) //无普通结果则查找图像能道，无图像通道则更新通道类型为空
                                    {
                                        ChannelType = null;
                                        writelog.Write(strDevice, "未设置通道：A/G", "log");
                                    }
                                    else
                                    {
                                        testItemID = FindRow[0]["项目id"].ToString();
                                        ChannelType = "0"; //普通结果
                                        result = Math.Round(Convert.ToDouble(dsResult.Tables[0].Rows[i - 1]["RESULT"].ToString()) / Convert.ToDouble(dsResult.Tables[0].Rows[i]["RESULT"].ToString()), 1).ToString();
                                        TestResultValue = TestResultValue + testItemID + "^" + result + "|";
                                    }
                                    #endregion
                                }

                            }
                            else
                            {
                                strTestTime = dsResult.Tables[0].Rows[i]["TIME"].ToString();
                                _channelNo = dsResult.Tables[0].Rows[i]["ITEM"].ToString();
                                FindRow = tItemChannel.Select("通道编码='" + _channelNo.Trim() + "'");
                                if (FindRow.Length == 0) //无普通结果则查找图像能道，无图像通道则更新通道类型为空
                                {
                                    ChannelType = null;
                                    writelog.Write(strDevice, "未设置通道：" + _channelNo, "log");
                                }
                                else
                                {
                                    testItemID = FindRow[0]["项目id"].ToString();
                                    ChannelType = "0"; //普通结果
                                    TestResultValue = TestResultValue + testItemID + "^" + dsResult.Tables[0].Rows[i]["RESULT"].ToString() + "|";
                                }
                                TestResultValue = strTestTime + "|" + strSampleNo + "^" + strSampleType + "^" + strBarCode + "|" + strOperator + "|" + StrSpecimen + "|" + "|" + TestResultValue;
                                try
                                {
                                    saveResult.SaveTextResult(strInstrument_id, TestResultValue, TestGraph, null);
                                    saveResult.UpdateData();
                                    writelog.Write(strDevice, "解析结果： " + TestResultValue, "result");
                                    TestResultValue = "";
                                }
                                catch (Exception ex)
                                {
                                    writelog.Write(strDevice, "保存失败： " + ex.ToString(), "log");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                writelog.Write(strDevice, "处理失败： " + e.ToString(), "log");
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void init()
        {
            TestResultValue = null;      //解析后的通用字符串
            TestGraph = new List<string>(); //图像列表
            begin = -1;
            end = -1;
            strTestTime = null;
            strSampleNo = null;
            strBarCode = null;
            strOperator = null;
            strSampleType = null;
            StrSpecimen = null;
            subString = null;
            ResultFlag = false;
        }

        /// <summary>
        /// 检查是否有更新
        /// </summary>
        public void IsUpdate(string strSource)
        {
            ds = dsHandle.GetDataSet(@"解析类型,通讯程序名,
                                                        Extractvalue(通讯参数, '/root/db_type') as db_type,
                                                        Extractvalue(通讯参数, '/root/db_name') as db_name,
                                                        Extractvalue(通讯参数, '/root/user_name') as user_name,
                                                        Extractvalue(通讯参数, '/root/password') as password,
                                                        Extractvalue(通讯参数, '/root/server_name') as server_name,
                                                        Extractvalue(通讯参数, '/root/parastr') as parastr,
                                                        Extractvalue(通讯参数, '/root/selectstr') as selectstr", "检验仪器", "id = '" + strInstrument_id + "'");
            sqlstr = ds.Tables[0].Rows[0]["selectstr"].ToString();
            try
            {
                conStr = strSource.Split('|')[1].ToString();
                conDB.ConnectionString = conStr;
                conDB.Open();
            }
            catch (Exception e)
            {
                //conDB.Close();
                writelog.Write("数据库连接失败！" + e.Message, "log");
                strError = "数据库连接失败！" + e.Message;
                return;
            }
            if (conDB.State != ConnectionState.Open) return;
            strDevice = dsHandle.GetDataSet("名称", "检验仪器", "id= '" + strInstrument_id + "'").Tables[0].Rows[0]["名称"].ToString();//取仪器名称
            DataTable dtSample = OracleHelper.GetDataTable(@"select   wm_concat(样本序号) 样本序号
                                                                                          from 检验记录
                                                                                          where id in (select 记录id from 检验普通结果 where 结果标志 is not null)
                                                                                                   and 仪器id = '" + strInstrument_id + @"'
                                                                                                   and to_char(核收时间, 'yyyy-mm-dd') = to_char(sysdate, 'yyyy-mm-dd')");
            string _sampleNo = dtSample.Rows[0]["样本序号"].ToString();
            if (string.IsNullOrEmpty(sqlstr))
            {
                writelog.Write(strDevice, "请在BH中设置相应的SQL语句；");
                return;
            }
            sqlstr = sqlstr.Replace("parameter", _sampleNo == "" ? "'" + -1 + "'" : "'" + _sampleNo + "'");//替换样本号
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

        public System.Drawing.Image LocalIMG(string IMG)
        {
            throw new NotImplementedException();
        }

        public string GetCmd(string dataIn, string ack_term)
        {
            throw new NotImplementedException();
        }

        public void GetRules(string StrDevice)
        {
            DataSet dsTestItem = new DataSet();
            DataSet dsRules = new DataSet();
            strInstrument_id = StrDevice;

            DrTestTimeSignField = null;         //检验时间标识
            DrTestTimeField = null;             //检验时间
            DrSampleNoSignField = null;
            DrSampleNoField = null;
            DrBarCodeSignField = null;
            DrBarCodeField = null;
            DrSampleTypeSignField = null;
            DrSampleTypeField = null;
            DrOperatorSignField = null;
            DrOperatorField = null;
            DrSpecimenSignField = null;
            DrSpecimenField = null;

            DrResultSignField = null;
            DrResultInfoField = null;
            DrResultCountField = null;
            DrSingleResultField = null;
            DrChannelField = null;
            DrResultField = null;

            dsRules = dsHandle.GetDataSet(@"Extractvalue(Column_Value, '/item/item_code') As item_code, Extractvalue(Column_Value, '/item/separated_first') As separated_first, 
                                                   Extractvalue(Column_Value, '/item/no_first') As no_first, Extractvalue(Column_Value, '/item/separated_second') As separated_second,
                                                   Extractvalue(Column_Value, '/item/no_second') As no_second, Extractvalue(Column_Value, '/item/start_bits') As start_bits,
                                                   Extractvalue(Column_Value, '/item/length') As length, Extractvalue(Column_Value, '/item/sign') As sign,Extractvalue(Column_Value, '/item/format') As format",
                                                   "Table(Xmlsequence(Extract((Select 解析规则 From 检验仪器 Where ID = '" + strInstrument_id + "'), '/root/item'))) ", "");
            //检验指标通道            
            //ds_ItemChannel = dsHandle.GetDataSet("通道编码,项目id,nvl(小数位数,2) as 小数位数,nvl(换算比,0) as 换算比", "仪器检测项目", "仪器id = '" + strInstrument_id + "'");
            tItemChannel = OracleHelper.GetDataTable(@"Select 通道编码, m.项目id, Nvl(小数位数, 2) As 小数位数, Nvl(换算比, 0) As 换算比, Nvl(加算值, 0) As 加算值, j.结果类型
                                        From 仪器检测项目 m, 检验项目 j
                                        Where m.项目id = j.项目id and m.仪器Id='" + StrDevice + "'");
            ds_ItemChannel.CaseSensitive = true;
            //检验图像通道
            //ds_GraphChannel = dsHandle.GetDataSet("CHANNEL_NO,GRAPH_TYPE", "TEST_GRAPH_CHANNEL", "instrument_id = '" + strInstrument_id + "'");
            //ds_GraphChannel.CaseSensitive = true;

            FindRow = dsRules.Tables[0].Select("item_code = '01'");         //检验日期标识
            if (FindRow.Length != 0) DrTestTimeSignField = FindRow[0];
            FindRow = dsRules.Tables[0].Select("item_code = '02'");             //检验日期
            if (FindRow.Length != 0) DrTestTimeField = FindRow[0];
            FindRow = dsRules.Tables[0].Select("item_code = '03'");       //常规样本号标识
            if (FindRow.Length != 0) DrSampleNoSignField = FindRow[0];
            FindRow = dsRules.Tables[0].Select("item_code = '04'");           //常规样本号
            if (FindRow.Length != 0) DrSampleNoField = FindRow[0];
            else
            {
                DrSampleNoField = null;
            }
            FindRow = dsRules.Tables[0].Select("item_code = '05'");      //质控样本号
            if (FindRow.Length != 0) DrQCSampleField = FindRow[0];
            FindRow = dsRules.Tables[0].Select("item_code = '06'");        //条码号标识
            if (FindRow.Length != 0) DrBarCodeSignField = FindRow[0];
            FindRow = dsRules.Tables[0].Select("item_code = '07'");        //条码号
            if (FindRow.Length != 0) DrBarCodeField = FindRow[0];
            FindRow = dsRules.Tables[0].Select("item_code = '08'");        //样本类型标识
            if (FindRow.Length != 0) DrSampleTypeSignField = FindRow[0];
            FindRow = dsRules.Tables[0].Select("item_code = '09'");        //样本类型
            if (FindRow.Length != 0) DrSampleTypeField = FindRow[0];
            FindRow = dsRules.Tables[0].Select("item_code = '10'");        //检验人标识
            if (FindRow.Length != 0) DrOperatorSignField = FindRow[0];
            FindRow = dsRules.Tables[0].Select("item_code = '11'");        //检验人
            if (FindRow.Length != 0) DrOperatorField = FindRow[0];
            FindRow = dsRules.Tables[0].Select("item_code = '12'");        //标本标识
            if (FindRow.Length != 0) DrSpecimenSignField = FindRow[0];
            FindRow = dsRules.Tables[0].Select("item_code = '13'");        //标本
            if (FindRow.Length != 0) DrSpecimenField = FindRow[0];

            FindRow = dsRules.Tables[0].Select("item_code = '14'");        //结果标识
            if (FindRow.Length != 0) DrResultSignField = FindRow[0];
            FindRow = dsRules.Tables[0].Select("item_code = '15'");        //结果信息
            if (FindRow.Length != 0) DrResultInfoField = FindRow[0];
            FindRow = dsRules.Tables[0].Select("item_code = '16'");        //结果数
            if (FindRow.Length != 0) DrResultCountField = FindRow[0];
            FindRow = dsRules.Tables[0].Select("item_code = '17'");      //单个结果
            if (FindRow.Length != 0) DrSingleResultField = FindRow[0];
            FindRow = dsRules.Tables[0].Select("item_code = '18'");        //通道号
            if (FindRow.Length != 0) DrChannelField = FindRow[0];
            FindRow = dsRules.Tables[0].Select("item_code = '19'");        //结果值
            if (FindRow.Length != 0) DrResultField = FindRow[0];
            FindRow = dsRules.Tables[0].Select("item_code = '20'");        //盘号
            //if (FindRow.Length != 0) Field_Row[4] = FindRow[0];
            FindRow = dsRules.Tables[0].Select("item_code = '21'");        //杯号
            //if (FindRow.Length != 0) Field_Row[5] = FindRow[0];

            strDevice = OracleHelper.GetDataTable("select 名称 from 检验仪器 where id='" + StrDevice + "'").Rows[0]["名称"].ToString();
        }

        public void SetVariable(System.Data.DataTable dt)
        {
            throw new NotImplementedException();
        }
    }
}