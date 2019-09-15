using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using System.Collections;
using System.Timers;

namespace ZLCHSLisComm
{
    public class Init_TXT
    {
        public string strInstrumentID;                                  //仪器ID
        public Boolean bnReadBatch;                                 //单个读取
        public string strError;

        string filePath;                                                      //文件路径
        string fileName;                                                   //文件名称
        string fileType;                                                     //文件类型
        string readType;                                                  //读取方式  
        string readEnd;                                                   // 读取后操作
        string dataRow;                                                  //数据开始行
        string dataEnd;                                                   //数据结束串
        string RemarkContent;//设备备注
        string inbuff;                                                       //传入缓冲
        string outbuff;                                                     //传出缓冲
        string detype;                                                     //单帧，多帧
        string datatype;                                                  //ascii,文本
        string databegin;                                                //数据开始位
        string dataend;                                                   //数据结束位
        string inbegin;                                                    //接收数据开始应答
        string inend;                                                      //接收数据结束应答
        string SubBegin;                                                //多帧时开始位
        string SubEnd;                                                   //多帧时结束位
        string Ack_term;                                                 //全部应答
        string Ack_all;                                                    //条件应答
        string resolveType;                                            //解析类型
        string CommProgramName;                               //通讯程序名
        System.Timers.Timer aTimer = new System.Timers.Timer();
        GetResultString resString = new GetResultString();
        DataSetHandle dsHandle = new DataSetHandle();
        DataSet ds = new DataSet();
        public object obj = null;                                             //定义反射对象
        public static IDataResolve IResolve;                            //定义数据解析接口
        DataTable VarDT = new DataTable();
        public void Start()
        {
            ds = dsHandle.GetDataSet(@"解析类型,通讯程序名,备注,Extractvalue(通讯参数, '/root/file_path') As 文件路径, Extractvalue(通讯参数, '/root/file_type') As 文件类型, Extractvalue(通讯参数, '/root/read_mode') As 读取方式,
                                    Extractvalue(通讯参数, '/root/read_end') As 读取后操作, Extractvalue(通讯参数, '/root/file_name') As 文件名, Extractvalue(通讯参数, '/root/data_beginrows') As 数据开始行,
                                    Extractvalue(通讯参数, '/root/date_end') As 数据结束串", "检验仪器", "id = '" + strInstrumentID + "'");
            VarDT=ds.Tables[0];
            filePath = ds.Tables[0].Rows[0]["文件路径"].ToString();
            fileName = ds.Tables[0].Rows[0]["文件名"].ToString();
            fileType = ds.Tables[0].Rows[0]["文件类型"].ToString();
            readType = ds.Tables[0].Rows[0]["读取方式"].ToString();
            readEnd = ds.Tables[0].Rows[0]["读取后操作"].ToString();
            dataRow = ds.Tables[0].Rows[0]["数据开始行"].ToString();
            dataEnd = ds.Tables[0].Rows[0]["数据结束串"].ToString();
            CommProgramName = ds.Tables[0].Rows[0]["通讯程序名"].ToString();
            resolveType = ds.Tables[0].Rows[0]["解析类型"].ToString();
            RemarkContent = ds.Tables[0].Rows[0]["备注"].ToString();
            ds = dsHandle.GetDataSet(@"Extractvalue(接收规则, '/root/buffer_in') As Buffer_In, Extractvalue(接收规则, '/root/buffer_out') As Buffer_out, 
                                       Extractvalue(接收规则, '/root/data_type') As data_type, Extractvalue(接收规则, '/root/data_begin') As data_begin, 
                                       Extractvalue(接收规则, '/root/data_end') As data_end, Extractvalue(接收规则, '/root/start_cmd') As start_cmd, 
                                       Extractvalue(接收规则, '/root/end_cmd') As end_cmd, Extractvalue(接收规则, '/root/Ack_all') As Ack_all, 
                                       Extractvalue(接收规则, '/root/ack_term') As ack_term, Extractvalue(接收规则, '/root/decode_mode') As decode_mode, 
                                       Extractvalue(接收规则, '/root/begin_bits') As begin_bits, Extractvalue(接收规则, '/root/end_bits') As end_bits",
                                       "检验仪器", "id = '" + strInstrumentID + "'");
            inbuff = ds.Tables[0].Rows[0]["buffer_in"].ToString();
            outbuff = ds.Tables[0].Rows[0]["buffer_out"].ToString();
            datatype = ds.Tables[0].Rows[0]["data_type"].ToString();
            databegin = ds.Tables[0].Rows[0]["data_begin"].ToString();
            dataend = ds.Tables[0].Rows[0]["data_end"].ToString();
            // inbegin = ds.Tables[0].Rows[0][""].ToString();
            // inend = ds.Tables[0].Rows[0][6].ToString();
            detype = ds.Tables[0].Rows[0]["decode_mode"].ToString();
            SubBegin = ds.Tables[0].Rows[0]["begin_bits"].ToString();
            SubEnd = ds.Tables[0].Rows[0]["end_bits"].ToString();
            Ack_term = ds.Tables[0].Rows[0]["ack_term"].ToString();
            Ack_all = ds.Tables[0].Rows[0]["Ack_all"].ToString();

            resString.strDetype = detype;
            resString.strInstrument_id = strInstrumentID;
            resString.strSubBegin = SubBegin;
            resString.strSubEnd = SubEnd;
            resString.strDataBegin = databegin;
            resString.strDataEnd = dataend;
            resString.listInputResult = new List<string>();
            resString.GetRules(strInstrumentID);

            //ds = dsHandle.GetDataSet("instrument_name", "test_instrument", "instrument_id = '" + strInstrumentID + "'");
            ds = dsHandle.GetDataSet("名称", "检验仪器", "id= '" + strInstrumentID + "'");
            if (resolveType == "1")
            {
                IResolve = resString;
            }
            else
            {
                obj = ObjectReflection.CreateObject(CommProgramName.Substring(0, CommProgramName.IndexOf(".dll")));
                IResolve = obj as IDataResolve;
                IResolve.GetRules(strInstrumentID);
            }


            if (fileType == "1")//单文件
            {
                // 
                if (readType == "3")//自动读取
                {
                    IResolve.SetVariable(VarDT);
                    AutoExecute(null, null);
                    aTimer.Elapsed += new ElapsedEventHandler(AutoExecute);
                    aTimer.Interval = 10000;
                    aTimer.Enabled = true;
                }
                else
                {
                    ReadFile(filePath, fileName, readType, fileType);
                }
                //resString.ParseResult();
            }
            else if (fileType == "2")//多文件
            {
                string[] strFiles = Directory.GetFiles(filePath, fileName);
                strFiles = FilterOutoffHiddenFiles(strFiles);
                foreach (string name in strFiles)
                {
                    if (string.IsNullOrEmpty(name)) continue;
                    //多文件读取一行保存一次
                    fileName = name.Replace(filePath + "\\", "");
                    resString.listInputResult = new List<string>();
                    ReadFile(filePath, fileName, readType, fileType);
                    //resString.ImmediatelyUpdate = true;
                    // resString.ParseResult();
                    //System.Threading.Thread.Sleep(2000);
                }
                int mm = 0;
            }

        }

        public void AutoExecute(object sender, EventArgs e)
        {
            Write_Log wl = new Write_Log();
            try
            {
                string strResult = "";
                string strReserved = "";
                string strCmd = "";
                //wl.Write(DateTime.Now.ToString()+" AUTO  \r\n", "log");
                string str = @"AAAI10P190000000139501072020111342013000090007011407005287849514403230901029013444501450931641340464000000000000000100000120500782550271930162360000000000000000000000000000000000000000000000000000000000001003004008014022029037045053057060059057057053049045045044041041038039039040038037038038037035033031033031031029031031031032031034033036036038039038036035034034034034035038041041042044043045044044045047050051055056060066067069073078080087092097104110112119131133139147152162171178184195203208217220226230229232238245249254253253255254250251249249248250251250245241241240242239239234231227219213203201193193190184180176172163156146141132125116114108106105101099094088084080074069063060056054050047046043041037035032030029027025024022021021019016014013012013011011010010009008008007007006006007007007006006005005004004004004004004004004004004004003004003003003002002002002002002002002002002003003002003003003003003003003003003000000000000000000000000000000000000000000000000000000000000000000000000000001001002003004004005005005005004004003003003002002002003003003004005006006008009012014017022025031038047057069080093107124141156172188201214227237241248253254255255253248242235226218207198185177169159151144138132128120116113109106102099096094091089085083081079077075073069069066063060058057054053050047045043039037035031029028026025023021019018016015013013012012012012011011010009008008008007007006006006006005005005004004004004004004003003004003003003003003003003003003002002002002002002003002003003002003002002002002002002001001001001001001001001001001001001001001001001001001001001001001000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001002003005007010014018022028035042050061072085098112126139152163175184194202211220229235242247253254255253252249246242238236235233231231231229228228228226225224223221220219218219221219218217217213210205200194188184180176173169166164163160157156156156156157158160162164166167169170171172173172171169167164161158155151148145143141139136133131129126124121118116114112111109108107106104103100097095094091089087086085085084083082081079077074072069067065064062060058057055053051049047046045045044044045046047048049050051053052052051051051051050049049049048048046045044044043042041041041042043044045047048050050051051051050049047045042040038037035034032030029028026024021019018018017017017018019020020021021021021021021021021021021";
                // IResolve.ParseResult(str, ref strResult, ref strReserved, ref strCmd);
               // IResolve.ParseResult("", ref strResult, ref strReserved, ref strCmd);
                // System.Windows.Forms.MessageBox.Show("dd");

                
                    string[] s = RemarkContent.Trim().Split('|');
                    if (s.Length > 0 && !string.IsNullOrEmpty(RemarkContent))
                    {
                        for (int i = 0; i < s.Length; i++)
                        {
                            if (s[i].IndexOf("targetDeviceID") != -1 && !string.IsNullOrEmpty(s[i].Split(':')[1]))
                            {
                                IResolve.ParseResult(str, ref strResult, ref s[i].Split(':')[1], ref strCmd);
                            }
                        }
                    }
                    else
                    {
                        IResolve.ParseResult(str, ref strResult, ref strReserved, ref strCmd);
                    }


            }
            catch (Exception ex)
            {
                wl.Write(ex.Message.ToString(), "log");
            }
        }

        /// <summary>
        /// 去除隐藏文件
        /// </summary>
        /// <param name="filenames">符合条件的文件名称数组</param>
        private string[] FilterOutoffHiddenFiles(string[] filenames)
        {
            string[] filenames1 = new string[filenames.Length];
            int i = 0;
            foreach (string str in filenames)
            {
                FileInfo fi = new FileInfo(str);
                if ((fi.Attributes & FileAttributes.Hidden) == 0)
                {
                    filenames1[i++] = str;
                }
            }
            return filenames1;
        }

        /// <summary>
        /// 读取文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="fileName">文件名称</param>
        /// <param name="readType">打开方式</param>
        /// <param name="fileType">文件类型</param>
        private void ReadFile(string filePath, string fileName, string readType, string fileType)
        {
            int readrow = 0;
            String inputStr;
            string RawStr;
            string strResult="";
            string strReserved="";
            string strCmd="";
            if (readType == "1")//按行
            {
                FileStream ifs = new FileStream(filePath + "\\" + fileName, FileMode.Open, FileAccess.Read);
                StreamReader SrReadLin = new StreamReader(ifs, System.Text.Encoding.Default);
                while (true)
                {
                    inputStr = SrReadLin.ReadLine();
                    readrow += 1;
                    if (readrow < Convert.ToInt32(string.IsNullOrEmpty(dataRow) ? "0" : dataRow)) continue;
                    if (String.IsNullOrEmpty(inputStr)) break;
                    if (inputStr.IndexOf(dataEnd) >= 0 && !string.IsNullOrEmpty(dataEnd)) break;
                    //文件类型为多文件
                    if (fileType == "2") inputStr = inputStr + fileName.Substring(0, fileName.IndexOf("."));




                    string[] s = RemarkContent.Trim().Split('|');
                    if (s.Length > 0)
                    {
                        for (int i = 0; i < s.Length; i++)
                        {
                            if (s[i].IndexOf("targetDeviceID") != -1 && !string.IsNullOrEmpty(s[i].Split(':')[1]))
                            {
                                IResolve.ParseResult(inputStr, ref strResult, ref s[i].Split(':')[1], ref strCmd);
                            }
                        }
                    }
                    else
                    {
                        IResolve.ParseResult(inputStr, ref strResult, ref strReserved, ref strCmd);
                    }


                  
                    //if (fileType == "单文件")
                    //{
                    //resString.listInputResult = new List<string>();
                    // resString.listInputResult.Add(inputStr);
                    //    resString.ParseResult();
                    //}
                }
                SrReadLin.Close();
                ifs.Close();
            }
            else if (readType == "2")//全部
            {
               // FileStream ifs = new FileStream(filePath + "\\" + fileName, FileMode.Open, FileAccess.Read);
                FileStream ifs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                StreamReader SrReadLin = new System.IO.StreamReader(ifs, System.Text.Encoding.Default);
                inputStr = SrReadLin.ReadToEnd();

                SrReadLin.Close();
                ifs.Close();




                string[] s = RemarkContent.Trim().Split('|');
                if (s.Length > 0)
                {
                    for (int i = 0; i < s.Length; i++)
                    {
                        if (s[i].IndexOf("targetDeviceID") != -1 && !string.IsNullOrEmpty(s[i].Split(':')[1]))
                        {
                            IResolve.ParseResult(inputStr, ref strResult, ref s[i].Split(':')[1], ref strCmd);
                        }
                    }
                }
                else
                {
                    IResolve.ParseResult(inputStr, ref strResult, ref strReserved, ref strCmd);
                }

                //while (true)
                //{
                //    RawStr= inputStr.Substring(inputStr.IndexOf(StrChange(databegin)), inputStr.IndexOf(StrChange(dataend)) - inputStr.IndexOf(databegin));
                //    resString.listInputResult.Add(RawStr);
                //    inputStr = inputStr.Substring(inputStr.IndexOf(StrChange(dataend)) + 1);
                //    if (!inputStr.Contains(StrChange(dataend)) || String.IsNullOrEmpty( StrChange(dataend)) ) break;                        
                //}
                
            }
        }

        private string StrChange(string SourceStr)
        {
            if (SourceStr.Contains("<") && SourceStr.Contains(">")) return ((char)int.Parse(SourceStr.Replace("<","").Replace(">",""))).ToString();
            else return SourceStr;
            //Contains包含
        }

    }
}