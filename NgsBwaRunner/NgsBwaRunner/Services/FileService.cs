using System;
using System.IO;

namespace NgsBwaRunner.Services
{
    public class FileService
    {
        // 检查本地工具
        public bool CheckLocalTools()
        {
            bool bwaExists = File.Exists(Config.Settings.LOCAL_BWA_PATH);
            bool samtoolsExists = File.Exists(Config.Settings.LOCAL_SAMTOOLS_PATH);
            bool bcftoolsExists = File.Exists(Config.Settings.LOCAL_BCFTOOLS_PATH);

            if (!bwaExists)
                Utils.Logger.LogWarning($"BWA工具不存在: {Config.Settings.LOCAL_BWA_PATH}");
            if (!samtoolsExists)
                Utils.Logger.LogWarning($"Samtools工具不存在: {Config.Settings.LOCAL_SAMTOOLS_PATH}");
            if (!bcftoolsExists)
                Utils.Logger.LogWarning($"BCFtools工具不存在: {Config.Settings.LOCAL_BCFTOOLS_PATH}");

            bool allExist = bwaExists && samtoolsExists && bcftoolsExists;

            if (allExist)
                Utils.Logger.LogSuccess("所有本地工具都存在");
            else
                Utils.Logger.LogError("部分本地工具缺失");

            return allExist;
        }

        // 检查文件类型
        public string CheckFileType(string filePath)
        {
            if (!File.Exists(filePath))
                return "文件不存在";

            string extension = Path.GetExtension(filePath).ToLower();

            if (extension == ".fa" || extension == ".fasta" || extension == ".fna")
                return "FASTA参考序列";
            else if (extension == ".fq" || extension == ".fastq")
                return "FASTQ测序数据";
            else if (extension == ".gz")
            {
                string name = Path.GetFileNameWithoutExtension(filePath);
                string innerExtension = Path.GetExtension(name).ToLower();
                if (innerExtension == ".fq" || innerExtension == ".fastq")
                    return "压缩的FASTQ测序数据";
            }
            else if (extension == ".sam")
                return "SAM比对结果";
            else if (extension == ".bam")
                return "BAM比对结果";
            else if (extension == ".vcf")
                return "VCF变异文件";

            return "未知文件类型";
        }

        // 清理临时文件
        public void CleanTempFiles()
        {
            try
            {
                string tempDir = Path.Combine(Path.GetTempPath(), "NgsBwaRunner");
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                    Utils.Logger.LogInfo($"清理临时目录: {tempDir}");
                }
            }
            catch (Exception ex)
            {
                Utils.Logger.LogWarning($"清理临时文件失败: {ex.Message}");
            }
        }

        // 获取文件基本名（不带扩展名）
        public string GetBaseName(string filePath)
        {
            return Path.GetFileNameWithoutExtension(filePath);
        }
    }
}