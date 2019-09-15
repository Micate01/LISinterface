using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.OracleClient;
using System.Windows.Forms;
//using System.Data.OleDb;

namespace ZLCHSLisComm
{
    public class DataSetHandle
    {
        ConnectDB init;
        Write_Log writelog;
        public string strError;

        public DataSet GetDataSet(string column, string tablename, string strwhere)
        {
            using (OracleConnection _connection = new OracleConnection(OracleHelper.GetConnectionstring()))
            {
                writelog = new Write_Log();
                DataSet ds = new DataSet();
                string sqlstr = "select " + column + " from " + tablename;
                if (!String.IsNullOrEmpty(strwhere))
                {
                    sqlstr = sqlstr + " where " + strwhere;
                }
                try
                {
                    _connection.Open();
                    OracleDataAdapter da = new OracleDataAdapter();
                    da.SelectCommand = new OracleCommand(sqlstr, _connection);
                    da.Fill(ds);
                }
                catch (Exception ex)
                {
                    writelog.Write("填充数据时出错!" + ex.Message + System.Environment.NewLine + sqlstr, "log");
                    return null;
                }
                finally { 
                    _connection.Close();
                    _connection.Dispose();
                }

                writelog = null;
                return ds;
            }




            //writelog = new Write_Log();
            //if (ConnectDB.con.State != ConnectionState.Open)
            //{
            //    init = new ConnectDB();
            //    strError = init.DBConnect();
            //    if (!string.IsNullOrEmpty(strError))
            //    {
            //        writelog.Write(strError, "log");
            //        return null;
            //    }
            //}


            //string sqlstr = "select " + column + " from " + tablename;
            //if (!String.IsNullOrEmpty(strwhere))
            //{
            //    sqlstr = sqlstr + " where " + strwhere;
            //}
            ////writelog.Write(sqlstr, "log");

            //DataSet ds = new DataSet();
            //writelog.Write("1连接状态：" + ConnectDB.con.State.ToString() + "\r\n 连接字符串：" + ConnectDB.con.ConnectionString, "log");
            //OracleDataAdapter ada = new OracleDataAdapter(sqlstr, ConnectDB.con);

            //try
            //{
            //    writelog.Write("2连接状态："+ConnectDB.con.State.ToString()+"\r\n 连接字符串："+ ConnectDB.con.ConnectionString, "log");
            //    ada.Fill(ds, tablename);
            //}
            //catch (Exception ex)
            //{
                
            //    writelog.Write("填充数据时出错!" + ex.Message + System.Environment.NewLine + sqlstr, "log");
            //    return null;
            //}

            ////init.DisConnectDB(ConnectDB.con);
            //init = null;
            //writelog = null;
            //return ds;
        }
    }
}
