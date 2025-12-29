using System.Collections.Generic;

namespace NgsBwaRunner.Config
{
    public static class Commands
    {
        // SSH连接和系统检查命令
        public static class System
        {
            public const string CHECK_CPU_CORES = "nproc";
            public const string CHECK_MEMORY = "cat /proc/meminfo";
            public const string CHECK_TMP_SPACE = "df -k /tmp | awk 'NR==2 {print $4}'";
            public const string CHECK_CPU_MODEL = "cat /proc/cpuinfo | grep 'model name' | head -1 | cut -d':' -f2 | sed 's/^ //'";
            public const string CHECK_SYSTEM_INFO = "uname -a";
            public const string CREATE_DIRECTORY = "mkdir -p {0}";
            public const string CHMOD_EXECUTE = "chmod +x {0}";
            public const string DELETE_FILE = "rm -f {0}";
            public const string FILE_SIZE = "wc -c < {0}";
            public const string CHECK_FILE_EXISTS = "test -f {0} && echo 'exists' || echo 'not exists'";
        }

        // BWA相关命令
        public static class Bwa
        {
            public const string INDEX = "cd {0} && ./bwa index -a bwtsw ref.fa";
            public const string ALIGN_SINGLE = "cd {0} && ./bwa mem -t {1} -R '{2}' ref.fa {3} > {4}";
            public const string ALIGN_PAIRED = "cd {0} && ./bwa mem -t {1} -R '{2}' ref.fa {3} {4} > {5}";
        }

        // Samtools相关命令
        public static class Samtools
        {
            public const string SAM_TO_BAM = "cd {0} && ./samtools view -bS {1} -o {2}";
            public const string SORT_BAM = "cd {0} && ./samtools sort -@ {1} {2} -o {3}";
            public const string INDEX_BAM = "cd {0} && ./samtools index {1}";
            public const string MPILEUP = "cd {0} && ./samtools mpileup -f ref.fa {1} > {2}";
            public const string FLAGSTAT = "cd {0} && ./samtools flagstat {1} > {2}";

            // 添加：查看BAM文件头部信息
            public const string VIEW_BAM_HEADER = "cd {0} && ./samtools view -H {1} | head -10";
        }

        // Bcftools相关命令
        public static class Bcftools
        {
            // 方法1：管道方式（不使用-u参数）
            public const string MPILEUP_AND_CALL = "cd {0} && ./samtools mpileup -f ref.fa {1} | ./bcftools call -mv -o {2}";

            // 方法2：两步法（已验证可用）
            public const string BCFTOOLS_MPILEUP = "cd {0} && ./bcftools mpileup -f ref.fa {1} -o {2}";
            public const string CALL_VARIANTS = "cd {0} && ./bcftools call -mv {1} -o {2}";

            // 方法3：直接使用bcftools进行整个流程
            public const string FULL_VARIANT_CALLING = "cd {0} && ./bcftools mpileup -f ref.fa {1} | ./bcftools call -mv -o {2}";

            // 方法4：查看VCF头部信息
            public const string VIEW_VCF_HEADER = "cd {0} && ./bcftools view -h {1} | head -20";

            // 方法5：查看VCF内容（前几行）
            public const string VIEW_VCF_CONTENT = "cd {0} && ./bcftools view {1} | head -20";

            // 变异过滤命令（修复版：移除%前缀）
            public const string FILTER_VARIANTS = "cd {0} && ./bcftools filter -s LowQual -e 'QUAL<20 || DP<10' {1} > {2}";

            // 备用过滤命令1：只过滤QUAL
            public const string FILTER_VARIANTS_QUAL_ONLY = "cd {0} && ./bcftools filter -s LowQual -e 'QUAL<20' {1} > {2}";

            // 备用过滤命令2：过滤QUAL和INFO/DP
            public const string FILTER_VARIANTS_INFO_DP = "cd {0} && ./bcftools filter -s LowQual -e 'QUAL<20 || INFO/DP<10' {1} > {2}";

            // 备用过滤命令3：过滤QUAL和FORMAT/DP
            public const string FILTER_VARIANTS_FORMAT_DP = "cd {0} && ./bcftools filter -s LowQual -e 'QUAL<20 || FORMAT/DP<10' {1} > {2}";

            // 备用过滤命令4：更简单的过滤
            public const string FILTER_VARIANTS_SIMPLE = "cd {0} && ./bcftools filter -s LowQual -e 'QUAL<10' {1} > {2}";

            // 统计VCF变异数量
            public const string COUNT_VARIANTS = "cd {0} && grep -c '^[^#]' {1}";
        }
    }
}