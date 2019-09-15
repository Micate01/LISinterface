using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
//using System.Data.OleDb;
using System.Data.OracleClient;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ZLCHSLisComm
{
    public class SaveResult
    {
        public string strError;
        public string strOrgan; //机构id
        public List<DataTable> dtSave = new List<DataTable>();
        public List<string> selectStr = new List<string>();
        List<string> strGraph = new List<string>();
        List<string> strGraphID = new List<string>();
        string strUpdateSQL;
        string strDeviceName;
        DataSet dsGUID = new DataSet();
        DataSet dsTestApplyMain = new DataSet();
        DataSet dsTestApplyDetail = new DataSet();
        DataSet dsTestSpecimenList = new DataSet();
        DataSet dsTestSample = new DataSet();
        DataSet dsTestResult = new DataSet();
        DataSet dsTestImage = new DataSet();
        DataSet dsFormulaResult = new DataSet();
        DataSet dsTemp = new DataSet();
        DataSetHandle dsHandle = new DataSetHandle();

        //ConnectDB init = new ConnectDB();                           
        string strSampleNO;                        //样本号
        string strBarCode;                         //条码
        string strTestApplyID;                     //申请ID
        string strTestApplyDetailID;               //申请明细ID
        string strTestSpecimenID;                  //样本ID
        string strTestSampleID;                    //标本ID
        string strTestResultID;                    //结果ID
        string strResultFlag;                      //结果标志
        string strLimit;                           //参考区间
        string strResult;                          //结果值
        string strTestItemID;                      //指标ID
        string strIncomeCycleFlag;                 //核收周期
        string strSampleInfo;                      //样本类型
        string strSex;                             //性别
        string strAge;                             //年龄
        string strSampleId;                        //样本ID
        string strSampleType;                      //标本类型
        string strData;                            //检验时间
        string strSample;                          //标本
        string strBeginDate;
        string strEndDate;
        string strState;                           //记录状态
        string strBirth;                           //出生日期

        DataRow[] FindRow;
        DateTime TestTime;
        string[] aRecord;
        string[] aItem;

        bool blnQryWithSampleNO;                   //按样本号查询
        bool blnWaif;                               //无主样本
        bool blnAuditing;                           //是否审核
        Write_Log writeLog = new Write_Log();

        /// <summary>
        /// 保存标准检验结果到数据库
        /// </summary>
        public void SaveTextResult(string DeviceID, string ResultVale, List<string> GraphVale, DataRow drSampleNO)
        {
           
                aItem = ResultVale.Split('|');
                blnAuditing = false;
                /*
                 * GetUpperBound可以获取数组的最高下标。
                 * GetLowerBound可以获取数组的最低下标。
                 * string[,] myStrArr2=new string[,]{{"油","盐"},{"围城","晨露"},{"毛毛熊","Snoopy"}};
    for(int i=myStrArr2.GetLowerBound(0);i<=myStrArr2.GetUpperBound(0);i++)
    {
    for(int i=myStrArr2.GetLowerBound(1);i<=myStrArr2.GetUpperBound(1);i++)
    {
    //处理每一个元素
    }
    }
    0表示二维数组的第一维，1表示数组的第二维
                 */
                if (aItem.GetUpperBound(0) < 5) return;
                strData = aItem[0];
                strSampleNO = aItem[1].Split('^')[0] != null ? Convert.ToInt32(aItem[1].Split('^')[0]).ToString() : null;
                strSampleType = aItem[1].Split('^')[1];
                strBarCode = aItem[1].Split('^')[2];
                strSample = aItem[3];

                strBeginDate = Convert.ToDateTime(strData).ToString("yyyy-MM-dd");
                strEndDate = Convert.ToDateTime(strData).ToString("yyyy-MM-dd") + " 23:59:59";
                strDeviceName = OracleHelper.GetDataTable("select max(名称) from 检验仪器 where id='" + DeviceID + "'").Rows[0][0].ToString();
                if (String.IsNullOrEmpty(strBarCode)) blnQryWithSampleNO = true;
                else
                {
                    dsTemp = dsHandle.GetDataSet(@"j.id,申请来源,j.性别,年龄数字|| 年龄单位 as 年龄,i.出生日期,样本类型,j.记录状态", "检验记录 j,个人信息 i", "j.个人id=i.id(+) and 合并id is null and 仪器id='" + DeviceID + "' and 样本条码='" + strBarCode + "'");
                    if (dsTemp.Tables[0].Rows.Count != 0) blnQryWithSampleNO = false;
                    else
                        blnQryWithSampleNO = true;
                }
                if (blnQryWithSampleNO)
                    dsTemp = dsHandle.GetDataSet(@"j.id,申请来源,j.性别,年龄数字|| 年龄单位 as 年龄,i.出生日期,样本类型,j.记录状态", "检验记录 j,个人信息 i", "j.个人id=i.id(+) and 仪器id='" + DeviceID + "' and 合并id is null and 样本序号='" + strSampleNO + "' and 核收时间 between to_date('" + strBeginDate + "','yyyy-mm-dd') and to_date('" + strEndDate + "','yyyy-MM-dd hh24:mi:ss')");
                if (dsTemp == null)
                { writeLog.Write(strDeviceName, "dsTemp数据集为空,不保存该条数据的记录！错误查看最外层的log文件，程序自动回滚！", "log"); return; }
                blnWaif = false;
                if (dsTemp.Tables[0].Rows.Count == 0)//如果未找到标本，则插入一条无主样本
                {
                    blnWaif = true;
                    OracleParameter[] parms = new OracleParameter[]
                    {                        
                    new OracleParameter("机构id_In", OracleType.VarChar),
                    new OracleParameter("Id_In", OracleType.VarChar),
                    new OracleParameter("申请来源_In", OracleType.Int32),
                    new OracleParameter("单据性质_In", OracleType.Int32),                   
                    new OracleParameter("采集方式id_In",OracleType.VarChar),
                    new OracleParameter("样本类型_In", OracleType.VarChar),
                    new OracleParameter("样本序号_In", OracleType.VarChar),                    
                    new OracleParameter("核收时间_In", OracleType.DateTime),
                    new OracleParameter("仪器id_In", OracleType.VarChar)
                    };
                    strSampleId = dsHandle.GetDataSet("max(机构id) as 机构id,newid", "检验仪器", "id='" + DeviceID + "'").Tables[0].Rows[0][1].ToString();
                    parms[0].Value = dsHandle.GetDataSet("max(机构id) as 机构id,newid", "检验仪器", "id='" + DeviceID + "'").Tables[0].Rows[0][0].ToString();
                    parms[1].Value = strSampleId;
                    parms[2].Value = 0;
                    parms[3].Value = 0;
                    parms[4].Value = null;
                    parms[5].Value = strSample;
                    parms[6].Value = strSampleNO;
                    parms[7].Value = Convert.ToDateTime(strData);
                    parms[8].Value = DeviceID;
                    OracleHelper.RunProcedure("p_检验记录_Insert", parms, ref strError);
                    if (strError != "")
                        writeLog.Write(strDeviceName, "执行<p_检验记录_insert>错误：" + strError, "log");
                    else
                        writeLog.Write(strDeviceName, DateTime.Now.ToString() + "写入无主样本！\r\n检验id=" + strSampleId + ",标本号=" + strSampleNO + ",仪器id=" + DeviceID, "log");
                }
                else
                {
                    strSampleId = dsTemp.Tables[0].Rows[0]["id"].ToString();
                    strSex = dsTemp.Tables[0].Rows[0]["性别"].ToString();
                    strAge = dsTemp.Tables[0].Rows[0]["年龄"].ToString();
                    strState = dsTemp.Tables[0].Rows[0]["记录状态"].ToString();
                    strSample = dsTemp.Tables[0].Rows[0]["样本类型"].ToString();
                    blnAuditing = dsTemp.Tables[0].Rows[0]["记录状态"].ToString() == "4";
                    strBirth = dsTemp.Tables[0].Rows[0]["出生日期"].ToString();

                    writeLog.Write(strDeviceName, DateTime.Now.ToString() + "找到样本！\r\n检验id=" + strSampleId + ",标本号=" + strSampleNO + ",仪器id=" + DeviceID, "log");
                }
                if (!blnAuditing) //未审核标本则更新检查结果
                {
                    strResult = "";
                    Int32 j;
                    for (j = 5; j <= aItem.GetUpperBound(0); ++j)
                        if (aItem[j].Contains("^"))
                            strResult = strResult + "|" + aItem[j].Split('^')[0].ToString() + "," + aItem[j].Split('^')[1].ToString();
                    if (strResult != "")
                    {
                        strResult = strResult.Substring(1);
                        OracleParameter[] parms = new OracleParameter[]
                        {
                            new OracleParameter("记录id_In",OracleType.VarChar),
                            new OracleParameter("仪器id_In",OracleType.VarChar), 
                            new OracleParameter("检验时间_In",OracleType.DateTime), 
                            new OracleParameter("样本类型_In",OracleType.VarChar), 
                            new OracleParameter("性别_In",OracleType.Int32),
                            new OracleParameter("出生日期_In",OracleType.DateTime),
                            new OracleParameter("检验指标_In",OracleType.VarChar),
                            new OracleParameter("备注_In",OracleType.VarChar)
                        };
                        parms[0].Value = strSampleId;
                        parms[1].Value = DeviceID;
                        parms[2].Value = Convert.ToDateTime(aItem[0].ToString());
                        parms[3].Value = strSample;
                        parms[4].Value = strSex == "男" ? 1 : (strSex == "女" ? 2 : 0);
                        parms[5].Value = strBirth == "" ? null : strBirth;
                        parms[6].Value = strResult;
                        //因调用p_检验普通结果_Batchupdate会将“检验备注”清空，固需传入以前检验备注的值
                        parms[7].Value = OracleHelper.GetOracleValue("select 检验备注 from 检验记录 where ID = '" + strSampleId + "'");//检验备注
                       // OracleHelper.RunProcedure("p_检验普通结果_batup_bs330e", parms, ref strError);
                        OracleHelper.RunProcedure("p_检验普通结果_batchupdate", parms, ref strError);
                        if (!String.IsNullOrEmpty(strError))
                        {
                            writeLog.Write(strDeviceName, "执行<p_检验普通结果_Batchupdate>:" + strError, "log");
                            writeLog.Write(strDeviceName, "  parms[0]:" + strSampleId, "log");
                            writeLog.Write(strDeviceName, "  parms[1]:" + DeviceID, "log");
                            writeLog.Write(strDeviceName, "  parms[2]:" + Convert.ToDateTime(aItem[0].ToString()), "log");
                            writeLog.Write(strDeviceName, "  parms[3]:" + strSample, "log");
                            writeLog.Write(strDeviceName, "  parms[4]:" + (strSex == "男" ? 1 : (strSex == "女" ? 2 : 0)), "log");
                            writeLog.Write(strDeviceName, "  parms[5]:" + (strBirth == "" ? null : strBirth), "log");
                            writeLog.Write(strDeviceName, "  parms[6]:" + strResult, "log");

                        }
                        else
                        {
                            writeLog.Write(strDeviceName, DateTime.Now.ToString() + "更新检验结果！\r\n检验id=" + strSampleId + ",标本号=" + strSampleNO + ",仪器id=" + DeviceID + ",结果：" + strResult, "log");
                            SaveResult.Appstr = strSampleId;

                            if (GraphVale != null && GraphVale.Count > 0)
                            {
                                OracleHelper.SaveImage(strSampleId, GraphVale);
                            }
                        }
                    }
                }
        
        }

        public void SaveTextResult_bak(string instrument_id, string ResultValue, List<string> GraphValue, DataRow drSampleNO)
        {

            dsGUID = dsHandle.GetDataSet("nvl(INCOME_CYCLE_FLAG,1) INCOME_CYCLE_FLAG", "TEST_INSTRUMENT", "INSTRUMENT_ID = '" + instrument_id + "'");
            if (dsGUID.Tables[0].Rows.Count == 0)
            {
                writeLog.Write("未设置核收周期", "log");
                return;
            }
            strIncomeCycleFlag = dsGUID.Tables[0].Rows[0]["INCOME_CYCLE_FLAG"].ToString();
            TestTime = DateTime.Parse(ResultValue.Split('|')[0].ToString());
            strSampleNO = ResultValue.Split('|')[1].Split('^')[0];
            strBarCode = ResultValue.Split('|')[1].Split('^')[1];
            strSampleInfo = ResultValue.Split('|')[1].Split('^')[2];
            if (string.IsNullOrEmpty(strSampleInfo)) strSampleInfo = "0";
            //if (string.IsNullOrEmpty(strSampleInfo) || strSampleInfo == "S" || strSampleInfo == "s")
            //{
            //    strSampleInfo = "0";
            //}
            //else if (strSampleInfo == "Q" || strSampleInfo == "q")
            //{
            //    strSampleInfo = "1";
            //}
            //else
            //{
            //    //质控先不处理
            //    strSampleInfo = "0";
            //}

            if (String.IsNullOrEmpty(strSampleNO) && string.IsNullOrEmpty(strBarCode))
            {
                if (drSampleNO == null)
                {
                    dsGUID = dsHandle.GetDataSet("nvl(max(sample_no),0) +1", "test_sample ,test_apply_main ", "instrument_id ='" + instrument_id + "' and INCOME_TIME >= trunc(to_date('" + TestTime + "','yyyy-MM-dd HH24:mi:ss')) and INCOME_TIME < trunc(to_date('" + TestTime + "','yyyy-MM-dd HH24:mi:ss')) + 1 and test_sample.test_apply_id = test_apply_main.test_apply_id and nvl(test_apply_main.is_emergency,0) = " + Convert.ToInt32(strSampleInfo));
                    strSampleNO = dsGUID.Tables[0].Rows[0][0].ToString();
                }
                writeLog.Write("【标本号为空】", "log");
                return;
            }
            if (!string.IsNullOrEmpty(strBarCode))
            {
                dsGUID = dsHandle.GetDataSet("Count(*)", "TEST_SAMPLE", "BARCODE='" + strBarCode + "' AND INSTRUMENT_ID = '" + instrument_id + "' and INCOME_TIME >= trunc(to_date('" + TestTime + "','yyyy-MM-dd HH24:mi:ss')) and INCOME_TIME < trunc(to_date('" + TestTime + "','yyyy-MM-dd HH24:mi:ss')) + 1");
            }
            else
            {
                try
                {
                    strSampleNO = Convert.ToDouble(strSampleNO).ToString();
                }
                catch (Exception e)
                {
                    writeLog.Write("【仪器样本号错误】" + e.Message, "log");
                    return;
                }
                if (strIncomeCycleFlag == "1")  //日核收
                {
                    dsGUID = dsHandle.GetDataSet("Count(*)", "TEST_SAMPLE,test_apply_main", "SAMPLE_NO='" + strSampleNO + "' AND INSTRUMENT_ID = '" + instrument_id + "' and INCOME_TIME >= trunc(to_date('" + TestTime + "','yyyy-MM-dd HH24:mi:ss')) and INCOME_TIME < trunc(to_date('" + TestTime + "','yyyy-MM-dd HH24:mi:ss')) + 1 and test_sample.test_apply_id = test_apply_main.test_apply_id and nvl(test_apply_main.is_emergency,0) = " + strSampleInfo);
                }
                else if (strIncomeCycleFlag == "2") //周核收
                {
                    dsGUID = dsHandle.GetDataSet("Count(*)", "TEST_SAMPLE,test_apply_main", "SAMPLE_NO='" + strSampleNO + "' AND INSTRUMENT_ID = '" + instrument_id + "' and INCOME_TIME >= trunc(to_date('" + TestTime + "','yyyy-MM-dd HH24:mi:ss') -to_number(to_char(to_date('" + TestTime + "','yyyy-MM-dd HH24:mi:ss'),'d')) +2) and INCOME_TIME < trunc(to_date('" + TestTime + "','yyyy-MM-dd HH24:mi:ss')) + 1 and test_sample.test_apply_id = test_apply_main.test_apply_id and nvl(test_apply_main.is_emergency,0) = " + strSampleInfo);
                }
                else if (strIncomeCycleFlag == "3") //月核收
                {
                    dsGUID = dsHandle.GetDataSet("Count(*)", "TEST_SAMPLE,test_apply_main", "SAMPLE_NO='" + strSampleNO + "' AND INSTRUMENT_ID = '" + instrument_id + "' and INCOME_TIME >= trunc(to_date('" + TestTime + "','yyyy-MM-dd HH24:mi:ss'),'month') and INCOME_TIME < trunc(to_date('" + TestTime + "','yyyy-MM-dd HH24:mi:ss')) + 1 and test_sample.test_apply_id = test_apply_main.test_apply_id and nvl(test_apply_main.is_emergency,0) = " + strSampleInfo);
                }
                else if (strIncomeCycleFlag == "4") //年核收
                {
                    dsGUID = dsHandle.GetDataSet("Count(*)", "TEST_SAMPLE,test_apply_main", "SAMPLE_NO='" + strSampleNO + "' AND INSTRUMENT_ID = '" + instrument_id + "' and INCOME_TIME >= trunc(to_date('" + TestTime + "','yyyy-MM-dd HH24:mi:ss'),'year') and INCOME_TIME < trunc(to_date('" + TestTime + "','yyyy-MM-dd HH24:mi:ss')) + 1 and test_sample.test_apply_id = test_apply_main.test_apply_id and nvl(test_apply_main.is_emergency,0) = " + strSampleInfo);
                }
                else if (strIncomeCycleFlag == "5") //自动累加
                {
                    dsGUID = dsHandle.GetDataSet("Count(*)", "TEST_SAMPLE,test_apply_main", "SAMPLE_NO='" + strSampleNO + "' AND INSTRUMENT_ID = '" + instrument_id + "' and test_sample.test_apply_id = test_apply_main.test_apply_id and nvl(test_apply_main.is_emergency,0) = " + strSampleInfo + " and test_apply_main.patient_resource > 0");
                }
                else if (strIncomeCycleFlag == "6") //未审核累加
                {
                    dsGUID = dsHandle.GetDataSet("Count(*)", "TEST_SAMPLE,test_apply_main", "SAMPLE_NO='" + strSampleNO + "' AND INSTRUMENT_ID = '" + instrument_id + "' and sample_status < 9 and test_sample.test_apply_id = test_apply_main.test_apply_id and nvl(test_apply_main.is_emergency,0) = " + strSampleInfo + " and test_apply_main.patient_resource > 0");
                }
            }

            //已经存在样本
            if (Convert.ToInt32(dsGUID.Tables[0].Rows[0][0].ToString()) > 1)
            {
                writeLog.Write("【标本号重复】:" + strSampleNO, "log");
                return;
            }
            else if (dsGUID.Tables[0].Rows[0][0].ToString() == "1")
            {
                if (!string.IsNullOrEmpty(strBarCode))
                {
                    dsGUID = dsHandle.GetDataSet("TEST_SAMPLE_ID,TEST_APPLY_ID,sample_status", "TEST_SAMPLE", "BARCODE='" + strBarCode + "' AND INSTRUMENT_ID = '" + instrument_id + "' and INCOME_TIME >= trunc(to_date('" + TestTime + "','yyyy-MM-dd HH24:mi:ss')) and INCOME_TIME < trunc(to_date('" + TestTime + "','yyyy-MM-dd HH24:mi:ss')) + 1");
                }
                else
                {
                    //dsGUID = dsHandle.GetDataSet("TEST_SAMPLE_ID,TEST_APPLY_ID,sample_status", "TEST_SAMPLE", "SAMPLE_NO='" + strSampleNO + "' AND INSTRUMENT_ID = '" + instrument_id + "' and INCOME_TIME >= trunc(to_date('" + TestTime + "','yyyy-MM-dd HH24:mi:ss')) and INCOME_TIME < trunc(to_date('" + TestTime + "','yyyy-MM-dd HH24:mi:ss')) + 1");
                    if (strIncomeCycleFlag == "1")  //日核收
                    {
                        dsGUID = dsHandle.GetDataSet("TEST_SAMPLE_ID,TEST_SAMPLE.TEST_APPLY_ID,sample_status", "TEST_SAMPLE,test_apply_main", "SAMPLE_NO='" + strSampleNO + "' AND INSTRUMENT_ID = '" + instrument_id + "' and INCOME_TIME >= trunc(to_date('" + TestTime + "','yyyy-MM-dd HH24:mi:ss')) and INCOME_TIME < trunc(to_date('" + TestTime + "','yyyy-MM-dd HH24:mi:ss')) + 1 and test_sample.test_apply_id = test_apply_main.test_apply_id and nvl(test_apply_main.is_emergency,0) = " + strSampleInfo);
                    }
                    else if (strIncomeCycleFlag == "2") //周核收
                    {
                        dsGUID = dsHandle.GetDataSet("TEST_SAMPLE_ID,TEST_SAMPLE.TEST_APPLY_ID,sample_status", "TEST_SAMPLE,test_apply_main", "SAMPLE_NO='" + strSampleNO + "' AND INSTRUMENT_ID = '" + instrument_id + "' and INCOME_TIME >= trunc(to_date('" + TestTime + "','yyyy-MM-dd HH24:mi:ss') -to_number(to_char(to_date('" + TestTime + "','yyyy-MM-dd HH24:mi:ss'),'d')) +2) and INCOME_TIME < trunc(to_date('" + TestTime + "','yyyy-MM-dd HH24:mi:ss')) + 1 and test_sample.test_apply_id = test_apply_main.test_apply_id and nvl(test_apply_main.is_emergency,0) = " + strSampleInfo);
                    }
                    else if (strIncomeCycleFlag == "3") //月核收
                    {
                        dsGUID = dsHandle.GetDataSet("TEST_SAMPLE_ID,TEST_SAMPLE.TEST_APPLY_ID,sample_status", "TEST_SAMPLE,test_apply_main", "SAMPLE_NO='" + strSampleNO + "' AND INSTRUMENT_ID = '" + instrument_id + "' and INCOME_TIME >= trunc(to_date('" + TestTime + "','yyyy-MM-dd HH24:mi:ss'),'month') and INCOME_TIME < trunc(to_date('" + TestTime + "','yyyy-MM-dd HH24:mi:ss')) + 1 and test_sample.test_apply_id = test_apply_main.test_apply_id and nvl(test_apply_main.is_emergency,0) = " + strSampleInfo);
                    }
                    else if (strIncomeCycleFlag == "4") //年核收
                    {
                        dsGUID = dsHandle.GetDataSet("TEST_SAMPLE_ID,TEST_SAMPLE.TEST_APPLY_ID,sample_status", "TEST_SAMPLE,test_apply_main", "SAMPLE_NO='" + strSampleNO + "' AND INSTRUMENT_ID = '" + instrument_id + "' and INCOME_TIME >= trunc(to_date('" + TestTime + "','yyyy-MM-dd HH24:mi:ss'),'year') and INCOME_TIME < trunc(to_date('" + TestTime + "','yyyy-MM-dd HH24:mi:ss')) + 1 and test_sample.test_apply_id = test_apply_main.test_apply_id and nvl(test_apply_main.is_emergency,0) = " + strSampleInfo);
                    }
                    else if (strIncomeCycleFlag == "5") //自动累加
                    {
                        dsGUID = dsHandle.GetDataSet("TEST_SAMPLE_ID,TEST_SAMPLE.TEST_APPLY_ID,sample_status", "TEST_SAMPLE,test_apply_main", "SAMPLE_NO='" + strSampleNO + "' AND INSTRUMENT_ID = '" + instrument_id + "' and test_sample.test_apply_id = test_apply_main.test_apply_id and nvl(test_apply_main.is_emergency,0) = " + strSampleInfo + " and test_apply_main.patient_resource > 0");
                    }
                    else if (strIncomeCycleFlag == "6") //未审核累加
                    {
                        dsGUID = dsHandle.GetDataSet("TEST_SAMPLE_ID,TEST_SAMPLE.TEST_APPLY_ID,sample_status", "TEST_SAMPLE,test_apply_main", "SAMPLE_NO='" + strSampleNO + "' AND INSTRUMENT_ID = '" + instrument_id + "' and sample_status < 9 and test_sample.test_apply_id = test_apply_main.test_apply_id and nvl(test_apply_main.is_emergency,0) = " + strSampleInfo + " and test_apply_main.patient_resource > 0");
                    }
                }

                //标本已审，返回
                if (Convert.ToInt32(dsGUID.Tables[0].Rows[0]["sample_status"].ToString()) > 7)
                {
                    writeLog.Write("标本号：" + strSampleNO + "、条码号：" + strBarCode + "已审!", "log");
                    return;
                }

                strTestSampleID = dsGUID.Tables[0].Rows[0]["TEST_SAMPLE_ID"].ToString();
                strTestApplyID = dsGUID.Tables[0].Rows[0]["TEST_APPLY_ID"].ToString();
                dsGUID = dsHandle.GetDataSet("MIN(C.AGE),MIN(C.GENDER),MIN(B.SAMPLE_TYPE)", "TEST_APPLY_DETAIL B,TEST_APPLY_MAIN C", "b.test_apply_id = c.test_apply_id and b.sample_id = '" + strTestSampleID + "'");
                if (dsGUID.Tables[0].Rows.Count.ToString() == "0")
                {
                    writeLog.Write("申请信息不全,sample_id:" + strTestSampleID, "log");
                    return;
                }

                strAge = dsGUID.Tables[0].Rows[0][0].ToString();
                strSex = dsGUID.Tables[0].Rows[0][1].ToString();
                strSampleType = dsGUID.Tables[0].Rows[0][2].ToString();

                dsTestApplyDetail = dsHandle.GetDataSet("test_apply_detail_id,status", "test_apply_detail", "sample_id = '" + strTestSampleID + "'");
                foreach (DataRow dr in dsTestApplyDetail.Tables[0].Rows)
                {
                    dr["status"] = 7;
                }

                dsGUID = dsHandle.GetDataSet("to_char(income_time ,'yyyy-MM-dd HH24:mi:ss') INCOME_TIME", "TEST_SAMPLE", "TEST_SAMPLE_ID = '" + strTestSampleID + "'");
                if (DateTime.Parse(dsGUID.Tables[0].Rows[0][0].ToString()) > TestTime)
                {
                    TestTime = DateTime.Parse(dsGUID.Tables[0].Rows[0][0].ToString());
                }

                dsTestSample = dsHandle.GetDataSet("TEST_SAMPLE_ID,SAMPLE_NO,TEST_OPERATOR,TEST_TIME,SAMPLE_STATUS,GROUP_ID,INSTRUMENT_ID,TEST_APPLY_ID,SAMPLE_STATUS", "TEST_SAMPLE", "TEST_SAMPLE_ID = '" + strTestSampleID + "'");
                dsTestResult = dsHandle.GetDataSet("TEST_RESULT_ID,TEST_SAMPLE_ID,TEST_APPLY_ID,TEST_ITEM_ID,INSTRUMENT_ID,MICROPLATE_ID,RESULT_VALUE,RESULT_FLAG,ORIGINAL_RESULT,RESULT_COUNT,IS_VALID,OD_VALUE,CUTOFF_VALUE,ADDI_VALUE,REFER_LIMIT", "TEST_RESULT", "test_sample_id = '" + strTestSampleID + "'");
                dsFormulaResult = dsHandle.GetDataSet("a.test_item_id,a.FORMULA", "test_item a,test_instrument_item_channel b,test_result c", "a.test_item_id = b.test_item_id and a.IS_CALC = '1' and c.test_item_id = b.test_item_id and b.instrument_id = '" + instrument_id + "' and test_sample_id = '" + strTestSampleID + "'");
                dsTestSample.Tables[0].PrimaryKey = new DataColumn[] { dsTestSample.Tables[0].Columns["TEST_SAMPLE_ID"] };
                dsTestSample.Tables[0].DataSet.Tables[0].Rows[0]["TEST_TIME"] = TestTime;
                dsTestSample.Tables[0].DataSet.Tables[0].Rows[0]["SAMPLE_STATUS"] = 7;
                dsTestResult.Tables[0].PrimaryKey = new DataColumn[] { dsTestResult.Tables[0].Columns["TEST_RESULT_ID"] };

                //普通结果
                for (int i = 5; i < ResultValue.Split('|').Length; i++)
                {
                    if (String.IsNullOrEmpty(ResultValue.Split('|')[i].ToString())) break;
                    //已经存在结果
                    strTestItemID = ResultValue.Split('|')[i].Split('^')[0];
                    strResult = ResultValue.Split('|')[i].Split('^')[1].ToString();

                    FindRow = dsTestResult.Tables[0].Select("TEST_ITEM_ID = '" + strTestItemID + "'");
                    if (FindRow.Length != 0)
                    {
                        foreach (DataRow dr in FindRow)
                        {
                            if (string.IsNullOrEmpty(strResult))
                            {
                                strResultFlag = null;
                                dr["RESULT_VALUE"] = strResult;
                                dr["RESULT_FLAG"] = strResultFlag;
                            }
                            else
                            {
                                strLimit = dr["REFER_LIMIT"].ToString();
                                dsGUID = dsHandle.GetDataSet("F_GET_TEST_RESULT_FLAG('" + strTestItemID + "','" + strLimit + "','" + strResult + "')", "dual", "");
                                strResultFlag = dsGUID.Tables[0].Rows[0][0].ToString();
                                if (strResultFlag == "-1") strResultFlag = null;
                                //普通结果为空时写原始结果
                                if (string.IsNullOrEmpty(dr["RESULT_VALUE"].ToString())) dr["ORIGINAL_RESULT"] = strResult;
                                dr["RESULT_VALUE"] = strResult;
                                dr["RESULT_FLAG"] = strResultFlag;
                            }
                        };
                    }
                    else
                    {
                        dsGUID = dsHandle.GetDataSet("F_GET_TEST_ITEM_LIMIT_NEW('" + instrument_id + "','" + strTestItemID + "','" + strAge + "','" + strSex + "','" + strSampleType + "') LIMIT_VALUE", "dual", "");
                        strLimit = dsGUID.Tables[0].Rows[0]["LIMIT_VALUE"].ToString();
                        dsGUID = dsHandle.GetDataSet("F_GET_TEST_RESULT_FLAG('" + strTestItemID + "','" + strLimit + "','" + strResult + "')", "dual", "");
                        strResultFlag = dsGUID.Tables[0].Rows[0][0].ToString();
                        if (strResultFlag == "-1") strResultFlag = null;
                        dsGUID = dsHandle.GetDataSet("ZLBASE.GUID", "dual", "");
                        strTestResultID = dsGUID.Tables[0].Rows[0][0].ToString();

                        dsTestResult.Tables[0].Rows.Add(strTestResultID, strTestSampleID, strTestApplyID, strTestItemID, instrument_id, null, strResult, strResultFlag, strResult, 1, 1, null, null, null, strLimit);
                    }

                    //更新计算公式
                    foreach (DataRow drFormula in dsFormulaResult.Tables[0].Rows)
                    {
                        if (string.IsNullOrEmpty(strResult)) continue;
                        //writeLog.Write("log", "0" + strResult);
                        drFormula["Formula"] = drFormula["Formula"].ToString().Replace("[" + strTestItemID + "]", strResult);
                    };
                }
                //添加计算公式的结果
                foreach (DataRow drFormula in dsFormulaResult.Tables[0].Rows)
                {
                    //writeLog.Write(drFormula["FORMULA"].ToString(), "log");
                    //未替换的指标从数据库中重新读取一次
                    if (drFormula["FORMULA"].ToString().IndexOf("[") >= 0)
                    {
                        //continue;
                        while (drFormula["FORMULA"].ToString().IndexOf("[") >= 0)
                        {
                            strTestItemID = drFormula["FORMULA"].ToString().Substring(drFormula["FORMULA"].ToString().IndexOf("[") + 1, drFormula["FORMULA"].ToString().IndexOf("]") - drFormula["FORMULA"].ToString().IndexOf("[") - 1);

                            dsGUID = dsHandle.GetDataSet("result_value", "test_result", "test_sample_id = '" + strTestSampleID + "' and test_item_id = '" + strTestItemID + "'");
                            if (dsGUID.Tables[0].Rows.Count.ToString() == "0")
                            {
                                writeLog.Write("计算项目的指标在结果表中不存在!" + drFormula["FORMULA"].ToString(), "log");
                            }
                            strResult = dsGUID.Tables[0].Rows[0][0].ToString();
                            //writeLog.Write(strResult, "log");
                            if (string.IsNullOrEmpty(strResult)) break;
                            //writeLog.Write("log", "1" + strResult);
                            drFormula["Formula"] = drFormula["Formula"].ToString().Replace("[" + strTestItemID + "]", strResult);
                        }
                    }
                    //仍然存在指标没有结果的时候不处理
                    if (drFormula["FORMULA"].ToString().IndexOf("[") >= 0)
                    {
                        writeLog.Write("计算项目的指标设置错误!" + drFormula["FORMULA"].ToString(), "log");
                        continue;
                    }
                    foreach (DataRow dr in dsTestResult.Tables[0].Select("TEST_ITEM_ID = '" + drFormula["TEST_ITEM_ID"].ToString() + "'"))
                    {

                        strTestItemID = drFormula["TEST_ITEM_ID"].ToString();
                        dsGUID = dsHandle.GetDataSet(drFormula["FORMULA"].ToString(), "dual", "");
                        if (dsGUID.Tables[0].Rows.Count < 1)
                        {
                            writeLog.Write("计算指标结果错误：" + "select " + drFormula["FORMULA"].ToString() + " from dual", "log");
                            continue;
                        }
                        strResult = dsGUID.Tables[0].Rows[0][0].ToString();
                        //writeLog.Write("计算公式" + drFormula["FORMULA"].ToString(), "log");
                        //writeLog.Write("计算结果" + strResult, "log");
                        //strResult = "" + Math.Round(Convert.ToDouble(dsGUID.Tables[0].Rows[0][0].ToString()), 4);
                        dsGUID = dsHandle.GetDataSet("RESULT_TYPE,nvl(decimal_level,0) decimal_level,conversion_ratio", "test_item,test_instrument_item_channel", "test_instrument_item_channel.instrument_id = '" + instrument_id + "' and test_instrument_item_channel.test_item_id = '" + strTestItemID + "' and test_instrument_item_channel.test_item_id = test_item.test_item_id");
                        //定量
                        if (dsGUID.Tables[0].Rows[0]["RESULT_TYPE"].ToString() == "1")
                        {
                            try
                            {
                                if (dsGUID.Tables[0].Rows[0]["decimal_level"].ToString() == "0")
                                {
                                    strResult = (Convert.ToDouble(strResult)).ToString("f0");
                                }
                                else if (dsGUID.Tables[0].Rows[0]["decimal_level"].ToString() == "1")
                                {
                                    strResult = (Convert.ToDouble(strResult)).ToString("f1");
                                }
                                else if (dsGUID.Tables[0].Rows[0]["decimal_level"].ToString() == "2")
                                {
                                    strResult = (Convert.ToDouble(strResult)).ToString("f2");
                                }
                                else if (dsGUID.Tables[0].Rows[0]["decimal_level"].ToString() == "3")
                                {
                                    strResult = (Convert.ToDouble(strResult)).ToString("f3");
                                }
                                else if (dsGUID.Tables[0].Rows[0]["decimal_level"].ToString() == "4")
                                {
                                    strResult = (Convert.ToDouble(strResult)).ToString("f4");
                                }
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        if (string.IsNullOrEmpty(dr["RESULT_VALUE"].ToString())) dr["ORIGINAL_RESULT"] = strResult;
                        dr["RESULT_VALUE"] = strResult;
                        strLimit = dr["REFER_LIMIT"].ToString();
                        dsGUID = dsHandle.GetDataSet("F_GET_TEST_RESULT_FLAG('" + strTestItemID + "','" + strLimit + "','" + strResult + "')", "dual", "");
                        strResultFlag = dsGUID.Tables[0].Rows[0][0].ToString();

                        if (strResultFlag == "-1") strResultFlag = null;
                        dr["RESULT_FLAG"] = strResultFlag;
                    }
                };

                dtSave.Add(dsTestSample.Tables[0]);
                dtSave.Add(dsTestResult.Tables[0]);
                dtSave.Add(dsTestApplyDetail.Tables[0]);
                selectStr.Add("select TEST_SAMPLE_ID,SAMPLE_NO,TEST_OPERATOR,TEST_TIME,SAMPLE_STATUS,GROUP_ID,INSTRUMENT_ID,TEST_APPLY_ID from TEST_SAMPLE where test_sample_id = '" + strTestSampleID + "'");
                selectStr.Add("select TEST_RESULT_ID,TEST_SAMPLE_ID,TEST_APPLY_ID,TEST_ITEM_ID,INSTRUMENT_ID,MICROPLATE_ID,RESULT_VALUE,RESULT_FLAG,ORIGINAL_RESULT,RESULT_COUNT,IS_VALID,OD_VALUE,CUTOFF_VALUE,ADDI_VALUE,REFER_LIMIT from TEST_RESULT where Test_Sample_Id = '" + strTestSampleID + "'");
                selectStr.Add("select test_apply_detail_id,status from test_apply_detail where sample_id = '" + strTestSampleID + "'");

                //图像
                if (GraphValue.Count > 0)
                {
                    dsTestImage = dsHandle.GetDataSet("GRAPH_RESULT_ID,TEST_SAMPLE_ID,GRAPH_TYPE, GRAPH_RESULT", "TEST_GRAPH_RESULT", "TEST_SAMPLE_ID = '" + strTestSampleID + "'");

                    if (dsTestImage.Tables[0].Rows.Count > 0)
                    {
                        for (int i = dsTestImage.Tables[0].Rows.Count; i > 0; i--)
                        {
                            dsTestImage.Tables[0].Rows[i - 1].Delete();
                        }
                    }
                    //if (dsTestImage.Tables[0].Rows.Count == 2)
                    //{
                    //    dsTestImage.Tables[0].Rows[1].Delete();
                    //    dsTestImage.Tables[0].Rows[0].Delete();
                    //}
                    //if (dsTestImage.Tables[0].Rows.Count == 1)
                    //{
                    //    dsTestImage.Tables[0].Rows[0].Delete();
                    //}

                    for (int IImage = 0; IImage < GraphValue.Count; IImage++)
                    {
                        dsGUID = dsHandle.GetDataSet("ZLBASE.GUID", "dual", "");
                        dsTestImage.Tables[0].Rows.Add(dsGUID.Tables[0].Rows[0][0].ToString(), strTestSampleID, (IImage + 1).ToString());
                        strGraphID.Add(dsGUID.Tables[0].Rows[0][0].ToString());
                        strGraph.Add(GraphValue[IImage].ToString());
                    }
                    dtSave.Add(dsTestImage.Tables[0]);
                    selectStr.Add("select GRAPH_RESULT_ID,TEST_SAMPLE_ID,GRAPH_TYPE from TEST_GRAPH_RESULT");
                }
            }
            else
            {
                dsGUID = dsHandle.GetDataSet("ZLBASE.GUID", "dual", "");
                strTestApplyID = dsGUID.Tables[0].Rows[0][0].ToString();
                dsGUID = dsHandle.GetDataSet("ZLBASE.GUID", "dual", "");
                strTestApplyDetailID = dsGUID.Tables[0].Rows[0][0].ToString();
                dsGUID = dsHandle.GetDataSet("ZLBASE.GUID", "dual", "");
                strTestSpecimenID = dsGUID.Tables[0].Rows[0][0].ToString();
                dsGUID = dsHandle.GetDataSet("ZLBASE.GUID", "dual", "");
                strTestSampleID = dsGUID.Tables[0].Rows[0][0].ToString();
                dsTestApplyMain = dsHandle.GetDataSet("TEST_APPLY_ID,IS_EMERGENCY,STATUS,PATIENT_RESOURCE,ORDER_DATE", "TEST_APPLY_MAIN", "TEST_APPLY_ID = '" + strTestApplyID + "'");
                dsTestSpecimenList = dsHandle.GetDataSet("SPECIMEN_ID,BARCODE,STATUS,PICK_FEE_STATUS,PICK_FEE", "TEST_SPECIMEN_LIST", "SPECIMEN_ID = '" + strTestSpecimenID + "'");
                dsTestApplyDetail = dsHandle.GetDataSet("TEST_APPLY_DETAIL_ID,TEST_APPLY_ID,SPECIMEN_ID,SAMPLE_TYPE,SNO,STATUS,SAMPLE_ID,PICK_DEPT_ID,EXEC_DEPT_ID", "TEST_APPLY_DETAIL", "TEST_APPLY_ID = '" + strTestApplyID + "'");
                dsTestSample = dsHandle.GetDataSet("TEST_SAMPLE_ID,SAMPLE_NO,INCOME_TIME,TEST_OPERATOR,TEST_TIME,SAMPLE_STATUS,GROUP_ID,INSTRUMENT_ID,TEST_APPLY_ID", "TEST_SAMPLE", "TEST_SAMPLE_ID = '" + strTestSampleID + "'");
                dsTestResult = dsHandle.GetDataSet("TEST_RESULT_ID,TEST_SAMPLE_ID,TEST_APPLY_ID,TEST_ITEM_ID,INSTRUMENT_ID,MICROPLATE_ID,RESULT_VALUE,RESULT_FLAG,ORIGINAL_RESULT,RESULT_COUNT,IS_VALID,OD_VALUE,CUTOFF_VALUE,ADDI_VALUE,REFER_LIMIT", "TEST_RESULT", "TEST_SAMPLE_ID='" + strTestSampleID + "'");
                if (strSampleInfo == "0")
                {
                    dsTestApplyMain.Tables[0].Rows.Add(strTestApplyID, 0, 1, -1, TestTime);
                }
                else if (strSampleInfo == "1")
                {
                    dsTestApplyMain.Tables[0].Rows.Add(strTestApplyID, 1, 1, -1, TestTime);
                }
                else if (strSampleInfo == "-2")
                {
                    dsTestApplyMain.Tables[0].Rows.Add(strTestApplyID, 0, 1, -2, TestTime);
                }
                dsTestSpecimenList.Tables[0].Rows.Add(strTestSpecimenID, ResultValue.Split('|')[1].Split('^')[1], 5, 0, 0);
                dsTestApplyDetail.Tables[0].Rows.Add(strTestApplyDetailID, strTestApplyID, strTestSpecimenID, ResultValue.Split('|')[3], 1, 7, strTestSampleID, null, null);
                dsTestSample.Tables[0].Rows.Add(strTestSampleID, ResultValue.Split('|')[1].Split('^')[0], TestTime, "", TestTime, 7, "", instrument_id, strTestApplyID);

                //普通结果
                for (int i = 5; i < ResultValue.Split('|').Length; i++)
                {
                    if (ResultValue.Split('|')[i].Length == 0) break;
                    strTestItemID = ResultValue.Split('|')[i].Split('^')[0].ToString();
                    strResult = ResultValue.Split('|')[i].Split('^')[1].ToString();

                    ////无标本记录时不处理参考值
                    //strResultFlag = "";
                    //strLimit = "";
                    dsGUID = dsHandle.GetDataSet("sample_type", "test_item", "test_item_id = '" + strTestItemID + "'");
                    strSampleType = dsGUID.Tables[0].Rows[0][0].ToString();
                    dsGUID = dsHandle.GetDataSet("F_GET_TEST_ITEM_LIMIT_NEW('" + instrument_id + "','" + strTestItemID + "','1岁','男','" + strSampleType + "') LIMIT_VALUE", "dual", "");
                    strLimit = dsGUID.Tables[0].Rows[0]["LIMIT_VALUE"].ToString();
                    dsGUID = dsHandle.GetDataSet("F_GET_TEST_RESULT_FLAG('" + strTestItemID + "','" + strLimit + "','" + strResult + "')", "dual", "");
                    if (dsGUID.Tables[0].Rows.Count < 0)
                    {
                        writeLog.Write("查询未返回结果" + "select F_GET_TEST_RESULT_FLAG('" + strTestItemID + "','" + strLimit + "',') from dual", "log");
                        continue;
                    }
                    strResultFlag = dsGUID.Tables[0].Rows[0][0].ToString();
                    if (strResultFlag == "") strResultFlag = null;
                    dsGUID = dsHandle.GetDataSet("ZLBASE.GUID", "dual", "");
                    strTestResultID = dsGUID.Tables[0].Rows[0][0].ToString();

                    dsTestResult.Tables[0].Rows.Add(strTestResultID, strTestSampleID, strTestApplyID, strTestItemID, instrument_id, "", strResult, strResultFlag, strResult, 1, 1, null, null, null, strLimit);
                }

                selectStr.Add("select TEST_APPLY_ID,IS_EMERGENCY,STATUS,PATIENT_RESOURCE,ORDER_DATE from TEST_APPLY_MAIN");
                selectStr.Add("select SPECIMEN_ID,BARCODE,STATUS,PICK_FEE_STATUS,PICK_FEE from TEST_SPECIMEN_LIST");
                selectStr.Add("select TEST_APPLY_DETAIL_ID,TEST_APPLY_ID,SPECIMEN_ID,SAMPLE_TYPE,SNO,STATUS,SAMPLE_ID,PICK_DEPT_ID,EXEC_DEPT_ID from TEST_APPLY_DETAIL");
                selectStr.Add("select TEST_SAMPLE_ID,SAMPLE_NO,INCOME_TIME,TEST_OPERATOR,TEST_TIME,SAMPLE_STATUS,GROUP_ID,INSTRUMENT_ID,TEST_APPLY_ID from TEST_SAMPLE");
                selectStr.Add("select TEST_RESULT_ID,TEST_SAMPLE_ID,TEST_APPLY_ID,TEST_ITEM_ID,INSTRUMENT_ID,MICROPLATE_ID,RESULT_VALUE,RESULT_FLAG,ORIGINAL_RESULT,RESULT_COUNT,IS_VALID,OD_VALUE,CUTOFF_VALUE,ADDI_VALUE,REFER_LIMIT from TEST_RESULT");

                dtSave.Add(dsTestApplyMain.Tables[0]);
                dtSave.Add(dsTestSpecimenList.Tables[0]);
                dtSave.Add(dsTestApplyDetail.Tables[0]);
                dtSave.Add(dsTestSample.Tables[0]);
                dtSave.Add(dsTestResult.Tables[0]);

                //图像
                if (GraphValue.Count > 0)
                {
                    dsTestImage = dsHandle.GetDataSet("GRAPH_RESULT_ID,TEST_SAMPLE_ID,GRAPH_TYPE,GRAPH_RESULT", "TEST_GRAPH_RESULT", "TEST_SAMPLE_ID = '" + strTestSampleID + "'");
                    if (dsTestImage.Tables[0].Rows.Count > 0)
                    {
                        try
                        {
                            dsTestImage.Tables[0].Rows[1].Delete();
                            dsTestImage.Tables[0].Rows[0].Delete();
                        }
                        catch (Exception ex)
                        {
                            writeLog.Write("删除图像结果失败" + ex.Message, "log");
                        }

                    }
                    for (int IImage = 0; IImage < GraphValue.Count; IImage++)
                    {
                        dsGUID = dsHandle.GetDataSet("ZLBASE.GUID", "dual", "");
                        dsTestImage.Tables[0].Rows.Add(dsGUID.Tables[0].Rows[0][0].ToString(), strTestSampleID, (IImage + 1).ToString());
                        strGraphID.Add(dsGUID.Tables[0].Rows[0][0].ToString());
                        strGraph.Add(GraphValue[IImage].ToString());
                    }
                    dtSave.Add(dsTestImage.Tables[0]);
                    selectStr.Add("select GRAPH_RESULT_ID,TEST_SAMPLE_ID,GRAPH_TYPE from TEST_GRAPH_RESULT");
                }
            }
        }

        //oracle连接
        public void UpdateData()
        {
            System.Data.OracleClient.OracleTransaction sqltran;
            //将事务绑定到连接对像
            //init.DBConnect();        
            try
            {
                //writeLog.Write("" + dtSave.Count, "log");
                using (OracleConnection connection = new OracleConnection(OracleHelper.GetConnectionstring()))
                {
                    sqltran = connection.BeginTransaction();// ConnectDB.con.BeginTransaction();
                    for (int i = 0; i < dtSave.Count; i++)
                    {

                        OracleDataAdapter adapter = new OracleDataAdapter(selectStr[i], connection);
                        OracleCommandBuilder cmdBuilder = new OracleCommandBuilder(adapter);

                        try
                        {
                            adapter.SelectCommand = new OracleCommand(selectStr[i], ConnectDB.con, sqltran);
                            adapter.InsertCommand = cmdBuilder.GetInsertCommand();
                            adapter.UpdateCommand = cmdBuilder.GetUpdateCommand();

                            if (dtSave[i].Select(null, null, DataViewRowState.Deleted).Length > 0)
                            {
                                adapter.Update(dtSave[i].Select(null, null, DataViewRowState.Deleted));
                            }
                            if (dtSave[i].Select(null, null, DataViewRowState.ModifiedCurrent).Length > 0)
                            {
                                adapter.Update(dtSave[i].Select(null, null, DataViewRowState.ModifiedCurrent));
                            }
                            if (dtSave[i].Select(null, null, DataViewRowState.Added).Length > 0)
                            {
                                adapter.Update(dtSave[i].Select(null, null, DataViewRowState.Added));
                            }
                        }
                        catch (Exception ex)
                        {
                            writeLog.Write("UpdateData处理：" + ex.Message, "log");
                        }
                    }
                    try
                    {
                        sqltran.Commit();
                    }
                    catch (Exception ex1)
                    {
                        sqltran.Rollback();
                        writeLog.Write("事务提交失败：" + ex1.Message, "log");
                    }
                    finally
                    {
                        connection.Close();
                        connection.Dispose();
                    }
                }
                for (int i = 0; i < strGraph.Count; i++)
                {
                    //writeLog.Write(strGraph[i].ToString(), "log");
                    System.IO.FileStream fs = new System.IO.FileStream(strGraph[i].ToString(), System.IO.FileMode.OpenOrCreate);
                    byte[] blob = new byte[fs.Length];
                    fs.Read(blob, 0, blob.Length);
                    fs.Close();

                    strUpdateSQL = "update test_graph_result set GRAPH_RESULT = :Photo where GRAPH_RESULT_ID = '" + strGraphID[i] + "'";
                    OracleCommand oraCmd = new OracleCommand(strUpdateSQL, ConnectDB.con);
                    OracleParameter op = new OracleParameter("Photo", OracleType.Blob);
                    op.Value = blob;
                    oraCmd.Parameters.Add(op);
                    try
                    {
                        oraCmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        writeLog.Write("UpdateDataIO处理：" + ex.Message, "log");
                    }
                }
            }
            catch (System.Data.OracleClient.OracleException sqlex)
            {
                // sqltran.Rollback();
                writeLog.Write("UpdateData_sqlex:"+sqlex.Message, "log");
                //return sqlex.Message;
            }
            catch (Exception ex)
            {
               // sqltran.Rollback();
                writeLog.Write("UpdateData_catch:"+ex.Message, "log");
                //return ex.Message;
            }
            finally
            {
                sqltran = null;
            }
          
        }


        //定义全局变量
        private static string appstr;

        public static string Appstr
        {
            get { return SaveResult.appstr; }
            set { SaveResult.appstr = value; }
        }



    }
}