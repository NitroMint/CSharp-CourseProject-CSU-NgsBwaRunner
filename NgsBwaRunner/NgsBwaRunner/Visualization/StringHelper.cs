using System;

namespace NgsBwaRunner.Visualization
{
    public static class StringHelper
    {
        // 截断字符串，避免过长
        public static string Truncate(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            if (text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength) + "...";
        }

        // 判断字符串是否为空或空白
        public static bool IsEmpty(string text)
        {
            return string.IsNullOrWhiteSpace(text);
        }

        // 如果为空则返回默认值
        public static string DefaultIfEmpty(string text, string defaultValue)
        {
            return IsEmpty(text) ? defaultValue : text;
        }

        // 从命令中提取基本命令名（去掉路径和参数）
        public static string ExtractCommandName(string fullCommand)
        {
            if (string.IsNullOrEmpty(fullCommand))
                return "unknown";

            string command = fullCommand.Trim();

            // 去掉路径，只保留命令名
            if (command.Contains('/'))
            {
                int lastSlash = command.LastIndexOf('/');
                command = command.Substring(lastSlash + 1);
            }

            // 去掉参数，只保留第一个单词
            string[] parts = command.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0)
            {
                command = parts[0];
            }

            return command;
        }

        // 安全地显示命令，隐藏敏感信息
        public static string SafeCommandDisplay(string command)
        {
            if (string.IsNullOrEmpty(command))
                return "";

            // 隐藏包含敏感信息的部分
            if (command.Contains("password", StringComparison.OrdinalIgnoreCase) ||
                command.Contains("pass", StringComparison.OrdinalIgnoreCase) ||
                command.Contains("secret", StringComparison.OrdinalIgnoreCase))
            {
                return "[命令包含敏感信息，已隐藏]";
            }

            return command;
        }

        // 格式化时间间隔
        public static string FormatTimeSpan(TimeSpan timeSpan)
        {
            if (timeSpan.TotalHours >= 1)
                return $"{timeSpan.TotalHours:F1}小时";
            else if (timeSpan.TotalMinutes >= 1)
                return $"{timeSpan.TotalMinutes:F1}分钟";
            else
                return $"{timeSpan.TotalSeconds:F1}秒";
        }
    }
}