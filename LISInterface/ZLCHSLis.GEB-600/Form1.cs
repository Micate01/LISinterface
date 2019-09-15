using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ZLCHSLis.Urine.GEB600;
namespace ZLCHSLis.Urine.GEB600
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string strOutText = @"
#0005     2011-06-21
            09:26:53
*LEU  +-  15  leu/uL
*NIT  +             
 URO      Normal    
 BIL  -             
*Vc   +-  0.6 mmol/L
 PRO  -   0      g/L
*BLD  2+  80  ery/uL
 pH       5.0       
 SG      >1.030     
 GLU  -   0   mmol/L
 KET  -   0   mmol/L
                    

                    
";

            string result="",strre="",strcom="";
            string msg = strOutText.Substring(strOutText.IndexOf(this.textBox1.Text)+4, Convert.ToInt32(this.textBox2.Text));
            MessageBox.Show(msg);
            MessageBox.Show("检验标本号:" + strOutText.Substring(4, 4) + "检验时间：" + DateTime.Parse(strOutText.Substring(12,11)+" "+strOutText.Substring(36,9)).ToString("yyyy-MM-dd HH:mm:ss"));
           // ResolveResult r = new ResolveResult();
            //ResolveResult.ParseResult(strOutText, ref result, ref strcom, ref strcom);
            
            //MessageBox.Show(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }
    }
}
