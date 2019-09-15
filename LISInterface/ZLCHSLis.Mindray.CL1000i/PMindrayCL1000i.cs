/*************************************
 * 名称：迈瑞系列仪器
 * 功能：迈瑞HL7双向通讯解析程序
 * 作者：谢天
 * 时间：2017-11-09
 * 通讯类型：网络
 * 备注:
 * ***********************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using ZLCHSLisComm;

namespace ZLCHSLis.Mindray.CL1000i
{
    public class PMindrayCL1000i
    {
        //本段落主要是用于基于HL7通讯协议的,主要是迈瑞公司的产品
        public const string QRYQ02 = "QRY^Q02"; //仪器向lis请求下载样本
        public const string ORUR01 = "ORU^R01"; //仪器向lis发送样本结果或者质控
        public string fengexian = "---fengexian---";
        public string fenggexiansend = "---fengexiansend---";
        public string replaceDSC = "??^^^???^^^????";//用于替换DSC结束符用
        public const int TM = 1;//指定样本条码下载样本
        public const int BH = 2;//指定样本编号范围下载样本
        public const int SJ = 3;//指定样本检测时间范围
        public const int ZK = 4;//质控
        public const int JG = 5;//样本结果       
        public char CR;//换行符
        public char SB;//开始符
        public char EB;//结束符
        public string QCK_Q02_NF, ORR_002;//没有查询到相关到样本信息,应答给仪器
        Write_Log writelog = new Write_Log();
        string StrDevice = "";
        private string StrDeviceID;
        private string sqlstart;
        private string sqlend;
        IniFile ConfigIni;
        string orgId;
        public PMindrayCL1000i()
        {
            CR = Convert.ToChar(13);//换行符
            SB = Convert.ToChar(11);//开始符
            EB = Convert.ToChar(28);//结束符
            QCK_Q02_NF = SB + @"MSH|^~\&|||||{0}||QCK^Q02|{1}|P|2.3.1||||||ASCII|||" + CR
                + "MSA|AA|{1}|Message accepted|||0|" + CR
                + "ERR|0|" + CR
                + "QAK|SR|NF|" + CR
                + EB + CR;
            ORR_002 = SB + @"MSH|^~\&|LIS||||{0}||ORR^O02|{1}|P|2.3.1||||||UNICODE" + CR
                         + "MSA|AR|9" + CR
                         + EB + CR;

            //获取机构id
            ConfigIni = new IniFile("SOLVESET.INI");
            orgId = ConfigIni.IniReadValue("EQUIPMENT", "Agencies");
            sqlstart = @"select 病人姓名,性别,出生日期,血型,病人类型,收费类型,住院号,床号,样本条码,to_number(样本编号)  样本编号,样本送检时间,是否急诊,送检医生,送检科室,样本类型,仪器模式,临床诊断,检验备注,wm_concat(项目编号)  项目编号
from
(Select distinct u.姓名 as 病人姓名,
       Decode(u.性别,
              '0',
              '未知的性别',
              '1',
              '男',
              '2',
              '女',
              '未说明的性别') 性别,
       to_char(u.出生日期, 'yyyymmddhh24miss') as 出生日期,
       Nvl(s.血型, '不详') 血型,
       Decode(Nvl(z.状态, 0), 1, '门诊', 2, '住院', '体检及其他') 病人类型,
       '' as 收费类型,
       z.住院号,
       z.床号,
       j.样本条码,
       j.样本序号 as 样本编号,
       to_char(j.核收时间, 'yyyymmddhh24miss') as 样本送检时间,
       Decode(Nvl(j.紧急申请, 0), 0, '否', 1, '是') 是否急诊,
       j.核收人 as 送检医生,
       b.简称 送检科室,
       j.样本类型,
       j.仪器模式,
       extractvalue(a.医嘱附注, '/root/diagnosis') As 临床诊断,
       j.检验备注,
       c.通道编码 项目编号
  From 个人信息 u,
       检验记录 j,
       部门 b,
       仪器检测项目 c,
       个人当前状态 z,
       检验申请项目 sq,
       检验报告项目 bg,
       (Select s.个人id, x.名称 血型
          From 个人既往史 s, Abo血型 x
         Where s.结果码 = x.编码(+)
           And s.项目码 = '01') s,
       个人医嘱记录 a
 Where u.Id = j.个人id
   And j.执行科室id(+) = b.资源id 
   and j.id = sq.记录id
   and bg.项目id = sq.项目id
   and a.id(+) = j.id
   and bg.报告项id = c.项目id
   And j.个人id = z.个人id(+)  
   And j.个人id = s.个人id(+) and j.仪器id='" + "<StrDeviceID>" + "'  And j.机构id = '" + orgId + @"'";
            sqlend = @")Group By 病人姓名,性别,出生日期,血型,病人类型,收费类型,住院号,床号,样本条码,样本编号,样本送检时间,是否急诊,送检医生,送检科室,样本类型,仪器模式,临床诊断,检验备注  order by 样本编号  ";


        }
        //针对QRY的请求作出QCK响应,BC5380系列,BC5180
        public string sendQCKXCG(string QRY, string StrDevice1, string StrDeviceID)
        {
            StrDevice = StrDevice1;
            this.StrDeviceID = StrDeviceID;
            sqlstart = sqlstart.Replace("<StrDeviceID>", StrDeviceID);
            string strTestTime = DateTime.Now.ToString("yyyyMMddHHmmss");
            string result = ""; ;
            string analysisResult = analysisQRY(QRY);
            //如果返回为空的话,//无对应样本应答
            if ("".Equals(analysisResult))
            {
                return "";
            }
            else
            {
                //规则: 查询下载样本类型|时间|消息控制id|样本条码    
                int queryType = Convert.ToInt16(analysisResult.Split('|')[0]);
                string messageTime = analysisResult.Split('|')[1];
                string messageID = analysisResult.Split('|')[2];
                //条码号或者是样本编号范围
                string sampleNO = analysisResult.Split('|')[3];
                switch (queryType)
                {
                    case TM: result = string.Format(querySampleInfoXCG(sampleNO), strTestTime, messageID); break;
                    case BH: result = string.Format(querySampleInfoXCG(sampleNO, 1), strTestTime, messageID); break;
                    case SJ: result = string.Format(querySampleInfo(), strTestTime, messageID); break;
                    //如果是质控或者结果到话原样返回
                    case JG: result = analysisResult; break;
                    case ZK: result = analysisResult; break;
                }
                return result;
            }
        }
        //针对QRY的请求作出QCK响应,BS系列,CL系列
        public string sendQCK(string QRY, string StrDevice1, string StrDeviceID)
        {
            StrDevice = StrDevice1;
            this.StrDeviceID = StrDeviceID;
            sqlstart = sqlstart.Replace("<StrDeviceID>", StrDeviceID);
            string strTestTime = DateTime.Now.ToString("yyyyMMddHHmmss");
            string result = ""; ;
            string analysisResult = analysisQRY(QRY);
            //如果返回为空的话,//无对应样本应答
            if ("".Equals(analysisResult))
            {
                return "";
            }
            else
            {
                //规则: 查询下载样本类型|时间|消息控制id|样本条码    
                int queryType = Convert.ToInt16(analysisResult.Split('|')[0]);
                string messageTime = analysisResult.Split('|')[1];
                string messageID = analysisResult.Split('|')[2];
                //条码号或者是样本编号范围
                string sampleNO = analysisResult.Split('|')[3];
                switch (queryType)
                {
                    case TM: result = string.Format(querySampleInfo(sampleNO), strTestTime, messageID); break;
                    case BH: result = string.Format(querySampleInfo(sampleNO, 1), strTestTime, messageID); break;
                    case SJ: result = string.Format(querySampleInfo(), strTestTime, messageID); break;
                    //如果是质控或者结果到话原样返回
                    case JG: result = analysisResult; break;
                    case ZK: result = analysisResult; break;
                }
                return result;
            }
        }
        //1指定单个样本条码查询
        public DataTable GetSampleInfo(string sampleNO)
        {

            string sql = "";
            if (sampleNO.Length > 0)
            {
                sql = sqlstart + @" And j.样本条码 = '" + sampleNO.Trim() + "'" + sqlend;
            }

            writelog.Write("sqllog", sql, "log");
            DataTable dt = OracleHelper.GetDataTable(sql);
            if (dt != null && dt.Rows.Count > 0)
            {
                return dt;
            }
            else
            {
                return null;
            }
        }
        public DataTable GetSampleInfoXC8001(string sampleNO, string StrDeviceID)
        {

            string sql = "";
            if (sampleNO.Length > 0)
            {
                sql = sqlstart + @" And j.样本条码 = '" + sampleNO.Trim() + "'" + sqlend;
            }
            sql = sql.Replace("<StrDeviceID>", StrDeviceID);
            writelog.Write("sqllog", sql, "log");
            DataTable dt = OracleHelper.GetDataTable(sql);
            if (dt != null && dt.Rows.Count > 0)
            {
                return dt;
            }
            else
            {
                return null;
            }
        }
        //2指定样本编号范围查询
        private DataTable GetSampleInfo(string sampleNoRange, int flag)
        {
            string[] s = sampleNoRange.Split(new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries);
            string no = "";
            foreach (string item in s)
            {
                no = no + item + ",";
            }
            no = no.Remove(no.Length - 1);

            string sql = "";
            sql = sqlstart + @" And j.样本序号  in(" + no + ")";
            sql = sql + @" And j.核收时间   between   trunc(sysdate) and   sysdate" + sqlend;
            writelog.Write("sqllog", sql, "log");
            DataTable dt = OracleHelper.GetDataTable(sql);
            if (dt != null && dt.Rows.Count > 0)
            {
                return dt;
            }
            else
            {
                return null;
            }
        }
        //3时间范围查询,查询当天到所有结果
        private DataTable GetSampleInfo()
        {




            string sql = "";
            sql = sqlstart + @" And j.核收时间   between   trunc(sysdate) and   sysdate" + sqlend;

            writelog.Write("sqllog", sql, "log");
            DataTable dt = OracleHelper.GetDataTable(sql);
            if (dt != null && dt.Rows.Count > 0)
            {
                return dt;
            }
            else
            {
                return null;
            }
        }
        //1指定单个样本条码查询
        private string querySampleInfo(string sampleBar)
        {
            string result = "";
            DataTable qckTable = this.GetSampleInfo(sampleBar);
            if (qckTable != null && qckTable.Rows.Count > 0)
            {
                result = this.getTableInfo(qckTable);
                return result;
            }
            else
            {
                writelog.Write(StrDevice, "没有查询到条码号:" + sampleBar, "log");
                return QCK_Q02_NF;
            }
        }
        //2指定样本编号范围查询
        private string querySampleInfo(string sampleNO, int flag)
        {
            string result = "";
            DataTable qckTable = this.GetSampleInfo(sampleNO, 1);
            if (qckTable != null && qckTable.Rows.Count > 0)
            {
                result = this.getTableInfo(qckTable);
                return result;
            }
            else
            {
                writelog.Write(StrDevice, "没有查询到样本编号范围:" + sampleNO, "log");
                return QCK_Q02_NF;
            }
        }
        //3时间范围查询,查询当天到所有结果
        private string querySampleInfo()
        {

            string result = "";
            DataTable qckTable = this.GetSampleInfo();
            if (qckTable != null && qckTable.Rows.Count > 0)
            {
                result = this.getTableInfo(qckTable);
                return result;
            }
            else
            {
                writelog.Write(StrDevice, "当天时间范围内没有获取到样本信息", "log");
                return QCK_Q02_NF;
            }

        }
        //a定单个样本条码查询血常规
        private string querySampleInfoXCG(string sampleBar)
        {
            string result = "";
            DataTable qckTable = this.GetSampleInfo(sampleBar);
            if (qckTable != null && qckTable.Rows.Count > 0)
            {
                result = this.getTableInfoXCG(qckTable);
                return result;
            }
            else
            {
                writelog.Write(StrDevice, "querySampleInfoXCG->没有查询到条码号:" + sampleBar, "log");
                return this.ORR_002;
            }
        }
        //b定单个样本编号查询血常规
        private string querySampleInfoXCG(string sampleBar, int flag)
        {
            string result = "";
            DataTable qckTable = this.GetSampleInfo(sampleBar, 1);
            if (qckTable != null && qckTable.Rows.Count > 0)
            {
                result = this.getTableInfoXCGBH(qckTable);
                return result;
            }
            else
            {
                writelog.Write(StrDevice, "querySampleInfoXCG->没有查询到条码号:" + sampleBar, "log");
                return this.ORR_002;
            }
        }
        //处理从zlbh里面得到到样本信息
        private string getTableInfo(DataTable dt)
        {
            DataTable qckTable = dt;
            string result = "";
            int i = 0;
            foreach (DataRow item in qckTable.Rows)
            {
                i++;
                string name = item["病人姓名"].ToString();
                string sex = item["性别"].ToString();
                if (sex.Equals("男"))
                {
                    sex = "M";
                }
                else if (sex.Equals("女"))
                {
                    sex = "W";
                }
                else
                {
                    sex = "O";
                }
                string birth = item["出生日期"].ToString();
                string bloodType = item["血型"].ToString();
                string patientType = item["病人类型"].ToString();
                if (patientType.Equals("门诊"))
                {
                    patientType = "outpatient";
                }
                else if (patientType.Equals("住院"))
                {
                    patientType = "inpatient";
                }
                else
                {
                    patientType = "Other";
                }
                string moneyType = item["收费类型"].ToString();
                string patientNum = item["住院号"].ToString();
                string bedNum = item["床号"].ToString();
                string sampleBar = item["样本条码"].ToString();
                string sampleNum = item["样本编号"].ToString();
                string sampleSendTime = item["样本送检时间"].ToString();
                string isEmergency = item["是否急诊"].ToString();
                if (isEmergency.Equals("是"))
                {
                    isEmergency = "Y";
                }
                else if (isEmergency.Equals("否"))
                {
                    isEmergency = "N";
                }
                else
                {
                    isEmergency = "N";
                }
                string sampleType = item["样本类型"].ToString();
                string detectDoctor = item["送检医生"].ToString();
                string dept = item["送检科室"].ToString();
                string sampleItems = item["项目编号"].ToString();
                //有对应样本应答
                string QCK_Q02_OK = SB + @"MSH|^~\&|||||{0}||QCK^Q02|{1}|P|2.3.1||||||ASCII|||" + CR
                  + "MSA|AA|{1}|Message accepted|||0|" + CR
                  + "ERR|0|" + CR
                  + "QAK|SR|OK|" + CR
                  + EB + CR;
                //只有第一条才有
                //if (i == 1)
                //    result = result + QCK_Q02_OK + fenggexiansend;
                //QCK_Q02_NF = string.Format(QCK_Q02_303231211", 4);
                //有对应样本的情况下，返回病人，样本，项目信息DSR_Q03
                string DSR_Q03 = SB + @"MSH|^~\&|||||{0}||DSR^Q03|{1}|P|2.3.1||||||ASCII|||" + CR
                    + "MSA|AA|{1}|Message accepted|||0|" + CR
                    + "ERR|0|" + CR
                    + "QAK|SR|OK|" + CR
                    + "QRD|{0}|R|D|2|||RD||OTH|||T|" + CR
                    + "QRF||||||RCT|COR|ALL||" + CR
                    + "DSP|1||" + patientNum + "|||" + CR
                    + "DSP|2||" + bedNum + "|||" + CR
                    + "DSP|3||" + name + "|||" + CR
                    + "DSP|4||" + birth + "|||" + CR
                    + "DSP|5||" + sex + "|||" + CR
                    + "DSP|6||" + bloodType + "|||" + CR
                    + "DSP|7|||||" + CR
                    + "DSP|8|||||" + CR
                    + "DSP|9|||||" + CR
                    + "DSP|10|||||" + CR
                    + "DSP|11|||||" + CR
                    + "DSP|12|||||" + CR
                    + "DSP|13|||||" + CR
                    + "DSP|14|||||" + CR
                    + "DSP|15||" + patientType + "|||" + CR
                    + "DSP|16|||||" + CR
                    + "DSP|17|||||" + CR
                    + "DSP|18|||||" + CR
                    + "DSP|19|||||" + CR
                    + "DSP|20|||||" + CR
                    + "DSP|21||" + sampleBar + "|||" + CR
                    + "DSP|22||" + sampleNum + "|||" + CR
                    + "DSP|23||" + sampleSendTime + "|||" + CR
                    + "DSP|24||" + isEmergency + "|||" + CR
                    + "DSP|25|||||" + CR
                    + "DSP|26||" + sampleType + "|||" + CR
                    + "DSP|27||" + detectDoctor + "|||" + CR
                    + "DSP|28||" + dept + "|||" + CR;
                DSR_Q03 += assembleDSP(sampleItems);
               
                if (i == qckTable.Rows.Count)
                    result = result + DSR_Q03 + "DSC|" + "" + "|" + CR + EB + CR + fengexian;
                else
                result = result + DSR_Q03 + "DSC|" + i + "|" + CR + EB + CR + fengexian;
                writelog.Write(StrDevice, result, "log");
            }
            return result;

        }
        //处理从zlbh里面得到到样本信息(XCG)(样本编号为条码单格式的方式)
        private string getTableInfoXCG(DataTable dt)
        {
            DataTable qckTable = dt;
            string result = "";
            foreach (DataRow item in qckTable.Rows)
            {
                string name = item["病人姓名"].ToString();

                string sex = item["性别"].ToString();

                if (sex.Equals("男"))
                {
                    sex = "M";
                }
                else if (sex.Equals("女"))
                {
                    sex = "F";
                }
                else
                {
                    sex = "O";
                }
                string birth = item["出生日期"].ToString();
                string year = birth.Substring(0, 4);
                string mounth = birth.Substring(4, 2);
                string day = birth.Substring(6, 2);
                DateTime birthDate = Convert.ToDateTime(year + "-" + mounth + "-" + day);
                System.TimeSpan t3 = System.DateTime.Now - birthDate;
                int age = Convert.ToInt16(t3.TotalDays / 365);
                string bloodType = item["血型"].ToString();
                string patientType = item["病人类型"].ToString();
                string detectMode = item["仪器模式"].ToString();
                if (patientType.Equals("门诊"))
                {
                    patientType = "outpatient";
                }
                else if (patientType.Equals("住院"))
                {
                    patientType = "inpatient";
                }
                else
                {
                    patientType = "Other";
                }
                string moneyType = item["收费类型"].ToString();
                string patientNum = item["住院号"].ToString();
                string bedNum = item["床号"].ToString();
                string sampleBar = item["样本条码"].ToString();
                string sampleNum = item["样本编号"].ToString();
                string sampleSendTime = item["样本送检时间"].ToString();
                string isEmergency = item["是否急诊"].ToString();
                string diagnosis = item["临床诊断"].ToString();
                string RemarkContent = item["检验备注"].ToString();
                if (isEmergency.Equals("是"))
                {
                    isEmergency = "Y";
                }
                else if (isEmergency.Equals("否"))
                {
                    isEmergency = "N";
                }
                else
                {
                    isEmergency = "N";
                }
                string sampleType = item["样本类型"].ToString();
                string detectDoctor = item["送检医生"].ToString();
                string dept = item["送检科室"].ToString();
                string sampleItems = item["项目编号"].ToString();
                //有对应样本应答
                string i;
                i = SB + @"MSH|^~\&|LIS||||{0}||ORR^O02|{1}|P|2.3.1||||||UNIDCODE|||" + CR
                       + @"MSA|AA|{1}" + CR;


                i = i + @"PID|1||<PATNO>^^^^MR||^<PATNA>||<BIRTHDAY>|<PATSEX><CR>";
                i = i + @"PV1|1|<PARTTYPE>|<dept>^^<BEDNO>|||||||||||||||||自费<CR>";
                i = i + @"ORC|AF|<RCODE>|||<CR>";
                i = i + @"OBR|1|<RCODE>||||<OBR06>||||<OBR10>|||<OBR13>|<OBR14>||||||||20160501||HM||||<OBR28>||||<OBR32><CR>";
                i = i + @"OBX|1|IS|08001^Take Mode^99MRC||<TAKEMODE>||||||F<CR>";
                i = i + @"OBX|2|IS|08002^Blood Mode^99MRC||<BLOODMODE>||||||F<CR>";
                i = i + @"OBX|3|IS|08003^Test Mode^99MRC||<TESTMODE>||||||F<CR>";
                i = i + @"OBX|4|IS|01002^Ref Group^99MRC||<REFGROUP>||||||F<CR>";
                i = i + @"OBX|5|NM|30525-0^Age^LN||<PATAGE>|yr|||||F<CR>";
                i = i + @"OBX|6|ST|01001^Remark^99MRC||<RemarkContent>||||||F<CR>";
                i = i + @"<EB><CR>";
                i = i.Replace("<CR>", this.CR + "");
                i = i.Replace("<EB>", this.EB + "");
                i = i.Replace("<PATNO>", patientNum);//病历号，这个地方取住院号
                i = i.Replace("<PATNA>", name);
                i = i.Replace("<BIRTHDAY>", birth);
                i = i.Replace("<PATSEX>", sex.Replace("M", "男").Replace("F", "女").Replace("O", "未知"));
                i = i.Replace("<PARTTYPE>", patientType.Replace("outpatient", "门诊").Replace("inpatient", "住院"));
                i = i.Replace("<dept>", dept);
                i = i.Replace("<BEDNO>", bedNum);
                // i = i.Replace("<RCODE>", sampleNum);
                // 把条码号替换为样本号回传给仪器
                i = i.Replace("<RCODE>", sampleBar);
                i = i.Replace("<OBR06>", sampleSendTime);
                i = i.Replace("<OBR10>", detectDoctor);
                i = i.Replace("<OBR13>", "");//疾病诊断
                i = i.Replace("<OBR14>", sampleSendTime);
                i = i.Replace("<OBR28>", detectDoctor);
                i = i.Replace("<OBR32>", detectDoctor);
                i = i.Replace("<RemarkContent>", diagnosis);
                string BLOODMODE = "W";
                string TAKEMODE = "A";
                string TESTMODE = "CBC+DIFF";
                if ("".Equals(bedNum))
                {
                    BLOODMODE = "P"; TAKEMODE = "C";
                }
                //全血;预稀释;全血+CBC
                if ("全血".Equals(detectMode))
                {
                    BLOODMODE = "W"; TAKEMODE = "A";
                }
                if ("预稀释".Equals(detectMode))
                {
                    BLOODMODE = "P"; TAKEMODE = "C";
                }
                if ("全血计数".Equals(detectMode))
                {
                    TESTMODE = "CBC"; BLOODMODE = "W"; TAKEMODE = "A";
                }
                i = i.Replace("<TAKEMODE>", TAKEMODE);
                i = i.Replace("<BLOODMODE>", BLOODMODE);
                i = i.Replace("<TESTMODE>", TESTMODE);
                i = i.Replace("<PATAGE>", age + "");
                i = i.Replace("<AGEUNIT>", age * 365 * 24 + "");
                //进样模式（Take Mode）  取值为以下枚举：“O” - 开放“A” - 自动“C” – 封闭
                //血样模式（Blood Mode）取值为以下枚举：“W”- 全血  “P” - 预稀释
                //测量模式（Test Mode）取值为以下枚举：“CBC”“CBC+DIFF“
                //血型  形式为“AB血型 RH血型”其中AB血型取值有“A”、“B”、“AB”、“O”四种RH血型取值有“RH+”、“RH-”两种。
                //质控级别（Qc Level）取以下枚举值：“L”- 低“M”- 中“H”- 高

                if (age < 1)
                { i = i.Replace("<REFGROUP>", "新生儿"); }
                else
                {
                    if (age < 13)
                    {
                        i = i.Replace("<REFGROUP>", "儿童");
                    }
                    else
                    {
                        if (sex.Equals("W"))
                        {
                            i = i.Replace("<REFGROUP>", "女");
                        }
                        else
                        {
                            i = i.Replace("<REFGROUP>", "男");
                        }
                    }

                }
                result = result + i;
                writelog.Write(StrDevice, "getTableInfoXCG->" + result, "log");
            }
            return result;

        }
        //处理从zlbh里面得到到样本信息(XCG)(样本编号的方式)
        private string getTableInfoXCGBH(DataTable dt)
        {
            DataTable qckTable = dt;
            string result = "";
            foreach (DataRow item in qckTable.Rows)
            {
                string name = item["病人姓名"].ToString();

                string sex = item["性别"].ToString();

                if (sex.Equals("男"))
                {
                    sex = "M";
                }
                else if (sex.Equals("女"))
                {
                    sex = "F";
                }
                else
                {
                    sex = "O";
                }
                string birth = item["出生日期"].ToString();
                string year = birth.Substring(0, 4);
                string mounth = birth.Substring(4, 2);
                string day = birth.Substring(6, 2);
                DateTime birthDate = Convert.ToDateTime(year + "-" + mounth + "-" + day);
                System.TimeSpan t3 = System.DateTime.Now - birthDate;
                int age = Convert.ToInt16(t3.TotalDays / 365);
                string bloodType = item["血型"].ToString();
                string patientType = item["病人类型"].ToString();
                string detectMode = item["仪器模式"].ToString();
                if (patientType.Equals("门诊"))
                {
                    patientType = "outpatient";
                }
                else if (patientType.Equals("住院"))
                {
                    patientType = "inpatient";
                }
                else
                {
                    patientType = "Other";
                }
                string moneyType = item["收费类型"].ToString();
                string patientNum = item["住院号"].ToString();
                string bedNum = item["床号"].ToString();
                string sampleBar = item["样本条码"].ToString();
                string sampleNum = item["样本编号"].ToString();
                string sampleSendTime = item["样本送检时间"].ToString();
                string isEmergency = item["是否急诊"].ToString();
                string diagnosis = item["临床诊断"].ToString();
                string RemarkContent = item["检验备注"].ToString();
                if (isEmergency.Equals("是"))
                {
                    isEmergency = "Y";
                }
                else if (isEmergency.Equals("否"))
                {
                    isEmergency = "N";
                }
                else
                {
                    isEmergency = "N";
                }
                string sampleType = item["样本类型"].ToString();
                string detectDoctor = item["送检医生"].ToString();
                string dept = item["送检科室"].ToString();
                string sampleItems = item["项目编号"].ToString();
                //有对应样本应答
                string i;
                i = SB + @"MSH|^~\&|LIS||||{0}||ORR^O02|{1}|P|2.3.1||||||UNIDCODE|||" + CR
                       + @"MSA|AA|{1}" + CR;


                i = i + @"PID|1||<PATNO>^^^^MR||^<PATNA>||<BIRTHDAY>|<PATSEX><CR>";
                i = i + @"PV1|1|<PARTTYPE>|<dept>^^<BEDNO>|||||||||||||||||自费<CR>";
                i = i + @"ORC|AF|<RCODE>|||<CR>";
                i = i + @"OBR|1|<RCODE>||||<OBR06>||||<OBR10>|||<OBR13>|<OBR14>||||||||20160501||HM||||<OBR28>||||<OBR32><CR>";
                i = i + @"OBX|1|IS|08001^Take Mode^99MRC||<TAKEMODE>||||||F<CR>";
                i = i + @"OBX|2|IS|08002^Blood Mode^99MRC||<BLOODMODE>||||||F<CR>";
                i = i + @"OBX|3|IS|08003^Test Mode^99MRC||<TESTMODE>||||||F<CR>";
                i = i + @"OBX|4|IS|01002^Ref Group^99MRC||<REFGROUP>||||||F<CR>";
                i = i + @"OBX|5|NM|30525-0^Age^LN||<PATAGE>|yr|||||F<CR>";
                i = i + @"OBX|6|ST|01001^Remark^99MRC||<RemarkContent>||||||F<CR>";
                i = i + @"<EB><CR>";
                i = i.Replace("<CR>", this.CR + "");
                i = i.Replace("<EB>", this.EB + "");
                i = i.Replace("<PATNO>", patientNum);//病历号，这个地方取住院号
                i = i.Replace("<PATNA>", name);
                i = i.Replace("<BIRTHDAY>", birth);
                i = i.Replace("<PATSEX>", sex.Replace("M", "男").Replace("F", "女").Replace("O", "未知"));
                i = i.Replace("<PARTTYPE>", patientType.Replace("outpatient", "门诊").Replace("inpatient", "住院"));
                i = i.Replace("<dept>", dept);
                i = i.Replace("<BEDNO>", bedNum);
                i = i.Replace("<RCODE>", sampleNum);
                // 把条码号替换为样本号回传给仪器
                //  i = i.Replace("<RCODE>", sampleBar);
                i = i.Replace("<OBR06>", sampleSendTime);
                i = i.Replace("<OBR10>", detectDoctor);
                i = i.Replace("<OBR13>", "");//疾病诊断
                i = i.Replace("<OBR14>", sampleSendTime);
                i = i.Replace("<OBR28>", detectDoctor);
                i = i.Replace("<OBR32>", detectDoctor);
                i = i.Replace("<RemarkContent>", diagnosis);
                string BLOODMODE = "W";
                string TAKEMODE = "A";
                string TESTMODE = "CBC+DIFF";
                if ("".Equals(bedNum))
                {
                    BLOODMODE = "P"; TAKEMODE = "C";
                }
                //全血;预稀释;全血+CBC
                if ("全血".Equals(detectMode))
                {
                    BLOODMODE = "W"; TAKEMODE = "A";
                }
                if ("预稀释".Equals(detectMode))
                {
                    BLOODMODE = "P"; TAKEMODE = "C";
                }
                if ("全血计数".Equals(detectMode))
                {
                    TESTMODE = "CBC"; BLOODMODE = "W"; TAKEMODE = "A";
                }
                i = i.Replace("<TAKEMODE>", TAKEMODE);
                i = i.Replace("<BLOODMODE>", BLOODMODE);
                i = i.Replace("<TESTMODE>", TESTMODE);
                i = i.Replace("<PATAGE>", age + "");
                i = i.Replace("<AGEUNIT>", age * 365 * 24 + "");
                //进样模式（Take Mode）  取值为以下枚举：“O” - 开放“A” - 自动“C” – 封闭
                //血样模式（Blood Mode）取值为以下枚举：“W”- 全血  “P” - 预稀释
                //测量模式（Test Mode）取值为以下枚举：“CBC”“CBC+DIFF“
                //血型  形式为“AB血型 RH血型”其中AB血型取值有“A”、“B”、“AB”、“O”四种RH血型取值有“RH+”、“RH-”两种。
                //质控级别（Qc Level）取以下枚举值：“L”- 低“M”- 中“H”- 高

                if (age < 1)
                { i = i.Replace("<REFGROUP>", "新生儿"); }
                else
                {
                    if (age < 13)
                    {
                        i = i.Replace("<REFGROUP>", "儿童");
                    }
                    else
                    {
                        if (sex.Equals("W"))
                        {
                            i = i.Replace("<REFGROUP>", "女");
                        }
                        else
                        {
                            i = i.Replace("<REFGROUP>", "男");
                        }
                    }

                }
                result = result + i;
                writelog.Write(StrDevice, "getTableInfoXCG->" + result, "log");
            }
            return result;

        }
        //解析仪器发过来的请求
        private string analysisQRY(string QRY)
        {
            string returnVal = "";
            string[] analysis = QRY.Split('|');


            if (analysis[8].Equals(QRYQ02))
            {
                //判断是否是指定条码下载样本
                if (analysis[28] != null && !analysis[28].Equals(""))
                {
                    //规则: 查询下载样本类型|时间|消息控制id|样本条码

                    //如果满足2,3,4-8这样到规则就表示是范围
                    if (analysis[28].Contains("-") || analysis[28].Contains(",") || analysis[28].Length < 8)
                    {
                        string fenxi = "";
                        string strAccept = analysis[28];
                        string[] sNo = strAccept.Split(',');
                        foreach (string item in sNo)
                        {
                            if (item.Contains("-"))
                            {
                                int sss_start = Convert.ToInt16(item.Split('-')[0]);
                                int sss_end = Convert.ToInt16(item.Split('-')[1]);
                                for (int i = sss_start; i <= sss_end; i++)
                                {
                                    fenxi = fenxi + i + "^";
                                }
                            }
                            else
                            {
                                fenxi = fenxi + item + "^";
                            }

                        }
                        //通过条码位置自定义输入样本范围
                        //规则: 查询下载样本类型|时间|消息控制id|样本号1^样本号2.....
                        returnVal = BH + "|" + analysis[6] + '|' + analysis[9] + '|' + fenxi;
                    }
                    else
                    {
                        //规则: 查询下载样本类型|时间|消息控制id|样本条码
                        returnVal = TM + "|" + analysis[6] + '|' + analysis[9] + '|' + analysis[28];
                    }
                }
                else if (analysis[37] != null && !analysis[37].Equals(""))
                {
                    //规则: 查询下载样本类型|时间|消息控制id|样本开始编号|样本结束编号
                    string fenxi = "";
                    //analysis[37]开始编号,analysis[38]结束编号
                    int sss_start = Convert.ToInt16(analysis[37]);
                    int sss_end = Convert.ToInt16(analysis[38]);
                    for (int i = sss_start; i <= sss_end; i++)
                    {
                        fenxi = fenxi + i + "^";
                    }
                    //规则: 查询下载样本类型|时间|消息控制id|样本号1^样本号2.....
                    returnVal = BH + "|" + analysis[6] + '|' + analysis[9] + '|' + fenxi;
                }
                else
                {
                    //规则: 查询下载样本类型|时间|消息控制id|测试时间开始|测试时间结束
                    returnVal = SJ + "|" + analysis[6] + '|' + analysis[9] + '|' + analysis[35] + '|' + analysis[36];
                }

            }
            //血常规BC系列
            if (analysis[8].Equals("ORM^O01"))
            {   //如果接收的样本号大于8位那么处理为条码号
                if (analysis[20].Length >= 8)
                    //规则: 查询下载样本类型|时间|消息控制id|条码号
                    returnVal = TM + "|" + analysis[6] + '|' + analysis[9] + '|' + analysis[20];
                else
                    //规则: 查询下载样本类型|时间|消息控制id|样本号
                    returnVal = BH + "|" + analysis[6] + '|' + analysis[9] + '|' + analysis[20];

            }
            if (analysis[8].Equals(ORUR01))
            {
                //如果第16个是0到话那么就是样本结果
                if (analysis[15].Equals("0") || analysis[15].Equals(""))
                {
                    //规则: 查询下载样本类型|时间|消息控制id|值空
                    returnVal = JG + "|" + analysis[6] + '|' + analysis[9] + '|' + "" + '|' + "";
                }
                //如果第16个是2到话那么就是质控
                if (analysis[15].Equals("2"))
                {

                    //规则: 查询下载样本类型|时间|消息控制id|值空
                    returnVal = ZK + "|" + analysis[6] + '|' + analysis[9] + '|' + "" + '|' + "";
                }
                if (analysis[10].Equals("Q"))
                {
                    //规则: 查询下载样本类型|时间|消息控制id|值空
                    returnVal = ZK + "|" + analysis[6] + '|' + analysis[9] + '|' + "" + '|' + "";
                }

            }
            return returnVal;

        }
        //item表示是项目的通道码 规则:通道码1|通道码2|通道码3
        private string assembleDSP(string item)
        {
            string returnVal = "";
            int countDSP = 29;
            string[] items = item.Split(',');
            foreach (string s in items)
            {
                returnVal = returnVal + "DSP|" + countDSP++ + "||" + s + "^^^|||" + CR;

            }
            return returnVal;
        }

    }
}
