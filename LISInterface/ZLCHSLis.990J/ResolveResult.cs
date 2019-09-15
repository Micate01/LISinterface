/*************************************
 * 名称：南方South990J解析程序
 * 功能：查看血粘度
 * 作者：谢天
 * 时间：2015-11-05
 * 通讯类型：Access数据库
 * 备注:需要32位操作系统才可以,access2003不支持64位
 * ***********************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using ZLCHSLisComm;
using System.Data;
using System.Data.OleDb;
using System.Data.OracleClient;
using System.IO;

namespace ZLCHSLis.south990J
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
            throw new NotImplementedException();
        }

        public void ParseResult(string strSource, ref string strResult, ref string strReserved, ref string strCmd)
        {
            string file = "south990J样本号重复处理.txt";
            int a = 2;
            string temp = strSource;
            //连接access数据库
            //配置文件中读取密码,查询语句          
            //读取配置文件
            //a1,a2,a3,a4  仪器通道码和his通道码一致  
            StreamReader sr = new StreamReader(@".\south990J.txt", System.Text.Encoding.GetEncoding("GB2312"));
            string bounds = sr.ReadLine();
            string configPath = sr.ReadLine();           
            sr.Close();
             OleDbConnection sConn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + configPath + ";Persist Security Info=False;Jet OLEDB:Database Password=iamysh");         
            string sqlMessage = null;
            try
            {                
                sConn.Open();               
                writelog.Write(strDevice, "处理进度： " + "连接成功", "log");
            }
            catch (Exception ex)
            {
                writelog.Write(strDevice, "处理失败： " + ex.ToString(), "log");
            }
            string dateTime = DateTime.Now.ToString("yyyy-MM-dd");
            sqlMessage = "select * from mainsj  where a55= '" + dateTime+'\'';
            OleDbCommand sCmd = new OleDbCommand(sqlMessage, sConn);
            OleDbDataAdapter myda = new OleDbDataAdapter(sqlMessage, sConn);
            DataTable ds = new DataTable();
            DataSet ds_GraphChannel = new DataSet();
            DataSet dsGUID = new DataSet();
            DataSet dsTestItem = new DataSet();
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
               
               // IsUpdate(strSource);//重新检查是否有新数据
                
                DataSet dsResult = new DataSet();
                myda.Fill(dsResult);
              
                //tItemChannel得到数据集，提供样本号为后面判断是否有新数据
                tItemChannel = OracleHelper.GetDataTable(@"Select 通道编码, m.项目id, Nvl(小数位数, 2) As 小数位数, Nvl(换算比, 0) As 换算比, Nvl(加算值, 0) As 加算值, j.结果类型
                                                                                From 仪器检测项目 m, 检验项目 j
                                                                                Where m.项目id = j.项目id and m.仪器Id='" + strInstrument_id + "'");
                if (dsResult.Tables[0].Rows.Count == 0)//判断是否有新标本
                {
                    writelog.Write(strDevice, DateTime.Now.ToString() + "未检测到新数据！", "log");
                    return;
                }
             //   DataTable dt1 = SelectDistinct(dsResult.Tables[0], "ChemExamineID");
                DataTable dt1 = dsResult.Tables[0];
               
                string[] boundsArray = bounds.Split('|');
              
             
                foreach (DataRow dr in dt1.Rows) //循环标本号
                {
                    string time1 = "";
                    string no1 = "";
                    StringBuilder sBuilder = new StringBuilder("");                  
                    strTestTime = DateTime.Parse(dr["a55"].ToString()).ToString("yyyy-MM-dd");
                    strSampleNo = dr["a1"].ToString();
                    time1 = strTestTime;
                    no1 = strSampleNo;
                    writelog.Write(strDevice, "处理失败： " + strTestTime + strSampleNo, "log");

                    for (int kl = 0; kl < boundsArray.Length; kl++)
                    {
                        sBuilder.Append(boundsArray[kl] + ',' + dr[boundsArray[kl]] + '|');
                    }
                    string TestResultValue = "";
                    if (Helper.CompareSampleNoAndTime(file, no1, time1))
                    {
                        continue;
                    }
                    string str = sBuilder.ToString().Remove(sBuilder.Length - 1, 1);
                    string[] strs = str.Split('|');                   
                    for (int i = 0; i < strs.Length; i++)
                    {

                        FindRow = tItemChannel.Select("通道编码='" + strs[i].Split(',')[0] + "'");
                        if (FindRow.Length == 0) //无普通结果则查找图像能道，无图像通道则更新通道类型为空
                        {
                            ChannelType = null;
                            writelog.Write(strDevice, "未设置通道：" + strs[i].Split(',')[0], "log");
                        }
                        else
                        {
                            testItemID = FindRow[0]["项目id"].ToString();
                            ChannelType = "0"; //普通结果
                            TestResultValue = TestResultValue + testItemID + "^" + strs[i].Split(',')[1] + "|";
                        }

                    }

                    TestResultValue = strTestTime + "|" + strSampleNo + "^" + strSampleType + "^" + strBarCode + "|" + strOperator + "|" + StrSpecimen + "|" + "|" + TestResultValue;

                    try
                    {
                        saveResult.SaveTextResult(strInstrument_id, TestResultValue, TestGraph, null);
                        saveResult.UpdateData();
                        writelog.Write(strDevice, "解析结果： " + TestResultValue, "result");
                    }
                    catch (Exception ex)
                    {
                        writelog.Write(strDevice, "保存失败： " + ex.ToString(), "log");
                    }
                }
            }
            catch (Exception exp1)
            {
                writelog.Write(strDevice, "处理失败： " + exp1.ToString(), "log");
            }
            finally
            {
                if (sConn != null) sConn.Close(); sConn = null;
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
