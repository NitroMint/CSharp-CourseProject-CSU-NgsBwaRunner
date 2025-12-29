using System;

namespace NgsBwaRunner.Services
{
    /// <summary>
    /// 远程服务器清理服务
    /// </summary>
    public class RemoteCleanupService
    {
        private readonly SshService _sshService;

        public RemoteCleanupService(SshService sshService)
        {
            _sshService = sshService ?? throw new ArgumentNullException(nameof(sshService));
        }

        /// <summary>
        /// 删除远程临时目录（安全版本）
        /// </summary>
        public bool DeleteRemoteTempDirectory()
        {
            try
            {
                if (!_sshService.IsConnected)
                {
                    Utils.Logger.LogWarning("SSH未连接，无法清理远程目录");
                    return false;
                }

                Utils.Logger.LogInfo("开始清理远程临时目录...");

                // 1. 先检查目录是否存在
                string checkCmd = $"if [ -d \"{Config.Settings.REMOTE_WORK_DIR}\" ]; then echo 'exists'; else echo 'not_exists'; fi";
                string result = _sshService.ExecuteCommand(checkCmd).Trim();

                if (result != "exists")
                {
                    Utils.Logger.LogInfo("远程临时目录不存在，无需清理");
                    return true;
                }

                // 2. 查看目录内容（用于日志）
                string listCmd = $"ls -la \"{Config.Settings.REMOTE_WORK_DIR}\"";
                string dirContents = _sshService.ExecuteCommand(listCmd);
                Utils.Logger.LogInfo($"远程目录内容:\n{dirContents}");

                // 3. 安全删除目录（使用rm -rf命令）
                string deleteCmd = $"rm -rf \"{Config.Settings.REMOTE_WORK_DIR}\"";
                Utils.Logger.LogInfo($"执行删除命令: {deleteCmd}");
                _sshService.ExecuteCommand(deleteCmd);

                // 4. 验证删除是否成功
                string verifyCmd = $"if [ ! -d \"{Config.Settings.REMOTE_WORK_DIR}\" ]; then echo 'deleted'; else echo 'still_exists'; fi";
                string verifyResult = _sshService.ExecuteCommand(verifyCmd).Trim();

                if (verifyResult == "deleted")
                {
                    Utils.Logger.LogSuccess("远程临时目录删除成功");
                    return true;
                }
                else
                {
                    Utils.Logger.LogError("远程临时目录删除失败，目录仍然存在");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Utils.Logger.LogError($"清理远程临时目录失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取远程目录大小信息
        /// </summary>
        public string GetRemoteDirectoryInfo()
        {
            try
            {
                if (!_sshService.IsConnected)
                {
                    return "SSH未连接";
                }

                string infoCmd = $"if [ -d \"{Config.Settings.REMOTE_WORK_DIR}\" ]; then " +
                               $"echo '目录存在'; " +
                               $"du -sh \"{Config.Settings.REMOTE_WORK_DIR}\"; " +
                               $"echo '内容列表:'; " +
                               $"ls -lh \"{Config.Settings.REMOTE_WORK_DIR}\" 2>/dev/null || echo '目录为空'; " +
                               $"else echo '目录不存在'; fi";

                string result = _sshService.ExecuteCommand(infoCmd);
                return result;
            }
            catch (Exception ex)
            {
                return $"获取目录信息失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 清理部分临时文件（保留工具）
        /// </summary>
        public bool CleanTempFilesOnly()
        {
            try
            {
                if (!_sshService.IsConnected)
                {
                    return false;
                }

                Utils.Logger.LogInfo("开始清理临时文件（保留工具）...");

                // 清理SAM/BAM/VCF等临时文件，保留工具文件
                string cleanCmd = $"cd \"{Config.Settings.REMOTE_WORK_DIR}\" && " +
                                 $"rm -f *.sam *.bam *.vcf *.bcf *.flagstat *.mpileup *.fq *.fa.* 2>/dev/null || true";

                _sshService.ExecuteCommand(cleanCmd);

                Utils.Logger.LogSuccess("临时文件清理完成");
                return true;
            }
            catch (Exception ex)
            {
                Utils.Logger.LogError($"清理临时文件失败: {ex.Message}");
                return false;
            }
        }
    }
}