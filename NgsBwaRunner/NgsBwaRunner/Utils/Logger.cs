using System;
using System.IO;

namespace NgsBwaRunner.Utils
{
    public static class Logger
    {
        private static readonly string _logDirectory = "./logs";
        private static readonly string _logFile = Path.Combine(_logDirectory, $"{DateTime.Now:yyyyMMdd}.log");

        static Logger()
        {
            EnsureLogDirectory();
        }

        private static void EnsureLogDirectory()
        {
            try
            {
                if (!Directory.Exists(_logDirectory))
                {
                    Directory.CreateDirectory(_logDirectory);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"创建日志目录失败: {ex.Message}");
            }
        }

        public static void LogInfo(string message)
        {
            WriteLog("INFO", message);
        }

        public static void LogSuccess(string message)
        {
            WriteLog("SUCCESS", message);
        }

        public static void LogWarning(string message)
        {
            WriteLog("WARNING", message);
        }

        public static void LogError(string message)
        {
            WriteLog("ERROR", message);
        }

        public static void LogDebug(string message)
        {
            WriteLog("DEBUG", message);
        }

        private static void WriteLog(string level, string message)
        {
            try
            {
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
                File.AppendAllText(_logFile, logEntry + Environment.NewLine);

                // 同时在控制台输出（调试用）
                if (Config.Settings.LOG_TO_CONSOLE || level == "ERROR")
                {
                    Console.WriteLine(logEntry);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"日志写入失败: {ex.Message}");
            }
        }

        // 获取日志内容
        public static string GetLogContent(int maxLines = 100)
        {
            try
            {
                if (!File.Exists(_logFile))
                    return "日志文件不存在";

                string[] lines = File.ReadAllLines(_logFile);
                if (lines.Length > maxLines)
                {
                    string[] recentLines = new string[maxLines];
                    Array.Copy(lines, lines.Length - maxLines, recentLines, 0, maxLines);
                    return string.Join(Environment.NewLine, recentLines);
                }

                return string.Join(Environment.NewLine, lines);
            }
            catch (Exception ex)
            {
                return $"读取日志失败: {ex.Message}";
            }
        }

        // 清空今日日志
        public static void ClearTodayLog()
        {
            try
            {
                if (File.Exists(_logFile))
                {
                    File.Delete(_logFile);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"清空日志失败: {ex.Message}");
            }
        }
    }
}