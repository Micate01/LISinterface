/*************************************
 * 名称：FIA8600
 * 功能：免疫定量分析
 * 作者：谢天
 * 时间：2017-4-12
 * 通讯类型：串口
 * COM1 波特率:9600   数据位:8   停止位:1   校验:无    控制流:无 
 * 备注:无需应答字符串
 * 新FIA8K与上位机之间的通信协议
波特率:9600  数据位：8位  停止位：1位  校验位：None
计算机只负责接受仪器上传的结果；接收44个字节；
上传结果格式如下：
起始节（ 3个字节）： 0x20+0x20+0xfd
有效字（39个字节）： •••(Vcbuf[38])
校验字（ 1个字节）： •••
结束字（ 1个字节）： 0xFC

有效字（39个字节）的详细说明：
1.	Vcbuf[0]-------测量项目
   Vcbuf[0]=1为测量cTnI；
Vcbuf[0]=2为测量NT-proBNP；
Vcbuf[0]=3为测量CRP；
Vcbuf[0]=4为测量cTnI+NT-proBNP；
Vcbuf[0]=5为测量cTnI+CKMB+Myo；
Vcbuf[0]=7为测量D-Dimer；
Vcbuf[0]=8为测量PCT；
Vcbuf[0]=9为测量mAlb；
Vcbuf[0]=0x0A为测量NGAL；
Vcbuf[0]=0x0B为测量CysC；
Vcbuf[0]=0x0C为测量β2-MG；
Vcbuf[0]=0x0D为测量hs-CRP；
Vcbuf[0]=0x0E为测量HCG；
Vcbuf[0]=0x0F为测量H-FABP；
Vcbuf[0]=0x10为测量BNP；
Vcbuf[0]=0x11为测量PCT/CRP；
Vcbuf[0]=0x12为测量CK-MB/cTnI/H-FABP；
Vcbuf[0]=0x13为测量NT-proBNP/BNP；
Vcbuf[0]=0x14为测量NGAL/mAlb；
Vcbuf[0]=0x15为测量HbA1c；
Vcbuf[0]=0x16为测量h-cTnI；

注意：用串口调试助手看下仪器发送上来十六进制数据，对照进行解析。

2.	Vcbuf[2],Vcbuf[3] -------为测量No号
Vcbuf[3]为高8位；Vcbuf[2]为低8位
3.	Vcbuf[ 4, 5, 6, 7]-------cTnI的检测结果；4个字节组成float字符
低位在前，高位在后
4.	Vcbuf[ 8, 9,10,11] -------CKMB的检测结果；4个字节组成float字符
低位在前，高位在后
5.	Vcbuf[12,13,14,15] -------Myo的检测结果；4个字节组成float字符
低位在前，高位在后
6.	Vcbuf[16,17,18,19] -------CRP的检测结果；4个字节组成float字符
低位在前，高位在后
7.	Vcbuf[20,21,22,23] -------NT-proBNP的检测结果；4个字节组成float字符
低位在前，高位在后
8.	Vcbuf[24] -------样本模式
Vcbuf[24]=0 为全血模式
Vcbuf[24]=1为血清血浆模式
Vcbuf[24]=2为尿液模式
9.	Vcbuf[25,26,27,28,29,30] -------测量时间--年、月、日、时、分、秒
Vcbuf[25]为年
Vcbuf[26]为月
Vcbuf[27]为日
Vcbuf[28]为时
Vcbuf[29]为分
Vcbuf[30]为秒
10.	Vcbuf[32] -------为测量ID号高2位
11.	Vcbuf[34,35] -------为测量ID号中间4位
Vcbuf[35]*256+ Vcbuf[34]
12.	Vcbuf[36,37] -------为测量ID号低4为
Vcbuf[37]*256+ Vcbuf[36]
13.	Vcbuf[38] -------测量状态标志位
Vcbuf[38]=1 显示准备测量
Vcbuf[38]=2 显示正在测量
Vcbuf[38]=3 显示测试结果

实例分析：
就绪状态：按确定键进入准备测量上传的代码：
20 20 FD 01 00 1E 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 00 00 00 00 00 00 00 00 00 00 00 

00 00 01 21 FC 
准备测量按确定键进入正在测量上传的代码：
    1、没打开条形码功能：
20 20 FD 01 00 1E 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 00 00 00 00 00 00 00 00 00 00 00 

00 00 02 22 FC
2、打开条形码功能 
20 20 FD 01 00 F3 13 36 39 32 33 34 35 30 36 35 33 33 32 36 0D 00 00 00 00 00 00 01 00 00 00 00 00 00 00 00 00 00 00 

00 00 02 22 FC
红色的数字即表示上传的条形码,0D用于判断接受条形码结束的字符。

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

namespace ZLCHSLis.FIA8600
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

        string startStr;//仪器发送开始符号
        string endStr;//仪器发送结束符号
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
        public void ParseResult()
        {
            throw new NotImplementedException();
        }

        private double getItemValue(string[] strs, int start, int step)
        {
            int value1 = Convert.ToInt16(strs[start + step], 16);
            int value2 = Convert.ToInt16(strs[start + 1 + step], 16);
            int value3 = Convert.ToInt16(strs[start + 2 + step], 16);
            int value4 = Convert.ToInt16(strs[start + 3 + step], 16);
            byte[] bytes = { (byte)value1, (byte)value2, (byte)value3, (byte)value4 };
            Double value = Math.Round(Convert.ToDouble(BitConverter.ToSingle(bytes, 0).ToString()), 2);
            return value;
        }

        public void ParseResult(string strSource, ref string strResult, ref string strReserved, ref string strCmd)
        {


            strResult += strSource.Trim();
            string endStr = "FC";
            if (strResult.EndsWith(endStr))
            {

                strSource = strResult;
                strResult = "";
                try
                {
                    //将接收到的字符串写入到日志文件中
                    writelog.Write(strDevice, "接收数据开始：\r\n " + strSource, "log");
                    writelog.Write(strDevice, "接收数据完成 ", "log");
                    int step = 3;
                    string[] outs = strSource.Split(new string[] { "FC" }, StringSplitOptions.RemoveEmptyEntries);
                    for (int ii = 1; ii < outs.Length; ii++)
                    {
                        strSource = outs[ii];
                        #region start
                        string[] strSourceArray = strSource.Split(' ');
                        strTestTime = "20" + Convert.ToInt16(strSourceArray[25 + step], 16) + "-" + Convert.ToInt16
    (strSourceArray[26 + step], 16) + "-" + Convert.ToInt16(strSourceArray[27 + step], 16);
                        string low = strSourceArray[5];
                        string high = strSourceArray[6];
                        strSampleNo = Convert.ToInt16(high + low, 16) + "";
                        Double cTnI = getItemValue(strSourceArray, 4, step);
                        Double CKMB = getItemValue(strSourceArray, 8, step);
                        Double Myo = getItemValue(strSourceArray, 12, step);
                        Double CRP = getItemValue(strSourceArray, 16, step);
                        Double NT_proBNP = getItemValue(strSourceArray, 20, step);
                        string str = "";                    //
                        Dictionary<string, string> dic = new Dictionary<string, string>();
                        dic.Add("01", "cTnI");
                        dic.Add("02", "NT-proBNP");
                        dic.Add("03", "CRP");
                        dic.Add("04", "cTnI+NT-proBNP");
                        dic.Add("05", "cTnI+CKMB+Myo");
                        dic.Add("07", "D-Dimer");
                        dic.Add("08", "PCT");
                        dic.Add("09", "mAlb");
                        dic.Add("0A", "NGAL");
                        dic.Add("0B", "CysC");
                        dic.Add("0C", "β2-MG");
                        dic.Add("0D", "hs-CRP");
                        dic.Add("0E", "HCG");
                        dic.Add("0F", "H-FABP");
                        dic.Add("10", "BNP");
                        dic.Add("11", "PCT/CRP");
                        dic.Add("12", "CK-MB/cTnI/H-FABP");
                        dic.Add("13", "NT-proBNP/BNP");
                        dic.Add("14", "NGAL/mAlb");
                        dic.Add("15", "HbA1c");
                        dic.Add("16", "h-cTnI");
                        string TestItemKey = strSourceArray[step];
                        string TestItemValue = "";
                        dic.TryGetValue(TestItemKey, out TestItemValue);
                        if (!string.IsNullOrEmpty(TestItemValue))
                        {
                            if (cTnI > 0) str += TestItemValue + "," + cTnI + "|";
                            if (CKMB > 0) str += "CKMB," + CKMB + "|";
                            if (Myo > 0) str += "Myo," + Myo + "|";
                            if (CRP > 0) str += "CRP," + CRP + "|";
                            if (NT_proBNP > 0) str += "NT-proBNP," + NT_proBNP + "|";


                            str = str.Remove(str.Length - 1, 1);
                            string[] strs = str.Split('|');
                            string ChannelType = "";     //0-普通结果;1-直方图;2-散点图;3-直方图界标;4-散点图界标;5-BASE64
                            string testItemID = "";
                            string TestResultValue = "";
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
                            TestResultValue = strTestTime + "|" + strSampleNo + "^" + strSampleType + "^" + strBarCode + "|"

        + strOperator + "|" + StrSpecimen + "|" + "|" + TestResultValue;
                            saveResult = new SaveResult();
                            if (!string.IsNullOrEmpty(strSampleNo))
                            {
                                saveResult.SaveTextResult(strInstrument_id, TestResultValue, TestGraph, DrSampleNoField);


                                if (ImmediatelyUpdate)
                                {
                                    saveResult.UpdateData();
                                }
                            }
                        #endregion
                        }
                    }
                  
                }
                catch (Exception e)
                {
                    writelog.Write(strDevice, "处理失败： " + e.ToString(), "log");
                }
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
