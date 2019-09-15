/******************************
 * 类名：折线图绘制
 * 作者：张祥明
 * 时间：2011-11-25
 * ****************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
namespace ZLCHSLisComm
{
    /// <summary>
    /// 折线图绘制
    /// </summary>
    public class FoldLineDiagram
    {
        private int m_Height = 100;//图片的宽。
        private int m_Width = 150;//图片的高
        private int m_Padding = 50;//默认上下边距
        private bool m_IsCalibration = false; //是否显示界标


        private ArrayList m_XAxis;//折线图中的点的X坐标。
        private ArrayList m_YAxis;//折线图中的点的Y坐标。
        private Color m_graphColor = Color.Red;//折线的颜色
        private float m_XSlice = 1;//
        private float m_YSlice = 1;//
        private Graphics objGraphics;//图形对象。
        private Bitmap objBitmap;//位图对象
        private string m_XAxisText = "";//X轴注释。
        private string m_YAxisText = "";//Y轴注释。
        private string m_Title = "";//图标标题.
        private Color m_TitleBackColor = Color.Cyan;//标题背景色
        private Color m_TitleForeColor = Color.Green;//标题字体颜色

        /// <summary>
        /// 图片的宽
        /// </summary>
        public int Width
        {
            get { return m_Width; }
            set
            {
                if (value > 200)
                {
                    m_Width = value;
                }
            }
        }

        /// <summary>
        /// 图片的高
        /// </summary>
        public int Height
        {
            get { return m_Height; }
            set
            {
                if (value > 200)
                {
                    m_Height = value;
                }
            }
        }
        /// <summary>
        /// 填充上下边距
        /// </summary>
        public int Padding
        {
            get { return m_Padding; }
            set
            {

                m_Padding = value;

            }
        }

        /// <summary>
        /// 是否显示界标
        /// </summary>
        public bool IsCalibration
        {
            get { return m_IsCalibration; }
            set { m_IsCalibration = value; }
        }

        /// <summary>
        /// 折线图中的点的X坐标。
        /// </summary>
        public ArrayList XAxis
        {
            get { return m_XAxis; }
            set { m_XAxis = value; }
        }

        /// <summary>
        /// 折线图中的点的Y坐标。
        /// </summary>
        public ArrayList YAxis
        {
            get { return m_YAxis; }
            set { m_YAxis = value; }
        }

        /// <summary>
        /// 折线的颜色
        /// </summary>
        public Color GraphColor
        {
            set { m_graphColor = value; }
            get { return m_graphColor; }
        }

        /// <summary>
        /// X轴一个大刻度所表示的数量。
        /// </summary>
        public float XSlice
        {
            set { m_XSlice = value; }
            get { return m_XSlice; }
        }
        /// <summary>
        /// Y轴一个大刻度所表示的数量。
        /// </summary>
        public float YSlice
        {
            set { m_YSlice = value; }
            get { return m_YSlice; }
        }

        /// <summary>
        ///X轴注释。 
        /// </summary>
        public string XAxisText
        {
            get { return m_XAxisText; }
            set { m_XAxisText = value; }
        }

        /// <summary>
        /// Y轴注释。
        /// </summary>
        public string YAxisText
        {
            get { return m_YAxisText; }
            set { m_YAxisText = value; }
        }

        /// <summary>
        /// 图标标题.
        /// </summary>
        public string Title
        {
            get { return m_Title; }
            set { m_Title = value; }
        }

        /// <summary>
        /// 标题背景色
        /// </summary>
        public Color TitleBackColor
        {
            get { return m_TitleBackColor; }
            set { m_TitleBackColor = value; }
        }

        /// <summary>
        /// 标题字体颜色
        /// </summary>
        public Color TitleForeColor
        {
            get { return m_TitleForeColor; }
            set { m_TitleForeColor = value; }
        }

        /// <summary>
        /// 建立图像，位图对象，设置图像背景颜色，画X轴和Y轴
        /// </summary>
        public void InitializeGraph()
        {
            objBitmap = new Bitmap(Width, Height);
            objGraphics = Graphics.FromImage(objBitmap);
            objGraphics.FillRectangle(new SolidBrush(Color.White), 0, 0, Width, Height);//以白色作为图像的总体背景颜色。
            //画X轴
            objGraphics.DrawLine(new Pen(new SolidBrush(Color.Black), 1), Padding, Height - Padding, Width - Padding, Height - Padding);
            PointF[] xpt = new PointF[3] { new PointF(Width - 75, Height - 100), new PointF(Width - 100, Height - 108), new PointF(Width - 100, Height - 92) };//x轴三角形
            //objGraphics.DrawPolygon(Pens.Black, xpt);
            //objGraphics.FillPolygon(new SolidBrush(Color.Black), xpt);
            SetXAxis(ref objGraphics, XSlice);
            //画Y轴
            objGraphics.DrawLine(new Pen(new SolidBrush(Color.Black), 1), Padding, Height - Padding, Padding, 30);
            PointF[] ypt = new PointF[3] { new PointF(100, 10), new PointF(92, 30), new PointF(108, 30) };//x轴三角形
            //objGraphics.DrawPolygon(Pens.Black, ypt);
            //objGraphics.FillPolygon(new SolidBrush(Color.Black), ypt);
            SetYAxis(ref objGraphics, YSlice);
            if (Title == "")
            {
                //画X轴原点
                objGraphics.DrawString("1", new Font("Courier New", 10), new SolidBrush(Color.Black), Padding, Height - (90 - Padding));
                //画Y轴原点
                objGraphics.DrawString("0", new Font("Courier New", 10), new SolidBrush(Color.Black), Padding - 20, Height - (110 - Padding));
            }
            else
            {
                //画0点
                objGraphics.DrawString("0", new Font("Courier New", 10), new SolidBrush(Color.Black), Padding, Height - (90 - Padding));
            }
            //写坐标轴的注释。
            SetAxisText(ref objGraphics);
            //写标题
            CreateTitle(ref objGraphics);

        }

        /// <summary>
        /// 
        /// 设置X，Y轴上的注释
        /// </summary>
        /// <param name="objGraphics">Graphics </param>
        private void SetAxisText(ref Graphics objGraphics)
        {
            //写X轴注释
            objGraphics.DrawString(XAxisText, new Font("Courier New", 10), new SolidBrush(Color.LimeGreen), Width / 2 - 20, Height - 20);
            //写Y轴注释。
            int X = 30;
            int Y = (Height / 2) - 50;
            for (int iIndex = 0; iIndex < YAxisText.Length; iIndex++)
            {
                objGraphics.DrawString(YAxisText[iIndex].ToString(), new Font("Courier New", 10), new SolidBrush(Color.LimeGreen), X - 20, Y);
                Y += 15;
            }
        }
        /// <summary>
        /// 画标题
        /// </summary>
        /// <param name="objGraphics">Graphics</param>
        private void CreateTitle(ref Graphics objGraphics)
        {
            //绘制标题背景
            // objGraphics.FillRectangle(new SolidBrush(TitleBackColor), Height - 100, 5, Height - (200-Padding), 20);
            Rectangle rect = new Rectangle(Height - 100, 5, Height - (200 - Padding), 20);
            objGraphics.DrawString(Title, new Font("Verdana", 10), new SolidBrush(TitleForeColor), rect);
        }

        /// <summary>
        /// 生成折线图(绘制折线)
        /// </summary>
        public void DrawContent()
        {
            SetPixels(ref objGraphics);//画折线
        }

        /// <summary>
        /// 画折线
        /// </summary>
        /// <param name="objGraphics">Graphics</param>
        private void SetPixels(ref Graphics objGraphics)
        {
            float X1 = float.Parse(XAxis[0].ToString()) / XSlice * 50;
            float Y1 = float.Parse(YAxis[0].ToString()) / YSlice * 50;

            if (XAxis.Count == YAxis.Count)//正常情况下。
            {
                //依次连接各个点，形成折线。
                for (int iXaxis = 0, iYaxis = 0; (iXaxis < XAxis.Count - 1 && iYaxis < YAxis.Count - 1); iXaxis++, iYaxis++)
                {
                    PlotGraph(ref objGraphics, X1, Y1, float.Parse(XAxis[iXaxis + 1].ToString()) / XSlice * 50, float.Parse(YAxis[iYaxis + 1].ToString()) / YSlice * 50);
                    X1 = float.Parse(XAxis[iXaxis + 1].ToString()) / XSlice * 50;
                    Y1 = float.Parse(YAxis[iYaxis + 1].ToString()) / YSlice * 50;
                }
            }
        }

        /// <summary>
        /// 返回Bitmap对象。
        /// </summary>
        /// <returns></returns>
        public Bitmap GetGraph()
        {
            return objBitmap;
        }

        /// <summary>
        /// 两点之间画直线。
        /// </summary>
        /// <param name="objGraphics">objGraphics</param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        private void PlotGraph(ref Graphics objGraphics, float x1, float y1, float x2, float y2)//
        {
            //new Pen (new SolidBrush(GraphColor),画出曲线的宽度)  //1 
            objGraphics.DrawLine(new Pen(new SolidBrush(GraphColor), 1), x1 + Padding, (Height - Padding) - y1, x2 + Padding, (Height - Padding) - y2);
            // objGraphics.DrawLine(new Pen(new SolidBrush(GraphColor), 1), x1 + 50, (Height - 50) - y1, x2 + 50, (Height - 50) - y2);
        }

        /// <summary>
        /// 画X轴的刻度.
        /// </summary>
        /// <param name="objGraphics">objGraphics</param>
        /// <param name="iSlices"></param>
        private void SetXAxis(ref Graphics objGraphics, float iSlices)
        {
            float x1 = Padding, y1 = Height - (110 - Padding), x2 = Padding, y2 = Height - (90 - Padding);
            //float x1 = 50, y1 = Height - 60, x2 = 50, y2 = Height - 40;
            int iCount = 0;
            int iSliceCount = 1;
            //Width
            if (Title == "")
            {
                for (int iIndex = 0; iIndex <= 825 - 200; iIndex += 25)
                {
                    if (iCount == 5)
                    {
                        /**********画竖虚线*************/
                        if (!IsCalibration)
                        {
                            Pen penDashed = new Pen(new SolidBrush(Color.Black));
                            penDashed.DashStyle = DashStyle.Dash;
                            objGraphics.DrawLine(penDashed, x1 + iIndex, y2, x2 + iIndex, x1 - 10 / 2); //绘制竖线虚线
                        }
                        /*************END*********/
                        objGraphics.DrawLine(new Pen(new SolidBrush(Color.Black)), x1 + iIndex, y1 + 5, x2 + iIndex, y2 - 10);

                        switch (iSliceCount)
                        {
                            case 1: objGraphics.DrawString(Convert.ToString(3), new Font("verdana", 8), new SolidBrush(Color.Black), x1 + iIndex - 10, y2);//X刻度下的坐标
                                iCount = 0;
                                iSliceCount++; break;
                            case 2: objGraphics.DrawString(Convert.ToString(10), new Font("verdana", 8), new SolidBrush(Color.Black), x1 + iIndex - 10, y2);//X刻度下的坐标
                                iCount = 0;
                                iSliceCount++; break;
                            case 3: objGraphics.DrawString(Convert.ToString(30), new Font("verdana", 8), new SolidBrush(Color.Black), x1 + iIndex - 10, y2);//X刻度下的坐标
                                iCount = 0;
                                iSliceCount++; break;
                            case 4: objGraphics.DrawString(Convert.ToString(100), new Font("verdana", 8), new SolidBrush(Color.Black), x1 + iIndex - 10, y2);//X刻度下的坐标
                                iCount = 0;
                                iSliceCount++; break;
                            case 5: objGraphics.DrawString(Convert.ToString(200), new Font("verdana", 8), new SolidBrush(Color.Black), x1 + iIndex - 10, y2);//X刻度下的坐标
                                iCount = 0;
                                iSliceCount++; break;
                            default: break;
                        }
                    }
                    else
                    {
                        //if (iCount > 0)
                        // objGraphics.DrawLine(new Pen(new SolidBrush(Color.Black)), x1 + iIndex, y1 + 8, x2 + iIndex, y2 - 8);
                    }
                    iCount++;
                }
            }
            else
            {
                for (int iIndex = 0; iIndex <= 400 - 200; iIndex += 10)
                {
                    if (iCount == 5)
                    {
                        /**********画竖虚线*************/
                        if (IsCalibration)
                        {
                            Pen penDashed = new Pen(new SolidBrush(Color.Black));
                            penDashed.DashStyle = DashStyle.Dash;
                            objGraphics.DrawLine(penDashed, x1 + iIndex, y2, x2 + iIndex, x1 - 10 / 2); //绘制竖线虚线
                        }
                        /*************END*********/
                        objGraphics.DrawLine(new Pen(new SolidBrush(Color.Black)), x1 + iIndex, y1 + 5, x2 + iIndex, y2 - 5);
                        //objGraphics.DrawString(Convert.ToString(iSlices * iSliceCount), new Font("verdana", 8), new SolidBrush(Color.Black), x1 + iIndex - 10, y2);//X刻度下的坐标
                        switch (Title)
                        {
                            case "PLT":
                                objGraphics.DrawString(Convert.ToString(5 * iSliceCount), new Font("verdana", 8), new SolidBrush(Color.Black), x1 + iIndex - 10, y2);
                                break;
                            case "WBC":
                                objGraphics.DrawString(Convert.ToString(iSlices * iSliceCount), new Font("verdana", 8), new SolidBrush(Color.Black), x1 + iIndex - 10, y2);
                                break;
                            case "RBC":
                                objGraphics.DrawString(Convert.ToString(50 * iSliceCount), new Font("verdana", 8), new SolidBrush(Color.Black), x1 + iIndex - 10, y2);
                                break;
                            default:
                                break;
                        }
                        iCount = 0;
                        iSliceCount++;
                    }
                    else
                    {
                        // if (iCount > 0)
                        //objGraphics.DrawLine(new Pen(new SolidBrush(Color.Black)), x1 + iIndex, y1 + 8, x2 + iIndex, y2 - 8);
                    }
                    iCount++;
                }
            }
        }

        /// <summary>
        /// 画Y轴的刻度  
        /// </summary>
        /// <param name="objGraphics"></param>
        /// <param name="iSlices"></param>
        private void SetYAxis(ref Graphics objGraphics, float iSlices)//画Y轴的刻度
        {
            int x1 = 95 - Padding;
            int y1 = Height - (110 - Padding);
            int x2 = 105 - Padding;
            int y2 = Height - (110 - Padding);

            //int x1 = 45;
            //int y1 = Height - 60;
            //int x2 = 55;
            //int y2 = Height - 60;

            int iCount = 1;
            int iSliceCount = 1;
            if (Title == "")
            {
                for (int iIndex = 0; iIndex < 460 - 100; iIndex += 8)
                {
                    if (iCount == 5)
                    {
                        /**********画横线虚线*************/
                        if (!IsCalibration)
                        {
                            Pen penDashed = new Pen(new SolidBrush(Color.Black));
                            penDashed.DashStyle = DashStyle.Dash;
                            objGraphics.DrawLine(penDashed, x1, y1 - iIndex, Width - x2 + iIndex, y2 - iIndex); //绘制竖线虚线
                        }
                        /*************END*********/

                        objGraphics.DrawLine(new Pen(new SolidBrush(Color.Black)), x1 + 5, y1 - iIndex, x2 + 3, y2 - iIndex);
                        objGraphics.DrawString(Convert.ToString(iSlices * iSliceCount), new Font("verdana", 8), new SolidBrush(Color.Black), x1 - 20, y1 - iIndex - 8);
                        iCount = 0;
                        iSliceCount++;
                    }
                    else
                    {
                        //objGraphics.DrawLine(new Pen(new SolidBrush(Color.Goldenrod)), x1, (y1 - iIndex), x2, (y2 - iIndex));
                    }
                    iCount++;
                }
            }
        }

        /// <summary>
        /// 创建图像
        /// </summary>
        /// <param name="imgName">图片名称</param>
        /// <param name="imgType">图片类型</param>
        /// <param name="width">图片宽度</param>
        /// <param name="height">图片高度</param>
        /// <param name="xSlice">X轴刻度</param>
        /// <param name="ySlice">Y轴刻度</param>
        /// <param name="padding">X轴Y轴边距</param>
        /// <param name="leftBegin">从左边间隔多少像素开始</param>
        /// <param name="fltsValues">float[] 数据</param>
        /// <param name="clrsCurveColors">Color[] 颜色</param>
        /// <returns></returns>
        public static Bitmap CreateImage(string imgName, string imgType, int width, int height, int xSlice, int ySlice, int padding, int leftBegin, string yAxisValues, Color[] clrsCurveColors)
        {
            FoldLineDiagram line2d = new FoldLineDiagram();
            line2d.Height = height;
            line2d.Width = width;
            line2d.XSlice = xSlice;
            line2d.YSlice = ySlice;
            line2d.Title = imgName;
            line2d.TitleForeColor = Color.Black;
            line2d.GraphColor = Color.Red;
            line2d.InitializeGraph();
            ArrayList arX = new ArrayList();
            ArrayList arY = new ArrayList();
            string[] _yAxisValues = yAxisValues.Split('|');
            for (int i = 0; i < _yAxisValues.Length - 1; i++)
            {
                string strImageData = _yAxisValues[i];
                for (int n = 0; n < strImageData.TrimStart(';').Split(';').Length; n++)
                {
                    arX.Add(n + leftBegin);
                    arY.Add(Math.Abs(Convert.ToDecimal(strImageData.TrimStart(';').Split(';')[n])));
                }
                line2d.GraphColor = clrsCurveColors[i];
                line2d.XAxis = arX;
                line2d.YAxis = arY;
                line2d.DrawContent();
            }
            Bitmap bmp = line2d.GetGraph();
            return bmp;
        }

        /// <summary>
        /// 创建图像
        /// </summary>
        /// <param name="imgName">图片名称</param>
        /// <param name="imgType">图片类型</param>
        /// <param name="width">图片宽度</param>
        /// <param name="height">图片高度</param>
        /// <param name="xSlice">X轴刻度</param>
        /// <param name="ySlice">Y轴刻度</param>
        /// <param name="padding">X轴Y轴边距</param>
        /// <param name="leftBegin">从左边间隔多少像素开始</param>
        /// <param name="fltsValues">float[] 数据</param>
        /// <param name="clrsCurveColors">Color[] 颜色</param>
        /// <returns></returns>
        public static Bitmap CreateBmp(string imgName, string imgType, int width, int height, int xSlice, int ySlice, int padding, int leftBegin, string yAxisValues, Color[] clrsCurveColors)
        {
            FoldLineDiagram line2d = new FoldLineDiagram();
            line2d.Height = height;
            line2d.Width = width;
            line2d.XSlice = xSlice;
            line2d.YSlice = ySlice;
            line2d.Title = imgName;
            line2d.TitleForeColor = Color.Black;
            line2d.GraphColor = Color.Red;
            line2d.InitializeGraph();
            ArrayList arX = new ArrayList();
            ArrayList arY = new ArrayList();
            string[] _yAxisValues = yAxisValues.Split('|');
            for (int i = 0; i < _yAxisValues.Length - 1; i++)
            {
                string strImageData = _yAxisValues[i];
                arX = new ArrayList();
                arY = new ArrayList();
                for (int n = 0; n < strImageData.TrimStart(';').TrimEnd(';').Split(';').Length; n++)
                {
                    string Intx = Math.Abs(Convert.ToDecimal(strImageData.TrimStart(';').Split(';')[n].Split(',')[0])).ToString();
                    if (Intx == "5")
                    {
                        arX.Add(85);
                        arY.Add(Math.Abs(Convert.ToDecimal(strImageData.TrimStart(';').Split(';')[n].Split(',')[1])));
                        continue;
                    }
                    if (Intx == "50")
                    {
                        arX.Add(205);
                        arY.Add(Math.Abs(Convert.ToDecimal(strImageData.TrimStart(';').Split(';')[n].Split(',')[1])));
                        continue;
                    }
                    if (Intx == "100")
                    {
                        arX.Add(250);
                        arY.Add(Math.Abs(Convert.ToDecimal(strImageData.TrimStart(';').Split(';')[n].Split(',')[1])));
                        continue;
                    }
                    if (Intx == "200")
                    {
                        arX.Add(315);
                        arY.Add(Math.Abs(Convert.ToDecimal(strImageData.TrimStart(';').Split(';')[n].Split(',')[1])));
                        continue;
                    }
                    else
                    {
                        arX.Add(Math.Abs(Convert.ToDecimal(strImageData.TrimStart(';').Split(';')[n].Split(',')[0])));
                        arY.Add(Math.Abs(Convert.ToDecimal(strImageData.TrimStart(';').Split(';')[n].Split(',')[1])));
                    }
                }
                line2d.GraphColor = clrsCurveColors[i];
                line2d.XAxis = arX;
                line2d.YAxis = arY;
                line2d.DrawContent();
            }
            Bitmap bmp = line2d.GetGraph();
            return bmp;
        }

    }
}
