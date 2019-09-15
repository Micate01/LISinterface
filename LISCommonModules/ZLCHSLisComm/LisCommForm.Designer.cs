namespace ZLCHSLisComm
{
    partial class LisCommForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LisCommForm));
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.显示ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.退出ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.开始ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.停止ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lv_Device = new System.Windows.Forms.ListView();
            this.id = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.name = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.type = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.增加仪器ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.删除仪器ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ilst_device = new System.Windows.Forms.ImageList(this.components);
            this.label4 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.linkLabel6 = new System.Windows.Forms.LinkLabel();
            this.lab_drwView = new System.Windows.Forms.LinkLabel();
            this.linkLabel4 = new System.Windows.Forms.LinkLabel();
            this.lab_resultView = new System.Windows.Forms.LinkLabel();
            this.linkLabel2 = new System.Windows.Forms.LinkLabel();
            this.lab_logView = new System.Windows.Forms.LinkLabel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.but_begin = new System.Windows.Forms.Button();
            this.but_end = new System.Windows.Forms.Button();
            this.but_exit = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.contextMenuStrip1.SuspendLayout();
            this.contextMenuStrip2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "解析程序";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseDoubleClick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.显示ToolStripMenuItem,
            this.退出ToolStripMenuItem,
            this.开始ToolStripMenuItem,
            this.停止ToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(101, 92);
            // 
            // 显示ToolStripMenuItem
            // 
            this.显示ToolStripMenuItem.Name = "显示ToolStripMenuItem";
            this.显示ToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.显示ToolStripMenuItem.Text = "显示";
            this.显示ToolStripMenuItem.Click += new System.EventHandler(this.显示ToolStripMenuItem_Click);
            // 
            // 退出ToolStripMenuItem
            // 
            this.退出ToolStripMenuItem.Name = "退出ToolStripMenuItem";
            this.退出ToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.退出ToolStripMenuItem.Text = "退出";
            this.退出ToolStripMenuItem.Click += new System.EventHandler(this.退出ToolStripMenuItem_Click);
            // 
            // 开始ToolStripMenuItem
            // 
            this.开始ToolStripMenuItem.Name = "开始ToolStripMenuItem";
            this.开始ToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.开始ToolStripMenuItem.Text = "开始";
            this.开始ToolStripMenuItem.Click += new System.EventHandler(this.but_begin_Click);
            // 
            // 停止ToolStripMenuItem
            // 
            this.停止ToolStripMenuItem.Name = "停止ToolStripMenuItem";
            this.停止ToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.停止ToolStripMenuItem.Text = "停止";
            this.停止ToolStripMenuItem.Click += new System.EventHandler(this.but_end_Click);
            // 
            // lv_Device
            // 
            this.lv_Device.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.id,
            this.name,
            this.type});
            this.lv_Device.ContextMenuStrip = this.contextMenuStrip2;
            this.lv_Device.LargeImageList = this.ilst_device;
            this.lv_Device.Location = new System.Drawing.Point(4, 51);
            this.lv_Device.Name = "lv_Device";
            this.lv_Device.Size = new System.Drawing.Size(103, 171);
            this.lv_Device.TabIndex = 4;
            this.lv_Device.UseCompatibleStateImageBehavior = false;
            // 
            // contextMenuStrip2
            // 
            this.contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.增加仪器ToolStripMenuItem,
            this.删除仪器ToolStripMenuItem});
            this.contextMenuStrip2.Name = "contextMenuStrip2";
            this.contextMenuStrip2.Size = new System.Drawing.Size(125, 48);
            // 
            // 增加仪器ToolStripMenuItem
            // 
            this.增加仪器ToolStripMenuItem.Name = "增加仪器ToolStripMenuItem";
            this.增加仪器ToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.增加仪器ToolStripMenuItem.Text = "增加仪器";
            this.增加仪器ToolStripMenuItem.Click += new System.EventHandler(this.AddToolStripMenuItem_Click);
            // 
            // 删除仪器ToolStripMenuItem
            // 
            this.删除仪器ToolStripMenuItem.Name = "删除仪器ToolStripMenuItem";
            this.删除仪器ToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.删除仪器ToolStripMenuItem.Text = "移除仪器";
            this.删除仪器ToolStripMenuItem.Click += new System.EventHandler(this.delToolStripMenuItem_Click);
            // 
            // ilst_device
            // 
            this.ilst_device.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilst_device.ImageStream")));
            this.ilst_device.TransparentColor = System.Drawing.Color.Transparent;
            this.ilst_device.Images.SetKeyName(0, "package_view.png");
            this.ilst_device.Images.SetKeyName(1, "package.png");
            // 
            // label4
            // 
            this.label4.AutoEllipsis = true;
            this.label4.Location = new System.Drawing.Point(0, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(365, 31);
            this.label4.TabIndex = 1;
            this.label4.Text = "    说明：解析程序提供接收及解析仪器结果，如果该程序未启动，将不能接收及解析结果信息；增删仪器需停止后重新启动。\r\n";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.linkLabel6);
            this.panel1.Controls.Add(this.lab_drwView);
            this.panel1.Controls.Add(this.linkLabel4);
            this.panel1.Controls.Add(this.lab_resultView);
            this.panel1.Controls.Add(this.linkLabel2);
            this.panel1.Controls.Add(this.lab_logView);
            this.panel1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.panel1.Location = new System.Drawing.Point(6, 20);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(237, 96);
            this.panel1.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 62);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 1;
            this.label3.Text = "原始数据";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 37);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "解析结果";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "通讯日志";
            // 
            // linkLabel6
            // 
            this.linkLabel6.AutoSize = true;
            this.linkLabel6.Location = new System.Drawing.Point(165, 62);
            this.linkLabel6.Name = "linkLabel6";
            this.linkLabel6.Size = new System.Drawing.Size(65, 12);
            this.linkLabel6.TabIndex = 0;
            this.linkLabel6.TabStop = true;
            this.linkLabel6.Text = "打开文件夹";
            this.linkLabel6.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel6_LinkClicked);
            // 
            // lab_drwView
            // 
            this.lab_drwView.AutoSize = true;
            this.lab_drwView.Location = new System.Drawing.Point(123, 62);
            this.lab_drwView.Name = "lab_drwView";
            this.lab_drwView.Size = new System.Drawing.Size(29, 12);
            this.lab_drwView.TabIndex = 0;
            this.lab_drwView.TabStop = true;
            this.lab_drwView.Tag = "查看今日接收的原始数据文件";
            this.lab_drwView.Text = "查看";
            this.lab_drwView.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lab_drwView_LinkClicked_1);
            // 
            // linkLabel4
            // 
            this.linkLabel4.AutoSize = true;
            this.linkLabel4.Location = new System.Drawing.Point(165, 37);
            this.linkLabel4.Name = "linkLabel4";
            this.linkLabel4.Size = new System.Drawing.Size(65, 12);
            this.linkLabel4.TabIndex = 0;
            this.linkLabel4.TabStop = true;
            this.linkLabel4.Text = "打开文件夹";
            this.linkLabel4.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel4_LinkClicked);
            // 
            // lab_resultView
            // 
            this.lab_resultView.AutoSize = true;
            this.lab_resultView.Location = new System.Drawing.Point(123, 37);
            this.lab_resultView.Name = "lab_resultView";
            this.lab_resultView.Size = new System.Drawing.Size(29, 12);
            this.lab_resultView.TabIndex = 0;
            this.lab_resultView.TabStop = true;
            this.lab_resultView.Tag = "查看今日解析结果文件";
            this.lab_resultView.Text = "查看";
            this.lab_resultView.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lab_resultView_LinkClicked_1);
            // 
            // linkLabel2
            // 
            this.linkLabel2.AutoSize = true;
            this.linkLabel2.Location = new System.Drawing.Point(165, 11);
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.Size = new System.Drawing.Size(65, 12);
            this.linkLabel2.TabIndex = 0;
            this.linkLabel2.TabStop = true;
            this.linkLabel2.Text = "打开文件夹";
            this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
            // 
            // lab_logView
            // 
            this.lab_logView.AutoSize = true;
            this.lab_logView.Location = new System.Drawing.Point(123, 11);
            this.lab_logView.Name = "lab_logView";
            this.lab_logView.Size = new System.Drawing.Size(29, 12);
            this.lab_logView.TabIndex = 0;
            this.lab_logView.TabStop = true;
            this.lab_logView.Tag = "";
            this.lab_logView.Text = "查看";
            this.lab_logView.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lab_logView_LinkClicked_1);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.panel1);
            this.groupBox1.Controls.Add(this.but_begin);
            this.groupBox1.Controls.Add(this.but_end);
            this.groupBox1.Controls.Add(this.but_exit);
            this.groupBox1.Location = new System.Drawing.Point(111, 43);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(250, 180);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            // 
            // but_begin
            // 
            this.but_begin.Image = global::ZLCHSLisComm.Properties.Resources.breakpoint_run;
            this.but_begin.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.but_begin.Location = new System.Drawing.Point(6, 130);
            this.but_begin.Name = "but_begin";
            this.but_begin.Size = new System.Drawing.Size(69, 35);
            this.but_begin.TabIndex = 2;
            this.but_begin.Text = "启动";
            this.but_begin.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.but_begin.UseVisualStyleBackColor = true;
            this.but_begin.Click += new System.EventHandler(this.but_begin_Click);
            // 
            // but_end
            // 
            this.but_end.Image = global::ZLCHSLisComm.Properties.Resources.breakpoint_down;
            this.but_end.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.but_end.Location = new System.Drawing.Point(81, 130);
            this.but_end.Name = "but_end";
            this.but_end.Size = new System.Drawing.Size(69, 35);
            this.but_end.TabIndex = 2;
            this.but_end.Text = "停止";
            this.but_end.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.but_end.UseVisualStyleBackColor = true;
            this.but_end.Click += new System.EventHandler(this.but_end_Click);
            // 
            // but_exit
            // 
            this.but_exit.Image = global::ZLCHSLisComm.Properties.Resources.breakpoints_delete;
            this.but_exit.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.but_exit.Location = new System.Drawing.Point(156, 131);
            this.but_exit.Name = "but_exit";
            this.but_exit.Size = new System.Drawing.Size(69, 35);
            this.but_exit.TabIndex = 2;
            this.but_exit.Text = "退出";
            this.but_exit.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.but_exit.UseVisualStyleBackColor = true;
            this.but_exit.Click += new System.EventHandler(this.but_set_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(223, 231);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 7;
            this.button1.Text = "设置机构";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_2);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(223, 254);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 8;
            this.button2.Text = "设置编码";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 234);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 9;
            this.label5.Text = "当前机构";
            // 
            // textBox1
            // 
            this.textBox1.Enabled = false;
            this.textBox1.Location = new System.Drawing.Point(71, 231);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(136, 21);
            this.textBox1.TabIndex = 10;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 259);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 12);
            this.label6.TabIndex = 11;
            this.label6.Text = "当前编码";
            // 
            // textBox2
            // 
            this.textBox2.Enabled = false;
            this.textBox2.Location = new System.Drawing.Point(71, 256);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(136, 21);
            this.textBox2.TabIndex = 12;
            // 
            // LisCommForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(383, 289);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lv_Device);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "LisCommForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "LIS通讯程序";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.MaximumSizeChanged += new System.EventHandler(this.LisCommForm_MaximumSizeChanged);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LisComm_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.contextMenuStrip2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 显示ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 退出ToolStripMenuItem;
        private System.Windows.Forms.ListView lv_Device;
        private System.Windows.Forms.ColumnHeader id;
        private System.Windows.Forms.ColumnHeader name;
        private System.Windows.Forms.ColumnHeader type;
        private System.Windows.Forms.ImageList ilst_device;
        private System.Windows.Forms.Button but_begin;
        private System.Windows.Forms.Button but_end;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
        private System.Windows.Forms.ToolStripMenuItem 增加仪器ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 删除仪器ToolStripMenuItem;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button but_exit;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.LinkLabel linkLabel6;
        private System.Windows.Forms.LinkLabel lab_drwView;
        private System.Windows.Forms.LinkLabel linkLabel4;
        private System.Windows.Forms.LinkLabel lab_resultView;
        private System.Windows.Forms.LinkLabel linkLabel2;
        private System.Windows.Forms.LinkLabel lab_logView;
        private System.Windows.Forms.ToolStripMenuItem 开始ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 停止ToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBox2;
    }
}

