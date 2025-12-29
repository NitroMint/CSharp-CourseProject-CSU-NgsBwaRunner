using System;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using System.Drawing;
using NgsBwaRunner.Models;
using NgsBwaRunner.Services;
using NgsBwaRunner.Utils;
using NgsBwaRunner.Visualization.Services;

namespace NgsBwaRunner
{
    public partial class MainForm : Form
    {
        // 将字段声明为可空
        private SshService? _sshService;
        private BwaService? _bwaService;
        private SamtoolsService? _samtoolsService;
        private FileService? _fileService;
        private VisualizationManager? _visualizationManager;
        // 原有字段
        private string _currentSampleName = "";
        private string _localSamPath = "";
        private RemoteCleanupService? _remoteCleanupService;
        private bool _visualizationInitialized = false;

        public MainForm()
        {
            // 先初始化控件
            InitializeComponent();

            // 初始化服务
            InitializeServices();

            // 设置默认值
            SetupDefaultValues();

            // 检查工具 - 这里可能会出现控件访问问题
            CheckTools();

            // 初始化可视化按钮
            InitializeVisualizationButton();

            // 绑定事件
            btnGenerateVisualization.Click += BtnGenerateVisualization_Click;

            // 创建滚动面板 - 确保在所有控件初始化后执行
            CreateScrollPanel();
        }

        private void CreateScrollPanel()
        {
            // 检查是否有有效的控件可以添加到滚动面板
            if (this.Controls.Count == 0)
            {
                return; // 没有控件，直接返回
            }

            Panel mainScrollPanel = new Panel
            {
                Name = "mainScrollPanel",
                Dock = DockStyle.Fill,
                AutoScroll = true,
                AutoScrollMargin = new System.Drawing.Size(10, 10),
                AutoScrollMinSize = new System.Drawing.Size(this.ClientSize.Width, 760)
            };

            try
            {
                // 复制控件到数组，避免在遍历集合时修改它
                Control[] controls = new Control[this.Controls.Count];
                this.Controls.CopyTo(controls, 0);

                // 清空窗体控件
                this.Controls.Clear();

                // 添加控件到滚动面板
                foreach (Control control in controls)
                {
                    if (control != null) // 确保不为null
                    {
                        mainScrollPanel.Controls.Add(control);
                    }
                }

                // 添加滚动面板到窗体
                this.Controls.Add(mainScrollPanel);
            }
            catch (Exception ex)
            {
                // 如果创建滚动面板失败，至少恢复原始控件
                MessageBox.Show($"创建滚动面板时出错: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                // 重新添加控件到窗体
                foreach (Control control in mainScrollPanel.Controls)
                {
                    this.Controls.Add(control);
                }
            }
        }

        private void InitializeServices()
        {
            _sshService = new SshService();
            _fileService = new FileService();
            _remoteCleanupService = new RemoteCleanupService(_sshService);
        }

        private void SetupDefaultValues()
        {
            try
            {
                // 确保控件不为null再访问
                if (txtHost != null)
                    txtHost.Text = Config.Settings.DEFAULT_HOST;

                if (numPort != null)
                    numPort.Value = Config.Settings.DEFAULT_PORT;

                if (txtUser != null)
                    txtUser.Text = Config.Settings.DEFAULT_USER;

                if (txtPass != null)
                    txtPass.Text = Config.Settings.DEFAULT_PASS;

                // 设置默认输出路径
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                if (txtOutputPath != null)
                    txtOutputPath.Text = Path.Combine(desktop, "NGS_Output_" + DateTime.Now.ToString("yyyyMMdd"));

                // 初始化进度条
                if (progressBar1 != null)
                {
                    progressBar1.Minimum = 0;
                    progressBar1.Maximum = 100;
                    progressBar1.Value = 0;
                }
            }
            catch (Exception ex)
            {
                // 记录错误但不中断程序启动
                Console.WriteLine($"设置默认值时出错: {ex.Message}");
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // 窗体加载时刷新日志
            RefreshLogViewer();
        }

        // 检查工具文件
        private void CheckTools()
        {
            try
            {
                bool toolsReady = _fileService.CheckLocalTools();
                UpdateToolsStatus(toolsReady);

                if (!toolsReady)
                {
                    MessageBox.Show($"请在 tools 目录下放置以下Linux可执行文件:\n" +
                                  "1. bwa\n2. samtools\n3. bcftools\n\n" +
                                  "可以从Linux系统的/usr/bin/目录复制这些文件。",
                                  "工具文件缺失", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"检查工具失败: {ex.Message}");
            }
        }

        // ========== SSH连接 ==========
        private async void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                // 获取SSH配置
                var sshConfig = new Models.SshConfig(
                    txtHost.Text,
                    (int)numPort.Value,
                    txtUser.Text,
                    txtPass.Text
                );

                // 验证配置
                if (!sshConfig.IsValid())
                {
                    MessageBox.Show("请填写完整的SSH连接信息", "输入错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 更新UI
                UpdateStatus("正在连接服务器...", Color.Orange);
                UpdateConnectButton(false);
                UpdateProgress("连接服务器", 0);

                // 连接服务器
                bool connected = await System.Threading.Tasks.Task.Run(() => _sshService.Connect(sshConfig));

                if (connected)
                {
                    UpdateStatus("已连接 ✓", Color.Green);
                    UpdateProgress("连接成功", 50);

                    // 初始化服务
                    _bwaService = new BwaService(_sshService);
                    _samtoolsService = new SamtoolsService(_sshService);

                    // 上传工具
                    try
                    {
                        UpdateStatus("正在上传工具...", Color.Orange);
                        UpdateProgress("上传工具", 60);

                        await System.Threading.Tasks.Task.Run(() =>
                        {
                            _bwaService.UploadTools();
                            _samtoolsService.UploadTools();
                            _samtoolsService.UploadBcftools();
                        });

                        UpdateToolsStatus(true);
                        UpdateStatus("工具上传完成 ✓", Color.Green);
                        UpdateProgress("工具就绪", 100);

                        // 启用操作按钮 - 只在连接并上传工具成功后
                        UpdateOperationButtons(true);

                        // 初始化并启用可视化按钮 - 只在连接并上传工具成功后
                        InitializeVisualizationAndButtons();

                        UpdateStatusMessage("服务器连接成功，工具已上传");

                        MessageBox.Show("服务器连接成功，所有工具已部署完成！", "连接成功",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        UpdateToolsStatus(false);
                        UpdateStatus("工具上传失败 ✗", Color.Red);
                        UpdateProgress("工具错误", 0);

                        MessageBox.Show($"工具上传失败: {ex.Message}",
                            "工具错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                else
                {
                    UpdateStatus("连接失败 ✗", Color.Red);
                    UpdateProgress("连接失败", 0);
                    MessageBox.Show("连接失败，请检查:\n1. 服务器地址和端口\n2. 用户名和密码\n3. 服务器SSH服务是否开启",
                        "连接失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"连接失败: {ex.Message}");
                MessageBox.Show($"连接失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("连接失败 ✗", Color.Red);
                UpdateProgress("连接错误", 0);
            }
            finally
            {
                UpdateConnectButton(true);
            }
        }

        private void InitializeVisualizationAndEnableButtons()
        {
            try
            {
                InitializeVisualization();

                // 启用可视化选项卡中的按钮
                if (_visualizationInitialized)
                {
                    // 安全地启用按钮，确保控件不为null
                    this.BeginInvoke((Action)(() =>
                    {
                        if (btnRunAllVisualizations != null)
                            btnRunAllVisualizations.Enabled = true;
                        if (btnConfigureModules != null)
                            btnConfigureModules.Enabled = true;
                        if (btnRefreshModules != null)
                            btnRefreshModules.Enabled = true;
                        if (btnCleanRemoteTemp != null)
                            btnCleanRemoteTemp.Enabled = true;
                    }));
                }
            }
            catch (Exception ex)
            {
                Utils.Logger.LogError($"初始化可视化按钮失败: {ex.Message}");
            }
        }

        // ========== 文件浏览 ==========
        private void btnBrowseRef_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "FASTA文件 (*.fa;*.fasta;*.fna)|*.fa;*.fasta;*.fna|所有文件 (*.*)|*.*";
                ofd.Title = "选择参考序列文件";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtRefPath.Text = ofd.FileName;
                }
            }
        }

        // ========== 清理远程临时文件夹 ==========
        private async void btnCleanRemoteTemp_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_sshService.IsConnected)
                {
                    MessageBox.Show("请先连接服务器", "操作错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 1. 显示警告对话框
                DialogResult warningResult = MessageBox.Show(
                    "⚠️  警告：将删除服务器端临时文件夹资源\n\n" +
                    $"目录路径: {Config.Settings.REMOTE_WORK_DIR}\n\n" +
                    "这将删除所有上传的工具和中间文件。\n" +
                    "确定要继续吗？",
                    "确认删除",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (warningResult != DialogResult.Yes)
                {
                    return;
                }

                // 2. 显示目录信息
                UpdateStatus("获取远程目录信息...", Color.Orange);

                string dirInfo = await System.Threading.Tasks.Task.Run(() =>
                    _remoteCleanupService?.GetRemoteDirectoryInfo() ?? "服务未初始化");

                // 3. 确认对话框（显示具体信息）
                DialogResult confirmResult = MessageBox.Show(
                    "📁 远程目录信息：\n" +
                    "----------------------------------------\n" +
                    dirInfo + "\n" +
                    "----------------------------------------\n\n" +
                    "❓ 确定要删除这个目录及其所有内容吗？\n" +
                    "（此操作不可撤销）",
                    "最终确认",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirmResult != DialogResult.Yes)
                {
                    UpdateStatus("用户取消删除操作", Color.Blue);
                    return;
                }

                // 4. 执行删除
                UpdateStatus("正在删除远程临时目录...", Color.Orange);
                UpdateProgress("删除中...", 30);

                bool success = await System.Threading.Tasks.Task.Run(() =>
                    _remoteCleanupService?.DeleteRemoteTempDirectory() ?? false);

                if (success)
                {
                    UpdateStatus("远程临时目录删除完成 ✓", Color.Green);
                    UpdateProgress("删除完成", 100);

                    MessageBox.Show("✅ 服务器临时文件夹已成功删除",
                        "删除成功",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    UpdateStatus("删除失败 ✗", Color.Red);
                    UpdateProgress("删除失败", 0);

                    MessageBox.Show("❌ 删除服务器临时文件夹失败\n" +
                                  "请检查服务器连接或手动清理",
                        "删除失败",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Utils.Logger.LogError($"清理远程临时文件夹失败: {ex.Message}");
                MessageBox.Show($"清理失败: {ex.Message}",
                    "错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                UpdateStatus("清理失败 ✗", Color.Red);
            }
        }

        private void btnBrowseFastq_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "FASTQ文件 (*.fq;*.fastq;*.fq.gz;*.fastq.gz)|*.fq;*.fastq;*.fq.gz;*.fastq.gz|所有文件 (*.*)|*.*";
                ofd.Title = "选择测序数据文件";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtFastqPath.Text = ofd.FileName;
                    _currentSampleName = Path.GetFileNameWithoutExtension(ofd.FileName);
                }
            }
        }

        private void btnBrowseOutput_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "选择结果输出文件夹";
                fbd.SelectedPath = txtOutputPath.Text;

                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtOutputPath.Text = fbd.SelectedPath;
                }
            }
        }

        // ========== 上传参考序列 ==========
        private async void btnUploadRef_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_sshService.IsConnected)
                {
                    MessageBox.Show("请先连接服务器", "操作错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(txtRefPath.Text))
                {
                    MessageBox.Show("请选择参考序列文件", "文件错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                UpdateStatus("上传参考序列...", Color.Orange);
                UpdateProgress("正在上传...", 30);

                await System.Threading.Tasks.Task.Run(() => _bwaService.UploadReference(txtRefPath.Text));

                UpdateStatus("参考序列上传完成 ✓", Color.Green);
                UpdateProgress("上传完成", 100);

                MessageBox.Show("参考序列上传成功", "成功",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Logger.LogError($"上传参考序列失败: {ex.Message}");
                MessageBox.Show($"上传失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("上传失败 ✗", Color.Red);
                UpdateProgress("上传失败", 0);
            }
        }

        // ========== 构建索引 ==========
        private async void btnBuildIndex_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_sshService.IsConnected)
                {
                    MessageBox.Show("请先连接服务器", "操作错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                UpdateStatus("构建BWA索引...", Color.Orange);
                UpdateProgress("正在构建索引...", 30);

                await System.Threading.Tasks.Task.Run(() => _bwaService.BuildIndex());

                UpdateStatus("BWA索引构建完成 ✓", Color.Green);
                UpdateProgress("索引构建完成", 100);

                MessageBox.Show("BWA索引构建成功", "成功",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Logger.LogError($"构建索引失败: {ex.Message}");
                MessageBox.Show($"构建失败: {ex.Message}\n请确保已上传参考序列文件", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("构建失败 ✗", Color.Red);
                UpdateProgress("构建失败", 0);
            }
        }

        // ========== BWA比对 ==========
        private async void btnBwaAlign_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_sshService.IsConnected)
                {
                    MessageBox.Show("请先连接服务器", "操作错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(txtFastqPath.Text))
                {
                    MessageBox.Show("请选择FASTQ文件", "文件错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(_currentSampleName))
                {
                    _currentSampleName = Path.GetFileNameWithoutExtension(txtFastqPath.Text);
                }

                UpdateStatus("开始BWA比对...", Color.Orange);
                UpdateProgress("正在比对...", 0);

                // 执行比对
                _localSamPath = await System.Threading.Tasks.Task.Run(() =>
                    _bwaService.AlignSingleEnd(txtFastqPath.Text, _currentSampleName, Config.Settings.DEFAULT_THREADS));

                UpdateStatus("BWA比对完成 ✓", Color.Green);
                UpdateProgress("比对完成", 100);

                // 启用后续处理按钮
                btnProcessSam.Enabled = true;
                btnVariantDetection.Enabled = true;

                MessageBox.Show($"BWA比对完成\nSAM文件: {_localSamPath}", "成功",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Logger.LogError($"BWA比对失败: {ex.Message}");
                MessageBox.Show($"比对失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("比对失败 ✗", Color.Red);
                UpdateProgress("比对失败", 0);
            }
        }

        // ========== 处理SAM文件 ==========
        private async void btnProcessSam_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_sshService.IsConnected)
                {
                    MessageBox.Show("请先连接服务器", "操作错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(_currentSampleName))
                {
                    MessageBox.Show("请先完成BWA比对", "操作错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                UpdateStatus("处理SAM文件...", Color.Orange);
                UpdateProgress("处理中...", 0);

                // 定义远程文件路径
                string remoteSam = $"{Config.Settings.REMOTE_WORK_DIR}/{_currentSampleName}.sam";
                string remoteBam = $"{Config.Settings.REMOTE_WORK_DIR}/{_currentSampleName}.bam";
                string remoteSortedBam = $"{Config.Settings.REMOTE_WORK_DIR}/{_currentSampleName}_sorted.bam";

                // 1. SAM转BAM
                UpdateProgress("SAM转BAM", 20);
                await System.Threading.Tasks.Task.Run(() => _samtoolsService.SamToBam(remoteSam, remoteBam));

                // 2. 排序BAM
                UpdateProgress("排序BAM", 40);
                await System.Threading.Tasks.Task.Run(() => _samtoolsService.SortBam(remoteBam, remoteSortedBam, Config.Settings.DEFAULT_THREADS));

                // 3. 建立BAM索引
                UpdateProgress("建立BAM索引", 60);
                await System.Threading.Tasks.Task.Run(() => _samtoolsService.IndexBam(remoteSortedBam));

                // 4. 生成Flagstat统计
                UpdateProgress("生成统计", 80);
                string remoteFlagstat = $"{Config.Settings.REMOTE_WORK_DIR}/{_currentSampleName}_sorted.flagstat";
                await System.Threading.Tasks.Task.Run(() => _samtoolsService.GenerateFlagstat(remoteSortedBam, remoteFlagstat));

                UpdateStatus("SAM处理完成 ✓", Color.Green);
                UpdateProgress("处理完成", 100);

                MessageBox.Show($"SAM文件处理完成:\n" +
                              $"1. SAM → BAM 转换\n" +
                              $"2. BAM 排序\n" +
                              $"3. BAM 索引建立\n" +
                              $"4. 生成Flagstat统计", "成功",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Logger.LogError($"SAM处理失败: {ex.Message}");
                MessageBox.Show($"处理失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("处理失败 ✗", Color.Red);
                UpdateProgress("处理失败", 0);
            }
        }

        // ========== 变异检测 ==========
        private async void btnVariantDetection_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_sshService.IsConnected)
                {
                    MessageBox.Show("请先连接服务器", "操作错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(_currentSampleName))
                {
                    MessageBox.Show("请先完成BWA比对", "操作错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(txtOutputPath.Text))
                {
                    MessageBox.Show("请设置输出文件夹", "输出错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                UpdateStatus("开始变异检测...", Color.Orange);
                UpdateProgress("开始检测", 0);

                // 确保输出目录存在
                Directory.CreateDirectory(txtOutputPath.Text);

                // 检查服务器文件
                UpdateProgress("检查服务器文件", 10);
                string fileCheck = await System.Threading.Tasks.Task.Run(() =>
                    _samtoolsService.CheckServerFiles(_currentSampleName));

                Utils.Logger.LogInfo($"服务器文件检查结果:\n{fileCheck}");

                // 执行完整的变异检测流程
                UpdateProgress("执行变异检测流程", 20);

                string vcfFile = await System.Threading.Tasks.Task.Run(() =>
                    _samtoolsService.RunFullVariantDetection(_currentSampleName, txtOutputPath.Text));

                UpdateStatus("变异检测完成 ✓", Color.Green);
                UpdateProgress("检测完成", 100);

                // 读取VCF文件统计变异数
                int variantCount = 0;
                int originalCount = 0;

                if (File.Exists(vcfFile))
                {
                    try
                    {
                        var lines = File.ReadAllLines(vcfFile);
                        variantCount = lines.Count(line => !string.IsNullOrEmpty(line) && !line.StartsWith("#"));

                        // 检查原始VCF文件
                        string originalVcf = vcfFile.Replace("_filtered.vcf", ".vcf");
                        if (File.Exists(originalVcf))
                        {
                            var originalLines = File.ReadAllLines(originalVcf);
                            originalCount = originalLines.Count(line => !string.IsNullOrEmpty(line) && !line.StartsWith("#"));
                        }
                    }
                    catch (Exception ex)
                    {
                        Utils.Logger.LogWarning($"统计变异数失败: {ex.Message}");
                    }
                }

                string message;
                if (originalCount > 0)
                {
                    message = $"✅ 变异检测完成\n\n" +
                             $"📊 统计信息:\n" +
                             $"原始变异数: {originalCount} 个\n" +
                             $"过滤后变异数: {variantCount} 个\n" +
                             $"过滤掉变异: {originalCount - variantCount} 个\n\n" +
                             $"📁 结果文件:\n{vcfFile}";
                }
                else
                {
                    message = $"✅ 变异检测完成\n\n" +
                             $"📊 统计信息:\n" +
                             $"检测到 {variantCount} 个变异\n\n" +
                             $"📁 结果文件:\n{vcfFile}";
                }

                MessageBox.Show(message, "成功",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Logger.LogError($"变异检测失败: {ex.Message}");

                // 提供更详细的错误信息
                string errorMsg = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMsg += $"\n内部错误: {ex.InnerException.Message}";
                }

                MessageBox.Show($"❌ 变异检测失败\n\n" +
                               $"错误信息:\n{errorMsg}\n\n" +
                               $"🔧 可能的原因:\n" +
                               $"1. BAM文件未正确排序或索引\n" +
                               $"2. 参考序列索引不完整\n" +
                               $"3. bcftools版本兼容性问题(1.19)\n" +
                               $"4. 服务器资源不足\n" +
                               $"5. 过滤表达式不适用您的VCF格式", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                UpdateStatus("检测失败 ✗", Color.Red);
                UpdateProgress("检测失败", 0);
            }
        }

        // ========== 查看日志 ==========
        private void btnViewLogs_Click(object sender, EventArgs e)
        {
            RefreshLogViewer();
        }

        private void btnClearLogs_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定要清空今日的日志吗？", "确认",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Logger.ClearTodayLog();
                txtLogViewer.Text = "日志已清空";
                UpdateStatusMessage("今日日志已清空");
            }
        }

        // ========== UI更新方法 ==========
        private void RefreshLogViewer()
        {
            try
            {
                string logContent = Logger.GetLogContent(100);
                txtLogViewer.Text = logContent;

                // 滚动到最后
                txtLogViewer.SelectionStart = txtLogViewer.TextLength;
                txtLogViewer.ScrollToCaret();
            }
            catch (Exception ex)
            {
                txtLogViewer.Text = $"读取日志失败: {ex.Message}";
            }
        }

        private void UpdateStatus(string message, Color color)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateStatus(message, color)));
                return;
            }

            lblStatus.Text = message;
            lblStatus.ForeColor = color;
        }

        private void UpdateProgress(string message, int value)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateProgress(message, value)));
                return;
            }

            lblProgress.Text = message;
            progressBar1.Value = Math.Max(0, Math.Min(100, value));
        }

        private void UpdateToolsStatus(bool toolsReady)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateToolsStatus(toolsReady)));
                return;
            }

            // 确保控件不为null
            if (lblToolsStatus != null)
            {
                lblToolsStatus.Text = toolsReady ? "✓ 工具就绪" : "✗ 工具缺失";
                lblToolsStatus.ForeColor = toolsReady ? Color.LimeGreen : Color.Red;
            }
        }

        private void UpdateConnectButton(bool enabled)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateConnectButton(enabled)));
                return;
            }

            if (btnConnect != null)
            {
                btnConnect.Enabled = enabled;
            }
        }

        private void UpdateOperationButtons(bool enabled)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateOperationButtons(enabled)));
                return;
            }

            // 确保控件不为null再启用/禁用
            if (btnUploadRef != null)
                btnUploadRef.Enabled = enabled;
            if (btnBuildIndex != null)
                btnBuildIndex.Enabled = enabled;
            if (btnBwaAlign != null)
                btnBwaAlign.Enabled = enabled;
            if (btnProcessSam != null)
                btnProcessSam.Enabled = enabled;
            if (btnVariantDetection != null)
                btnVariantDetection.Enabled = enabled;
            if (btnTestVariantCmd != null)
                btnTestVariantCmd.Enabled = enabled;
            if (btnViewVcfInfo != null)
                btnViewVcfInfo.Enabled = enabled;
            if (btnGenerateVisualization != null)
                btnGenerateVisualization.Enabled = enabled;
            if (btnManageModules != null)
                btnManageModules.Enabled = enabled;
            if (btnCleanRemoteTemp != null)
                btnCleanRemoteTemp.Enabled = enabled;
        }

        private void UpdateStatusMessage(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateStatusMessage(message)));
                return;
            }

            if (statusStrip != null && statusStrip.InvokeRequired)
            {
                statusStrip.Invoke(new Action(() => UpdateStatusMessage(message)));
                return;
            }

            if (statusStrip != null)
            {
                foreach (var item in statusStrip.Items)
                {
                    if (item is ToolStripStatusLabel label && label.Name == "lblStatusMessage")
                    {
                        label.Text = message;
                        break;
                    }
                }
            }
        }

        // ========== 窗体关闭 ==========
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                // 1. 询问是否退出程序
                DialogResult exitResult = MessageBox.Show(
                    "确定要退出NGS BWA Runner程序吗？",
                    "确认退出",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (exitResult != DialogResult.Yes)
                {
                    e.Cancel = true; // 取消关闭
                    return;
                }

                // 2. 检查是否连接了服务器
                bool isConnected = _sshService?.IsConnected ?? false;

                if (isConnected)
                {
                    // 询问是否清理服务器临时文件夹
                    DialogResult cleanupResult = MessageBox.Show(
                        "是否要清空服务器端的临时文件夹？\n\n" +
                        $"目录: {Config.Settings.REMOTE_WORK_DIR}\n\n" +
                        "建议：\n" +
                        "• 如果不再需要分析数据，选择【是】\n" +
                        "• 如果希望保留工具和中间文件，选择【否】\n" +
                        "• 如果只想退出不清理，选择【取消】",
                        "清理服务器临时文件夹",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);

                    if (cleanupResult == DialogResult.Yes)
                    {
                        // 执行清理
                        UpdateStatus("正在清理服务器临时文件夹...", Color.Orange);

                        try
                        {
                            bool cleanupSuccess = _remoteCleanupService?.DeleteRemoteTempDirectory() ?? false;

                            if (cleanupSuccess)
                            {
                                Utils.Logger.LogInfo("程序退出前已清理服务器临时文件夹");
                                MessageBox.Show("服务器临时文件夹已清理",
                                    "清理完成",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                            }
                            else
                            {
                                Utils.Logger.LogWarning("程序退出前清理服务器临时文件夹失败");
                            }
                        }
                        catch (Exception ex)
                        {
                            Utils.Logger.LogError($"清理服务器临时文件夹失败: {ex.Message}");
                        }
                    }
                    else if (cleanupResult == DialogResult.Cancel)
                    {
                        e.Cancel = true; // 取消关闭
                        return;
                    }
                }

                // 3. 执行原有的清理逻辑
                UpdateStatus("正在关闭程序...", Color.Orange);

                // 断开SSH连接
                _sshService?.Disconnect();
                _sshService?.Dispose();

                // 清理本地临时文件
                _fileService?.CleanTempFiles();

                Logger.LogInfo("应用程序关闭");

                UpdateStatus("程序已关闭", Color.Gray);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"关闭时出错: {ex.Message}");
                Utils.Logger.LogError($"关闭时出错: {ex.Message}");
            }
        }

        // 在 MainForm 中添加测试方法
        private async void btnTestVariantCmd_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_sshService.IsConnected)
                {
                    MessageBox.Show("请先连接服务器", "操作错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                UpdateStatus("测试变异检测命令...", Color.Orange);

                bool testPassed = await System.Threading.Tasks.Task.Run(() =>
                    _samtoolsService.TestVariantCalling());

                if (testPassed)
                {
                    UpdateStatus("测试通过 ✓", Color.Green);
                    MessageBox.Show("变异检测命令测试通过！", "测试成功",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    UpdateStatus("测试失败 ✗", Color.Red);
                    MessageBox.Show("变异检测命令测试失败，请检查:\n" +
                                   "1. bcftools版本是否为1.19+\n" +
                                   "2. 参考序列是否已正确索引\n" +
                                   "3. 所有工具文件是否已上传", "测试失败",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"测试失败: {ex.Message}");
                MessageBox.Show($"测试失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("测试失败 ✗", Color.Red);
            }
        }

        // 查看VCF文件信息
        private async void btnViewVcfInfo_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_sshService.IsConnected)
                {
                    MessageBox.Show("请先连接服务器", "操作错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(_currentSampleName))
                {
                    MessageBox.Show("没有当前样本信息", "操作错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string remoteVcf = $"{Config.Settings.REMOTE_WORK_DIR}/{_currentSampleName}.vcf";

                // 查看VCF头部信息
                string headerCmd = $"cd {Config.Settings.REMOTE_WORK_DIR} && " +
                                  $"./bcftools view -h {Path.GetFileName(remoteVcf)} | head -30";

                UpdateStatus("正在获取VCF信息...", Color.Orange);

                string header = await System.Threading.Tasks.Task.Run(() =>
                    _sshService.ExecuteCommand(headerCmd));

                // 统计变异数
                string countCmd = $"cd {Config.Settings.REMOTE_WORK_DIR} && " +
                                 $"grep -v '^#' {Path.GetFileName(remoteVcf)} | wc -l";

                string count = await System.Threading.Tasks.Task.Run(() =>
                    _sshService.ExecuteCommand(countCmd));

                UpdateStatus("VCF信息获取完成 ✓", Color.Green);

                MessageBox.Show($"VCF文件信息:\n\n" +
                               $"变异数量: {count.Trim()} 个\n\n" +
                               $"文件头部信息:\n{header}",
                               "VCF文件信息",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"获取VCF信息失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("获取失败 ✗", Color.Red);
            }
        }

        // ========== 可视化相关方法 ==========

        // 添加可视化初始化方法
        private void InitializeVisualization()
        {
            try
            {
                if (!_visualizationInitialized)
                {
                    _visualizationManager = new VisualizationManager();
                    _visualizationManager.ModuleStarted += OnVisualizationModuleStarted;
                    _visualizationManager.ModuleCompleted += OnVisualizationModuleCompleted;
                    _visualizationManager.ModuleFailed += OnVisualizationModuleFailed;

                    // 显示发现的模块
                    var modules = _visualizationManager.GetModuleInfos();
                    Utils.Logger.LogInfo($"可视化系统初始化完成，发现 {modules.Count} 个模块");

                    // 更新UI显示模块信息
                    UpdateVisualizationModulesList();

                    _visualizationInitialized = true;
                }
            }
            catch (Exception ex)
            {
                Utils.Logger.LogError($"初始化可视化系统失败: {ex.Message}");
            }
        }

        // 在适当位置添加"生成可视化"按钮（可以在原界面上添加）
        private void InitializeVisualizationButton()
        {
            var btnGenerateVisualization = new Button
            {
                Text = "生成可视化图表",
                Location = new Point(20, 400),
                Size = new Size(150, 40),
                Enabled = false
            };

            btnGenerateVisualization.Click += BtnGenerateVisualization_Click;
        }

        // 在连接成功后调用
        private void InitializeVisualizationAndButtons()
        {
            try
            {
                InitializeVisualization();

                // 启用第二行按钮
                this.Invoke((Action)(() =>
                {
                    if (btnTestVariantCmd != null)
                        btnTestVariantCmd.Enabled = true;
                    if (btnViewVcfInfo != null)
                        btnViewVcfInfo.Enabled = true;
                    if (btnGenerateVisualization != null)
                        btnGenerateVisualization.Enabled = true;
                    if (btnManageModules != null)
                        btnManageModules.Enabled = true;

                    // 如果可视化模块存在，启用可视化选项卡按钮
                    if (_visualizationInitialized && _visualizationManager != null && _visualizationManager.GetModuleInfos().Count > 0)
                    {
                        if (btnRunAllVisualizations != null)
                            btnRunAllVisualizations.Enabled = true;
                        if (btnConfigureModules != null)
                            btnConfigureModules.Enabled = true;
                        if (btnRefreshModules != null)
                            btnRefreshModules.Enabled = true;

                        // 更新模块列表显示
                        UpdateVisualizationModulesList();
                    }
                }));
            }
            catch (Exception ex)
            {
                Utils.Logger.LogError($"初始化可视化按钮失败: {ex.Message}");
            }
        }

        // 更新模块列表
        private void UpdateVisualizationModulesList()
        {
            try
            {
                if (_visualizationManager != null)
                {
                    var modules = _visualizationManager.GetModuleInfos();
                    this.Invoke((Action)(() =>
                    {
                        if (lstModuleList != null)
                        {
                            lstModuleList.Items.Clear();
                            foreach (var module in modules)
                            {
                                lstModuleList.Items.Add($"{module.Name} - {module.Description}");
                            }
                        }
                        if (lblLoadedModules != null)
                        {
                            lblLoadedModules.Text = $"{modules.Count}个模块已加载";
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                Utils.Logger.LogError($"更新模块列表失败: {ex.Message}");
            }
        }

        // 可视化按钮点击事件
        private async void BtnGenerateVisualization_Click(object sender, EventArgs e)
        {
            try
            {
                Console.WriteLine("=== 调试：开始执行可视化按钮点击 ===");
                Utils.Logger.LogInfo("可视化按钮被点击");

                // 显示简单的消息框确认按钮事件被触发
                MessageBox.Show("调试：可视化按钮被点击，开始执行...", "调试信息",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 确保可视化系统已初始化
                Console.WriteLine("调试：调用 InitializeVisualization()");
                InitializeVisualization();

                if (_visualizationManager == null)
                {
                    MessageBox.Show("可视化管理器未初始化，请检查连接状态", "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Utils.Logger.LogError("_visualizationManager 为 null");
                    return;
                }

                Console.WriteLine($"调试：_visualizationManager 状态: {_visualizationManager != null}");

                // 获取当前处理的文件
                Console.WriteLine("调试：获取文件路径");
                string samFile = GetCurrentSamFile();
                string vcfFile = GetCurrentVcfFile();
                string filteredVcf = GetFilteredVcfFile();

                Console.WriteLine($"调试：SAM文件路径: {samFile}");
                Console.WriteLine($"调试：VCF文件路径: {vcfFile}");
                Console.WriteLine($"调试：过滤VCF文件路径: {filteredVcf}");
                Console.WriteLine($"调试：SAM文件存在: {File.Exists(samFile)}");
                Console.WriteLine($"调试：当前样本名: {_currentSampleName}");
                Console.WriteLine($"调试：输出目录: {txtOutputPath.Text}");

                if (string.IsNullOrEmpty(samFile) || !File.Exists(samFile))
                {
                    string errorMsg = $"请先完成比对并生成SAM文件\n当前SAM文件: {samFile ?? "未找到"}";
                    Console.WriteLine($"调试：SAM文件检查失败 - {errorMsg}");
                    MessageBox.Show(errorMsg, "提示",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 创建可视化上下文
                Console.WriteLine("调试：创建VisualizationContext");
                var context = new Visualization.Models.VisualizationContext
                {
                    SamFilePath = samFile,
                    VcfFilePath = vcfFile,
                    FilteredVcfFilePath = filteredVcf,
                    SampleName = _currentSampleName,
                    OutputDirectory = txtOutputPath.Text,
                    CombinePdf = true,
                    HighResolution = true,
                    ProgressReporter = (message, progress) =>
                    {
                        Console.WriteLine($"调试进度: {message}, {progress}%");
                        this.Invoke((Action)(() =>
                        {
                            lblStatus.Text = message;
                            progressBar1.Value = Math.Max(0, Math.Min(100, progress));
                        }));
                    }
                };

                // 显示进度
                Console.WriteLine("调试：更新状态为'开始生成可视化图表...'");
                UpdateStatus("开始生成可视化图表...", Color.Orange);

                // 执行所有可视化模块
                Console.WriteLine("调试：调用 GenerateAllVisualizationsAsync");
                var results = await _visualizationManager.GenerateAllVisualizationsAsync(context);

                // 统计结果
                int successCount = results.Count(r => r.Success);
                int totalCount = results.Count;

                Console.WriteLine($"调试：执行完成，成功 {successCount}/{totalCount}");

                UpdateStatus($"可视化完成: {successCount}/{totalCount} 个模块成功",
                    successCount == totalCount ? Color.Green : Color.Orange);

                // 显示结果
                string resultMessage = $"可视化图表生成完成！\n\n";
                resultMessage += $"成功: {successCount} 个模块\n";
                resultMessage += $"失败: {totalCount - successCount} 个模块\n\n";
                resultMessage += $"输出目录: {context.OutputDirectory}";

                if (totalCount - successCount > 0)
                {
                    resultMessage += "\n\n失败的模块:\n";
                    foreach (var result in results.Where(r => !r.Success))
                    {
                        resultMessage += $"  - {result.Message}\n";
                    }
                }

                MessageBox.Show(resultMessage, "可视化完成",
                    MessageBoxButtons.OK, successCount == totalCount ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

                Console.WriteLine("=== 调试：可视化按钮点击执行完成 ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"调试异常: {ex}");
                Utils.Logger.LogError($"生成可视化图表失败: {ex.Message}");
                MessageBox.Show($"生成可视化图表失败: {ex.Message}\n堆栈跟踪:\n{ex.StackTrace}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("可视化失败", Color.Red);
            }
        }

        // 可视化模块事件处理
        private void OnVisualizationModuleStarted(object sender, string moduleId)
        {
            this.Invoke((Action)(() =>
            {
                if (txtLogViewer != null)
                {
                    txtLogViewer.AppendText($"[{DateTime.Now:HH:mm:ss}] 开始执行模块: {moduleId}\n");
                }
            }));
        }

        private void OnVisualizationModuleCompleted(object sender, string moduleId)
        {
            this.Invoke((Action)(() =>
            {
                if (txtLogViewer != null)
                {
                    txtLogViewer.AppendText($"[{DateTime.Now:HH:mm:ss}] ✓ 模块完成: {moduleId}\n");
                }
            }));
        }

        private void OnVisualizationModuleFailed(object sender, string moduleId)
        {
            this.Invoke((Action)(() =>
            {
                if (txtLogViewer != null)
                {
                    txtLogViewer.AppendText($"[{DateTime.Now:HH:mm:ss}] ✗ 模块失败: {moduleId}\n");
                }
            }));
        }

        // 工具方法：获取当前处理的文件
        private string GetCurrentSamFile()
        {
            // 根据你的逻辑返回当前SAM文件路径
            // 例如：如果 _localSamPath 变量存储了SAM文件路径
            return _localSamPath;
        }

        private string GetCurrentVcfFile()
        {
            // 根据你的逻辑返回VCF文件路径
            return Path.Combine(txtOutputPath.Text, $"{_currentSampleName}.vcf");
        }

        private string GetFilteredVcfFile()
        {
            // 根据你的逻辑返回过滤后的VCF文件路径
            return Path.Combine(txtOutputPath.Text, $"{_currentSampleName}_filtered.vcf");
        }

        // 在适当位置调用初始化方法（例如在连接成功后）
        private void EnableVisualizationButton()
        {
            // 启用可视化按钮
            var btnVisualization = this.Controls.Find("btnGenerateVisualization", true).FirstOrDefault() as Button;
            if (btnVisualization != null)
            {
                btnVisualization.Enabled = true;
            }
        }

        private void btnManageModules_Click(object sender, EventArgs e)
        {
            MessageBox.Show("管理模块功能待实现", "提示",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnRunAllVisualizations_Click(object sender, EventArgs e)
        {
            MessageBox.Show("运行所有可视化功能待实现", "提示",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnConfigureModules_Click(object sender, EventArgs e)
        {
            MessageBox.Show("配置模块功能待实现", "提示",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnRefreshModules_Click(object sender, EventArgs e)
        {
            MessageBox.Show("刷新模块功能待实现", "提示",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }
    }
}