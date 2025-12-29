namespace NgsBwaRunner.Models
{
    public class ServerResource
    {
        public long TotalMemoryKB { get; set; }
        public long AvailableMemoryKB { get; set; }
        public int CpuCores { get; set; }
        public long TmpAvailableKB { get; set; }
        public string CpuModel { get; set; } = string.Empty; // 添加默认值
        public string SystemInfo { get; set; } = string.Empty; // 添加默认值

        // 计算内存GB
        public double TotalMemoryGB => TotalMemoryKB / 1024.0 / 1024.0;
        public double AvailableMemoryGB => AvailableMemoryKB / 1024.0 / 1024.0;
        public double TmpAvailableGB => TmpAvailableKB / 1024.0 / 1024.0;

        // 显示信息
        public string CoresDisplay => $"CPU: {CpuCores} 核心";
        public string MemoryDisplay => $"内存: {AvailableMemoryGB:F1}/{TotalMemoryGB:F1} GB";
        public string DiskDisplay => $"/tmp: {TmpAvailableGB:F1} GB";
    }
}