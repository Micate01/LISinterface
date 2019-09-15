using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ZLCHSLisComm
{
    public partial class SelDevice : Form
    {   
        public string strSelDevice;              //已选择的仪器
        public string newDevice;
        DataTable tmpTable =new DataTable();
        public SelDevice()
        {
            InitializeComponent();
            
        }

        private void SelDevice_Load(object sender, EventArgs e)
        {
            string DeviceId;
            IniFile ConfigIni = new IniFile("SOLVESET.INI");
            ListViewItem Litem =new  ListViewItem();
            DeviceId = ConfigIni.IniReadValue("EQUIPMENT", "Agencies");
            tmpTable = OracleHelper.GetDataTable("Select ID, 名称 From 检验仪器 Where 通讯类型 in(1,2,3,4) and 机构id='"+DeviceId+"' and ID Not In (Select Column_Value From Table(f_Split_String('"+strSelDevice+"')))");
            foreach (DataRow i in tmpTable.Rows)
            {                
                Litem = lv_device.Items.Add(i["名称"].ToString(),0);                
                Litem.Tag = i["id"].ToString();
            }
        }



        private void but_can_Click(object sender, EventArgs e)
        {
            newDevice = "";
            this.Close();
        }

        private void but_OK_Click_1(object sender, EventArgs e)
        {
            newDevice = "";
            if (lv_device.SelectedItems.Count > 0)
                foreach (ListViewItem item in lv_device.SelectedItems)
                {
                    newDevice = newDevice + "," + item.Tag;
                }        
            else
                newDevice = "";
            if (newDevice != "") newDevice = newDevice.Substring(1);
            this.Close();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void but_can_Click_1(object sender, EventArgs e)
        {
            newDevice = "";
            this.Close();
        }
    }
}
