/*************************************

 * ***********************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;
using ZLCHSLisComm;
using System.Collections;
using System.IO;
using System.Xml;
using System.Data.OracleClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace ZLCHSLis.BC5300
{
    public class ResolveResult : IDataResolve
    {
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
        //DataTable dt = new DataTable();
        //DataSet ds = new DataSet();
        Dictionary<String, String> _DIC = new Dictionary<String, String>();
        public void ParseResult()
        {
            throw new NotImplementedException();
        }

        public void ParseResult(string strSource, ref string strResult, ref string strReserved, ref string strCmd)
        {
           

            try
            {

                string wbc = ""; //白细胞数目
                string neu = ""; //中性粒细胞百分比
                string neu2 = "";//中性粒细胞数目
                string bas = ""; //嗜碱性粒细胞百分比
                string bas2 = "";//嗜碱性粒细胞数目
                string eos = ""; //嗜酸性粒细胞百分比
                string eos2 = "";//嗜酸性粒细胞数目
                string lym = ""; //淋巴细胞百分比
                string lym2 = ""; //淋巴细胞数目
                string mon = ""; //单核细胞百分比
                string mon2 = ""; //单核细胞数目
                string rbc = ""; //红细胞数目
                string hgb = ""; //血红蛋白浓度
                string mcv = ""; //平均红细胞体积
                string mch = ""; //平均红细胞血红蛋白含量
                string mchc = ""; //平均红细胞血红蛋白浓度
                string rdw_cv = "";//红细胞分布宽度变异系数
                string rdw_sd = ""; //红细胞分布宽度标准差
                string hct = ""; //红细胞压积
                string plt = ""; //血小板数目
                string mpv = ""; //平均血小板体积
                string pdw = ""; //血小板分布宽度
                string pct = "";//血小板压积

                if (strSource.Contains(((char)11).ToString()))
                {

                    string[] ss = strSource.Split(new char[] { '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    string ChannelCode = "";
                    for (int i = 0; i < ss.Length; i++)
                    {

                        if (ss[i].Split(new char[] { '|' })[0].Replace("\n", "").Equals("OBR"))
                        {
                            strSampleNo = ss[i].Split('|')[3];
                            string times = ss[i].Split('|')[7];
                            strTestTime = times.Substring(0, 4) + "-" + times.Substring(4, 2) + "-" + times.Substring(6, 2);
                        }
                        else if (ss[i].Split(new char[] { '|' })[0].Replace("\n", "").Equals("OBX"))
                        {
                            //OBX|6|NM|6690-2^WBC^LN||5.68|10*9/L|4.00-10.00|N|||F
                            if (ss[i].IndexOf("6690-2^WBC^LN") > 0)
                                wbc = ss[i].Split('|')[5];
                            //OBX|7|NM|704-7^BAS#^LN||0.01|10*9/L|0.00-0.10|N|||F
                            else if (ss[i].IndexOf("704-7^BAS#^LN") > 0)
                                bas2 = ss[i].Split('|')[5];
                            //OBX|8|NM|706-2^BAS%^LN||0.2|%|0.0-1.0|N|||F
                            else if (ss[i].IndexOf("706-2^BAS%^LN") > 0)
                                bas = ss[i].Split('|')[5];
                            //OBX|9|NM|751-8^NEU#^LN||2.92|10*9/L|2.00-7.00|N|||F
                            else if (ss[i].IndexOf("751-8^NEU#^LN") > 0)
                                neu = ss[i].Split('|')[5];
                            //OBX|10|NM|770-8^NEU%^LN||51.3|%|50.0-70.0|N|||F
                            else if (ss[i].IndexOf("770-8^NEU%^LN") > 0)
                                neu2 = ss[i].Split('|')[5];
                            //OBX|11|NM|711-2^EOS#^LN||0.14|10*9/L|0.02-0.50|N|||F
                            else if (ss[i].IndexOf("711-2^EOS#^LN") > 0)
                                eos = ss[i].Split('|')[5];
                            //OBX|12|NM|713-8^EOS%^LN||2.5|%|0.5-5.0|N|||F
                            else if (ss[i].IndexOf("713-8^EOS%^LN") > 0)
                                eos2 = ss[i].Split('|')[5];
                            //OBX|13|NM|731-0^LYM#^LN||2.32|10*9/L|0.80-4.00|N|||F
                            else if (ss[i].IndexOf("731-0^LYM#^LN") > 0)
                                lym = ss[i].Split('|')[5];
                            //OBX|14|NM|736-9^LYM%^LN||40.8|%|20.0-40.0|H|||F
                            else if (ss[i].IndexOf("736-9^LYM%^LN") > 0)
                                lym2 = ss[i].Split('|')[5];
                            //OBX|15|NM|742-7^MON#^LN||0.29|10*9/L|0.12-1.20|N|||F
                            else if (ss[i].IndexOf("742-7^MON#^LN") > 0)
                                mon = ss[i].Split('|')[5];
                            //OBX|16|NM|5905-5^MON%^LN||5.2|%|3.0-12.0|N|||F
                            else if (ss[i].IndexOf("5905-5^MON%^LN") > 0)
                                mon2 = ss[i].Split('|')[5];
                            else  if (ss[i].IndexOf("789-8^RBC^LN") > 0)
                                rbc = ss[i].Split('|')[5];
                            //OBX|22|NM|718-7^HGB^LN||114|g/L|110-150|N|||F
                            else if (ss[i].IndexOf("718-7^HGB^LN") > 0)
                                hgb = ss[i].Split('|')[5];
                            //OBX|23|NM|787-2^MCV^LN||87.5|fL|80.0-100.0|N|||F
                            else if (ss[i].IndexOf("787-2^MCV^LN") > 0)
                                mcv = ss[i].Split('|')[5];
                            //OBX|24|NM|785-6^MCH^LN||30.4|pg|27.0-34.0|N|||F
                            else if (ss[i].IndexOf("785-6^MCH^LN") > 0)
                                mch = ss[i].Split('|')[5];
                            //OBX|25|NM|786-4^MCHC^LN||347|g/L|320-360|N|||F
                            else if (ss[i].IndexOf("786-4^MCHC^LN") > 0)
                                mchc = ss[i].Split('|')[5];
                            //OBX|26|NM|788-0^RDW-CV^LN||10.8|%|11.0-16.0|L|||F
                            else if (ss[i].IndexOf("788-0^RDW-CV^LN") > 0)
                                rdw_cv = ss[i].Split('|')[5];
                            //OBX|27|NM|21000-5^RDW-SD^LN||39.9|fL|35.0-56.0|N|||F
                            else if (ss[i].IndexOf("21000-5^RDW-SD^LN") > 0)
                                rdw_sd = ss[i].Split('|')[5];
                            //OBX|28|NM|4544-3^HCT^LN||32.8|%|37.0-47.0|L|||F
                            else if (ss[i].IndexOf("4544-3^HCT^LN") > 0)
                                hct = ss[i].Split('|')[5];
                            //OBX|29|NM|777-3^PLT^LN||299|10*9/L|100-300|N|||F
                            else if (ss[i].IndexOf("777-3^PLT^LN") > 0)
                                plt = ss[i].Split('|')[5];
                            //OBX|30|NM|32623-1^MPV^LN||8.9|fL|6.5-12.0|N|||F
                            else if (ss[i].IndexOf("32623-1^MPV^LN") > 0)
                                mpv = ss[i].Split('|')[5];
                            //OBX|31|NM|32207-3^PDW^LN||15.9||9.0-17.0|N|||F
                            else if (ss[i].IndexOf("32207-3^PDW^LN") > 0)
                                pdw = ss[i].Split('|')[5];
                            //OBX|32|NM|10002^PCT^99MRC||0.266|%|0.108-0.282|N|||F
                            else if (ss[i].IndexOf("10002^PCT^99MRC") > 0)
                                pct = ss[i].Split('|')[5];
                        }
                    }
                }

                string[] strs = ("wbc," + wbc + "|neu," + neu + "|neu2," + neu2 + "|bas," + bas + "|bas2," + bas2 + "|eos," + eos + "|eos2," + eos2 + "|lym," + lym + "|lym2," + lym2 + "|mon," + mon + "|mon2," + mon2 + "|rbc," + rbc + "|hgb," + hgb + "|mcv," + mcv + "|mch," + mch + "|mchc," + mchc + "|rdw_cv," + rdw_cv + "|rdw_sd," + rdw_sd + "|hct," + hct + "|plt," + plt + "|mpv," + mpv + "|pdw," + pdw + "|pct," + pct).Split('|');

                //DataTable dt = new DataTable();
                //DataSet ds = new DataSet();
                //dt = ds.Tables[0];
                string ChannelType = "";     //0-普通结果;1-直方图;2-散点图;3-直方图界标;4-散点图界标;5-BASE64
                string testItemID = "";
                string TestResultValue = "";
                for (int i = 0; i < strs.Length; i++)
                {
                    // string ChannelCode = dt.Rows[i]["ChannelCode"].ToString(); 
                    // string _channelNo = ChannelCode;
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
                    //dt = ds.Tables[0];
                    //TestResultValue = TestResultValue + "testItemID项目ID" + "^" + strs[i][1] + "|";
                }

                TestResultValue = strTestTime + "|" + strSampleNo + "^" + strSampleType + "^" + strBarCode + "|" + strOperator + "|" + StrSpecimen + "|" + "|" + TestResultValue;
                saveResult = new SaveResult();
                if (!string.IsNullOrEmpty(strSampleNo) || !string.IsNullOrEmpty(strBarCode))
                {
                    saveResult.SaveTextResult(strInstrument_id, TestResultValue, TestGraph, DrSampleNoField);
                    if (ImmediatelyUpdate)
                    {
                        saveResult.UpdateData();
                    }
                }
                // 2011-06-21|0007^0^||标本||62B16EF4-7F88-4498-BF82-C107EE528513^15  leu/uL|252260FD-EFFD-45C2-B79B-8309D678F20C^|A135CEC5-C8E2-4FF5-BEB1-BA0944093A59^Normal|D7F59AD1-4F9A-4995-A75B-1528FCC42448^|8F2E74E4-D1F0-47CD-97C5-EA35379F4CFC^0.6 mmol/L|869F1A2C-38A3-4613-91B5-A1EAD630FD0F^0      g/L|66E36E0A-B509-4FF2-B440-6B7D2D76CD24^80  ery/uL|54A2C7DF-1D92-4467-8FC0-C3642C170F37^5.0|4FEAF5E1-EDD4-4171-B0BA-FF1F2C6A6C9D^>1.030|16401182-86E3-4CE1-9302-3934311ED89B^0   mmol/L|F08FE403-B04D-4B34-8AA2-A5CF63453AB4^0   mmol/L|
                if (!ImmediatelyUpdate)
                {
                    saveResult.UpdateData();
                }
            }
            catch (Exception e)
            {
                writelog.Write(strDevice, "处理失败： " + e.ToString(), "log");
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
