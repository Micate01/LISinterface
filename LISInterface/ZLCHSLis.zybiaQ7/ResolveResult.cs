/**
 * 文本ini文件
 * 2016-7-12

 * 
 * 中元生物 CRP
 * 
 * 
 * */


using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using ZLCHSLisComm;
namespace ZLCHSLis.zybiaQ7{   

    public class ResolveResult : IDataResolve
    {
        private Dictionary<string, string> _DIC = new Dictionary<string, string>();
        private DataRow DrBarCodeField;
        private DataRow DrBarCodeSignField;
        private DataRow DrChannelField;
        private DataRow DrOperatorField;
        private DataRow DrOperatorSignField;
        private DataRow DrQCSampleField;
        private DataRow DrResultCountField;
        private DataRow DrResultField;
        private DataRow DrResultInfoField;
        private DataRow DrResultSignField;
        private DataRow DrSampleNoField;
        private DataRow DrSampleNoSignField;
        private DataRow DrSampleTypeField;
        private DataRow DrSampleTypeSignField;
        private DataRow DrSingleResultField;
        private DataRow DrSpecimenField;
        private DataRow DrSpecimenSignField;
        private DataRow DrTestTimeField;
        private DataRow DrTestTimeSignField;
        private DataSet ds_ItemChannel = new DataSet();
        private DataSetHandle dsHandle = new DataSetHandle();
        private string FilePath = "";
        private DataRow[] FindRow;
        public bool ImmediatelyUpdate = false;
        public List<string> ListImagePosition = new List<string>();
        public List<string> listInputResult = new List<string>();
        private bool ResultFlag;
        private SaveResult saveResult;
        public string strACK_all;
        public string strACK_term;
        private string strBarCode;
        public string strDataBegin;
        public string strDataEnd;
        public string strDetype;
        private string strDevice;
        public string strInstrument_id;
        private string strOperator;
        private string strSampleNo;
        private string strSampleType;
        private string StrSpecimen;
        public string strSubBegin;
        public string strSubEnd;
        private string strTestTime;
        private string subString;
        private List<string> TestGraph;
        private string TestResultValue;
        private DataTable tItemChannel = new DataTable();
        private Write_Log writelog = new Write_Log();

        public string GetCmd(string dataIn, string ack_term)
        {
            throw new NotImplementedException();
        }

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        public void GetRules(string StrDevice)
        {
            DataSet set = new DataSet();
            DataSet set2 = new DataSet();
            this.strInstrument_id = StrDevice;
            this.DrTestTimeSignField = null;
            this.DrTestTimeField = null;
            this.DrSampleNoSignField = null;
            this.DrSampleNoField = null;
            this.DrBarCodeSignField = null;
            this.DrBarCodeField = null;
            this.DrSampleTypeSignField = null;
            this.DrSampleTypeField = null;
            this.DrOperatorSignField = null;
            this.DrOperatorField = null;
            this.DrSpecimenSignField = null;
            this.DrSpecimenField = null;
            this.DrResultSignField = null;
            this.DrResultInfoField = null;
            this.DrResultCountField = null;
            this.DrSingleResultField = null;
            this.DrChannelField = null;
            this.DrResultField = null;
            set2 = this.dsHandle.GetDataSet("Extractvalue(Column_Value, '/item/item_code') As item_code, Extractvalue(Column_Value, '/item/separated_first') As separated_first, \r\n                                                   Extractvalue(Column_Value, '/item/no_first') As no_first, Extractvalue(Column_Value, '/item/separated_second') As separated_second,\r\n                                                   Extractvalue(Column_Value, '/item/no_second') As no_second, Extractvalue(Column_Value, '/item/start_bits') As start_bits,\r\n                                                   Extractvalue(Column_Value, '/item/length') As length, Extractvalue(Column_Value, '/item/sign') As sign,Extractvalue(Column_Value, '/item/format') As format", "Table(Xmlsequence(Extract((Select 解析规则 From 检验仪器 Where ID = '" + this.strInstrument_id + "'), '/root/item'))) ", "");
            this.tItemChannel = OracleHelper.GetDataTable("Select 通道编码, m.项目id, Nvl(小数位数, 2) As 小数位数, Nvl(换算比, 0) As 换算比, Nvl(加算值, 0) As 加算值, j.结果类型\r\n                                        From 仪器检测项目 m, 检验项目 j\r\n                                        Where m.项目id = j.项目id and m.仪器Id='" + StrDevice + "'");
            this.ds_ItemChannel.CaseSensitive = true;
            this.FindRow = set2.Tables[0].Select("item_code = '01'");
            if (this.FindRow.Length != 0)
            {
                this.DrTestTimeSignField = this.FindRow[0];
            }
            this.FindRow = set2.Tables[0].Select("item_code = '02'");
            if (this.FindRow.Length != 0)
            {
                this.DrTestTimeField = this.FindRow[0];
            }
            this.FindRow = set2.Tables[0].Select("item_code = '03'");
            if (this.FindRow.Length != 0)
            {
                this.DrSampleNoSignField = this.FindRow[0];
            }
            this.FindRow = set2.Tables[0].Select("item_code = '04'");
            if (this.FindRow.Length != 0)
            {
                this.DrSampleNoField = this.FindRow[0];
            }
            else
            {
                this.DrSampleNoField = null;
            }
            this.FindRow = set2.Tables[0].Select("item_code = '05'");
            if (this.FindRow.Length != 0)
            {
                this.DrQCSampleField = this.FindRow[0];
            }
            this.FindRow = set2.Tables[0].Select("item_code = '06'");
            if (this.FindRow.Length != 0)
            {
                this.DrBarCodeSignField = this.FindRow[0];
            }
            this.FindRow = set2.Tables[0].Select("item_code = '07'");
            if (this.FindRow.Length != 0)
            {
                this.DrBarCodeField = this.FindRow[0];
            }
            this.FindRow = set2.Tables[0].Select("item_code = '08'");
            if (this.FindRow.Length != 0)
            {
                this.DrSampleTypeSignField = this.FindRow[0];
            }
            this.FindRow = set2.Tables[0].Select("item_code = '09'");
            if (this.FindRow.Length != 0)
            {
                this.DrSampleTypeField = this.FindRow[0];
            }
            this.FindRow = set2.Tables[0].Select("item_code = '10'");
            if (this.FindRow.Length != 0)
            {
                this.DrOperatorSignField = this.FindRow[0];
            }
            this.FindRow = set2.Tables[0].Select("item_code = '11'");
            if (this.FindRow.Length != 0)
            {
                this.DrOperatorField = this.FindRow[0];
            }
            this.FindRow = set2.Tables[0].Select("item_code = '12'");
            if (this.FindRow.Length != 0)
            {
                this.DrSpecimenSignField = this.FindRow[0];
            }
            this.FindRow = set2.Tables[0].Select("item_code = '13'");
            if (this.FindRow.Length != 0)
            {
                this.DrSpecimenField = this.FindRow[0];
            }
            this.FindRow = set2.Tables[0].Select("item_code = '14'");
            if (this.FindRow.Length != 0)
            {
                this.DrResultSignField = this.FindRow[0];
            }
            this.FindRow = set2.Tables[0].Select("item_code = '15'");
            if (this.FindRow.Length != 0)
            {
                this.DrResultInfoField = this.FindRow[0];
            }
            this.FindRow = set2.Tables[0].Select("item_code = '16'");
            if (this.FindRow.Length != 0)
            {
                this.DrResultCountField = this.FindRow[0];
            }
            this.FindRow = set2.Tables[0].Select("item_code = '17'");
            if (this.FindRow.Length != 0)
            {
                this.DrSingleResultField = this.FindRow[0];
            }
            this.FindRow = set2.Tables[0].Select("item_code = '18'");
            if (this.FindRow.Length != 0)
            {
                this.DrChannelField = this.FindRow[0];
            }
            this.FindRow = set2.Tables[0].Select("item_code = '19'");
            if (this.FindRow.Length != 0)
            {
                this.DrResultField = this.FindRow[0];
            }
            this.FindRow = set2.Tables[0].Select("item_code = '20'");
            this.FindRow = set2.Tables[0].Select("item_code = '21'");
            this.strDevice = OracleHelper.GetDataTable("select 名称 from 检验仪器 where id='" + StrDevice + "'").Rows[0]["名称"].ToString();
        }

        public Image LocalIMG(string IMG)
        {
            throw new NotImplementedException();
        }

        public void ParseResult()
        {
            throw new NotImplementedException();
        }

        public void ParseResult(string strSource, ref string strResult, ref string strReserved, ref string strCmd)
        {
            string str = strSource;
            try
            {
                StreamReader reader = new StreamReader(@".\zybiaQ7.txt", Encoding.GetEncoding("GB2312"));
                string[] strArray = reader.ReadLine().Split(new char[] { '|' });
                string path = reader.ReadLine();
                reader.Close();
                string str4 = "*.ini";
                string filePath = DateTime.Now.ToString("yyyy-MM-dd");
                List<string> list = new List<string>();
                FileInfo[] files = Directory.CreateDirectory(path).GetFiles(filePath + str4);
                foreach (FileInfo info2 in files)
                {
                    StringBuilder builder = new StringBuilder("");
                    filePath = info2.FullName.ToString();
                    try
                    {
                        string section = "Export";
                        string key = "When";
                        string str8 = "";
                        string str9 = "";
                        string str10 = "NumberCassettes";
                        int num = 0;
                        string def = "";
                        StringBuilder retVal = new StringBuilder(0xff);
                        GetPrivateProfileString(section, key, def, retVal, 0xff, filePath);
                        str8 = retVal.ToString();
                        if (!string.IsNullOrEmpty(str8))
                        {
                            int num3;
                            int num5;
                            str8 = str8.Split(new char[] { '/' })[0] + " " + str8.Split(new char[] { '/' })[1];
                            GetPrivateProfileString(section, str10, def, retVal, 0xff, filePath);
                            str10 = retVal.ToString();
                            if (!string.IsNullOrEmpty(str10))
                            {
                                num = Convert.ToInt16(str10);
                                num3 = 0;
                                while (num3 < num)
                                {
                                    string str14 = "C" + num3;
                                    string str11 = "SampleId";
                                    GetPrivateProfileString(str14, str11, def, retVal, 0xff, filePath);
                                    str9 = retVal.ToString();
                                    str11 = "NumberAnalytes";
                                    GetPrivateProfileString(str14, str11, def, retVal, 0xff, filePath);
                                    int num4 = Convert.ToInt16(retVal.ToString());
                                    str11 = "ExtraResult0";
                                    GetPrivateProfileString(str14, str11, def, retVal, 0xff, filePath);
                                    builder.Append("strSampleTime," + str8 + "|");
                                    builder.Append("strTestNo," + str9 + "|");
                                    if (retVal.ToString().IndexOf("(") != -1)
                                    {
                                        builder.Append(str11 + "," + retVal.ToString().Split(new char[] { '(' })[0] + "|");
                                    }
                                    else
                                    {
                                        builder.Append(str11 + "," + retVal.ToString() + "|");
                                    }
                                    num5 = 0;
                                    while (num5 < num4)
                                    {
                                        string str15 = str14 + "A" + num5;
                                        str11 = "Status";
                                        GetPrivateProfileString(str15, str11, def, retVal, 0xff, filePath);
                                        if (retVal.ToString().Equals("0"))
                                        {
                                            str11 = "Name";
                                            GetPrivateProfileString(str15, str11, def, retVal, 0xff, filePath);
                                            string str16 = retVal.ToString();
                                            builder.Append(str16);
                                            str11 = "Result";
                                            GetPrivateProfileString(str15, str11, def, retVal, 0xff, filePath);
                                            builder.Append("," + retVal.ToString() + "|");
                                        }
                                        num5++;
                                    }
                                    builder.Append('\r' + '\n');
                                    num3++;
                                }
                            }
                            string[] strArray2 = builder.ToString().Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string str18 in strArray2)
                            {
                                string str19 = str18;
                                string str20 = str18.Split(new char[] { '|' })[0].Split(new char[] { ',' })[1];
                                string str21 = str18.Split(new char[] { '|' })[1].Split(new char[] { ',' })[1];
                                for (num5 = 0; num5 < strArray.Length; num5++)
                                {
                                    string oldValue = strArray[num5].Split(new char[] { ',' })[0].Trim();
                                    string newValue = strArray[num5].Split(new char[] { ',' })[1].Trim();
                                    str19 = str19.Replace(oldValue, newValue).Replace("PG1I", "PG2");
                                }
                                string str24 = Helper.SampleNoToSampleBar(str21);
                                if (!string.IsNullOrEmpty(str24))
                                {
                                    str20 = str24.Split(new char[] { '|' })[0];
                                    str21 = str24.Split(new char[] { '|' })[1];
                                    this.strBarCode = str21;
                                }
                                string[] strArray3 = str19.ToString().Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                                string str26 = "";
                                string str27 = "";
                                for (num3 = 0; num3 < strArray3.Length; num3++)
                                {
                                    this.FindRow = this.tItemChannel.Select("通道编码='" + strArray3[num3].Split(new char[] { ',' })[0] + "'");
                                    if (this.FindRow.Length == 0)
                                    {
                                        this.writelog.Write(this.strDevice, "未设置通道：" + strArray3[num3].Split(new char[] { ',' })[0], "log");
                                    }
                                    else
                                    {
                                        str26 = this.FindRow[0]["项目id"].ToString();
                                        str27 = str27 + str26 + "^" + strArray3[num3].Split(new char[] { ',' })[1].Replace("RuPT", "阴性") + "|";
                                    }
                                }
                                str27 = str20 + "|" + str21 + "^" + this.strSampleType + "^" + this.strBarCode + "|" + this.strOperator + "|" + this.StrSpecimen + "||" + str27;
                                this.saveResult = new SaveResult();
                                if (!string.IsNullOrEmpty(str21) || !string.IsNullOrEmpty(this.strBarCode))
                                {
                                    if (string.IsNullOrEmpty(strReserved))
                                    {
                                        this.saveResult.SaveTextResult(this.strInstrument_id, str27, this.TestGraph, this.DrSampleNoField);
                                    }
                                    else
                                    {
                                        this.saveResult.SaveTextResult(strReserved, str27, this.TestGraph, this.DrSampleNoField);
                                    }
                                    if (this.ImmediatelyUpdate)
                                    {
                                        this.saveResult.UpdateData();
                                    }
                                }
                                if (!this.ImmediatelyUpdate)
                                {
                                    this.saveResult.UpdateData();
                                }
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception.ToString());
                    }
                }
                Thread.Sleep(0x1388);
            }
            catch (Exception exception2)
            {
                this.writelog.Write(this.strDevice, "处理失败： " + exception2.ToString(), "log");
            }
        }

        public void SetVariable(DataTable dt)
        {
        }

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filepath);
    }
}
