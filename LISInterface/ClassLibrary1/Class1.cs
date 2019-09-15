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
public class ResolveResult : IDataResolve
{
    // Fields
    private DataSetHandle dsHandle = new DataSetHandle();
    private DataRow[] FindRow;
    private SaveResult saveResult;
    private string strBarCode;
    private string strDevice;
    public string strInstrument_id;
    private string strOperator;
    private string strSampleNo;
    private string strSampleType;
    private string StrSpecimen;
    private string strTestTime;
    private List<string> TestGraph;
    private string TestResultValue;
    private DataTable tItemChannel = new DataTable();
    private Write_Log writelog = new Write_Log();

    // Methods
    public string GetCmd(string dataIn, string ack_term)
    {
        throw new NotImplementedException();
    }

    public void GetRules(string StrDevice)
    {
        this.strInstrument_id = StrDevice;
        this.tItemChannel = OracleHelper.GetDataTable("Select 通道编码, m.项目id, Nvl(小数位数, 2) As 小数位数, Nvl(换算比, 0) As 换算比, Nvl(加算值, 0) As 加算值, j.结果类型, y.名称\r\nFrom 检验仪器 y, 仪器检测项目 m, 检验项目 j\r\nWhere y.Id = m.仪器id(+) And m.项目id = j.项目id(+) And y.Id = '" + StrDevice + "'");
        this.strDevice = this.tItemChannel.Rows[0]["名称"].ToString();
    }

    public string GetStr_Section(string strSource)
    {
        string str = "";
        string str2 = "";
        string str3 = "";
        char ch = strSource[0];
        if (ch.Equals('\x0002'))
        {
            ch = '\x0002';
            if (strSource.Contains(ch.ToString()) && strSource.Contains((ch = '\x001a').ToString()))
            {
                string[] separator = new string[1];
                ch = '\x0002';
                separator[0] = ch.ToString();
                int length = strSource.Split(separator, StringSplitOptions.None).Length;
                separator = new string[1];
                ch = '\x001a';
                separator[0] = ch.ToString();
                int num2 = strSource.Split(separator, StringSplitOptions.None).Length;
                if (length != num2)
                {
                    return "";
                }
                ch = '\x0002';
                ch = '\x001a';
                ch = '\x0002';
                str = strSource.Substring(strSource.IndexOf(ch.ToString()), (strSource.LastIndexOf(ch.ToString()) - strSource.IndexOf(ch.ToString())) + 1).ToString();
                ch = '\x001a';
                if ((strSource.Length - strSource.LastIndexOf(ch.ToString())) != 1)
                {
                    ch = '\x001a';
                    ch = '\x001a';
                    str2 = strSource.Substring(strSource.LastIndexOf(ch.ToString()) + 1, (strSource.Length - strSource.LastIndexOf(ch.ToString())) - 1).ToString();
                }
                List<string> list = new List<string>();
                string[] strArray = str.Split(new char[] { '\x001a' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < strArray.Length; i++)
                {
                    list.Add(strArray[i] + '\x001a');
                }
                strSource = "";
                foreach (string str4 in list)
                {
                    ch = '\x0002';
                    if ((str4.Length - str4.Replace(ch.ToString(), "").Length) > 1)
                    {
                        str3 = str4.Substring(str4.LastIndexOf('\x0002'), str4.Length - str4.LastIndexOf('\x0002'));
                        strSource = strSource + str3;
                    }
                    else
                    {
                        strSource = strSource + str4;
                    }
                }
                return strSource;
            }
            return "";
        }
        return "";
    }

    public bool IsContinueConnecting()
    {
        return true;
    }

    public static Bitmap KiResizeImage(Bitmap bmp, int newW, int newH, int Mode)
    {
        try
        {
            Bitmap image = new Bitmap(newW, newH);
            Graphics graphics = Graphics.FromImage(image);
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.DrawImage(bmp, new Rectangle(0, 0, newW, newH), new Rectangle(0, 0, bmp.Width, bmp.Height), GraphicsUnit.Pixel);
            graphics.Dispose();
            return image;
        }
        catch
        {
            return null;
        }
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
        strSource = strSource.Replace("*", "0");
        if (this.GetStr_Section(strSource).Length == 0)
        {
            this.writelog.Write(this.strDevice, "仪器数据不完整，暂不解析：" + strSource, "log");
            strSource = str;
        }
        else
        {
            string s = "<?xml version=\"1.0\" standalone=\"yes\"?>\r\n<DocumentElement>\r\n  <解析规则>\r\n    <ChannelCode>SampleNo</ChannelCode>\r\n    <StartIndex>3</StartIndex>\r\n    <Length>7</Length>\r\n    <Divisor>1</Divisor>\r\n   <Decimal>0</Decimal>\r\n  </解析规则>\r\n  <解析规则>\r\n    <ChannelCode>Date</ChannelCode>\r\n    <StartIndex>11</StartIndex>\r\n    <Length>12</Length>\r\n    <Divisor>0</Divisor>\r\n   <Decimal>0</Decimal>\r\n  </解析规则>\r\n  <解析规则>\r\n    <ChannelCode>WBC</ChannelCode>\r\n    <StartIndex>23</StartIndex>\r\n    <Length>4</Length>\r\n    <Divisor>10</Divisor>\r\n   <Decimal>2</Decimal>\r\n  </解析规则>\r\n  <解析规则>\r\n    <ChannelCode>Lymph#</ChannelCode>\r\n    <StartIndex>27</StartIndex>\r\n    <Length>4</Length>\r\n    <Divisor>10</Divisor>\r\n   <Decimal>2</Decimal>\r\n  </解析规则>\r\n    <解析规则>\r\n    <ChannelCode>MID</ChannelCode>\r\n    <StartIndex>31</StartIndex>\r\n    <Length>5</Length>\r\n    <Divisor>100</Divisor>\r\n   <Decimal>2</Decimal>\r\n  </解析规则>\r\n      <解析规则>\r\n    <ChannelCode>Gran#</ChannelCode>\r\n    <StartIndex>36</StartIndex>\r\n    <Length>3</Length>\r\n    <Divisor>10</Divisor>\r\n   <Decimal>2</Decimal>\r\n  </解析规则>\r\n<解析规则>\r\n    <ChannelCode>Lymph%</ChannelCode>\r\n    <StartIndex>39</StartIndex>\r\n    <Length>3</Length>\r\n    <Divisor>10</Divisor>\r\n   <Decimal>2</Decimal>\r\n  </解析规则> \r\n    <解析规则>\r\n    <ChannelCode>Mid%</ChannelCode>\r\n    <StartIndex>42</StartIndex>\r\n    <Length>3</Length>\r\n    <Divisor>10</Divisor>\r\n   <Decimal>2</Decimal>\r\n  </解析规则>\r\n    <解析规则>\r\n    <ChannelCode>Gran%</ChannelCode>\r\n    <StartIndex>45</StartIndex>\r\n    <Length>3</Length>\r\n    <Divisor>10</Divisor>\r\n   <Decimal>2</Decimal>\r\n  </解析规则>\r\n  <解析规则>\r\n    <ChannelCode>RBC</ChannelCode>\r\n    <StartIndex>48</StartIndex>\r\n    <Length>3</Length>\r\n    <Divisor>100</Divisor>\r\n   <Decimal>2</Decimal>\r\n  </解析规则>\r\n  <解析规则>\r\n    <ChannelCode>HGB</ChannelCode>\r\n    <StartIndex>51</StartIndex>\r\n    <Length>4</Length>\r\n    <Divisor>10</Divisor>\r\n   <Decimal>2</Decimal>\r\n  </解析规则>\r\n  <解析规则>\r\n    <ChannelCode>MCHC</ChannelCode>\r\n    <StartIndex>55</StartIndex>\r\n    <Length>4</Length>\r\n    <Divisor>10</Divisor>\r\n   <Decimal>2</Decimal>\r\n  </解析规则>\r\n  <解析规则>\r\n    <ChannelCode>MCV</ChannelCode>\r\n    <StartIndex>58</StartIndex>\r\n    <Length>5</Length>\r\n    <Divisor>100</Divisor>\r\n   <Decimal>2</Decimal>\r\n  </解析规则>\r\n  <解析规则>\r\n    <ChannelCode>MCH</ChannelCode>\r\n    <StartIndex>62</StartIndex>\r\n    <Length>4</Length>\r\n    <Divisor>10</Divisor>\r\n   <Decimal>2</Decimal>\r\n  </解析规则>\r\n  <解析规则>\r\n    <ChannelCode>RDW-CV</ChannelCode>\r\n    <StartIndex>66</StartIndex>\r\n    <Length>3</Length>\r\n    <Divisor>10</Divisor>\r\n   <Decimal>2</Decimal>\r\n  </解析规则>\r\n  <解析规则>\r\n    <ChannelCode>HCT</ChannelCode>\r\n    <StartIndex>69</StartIndex>\r\n    <Length>4</Length>\r\n    <Divisor>100</Divisor>\r\n   <Decimal>2</Decimal>\r\n  </解析规则>\r\n  <解析规则>\r\n    <ChannelCode>PLT</ChannelCode>\r\n    <StartIndex>73</StartIndex>\r\n    <Length>3</Length>\r\n    <Divisor>1</Divisor>\r\n   <Decimal>2</Decimal>\r\n  </解析规则>\r\n  <解析规则>\r\n    <ChannelCode>MPV</ChannelCode>\r\n    <StartIndex>76</StartIndex>\r\n    <Length>3</Length>\r\n    <Divisor>10</Divisor>\r\n   <Decimal>2</Decimal>\r\n  </解析规则>\r\n  <解析规则>\r\n    <ChannelCode>PDW</ChannelCode>\r\n    <StartIndex>79</StartIndex>\r\n    <Length>3</Length>\r\n    <Divisor>10</Divisor>\r\n   <Decimal>2</Decimal>\r\n  </解析规则>\r\n  <解析规则>\r\n    <ChannelCode>PCT</ChannelCode>\r\n    <StartIndex>82</StartIndex>\r\n    <Length>4</Length>\r\n    <Divisor>10000</Divisor>\r\n   <Decimal>3</Decimal>\r\n  </解析规则>\r\n  <解析规则>\r\n    <ChannelCode>RDW-SD</ChannelCode>\r\n    <StartIndex>86</StartIndex>\r\n    <Length>3</Length>\r\n    <Divisor>10</Divisor>\r\n   <Decimal>2</Decimal>\r\n  </解析规则>\r\n</DocumentElement>";
            DataSet set = new DataSet();
            set.ReadXml(new StringReader(s));
            DataTable table = set.Tables[0];
            string[] separator = new string[] { '\x0002'.ToString() };
            string[] strArray = strSource.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < strArray.Length; i++)
            {
                int num2 = 0;
                while (num2 < table.Rows.Count)
                {
                    string str4 = table.Rows[num2]["ChannelCode"].ToString();
                    int num3 = Convert.ToInt32(table.Rows[num2]["StartIndex"].ToString());
                    int num4 = Convert.ToInt32(table.Rows[num2]["Length"].ToString());
                    int num5 = Convert.ToInt32(table.Rows[num2]["Divisor"].ToString());
                    int digits = Convert.ToInt32(table.Rows[num2]["Decimal"].ToString());
                    if (table.Rows[num2]["ChannelCode"].ToString() == "SampleNo")
                    {
                        this.strSampleNo = Math.Round((double) (Convert.ToDouble(strArray[i].Substring(num3 - 1, num4)) / ((double) num5)), digits).ToString();
                    }
                    else if (table.Rows[num2]["ChannelCode"].ToString() == "Date")
                    {
                        string str5 = strArray[i].Substring(num3 - 1, num4);
                        this.strTestTime = str5.Substring(4, 4) + "-" + str5.Substring(0, 2) + "-" + str5.Substring(2, 2) + " " + str5.Substring(8, 2) + ":" + str5.Substring(10) + ":00";
                    }
                    else
                    {
                        string str6 = str4;
                        this.FindRow = this.tItemChannel.Select("通道编码='" + str6.Trim() + "'");
                        if (this.FindRow.Length == 0)
                        {
                            this.writelog.Write(this.strDevice, "未设置通道：" + str6, "log");
                        }
                        else
                        {
                            string str7 = this.FindRow[0]["项目id"].ToString();
                            this.TestResultValue = this.TestResultValue + str7 + "^" + Math.Round((double) (Convert.ToDouble(strArray[i].Substring(num3 - 1, num4)) / ((double) num5)), digits).ToString() + "|";
                        }
                    }
                    num2++;
                }
                this.TestResultValue = this.strTestTime + "|" + this.strSampleNo + "^" + this.strSampleType + "^" + this.strBarCode + "|" + this.strOperator + "|" + this.StrSpecimen + "||" + this.TestResultValue;
                this.writelog.Write(this.strDevice, "解析结果： " + this.TestResultValue, "log");
                this.saveResult = new SaveResult();
                if (!string.IsNullOrEmpty(this.strSampleNo) || !string.IsNullOrEmpty(this.strBarCode))
                {
                    try
                    {
                        this.saveResult.SaveTextResult(this.strInstrument_id, this.TestResultValue, this.TestGraph, null);
                    }
                    catch (Exception exception)
                    {
                        this.writelog.Write(this.strDevice, "保存结果失败！错误原因 " + exception.Message, "log");
                    }
                    this.TestResultValue = "";
                    strReserved = "";
                }
                string cmdStr = "delete from 检验图像结果 where 记录id = '" + SaveResult.Appstr + "'";
                string msg = "";
                OracleHelper.OraExeNonQuery(cmdStr, ref msg);
                string str10 = strArray[i].Substring(0xa2, 0x31b);
                int length = str10.Length;
                string keyValues = "";
                num2 = 0;
                while (num2 < ((length / 3) - 1))
                {
                    keyValues = keyValues + ";" + str10.Substring((3 * num2) + 1, 3);
                    num2++;
                }
               // Bitmap bitmap = new Curve2D { M_YAxis = "250;230;220;200;190", M_YAxis = "400;1", XAxisCount = 4f, Width = 340f, Height = 263f, XSliceValue = 100f, YSliceValue = 100f, IsXSliceLine = false, IsYSliceLine = false, IsYSliceSign = false, IsXSliceSign = false, CurveSize = 1, FontSize = 14, Title = "WBC" }.CreateImage(keyValues);
                MemoryStream stream = new MemoryStream();
              //  bitmap.Save(stream, ImageFormat.Bmp);
                byte[] buffer = new byte[stream.Length];
                stream.Position = 0L;
                stream.Read(buffer, 0, Convert.ToInt32(stream.Length));
                OracleParameter[] parameterArray = new OracleParameter[] { new OracleParameter(":图像", 0x66) };
                parameterArray[0].Value = buffer;
                string str12 = Guid.NewGuid().ToString();
                OracleHelper.OraExeNonQuery("Insert into 检验图像结果(Id,记录ID,图像类型,文件目录,图像) values('" + str12 + "','" + SaveResult.Appstr + "','bmp','WBC',:图像)", parameterArray, ref msg);
                string str14 = strArray[i].Substring(0x3bd, 750);
                length = 0;
                length = str14.Length;
                keyValues = "";
                num2 = 0;
                while (num2 < ((length / 3) - 1))
                {
                    keyValues = keyValues + ";" + str14.Substring((3 * num2) + 1, 3);
                    num2++;
                }
                Color[] clrsCurveColors = new Color[] { Color.Black, Color.Blue };
                Bitmap bitmap2 = FoldLineDiagram.CreateImage("RBC", "", 340, 0x107, 50, 100, 50, 20, keyValues + "|", clrsCurveColors);
                MemoryStream stream2 = new MemoryStream();
                bitmap2.Save(stream2, ImageFormat.Bmp);
                byte[] buffer2 = new byte[stream2.Length];
                stream2.Position = 0L;
                stream2.Read(buffer2, 0, Convert.ToInt32(stream2.Length));
                OracleParameter[] parameterArray2 = new OracleParameter[] { new OracleParameter(":图像", 0x66) };
                parameterArray2[0].Value = buffer2;
                string str15 = Guid.NewGuid().ToString();
                OracleHelper.OraExeNonQuery("Insert into 检验图像结果(Id,记录ID,图像类型,文件目录,图像) values('" + str15 + "','" + SaveResult.Appstr + "','bmp','RBC',:图像)", parameterArray2, ref msg);
                string str17 = strArray[i].Substring(0x6ab, 660);
                length = str17.Length;
                keyValues = "";
                for (num2 = 0; num2 < ((length / 3) - 1); num2++)
                {
                    keyValues = keyValues + ";" + str17.Substring((3 * num2) + 1, 3);
                }
                clrsCurveColors = new Color[] { Color.Black, Color.Blue };
                Bitmap bitmap3 = FoldLineDiagram.CreateImage("PLT", "", 400, 300, 0x23, 100, 50, 0, keyValues + "|", clrsCurveColors);
                MemoryStream stream3 = new MemoryStream();
                bitmap3.Save(stream3, ImageFormat.Bmp);
                byte[] buffer3 = new byte[stream3.Length];
                stream3.Position = 0L;
                stream3.Read(buffer3, 0, Convert.ToInt32(stream3.Length));
                OracleParameter[] parameterArray3 = new OracleParameter[] { new OracleParameter(":图像", 0x66) };
                parameterArray3[0].Value = buffer3;
                string str18 = Guid.NewGuid().ToString();
                OracleHelper.OraExeNonQuery("Insert into 检验图像结果(Id,记录ID,图像类型,文件目录,图像) values('" + str18 + "','" + SaveResult.Appstr + "','bmp','RBC',:图像)", parameterArray3, ref msg);
            }
        }
    }

    public string SendSample(string strSample, ref short iSendStep, ref bool blnSuccess, string strResponse, bool blnUndo, short iType)
    {
        return null;
    }

    public void SetVariable(DataTable dt)
    {
    }
}

 