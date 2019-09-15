/**************************************
 * 说明：折线图绘制
 * 作者：张祥明
 * 时间：2013-03-10
 * 修改人：
 * 修改时间：
 * ************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Data;
using System.Drawing.Drawing2D;
using System.Collections;
using System.Drawing.Imaging;
namespace ZLCHSLisComm
{
    public class Curve2D
    {
        private Graphics objGraphics; //Graphics 类提供将对象绘制到显示设备的方法
        private Bitmap objBitmap; //位图对象
        private float fltWidth = 700; //图像宽度
        private float fltHeight = 400; //图像高度
        private float fltXSlice = 20; //X轴刻度宽度
        private float fltYSlice = 20; //Y轴刻度宽度
        private float fltYSliceValue = 5; //Y轴刻度的数值宽度
        private float fltXSliceValue = 10; //X轴刻度的数值宽度
        private float fltYSliceBegin = 0; //Y轴刻度开始值
        private float fltTension = 0.5f;
        private string strTitle = ""; //标题
        private string strXAxisText = ""; //X轴说明文字
        private string strYAxisText = ""; //Y轴说明文字
        private string[] strsKeys = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" }; //键
        private float[] fltsValues = new float[] { 20.0f, 30.0f, 50.0f, 55.4f, 21.6f, 12.8f, 99.5f, 36.4f, 78.2f, 56.4f, 45.8f, 66.5f, 99.5f, 36.4f, 78.2f, 56.4f, 45.8f, 66.5f, 20.0f, 30.0f, 50.0f, 55.4f, 21.6f, 12.8f }; //值
        private Color clrBgColor = Color.Snow; //背景色
        private Color clrTextColor = Color.Black; //文字颜色
        private Color clrBorderColor = Color.Black; //整体边框颜色
        private Color clrAxisColor = Color.Black; //轴线颜色
        private Color clrAxisTextColor = Color.Black; //轴说明文字颜色
        private Color clrSliceTextColor = Color.Black; //刻度文字颜色
        private Color clrSliceColor = Color.Black; //刻度颜色
        private Color[] clrsCurveColors = new Color[] { Color.Black, Color.Black, Color.Green }; //曲线颜色
        private float fltXSpace = 40f; //图像左右距离边缘距离
        private float fltYSpace = 40f; //图像上下距离边缘距离
        private int intFontSize = 9; //字体大小号数
        private float fltXRotateAngle = 0f; //X轴文字旋转角度

        private float fltYRotateAngle = 0f; //Y轴文字旋转角度
        private int intCurveSize = 2; //曲线线条大小
        private int intFontSpace = 0; //intFontSpace 是字体大小和距离调整出来的一个比较适合的数字
        private string m_YAxis; //Y轴的最大最小值

        private bool isXSliceLine = true; //是否画X轴的刻度线 
        private bool isYSliceLine = true;//是否画Y轴的刻度线
        private bool isXSliceSign = true; //是否画X轴的刻度标志记号
        private bool isYSliceSign = true;//是否画Y轴的刻度标志记号

        private string applianceType = "1"; //仪器类型
        private bool isXAxisValue = false; //是否传递X抽的值
        private float xAxisCount = 15;//X轴刻度总数
        private string newSliceValues = "";//重新定义新的刻度值；
        #region 公共属性
        /// <summary>
        /// 图像的宽度
        /// </summary>
        public float Width
        {
            set
            {
                if (value < 100)
                {
                    fltWidth = 100;
                }
                else
                {
                    fltWidth = value;
                }
            }
            get
            {
                if (fltWidth <= 100)
                {
                    return 100;
                }
                else
                {
                    return fltWidth;
                }
            }
        }
        /// <summary>
        /// 图像的高度
        /// </summary>
        public float Height
        {
            set
            {
                if (value < 100)
                {
                    fltHeight = 100;
                }
                else
                {
                    fltHeight = value;
                }
            }
            get
            {
                if (fltHeight <= 100)
                {
                    return 100;
                }
                else
                {
                    return fltHeight;
                }
            }
        }
        /// <summary>
        /// X轴刻度宽度
        /// </summary>
        public float XSlice
        {
            set { fltXSlice = value; }
            get { return fltXSlice; }
        }
        /// <summary>
        /// Y轴刻度宽度
        /// </summary>
        public float YSlice
        {
            set { fltYSlice = value; }
            get { return fltYSlice; }
        }
        /// <summary>
        /// Y轴刻度的数值宽度
        /// </summary>
        public float YSliceValue
        {
            set { fltYSliceValue = value; }
            get { return fltYSliceValue; }
        }
        /// <summary>
        /// X轴刻度的数值宽度
        /// </summary>
        public float XSliceValue
        {
            set { fltXSliceValue = value; }
            get { return fltXSliceValue; }

        }
        /// <summary>
        /// Y轴刻度开始值
        /// </summary>
        public float YSliceBegin
        {
            set { fltYSliceBegin = value; }
            get { return fltYSliceBegin; }
        }
        /// <summary>
        /// 张力系数
        /// </summary>
        public float Tension
        {
            set
            {
                if (value < 0.0f && value > 1.0f)
                {
                    fltTension = 0.5f;
                }
                else
                {
                    fltTension = value;
                }
            }
            get { return fltTension; }
        }
        /// <summary>
        /// 标题
        /// </summary>
        public string Title
        {
            set { strTitle = value; }
            get { return strTitle; }
        }
        /// <summary>
        /// 键，X轴数据
        /// </summary>
        public string[] Keys
        {
            set
            {
                strsKeys = value;
            }
            get { return strsKeys; }
        }
        /// <summary>
        /// 值，Y轴数据
        /// </summary>
        public float[] Values
        {
            set { fltsValues = value; }
            get { return fltsValues; }
        }
        /// <summary>
        /// 背景色
        /// </summary>
        public Color BgColor
        {
            set
            {
                clrBgColor = value;
            }
            get { return clrBgColor; }
        }
        /// <summary>
        /// 文字颜色
        /// </summary>
        public Color TextColor
        {
            set { clrTextColor = value; }
            get { return clrTextColor; }
        }
        /// <summary>
        /// 整体边框颜色
        /// </summary>
        public Color BorderColor
        {
            set { clrBorderColor = value; }
            get { return clrBorderColor; }
        }
        /// <summary>
        /// 轴线颜色
        /// </summary>
        public Color AxisColor
        {
            set
            {
                clrAxisColor = value;
            }
            get { return clrAxisColor; }
        }
        /// <summary>
        /// X轴说明文字
        /// </summary>
        public string XAxisText
        {
            set { strXAxisText = value; }
            get { return strXAxisText; }
        }
        /// <summary>
        /// Y轴说明文字
        /// </summary>
        public string YAxisText
        {
            set { strYAxisText = value; }
            get { return strYAxisText; }
        }
        /// <summary>
        /// 轴说明文字颜色
        /// </summary>
        public Color AxisTextColor
        {
            set { clrAxisTextColor = value; }
            get { return clrAxisTextColor; }
        }
        /// <summary>
        /// 刻度文字颜色
        /// </summary>
        public Color SliceTextColor
        {
            set
            {
                clrSliceTextColor = value;
            }
            get { return clrSliceTextColor; }
        }
        /// <summary>
        /// 刻度颜色
        /// </summary>
        public Color SliceColor
        {
            set { clrSliceColor = value; }
            get { return clrSliceColor; }
        }
        /// <summary>
        /// 曲线颜色
        /// </summary>
        public Color[] CurveColors
        {
            set { clrsCurveColors = value; }
            get
            {
                return clrsCurveColors;
            }
        }
        /// <summary>
        /// X轴文字旋转角度
        /// </summary>
        public float XRotateAngle
        {
            get { return fltXRotateAngle; }
            set { fltXRotateAngle = value; }
        }
        /// <summary>
        /// Y轴文字旋转角度
        /// </summary>
        public float YRotateAngle
        {
            get
            {
                return fltYRotateAngle;
            }
            set { fltYRotateAngle = value; }
        }
        /// <summary>
        /// 图像左右距离边缘距离
        /// </summary>
        public float XSpace
        {
            get
            {
                return fltXSpace;
            }
            set { fltXSpace = value; }
        }
        /// <summary>
        /// 图像上下距离边缘距离
        /// </summary>
        public float YSpace
        {
            get { return fltYSpace; }
            set { fltYSpace = value; }
        }
        /// <summary>
        /// 字体大小号数
        /// </summary>
        public int FontSize
        {
            get { return intFontSize; }
            set { intFontSize = value; }
        }
        /// <summary>
        /// 曲线线条大小
        /// </summary>
        public int CurveSize
        {
            get
            {
                return intCurveSize;
            }
            set { intCurveSize = value; }
        }
        /// <summary>
        /// Y轴的最大最小值 格式为 (50;1)
        /// </summary>
        public string M_YAxis
        {
            get { return m_YAxis; }
            set { m_YAxis = value; }
        }
        /// <summary>
        /// 是否绘制X轴的刻度线
        /// </summary>
        public bool IsXSliceLine
        {
            get { return isXSliceLine; }
            set { isXSliceLine = value; }
        }
        /// <summary>
        /// 是否绘制Y轴的刻度线
        /// </summary>
        public bool IsYSliceLine
        {
            get { return isYSliceLine; }
            set { isYSliceLine = value; }
        }

        /// <summary>
        /// 是否画X轴的刻度标志记号
        /// </summary>
        public bool IsXSliceSign
        {
            get { return isXSliceSign; }
            set { isXSliceSign = value; }
        }
        /// <summary>
        /// 是否画Y轴的刻度标志记号
        /// </summary>
        public bool IsYSliceSign
        {
            get { return isYSliceSign; }
            set { isYSliceSign = value; }
        }

        /// <summary>
        /// 仪器类型（1：常见血常规；2:血凝仪;.......）
        /// </summary>
        public string ApplianceType
        {
            get { return applianceType; }
            set { applianceType = value; }
        }

        /// <summary>
        /// 是否传递X轴的坐标值
        /// </summary>
        public bool IsXAxisValue
        {
            get { return isXAxisValue; }
            set { isXAxisValue = value; }
        }
        /// <summary>
        /// 设置X轴的刻度总数
        /// </summary>
        public float XAxisCount
        {
            get { return xAxisCount; }
            set { xAxisCount = value; }
        }
        /// <summary>
        /// 重新定义新的刻度值 说明：（原值；新值|原值；新值）示例数据："100;200|2200;300"
        /// </summary>
        public string NewSliceValues
        {
            get { return newSliceValues; }
            set { newSliceValues = value; }
        }



        #endregion
        /// <summary>
        /// 自动根据参数调整图像大小 (原始备份暂未使用改方法)
        /// </summary>
        public void Fit1()
        {
            //计算字体距离
            intFontSpace = FontSize + 5;
            //计算图像边距
            float fltSpace = Math.Min(Width / 6, Height / 6);
            XSpace = fltSpace;
            YSpace = fltSpace;
            //计算X轴刻度宽度
            XSlice = (Width - 2 * XSpace) / (Keys.Length - 1);
            //计算Y轴刻度宽度和Y轴刻度开始值
            float fltMinValue = 0;
            float fltMaxValue = 0;
            for (int i = 0; i < Values.Length; i++)
            {
                if (Values[i] < fltMinValue)
                {
                    fltMinValue = Values[i];
                }
                else if (Values[i] > fltMaxValue)
                {
                    fltMaxValue = Values[i];
                }
            }

            fltMaxValue = 1000;
            if (YSliceBegin > fltMinValue)
            {
                YSliceBegin = fltMinValue;
            }
            int intYSliceCount = (int)(fltMaxValue / YSliceValue);
            if (fltMaxValue % YSliceValue != 0)
            {
                intYSliceCount++;
            }
            YSlice = (Height - 2 * YSpace) / intYSliceCount;
        }

        /// <summary>
        /// 自动根据参数调整图像大小
        /// </summary>
        public void Fit()
        {
            //计算字体距离
            intFontSpace = FontSize + 5;
            //计算图像边距
            float fltSpace = Math.Min(Width / 6, Height / 6);
            string[] _M_YAxis = M_YAxis.Replace("|", ";").TrimStart(';').Split(';');
            // XSpace = fltSpace;
            //YSpace = fltSpace;
            //计算X轴刻度宽度
            XSlice = (Width - 2 * XSpace) / (_M_YAxis.Length - 1);
            //计算Y轴刻度宽度和Y轴刻度开始值
            float fltMinValue = 0;
            float fltMaxValue = 0;
            for (int i = 0; i < _M_YAxis.Length; i++)
            {
                if (Int32.Parse(_M_YAxis[i].ToString()) < fltMinValue)
                {
                    fltMinValue = Int32.Parse(_M_YAxis[i].ToString());
                }
                else if (Int32.Parse(_M_YAxis[i].ToString()) > fltMaxValue)
                {
                    fltMaxValue = Int32.Parse(_M_YAxis[i].ToString());
                }
            }
            if (YSliceBegin > fltMinValue)
            {
                YSliceBegin = fltMinValue;
            }
            int intYSliceCount = (int)(fltMaxValue / YSliceValue);
            if (fltMaxValue % YSliceValue != 0)
            {
                intYSliceCount++;
            }
            YSlice = (Height - 2 * YSpace) / intYSliceCount;
            XSlice = YSlice;
        }

        /// <summary>
        /// 生成图像并返回bmp图像对象
        /// </summary>
        /// <returns></returns>
        public Bitmap CreateImage(string KeyValues)
        {
            Fit();
            InitializeGraph();
            int intKeysCount = Keys.Length;
            int intValuesCount = Values.Length;
            if (intValuesCount % intKeysCount == 0)
            {
                //int intCurvesCount = intValuesCount / intKeysCount;
                //for (int i = 0; i < intCurvesCount; i++)
                //{
                //    float[] fltCurrentValues = new float[intKeysCount];
                //    for (int j = 0; j < intKeysCount; j++)
                //    {
                //        fltCurrentValues[j] = Values[i * intKeysCount + j];
                //    }
                //    DrawContent(ref objGraphics, fltCurrentValues, clrsCurveColors[i]);
                //}

                int intCurvesCount = intValuesCount / intKeysCount;
                for (int i = 0; i < intCurvesCount - 1; i++)
                {
                    //float[] fltCurrentValues = new float[intKeysCount];
                    //for (int j = 0; j < intKeysCount; j++)
                    //{
                    //    fltCurrentValues[j] = Values[i * intKeysCount + j];
                    //}

                    string[] _yAxisValues = KeyValues.ToString().Split('|');
                    if (IsXAxisValue == false) //判断是否传有X轴的值
                    {
                        for (int j = 0; j < _yAxisValues.Length; j++)
                        {
                            string strImageData = _yAxisValues[j];
                            float[] fltCurrentValues = new float[strImageData.TrimStart(';').Split(';').Length];
                            float[] fltCurrentXkeys = new float[strImageData.TrimStart(';').Split(';').Length];

                            for (int n = 0; n < fltCurrentValues.Length; n++)
                            {
                                fltCurrentValues[n] = float.Parse(Math.Abs(Convert.ToDecimal(strImageData.TrimStart(';').Split(';')[n])).ToString());
                            }
                            DrawContent(ref objGraphics, fltCurrentValues, clrsCurveColors[j]);
                        }
                    }
                    else if (IsXAxisValue == true)
                    {
                        for (int j = 0; j < _yAxisValues.Length; j++)
                        {
                            string strImageData = _yAxisValues[j];
                            float[] fltCurrentValues = new float[strImageData.TrimStart(';').Split(';').Length];
                            float[] fltCurrentXkeys = new float[strImageData.TrimStart(';').Split(';').Length];

                            for (int n = 0; n < fltCurrentValues.Length; n++)
                            {
                                fltCurrentValues[n] = float.Parse(Math.Abs(Convert.ToDecimal(strImageData.TrimStart(';').Split(';')[n].Split(',')[1])).ToString());
                                fltCurrentXkeys[n] = Int32.Parse(Math.Abs(Convert.ToDecimal(strImageData.TrimStart(';').Split(';')[n].Split(',')[0])).ToString());
                            }
                            DrawContent(ref objGraphics, fltCurrentValues, fltCurrentXkeys, clrsCurveColors[j]);
                        }
                    }
                    //DrawContent(ref objGraphics, fltCurrentValues, clrsCurveColors[i]);
                }
            }
            else
            {
                objGraphics.DrawString("发生错误，Values的长度必须是Keys的整数倍!", new Font("宋体", FontSize + 5), new SolidBrush(TextColor), new Point((int)XSpace, (int)(Height / 2)));
            }
            return objBitmap;
        }
        /// <summary>
        /// 初始化和填充图像区域，画出边框，初始标题
        /// </summary>
        private void InitializeGraph()
        {
            //根据给定的高度和宽度创建一个位图图像
            objBitmap = new Bitmap((int)Width, (int)Height);
            //从指定的 objBitmap 对象创建 objGraphics 对象 (即在objBitmap对象中画图)
            objGraphics = Graphics.FromImage(objBitmap);
            //根据给定颜色(LightGray)填充图像的矩形区域 (背景)
            //  objGraphics.DrawRectangle(new Pen(BorderColor, 1), 0, 0, Width - 1, Height - 1); //画边框
            objGraphics.FillRectangle(new SolidBrush(BgColor), 1, 1, Width - 2, Height - 2); //填充边框
            //画X轴,注意图像的原始X轴和Y轴计算是以左上角为原点，向右和向下计算的
            float fltX1 = XSpace;
            float fltY1 = Height - YSpace;
            float fltX2 = Width - XSpace + XSlice / 2;
            float fltY2 = fltY1;
            //objGraphics.DrawLine(new Pen(new SolidBrush(AxisColor), 1), fltX1, XSpace - 20, fltX2, XSpace - 20);  //绘制最上面一条线条 张祥明20130308添加
            objGraphics.DrawLine(new Pen(new SolidBrush(AxisColor), 1), fltX1, fltY1, fltX2 - 17, fltY2);
            //画Y轴
            fltX1 = XSpace;
            fltY1 = Height - YSpace;
            fltX2 = XSpace;
            fltY2 = YSpace - YSlice / 2;
            //objGraphics.DrawLine(new Pen(new SolidBrush(AxisColor), 1), (Width - XSpace + 20), fltY1, (Width - XSpace + 20), fltY2); //绘制最右边面一条线条 张祥明20130308添加
            objGraphics.DrawLine(new Pen(new SolidBrush(AxisColor), 1), fltX1, fltY1, fltX2, fltY2 + 17);
            //初始化轴线说明文字
            SetAxisText(ref objGraphics);

            //初始化Y轴上的刻度和文字
            SetYAxis(ref objGraphics);
            //初始化X轴上的刻度和文字
            SetXAxis(ref objGraphics);
            //初始化标题
            CreateTitle(ref objGraphics);
        }

        /// <summary>
        /// 初始化轴线说明文字
        /// </summary>
        /// <param name="objGraphics"></param>
        private void SetAxisText(ref Graphics objGraphics)
        {
            float fltX = Width - XSpace + XSlice / 2 - (XAxisText.Length - 1) * intFontSpace;
            float fltY = Height - YSpace - intFontSpace;
            //objGraphics.DrawString(XAxisText, new Font("宋体", FontSize), new SolidBrush(AxisTextColor), fltX, fltY);  //X轴文字原始向右对齐
            objGraphics.DrawString(XAxisText, new Font("宋体", FontSize), new SolidBrush(AxisTextColor), fltX / 2, fltY + YSpace - 5);
            fltX = XSpace + 5;
            fltY = YSpace - YSlice / 2 - intFontSpace;
            for (int i = 0; i < YAxisText.Length; i++)
            {
                objGraphics.DrawString(YAxisText[i].ToString(), new Font("宋体", FontSize), new SolidBrush(AxisTextColor), fltX - 40, fltY); //Y轴文字
                //fltY += intFontSpace; //字体上下距离
                fltX += intFontSpace;  //字体左右距离
            }
        }
        /// <summary>
        /// 初始化X轴上的刻度和文字
        /// </summary>
        /// <param name="objGraphics"></param>
        private void SetXAxis(ref Graphics objGraphics)
        {
            XSlice = (Width - 2 * XSpace) / XAxisCount;
            //计算Y轴刻度宽度和Y轴刻度开始值
            float fltX1 = XSpace;
            float fltY1 = Height - YSpace;
            float fltX2 = XSpace;
            float fltY2 = Height - YSpace;
            int iCount = 0;
            int iSliceCount = 1;
            float Scale = 0;
            float iWidth = ((Width - 2 * XSpace) / XSlice) * 50; //将要画刻度的长度分段，并乘以50，以10为单位画刻度线。
            //float fltSliceHeight = XSlice / 10; //刻度线的高度  如果为了刻度产地与Y轴一致  可以用 Yslice
            float fltSliceHeight = YSlice / 10;

            float fltSliceWidth = YSlice / 10; //刻度线的宽度-------------------------
            string strSliceText = string.Empty; //---------------------------
            objGraphics.TranslateTransform(fltX1, fltY1); //平移图像(原点)
            objGraphics.RotateTransform(XRotateAngle, MatrixOrder.Prepend); //旋转图像

            float XFisrtV = 1;
            if (NewSliceValues != "") //根据图片类型 判断是否重新定义刻度值 //血流仪
            {
                XFisrtV = ConvertXSliceValue(XFisrtV);
            }
            objGraphics.DrawString(XFisrtV.ToString(), new Font("楷体", FontSize), new SolidBrush(SliceTextColor), -6, 8); //画出默认刻度0
            objGraphics.ResetTransform(); //重置图像
            for (int i = 0; i <= iWidth; i += 10) //以10为单位
            {
                Scale = i * XSlice / 50;//即(i / 10) * (XSlice / 5)，将每个刻度分五部分画，但因为i以10为单位，得除以10
                if (iCount == 5)
                {
                    objGraphics.DrawLine(new Pen(new SolidBrush(AxisColor)), fltX1 + Scale, fltY1 + fltSliceHeight * 1.5f, fltX2 + Scale, fltY2 - fltSliceHeight * 1.5f);
                    //画网格虚线
                    if (IsXSliceLine)
                    {
                        Pen penDashed = new Pen(new SolidBrush(AxisColor));
                        penDashed.DashStyle = DashStyle.Dash;
                        objGraphics.DrawLine(penDashed, fltX1 + Scale, fltY1, fltX2 + Scale, (YSpace - YSlice / 2) + 17); //为了缩短距离 -17 张祥明2013-03-13
                    }

                    objGraphics.TranslateTransform(fltX1 + Scale, fltY1);
                    objGraphics.RotateTransform(XRotateAngle, MatrixOrder.Prepend);
                    float xValue = XSliceValue * iSliceCount + YSliceBegin;// 默认刻度值
                    if (NewSliceValues != "") //根据图片类型 判断是否重新定义刻度值 //血流仪
                    {
                        xValue = ConvertXSliceValue(xValue);
                    }
                    if (xValue != 0)
                        objGraphics.DrawString(Convert.ToString(xValue), new Font("楷体", FontSize),
                        new SolidBrush(SliceTextColor), -10, 8); //默认为 0，0  （-5,8 为了将刻度数字下调）
                    objGraphics.ResetTransform();

                    iCount = 0;
                    iSliceCount++;
                    if (fltX1 + Scale > Width - XSpace)
                    {
                        break;
                    }
                }
                else
                {
                    if (isXSliceSign)
                        objGraphics.DrawLine(new Pen(new SolidBrush(SliceColor)), fltX1 + Scale, fltY1 + fltSliceHeight, fltX2 + Scale, fltY2 - fltSliceHeight);
                }
                iCount++;
            }
            // XSlice = YSpace;
        }
        /// <summary>
        /// 初始化Y轴上的刻度和文字
        /// </summary>
        /// <param name="objGraphics"></param>
        private void SetYAxis(ref Graphics objGraphics)
        {
            if (!isYSliceSign) return;
            float fltX1 = XSpace;
            float fltY1 = Height - YSpace;
            float fltX2 = XSpace;
            float fltY2 = Height - YSpace;
            int iCount = 0;
            float Scale = 0;
            int iSliceCount = 1;
            float iWidth = ((Height - 2 * XSpace) / XSlice) * 50;  //将要画刻度的长度分段，并乘以50，以10为单位画刻度线。
            float fltSliceWidth = YSlice / 10; //刻度线的宽度
            string strSliceText = string.Empty;
            objGraphics.TranslateTransform(XSpace - intFontSpace * YSliceBegin.ToString().Length, Height - YSpace); //平移图像(原点)
            objGraphics.RotateTransform(YRotateAngle, MatrixOrder.Prepend); //旋转图像
            objGraphics.DrawString(YSliceBegin.ToString(), new Font("楷体", FontSize), new SolidBrush(SliceTextColor), -6, -6);
            objGraphics.ResetTransform(); //重置图像

            for (int i = 0; i < iWidth + 10; i += 10)
            {
                Scale = i * YSlice / 50; //即(i / 10) * (YSlice / 5)，将每个刻度分五部分画，但因为i以10为单位，得除以10
                if (iCount == 5)
                {
                    objGraphics.DrawLine(new Pen(new SolidBrush(AxisColor)), fltX1 - fltSliceWidth * 1.5f, fltY1 - Scale, fltX2 + fltSliceWidth * 1.5f, fltY2 - Scale);
                    //画网格虚线
                    if (IsYSliceLine)
                    {
                        Pen penDashed = new Pen(new SolidBrush(AxisColor));
                        penDashed.DashStyle = DashStyle.Dash;
                        objGraphics.DrawLine(penDashed, XSpace, fltY1 - Scale, (Width - XSpace + XSlice / 2) - 17, fltY2 - Scale); //为了缩短距离 -17 张祥明2013-03-13
                    }
                    //这里显示Y轴刻度
                    strSliceText = Convert.ToString(YSliceValue * iSliceCount + YSliceBegin);
                    objGraphics.TranslateTransform(XSpace - intFontSize * strSliceText.Length, fltY1 - Scale);
                    //平移图像(原点)
                    objGraphics.RotateTransform(YRotateAngle, MatrixOrder.Prepend); //旋转图像
                    objGraphics.DrawString(strSliceText, new Font("楷体", FontSize), new SolidBrush(SliceTextColor), -6, -6);//默认为 0，0  （-5,8 为了将刻度数字下调）
                    objGraphics.ResetTransform(); //重置图像
                    iCount = 0;
                    iSliceCount++;
                    if (fltX1 + Scale > Width - XSpace)
                    {
                        break;
                    }
                }
                else
                {
                    objGraphics.DrawLine(new Pen(new SolidBrush(SliceColor)), fltX1 - fltSliceWidth, fltY1 - Scale, fltX2 + fltSliceWidth, fltY2 - Scale);
                }

                iCount++;
            }
        }
        /// <summary>
        /// 画曲线
        /// </summary>
        /// <param name="objGraphics"></param>
        private void DrawContent(ref Graphics objGraphics, float[] fltCurrentValues, Color clrCurrentColor)
        {
            Pen CurvePen = new Pen(clrCurrentColor, CurveSize);
            PointF[] CurvePointF = new PointF[fltCurrentValues.Length];
            float keys = 0;
            float values = 0;
            for (int i = 0; i < fltCurrentValues.Length; i++)
            {
                keys = i + XSpace;//XSlice * i + XSpace;
                values = (Height - YSpace) + YSliceBegin - YSlice * (fltCurrentValues[i] / YSliceValue);
                CurvePointF[i] = new PointF(keys, values);
            }
            objGraphics.DrawCurve(CurvePen, CurvePointF, Tension);
        }

        /// <summary>
        /// 根据坐标值绘制图像内容
        /// </summary>
        /// <param name="objGraphics">objGraphics对象</param>
        /// <param name="fltCurrentValues">y轴坐标点内容</param>
        /// <param name="fltXkey">X轴坐标点内容</param> 
        /// <param name="clrCurrentColor"></param>
        private void DrawContent(ref Graphics objGraphics, float[] fltCurrentValues, float[] fltXkey, Color clrCurrentColor)
        {
            Pen CurvePen = new Pen(clrCurrentColor, CurveSize);
            PointF[] CurvePointF = new PointF[fltCurrentValues.Length];
            float keys = 0;
            float values = 0;
            for (int i = 0; i < fltCurrentValues.Length; i++)
            {
                if (ApplianceType == "1")
                {
                    keys = fltXkey[i] + XSpace;//XSlice * i + XSpace;
                }
                else if (ApplianceType == "2")  //血流仪器
                {
                    if (i > 0)
                    {
                        if (i <= 3)
                            if (i != 3)
                                keys = fltXkey[i] + XSpace + float.Parse((i * 110).ToString());
                            else
                                keys = fltXkey[i] + XSpace + float.Parse((i * 150).ToString());
                        else
                            keys = fltXkey[i] + XSpace + float.Parse("430");//XSlice * i + XSpace;
                    }
                    else
                        keys = fltXkey[i] + XSpace;//XSlice * i + XSpace;
                }
                values = (Height - YSpace) + YSliceBegin - YSlice * (fltCurrentValues[i] / YSliceValue);
                CurvePointF[i] = new PointF(keys, values);
            }
            objGraphics.DrawCurve(CurvePen, CurvePointF, Tension);
        }

        /// <summary>
        /// 初始化标题
        /// </summary>
        /// <param name="objGraphics"></param>
        private void CreateTitle(ref Graphics objGraphics)
        {
            objGraphics.DrawString(Title, new Font("宋体", FontSize), new SolidBrush(TextColor), new Point((int)(Width - XSpace) / 2 - intFontSize * Title.Length, (int)(YSpace - YSlice / 2 - intFontSpace)));
        }

        #region 扩展方法

        /// <summary>
        /// 针对血流仪 刻度转换
        /// </summary>
        /// <param name="fv">原始刻度值</param>
        /// <returns></returns>
        public float ConvertXSliceValue(float fv)
        {
            float XSValue = 0;
            Dictionary<float, float> dict = new Dictionary<float, float>();

            string[] _NewSlice = NewSliceValues.Split('|');
            for (int i = 0; i < _NewSlice.Length; i++)
            {
                string[] v = _NewSlice[i].Split(';');
                dict.Add(float.Parse(v[0].ToString()), float.Parse(v[1].ToString()));
            }
            //dict.Add(20, 3);
            //dict.Add(40, 10);
            //dict.Add(50, 30);
            //dict.Add(100, 100);
            //dict.Add(160, 200);

            try
            {
                XSValue = dict[fv];
            }
            catch { };
            return XSValue;
        }

        /// <summary>       
        /// /// 锐化        
        /// </summary>        
        /// <param name="b">原始Bitmap</param>        
        /// <param name="val">锐化程度。取值[0,1]。值越大锐化程度越高</param>        
        /// <returns>锐化后的图像</returns>        
        public static Bitmap KiSharpen(Bitmap b, float val)
        {
            if (b == null) { return null; }
            int w = b.Width; int h = b.Height;
            try
            {
                Bitmap bmpRtn = new Bitmap(w, h, PixelFormat.Format24bppRgb);
                BitmapData srcData = b.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                BitmapData dstData = bmpRtn.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
                unsafe
                {
                    byte* pIn = (byte*)srcData.Scan0.ToPointer();
                    byte* pOut = (byte*)dstData.Scan0.ToPointer(); int stride = srcData.Stride;
                    byte* p; for (int y = 0; y < h; y++)
                    {
                        for (int x = 0; x < w; x++)
                        {
                            //取周围9点的值。位于边缘上的点不做改变。               
                            if (x == 0 || x == w - 1 || y == 0 || y == h - 1)
                            {
                                //不做                          
                                pOut[0] = pIn[0];
                                pOut[1] = pIn[1]; pOut[2] = pIn[2];
                            }
                            else
                            {
                                int r1, r2, r3, r4, r5, r6, r7, r8, r0;
                                int g1, g2, g3, g4, g5, g6, g7, g8, g0;
                                int b1, b2, b3, b4, b5, b6, b7, b8, b0;
                                float vR, vG, vB;                                 //左上          
                                p = pIn - stride - 3; r1 = p[2];
                                g1 = p[1]; b1 = p[0];
                                //正上                            
                                p = pIn - stride;
                                r2 = p[2];
                                g2 = p[1];
                                b2 = p[0];
                                //右上                    
                                p = pIn - stride + 3;
                                r3 = p[2];
                                g3 = p[1]; b3 = p[0];
                                //左侧                     
                                p = pIn - 3;
                                r4 = p[2]; g4 = p[1];
                                b4 = p[0];
                                //右侧                 
                                p = pIn + 3;
                                r5 = p[2];
                                g5 = p[1];
                                b5 = p[0];
                                //右下                         
                                p = pIn + stride - 3;
                                r6 = p[2];
                                g6 = p[1];
                                b6 = p[0];
                                //正下             
                                p = pIn + stride;
                                r7 = p[2];
                                g7 = p[1];
                                b7 = p[0];
                                //右下                          
                                p = pIn + stride + 3;
                                r8 = p[2];
                                g8 = p[1];
                                b8 = p[0];
                                //自己                        
                                p = pIn;
                                r0 = p[2];
                                g0 = p[1];
                                b0 = p[0];
                                vR = (float)r0 - (float)(r1 + r2 + r3 + r4 + r5 + r6 + r7 + r8) / 8;
                                vG = (float)g0 - (float)(g1 + g2 + g3 + g4 + g5 + g6 + g7 + g8) / 8;
                                vB = (float)b0 - (float)(b1 + b2 + b3 + b4 + b5 + b6 + b7 + b8) / 8;
                                vR = r0 + vR * val; vG = g0 + vG * val;
                                vB = b0 + vB * val;
                                if (vR > 0)
                                {
                                    vR = Math.Min(255, vR);
                                }
                                else
                                {
                                    vR = Math.Max(0, vR);
                                }
                                if (vG > 0)
                                {
                                    vG = Math.Min(255, vG);
                                }
                                else
                                {
                                    vG = Math.Max(0, vG);
                                }
                                if (vB > 0)
                                {
                                    vB = Math.Min(255, vB);
                                }
                                else
                                {
                                    vB = Math.Max(0, vB);
                                }
                                pOut[0] = (byte)vB;
                                pOut[1] = (byte)vG; pOut[2] = (byte)vR;
                            } pIn += 3; pOut += 3;
                        }
                        // end of x              
                        pIn += srcData.Stride - w * 3;
                        pOut += srcData.Stride - w * 3;
                    } // end of y             
                }
                b.UnlockBits(srcData);
                bmpRtn.UnlockBits(dstData);
                return bmpRtn;
            }
            catch
            {
                return null;
            }
        } // end of KiSharpen

        /// <summary>
        /// 图片缩放
        /// </summary>
        /// <param name="imageFrom">Bitmap 图像</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="quality">默认可以填0</param>
        /// <returns></returns>
        public static Bitmap KiResizeImage(Bitmap imageFrom, int width, int height, int quality)
        {
            // 源图宽度及高度           
            int imageFromWidth = imageFrom.Width;
            int imageFromHeight = imageFrom.Height;
            // 生成的缩略图实际宽度及高度            
            if (width >= imageFromWidth && height >= imageFromHeight)
            {
                return imageFrom;
            }
            else
            {
                // 生成的缩略图在上述"画布"上的位置             
                int X = 0; int Y = 0;
                decimal wpercent = (decimal)width / imageFromWidth;
                decimal hpercent = (decimal)height / imageFromHeight;
                if (wpercent > hpercent)
                {
                    width = (int)(imageFromWidth * hpercent);
                }
                else if (wpercent < hpercent)
                {
                    height = (int)(imageFromHeight * wpercent);
                }
                // 创建画布             
                Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                bmp.SetResolution(imageFrom.HorizontalResolution, imageFrom.VerticalResolution);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    // 用白色清空               
                    g.Clear(Color.White);
                    // 指定高质量的双三次插值法。执行预筛选以确保高质量的收缩。此模式可产生质量最高的转换图像。  
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    // 指定高质量、低速度呈现。              
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    // 在指定位置并且按指定大小绘制指定的 Image 的指定部分。      
                    g.DrawImage(imageFrom, new Rectangle(X, Y, width, height), new Rectangle(0, 0, imageFromWidth, imageFromHeight), GraphicsUnit.Pixel);
                    return bmp;
                }
            }
        }
        #endregion
    }
}
