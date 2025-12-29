using System;
using System.IO;

namespace NgsBwaRunner.Services
{
    public class BwaService
    {
        private readonly SshService _sshService;
        private readonly FileService _fileService;

        public BwaService(SshService sshService)
        {
            _sshService = sshService;
            _fileService = new FileService();
        }

        // 上传工具到服务器
        public void UploadTools()
        {
            if (!_sshService.IsConnected)
            {
                throw new Exception("SSH未连接");
            }

            // 检查本地工具
            if (!_fileService.CheckLocalTools())
            {
                throw new Exception("本地工具文件缺失");
            }

            Utils.Logger.LogInfo("开始上传BWA工具到服务器");

            try
            {
                // 创建远程工作目录
                string createDirCmd = string.Format(Config.Commands.System.CREATE_DIRECTORY,
                    Config.Settings.REMOTE_WORK_DIR);
                _sshService.ExecuteCommand(createDirCmd);

                // 上传BWA工具
                bool bwaUploaded = _sshService.UploadFile(
                    Config.Settings.LOCAL_BWA_PATH,
                    $"{Config.Settings.REMOTE_WORK_DIR}/bwa");

                if (!bwaUploaded)
                    throw new Exception("BWA工具上传失败");

                // 给BWA添加执行权限
                string chmodCmd = $"chmod +x {Config.Settings.REMOTE_WORK_DIR}/bwa";
                _sshService.ExecuteCommand(chmodCmd);

                Utils.Logger.LogSuccess("BWA工具上传完成");
            }
            catch (Exception ex)
            {
                Utils.Logger.LogError($"BWA工具上传失败: {ex.Message}");
                throw;
            }
        }

        // 上传参考序列
        public void UploadReference(string localRefPath, string remoteFileName = "ref.fa")
        {
            if (!_sshService.IsConnected)
            {
                throw new Exception("SSH未连接");
            }

            if (!File.Exists(localRefPath))
            {
                throw new FileNotFoundException($"参考序列文件不存在: {localRefPath}");
            }

            Utils.Logger.LogInfo($"上传参考序列: {localRefPath}");

            try
            {
                // 上传文件
                bool uploaded = _sshService.UploadFile(
                    localRefPath,
                    $"{Config.Settings.REMOTE_WORK_DIR}/{remoteFileName}");

                if (!uploaded)
                    throw new Exception("参考序列上传失败");

                Utils.Logger.LogSuccess("参考序列上传完成");
            }
            catch (Exception ex)
            {
                Utils.Logger.LogError($"参考序列上传失败: {ex.Message}");
                throw;
            }
        }

        // 构建BWA索引
        public void BuildIndex()
        {
            if (!_sshService.IsConnected)
            {
                throw new Exception("SSH未连接");
            }

            Utils.Logger.LogInfo("开始构建BWA索引");

            string bwaIndexCmd = string.Format(Config.Commands.Bwa.INDEX, Config.Settings.REMOTE_WORK_DIR);

            try
            {
                _sshService.ExecuteCommand(bwaIndexCmd);
                Utils.Logger.LogSuccess("BWA索引构建完成");
            }
            catch (Exception ex)
            {
                Utils.Logger.LogError($"BWA索引构建失败: {ex.Message}");
                throw;
            }
        }

        // 执行BWA比对（单端测序）
        // 修改 BwaService.AlignSingleEnd 方法
        public string AlignSingleEnd(string localFastqPath, string sampleName, int threads = 4)
        {
            if (!_sshService.IsConnected)
            {
                throw new Exception("SSH未连接");
            }

            if (!File.Exists(localFastqPath))
            {
                throw new FileNotFoundException($"FASTQ文件不存在: {localFastqPath}");
            }

            Utils.Logger.LogInfo($"开始BWA比对（单端）: {localFastqPath}");

            try
            {
                // 上传FASTQ文件
                string remoteFastq = $"{Config.Settings.REMOTE_WORK_DIR}/{sampleName}.fq";
                Utils.Logger.LogInfo($"上传FASTQ文件: {remoteFastq}");

                bool uploaded = _sshService.UploadFile(localFastqPath, remoteFastq);
                if (!uploaded)
                    throw new Exception("FASTQ文件上传失败");

                // 执行BWA比对
                string remoteSam = $"{Config.Settings.REMOTE_WORK_DIR}/{sampleName}.sam";
                string alignCmd = string.Format(Config.Commands.Bwa.ALIGN_SINGLE,
                    Config.Settings.REMOTE_WORK_DIR,
                    threads,
                    Config.Settings.DEFAULT_READ_GROUP.Replace("sample1", sampleName),
                    $"{sampleName}.fq",
                    $"{sampleName}.sam");

                Utils.Logger.LogInfo($"执行BWA比对命令: {alignCmd}");
                _sshService.ExecuteCommand(alignCmd);

                // 检查远程SAM文件是否生成
                string checkCmd = $"ls -la {remoteSam}";
                string checkResult = _sshService.ExecuteCommand(checkCmd);
                Utils.Logger.LogInfo($"远程SAM文件检查结果: {checkResult}");

                // 下载SAM文件到本地 - 使用桌面输出目录
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string outputDir = Path.Combine(desktop, $"NGS_Output_{DateTime.Now:yyyyMMdd}");
                Directory.CreateDirectory(outputDir);

                string localSam = Path.Combine(outputDir, $"{sampleName}.sam");

                Utils.Logger.LogInfo($"下载SAM文件到本地: {localSam}");

                bool downloaded = _sshService.DownloadFile(remoteSam, localSam);

                if (downloaded && File.Exists(localSam))
                {
                    long fileSize = new FileInfo(localSam).Length;
                    Utils.Logger.LogSuccess($"BWA比对完成，生成SAM文件: {localSam} ({fileSize} bytes)");
                    return localSam;
                }
                else
                {
                    throw new Exception($"SAM文件下载失败或文件不存在: {localSam}");
                }
            }
            catch (Exception ex)
            {
                Utils.Logger.LogError($"BWA比对失败: {ex.Message}");
                throw;
            }
        }
    }
}