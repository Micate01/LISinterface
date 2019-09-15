namespace ZLCHSLisComm
{
    partial class SelDevice
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelDevice));
            this.label1 = new System.Windows.Forms.Label();
            this.lv_device = new System.Windows.Forms.ListView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.but_can = new System.Windows.Forms.Button();
            this.but_OK = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Padding = new System.Windows.Forms.Padding(2, 10, 0, 5);
            this.label1.Size = new System.Drawing.Size(169, 27);
            this.label1.TabIndex = 1;
            this.label1.Text = "选择需添加的仪器确定继续...";
            // 
            // lv_device
            // 
            this.lv_device.CausesValidation = false;
            this.lv_device.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lv_device.FullRowSelect = true;
            this.lv_device.Location = new System.Drawing.Point(0, 27);
            this.lv_device.Name = "lv_device";
            this.lv_device.Size = new System.Drawing.Size(270, 161);
            this.lv_device.SmallImageList = this.imageList1;
            this.lv_device.StateImageList = this.imageList1;
            this.lv_device.TabIndex = 3;
            this.lv_device.UseCompatibleStateImageBehavior = false;
            this.lv_device.View = System.Windows.Forms.View.List;
            this.lv_device.DoubleClick += new System.EventHandler(this.but_OK_Click_1);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "package.png");
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.but_can);
            this.panel1.Controls.Add(this.but_OK);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 151);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(270, 37);
            this.panel1.TabIndex = 4;
            this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
            // 
            // but_can
            // 
            this.but_can.Location = new System.Drawing.Point(203, 8);
            this.but_can.Name = "but_can";
            this.but_can.Size = new System.Drawing.Size(58, 26);
            this.but_can.TabIndex = 6;
            this.but_can.Text = "取消(&C)";
            this.but_can.UseVisualStyleBackColor = true;
            this.but_can.Click += new System.EventHandler(this.but_can_Click_1);
            // 
            // but_OK
            // 
            this.but_OK.Location = new System.Drawing.Point(128, 8);
            this.but_OK.Name = "but_OK";
            this.but_OK.Size = new System.Drawing.Size(58, 26);
            this.but_OK.TabIndex = 5;
            this.but_OK.Text = "确定(&O)";
            this.but_OK.UseVisualStyleBackColor = true;
            this.but_OK.Click += new System.EventHandler(this.but_OK_Click_1);
            // 
            // SelDevice
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(270, 188);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.lv_device);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SelDevice";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "选择仪器";
            this.Load += new System.EventHandler(this.SelDevice_Load);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListView lv_device;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button but_can;
        private System.Windows.Forms.Button but_OK;
        private System.Windows.Forms.ImageList imageList1;
    }
}