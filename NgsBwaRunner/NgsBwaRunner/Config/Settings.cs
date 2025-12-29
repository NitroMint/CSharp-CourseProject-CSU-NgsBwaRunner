namespace NgsBwaRunner.Config
{
    public static class Settings
    {
        // 默认服务器设置
        public const string DEFAULT_HOST = "100.000.00.00";
        public const int DEFAULT_PORT = 22;
        public const string DEFAULT_USER = "username";
        public const string DEFAULT_PASS = "000000";

        // 远程工作目录
        public const string REMOTE_WORK_DIR = "/tmp/ngs_bwa_runner";
        public const string REMOTE_RESULTS_DIR = "/tmp/ngs_bwa_runner/results";

        // 本地工具路径
        public static readonly string LOCAL_BWA_PATH = "./tools/bwa";
        public static readonly string LOCAL_SAMTOOLS_PATH = "./tools/samtools";
        public static readonly string LOCAL_BCFTOOLS_PATH = "./tools/bcftools";

        // 处理设置
        public const int READS_PER_CHUNK = 10000;
        public const int DEFAULT_THREADS = 4;
        public const int MAX_LOG_LINES = 500;

        // 超时设置（秒）
        public const int SSH_TIMEOUT = 30;
        public const int COMMAND_TIMEOUT = 3600; // 1小时

        // 文件相关设置
        public const long MIN_MEMORY_REQUIRED = 1024 * 1024 * 1024; // 1GB
        public const long MIN_TMP_SPACE_REQUIRED = 1024 * 1024 * 1024; // 1GB

        // 默认读取组信息（用于BWA -R参数）
        public const string DEFAULT_READ_GROUP = "@RG\\tID:sample1\\tSM:sample1\\tLB:lib1\\tPL:ILLUMINA";

        // 日志设置
        public const bool ENABLE_DEBUG_LOG = true;
        public const bool LOG_TO_CONSOLE = true;
    }
}