using System;

namespace NgsBwaRunner.Utils
{
    public static class ProgressHelper
    {
        // 计算进度百分比
        public static int CalculatePercentage(long current, long total)
        {
            if (total <= 0)
                return 0;

            double percentage = (double)current / total * 100;
            return Math.Min(100, Math.Max(0, (int)percentage));
        }

        // 格式化进度显示
        public static string FormatProgress(long current, long total)
        {
            if (total <= 0)
                return $"{FileHelper.FormatFileSize(current)}";

            int percentage = CalculatePercentage(current, total);
            return $"{percentage}% ({FileHelper.FormatFileSize(current)}/{FileHelper.FormatFileSize(total)})";
        }

        // 计算传输速度
        public static double CalculateSpeedKBps(long bytesTransferred, TimeSpan elapsed)
        {
            if (elapsed.TotalSeconds <= 0)
                return 0;

            return bytesTransferred / elapsed.TotalSeconds / 1024.0;
        }

        // 格式化速度显示
        public static string FormatSpeed(long bytesTransferred, TimeSpan elapsed)
        {
            double speedKBps = CalculateSpeedKBps(bytesTransferred, elapsed);

            if (speedKBps < 1024)
                return $"{speedKBps:F2} KB/s";
            else
                return $"{(speedKBps / 1024):F2} MB/s";
        }

        // 估算剩余时间
        public static TimeSpan EstimateRemainingTime(long current, long total, TimeSpan elapsed)
        {
            if (current <= 0 || elapsed.TotalSeconds <= 0)
                return TimeSpan.Zero;

            double speed = current / elapsed.TotalSeconds;
            long remaining = total - current;

            if (speed <= 0)
                return TimeSpan.Zero;

            double secondsRemaining = remaining / speed;
            return TimeSpan.FromSeconds(secondsRemaining);
        }
    }
}