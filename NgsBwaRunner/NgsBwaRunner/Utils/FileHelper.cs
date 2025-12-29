using System;
using System.IO;

namespace NgsBwaRunner.Utils
{
    public static class FileHelper
    {
        // 格式化文件大小
        public static string FormatFileSize(long bytes)
        {
            if (bytes < 1024)
                return $"{bytes} B";

            if (bytes < 1024 * 1024)
                return $"{(bytes / 1024.0):F2} KB";

            if (bytes < 1024 * 1024 * 1024)
                return $"{(bytes / (1024.0 * 1024.0)):F2} MB";

            return $"{(bytes / (1024.0 * 1024.0 * 1024.0)):F2} GB";
        }

        // 安全删除文件
        public static void SafeDelete(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Logger.LogDebug($"删除文件: {filePath}");
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"删除文件失败 {filePath}: {ex.Message}");
            }
        }

        // 安全删除目录
        public static void SafeDeleteDirectory(string dirPath)
        {
            try
            {
                if (Directory.Exists(dirPath))
                {
                    Directory.Delete(dirPath, true);
                    Logger.LogDebug($"删除目录: {dirPath}");
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"删除目录失败 {dirPath}: {ex.Message}");
            }
        }

        // 检查文件是否存在
        public static bool FileExists(string path)
        {
            bool exists = File.Exists(path);
            if (!exists)
                Logger.LogWarning($"文件不存在: {path}");
            return exists;
        }

        // 检查目录是否存在，不存在则创建
        public static void EnsureDirectoryExists(string dirPath)
        {
            try
            {
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                    Logger.LogDebug($"创建目录: {dirPath}");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"创建目录失败 {dirPath}: {ex.Message}");
                throw;
            }
        }

        // 获取文件扩展名（小写）
        public static string GetFileExtension(string filePath)
        {
            return Path.GetExtension(filePath).ToLower();
        }

        // 检查文件是否是指定扩展名
        public static bool HasExtension(string filePath, params string[] extensions)
        {
            string ext = GetFileExtension(filePath);
            foreach (string expectedExt in extensions)
            {
                if (ext == expectedExt.ToLower())
                    return true;
            }
            return false;
        }

        // 检查是否是FASTA文件
        public static bool IsFastaFile(string filePath)
        {
            return HasExtension(filePath, ".fa", ".fasta", ".fna");
        }

        // 检查是否是FASTQ文件
        public static bool IsFastqFile(string filePath)
        {
            return HasExtension(filePath, ".fq", ".fastq", ".fq.gz", ".fastq.gz");
        }
    }
}