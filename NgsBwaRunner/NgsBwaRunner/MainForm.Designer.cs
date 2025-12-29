namespace NgsBwaRunner
{
    partial class MainForm
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
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            groupBox1 = new GroupBox();
            tableLayoutPanel1 = new TableLayoutPanel();
            label1 = new Label();
            txtHost = new TextBox();
            label2 = new Label();
            numPort = new NumericUpDown();
            label3 = new Label();
            txtUser = new TextBox();
            label4 = new Label();
            txtPass = new TextBox();
            btnConnect = new Button();
            lblStatus = new Label();
            groupBox2 = new GroupBox();
            tableLayoutPanel2 = new TableLayoutPanel();
            label5 = new Label();
            txtRefPath = new TextBox();
            btnBrowseRef = new Button();
            label6 = new Label();
            txtFastqPath = new TextBox();
            btnBrowseFastq = new Button();
            label7 = new Label();
            txtOutputPath = new TextBox();
            btnBrowseOutput = new Button();
            groupBox3 = new GroupBox();
            tableLayoutPanel3 = new TableLayoutPanel();
            btnUploadRef = new Button();
            btnBuildIndex = new Button();
            btnBwaAlign = new Button();
            btnProcessSam = new Button();
            btnVariantDetection = new Button();
            btnTestVariantCmd = new Button();
            btnViewVcfInfo = new Button();
            btnGenerateVisualization = new Button();
            btnManageModules = new Button();
            btnCleanRemoteTemp = new Button();
            groupBox4 = new GroupBox();
            tableLayoutPanel4 = new TableLayoutPanel();
            label8 = new Label();
            lblToolsStatus = new Label();
            progressBar1 = new ProgressBar();
            lblProgress = new Label();
            tabPage2 = new TabPage();
            groupBox5 = new GroupBox();
            txtLogViewer = new TextBox();
            panel1 = new Panel();
            btnViewLogs = new Button();
            btnClearLogs = new Button();
            tabPage3 = new TabPage();
            groupBox6 = new GroupBox();
            tableLayoutPanel5 = new TableLayoutPanel();
            label9 = new Label();
            label10 = new Label();
            cbHighResolution = new CheckBox();
            cbCombinePDF = new CheckBox();
            btnRunAllVisualizations = new Button();
            btnConfigureModules = new Button();
            label11 = new Label();
            lblLoadedModules = new Label();
            lstModuleList = new ListBox();
            btnRefreshModules = new Button();
            statusStrip = new StatusStrip();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            lblStatusMessage = new ToolStripStatusLabel();
            toolStripStatusLabel2 = new ToolStripStatusLabel();
            toolStripStatusLabel3 = new ToolStripStatusLabel();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            groupBox1.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numPort).BeginInit();
            groupBox2.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            groupBox3.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            groupBox4.SuspendLayout();
            tableLayoutPanel4.SuspendLayout();
            tabPage2.SuspendLayout();
            groupBox5.SuspendLayout();
            panel1.SuspendLayout();
            tabPage3.SuspendLayout();
            groupBox6.SuspendLayout();
            tableLayoutPanel5.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Controls.Add(tabPage3);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 0);
            tabControl1.Margin = new Padding(7, 8, 7, 8);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(1829, 1344);
            tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(groupBox1);
            tabPage1.Controls.Add(groupBox2);
            tabPage1.Controls.Add(groupBox3);
            tabPage1.Controls.Add(groupBox4);
            tabPage1.Location = new Point(8, 45);
            tabPage1.Margin = new Padding(7, 8, 7, 8);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(23, 26, 23, 26);
            tabPage1.Size = new Size(1813, 1291);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "NGS分析流程";
            tabPage1.UseVisualStyleBackColor = true;
            tabPage1.Click += tabPage1_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(tableLayoutPanel1);
            groupBox1.Location = new Point(30, 8);
            groupBox1.Margin = new Padding(7, 8, 7, 8);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(7, 8, 7, 8);
            groupBox1.Size = new Size(1750, 310);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "SSH服务器连接";
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 4;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 187F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33333F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 187F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 66.66666F));
            tableLayoutPanel1.Controls.Add(label1, 0, 0);
            tableLayoutPanel1.Controls.Add(txtHost, 1, 0);
            tableLayoutPanel1.Controls.Add(label2, 2, 0);
            tableLayoutPanel1.Controls.Add(numPort, 3, 0);
            tableLayoutPanel1.Controls.Add(label3, 0, 1);
            tableLayoutPanel1.Controls.Add(txtUser, 1, 1);
            tableLayoutPanel1.Controls.Add(label4, 2, 1);
            tableLayoutPanel1.Controls.Add(txtPass, 3, 1);
            tableLayoutPanel1.Controls.Add(btnConnect, 0, 2);
            tableLayoutPanel1.Controls.Add(lblStatus, 1, 2);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(7, 39);
            tableLayoutPanel1.Margin = new Padding(7, 8, 7, 8);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33333F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33333F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33333F));
            tableLayoutPanel1.Size = new Size(1736, 263);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Right;
            label1.AutoSize = true;
            label1.Location = new Point(40, 28);
            label1.Margin = new Padding(7, 0, 7, 0);
            label1.Name = "label1";
            label1.Size = new Size(140, 31);
            label1.TabIndex = 0;
            label1.Text = "服务器地址:";
            // 
            // txtHost
            // 
            txtHost.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtHost.Location = new Point(194, 24);
            txtHost.Margin = new Padding(7, 8, 7, 8);
            txtHost.Name = "txtHost";
            txtHost.Size = new Size(440, 38);
            txtHost.TabIndex = 1;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Right;
            label2.AutoSize = true;
            label2.Location = new Point(753, 28);
            label2.Margin = new Padding(7, 0, 7, 0);
            label2.Name = "label2";
            label2.Size = new Size(68, 31);
            label2.TabIndex = 2;
            label2.Text = "端口:";
            // 
            // numPort
            // 
            numPort.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            numPort.Location = new Point(835, 24);
            numPort.Margin = new Padding(7, 8, 7, 8);
            numPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numPort.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numPort.Name = "numPort";
            numPort.Size = new Size(894, 38);
            numPort.TabIndex = 3;
            numPort.Value = new decimal(new int[] { 22, 0, 0, 0 });
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Right;
            label3.AutoSize = true;
            label3.Location = new Point(88, 115);
            label3.Margin = new Padding(7, 0, 7, 0);
            label3.Name = "label3";
            label3.Size = new Size(92, 31);
            label3.TabIndex = 4;
            label3.Text = "用户名:";
            // 
            // txtUser
            // 
            txtUser.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtUser.Location = new Point(194, 111);
            txtUser.Margin = new Padding(7, 8, 7, 8);
            txtUser.Name = "txtUser";
            txtUser.Size = new Size(440, 38);
            txtUser.TabIndex = 5;
            // 
            // label4
            // 
            label4.Anchor = AnchorStyles.Right;
            label4.AutoSize = true;
            label4.Location = new Point(753, 115);
            label4.Margin = new Padding(7, 0, 7, 0);
            label4.Name = "label4";
            label4.Size = new Size(68, 31);
            label4.TabIndex = 6;
            label4.Text = "密码:";
            // 
            // txtPass
            // 
            txtPass.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtPass.Location = new Point(835, 111);
            txtPass.Margin = new Padding(7, 8, 7, 8);
            txtPass.Name = "txtPass";
            txtPass.PasswordChar = '*';
            txtPass.Size = new Size(894, 38);
            txtPass.TabIndex = 7;
            // 
            // btnConnect
            // 
            btnConnect.Anchor = AnchorStyles.Right;
            btnConnect.Location = new Point(66, 189);
            btnConnect.Margin = new Padding(7, 8, 7, 8);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(114, 59);
            btnConnect.TabIndex = 8;
            btnConnect.Text = "连接";
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += btnConnect_Click;
            // 
            // lblStatus
            // 
            lblStatus.Anchor = AnchorStyles.Left;
            lblStatus.AutoSize = true;
            tableLayoutPanel1.SetColumnSpan(lblStatus, 3);
            lblStatus.Font = new Font("宋体", 9F, FontStyle.Bold, GraphicsUnit.Point, 134);
            lblStatus.Location = new Point(194, 206);
            lblStatus.Margin = new Padding(7, 0, 7, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(114, 24);
            lblStatus.TabIndex = 9;
            lblStatus.Text = "未连接 ✗";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(tableLayoutPanel2);
            groupBox2.Location = new Point(30, 347);
            groupBox2.Margin = new Padding(7, 8, 7, 8);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(7, 8, 7, 8);
            groupBox2.Size = new Size(1750, 310);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "文件选择";
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 3;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 280F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 187F));
            tableLayoutPanel2.Controls.Add(label5, 0, 0);
            tableLayoutPanel2.Controls.Add(txtRefPath, 1, 0);
            tableLayoutPanel2.Controls.Add(btnBrowseRef, 2, 0);
            tableLayoutPanel2.Controls.Add(label6, 0, 1);
            tableLayoutPanel2.Controls.Add(txtFastqPath, 1, 1);
            tableLayoutPanel2.Controls.Add(btnBrowseFastq, 2, 1);
            tableLayoutPanel2.Controls.Add(label7, 0, 2);
            tableLayoutPanel2.Controls.Add(txtOutputPath, 1, 2);
            tableLayoutPanel2.Controls.Add(btnBrowseOutput, 2, 2);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(7, 39);
            tableLayoutPanel2.Margin = new Padding(7, 8, 7, 8);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 3;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33333F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33333F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33333F));
            tableLayoutPanel2.Size = new Size(1736, 263);
            tableLayoutPanel2.TabIndex = 0;
            // 
            // label5
            // 
            label5.Anchor = AnchorStyles.Right;
            label5.AutoSize = true;
            label5.Location = new Point(66, 28);
            label5.Margin = new Padding(7, 0, 7, 0);
            label5.Name = "label5";
            label5.Size = new Size(207, 31);
            label5.TabIndex = 0;
            label5.Text = "参考序列(FASTA):";
            // 
            // txtRefPath
            // 
            txtRefPath.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtRefPath.Location = new Point(287, 24);
            txtRefPath.Margin = new Padding(7, 8, 7, 8);
            txtRefPath.Name = "txtRefPath";
            txtRefPath.Size = new Size(1255, 38);
            txtRefPath.TabIndex = 1;
            // 
            // btnBrowseRef
            // 
            btnBrowseRef.Anchor = AnchorStyles.Left;
            btnBrowseRef.Location = new Point(1556, 16);
            btnBrowseRef.Margin = new Padding(7, 8, 7, 8);
            btnBrowseRef.Name = "btnBrowseRef";
            btnBrowseRef.Size = new Size(173, 54);
            btnBrowseRef.TabIndex = 2;
            btnBrowseRef.Text = "浏览...";
            btnBrowseRef.UseVisualStyleBackColor = true;
            btnBrowseRef.Click += btnBrowseRef_Click;
            // 
            // label6
            // 
            label6.Anchor = AnchorStyles.Right;
            label6.AutoSize = true;
            label6.Location = new Point(63, 115);
            label6.Margin = new Padding(7, 0, 7, 0);
            label6.Name = "label6";
            label6.Size = new Size(210, 31);
            label6.TabIndex = 3;
            label6.Text = "测序数据(FASTQ):";
            // 
            // txtFastqPath
            // 
            txtFastqPath.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtFastqPath.Location = new Point(287, 111);
            txtFastqPath.Margin = new Padding(7, 8, 7, 8);
            txtFastqPath.Name = "txtFastqPath";
            txtFastqPath.Size = new Size(1255, 38);
            txtFastqPath.TabIndex = 4;
            // 
            // btnBrowseFastq
            // 
            btnBrowseFastq.Anchor = AnchorStyles.Left;
            btnBrowseFastq.Location = new Point(1556, 103);
            btnBrowseFastq.Margin = new Padding(7, 8, 7, 8);
            btnBrowseFastq.Name = "btnBrowseFastq";
            btnBrowseFastq.Size = new Size(173, 54);
            btnBrowseFastq.TabIndex = 5;
            btnBrowseFastq.Text = "浏览...";
            btnBrowseFastq.UseVisualStyleBackColor = true;
            btnBrowseFastq.Click += btnBrowseFastq_Click;
            // 
            // label7
            // 
            label7.Anchor = AnchorStyles.Right;
            label7.AutoSize = true;
            label7.Location = new Point(85, 203);
            label7.Margin = new Padding(7, 0, 7, 0);
            label7.Name = "label7";
            label7.Size = new Size(188, 31);
            label7.TabIndex = 6;
            label7.Text = "本地输出文件夹:";
            // 
            // txtOutputPath
            // 
            txtOutputPath.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtOutputPath.Location = new Point(287, 199);
            txtOutputPath.Margin = new Padding(7, 8, 7, 8);
            txtOutputPath.Name = "txtOutputPath";
            txtOutputPath.Size = new Size(1255, 38);
            txtOutputPath.TabIndex = 7;
            // 
            // btnBrowseOutput
            // 
            btnBrowseOutput.Anchor = AnchorStyles.Left;
            btnBrowseOutput.Location = new Point(1556, 189);
            btnBrowseOutput.Margin = new Padding(7, 8, 7, 8);
            btnBrowseOutput.Name = "btnBrowseOutput";
            btnBrowseOutput.Size = new Size(173, 59);
            btnBrowseOutput.TabIndex = 8;
            btnBrowseOutput.Text = "浏览...";
            btnBrowseOutput.UseVisualStyleBackColor = true;
            btnBrowseOutput.Click += btnBrowseOutput_Click;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(tableLayoutPanel3);
            groupBox3.Location = new Point(30, 673);
            groupBox3.Margin = new Padding(7, 8, 7, 8);
            groupBox3.Name = "groupBox3";
            groupBox3.Padding = new Padding(7, 8, 7, 8);
            groupBox3.Size = new Size(1750, 388);
            groupBox3.TabIndex = 2;
            groupBox3.TabStop = false;
            groupBox3.Text = "分析流程操作";
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 5;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel3.Controls.Add(btnUploadRef, 0, 0);
            tableLayoutPanel3.Controls.Add(btnBuildIndex, 1, 0);
            tableLayoutPanel3.Controls.Add(btnBwaAlign, 2, 0);
            tableLayoutPanel3.Controls.Add(btnProcessSam, 3, 0);
            tableLayoutPanel3.Controls.Add(btnVariantDetection, 4, 0);
            tableLayoutPanel3.Controls.Add(btnTestVariantCmd, 0, 1);
            tableLayoutPanel3.Controls.Add(btnViewVcfInfo, 1, 1);
            tableLayoutPanel3.Controls.Add(btnGenerateVisualization, 2, 1);
            tableLayoutPanel3.Controls.Add(btnManageModules, 3, 1);
            tableLayoutPanel3.Controls.Add(btnCleanRemoteTemp, 4, 1);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(7, 39);
            tableLayoutPanel3.Margin = new Padding(7, 8, 7, 8);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 2;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel3.Size = new Size(1736, 341);
            tableLayoutPanel3.TabIndex = 0;
            // 
            // btnUploadRef
            // 
            btnUploadRef.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnUploadRef.Enabled = false;
            btnUploadRef.Location = new Point(23, 26);
            btnUploadRef.Margin = new Padding(23, 26, 23, 26);
            btnUploadRef.Name = "btnUploadRef";
            btnUploadRef.Size = new Size(301, 118);
            btnUploadRef.TabIndex = 0;
            btnUploadRef.Text = "1. 上传参考序列";
            btnUploadRef.UseVisualStyleBackColor = true;
            btnUploadRef.Click += btnUploadRef_Click;
            // 
            // btnBuildIndex
            // 
            btnBuildIndex.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnBuildIndex.Enabled = false;
            btnBuildIndex.Location = new Point(370, 26);
            btnBuildIndex.Margin = new Padding(23, 26, 23, 26);
            btnBuildIndex.Name = "btnBuildIndex";
            btnBuildIndex.Size = new Size(301, 118);
            btnBuildIndex.TabIndex = 1;
            btnBuildIndex.Text = "2. 构建BWA索引";
            btnBuildIndex.UseVisualStyleBackColor = true;
            btnBuildIndex.Click += btnBuildIndex_Click;
            // 
            // btnBwaAlign
            // 
            btnBwaAlign.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnBwaAlign.Enabled = false;
            btnBwaAlign.Location = new Point(717, 26);
            btnBwaAlign.Margin = new Padding(23, 26, 23, 26);
            btnBwaAlign.Name = "btnBwaAlign";
            btnBwaAlign.Size = new Size(301, 118);
            btnBwaAlign.TabIndex = 2;
            btnBwaAlign.Text = "3. BWA比对";
            btnBwaAlign.UseVisualStyleBackColor = true;
            btnBwaAlign.Click += btnBwaAlign_Click;
            // 
            // btnProcessSam
            // 
            btnProcessSam.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnProcessSam.Enabled = false;
            btnProcessSam.Location = new Point(1064, 26);
            btnProcessSam.Margin = new Padding(23, 26, 23, 26);
            btnProcessSam.Name = "btnProcessSam";
            btnProcessSam.Size = new Size(301, 118);
            btnProcessSam.TabIndex = 3;
            btnProcessSam.Text = "4. 处理SAM/BAM";
            btnProcessSam.UseVisualStyleBackColor = true;
            btnProcessSam.Click += btnProcessSam_Click;
            // 
            // btnVariantDetection
            // 
            btnVariantDetection.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnVariantDetection.Enabled = false;
            btnVariantDetection.Location = new Point(1411, 26);
            btnVariantDetection.Margin = new Padding(23, 26, 23, 26);
            btnVariantDetection.Name = "btnVariantDetection";
            btnVariantDetection.Size = new Size(302, 118);
            btnVariantDetection.TabIndex = 4;
            btnVariantDetection.Text = "5. 变异检测";
            btnVariantDetection.UseVisualStyleBackColor = true;
            btnVariantDetection.Click += btnVariantDetection_Click;
            // 
            // btnTestVariantCmd
            // 
            btnTestVariantCmd.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnTestVariantCmd.Enabled = false;
            btnTestVariantCmd.Location = new Point(23, 196);
            btnTestVariantCmd.Margin = new Padding(23, 26, 23, 26);
            btnTestVariantCmd.Name = "btnTestVariantCmd";
            btnTestVariantCmd.Size = new Size(301, 119);
            btnTestVariantCmd.TabIndex = 5;
            btnTestVariantCmd.Text = "测试变异检测命令(开发者选项)";
            btnTestVariantCmd.UseVisualStyleBackColor = true;
            btnTestVariantCmd.Click += btnTestVariantCmd_Click;
            // 
            // btnViewVcfInfo
            // 
            btnViewVcfInfo.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnViewVcfInfo.Enabled = false;
            btnViewVcfInfo.Location = new Point(370, 196);
            btnViewVcfInfo.Margin = new Padding(23, 26, 23, 26);
            btnViewVcfInfo.Name = "btnViewVcfInfo";
            btnViewVcfInfo.Size = new Size(301, 119);
            btnViewVcfInfo.TabIndex = 6;
            btnViewVcfInfo.Text = "查看VCF文件信息";
            btnViewVcfInfo.UseVisualStyleBackColor = true;
            btnViewVcfInfo.Click += btnViewVcfInfo_Click;
            // 
            // btnGenerateVisualization
            // 
            btnGenerateVisualization.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnGenerateVisualization.Enabled = false;
            btnGenerateVisualization.Location = new Point(717, 196);
            btnGenerateVisualization.Margin = new Padding(23, 26, 23, 26);
            btnGenerateVisualization.Name = "btnGenerateVisualization";
            btnGenerateVisualization.Size = new Size(301, 119);
            btnGenerateVisualization.TabIndex = 7;
            btnGenerateVisualization.Text = "生成报告";
            btnGenerateVisualization.UseVisualStyleBackColor = true;
            btnGenerateVisualization.Click += BtnGenerateVisualization_Click;
            // 
            // btnManageModules
            // 
            btnManageModules.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnManageModules.Enabled = false;
            btnManageModules.Location = new Point(1064, 196);
            btnManageModules.Margin = new Padding(23, 26, 23, 26);
            btnManageModules.Name = "btnManageModules";
            btnManageModules.Size = new Size(301, 119);
            btnManageModules.TabIndex = 8;
            btnManageModules.Text = "管理可视化模块";
            btnManageModules.UseVisualStyleBackColor = true;
            btnManageModules.Click += btnManageModules_Click;
            // 
            // btnCleanRemoteTemp
            // 
            btnCleanRemoteTemp.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnCleanRemoteTemp.Enabled = false;
            btnCleanRemoteTemp.Location = new Point(1411, 196);
            btnCleanRemoteTemp.Margin = new Padding(23, 26, 23, 26);
            btnCleanRemoteTemp.Name = "btnCleanRemoteTemp";
            btnCleanRemoteTemp.Size = new Size(302, 119);
            btnCleanRemoteTemp.TabIndex = 9;
            btnCleanRemoteTemp.Text = "清除服务器临时文件夹";
            btnCleanRemoteTemp.UseVisualStyleBackColor = true;
            btnCleanRemoteTemp.Click += btnCleanRemoteTemp_Click;
            // 
            // groupBox4
            // 
            groupBox4.Controls.Add(tableLayoutPanel4);
            groupBox4.Location = new Point(30, 1077);
            groupBox4.Margin = new Padding(7, 8, 7, 8);
            groupBox4.Name = "groupBox4";
            groupBox4.Padding = new Padding(7, 8, 7, 8);
            groupBox4.Size = new Size(1750, 258);
            groupBox4.TabIndex = 3;
            groupBox4.TabStop = false;
            groupBox4.Text = "状态信息";
            // 
            // tableLayoutPanel4
            // 
            tableLayoutPanel4.ColumnCount = 3;
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 280F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 280F));
            tableLayoutPanel4.Controls.Add(label8, 0, 0);
            tableLayoutPanel4.Controls.Add(lblToolsStatus, 1, 0);
            tableLayoutPanel4.Controls.Add(progressBar1, 1, 1);
            tableLayoutPanel4.Controls.Add(lblProgress, 2, 1);
            tableLayoutPanel4.Dock = DockStyle.Fill;
            tableLayoutPanel4.Location = new Point(7, 39);
            tableLayoutPanel4.Margin = new Padding(7, 8, 7, 8);
            tableLayoutPanel4.Name = "tableLayoutPanel4";
            tableLayoutPanel4.RowCount = 2;
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel4.Size = new Size(1736, 211);
            tableLayoutPanel4.TabIndex = 0;
            // 
            // label8
            // 
            label8.Anchor = AnchorStyles.Right;
            label8.AutoSize = true;
            label8.Font = new Font("宋体", 9F, FontStyle.Bold, GraphicsUnit.Point, 134);
            label8.Location = new Point(138, 40);
            label8.Margin = new Padding(7, 0, 7, 0);
            label8.Name = "label8";
            label8.Size = new Size(135, 24);
            label8.TabIndex = 0;
            label8.Text = "工具状态：";
            // 
            // lblToolsStatus
            // 
            lblToolsStatus.Anchor = AnchorStyles.Left;
            lblToolsStatus.AutoSize = true;
            lblToolsStatus.Font = new Font("宋体", 9F, FontStyle.Bold, GraphicsUnit.Point, 134);
            lblToolsStatus.ForeColor = Color.Red;
            lblToolsStatus.Location = new Point(287, 40);
            lblToolsStatus.Margin = new Padding(7, 0, 7, 0);
            lblToolsStatus.Name = "lblToolsStatus";
            lblToolsStatus.Size = new Size(139, 24);
            lblToolsStatus.TabIndex = 1;
            lblToolsStatus.Text = "✗ 工具缺失";
            // 
            // progressBar1
            // 
            progressBar1.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            progressBar1.Location = new Point(287, 128);
            progressBar1.Margin = new Padding(7, 8, 7, 8);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(1162, 59);
            progressBar1.TabIndex = 2;
            // 
            // lblProgress
            // 
            lblProgress.Anchor = AnchorStyles.Left;
            lblProgress.AutoSize = true;
            lblProgress.Location = new Point(1463, 142);
            lblProgress.Margin = new Padding(7, 0, 7, 0);
            lblProgress.Name = "lblProgress";
            lblProgress.Size = new Size(110, 31);
            lblProgress.TabIndex = 3;
            lblProgress.Text = "准备就绪";
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(groupBox5);
            tabPage2.Location = new Point(8, 45);
            tabPage2.Margin = new Padding(7, 8, 7, 8);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(23, 26, 23, 26);
            tabPage2.Size = new Size(1813, 1291);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "日志查看";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // groupBox5
            // 
            groupBox5.Controls.Add(txtLogViewer);
            groupBox5.Controls.Add(panel1);
            groupBox5.Dock = DockStyle.Fill;
            groupBox5.Location = new Point(23, 26);
            groupBox5.Margin = new Padding(7, 8, 7, 8);
            groupBox5.Name = "groupBox5";
            groupBox5.Padding = new Padding(7, 8, 7, 8);
            groupBox5.Size = new Size(1767, 1239);
            groupBox5.TabIndex = 0;
            groupBox5.TabStop = false;
            groupBox5.Text = "运行日志";
            // 
            // txtLogViewer
            // 
            txtLogViewer.Dock = DockStyle.Fill;
            txtLogViewer.Font = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            txtLogViewer.Location = new Point(7, 114);
            txtLogViewer.Margin = new Padding(7, 8, 7, 8);
            txtLogViewer.Multiline = true;
            txtLogViewer.Name = "txtLogViewer";
            txtLogViewer.ReadOnly = true;
            txtLogViewer.ScrollBars = ScrollBars.Both;
            txtLogViewer.Size = new Size(1753, 1117);
            txtLogViewer.TabIndex = 1;
            txtLogViewer.WordWrap = false;
            // 
            // panel1
            // 
            panel1.Controls.Add(btnViewLogs);
            panel1.Controls.Add(btnClearLogs);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(7, 39);
            panel1.Margin = new Padding(7, 8, 7, 8);
            panel1.Name = "panel1";
            panel1.Size = new Size(1753, 75);
            panel1.TabIndex = 0;
            // 
            // btnViewLogs
            // 
            btnViewLogs.Location = new Point(14, 8);
            btnViewLogs.Margin = new Padding(7, 8, 7, 8);
            btnViewLogs.Name = "btnViewLogs";
            btnViewLogs.Size = new Size(175, 59);
            btnViewLogs.TabIndex = 0;
            btnViewLogs.Text = "刷新日志";
            btnViewLogs.UseVisualStyleBackColor = true;
            btnViewLogs.Click += btnViewLogs_Click;
            // 
            // btnClearLogs
            // 
            btnClearLogs.Location = new Point(203, 8);
            btnClearLogs.Margin = new Padding(7, 8, 7, 8);
            btnClearLogs.Name = "btnClearLogs";
            btnClearLogs.Size = new Size(175, 59);
            btnClearLogs.TabIndex = 1;
            btnClearLogs.Text = "清空日志";
            btnClearLogs.UseVisualStyleBackColor = true;
            btnClearLogs.Click += btnClearLogs_Click;
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(groupBox6);
            tabPage3.Location = new Point(8, 45);
            tabPage3.Margin = new Padding(7, 8, 7, 8);
            tabPage3.Name = "tabPage3";
            tabPage3.Padding = new Padding(23, 26, 23, 26);
            tabPage3.Size = new Size(1813, 1291);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "数据可视化";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // groupBox6
            // 
            groupBox6.Controls.Add(tableLayoutPanel5);
            groupBox6.Dock = DockStyle.Fill;
            groupBox6.Location = new Point(23, 26);
            groupBox6.Margin = new Padding(7, 8, 7, 8);
            groupBox6.Name = "groupBox6";
            groupBox6.Padding = new Padding(7, 8, 7, 8);
            groupBox6.Size = new Size(1767, 1239);
            groupBox6.TabIndex = 0;
            groupBox6.TabStop = false;
            groupBox6.Text = "可视化配置和生成";
            // 
            // tableLayoutPanel5
            // 
            tableLayoutPanel5.ColumnCount = 2;
            tableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel5.Controls.Add(label9, 0, 0);
            tableLayoutPanel5.Controls.Add(label10, 0, 1);
            tableLayoutPanel5.Controls.Add(cbHighResolution, 1, 1);
            tableLayoutPanel5.Controls.Add(cbCombinePDF, 1, 2);
            tableLayoutPanel5.Controls.Add(btnRunAllVisualizations, 0, 3);
            tableLayoutPanel5.Controls.Add(btnConfigureModules, 1, 3);
            tableLayoutPanel5.Controls.Add(label11, 0, 4);
            tableLayoutPanel5.Controls.Add(lblLoadedModules, 1, 4);
            tableLayoutPanel5.Controls.Add(lstModuleList, 0, 5);
            tableLayoutPanel5.Controls.Add(btnRefreshModules, 1, 5);
            tableLayoutPanel5.Dock = DockStyle.Fill;
            tableLayoutPanel5.Location = new Point(7, 39);
            tableLayoutPanel5.Margin = new Padding(7, 8, 7, 8);
            tableLayoutPanel5.Name = "tableLayoutPanel5";
            tableLayoutPanel5.RowCount = 6;
            tableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F));
            tableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F));
            tableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F));
            tableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Absolute, 150F));
            tableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F));
            tableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel5.Size = new Size(1753, 1192);
            tableLayoutPanel5.TabIndex = 0;
            // 
            // label9
            // 
            label9.Anchor = AnchorStyles.Left;
            label9.AutoSize = true;
            tableLayoutPanel5.SetColumnSpan(label9, 2);
            label9.Font = new Font("宋体", 14F, FontStyle.Bold, GraphicsUnit.Point, 134);
            label9.Location = new Point(7, 31);
            label9.Margin = new Padding(7, 0, 7, 0);
            label9.Name = "label9";
            label9.Size = new Size(407, 38);
            label9.TabIndex = 0;
            label9.Text = "基因组数据可视化配置";
            // 
            // label10
            // 
            label10.Anchor = AnchorStyles.Right;
            label10.AutoSize = true;
            label10.Location = new Point(783, 134);
            label10.Margin = new Padding(7, 0, 7, 0);
            label10.Name = "label10";
            label10.Size = new Size(86, 31);
            label10.TabIndex = 1;
            label10.Text = "设置：";
            // 
            // cbHighResolution
            // 
            cbHighResolution.Anchor = AnchorStyles.Left;
            cbHighResolution.AutoSize = true;
            cbHighResolution.Checked = true;
            cbHighResolution.CheckState = CheckState.Checked;
            cbHighResolution.Location = new Point(883, 132);
            cbHighResolution.Margin = new Padding(7, 8, 7, 8);
            cbHighResolution.Name = "cbHighResolution";
            cbHighResolution.Size = new Size(236, 35);
            cbHighResolution.TabIndex = 2;
            cbHighResolution.Text = "高分辨率(300dpi)";
            cbHighResolution.UseVisualStyleBackColor = true;
            // 
            // cbCombinePDF
            // 
            cbCombinePDF.Anchor = AnchorStyles.Left;
            cbCombinePDF.AutoSize = true;
            cbCombinePDF.Checked = true;
            cbCombinePDF.CheckState = CheckState.Checked;
            cbCombinePDF.Location = new Point(883, 232);
            cbCombinePDF.Margin = new Padding(7, 8, 7, 8);
            cbCombinePDF.Name = "cbCombinePDF";
            cbCombinePDF.Size = new Size(308, 35);
            cbCombinePDF.TabIndex = 3;
            cbCombinePDF.Text = "合并所有图表到单个PDF";
            cbCombinePDF.UseVisualStyleBackColor = true;
            // 
            // btnRunAllVisualizations
            // 
            btnRunAllVisualizations.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnRunAllVisualizations.Enabled = false;
            btnRunAllVisualizations.Font = new Font("宋体", 10F, FontStyle.Bold, GraphicsUnit.Point, 134);
            btnRunAllVisualizations.Location = new Point(23, 326);
            btnRunAllVisualizations.Margin = new Padding(23, 26, 23, 26);
            btnRunAllVisualizations.Name = "btnRunAllVisualizations";
            btnRunAllVisualizations.Size = new Size(830, 98);
            btnRunAllVisualizations.TabIndex = 4;
            btnRunAllVisualizations.Text = "生成所有可视化报告";
            btnRunAllVisualizations.UseVisualStyleBackColor = true;
            btnRunAllVisualizations.Click += btnRunAllVisualizations_Click;
            // 
            // btnConfigureModules
            // 
            btnConfigureModules.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnConfigureModules.Enabled = false;
            btnConfigureModules.Location = new Point(899, 326);
            btnConfigureModules.Margin = new Padding(23, 26, 23, 26);
            btnConfigureModules.Name = "btnConfigureModules";
            btnConfigureModules.Size = new Size(831, 98);
            btnConfigureModules.TabIndex = 5;
            btnConfigureModules.Text = "配置可视化模块...";
            btnConfigureModules.UseVisualStyleBackColor = true;
            btnConfigureModules.Click += btnConfigureModules_Click;
            // 
            // label11
            // 
            label11.Anchor = AnchorStyles.Right;
            label11.AutoSize = true;
            label11.Font = new Font("宋体", 9F, FontStyle.Bold, GraphicsUnit.Point, 134);
            label11.Location = new Point(621, 488);
            label11.Margin = new Padding(7, 0, 7, 0);
            label11.Name = "label11";
            label11.Size = new Size(248, 24);
            label11.TabIndex = 6;
            label11.Text = "已加载的可视化模块:";
            // 
            // lblLoadedModules
            // 
            lblLoadedModules.Anchor = AnchorStyles.Left;
            lblLoadedModules.AutoSize = true;
            lblLoadedModules.Font = new Font("宋体", 9F, FontStyle.Bold, GraphicsUnit.Point, 134);
            lblLoadedModules.ForeColor = Color.Blue;
            lblLoadedModules.Location = new Point(883, 488);
            lblLoadedModules.Margin = new Padding(7, 0, 7, 0);
            lblLoadedModules.Name = "lblLoadedModules";
            lblLoadedModules.Size = new Size(48, 24);
            lblLoadedModules.TabIndex = 7;
            lblLoadedModules.Text = "0个";
            // 
            // lstModuleList
            // 
            lstModuleList.Dock = DockStyle.Fill;
            lstModuleList.FormattingEnabled = true;
            lstModuleList.Location = new Point(7, 558);
            lstModuleList.Margin = new Padding(7, 8, 7, 8);
            lstModuleList.Name = "lstModuleList";
            lstModuleList.Size = new Size(862, 626);
            lstModuleList.TabIndex = 8;
            // 
            // btnRefreshModules
            // 
            btnRefreshModules.Anchor = AnchorStyles.Top;
            btnRefreshModules.Location = new Point(1214, 558);
            btnRefreshModules.Margin = new Padding(23, 8, 23, 8);
            btnRefreshModules.Name = "btnRefreshModules";
            btnRefreshModules.Size = new Size(200, 59);
            btnRefreshModules.TabIndex = 9;
            btnRefreshModules.Text = "刷新模块列表";
            btnRefreshModules.UseVisualStyleBackColor = true;
            btnRefreshModules.Click += btnRefreshModules_Click;
            // 
            // statusStrip
            // 
            statusStrip.ImageScalingSize = new Size(32, 32);
            statusStrip.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1, lblStatusMessage, toolStripStatusLabel2, toolStripStatusLabel3 });
            statusStrip.Location = new Point(0, 1344);
            statusStrip.Name = "statusStrip";
            statusStrip.Padding = new Padding(2, 0, 33, 0);
            statusStrip.Size = new Size(1829, 41);
            statusStrip.TabIndex = 1;
            statusStrip.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(116, 31);
            toolStripStatusLabel1.Text = "状态消息:";
            // 
            // lblStatusMessage
            // 
            lblStatusMessage.Name = "lblStatusMessage";
            lblStatusMessage.Size = new Size(278, 31);
            lblStatusMessage.Text = "准备就绪 - 请连接服务器";
            // 
            // toolStripStatusLabel2
            // 
            toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            toolStripStatusLabel2.Size = new Size(1126, 31);
            toolStripStatusLabel2.Spring = true;
            // 
            // toolStripStatusLabel3
            // 
            toolStripStatusLabel3.Name = "toolStripStatusLabel3";
            toolStripStatusLabel3.Size = new Size(274, 31);
            toolStripStatusLabel3.Text = "NGS BWA Runner v1.0";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(14F, 31F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1829, 1385);
            Controls.Add(tabControl1);
            Controls.Add(statusStrip);
            Margin = new Padding(7, 8, 7, 8);
            MinimumSize = new Size(1832, 1438);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "NGS BWA Runner - 完整的NGS分析流程和可视化";
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numPort).EndInit();
            groupBox2.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            groupBox3.ResumeLayout(false);
            tableLayoutPanel3.ResumeLayout(false);
            groupBox4.ResumeLayout(false);
            tableLayoutPanel4.ResumeLayout(false);
            tableLayoutPanel4.PerformLayout();
            tabPage2.ResumeLayout(false);
            groupBox5.ResumeLayout(false);
            groupBox5.PerformLayout();
            panel1.ResumeLayout(false);
            tabPage3.ResumeLayout(false);
            groupBox6.ResumeLayout(false);
            tableLayoutPanel5.ResumeLayout(false);
            tableLayoutPanel5.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TabControl tabControl1;
        private TabPage tabPage1;
        private GroupBox groupBox1;
        private TableLayoutPanel tableLayoutPanel1;
        private Label label1;
        private TextBox txtHost;
        private Label label2;
        private NumericUpDown numPort;
        private Label label3;
        private TextBox txtUser;
        private Label label4;
        private TextBox txtPass;
        private Button btnConnect;
        private Label lblStatus;
        private GroupBox groupBox2;
        private TableLayoutPanel tableLayoutPanel2;
        private Label label5;
        private TextBox txtRefPath;
        private Button btnBrowseRef;
        private Label label6;
        private TextBox txtFastqPath;
        private Button btnBrowseFastq;
        private Label label7;
        private TextBox txtOutputPath;
        private Button btnBrowseOutput;
        private GroupBox groupBox3;
        private TableLayoutPanel tableLayoutPanel3;
        private Button btnUploadRef;
        private Button btnBuildIndex;
        private Button btnBwaAlign;
        private Button btnProcessSam;
        private Button btnVariantDetection;
        private GroupBox groupBox4;
        private TableLayoutPanel tableLayoutPanel4;
        private Label label8;
        private Label lblToolsStatus;
        private ProgressBar progressBar1;
        private Label lblProgress;
        private TabPage tabPage2;
        private GroupBox groupBox5;
        private TextBox txtLogViewer;
        private Panel panel1;
        private Button btnViewLogs;
        private Button btnClearLogs;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private ToolStripStatusLabel lblStatusMessage;
        private ToolStripStatusLabel toolStripStatusLabel2;
        private ToolStripStatusLabel toolStripStatusLabel3;
        private Button btnTestVariantCmd;
        private Button btnViewVcfInfo;
        private Button btnGenerateVisualization;
        private Button btnManageModules;
        private TabPage tabPage3;
        private GroupBox groupBox6;
        private TableLayoutPanel tableLayoutPanel5;
        private Label label9;
        private Label label10;
        private CheckBox cbHighResolution;
        private CheckBox cbCombinePDF;
        private Button btnRunAllVisualizations;
        private Button btnConfigureModules;
        private Label label11;
        private Label lblLoadedModules;
        private ListBox lstModuleList;
        private Button btnRefreshModules;
        private Button btnCleanRemoteTemp;
    }
}