using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
namespace ZLCHSLisComm
{
    public interface IDataResolve
    {
        void ParseResult();
        void ParseResult(string strSource, ref string strResult, ref string strReserved, ref string strCmd);
        // string GetDate(string FieldPara, DataRow drField);
        System.Drawing.Image LocalIMG(string IMG);
        string GetCmd(string dataIn, string ack_term);
        void GetRules(string StrDevice);
        void SetVariable(DataTable dt);
    }
}
