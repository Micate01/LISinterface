using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
//using System.Data.OleDb;
using System.Data.OracleClient;

namespace ZLCHSLisComm
{
    class DrawGraph
    {
        ConnectDB init = new ConnectDB();
        string sqlstr = "";
        string sqlLandmark = "";
        DataSet dsGraph = new DataSet();
        DataSet dsLandmark = new DataSet();
        DataTable dtGraph = new DataTable("");
        DataTable dtLandmark = new DataTable("");
        DataRow drGraph;
        //OleDbDataAdapter ada = null;
        OracleDataAdapter ada = null;

        public void DrawGph( string GphType ,decimal ASKID ,decimal Channel ,int BG_Width ,int BG_Height ,int left ,int right ,int up ,int down ,int xmax,int ymax)
        {
            init.DBConnect();
            if (GphType == "BASE64")
            {
                sqlstr = "select ID,����ֵ from ����ͼ�μ�¼ where ����ID  = " + ASKID.ToString() + " and ������ĿID = " + Channel.ToString();

                ada = new OracleDataAdapter(sqlstr, ConnectDB.con);
                ada.Fill(dsGraph, "����ͼ�μ�¼");
                dtGraph = dsGraph.Tables[0];
                Base64StringToImage();
                return;
            }
            else
            {
                if (GphType == "ɢ��ͼ")
                {
                    sqlstr = "select to_number(substr( column_value , instr( column_value ,'|') +1 )) getrow,"
                            + "to_number(substr( column_value , 1,instr( column_value ,'|') -1 )) rowvalue "
                            + "from table(nec_ɢ��ͼ(" + ASKID.ToString() + ",'" + Channel.ToString() + "'))";
                }
                else if (GphType == "����ͼ")
                {
                    sqlstr = "select to_number(substr( column_value , instr( column_value ,'|') +1 )) getrow,"
                                + "to_number(substr( column_value , 1,instr( column_value ,'|') -1 )) rowvalue "
                                + "from table(Nec_Str2list( "
                                + "(select ����ֵ from ����ͼ�μ�¼ where ����ID  = " + ASKID.ToString() + " and ������ĿID = " + Channel.ToString() + "),',',null,'1'))";
                }
                //���
                sqlLandmark = "select ID,���,ͼ�� from ����ͼ�μ�¼ where ����ID  = " + ASKID.ToString() + " and ������ĿID = " + Channel.ToString();

                ada = new OracleDataAdapter(sqlstr, ConnectDB.con);
                ada.Fill(dsGraph, "����ͼ�μ�¼");
                dtGraph = dsGraph.Tables[0];

                ada = new OracleDataAdapter(sqlLandmark, ConnectDB.con);
                ada.Fill(dsLandmark, "����ͼ�μ�¼");
                dtLandmark = dsLandmark.Tables[0];

                //��ͼ����
                if (ymax == 0) ymax = 500;
                float pic_X = (BG_Width - left - right) / (xmax - 1);
                float pic_H = (BG_Height - up - down) / (ymax - 1);

                Pen Pic_Bolder = new Pen(Color.Black, 1);
                Pen Pic_line = new Pen(Color.Black, 1);
                Pen Pic_Data = new Pen(Color.Black, 1);
                SolidBrush brusth = new SolidBrush(Color.Blue);

                Bitmap Bg = new Bitmap(BG_Width, BG_Height, PixelFormat.Format24bppRgb);
                Graphics Ph = Graphics.FromImage(Bg);

                Font f = new Font(FontFamily.GenericSerif, 1.0F, FontStyle.Regular);
                Ph.Clear(Color.White);

                //Rectangle rec = new Rectangle(50, 15, 360, 150);
                //Ph.DrawRectangle(Pic_Bolder, rec);

                //ԭ��
                PointF cpt = new PointF(left, BG_Height - down);
                //x��������
                PointF[] xpt = new PointF[3] { new PointF(BG_Width - right + 5, cpt.Y), new PointF(BG_Width - right, cpt.Y - 5), new PointF(BG_Width - right, cpt.Y + 5) };
                //y��������
                PointF[] ypt = new PointF[3] { new PointF(cpt.X, up), new PointF(cpt.X - 5, up + 5), new PointF(cpt.X + 5, up + 5) };

                Point[] DataPt = new Point[dtGraph.Rows.Count];
                //�����
                int x;
                int y;
                int m = 0;
                for (int n = 0; n < dtGraph.Rows.Count; n++)
                {
                    drGraph = dtGraph.Rows[n];
                    x = Convert.ToInt32((Convert.ToInt32(drGraph[0].ToString()) - 1) * pic_X + cpt.X);
                    y = Convert.ToInt32(cpt.Y - Convert.ToInt32(drGraph[1].ToString()) * pic_H);
                    DataPt[m] = new Point(x, y);
                    m++;
                }

                //��x��
                Ph.DrawLine(Pens.Black, cpt.X, cpt.Y, BG_Width - right, cpt.Y);
                //��x���ͷ
                Ph.DrawPolygon(Pens.Black, xpt);
                //���x���ͷ
                Ph.FillPolygon(new SolidBrush(Color.Black), xpt);
                Ph.DrawString("x", new Font("����", 9), Brushes.Black, new PointF(BG_Width - right - 10, cpt.Y + 5));

                //��y��
                Ph.DrawLine(Pens.Black, cpt.X, cpt.Y, cpt.X, up);
                //��y���ͷ
                Ph.DrawPolygon(Pens.Black, ypt);
                //���y���ͷ
                Ph.FillPolygon(new SolidBrush(Color.Black), ypt);
                Ph.DrawString("y", new Font("����", 9), Brushes.Black, new PointF(cpt.X - 10, up + 10));

                ////��x��̶�
                //for (int i = 0; i < 900 / 50; i++)
                //{
                //    gph.DrawLine(Pens.White, cpt.X + i * 50, cpt.Y, cpt.X + i * 50, cpt.Y - 3);
                //    gph.DrawString(i.ToString(), new Font("����", 12), Brushes.White, new PointF(cpt.X + i * 50, cpt.Y + 20));
                //}
                Ph.DrawString(Convert.ToString((xmax - 1)), new Font("����", 9), Brushes.Black, new PointF(cpt.X + (xmax - 1) * pic_X, cpt.Y + 5));
                ////��y��̶�
                //for (int i = 0; i < 27; i++)
                //{
                //    gph.DrawLine(Pens.White, cpt.X, cpt.Y - i * 20, cpt.X + 3, cpt.Y - i * 20);
                //    gph.DrawString(Convert.ToString(i), new Font("����", 12), Brushes.White, new PointF(cpt.X - 30, cpt.Y - i * 20 - 10));
                //}
                Ph.DrawString(Convert.ToString((ymax - 1)), new Font("����", 9), Brushes.Black, new PointF(cpt.X - 20, cpt.Y - (ymax - 1) * pic_H));

                if (GphType == "ɢ��ͼ")
                {
                    //����
                    Point p = new Point(0, 0);
                    for (int n = 0; n < DataPt.Length; n++)
                    {
                        p = new Point(DataPt[n].X, DataPt[n].Y);
                        Ph.DrawString("��", f, new SolidBrush(Color.Black), p);
                    }
                }
                else if (GphType == "����ͼ")
                {
                    //������
                    Ph.DrawCurve(Pic_Data, DataPt);
                }

                //rec.
                Point SPoint = new Point();
                Point Epoint = new Point();
                if (dtLandmark.Rows[0]["���"].ToString().Length != 0)
                {
                    if (dtLandmark.Rows[0]["���"].ToString().Split(' ').Length == 18)
                    {
                        //ʵ��
                        Pic_line.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
                        SPoint.X = Convert.ToInt32(cpt.X + Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[0].ToString()) * pic_X);
                        SPoint.Y = Convert.ToInt32(cpt.Y);
                        Epoint.X = Convert.ToInt32(cpt.X + Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[0].ToString()) * pic_X);
                        Epoint.Y = Convert.ToInt32(cpt.Y - Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[12].ToString()) * pic_H);
                        Ph.DrawLine(Pic_line, SPoint, Epoint);

                        SPoint.X = Convert.ToInt32(cpt.X + Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[1].ToString()) * pic_X);
                        SPoint.Y = Convert.ToInt32(cpt.Y - Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[12].ToString()) * pic_H);
                        Epoint.X = Convert.ToInt32(cpt.X + Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[1].ToString()) * pic_X);
                        Epoint.Y = Convert.ToInt32(cpt.Y - Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[13].ToString()) * pic_H);
                        Ph.DrawLine(Pic_line, SPoint, Epoint);

                        SPoint.X = Convert.ToInt32(cpt.X + Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[2].ToString()) * pic_X);
                        SPoint.Y = Convert.ToInt32(cpt.Y - Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[13].ToString()) * pic_H);
                        Epoint.X = Convert.ToInt32(cpt.X + Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[2].ToString()) * pic_X);
                        Epoint.Y = Convert.ToInt32(cpt.Y - (ymax - 1) * pic_H);
                        Ph.DrawLine(Pic_line, SPoint, Epoint);

                        SPoint.X = Convert.ToInt32(cpt.X + Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[3].ToString()) * pic_X);
                        SPoint.Y = Convert.ToInt32(cpt.Y - Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[12].ToString()) * pic_H);
                        Epoint.X = Convert.ToInt32(cpt.X + Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[3].ToString()) * pic_X);
                        Epoint.Y = Convert.ToInt32(cpt.Y - Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[13].ToString()) * pic_H);
                        Ph.DrawLine(Pic_line, SPoint, Epoint);

                        SPoint.X = Convert.ToInt32(cpt.X + Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[4].ToString()) * pic_X);
                        SPoint.Y = Convert.ToInt32(cpt.Y - Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[14].ToString()) * pic_H);
                        Epoint.X = Convert.ToInt32(cpt.X + Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[4].ToString()) * pic_X);
                        Epoint.Y = Convert.ToInt32(cpt.Y - Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[13].ToString()) * pic_H);
                        Ph.DrawLine(Pic_line, SPoint, Epoint);

                        SPoint.X = Convert.ToInt32(cpt.X + Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[5].ToString()) * pic_X);
                        SPoint.Y = Convert.ToInt32(cpt.Y);
                        Epoint.X = Convert.ToInt32(cpt.X + Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[5].ToString()) * pic_X);
                        Epoint.Y = Convert.ToInt32(cpt.Y - Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[12].ToString()) * pic_H);
                        Ph.DrawLine(Pic_line, SPoint, Epoint);

                        SPoint.X = Convert.ToInt32(cpt.X + Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[6].ToString()) * pic_X);
                        SPoint.Y = Convert.ToInt32(cpt.Y);
                        Epoint.X = Convert.ToInt32(cpt.X + Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[6].ToString()) * pic_X);
                        Epoint.Y = Convert.ToInt32(cpt.Y - Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[12].ToString()) * pic_H);
                        Ph.DrawLine(Pic_line, SPoint, Epoint);

                        SPoint.X = Convert.ToInt32(cpt.X + Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[0].ToString()) * pic_X);
                        SPoint.Y = Convert.ToInt32(cpt.Y - Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[12].ToString()) * pic_H);
                        Epoint.X = Convert.ToInt32(cpt.X + Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[7].ToString()) * pic_X);
                        Epoint.Y = Convert.ToInt32(cpt.Y - Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[12].ToString()) * pic_H);
                        Ph.DrawLine(Pic_line, SPoint, Epoint);

                        SPoint.X = Convert.ToInt32(cpt.X + Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[10].ToString()) * pic_X);
                        SPoint.Y = Convert.ToInt32(cpt.Y - Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[14].ToString()) * pic_H);
                        Epoint.X = Convert.ToInt32(cpt.X + Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[9].ToString()) * pic_X);
                        Epoint.Y = Convert.ToInt32(cpt.Y - Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[12].ToString()) * pic_H);
                        Ph.DrawLine(Pic_line, SPoint, Epoint);

                        SPoint.X = Convert.ToInt32(cpt.X + Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[11].ToString()) * pic_X);
                        SPoint.Y = Convert.ToInt32(cpt.Y);
                        Epoint.X = Convert.ToInt32(cpt.X + Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[11].ToString()) * pic_X);
                        Epoint.Y = Convert.ToInt32(cpt.Y - Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[14].ToString()) * pic_H);
                        Ph.DrawLine(Pic_line, SPoint, Epoint);

                        SPoint.X = Convert.ToInt32(cpt.X + Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[1].ToString()) * pic_X);
                        SPoint.Y = Convert.ToInt32(cpt.Y - Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[13].ToString()) * pic_H);
                        Epoint.X = Convert.ToInt32(cpt.X + 127 * pic_X);
                        Epoint.Y = Convert.ToInt32(cpt.Y - Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[13].ToString()) * pic_H);
                        Ph.DrawLine(Pic_line, SPoint, Epoint);

                        SPoint.X = Convert.ToInt32(cpt.X + Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[10].ToString()) * pic_X);
                        SPoint.Y = Convert.ToInt32(cpt.Y - Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[14].ToString()) * pic_H);
                        Epoint.X = Convert.ToInt32(cpt.X + 127 * pic_X);
                        Epoint.Y = Convert.ToInt32(cpt.Y - Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[14].ToString()) * pic_H);
                        Ph.DrawLine(Pic_line, SPoint, Epoint);

                        SPoint.X = Convert.ToInt32(cpt.X + Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[7].ToString()) * pic_X);
                        SPoint.Y = Convert.ToInt32(cpt.Y - Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[12].ToString()) * pic_H);
                        Epoint.X = Convert.ToInt32(cpt.X + Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[8].ToString()) * pic_X);
                        Epoint.Y = Convert.ToInt32(cpt.Y);
                        Ph.DrawLine(Pic_line, SPoint, Epoint);

                        SPoint.X = Convert.ToInt32(cpt.X + (xmax - 1) * pic_X);
                        SPoint.Y = Convert.ToInt32(cpt.Y);
                        Epoint.X = Convert.ToInt32(cpt.X + (xmax - 1) * pic_X);
                        Epoint.Y = Convert.ToInt32(cpt.Y - (ymax - 1) * pic_H);
                        Ph.DrawLine(Pic_line, SPoint, Epoint);
                        //����
                        Pic_line.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                        SPoint.X = Convert.ToInt32(cpt.X + Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[2].ToString()) * pic_X);
                        SPoint.Y = Convert.ToInt32(cpt.Y - (Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[13].ToString()) + Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[15].ToString())) * pic_H);
                        Epoint.X = Convert.ToInt32(cpt.X + (xmax - 1) * pic_X);
                        Epoint.Y = Convert.ToInt32(cpt.Y - (Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[13].ToString()) + Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[15].ToString())) * pic_H);
                        Ph.DrawLine(Pic_line, SPoint, Epoint);

                        SPoint.X = Convert.ToInt32(cpt.X + Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[1].ToString()) * pic_X);
                        SPoint.Y = Convert.ToInt32(cpt.Y - (Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[12].ToString()) + Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[17].ToString())) * pic_H);
                        Epoint.X = Convert.ToInt32(cpt.X + Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[9].ToString()) * pic_X);
                        Epoint.Y = Convert.ToInt32(cpt.Y - (Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[12].ToString()) + Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[17].ToString())) * pic_H);
                        Ph.DrawLine(Pic_line, SPoint, Epoint);

                        SPoint.X = Convert.ToInt32(cpt.X + Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[9].ToString()) * pic_X);
                        SPoint.Y = Convert.ToInt32(cpt.Y - Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[12].ToString()) * pic_H);
                        Epoint.X = Convert.ToInt32(cpt.X + Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[9].ToString()) * pic_X);
                        Epoint.Y = Convert.ToInt32(cpt.Y - (Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[12].ToString()) + Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[17].ToString())) * pic_H);
                        Ph.DrawLine(Pic_line, SPoint, Epoint);

                        SPoint.X = Convert.ToInt32(cpt.X + (Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[9].ToString()) - Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[16].ToString())) * pic_X);
                        SPoint.Y = Convert.ToInt32(cpt.Y - Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[12].ToString()) * pic_H);
                        Epoint.X = Convert.ToInt32(cpt.X + Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[8].ToString()) * pic_X);
                        Epoint.Y = Convert.ToInt32(cpt.Y - (Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[14].ToString()) + Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[16].ToString())) * pic_H);
                        Ph.DrawLine(Pic_line, SPoint, Epoint);

                        SPoint.X = Convert.ToInt32(cpt.X + Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[9].ToString()) * pic_X);
                        SPoint.Y = Convert.ToInt32(cpt.Y - Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[14].ToString()) * pic_H);
                        Epoint.X = Convert.ToInt32(cpt.X + Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[9].ToString()) * pic_X);
                        Epoint.Y = Convert.ToInt32(cpt.Y - (Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[14].ToString()) + Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[16].ToString())) * pic_H);
                        Ph.DrawLine(Pic_line, SPoint, Epoint);
                    }
                    else
                    {
                        for (int i = 0; i < dtLandmark.Rows[0]["���"].ToString().Split(' ').Length; i++)
                        {
                            if (Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[i].ToString()) == 0) continue;
                            Pic_line.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                            SPoint.X = Convert.ToInt32(cpt.X + Convert.ToInt32(dtLandmark.Rows[0]["���"].ToString().Split(' ')[i].ToString()) * pic_X);
                            SPoint.Y = Convert.ToInt32(cpt.Y);
                            Epoint.X = Convert.ToInt32(cpt.X + Convert.ToDouble(dtLandmark.Rows[0]["���"].ToString().Split(' ')[i].ToString()) * pic_X);
                            Epoint.Y = up;
                            Ph.DrawLine(Pic_line, SPoint, Epoint);
                        }
                    }
                }

                //������
                string Title = "�����߲���";
                SolidBrush brush = new SolidBrush(Color.RoyalBlue);
                Ph.DrawString(Title, new Font("Franklin Gothic Demi", 12, FontStyle.Italic), brush, new Point(75, 0));
                Ph.Save();

                Bg.Save("11.Bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                //Bg.Save(Response.OutputStream, ImageFormat.Gif);

                Bitmap b = new Bitmap(320, 180);
                Graphics g = Graphics.FromImage(b);

                //ͼ������
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(Bg, new Rectangle(0, 0, 320, 180), new Rectangle(0, 0, Bg.Width, Bg.Height), GraphicsUnit.Pixel);
                g.Dispose();

                b.Save("11.Bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                SavePhoto(dtLandmark.Rows[0]["ID"].ToString());
            }
        }

        public void SavePhoto(string ID)
        {
            try
            {
                FileStream fs = new FileStream("11.Bmp", FileMode.OpenOrCreate);
                byte[] blob = new byte[fs.Length];
                fs.Read(blob, 0, blob.Length);
                fs.Close();

                string strSQL = "update ����ͼ�μ�¼ set ͼ�� =:Photo where ID = '" + ID + "' ";
                OracleCommand cmd = new OracleCommand(strSQL, ConnectDB.con);
                OracleParameter op = new OracleParameter("Photo", OracleType.Blob);
                op.Value = blob;
                cmd.Parameters.Add(op);
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
            //System.IO.File.Delete("11.Bmp");
        }

        private void Base64StringToImage()
        {
            string base64String = dtGraph.Rows[0]["����ֵ"].ToString();
            FileStream ifs = new FileStream(@"c:\ABX", FileMode.Open, FileAccess.Read);
            StreamReader sr = new System.IO.StreamReader(ifs, System.Text.Encoding.Default);
            base64String = sr.ReadToEnd();


            //System.IO.StreamReader inFile;
            //try
            //{
            //    char[] base64CharArray;
            //    inFile = new System.IO.StreamReader(inputFileName, System.Text.Encoding.Default);
            //    base64CharArray = new char[inFile.BaseStream.Length];
            //    inFile.Read(base64CharArray, 0, (int)inFile.BaseStream.Length);
            //    base64String = new string(base64CharArray);
            //}
            //catch (System.Exception exp)
            //{
            //    // Error creating stream or reading from it.
            //    System.Console.WriteLine("{0}", exp.Message);
            //    return;
            //}

            // Convert the Base64 UUEncoded input into binary output.
            byte[] bytes;
            try
            {
                bytes = System.Convert.FromBase64String(base64String);
            }
            catch (System.ArgumentNullException)
            {
                System.Console.WriteLine("Base 64 string is null.");
                return;
            }
            catch (System.FormatException)
            {
                System.Console.WriteLine("Base 64 string length is not 4 or is not an even multiple of 4.");
                return;
            }

            MemoryStream ms = new MemoryStream(bytes);
            Image image = Image.FromStream(ms);
            image.Save(@"c:\δ����.bmp", ImageFormat.Bmp);
        }
    }
}
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       