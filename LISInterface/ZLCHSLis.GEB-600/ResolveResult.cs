/*************************************
 * 名称：尿常规GEB-600解析程序
 * 功能：尿常规数据解析
 * 作者：张祥明
 * 时间：2011-06-25
 * 4800  数据位7 停止位 1  校验方式 none
 * ***********************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;
using ZLCHSLisComm;
using System.Collections;
namespace ZLCHSLis.Urine.GEB600
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

        int begin;
        int end;

        string strTestTime; //检验时间
        string strSampleNo;//标本号
        string strBarCode;  //条码
        string strOperator; //检验医师
        string strSampleType; //检验类型
        string StrSpecimen;   //标本类型
        Boolean ResultFlag;                 //结果开始标志
        string subString;                    //临时存放解析字符串
        DataRow[] FindRow;                  //解析设置
        DataSet ds_ItemChannel = new DataSet();
        DataTable tItemChannel = new DataTable();

        public void ParseResult()
        { }

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
            string testItemID = "";
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
            //判断是否有有接受字符串
            if (string.IsNullOrEmpty(strSource)) return;
            /*******检验日期*********/
            try
            {
                strTestTime = DateTime.Parse(strSource.Substring(12, 11) + " " + strSource.Substring(36, 9)).ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch
            {
                strTestTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            /********标本号********/
            strSampleNo = strSource.Substring(4, 4);
            /****解析结果****/
            string ChannelType;     //0-普通结果;1-直方图;2-散点图;3-直方图界标;4-散点图界标;5-BASE64
            ArrayList alist = new ArrayList();
            alist.Add("LEU");
            alist.Add("NIT");
            alist.Add("URO");
            alist.Add("BIL");
            alist.Add("Vc");
            alist.Add("PRO");
            alist.Add("BLD");
            alist.Add("pH");
            alist.Add("SG");
            alist.Add("GLU");
            alist.Add("KET");
            for (int i = 0; i < alist.Count; i++)
            {
                string _channelNo = alist[i].ToString();
                FindRow = tItemChannel.Select("通道编码='" +_channelNo .Trim() + "'");
                if (FindRow.Length == 0) //无普通结果则查找图像能道，无图像通道则更新通道类型为空
                {
                    ChannelType = null;
                }
                else
                {
                    testItemID = FindRow[0]["项目id"].ToString();                 
                    ChannelType = "0"; //普通结果
                }
                if ("pH,SG".Contains(_channelNo))//酸碱值、尿比重 特殊处理
                {
                    result = strSource.Substring(strSource.IndexOf(_channelNo) + 4, 10);
                }
                else
                {
                    result = strSource.Substring(strSource.IndexOf(_channelNo) + 4, 4);
                    result = TextTransform(result);
                }
                TestResultValue = TestResultValue + testItemID + "^" + result.Trim() + "|";
            }
            strSampleType = "0";
            TestResultValue = strTestTime + "|" + strSampleNo + "^" + strSampleType + "^" + strBarCode + "|" + strOperator + "|" + StrSpecimen + "|" + "|" + TestResultValue;
            writelog.Write(strDevice, "解析结果： " + TestResultValue, "result");
            saveResult = new SaveResult();
            if (!string.IsNullOrEmpty(strSampleNo) || !string.IsNullOrEmpty(strBarCode))
            {
                saveResult.SaveTextResult(strInstrument_id, TestResultValue, TestGraph, DrSampleNoField);
                if (ImmediatelyUpdate) saveResult.UpdateData();
            }
            // 2011-06-21|0007^0^||标本||62B16EF4-7F88-4498-BF82-C107EE528513^15  leu/uL|252260FD-EFFD-45C2-B79B-8309D678F20C^|A135CEC5-C8E2-4FF5-BEB1-BA0944093A59^Normal|D7F59AD1-4F9A-4995-A75B-1528FCC42448^|8F2E74E4-D1F0-47CD-97C5-EA35379F4CFC^0.6 mmol/L|869F1A2C-38A3-4613-91B5-A1EAD630FD0F^0      g/L|66E36E0A-B509-4FF2-B440-6B7D2D76CD24^80  ery/uL|54A2C7DF-1D92-4467-8FC0-C3642C170F37^5.0|4FEAF5E1-EDD4-4171-B0BA-FF1F2C6A6C9D^>1.030|16401182-86E3-4CE1-9302-3934311ED89B^0   mmol/L|F08FE403-B04D-4B34-8AA2-A5CF63453AB4^0   mmol/L|
            if (!ImmediatelyUpdate)
            {
                saveResult.UpdateData();
            }
        }

        /// <summary>
        /// 字符串解析
        /// </summary>
        /// <param name="text">转换字符串</param>
        /// <returns></returns>
        private string TextTransform(string text)
        {
            string returnText = "";
            switch (text.TrimStart(' ').TrimEnd(' '))
            {
                case "+":
                    returnText = "阳性";
                    break;
                case "-":
                    returnText = "阴性";
                    break;
                case " ":
                    returnText = "阴性";
                    break;
                case "":
                    returnText = "阴性";
                    break;
                default:
                    returnText = text;
                    break;
            }
            return returnText;
        }

        /// <summary>
        /// 通过计算偏移量，得到新的结果值
        /// </summary>
        /// <param name="channel">通道号</param>
        /// <param name="CONVERSION_RATIO">换算比</param>
        /// <param name="DECIMAL_LEVEL">小数位数</param>
        /// <param name="result">原结果值</param>
        /// <returns></returns>
        private string GetNewResult(decimal CONVERSION_RATIO, decimal DECIMAL_LEVEL, string result)
        {
            double decResult;

            try
            {
                decResult = Convert.ToDouble(result);
            }
            catch (Exception ex)
            {
                return result;
            }
            if (CONVERSION_RATIO != 0) decResult = decResult * Convert.ToDouble(CONVERSION_RATIO);
            if (DECIMAL_LEVEL == 0) result = decResult.ToString("f0");
            else if (DECIMAL_LEVEL == 1) result = decResult.ToString("f1");
            else if (DECIMAL_LEVEL == 2) result = decResult.ToString("f2");
            else if (DECIMAL_LEVEL == 3) result = decResult.ToString("f3");
            else result = decResult.ToString("f2");

            return result;
        }
        /// <summary>
        /// 通过解析规则对字符串进行解析，返回解析后的字符串
        /// </summary>
        /// <param name="fieldPara">待解析串</param>
        /// <param name="drField">解析规则行</param>
        private string GetDate(string FieldPara, DataRow drField)
        {
            string rdata = FieldPara;
            string SeparatorFirst; //第一分隔符
            int NoFirst;//第一分隔序号
            string SeparatorSecond;//第二分隔符
            int NoSecond;//第二分隔序号
            int StartBits;//开始位
            int Length;//载取长度

            try
            {
                SeparatorFirst = StrChange(drField["separated_first"].ToString());
                NoFirst = int.Parse(drField["no_first"].ToString());
                SeparatorSecond = StrChange(drField["separated_second"].ToString());
                NoSecond = int.Parse(drField["no_second"].ToString());
                StartBits = int.Parse(drField["start_bits"].ToString());
                Length = int.Parse(drField["length"].ToString());
            }
            catch (Exception e)
            {
                writelog.Write(strDevice, "解析规则设置错误：" + e.Message, "log");
                return "";
            }


            if (drField == null) return "";
            if (FieldPara.Length < Length + StartBits - 1 && Length > 0) return "";
            if (String.IsNullOrEmpty(SeparatorFirst) && String.IsNullOrEmpty(SeparatorSecond) && NoFirst == 0 && NoSecond == 0 && StartBits == 0 && Length == 0)
            {
                return FieldPara;
            }
            if (String.IsNullOrEmpty(SeparatorFirst) && String.IsNullOrEmpty(SeparatorSecond))
            {
                if (StartBits > 0 && Length == 0)
                {
                    rdata = FieldPara.Substring(StartBits - 1);
                }
                else if (StartBits > 0 && Length > 0)
                {
                    rdata = FieldPara.Substring(StartBits - 1, Length);
                }
                return rdata;
            }
            //第一个分隔符
            if (!String.IsNullOrEmpty(SeparatorFirst))
            {
                if (NoFirst != 0 && FieldPara.Split(SeparatorFirst.ToCharArray()).Length >= NoFirst - 1)
                {
                    if (SeparatorFirst.Length == 1) rdata = FieldPara.Split(SeparatorFirst.ToCharArray())[NoFirst - 1].ToString();
                    else rdata = Regex.Split(FieldPara, (SeparatorFirst == "|" ? @"\|" : SeparatorFirst))[NoFirst - 1].ToString();
                }
                else rdata = FieldPara;

            }
            //第二个分隔符
            FieldPara = rdata;
            if (!String.IsNullOrEmpty(SeparatorSecond))
            {
                if (NoFirst != 0)
                {
                    if (SeparatorSecond.Length == 1) rdata = FieldPara.Split(SeparatorSecond.ToCharArray())[NoSecond - 1].ToString();
                    else rdata = Regex.Split(FieldPara, (SeparatorSecond == "|" ? @"\|" : SeparatorSecond))[NoSecond - 1].ToString();
                }
            }

            if (StartBits > 0 && Length > 0 && rdata.Length < StartBits + Length - 1) return "";
            if (StartBits > 0 && Length > 0) rdata = rdata.Substring(StartBits - 1, Length);

            return rdata;
        }

        //DrawGraph dg = new DrawGraph();

        ////dg.DrawGph("折线图", 56347, 19 ,4480,2240 ,32,32,18,18 ,108,0);
        //    //dg.DrawGph("散点图", 56347, 23,640,360 ,32,32,18,18 ,128,128);
        //    dg.DrawGph("BASE64", 1, 42, 640, 360, 32, 32, 18, 18, 128, 128);

        //private void Base64StringToImage(string inputFileName)
        //{
        //    System.IO.StreamReader inFile;
        //    string base64String;

        //    try
        //    {
        //        char[] base64CharArray;
        //        inFile = new System.IO.StreamReader(inputFileName, System.Text.Encoding.Default);
        //        base64CharArray = new char[inFile.BaseStream.Length];
        //        inFile.Read(base64CharArray, 0, (int)inFile.BaseStream.Length);
        //        base64String = new string(base64CharArray);
        //    }
        //    catch (System.Exception exp)
        //    {
        //        // Error creating stream or reading from it.
        //        System.Console.WriteLine("{0}", exp.Message);
        //        return;
        //    }

        //    // Convert the Base64 UUEncoded input into binary output.
        //    byte[] bytes;
        //    try
        //    {
        //        bytes = System.Convert.FromBase64String(base64String);
        //    }
        //    catch (System.ArgumentNullException)
        //    {
        //        System.Console.WriteLine("Base 64 string is null.");
        //        return;
        //    }
        //    catch (System.FormatException)
        //    {
        //        System.Console.WriteLine("Base 64 string length is not " +
        //            "4 or is not an even multiple of 4.");
        //        return;
        //    }

        //    MemoryStream ms = new MemoryStream(bytes);
        //    Image image = Image.FromStream(ms);
        //    image.Save(@"c:\未命名1.bmp", ImageFormat.Bmp);
        //}
        //OpenFileDialog dlg = new OpenFileDialog();
        //    dlg.Title = "选择要转换的图片";
        //    dlg.Filter = "Image files (*.jpg;*.bmp;*.gif)|*.jpg*.jpeg;*.gif;*.bmp|AllFiles (*.*)|*.*";
        //    if (DialogResult.OK == dlg.ShowDialog())
        //    {
        //        ImgToBase64String(dlg.FileName);
        //    }
        private void init()
        {
            TestResultValue = null;      //解析后的通用字符串
            TestGraph = new List<string>(); //图像列表

            //DrTestTimeSignField = null;         //检验时间标识
            //DrTestTimeField = null;             //检验时间
            //DrSampleNoSignField = null;
            //DrSampleNoField = null;
            //DrBarCodeSignField = null;
            //DrBarCodeField = null;
            //DrSampleTypeSignField = null;
            //DrSampleTypeField = null;
            //DrOperatorSignField = null;
            //DrOperatorField = null;
            //DrSpecimenSignField = null;
            //DrSpecimenField = null;

            //DrResultSignField = null;
            //DrResultInfoField = null;
            //DrResultCountField = null;
            //DrSingleResultField = null;
            //DrChannelField = null;
            //DrResultField = null;

            //DrQCSampleField = null;

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

            //FindRow = null;
        }

        /// <summary>
        /// 选取本地图片
        /// </summary>
        /// <param name="IMG"></param>
        /// <returns></returns>
        public System.Drawing.Image LocalIMG(string IMG)
        {
            System.IO.FileStream fs = new System.IO.FileStream(IMG, System.IO.FileMode.Open);
            //System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(fs);
            System.Drawing.Image image = System.Drawing.Image.FromStream(fs, true);
            fs.Close();
            return image;
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
            if (String.IsNullOrEmpty(SourceData)) return "";
            if (SourceData.Contains("<") && SourceData.Contains(">")) return ((char)int.Parse(SourceData.Replace("<", "").Replace(">", ""))).ToString();
            else return SourceData;
        }

        /// <summary>
        /// 获取仪器解析规则及项目通道
        /// </summary>
        /// <param name="StrDevice">仪器ID</param>
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

        public void SetVariable(DataTable dt)
        {

        }
    }
}
