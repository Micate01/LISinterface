/****
 * ===1===start
 * �޸�ʱ��:2017-09-12
 * �޸���:л��
 * �޸�����:֧�ֶ��߳�,�ܹ�ͬʱ����������ݿ����ͽ���ͬʱ����
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
        public string status;//�Ƿ�����
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
        DataTable tItemChannel = new DataTable();          //��Ŀͨ��
        List<string> TestGraph = new List<string>();        //ͼ���б�
        DataSet ds = new DataSet();
        DataSet dsResult;
        Write_Log writeLog = new Write_Log();
        System.Timers.Timer aTimer = new System.Timers.Timer();
        System.Timers.Timer aTimer1 = new System.Timers.Timer();
        public object obj = null;                                             //���巴�����
        public static IDataResolve IResolve;                            //�������ݽ����ӿ�
        string resolveType;                                            //��������
        string CommProgramName;                               //ͨѶ������
        string strDevice;
        //DATABASE
        /// <summary>
        /// ��ʼ��ִ��
        /// </summary>
        public void Init( )
        {
            status = "open";
            ds = dsHandle.GetDataSet(@"��������,ͨѶ������,��ע,
                                                        Extractvalue(ͨѶ����, '/root/db_type') as db_type,
                                                        Extractvalue(ͨѶ����, '/root/db_name') as db_name,
                                                        Extractvalue(ͨѶ����, '/root/user_name') as user_name,
                                                        Extractvalue(ͨѶ����, '/root/password') as password,
                                                        Extractvalue(ͨѶ����, '/root/server_name') as server_name,
                                                        Extractvalue(ͨѶ����, '/root/parastr') as parastr,
                                                        Extractvalue(ͨѶ����, '/root/selectstr') as selectstr", "��������", "id = '" + strInstrumentID + "'");
            databasetype = ds.Tables[0].Rows[0]["db_type"].ToString();           
            servername = ds.Tables[0].Rows[0]["server_name"].ToString();
            databasename = ds.Tables[0].Rows[0]["db_name"].ToString();
            username = ds.Tables[0].Rows[0]["user_name"].ToString();
            password = ds.Tables[0].Rows[0]["password"].ToString();
            parastr = ds.Tables[0].Rows[0]["parastr"].ToString();
            sqlstr = ds.Tables[0].Rows[0]["selectstr"].ToString();
            CommProgramName = ds.Tables[0].Rows[0]["ͨѶ������"].ToString();
            resolveType = ds.Tables[0].Rows[0]["��������"].ToString();         
            if (sqlstr.IndexOf("[SAMPLE_NO]") > 0)
            {
                sqlstr = sqlstr.Replace("[SAMPLE_NO]", strTestNO);
            }            
            ds = dsHandle.GetDataSet(@"Extractvalue(���չ���, '/root/buffer_in') As Buffer_In, Extractvalue(���չ���, '/root/buffer_out') As Buffer_out, 
                                                       Extractvalue(���չ���, '/root/data_type') As data_type, Extractvalue(���չ���, '/root/data_begin') As data_begin, 
                                                       Extractvalue(���չ���, '/root/data_end') As data_end, Extractvalue(���չ���, '/root/start_cmd') As start_cmd, 
                                                       Extractvalue(���չ���, '/root/end_cmd') As end_cmd, Extractvalue(���չ���, '/root/Ack_all') As Ack_all, 
                                                       Extractvalue(���չ���, '/root/ack_term') As ack_term, Extractvalue(���չ���, '/root/decode_mode') As decode_mode, 
                                                       Extractvalue(���չ���, '/root/begin_bits') As begin_bits, Extractvalue(���չ���, '/root/end_bits') As end_bits",
                                                    "��������", "id = '" + strInstrumentID + "'");
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
            ds = dsHandle.GetDataSet("����", "��������", "id= '" + strInstrumentID + "'");

            strDevice = ds.Tables[0].Rows[0]["����"].ToString();
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
                    //���䶯̬���ӿ�ִ��
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
        /// ��ȡ����
        /// </summary>
        public void Start()
        {
            try
            {
                conDB.Open();
            }
            catch (Exception e)
            {              
                writeLog.Write("���ݿ�����ʧ�ܣ�" + e.Message, "log");
                strError = "���ݿ�����ʧ�ܣ�" + e.Message;
                return;
            }
            if (conDB.State != ConnectionState.Open) return;

            DataTable dtSample = OracleHelper.GetDataTable(@"select   wm_concat(�������) �������
                                                                                          from �����¼
                                                                                          where id in (select ��¼id from ������ͨ��� where �����־ is not null)
                                                                                                   and ����id = '" + strInstrumentID + @"'
                                                                                                   and to_char(����ʱ��, 'yyyy-mm-dd') = to_char(sysdate, 'yyyy-mm-dd')");
            string _sampleNo = dtSample.Rows[0]["�������"].ToString();
            if (string.IsNullOrEmpty(sqlstr))
                sqlstr = @"SELECT dDate, nSid, sItem, fConc, sConc
                                  FROM Result
                                 where Format(dDate, \""yyyy-mm-dd\"") = Format(date() - 1, \""yyyy-mm-dd\"")
                                   and nSid not in (parameter)
                                 order by nsid";
            sqlstr = sqlstr.Replace("parameter", _sampleNo == "" ? "-1" : _sampleNo);//�滻������
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
        /// �Ͽ����ݿ�����
        /// </summary>
        public void Stop()
        {
            conDB.Close();
        } 
        /// <summary>
        /// ���䶯ִ̬��
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
                    string strSource = sqlstr + "|" + conStr + "|" + databasetype + "|" + parastr; //SQL���+���Ӵ�+���ݿ�����+��������
                    IResolve.ParseResult(strSource, ref strResult, ref strReserved, ref strCmd);                   
                } 
                catch (Exception ex)
                {
                    wl.Write(ex.Message.ToString(), "log");
                }               
        }

        /// <summary>
        /// �ж��Ƿ����ظ�
        /// </summary>
        public void IsUpdate()
        {
            ds = dsHandle.GetDataSet(@"��������,ͨѶ������,
                                                        Extractvalue(ͨѶ����, '/root/db_type') as db_type,
                                                        Extractvalue(ͨѶ����, '/root/db_name') as db_name,
                                                        Extractvalue(ͨѶ����, '/root/user_name') as user_name,
                                                        Extractvalue(ͨѶ����, '/root/password') as password,
                                                        Extractvalue(ͨѶ����, '/root/server_name') as server_name,
                                                        Extractvalue(ͨѶ����, '/root/parastr') as parastr,
                                                        Extractvalue(ͨѶ����, '/root/selectstr') as selectstr", "��������", "id = '" + strInstrumentID + "'");
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
                writeLog.Write("���ݿ�����ʧ�ܣ�" + e.Message, "log");
                strError = "���ݿ�����ʧ�ܣ�" + e.Message;
                return;
            }
            if (conDB.State != ConnectionState.Open) return;

            DataTable dtSample = OracleHelper.GetDataTable(@"select   wm_concat(�������) �������
                                                                                          from �����¼
                                                                                          where id in (select ��¼id from ������ͨ��� where �����־ is not null)
                                                                                                   and ����id = '" + strInstrumentID + @"'
                                                                                                   and to_char(����ʱ��, 'yyyy-mm-dd') = to_char(sysdate, 'yyyy-mm-dd')");
            string _sampleNo = dtSample.Rows[0]["�������"].ToString();
            if (string.IsNullOrEmpty(sqlstr))
                sqlstr = @"SELECT dDate, nSid, sItem, fConc, sConc
                                  FROM Result
                                 where Format(dDate, \""yyyy-mm-dd\"") = Format(date() - 1, \""yyyy-mm-dd\"")
                                   and nSid not in (parameter)
                                 order by nsid";
            sqlstr = sqlstr.Replace("parameter", _sampleNo == "" ? "-1" : _sampleNo);//�滻������
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
        /// Ĭ�Ͻ�����ʽ
        /// </summary>
        /// <param name="a"></param>
        /// <param name="e"></param>
        public void GetResult(object a, EventArgs e)
        {
            try
            {
                string strTestTime;              //����ʱ��
                string strSampleNo;            //�걾��
                string strBarCode = "";        //����
                string strOperator = "";       //����ҽʦ
                string strSampleType = "";   //��������
                string StrSpecimen = "";      //�걾����
                string ChannelType;             //0-��ͨ���;1-ֱ��ͼ;2-ɢ��ͼ;3-ֱ��ͼ���;4-ɢ��ͼ���;5-BASE64
                string testItemID = "";          //ͨ����ĿID
                DataRow[] FindRow;             //��������
                TestGraph = new List<string>();
                saveResult = new SaveResult();
                IsUpdate();//���¼���Ƿ���������
                tItemChannel = OracleHelper.GetDataTable(@"Select ͨ������, m.��Ŀid, Nvl(С��λ��, 2) As С��λ��, Nvl(�����, 0) As �����, Nvl(����ֵ, 0) As ����ֵ, j.�������
                                                                                From ���������Ŀ m, ������Ŀ j
                                                                                Where m.��Ŀid = j.��Ŀid and m.����Id='" + strInstrumentID + "'");
                if (dsResult.Tables[0].Rows.Count == 0)//�ж��Ƿ����±걾
                {
                    writeLog.Write(DateTime.Now.ToString() + "δ��⵽�����ݣ�", "log");
                    return;
                }

                DataTable _dsResult = SelectDistinct(dsResult.Tables[0], "nSid");
                foreach (DataRow dr in _dsResult.Rows) //ѭ���걾��
                {
                    strTestTime = DateTime.Parse(dr["dDate"].ToString()).ToString("yyyy-MM-dd") + " " + DateTime.Now.ToString("HH:mm:ss"); ;
                    strSampleNo = dr["nSid"].ToString();
                    string TestResultValue = "";
                    foreach (DataRow dr1 in dsResult.Tables[0].Select("nSid='" + dr["nSid"].ToString() + "'"))//ѭ���걾������ļ�����Ŀ
                    {
                        string _channelNo = dr1["sItem"].ToString();
                        FindRow = tItemChannel.Select("ͨ������='" + _channelNo.Trim() + "'");
                        if (FindRow.Length == 0) //����ͨ��������ͼ���ܵ�����ͼ��ͨ�������ͨ������Ϊ��
                        {
                            ChannelType = null;
                            writeLog.Write(strDevice, "δ����ͨ����" + _channelNo, "log");

                        }
                        else
                        {
                            testItemID = FindRow[0]["��Ŀid"].ToString();
                            ChannelType = "0"; //��ͨ���
                            TestResultValue = TestResultValue + testItemID + "^" + dr1["Result"].ToString() + "|";
                        }
                    }
                    TestResultValue = strTestTime + "|" + strSampleNo + "^" + strSampleType + "^" + strBarCode + "|" + strOperator + "|" + StrSpecimen + "|" + "|" + TestResultValue;
                    try
                    {
                        saveResult.SaveTextResult(strInstrumentID, TestResultValue, TestGraph, null);
                        saveResult.UpdateData();
                        writeLog.Write(strDevice, "��������� " + TestResultValue, "result");
                    }
                    catch (Exception ex)
                    {
                        writeLog.Write(strDevice, "����ʧ�ܣ� " + ex.ToString(), "log");
                    }
                    //System.Windows.Forms.MessageBox.Show(TestResultValue);
                }
            }
            catch (Exception exp1)
            {
                writeLog.Write(strDevice, "����ʧ�ܣ� " + exp1.ToString(), "log");
            }
        }

        /// <summary>
        /// ɸѡDataTable���ظ�����
        /// </summary>
        /// <param name="SourceTable">Դ����DataTable</param>
        /// <param name="keyFields">ɸѡ���ظ��ֶ�</param>
        /// <returns></returns>t
        public static DataTable SelectDistinct(DataTable SourceTable, string keyFields)
        {
            DataTable dtRet = SourceTable.Clone();//���巵�ؼ�¼��
            StringBuilder sRet = new StringBuilder();//����Ƚ϶���
            //��ò������б�
            string[] sFields = keyFields.Split(',');//��ò������б�
            if (sFields.Length == 0)
                throw new ArgumentNullException("�޲�����");
            int result = 0;//����ѭ������
            string sLastValue = "";//�������ֵ
            SourceTable.Select("", keyFields);//������������
            foreach (DataRow row in SourceTable.Rows)//��ʼ�ȶ�
            {
                sRet.Length = 0;
                for (result = 0; result < sFields.Length; result++)//��������������ϵ��ַ����У���','�ָ��Ϊ�Ƚ϶���
                    sRet.Append(row[sFields[result]]).Append(",");
                result = string.Compare(sRet.ToString(), sLastValue, true);//���бȽϲ��жϱȽϽ��
                switch (result)
                {
                    case 0://��ͬ�����
                        break;
                    case -1://��ͬ����룬������ǰ�Ƚ��ַ�����������ֵ
                    case 1:
                        dtRet.Rows.Add(row.ItemArray);
                        sLastValue = sRet.ToString();
                        break;
                }
            }
            return dtRet;
        }

        /// <summary>
        /// ѡȡ����ͼƬ
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
        /// MSH|��������|������^�����^��������<13>
        /// OBX|ͨ����^���ֵ<13>
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
                    foreach (DataRow drRow in ds.Tables[0].Select("�������='" + DateTime.Now.ToString("yyyy/M/dd") + "'"))
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
                        //c:Program Files\����оƬͼ��������\image
                        //c:Program Files\����оƬͼ��������\temp
                        TestGraph = new List<string>();
                        //fileInfo = new FileInfo(@"c:Program Files\����оƬͼ��������\temp\" + strSampleNO + ".bmp");
                        //if (fileInfo.Exists)
                        //{   //�ļ�����
                        //    TestGraph.Add(fileInfo.FullName);
                        //    //writeLog.Write("log", fileInfo.FullName);
                        //}
                        //else
                        //{
                        fileInfo = new FileInfo(@"c:Program Files\����оƬͼ��������\image\" + Convert.ToInt32(DateTime.Now.ToString("yyMMdd")).ToString() + strSampleNO + ".bmp");
                        if (fileInfo.Exists)
                        {   //�ļ�����
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
                            writeLog.Write("δ����ͨ����" + strChannel, "log");
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
                                //�Ѿ���˵ı걾���ô���
                                //dsGuid = dsHandle.GetDataSet("status", "test_sample", "instrument_id='" + strInstrumentID + "' and sample_no = '" + strSampleNO + "'");
                                //if (Convert.ToInt32(dsGuid.Tables[0].Rows[0]["status"].ToString()) > 7) continue;

                                strChannel = ds.Tables[0].Rows[i][1].ToString();
                                dsGuid = dsHandle.GetDataSet("test_item_id", "test_instrument_item_channel", "channel_no = '" + strChannel + "' and instrument_id = '" + strInstrumentID + "'");
                                if (dsGuid.Tables[0].Rows.Count == 0)
                                {
                                    writeLog.Write("δ����ͨ����" + strChannel, "log");
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
                                //�Ѿ���˻��߷����ı걾������ȡ����
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
                                {   //�ļ�������
                                    return;
                                }
                                //��������ȫ·�������������·��
                                FileName = fileInfo.FullName;
                                long OpStation = GetPrivateProfileString("EQUIPMENT", "IMAGE", "", objStrBd, 256, FileName);
                                if (OpStation > 0)
                                {
                                    FileName = objStrBd.ToString();
                                    long lngNo = 0;
                                    //�ж�H0001.jpg�Ƿ����
                                    fileInfo = new FileInfo(FileName + @"\" + DateTime.Parse(resultstring.Split('|')[0].ToString()).ToString("yyyyMMdd") + @"\" + strSampleNo + @"\H0002.jpg");
                                    if (fileInfo.Exists)
                                    {   //�ļ�����
                                        strImagePosition = strImagePosition + ";" + fileInfo.FullName;
                                        lngNo += 1;
                                    }
                                    //�ж�H0002.jpg�Ƿ����
                                    fileInfo = new FileInfo(FileName + @"\" + DateTime.Parse(resultstring.Split('|')[0].ToString()).ToString("yyyyMMdd") + @"\" + strSampleNo + @"\H0003.jpg");
                                    if (fileInfo.Exists)
                                    {   //�ļ�����
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
                            //�µ������Ż��߽���
                            if (sample_new != sample_old)
                            {
                                if (!string.IsNullOrEmpty(resultstring))
                                {
                                    resString.listInputResult.Add(resultstring);
                                    resString.ParseResult();
                                    resultstring = "";
                                }

                                if (ds.Tables[0].Rows[i][0] != null) resultstring = "MSH|" + (ds.Tables[0].Rows[i][0].ToString().Substring(0, 6).IndexOf('-') > 0 ? ds.Tables[0].Rows[i][0].ToString().Substring(2) : ds.Tables[0].Rows[i][0].ToString()) + "|";         //��������
                                if (ds.Tables[0].Rows[i][1] != null) resultstring = resultstring + Convert.ToInt32(ds.Tables[0].Rows[i][1].ToString()) + "^";   //������
                                if (ds.Tables[0].Rows[i][2] != null) resultstring = resultstring + ds.Tables[0].Rows[i][2].ToString() + "^";   //�����
                                if (ds.Tables[0].Rows[i][3] != null) resultstring = resultstring + ds.Tables[0].Rows[i][3].ToString() + "|";   //��������
                                resultstring = resultstring + (char)13;
                                resultstring = resultstring + "OBX|";
                                if (ds.Tables[0].Rows[i][6] != null) resultstring = resultstring + ds.Tables[0].Rows[i][6].ToString() + "^";   //ͨ����
                                if (ds.Tables[0].Rows[i][7] != null) resultstring = resultstring + ds.Tables[0].Rows[i][7].ToString() + "|";   //���ֵ
                                resultstring = resultstring + (char)13;

                                sample_old = sample_new;
                            }
                            else
                            {
                                resultstring = resultstring + "OBX|";
                                if (ds.Tables[0].Rows[i][6] != null) resultstring = resultstring + ds.Tables[0].Rows[i][6].ToString() + "^";   //ͨ����
                                if (ds.Tables[0].Rows[i][7] != null) resultstring = resultstring + ds.Tables[0].Rows[i][7].ToString() + "|";   //���ֵ
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
