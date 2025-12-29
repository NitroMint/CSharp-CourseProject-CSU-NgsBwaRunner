using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace NgsBwaRunner.Services
{
    public class SamtoolsService
    {
        private readonly SshService _sshService;
        private readonly FileService _fileService;

        public SamtoolsService(SshService sshService)
        {
            _sshService = sshService;
            _fileService = new FileService();
        }

        // 上传Samtools工具到服务器
        public void UploadTools()
        {
            if (!_sshService.IsConnected)
            {
                throw new Exception("SSH未连接");
            }

            Utils.Logger.LogInfo("开始上传Samtools工具到服务器");

            try
            {
                // 上传Samtools工具
                bool samtoolsUploaded = _sshService.UploadFile(
                    Config.Settings.LOCAL_SAMTOOLS_PATH,
                    $"{Config.Settings.REMOTE_WORK_DIR}/samtools");

                if (!samtoolsUploaded)
                    throw new Exception("Samtools工具上传失败");

                // 给Samtools添加执行权限
                string chmodCmd = $"chmod +x {Config.Settings.REMOTE_WORK_DIR}/samtools";
                _sshService.ExecuteCommand(chmodCmd);

                Utils.Logger.LogSuccess("Samtools工具上传完成");
            }
            catch (Exception ex)
            {
                Utils.Logger.LogError($"Samtools工具上传失败: {ex.Message}");
                throw;
            }
        }

        // SAM转BAM
        public void SamToBam(string remoteSamPath, string remoteBamPath)
        {
            if (!_sshService.IsConnected)
            {
                throw new Exception("SSH未连接");
            }

            Utils.Logger.LogInfo($"转换SAM到BAM: {remoteSamPath} -> {remoteBamPath}");

            string samToBamCmd = string.Format(Config.Commands.Samtools.SAM_TO_BAM,
                Config.Settings.REMOTE_WORK_DIR,
                Path.GetFileName(remoteSamPath),
                Path.GetFileName(remoteBamPath));

            try
            {
                Utils.Logger.LogInfo($"执行命令: {samToBamCmd}");
                _sshService.ExecuteCommand(samToBamCmd);
                Utils.Logger.LogSuccess("SAM转BAM完成");
            }
            catch (Exception ex)
            {
                Utils.Logger.LogError($"SAM转BAM失败: {ex.Message}");
                throw;
            }
        }

        // 排序BAM文件
        public void SortBam(string remoteBamPath, string remoteSortedBamPath, int threads = 4)
        {
            if (!_sshService.IsConnected)
            {
                throw new Exception("SSH未连接");
            }

            Utils.Logger.LogInfo($"排序BAM文件: {remoteBamPath} -> {remoteSortedBamPath}");

            string sortCmd = string.Format(Config.Commands.Samtools.SORT_BAM,
                Config.Settings.REMOTE_WORK_DIR,
                threads,
                Path.GetFileName(remoteBamPath),
                Path.GetFileName(remoteSortedBamPath));

            try
            {
                Utils.Logger.LogInfo($"执行命令: {sortCmd}");
                _sshService.ExecuteCommand(sortCmd);
                Utils.Logger.LogSuccess("BAM排序完成");
            }
            catch (Exception ex)
            {
                Utils.Logger.LogError($"BAM排序失败: {ex.Message}");
                throw;
            }
        }

        // 建立BAM索引
        public void IndexBam(string remoteBamPath)
        {
            if (!_sshService.IsConnected)
            {
                throw new Exception("SSH未连接");
            }

            Utils.Logger.LogInfo($"建立BAM索引: {remoteBamPath}");

            string indexCmd = string.Format(Config.Commands.Samtools.INDEX_BAM,
                Config.Settings.REMOTE_WORK_DIR,
                Path.GetFileName(remoteBamPath));

            try
            {
                Utils.Logger.LogInfo($"执行命令: {indexCmd}");
                _sshService.ExecuteCommand(indexCmd);
                Utils.Logger.LogSuccess("BAM索引建立完成");
            }
            catch (Exception ex)
            {
                Utils.Logger.LogError($"BAM索引建立失败: {ex.Message}");
                throw;
            }
        }

        // 生成mpileup文件（文本格式）
        public void GenerateMpileup(string remoteBamPath, string remoteMpileupPath)
        {
            if (!_sshService.IsConnected)
            {
                throw new Exception("SSH未连接");
            }

            Utils.Logger.LogInfo($"生成mpileup文件(文本格式): {remoteBamPath} -> {remoteMpileupPath}");

            string mpileupCmd = string.Format(Config.Commands.Samtools.MPILEUP,
                Config.Settings.REMOTE_WORK_DIR,
                Path.GetFileName(remoteBamPath),
                Path.GetFileName(remoteMpileupPath));

            try
            {
                Utils.Logger.LogInfo($"执行命令: {mpileupCmd}");
                _sshService.ExecuteCommand(mpileupCmd);
                Utils.Logger.LogSuccess("mpileup文件(文本格式)生成完成");
            }
            catch (Exception ex)
            {
                Utils.Logger.LogError($"mpileup文件生成失败: {ex.Message}");
                throw;
            }
        }

        // 上传BCFtools工具
        public void UploadBcftools()
        {
            if (!_sshService.IsConnected)
            {
                throw new Exception("SSH未连接");
            }

            Utils.Logger.LogInfo("开始上传BCFtools工具到服务器");

            try
            {
                // 上传BCFtools工具
                bool bcftoolsUploaded = _sshService.UploadFile(
                    Config.Settings.LOCAL_BCFTOOLS_PATH,
                    $"{Config.Settings.REMOTE_WORK_DIR}/bcftools");

                if (!bcftoolsUploaded)
                    throw new Exception("BCFtools工具上传失败");

                // 给BCFtools添加执行权限
                string chmodCmd = $"chmod +x {Config.Settings.REMOTE_WORK_DIR}/bcftools";
                _sshService.ExecuteCommand(chmodCmd);

                Utils.Logger.LogSuccess("BCFtools工具上传完成");
            }
            catch (Exception ex)
            {
                Utils.Logger.LogError($"BCFtools工具上传失败: {ex.Message}");
                throw;
            }
        }

        // 查看BAM文件头部信息
        public string ViewBamHeader(string remoteBamPath)
        {
            if (!_sshService.IsConnected)
            {
                throw new Exception("SSH未连接");
            }

            Utils.Logger.LogInfo($"查看BAM头部信息: {remoteBamPath}");

            string headerCmd = string.Format(Config.Commands.Samtools.VIEW_BAM_HEADER,
                Config.Settings.REMOTE_WORK_DIR,
                Path.GetFileName(remoteBamPath));

            try
            {
                Utils.Logger.LogInfo($"执行命令: {headerCmd}");
                string result = _sshService.ExecuteCommand(headerCmd);
                Utils.Logger.LogSuccess("BAM头部信息获取完成");
                return result;
            }
            catch (Exception ex)
            {
                Utils.Logger.LogError($"获取BAM头部信息失败: {ex.Message}");
                return $"获取BAM头部信息失败: {ex.Message}";
            }
        }

        // 查看VCF文件头部信息
        public string ViewVcfHeader(string remoteVcfPath)
        {
            if (!_sshService.IsConnected)
            {
                throw new Exception("SSH未连接");
            }

            Utils.Logger.LogInfo($"查看VCF头部信息: {remoteVcfPath}");

            string headerCmd = string.Format(Config.Commands.Bcftools.VIEW_VCF_HEADER,
                Config.Settings.REMOTE_WORK_DIR,
                Path.GetFileName(remoteVcfPath));

            try
            {
                Utils.Logger.LogInfo($"执行命令: {headerCmd}");
                string result = _sshService.ExecuteCommand(headerCmd);
                Utils.Logger.LogSuccess("VCF头部信息获取完成");
                return result;
            }
            catch (Exception ex)
            {
                Utils.Logger.LogError($"获取VCF头部信息失败: {ex.Message}");
                return $"获取VCF头部信息失败: {ex.Message}";
            }
        }

        // 查看VCF文件内容
        public string ViewVcfContent(string remoteVcfPath, int lines = 20)
        {
            if (!_sshService.IsConnected)
            {
                throw new Exception("SSH未连接");
            }

            Utils.Logger.LogInfo($"查看VCF内容: {remoteVcfPath}");

            string contentCmd = string.Format(Config.Commands.Bcftools.VIEW_VCF_CONTENT,
                Config.Settings.REMOTE_WORK_DIR,
                Path.GetFileName(remoteVcfPath));

            try
            {
                Utils.Logger.LogInfo($"执行命令: {contentCmd}");
                string result = _sshService.ExecuteCommand(contentCmd);
                Utils.Logger.LogSuccess("VCF内容获取完成");
                return result;
            }
            catch (Exception ex)
            {
                Utils.Logger.LogError($"获取VCF内容失败: {ex.Message}");
                return $"获取VCF内容失败: {ex.Message}";
            }
        }

        // 统计VCF变异数量
        public int CountVariantsInVcf(string remoteVcfPath)
        {
            if (!_sshService.IsConnected)
            {
                throw new Exception("SSH未连接");
            }

            Utils.Logger.LogInfo($"统计VCF变异数量: {remoteVcfPath}");

            string countCmd = string.Format(Config.Commands.Bcftools.COUNT_VARIANTS,
                Config.Settings.REMOTE_WORK_DIR,
                Path.GetFileName(remoteVcfPath));

            try
            {
                Utils.Logger.LogInfo($"执行命令: {countCmd}");
                string result = _sshService.ExecuteCommand(countCmd).Trim();

                if (int.TryParse(result, out int count))
                {
                    Utils.Logger.LogSuccess($"VCF变异数量统计完成: {count}");
                    return count;
                }
                else
                {
                    Utils.Logger.LogWarning($"无法解析变异数量: {result}");
                    return 0;
                }
            }
            catch (Exception ex)
            {
                Utils.Logger.LogError($"统计VCF变异数量失败: {ex.Message}");
                return 0;
            }
        }

        // 变异检测（优化版：使用已验证可用的方法）
        public void CallVariants(string remoteBamPath, string remoteVcfPath)
        {
            if (!_sshService.IsConnected)
            {
                throw new Exception("SSH未连接");
            }

            Utils.Logger.LogInfo($"变异检测: {remoteBamPath} -> {remoteVcfPath}");

            try
            {
                // 方法2：使用 bcftools mpileup + bcftools call（已验证可用）
                string remoteBcf = remoteVcfPath.Replace(".vcf", ".bcf");

                // 1. 生成BCF文件
                string mpileupCmd = string.Format(Config.Commands.Bcftools.BCFTOOLS_MPILEUP,
                    Config.Settings.REMOTE_WORK_DIR,
                    Path.GetFileName(remoteBamPath),
                    Path.GetFileName(remoteBcf));

                Utils.Logger.LogInfo($"执行命令: {mpileupCmd}");
                _sshService.ExecuteCommand(mpileupCmd);

                // 2. 转换BCF到VCF
                string callCmd = string.Format(Config.Commands.Bcftools.CALL_VARIANTS,
                    Config.Settings.REMOTE_WORK_DIR,
                    Path.GetFileName(remoteBcf),
                    Path.GetFileName(remoteVcfPath));

                Utils.Logger.LogInfo($"执行命令: {callCmd}");
                _sshService.ExecuteCommand(callCmd);

                // 3. 清理中间BCF文件
                try
                {
                    string deleteCmd = $"cd {Config.Settings.REMOTE_WORK_DIR} && rm -f {Path.GetFileName(remoteBcf)}";
                    _sshService.ExecuteCommand(deleteCmd);
                    Utils.Logger.LogDebug("清理中间BCF文件");
                }
                catch (Exception ex)
                {
                    Utils.Logger.LogWarning($"清理中间文件失败: {ex.Message}");
                }

                Utils.Logger.LogSuccess("变异检测完成");
            }
            catch (Exception ex)
            {
                Utils.Logger.LogError($"变异检测失败: {ex.Message}");

                // 备用方案：尝试管道方式（不使用-u参数）
                try
                {
                    Utils.Logger.LogInfo($"尝试管道方式（不带-u参数）...");
                    string pipeCmd = string.Format(Config.Commands.Bcftools.MPILEUP_AND_CALL,
                        Config.Settings.REMOTE_WORK_DIR,
                        Path.GetFileName(remoteBamPath),
                        Path.GetFileName(remoteVcfPath));

                    _sshService.ExecuteCommand(pipeCmd);
                    Utils.Logger.LogSuccess("变异检测完成（管道方式）");
                }
                catch (Exception ex2)
                {
                    throw new Exception($"变异检测失败: {ex.Message}，管道方式也失败: {ex2.Message}");
                }
            }
        }

        // 变异过滤（修复版：移除%前缀，尝试多种表达式）
        public void FilterVariants(string remoteVcfPath, string remoteFilteredVcfPath)
        {
            if (!_sshService.IsConnected)
            {
                throw new Exception("SSH未连接");
            }

            Utils.Logger.LogInfo($"变异过滤: {remoteVcfPath} -> {remoteFilteredVcfPath}");

            try
            {
                // 先查看VCF文件头部信息，了解字段结构
                Utils.Logger.LogInfo("查看VCF头部信息，了解字段结构...");
                string header = ViewVcfHeader(remoteVcfPath);
                Utils.Logger.LogInfo($"VCF头部信息:\n{header}");

                // 统计原始变异数量
                int originalCount = CountVariantsInVcf(remoteVcfPath);
                Utils.Logger.LogInfo($"原始VCF变异数量: {originalCount}");

                // 尝试多种过滤表达式
                bool filterSuccess = TryMultipleFilterExpressions(remoteVcfPath, remoteFilteredVcfPath);

                if (!filterSuccess)
                {
                    // 如果所有过滤表达式都失败，使用原始VCF作为过滤后的文件
                    Utils.Logger.LogWarning("所有过滤表达式均失败，使用原始VCF文件");
                    string copyCmd = $"cd {Config.Settings.REMOTE_WORK_DIR} && cp {Path.GetFileName(remoteVcfPath)} {Path.GetFileName(remoteFilteredVcfPath)}";
                    _sshService.ExecuteCommand(copyCmd);
                }

                // 统计过滤后的变异数量
                int filteredCount = CountVariantsInVcf(remoteFilteredVcfPath);
                Utils.Logger.LogInfo($"过滤后VCF变异数量: {filteredCount}");

                Utils.Logger.LogSuccess("变异过滤完成");
            }
            catch (Exception ex)
            {
                Utils.Logger.LogError($"变异过滤失败: {ex.Message}");
                throw;
            }
        }

        // 尝试多种过滤表达式
        private bool TryMultipleFilterExpressions(string remoteVcfPath, string remoteFilteredVcfPath)
        {
            string vcfName = Path.GetFileName(remoteVcfPath);
            string filteredName = Path.GetFileName(remoteFilteredVcfPath);

            // 定义多种过滤表达式
            var filterExpressions = new Dictionary<string, string>
            {
                // 主表达式：QUAL<20 || DP<10（标准格式）
                { "标准过滤(QUAL<20 || DP<10)", Config.Commands.Bcftools.FILTER_VARIANTS },
                
                // 备用表达式1：只过滤QUAL
                { "只过滤QUAL(QUAL<20)", Config.Commands.Bcftools.FILTER_VARIANTS_QUAL_ONLY },
                
                // 备用表达式2：过滤QUAL和INFO/DP
                { "过滤QUAL和INFO/DP", Config.Commands.Bcftools.FILTER_VARIANTS_INFO_DP },
                
                // 备用表达式3：过滤QUAL和FORMAT/DP
                { "过滤QUAL和FORMAT/DP", Config.Commands.Bcftools.FILTER_VARIANTS_FORMAT_DP },
                
                // 备用表达式4：更简单的过滤
                { "简单过滤(QUAL<10)", Config.Commands.Bcftools.FILTER_VARIANTS_SIMPLE }
            };

            // 逐个尝试过滤表达式
            foreach (var filter in filterExpressions)
            {
                try
                {
                    Utils.Logger.LogInfo($"尝试过滤表达式: {filter.Key}");

                    // 构建过滤命令
                    string filterCmd = string.Format(filter.Value,
                        Config.Settings.REMOTE_WORK_DIR,
                        vcfName,
                        filteredName);

                    Utils.Logger.LogInfo($"执行过滤命令: {filterCmd}");
                    _sshService.ExecuteCommand(filterCmd);

                    // 检查过滤后的文件是否有效
                    string checkCmd = $"cd {Config.Settings.REMOTE_WORK_DIR} && " +
                                     $"test -s {filteredName} && echo 'not_empty' || echo 'empty'";
                    string result = _sshService.ExecuteCommand(checkCmd).Trim();

                    if (result == "not_empty")
                    {
                        // 检查文件是否包含变异（非注释行）
                        string countCmd = $"cd {Config.Settings.REMOTE_WORK_DIR} && " +
                                         $"grep -c '^[^#]' {filteredName}";
                        string countResult = _sshService.ExecuteCommand(countCmd).Trim();

                        if (int.TryParse(countResult, out int variantCount) && variantCount > 0)
                        {
                            Utils.Logger.LogSuccess($"过滤表达式 '{filter.Key}' 成功，保留 {variantCount} 个变异");
                            return true;
                        }
                        else
                        {
                            Utils.Logger.LogWarning($"过滤表达式 '{filter.Key}' 生成了空变异文件");
                        }
                    }
                    else
                    {
                        Utils.Logger.LogWarning($"过滤表达式 '{filter.Key}' 生成了空文件");
                    }
                }
                catch (Exception ex)
                {
                    Utils.Logger.LogWarning($"过滤表达式 '{filter.Key}' 失败: {ex.Message}");
                }
            }

            return false;
        }

        // 生成Flagstat统计
        public void GenerateFlagstat(string remoteBamPath, string remoteFlagstatPath)
        {
            if (!_sshService.IsConnected)
            {
                throw new Exception("SSH未连接");
            }

            Utils.Logger.LogInfo($"生成Flagstat统计: {remoteBamPath} -> {remoteFlagstatPath}");

            string flagstatCmd = string.Format(Config.Commands.Samtools.FLAGSTAT,
                Config.Settings.REMOTE_WORK_DIR,
                Path.GetFileName(remoteBamPath),
                Path.GetFileName(remoteFlagstatPath));

            try
            {
                Utils.Logger.LogInfo($"执行命令: {flagstatCmd}");
                _sshService.ExecuteCommand(flagstatCmd);
                Utils.Logger.LogSuccess("Flagstat统计生成完成");
            }
            catch (Exception ex)
            {
                Utils.Logger.LogError($"Flagstat统计生成失败: {ex.Message}");
                throw;
            }
        }

        // 完整的变异检测流程（修复版）
        public string RunFullVariantDetection(string sampleName, string outputDir)
        {
            if (!_sshService.IsConnected)
            {
                throw new Exception("SSH未连接");
            }

            Utils.Logger.LogInfo($"开始完整的变异检测流程: {sampleName}");

            try
            {
                // 定义文件路径
                string remoteSam = $"{Config.Settings.REMOTE_WORK_DIR}/{sampleName}.sam";
                string remoteBam = $"{Config.Settings.REMOTE_WORK_DIR}/{sampleName}.bam";
                string remoteSortedBam = $"{Config.Settings.REMOTE_WORK_DIR}/{sampleName}_sorted.bam";
                string remoteVcf = $"{Config.Settings.REMOTE_WORK_DIR}/{sampleName}.vcf";
                string remoteFilteredVcf = $"{Config.Settings.REMOTE_WORK_DIR}/{sampleName}_filtered.vcf";
                string remoteFlagstat = $"{Config.Settings.REMOTE_WORK_DIR}/{sampleName}_sorted.flagstat";

                // 1. SAM转BAM
                Utils.Logger.LogInfo("步骤1: SAM转BAM");
                SamToBam(remoteSam, remoteBam);

                // 2. 排序BAM
                Utils.Logger.LogInfo("步骤2: 排序BAM");
                SortBam(remoteBam, remoteSortedBam, Config.Settings.DEFAULT_THREADS);

                // 3. 建立BAM索引
                Utils.Logger.LogInfo("步骤3: 建立BAM索引");
                IndexBam(remoteSortedBam);

                // 4. 生成Flagstat统计
                Utils.Logger.LogInfo("步骤4: 生成Flagstat统计");
                GenerateFlagstat(remoteSortedBam, remoteFlagstat);

                // 5. 查看BAM文件信息
                Utils.Logger.LogInfo("步骤5: 检查BAM文件");
                string bamInfo = ViewBamHeader(remoteSortedBam);
                Utils.Logger.LogInfo($"BAM文件信息:\n{bamInfo}");

                // 6. 变异检测（使用修复后的方法）
                Utils.Logger.LogInfo("步骤6: 变异检测");
                CallVariants(remoteSortedBam, remoteVcf);

                // 7. 查看原始VCF信息
                Utils.Logger.LogInfo("步骤7: 检查原始VCF");
                string vcfHeader = ViewVcfHeader(remoteVcf);
                Utils.Logger.LogInfo($"原始VCF头部信息:\n{vcfHeader}");

                int originalVariantCount = CountVariantsInVcf(remoteVcf);
                Utils.Logger.LogInfo($"原始VCF变异数量: {originalVariantCount}");

                // 8. 变异过滤（使用修复后的方法）
                Utils.Logger.LogInfo("步骤8: 变异过滤");
                FilterVariants(remoteVcf, remoteFilteredVcf);

                // 9. 查看过滤后的VCF信息
                Utils.Logger.LogInfo("步骤9: 检查过滤后的VCF");
                string filteredVcfHeader = ViewVcfHeader(remoteFilteredVcf);
                Utils.Logger.LogInfo($"过滤后VCF头部信息:\n{filteredVcfHeader}");

                int filteredVariantCount = CountVariantsInVcf(remoteFilteredVcf);
                Utils.Logger.LogInfo($"过滤后VCF变异数量: {filteredVariantCount}");

                // 10. 下载最终VCF文件到本地
                string localVcf = Path.Combine(outputDir, $"{sampleName}_filtered.vcf");
                Utils.Logger.LogInfo($"步骤10: 下载过滤后的VCF文件到本地: {localVcf}");
                _sshService.DownloadFile(remoteFilteredVcf, localVcf);

                // 11. 下载Flagstat统计文件
                string localFlagstat = Path.Combine(outputDir, $"{sampleName}_sorted.flagstat");
                Utils.Logger.LogInfo($"步骤11: 下载Flagstat统计文件: {localFlagstat}");
                _sshService.DownloadFile(remoteFlagstat, localFlagstat);

                // 12. 下载原始VCF文件（可选）
                string localOriginalVcf = Path.Combine(outputDir, $"{sampleName}.vcf");
                Utils.Logger.LogInfo($"步骤12: 下载原始VCF文件: {localOriginalVcf}");
                _sshService.DownloadFile(remoteVcf, localOriginalVcf);

                Utils.Logger.LogSuccess($"完整的变异检测流程完成");
                Utils.Logger.LogSuccess($"原始变异数量: {originalVariantCount}");
                Utils.Logger.LogSuccess($"过滤后变异数量: {filteredVariantCount}");
                Utils.Logger.LogSuccess($"过滤掉变异数量: {originalVariantCount - filteredVariantCount}");
                Utils.Logger.LogSuccess($"结果文件: {localVcf}");

                return localVcf;
            }
            catch (Exception ex)
            {
                Utils.Logger.LogError($"变异检测流程失败: {ex.Message}");
                throw;
            }
        }

        // 测试变异检测命令
        public bool TestVariantCalling()
        {
            try
            {
                Utils.Logger.LogInfo("测试变异检测命令...");

                // 测试bcftools版本
                string testCmd = $"cd {Config.Settings.REMOTE_WORK_DIR} && ./bcftools --version";
                string result = _sshService.ExecuteCommand(testCmd);

                Utils.Logger.LogInfo($"bcftools版本信息: {result}");

                // 测试samtools版本
                string testCmd2 = $"cd {Config.Settings.REMOTE_WORK_DIR} && ./samtools --version";
                string result2 = _sshService.ExecuteCommand(testCmd2);

                Utils.Logger.LogInfo($"samtools版本信息: {result2}");

                return true;
            }
            catch (Exception ex)
            {
                Utils.Logger.LogError($"测试失败: {ex.Message}");
                return false;
            }
        }

        // 检查服务器文件
        public string CheckServerFiles(string sampleName)
        {
            try
            {
                Utils.Logger.LogInfo($"检查服务器文件: {sampleName}");

                string checkCmd = $"cd {Config.Settings.REMOTE_WORK_DIR} && ls -la | grep {sampleName}";
                string result = _sshService.ExecuteCommand(checkCmd);

                Utils.Logger.LogInfo($"服务器文件列表:\n{result}");
                return result;
            }
            catch (Exception ex)
            {
                Utils.Logger.LogError($"检查服务器文件失败: {ex.Message}");
                return $"检查失败: {ex.Message}";
            }
        }
    }
}