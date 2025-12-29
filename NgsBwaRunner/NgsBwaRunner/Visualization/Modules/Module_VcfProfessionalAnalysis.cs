// Visualization/Modules/Module_VcfProfessionalAnalysis.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NgsBwaRunner.Visualization.Interfaces;
using NgsBwaRunner.Visualization.Models;
using NgsBwaRunner.Visualization.Attributes;

namespace NgsBwaRunner.Visualization.Modules
{
    /// <summary>
    /// VCF文件专业级分析报告模块
    /// 支持原始VCF和过滤后VCF的对比分析，包含详细的变异统计和质量评估
    /// </summary>
    [VisualizationModule(
        "vcf.professional.analysis",
        "VCF专业分析报告",
        "生成VCF文件的专业级变异分析报告，包含原始与过滤后结果的对比统计和质量评估",
        80
    )]
    public class Module_VcfProfessionalAnalysis : IVisualizationModule
    {
        // 模块属性
        public string ModuleId => "vcf.professional.analysis";
        public string DisplayName => "VCF专业分析报告";
        public string Description => "生成VCF文件的专业级变异分析报告，包含原始与过滤后结果的对比统计和质量评估";
        public int Priority => 80;
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 执行模块生成专业报告
        /// </summary>
        public async Task<VisualizationResult> ExecuteAsync(VisualizationContext context)
        {
            try
            {
                // 验证输入文件
                bool hasOriginalVcf = !string.IsNullOrEmpty(context.VcfFilePath) && File.Exists(context.VcfFilePath);
                bool hasFilteredVcf = !string.IsNullOrEmpty(context.FilteredVcfFilePath) && File.Exists(context.FilteredVcfFilePath);

                if (!hasOriginalVcf && !hasFilteredVcf)
                {
                    return VisualizationResult.ErrorResult("VCF文件不存在",
                        $"原始VCF: {context.VcfFilePath ?? "未提供"}\n过滤后VCF: {context.FilteredVcfFilePath ?? "未提供"}");
                }

                // 报告进度
                context.ProgressReporter?.Invoke("开始分析VCF文件...", 10);

                // 分析原始VCF文件
                VcfAnalysisResult originalAnalysis = null;
                if (hasOriginalVcf)
                {
                    originalAnalysis = await AnalyzeVcfFileAsync(context.VcfFilePath, "原始", context.ProgressReporter, 10, 40);
                }

                // 分析过滤后VCF文件
                VcfAnalysisResult filteredAnalysis = null;
                if (hasFilteredVcf)
                {
                    filteredAnalysis = await AnalyzeVcfFileAsync(context.FilteredVcfFilePath, "过滤后", context.ProgressReporter, 40, 70);
                }

                // 生成报告
                context.ProgressReporter?.Invoke("生成专业分析报告...", 70);

                string reportFileName = $"{context.SampleName ?? "unknown"}_vcf_professional_analysis.txt";
                string reportPath = Path.Combine(context.OutputDirectory ?? ".", reportFileName);

                await GenerateProfessionalReportAsync(originalAnalysis, filteredAnalysis, reportPath, context);

                context.ProgressReporter?.Invoke("VCF专业分析完成", 100);

                // 准备返回消息
                var message = new StringBuilder();
                message.AppendLine("✅ VCF专业分析报告生成成功");

                if (originalAnalysis != null)
                {
                    message.AppendLine($"📊 原始变异数: {originalAnalysis.TotalVariants:N0}");
                }

                if (filteredAnalysis != null)
                {
                    message.AppendLine($"🎯 过滤后变异数: {filteredAnalysis.TotalVariants:N0}");

                    if (originalAnalysis != null)
                    {
                        double retentionRate = (double)filteredAnalysis.TotalVariants / originalAnalysis.TotalVariants * 100;
                        message.AppendLine($"📈 保留率: {retentionRate:F1}%");
                    }
                }

                message.AppendLine($"📁 报告文件: {reportFileName}");

                return VisualizationResult.SuccessResult(
                    message.ToString(),
                    reportPath
                );
            }
            catch (Exception ex)
            {
                Utils.Logger.LogError($"VCF专业分析模块执行失败: {ex.Message}");
                return VisualizationResult.ErrorResult(
                    $"生成VCF专业分析报告失败: {ex.Message}",
                    ex.ToString()
                );
            }
        }

        /// <summary>
        /// 分析VCF文件
        /// </summary>
        private async Task<VcfAnalysisResult> AnalyzeVcfFileAsync(string vcfFilePath, string analysisType,
            Action<string, int> progressReporter, int startProgress, int endProgress)
        {
            var result = new VcfAnalysisResult
            {
                FilePath = vcfFilePath,
                AnalysisType = analysisType,
                FileName = Path.GetFileName(vcfFilePath)
            };

            var stats = new VcfStatistics();
            var variantCounts = new VariantTypeCounts();
            var qualityMetrics = new QualityMetrics();
            var chromosomeDistribution = new Dictionary<string, ChromosomeStats>();
            var filterStats = new Dictionary<string, int>();
            var consequenceStats = new Dictionary<string, int>();

            int lineCount = 0;
            int variantCount = 0;
            int sampledVariants = 0;
            const int maxSamples = 10000;

            using (var reader = new StreamReader(vcfFilePath))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    lineCount++;

                    // 解析头部信息
                    if (line.StartsWith("##"))
                    {
                        ParseVcfHeaderLine(line, result);
                        continue;
                    }
                    else if (line.StartsWith("#CHROM"))
                    {
                        ParseVcfColumnHeader(line, result);
                        continue;
                    }

                    // 进度报告
                    if (lineCount % 1000 == 0)
                    {
                        int progress = startProgress + (int)((endProgress - startProgress) * (double)lineCount / 100000);
                        progressReporter?.Invoke($"分析{analysisType}VCF文件... 已处理 {lineCount:N0} 行", progress);
                    }

                    // 采样限制
                    if (sampledVariants >= maxSamples)
                    {
                        result.IsSampled = true;
                        result.SampleSize = maxSamples;
                        // 继续解析以获取总数，但不详细分析
                        variantCount++;
                        continue;
                    }

                    // 解析变异记录
                    ParseVariantRecord(line, stats, variantCounts, qualityMetrics,
                        chromosomeDistribution, filterStats, consequenceStats);

                    variantCount++;
                    sampledVariants++;
                }
            }

            // 计算衍生统计
            CalculateStatistics(stats, variantCounts, qualityMetrics, chromosomeDistribution);

            // 保存结果
            result.TotalLines = lineCount;
            result.TotalVariants = variantCount;
            result.SampledVariants = sampledVariants;
            result.Statistics = stats;
            result.VariantTypeCounts = variantCounts;
            result.QualityMetrics = qualityMetrics;
            result.ChromosomeDistribution = chromosomeDistribution;
            result.FilterStats = filterStats;
            result.ConsequenceStats = consequenceStats;
            result.AnalysisTimestamp = DateTime.Now;

            return result;
        }

        /// <summary>
        /// 解析VCF头部行
        /// </summary>
        private void ParseVcfHeaderLine(string line, VcfAnalysisResult result)
        {
            // 文件格式版本
            if (line.StartsWith("##fileformat="))
            {
                result.FileFormat = line.Substring(13);
            }
            // 参考基因组
            else if (line.StartsWith("##reference="))
            {
                result.ReferenceGenome = line.Substring(12);
            }
            // 命令行
            else if (line.StartsWith("##commandline="))
            {
                result.CommandLine = line.Substring(14);
            }
            // 软件信息
            else if (line.StartsWith("##source="))
            {
                result.SourceSoftware = line.Substring(9);
            }
            // 过滤描述
            else if (line.StartsWith("##FILTER="))
            {
                // 解析过滤条件
                var filterInfo = ParseFilterDescription(line);
                if (filterInfo != null)
                {
                    result.FilterDescriptions.Add(filterInfo);
                }
            }
            // 信息字段描述
            else if (line.StartsWith("##INFO="))
            {
                result.InfoFieldCount++;
            }
            // 格式字段描述
            else if (line.StartsWith("##FORMAT="))
            {
                result.FormatFieldCount++;
            }
        }

        /// <summary>
        /// 解析过滤描述
        /// </summary>
        private FilterDescription ParseFilterDescription(string line)
        {
            try
            {
                // 示例：##FILTER=<ID=LowQual,Description="Low quality">
                var parts = line.Substring(9).Split(',');
                var filter = new FilterDescription();

                foreach (var part in parts)
                {
                    if (part.StartsWith("ID="))
                        filter.Id = part.Substring(3);
                    else if (part.StartsWith("Description="))
                        filter.Description = part.Substring(12).Trim('"');
                }

                return string.IsNullOrEmpty(filter.Id) ? null : filter;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 解析VCF列标题
        /// </summary>
        private void ParseVcfColumnHeader(string line, VcfAnalysisResult result)
        {
            var columns = line.Split('\t');
            if (columns.Length >= 8)
            {
                result.HasFormatColumn = columns.Length > 8;
                result.HasSampleColumns = columns.Length > 9;
                result.SampleCount = columns.Length - 9;
            }
        }

        /// <summary>
        /// 解析变异记录
        /// </summary>
        private void ParseVariantRecord(string line, VcfStatistics stats, VariantTypeCounts variantCounts,
            QualityMetrics qualityMetrics, Dictionary<string, ChromosomeStats> chromosomeDistribution,
            Dictionary<string, int> filterStats, Dictionary<string, int> consequenceStats)
        {
            var fields = line.Split('\t');
            if (fields.Length < 8) return;

            stats.TotalVariants++;

            // 1. 染色体位置
            string chrom = fields[0];
            if (!chromosomeDistribution.ContainsKey(chrom))
                chromosomeDistribution[chrom] = new ChromosomeStats();
            chromosomeDistribution[chrom].VariantCount++;

            // 2. 位置和ID
            if (int.TryParse(fields[1], out int pos))
            {
                stats.MinPosition = Math.Min(stats.MinPosition, pos);
                stats.MaxPosition = Math.Max(stats.MaxPosition, pos);
            }

            string id = fields[2];
            if (id != ".")
            {
                stats.VariantsWithId++;
            }

            // 3. 参考和替代等位基因
            string refAllele = fields[3];
            string altAlleles = fields[4];

            UpdateVariantTypeCounts(refAllele, altAlleles, variantCounts);
            stats.TotalAlleles++; // 参考等位基因

            // 4. 质量分数
            if (double.TryParse(fields[5], out double qual))
            {
                qualityMetrics.QualValues.Add(qual);
                if (qual >= 30) stats.HighQualityVariants++;
                if (qual < 10) stats.LowQualityVariants++;
                if (qual < 1) stats.VeryLowQualityVariants++;
            }

            // 5. 过滤状态
            string filter = fields[6];
            if (filter == "PASS" || filter == ".")
            {
                stats.PassedVariants++;
            }
            else
            {
                stats.FilteredVariants++;

                // 统计各个过滤器的使用情况
                var filters = filter.Split(';');
                foreach (var f in filters)
                {
                    if (!filterStats.ContainsKey(f))
                        filterStats[f] = 0;
                    filterStats[f]++;
                }
            }

            // 6. 信息字段（关键！）
            if (fields.Length > 7)
            {
                ParseInfoField(fields[7], stats, qualityMetrics, consequenceStats);
            }

            // 7. 格式和样本字段（如果存在）
            if (fields.Length > 8)
            {
                ParseFormatAndSampleFields(fields, stats, qualityMetrics);
            }
        }

        /// <summary>
        /// 更新变异类型统计
        /// </summary>
        private void UpdateVariantTypeCounts(string refAllele, string altAlleles, VariantTypeCounts counts)
        {
            var altList = altAlleles.Split(',');

            foreach (var alt in altList)
            {
                counts.TotalVariants++;

                // 判断变异类型
                if (refAllele.Length == 1 && alt.Length == 1)
                {
                    // SNP
                    counts.SnpCount++;

                    // 转换/颠换
                    if (IsTransition(refAllele, alt))
                        counts.Transitions++;
                    else if (IsTransversion(refAllele, alt))
                        counts.Transversions++;
                }
                else if (refAllele.Length > alt.Length)
                {
                    // 缺失
                    counts.DeletionCount++;
                    counts.IndelCount++;
                }
                else if (refAllele.Length < alt.Length)
                {
                    // 插入
                    counts.InsertionCount++;
                    counts.IndelCount++;
                }
                else
                {
                    // 替换（MNP）
                    counts.MnpCount++;
                }

                // 多等位基因统计
                if (altList.Length > 1)
                {
                    counts.MultiallelicCount++;
                }
            }
        }

        /// <summary>
        /// 判断是否为转换（嘌呤<->嘌呤 或 嘧啶<->嘧啶）
        /// </summary>
        private bool IsTransition(string refAllele, string altAllele)
        {
            var purines = new HashSet<char> { 'A', 'G' };
            var pyrimidines = new HashSet<char> { 'C', 'T' };

            char refBase = refAllele[0];
            char altBase = altAllele[0];

            return (purines.Contains(refBase) && purines.Contains(altBase)) ||
                   (pyrimidines.Contains(refBase) && pyrimidines.Contains(altBase));
        }

        /// <summary>
        /// 判断是否为颠换（嘌呤<->嘧啶）
        /// </summary>
        private bool IsTransversion(string refAllele, string altAllele)
        {
            var purines = new HashSet<char> { 'A', 'G' };
            var pyrimidines = new HashSet<char> { 'C', 'T' };

            char refBase = refAllele[0];
            char altBase = altAllele[0];

            return (purines.Contains(refBase) && pyrimidines.Contains(altBase)) ||
                   (pyrimidines.Contains(refBase) && purines.Contains(altBase));
        }

        /// <summary>
        /// 解析INFO字段
        /// </summary>
        private void ParseInfoField(string infoField, VcfStatistics stats,
            QualityMetrics qualityMetrics, Dictionary<string, int> consequenceStats)
        {
            if (string.IsNullOrEmpty(infoField) || infoField == ".")
                return;

            var infoPairs = infoField.Split(';');

            foreach (var pair in infoPairs)
            {
                var keyValue = pair.Split('=');
                if (keyValue.Length != 2) continue;

                string key = keyValue[0];
                string value = keyValue[1];

                switch (key)
                {
                    case "DP":
                        if (int.TryParse(value, out int dp))
                        {
                            qualityMetrics.DepthValues.Add(dp);
                            stats.TotalDepth += dp;
                            stats.VariantsWithDepth++;
                        }
                        break;

                    case "AF":
                        var afValues = value.Split(',');
                        foreach (var afStr in afValues)
                        {
                            if (double.TryParse(afStr, out double af))
                            {
                                qualityMetrics.AfValues.Add(af);
                                stats.VariantsWithAF++;
                            }
                        }
                        break;

                    case "MQ":
                        if (double.TryParse(value, out double mq))
                        {
                            qualityMetrics.MqValues.Add(mq);
                        }
                        break;

                    case "QD":
                        if (double.TryParse(value, out double qd))
                        {
                            qualityMetrics.QdValues.Add(qd);
                        }
                        break;

                    case "FS":
                        if (double.TryParse(value, out double fs))
                        {
                            qualityMetrics.FsValues.Add(fs);
                        }
                        break;

                    case "MQ0":
                        if (int.TryParse(value, out int mq0))
                        {
                            qualityMetrics.Mq0Values.Add(mq0);
                        }
                        break;

                    case "CSQ":
                    case "ANN":
                        // 变异后果注释
                        ParseConsequenceAnnotation(value, consequenceStats);
                        break;
                }
            }
        }

        /// <summary>
        /// 解析变异后果注释
        /// </summary>
        private void ParseConsequenceAnnotation(string annotation, Dictionary<string, int> consequenceStats)
        {
            try
            {
                // 简化处理：只统计最常见的后果
                var consequences = annotation.Split('|');
                foreach (var cons in consequences)
                {
                    if (!string.IsNullOrEmpty(cons) && cons.Length < 50) // 避免过长的字段
                    {
                        if (!consequenceStats.ContainsKey(cons))
                            consequenceStats[cons] = 0;
                        consequenceStats[cons]++;
                    }
                }
            }
            catch
            {
                // 忽略解析错误
            }
        }

        /// <summary>
        /// 解析FORMAT和样本字段
        /// </summary>
        private void ParseFormatAndSampleFields(string[] fields, VcfStatistics stats, QualityMetrics qualityMetrics)
        {
            if (fields.Length < 10) return;

            string format = fields[8];
            var formatTags = format.Split(':');

            // 检查关键格式字段
            bool hasGT = format.Contains("GT");
            bool hasDP = format.Contains("DP");
            bool hasGQ = format.Contains("GQ");
            bool hasAD = format.Contains("AD");
            bool hasPL = format.Contains("PL");

            if (hasGT) stats.VariantsWithGenotype++;
            if (hasDP) stats.VariantsWithSampleDepth++;
            if (hasGQ) stats.VariantsWithGenotypeQuality++;

            // 解析样本数据（只解析第一个样本）
            if (fields.Length > 9)
            {
                string sampleData = fields[9];
                ParseSampleData(sampleData, formatTags, qualityMetrics);
            }
        }

        /// <summary>
        /// 解析样本数据
        /// </summary>
        private void ParseSampleData(string sampleData, string[] formatTags, QualityMetrics qualityMetrics)
        {
            var values = sampleData.Split(':');

            for (int i = 0; i < Math.Min(formatTags.Length, values.Length); i++)
            {
                string tag = formatTags[i];
                string value = values[i];

                if (value == ".") continue;

                switch (tag)
                {
                    case "GT":
                        // 基因型统计
                        qualityMetrics.Genotypes.Add(value);
                        break;

                    case "DP":
                        if (int.TryParse(value, out int sampleDP))
                        {
                            qualityMetrics.SampleDepthValues.Add(sampleDP);
                        }
                        break;

                    case "GQ":
                        if (int.TryParse(value, out int gq))
                        {
                            qualityMetrics.GqValues.Add(gq);
                        }
                        break;

                    case "AD":
                        // 等位基因深度
                        var adValues = value.Split(',');
                        foreach (var adStr in adValues)
                        {
                            if (int.TryParse(adStr, out int ad))
                            {
                                qualityMetrics.AdValues.Add(ad);
                            }
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// 计算统计信息
        /// </summary>
        private void CalculateStatistics(VcfStatistics stats, VariantTypeCounts variantCounts,
            QualityMetrics qualityMetrics, Dictionary<string, ChromosomeStats> chromosomeDistribution)
        {
            // 基本统计
            stats.TiTvRatio = variantCounts.Transversions > 0 ?
                (double)variantCounts.Transitions / variantCounts.Transversions : 0;

            stats.HeterozygousRate = qualityMetrics.Genotypes.Count > 0 ?
                (double)qualityMetrics.Genotypes.Count(gt => gt.Contains("0/1") || gt.Contains("0|1")) /
                qualityMetrics.Genotypes.Count : 0;

            stats.HomozygousAltRate = qualityMetrics.Genotypes.Count > 0 ?
                (double)qualityMetrics.Genotypes.Count(gt => gt.Contains("1/1") || gt.Contains("1|1")) /
                qualityMetrics.Genotypes.Count : 0;

            // 质量指标计算
            if (qualityMetrics.QualValues.Count > 0)
            {
                qualityMetrics.MeanQual = qualityMetrics.QualValues.Average();
                qualityMetrics.MedianQual = CalculateMedian(qualityMetrics.QualValues);
                qualityMetrics.QualStdDev = CalculateStandardDeviation(qualityMetrics.QualValues);
            }

            if (qualityMetrics.DepthValues.Count > 0)
            {
                qualityMetrics.MeanDepth = qualityMetrics.DepthValues.Average();
                qualityMetrics.MedianDepth = CalculateMedian(qualityMetrics.DepthValues.Select(x => (double)x).ToArray());
            }

            if (qualityMetrics.AfValues.Count > 0)
            {
                qualityMetrics.MeanAF = qualityMetrics.AfValues.Average();
                qualityMetrics.MedianAF = CalculateMedian(qualityMetrics.AfValues);
            }

            if (qualityMetrics.GqValues.Count > 0)
            {
                qualityMetrics.MeanGQ = qualityMetrics.GqValues.Average();
                qualityMetrics.MedianGQ = CalculateMedian(qualityMetrics.GqValues.Select(x => (double)x).ToArray());
            }

            // 染色体分布排序
            stats.TopChromosomes = chromosomeDistribution
                .OrderByDescending(kv => kv.Value.VariantCount)
                .Take(10)
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        /// <summary>
        /// 计算中位数
        /// </summary>
        private double CalculateMedian(IEnumerable<double> values)
        {
            var sorted = values.OrderBy(x => x).ToArray();
            if (sorted.Length == 0) return 0;

            int mid = sorted.Length / 2;
            return sorted.Length % 2 == 0 ? (sorted[mid - 1] + sorted[mid]) / 2.0 : sorted[mid];
        }

        /// <summary>
        /// 计算标准差
        /// </summary>
        private double CalculateStandardDeviation(List<double> values)
        {
            if (values == null || values.Count < 2)
                return 0;

            double mean = values.Average();
            double sumOfSquares = values.Sum(x => Math.Pow(x - mean, 2));
            return Math.Sqrt(sumOfSquares / (values.Count - 1));
        }

        /// <summary>
        /// 生成专业报告
        /// </summary>
        private async Task GenerateProfessionalReportAsync(VcfAnalysisResult originalAnalysis,
            VcfAnalysisResult filteredAnalysis, string reportPath, VisualizationContext context)
        {
            var report = new StringBuilder();

            // 1. 报告标题
            report.AppendLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            report.AppendLine("║                    VCF文件专业级变异分析报告                                 ║");
            report.AppendLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            report.AppendLine();
            report.AppendLine($"📅 报告生成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"📁 样本名称: {context.SampleName ?? "未知样本"}");

            if (originalAnalysis != null)
            {
                report.AppendLine($"📊 原始VCF文件: {originalAnalysis.FileName}");
            }

            if (filteredAnalysis != null)
            {
                report.AppendLine($"🎯 过滤后VCF文件: {filteredAnalysis.FileName}");
            }
            report.AppendLine();

            // 2. 文件基本信息
            report.AppendLine("════════════════════════════════════════════════════════════════════════════════");
            report.AppendLine("📄 文件基本信息");
            report.AppendLine("────────────────────────────────────────────────────────────────────────────────");

            if (originalAnalysis != null)
            {
                AppendFileInfo(report, "原始", originalAnalysis);
            }

            if (filteredAnalysis != null)
            {
                AppendFileInfo(report, "过滤后", filteredAnalysis);
            }

            // 对比信息
            if (originalAnalysis != null && filteredAnalysis != null)
            {
                report.AppendLine();
                report.AppendLine($"📈 过滤效果对比:");
                long removed = originalAnalysis.TotalVariants - filteredAnalysis.TotalVariants;
                double retentionRate = (double)filteredAnalysis.TotalVariants / originalAnalysis.TotalVariants * 100;
                double removalRate = (double)removed / originalAnalysis.TotalVariants * 100;

                report.AppendLine($"   • 总变异数变化: {originalAnalysis.TotalVariants:N0} → {filteredAnalysis.TotalVariants:N0}");
                report.AppendLine($"   • 过滤掉变异数: {removed:N0} ({removalRate:F1}%)");
                report.AppendLine($"   • 保留变异数: {filteredAnalysis.TotalVariants:N0} ({retentionRate:F1}%)");
            }
            report.AppendLine();

            // 3. 变异类型统计
            report.AppendLine("════════════════════════════════════════════════════════════════════════════════");
            report.AppendLine("🧬 变异类型统计");
            report.AppendLine("────────────────────────────────────────────────────────────────────────────────");

            if (originalAnalysis != null)
            {
                AppendVariantTypeStats(report, "原始", originalAnalysis.VariantTypeCounts);
            }

            if (filteredAnalysis != null)
            {
                AppendVariantTypeStats(report, "过滤后", filteredAnalysis.VariantTypeCounts);
            }

            // Ti/Tv比率
            if (originalAnalysis != null && originalAnalysis.Statistics != null)
            {
                report.AppendLine();
                report.AppendLine($"🎯 Ti/Tv比率 (转换/颠换):");
                report.AppendLine($"   • 转换(Transition): {originalAnalysis.VariantTypeCounts?.Transitions:N0}");
                report.AppendLine($"   • 颠换(Transversion): {originalAnalysis.VariantTypeCounts?.Transversions:N0}");
                report.AppendLine($"   • Ti/Tv比率: {originalAnalysis.Statistics.TiTvRatio:F3}");

                string tiTvAssessment = GetTiTvAssessment(originalAnalysis.Statistics.TiTvRatio);
                report.AppendLine($"   • 评估: {tiTvAssessment}");
            }
            report.AppendLine();

            // 4. 质量指标统计
            report.AppendLine("════════════════════════════════════════════════════════════════════════════════");
            report.AppendLine("🎯 质量指标统计");
            report.AppendLine("────────────────────────────────────────────────────────────────────────────────");

            if (originalAnalysis != null)
            {
                AppendQualityMetrics(report, "原始", originalAnalysis.QualityMetrics);
            }

            if (filteredAnalysis != null)
            {
                AppendQualityMetrics(report, "过滤后", filteredAnalysis.QualityMetrics);
            }
            report.AppendLine();

            // 5. 过滤统计
            if (filteredAnalysis?.FilterStats != null && filteredAnalysis.FilterStats.Count > 0)
            {
                report.AppendLine("════════════════════════════════════════════════════════════════════════════════");
                report.AppendLine("🔍 过滤统计");
                report.AppendLine("────────────────────────────────────────────────────────────────────────────────");

                report.AppendLine($"过滤掉的变异分布:");
                int totalFiltered = filteredAnalysis.FilterStats.Values.Sum();

                foreach (var kv in filteredAnalysis.FilterStats.OrderByDescending(x => x.Value))
                {
                    double percentage = (double)kv.Value / totalFiltered * 100;
                    report.AppendLine($"  • {kv.Key,-20} {kv.Value,8:N0} ({percentage,6:F1}%)");
                }
                report.AppendLine();
            }

            // 6. 染色体分布
            if (originalAnalysis?.Statistics?.TopChromosomes != null && originalAnalysis.Statistics.TopChromosomes.Count > 0)
            {
                report.AppendLine("════════════════════════════════════════════════════════════════════════════════");
                report.AppendLine("🧬 染色体分布（前10位）");
                report.AppendLine("────────────────────────────────────────────────────────────────────────────────");

                long totalVariants = originalAnalysis.Statistics.TopChromosomes.Values.Sum(v => v.VariantCount);

                foreach (var kv in originalAnalysis.Statistics.TopChromosomes)
                {
                    double percentage = (double)kv.Value.VariantCount / totalVariants * 100;
                    report.AppendLine($"  • {kv.Key,-10} {kv.Value.VariantCount,8:N0} ({percentage,6:F1}%)");
                }
                report.AppendLine();
            }

            // 7. 变异后果统计（如果有注释）
            if (originalAnalysis?.ConsequenceStats != null && originalAnalysis.ConsequenceStats.Count > 0)
            {
                report.AppendLine("════════════════════════════════════════════════════════════════════════════════");
                report.AppendLine("🔬 变异后果统计（前10位）");
                report.AppendLine("────────────────────────────────────────────────────────────────────────────────");

                var topConsequences = originalAnalysis.ConsequenceStats
                    .OrderByDescending(kv => kv.Value)
                    .Take(10);

                foreach (var kv in topConsequences)
                {
                    double percentage = (double)kv.Value / originalAnalysis.TotalVariants * 100;
                    report.AppendLine($"  • {kv.Key,-30} {kv.Value,6:N0} ({percentage,5:F1}%)");
                }
                report.AppendLine();
            }

            // 8. 数据质量评估
            report.AppendLine("════════════════════════════════════════════════════════════════════════════════");
            report.AppendLine("📋 数据质量综合评估");
            report.AppendLine("────────────────────────────────────────────────────────────────────────────────");

            var assessments = new List<string>();

            if (originalAnalysis != null)
            {
                assessments.AddRange(PerformVcfQualityAssessment(originalAnalysis, filteredAnalysis));
            }

            foreach (var assessment in assessments)
            {
                report.AppendLine(assessment);
            }
            report.AppendLine();

            // 9. 建议
            report.AppendLine("════════════════════════════════════════════════════════════════════════════════");
            report.AppendLine("💡 建议和后续分析建议");
            report.AppendLine("────────────────────────────────────────────────────────────────────────────────");

            var recommendations = GenerateVcfRecommendations(originalAnalysis, filteredAnalysis);
            for (int i = 0; i < recommendations.Count; i++)
            {
                report.AppendLine($"{i + 1}. {recommendations[i]}");
            }
            report.AppendLine();

            // 10. 报告结束
            report.AppendLine("════════════════════════════════════════════════════════════════════════════════");
            report.AppendLine("📄 报告结束");
            report.AppendLine($"生成工具: NGS BWA Runner - VCF专业分析模块 v1.0");
            report.AppendLine($"分析完成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

            await File.WriteAllTextAsync(reportPath, report.ToString());
            Utils.Logger.LogInfo($"VCF专业分析报告已保存: {reportPath}");
        }

        /// <summary>
        /// 追加文件信息
        /// </summary>
        private void AppendFileInfo(StringBuilder sb, string type, VcfAnalysisResult analysis)
        {
            sb.AppendLine($"📁 {type}文件信息:");
            sb.AppendLine($"   • 文件格式: {analysis.FileFormat ?? "未知"}");
            sb.AppendLine($"   • 参考基因组: {analysis.ReferenceGenome ?? "未指定"}");
            sb.AppendLine($"   • 总行数: {analysis.TotalLines:N0}");
            sb.AppendLine($"   • 总变异数: {analysis.TotalVariants:N0}");
            sb.AppendLine($"   • 样本数: {analysis.SampleCount}");

            if (analysis.IsSampled)
            {
                sb.AppendLine($"   • 采样分析: 基于 {analysis.SampleSize:N0} 个变异");
            }
        }

        /// <summary>
        /// 追加变异类型统计
        /// </summary>
        private void AppendVariantTypeStats(StringBuilder sb, string type, VariantTypeCounts counts)
        {
            if (counts == null) return;

            sb.AppendLine($"📊 {type}变异类型:");
            sb.AppendLine($"   • SNP: {counts.SnpCount:N0} ({GetPercentage(counts.SnpCount, counts.TotalVariants):F1}%)");
            sb.AppendLine($"   • 插入: {counts.InsertionCount:N0} ({GetPercentage(counts.InsertionCount, counts.TotalVariants):F1}%)");
            sb.AppendLine($"   • 缺失: {counts.DeletionCount:N0} ({GetPercentage(counts.DeletionCount, counts.TotalVariants):F1}%)");
            sb.AppendLine($"   • InDel合计: {counts.IndelCount:N0} ({GetPercentage(counts.IndelCount, counts.TotalVariants):F1}%)");
            sb.AppendLine($"   • 多等位基因: {counts.MultiallelicCount:N0}");
            sb.AppendLine($"   • MNP: {counts.MnpCount:N0}");
        }

        /// <summary>
        /// 追加质量指标
        /// </summary>
        private void AppendQualityMetrics(StringBuilder sb, string type, QualityMetrics metrics)
        {
            if (metrics == null) return;

            sb.AppendLine($"🎯 {type}质量指标:");

            if (metrics.QualValues.Count > 0)
            {
                sb.AppendLine($"   • QUAL平均值: {metrics.MeanQual:F1}");
                sb.AppendLine($"   • QUAL中位数: {metrics.MedianQual:F1}");
                sb.AppendLine($"   • QUAL标准差: {metrics.QualStdDev:F1}");
            }

            if (metrics.DepthValues.Count > 0)
            {
                sb.AppendLine($"   • 平均深度(DP): {metrics.MeanDepth:F1}");
                sb.AppendLine($"   • 深度中位数: {metrics.MedianDepth:F1}");
            }

            if (metrics.AfValues.Count > 0)
            {
                sb.AppendLine($"   • 平均等位基因频率(AF): {metrics.MeanAF:F3}");
                sb.AppendLine($"   • AF中位数: {metrics.MedianAF:F3}");
            }

            if (metrics.GqValues.Count > 0)
            {
                sb.AppendLine($"   • 平均基因型质量(GQ): {metrics.MeanGQ:F1}");
            }
        }

        /// <summary>
        /// 获取Ti/Tv评估
        /// </summary>
        private string GetTiTvAssessment(double tiTvRatio)
        {
            // 正常人类全基因组Ti/Tv比率约为2.0-2.1
            if (tiTvRatio >= 1.8 && tiTvRatio <= 2.3)
                return "✅ 正常范围 (2.0-2.1)";
            else if (tiTvRatio >= 1.5 && tiTvRatio < 1.8)
                return "⚠️  偏低 (可能过滤过度)";
            else if (tiTvRatio > 2.3 && tiTvRatio <= 3.0)
                return "⚠️  偏高 (可能过滤不足)";
            else if (tiTvRatio < 1.5)
                return "❌ 严重偏低 (可能数据质量问题)";
            else
                return "❌ 异常 (需检查数据)";
        }

        /// <summary>
        /// 计算百分比
        /// </summary>
        private double GetPercentage(long part, long total)
        {
            return total > 0 ? (double)part / total * 100 : 0;
        }

        /// <summary>
        /// 执行VCF质量评估
        /// </summary>
        private List<string> PerformVcfQualityAssessment(VcfAnalysisResult original, VcfAnalysisResult filtered)
        {
            var assessments = new List<string>();

            // 1. 变异数量评估
            if (original.TotalVariants < 1000)
            {
                assessments.Add("⚠️  变异数量较少 (<1000) - 可能是靶向测序或数据量不足");
            }
            else if (original.TotalVariants > 1000000)
            {
                assessments.Add("✅ 变异数量丰富 (>1M) - 适合全基因组分析");
            }
            else
            {
                assessments.Add("✅ 变异数量适中 - 适合外显子组或靶向测序分析");
            }

            // 2. Ti/Tv比率评估（如果可用）
            if (original.Statistics?.TiTvRatio > 0)
            {
                assessments.Add(GetTiTvAssessment(original.Statistics.TiTvRatio));
            }

            // 3. 过滤效果评估
            if (filtered != null)
            {
                double retentionRate = (double)filtered.TotalVariants / original.TotalVariants * 100;

                if (retentionRate >= 70)
                {
                    assessments.Add($"⚠️  过滤效果较弱 (保留{retentionRate:F1}%) - 可能过滤条件过于宽松");
                }
                else if (retentionRate >= 30 && retentionRate < 70)
                {
                    assessments.Add($"✅ 过滤效果适中 (保留{retentionRate:F1}%)");
                }
                else if (retentionRate >= 10 && retentionRate < 30)
                {
                    assessments.Add($"✅ 过滤效果明显 (保留{retentionRate:F1}%) - 适合严格分析");
                }
                else
                {
                    assessments.Add($"❌ 过滤过于严格 (保留{retentionRate:F1}%) - 可能丢失重要变异");
                }
            }

            // 4. 质量分数评估
            if (original.QualityMetrics?.MeanQual > 0)
            {
                if (original.QualityMetrics.MeanQual >= 50)
                {
                    assessments.Add("✅ 平均QUAL分数优秀 (≥50) - 变异可信度高");
                }
                else if (original.QualityMetrics.MeanQual >= 30)
                {
                    assessments.Add("⚠️  平均QUAL分数一般 (30-50) - 需谨慎解读");
                }
                else
                {
                    assessments.Add("❌ 平均QUAL分数较低 (<30) - 建议进一步过滤");
                }
            }

            // 5. InDel比例评估
            if (original.VariantTypeCounts?.TotalVariants > 0)
            {
                double indelRatio = (double)original.VariantTypeCounts.IndelCount / original.VariantTypeCounts.TotalVariants * 100;

                if (indelRatio >= 10 && indelRatio <= 20)
                {
                    assessments.Add($"✅ InDel比例正常 ({indelRatio:F1}%)");
                }
                else if (indelRatio < 5)
                {
                    assessments.Add($"⚠️  InDel比例偏低 ({indelRatio:F1}%) - 可能InDel检出不足");
                }
                else if (indelRatio > 30)
                {
                    assessments.Add($"⚠️  InDel比例偏高 ({indelRatio:F1}%) - 可能数据质量问题");
                }
            }

            return assessments;
        }

        /// <summary>
        /// 生成VCF分析建议
        /// </summary>
        private List<string> GenerateVcfRecommendations(VcfAnalysisResult original, VcfAnalysisResult filtered)
        {
            var recommendations = new List<string>();

            // 通用建议
            recommendations.Add("使用ANNOVAR或SnpEff进行变异功能注释，了解变异生物学意义");
            recommendations.Add("使用dbSNP、gnomAD、ClinVar等数据库进行变异频率和临床意义注释");

            // 基于数据的建议
            if (original?.TotalVariants < 5000)
            {
                recommendations.Add("变异数量较少，建议检查测序深度和覆盖度，确认是否为靶向测序");
            }

            if (original?.QualityMetrics?.MeanQual < 30)
            {
                recommendations.Add("平均QUAL分数较低，建议使用更严格的过滤条件（如QD<2.0, FS>60.0, MQ<40.0）");
            }

            if (filtered != null)
            {
                double retentionRate = (double)filtered.TotalVariants / original.TotalVariants * 100;

                if (retentionRate > 80)
                {
                    recommendations.Add("过滤保留率较高，考虑增加过滤严格度以减少假阳性");
                }
                else if (retentionRate < 20)
                {
                    recommendations.Add("过滤保留率较低，考虑放宽过滤条件以避免丢失真阳性变异");
                }
            }

            // 分析流程建议
            recommendations.Add("对于临床分析，建议遵循ACMG指南进行变异分类和解读");
            recommendations.Add("使用IGV等可视化工具检查关键变异在原始比对数据中的支持情况");
            recommendations.Add("考虑使用群体遗传学数据库（如千人基因组、gnomAD）进行等位基因频率过滤");

            // 报告和输出建议
            recommendations.Add("生成Excel格式的变异列表，便于临床医生或研究人员查看");
            recommendations.Add("创建变异分类报告，区分致病性、可能致病性、意义不明等类别");

            return recommendations;
        }
    }

    // ==================== 支持类定义 ====================

    /// <summary>
    /// VCF分析结果
    /// </summary>
    public class VcfAnalysisResult
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string AnalysisType { get; set; }
        public long TotalLines { get; set; }
        public long TotalVariants { get; set; }
        public int SampledVariants { get; set; }
        public bool IsSampled { get; set; }
        public int SampleSize { get; set; }
        public string FileFormat { get; set; }
        public string ReferenceGenome { get; set; }
        public string CommandLine { get; set; }
        public string SourceSoftware { get; set; }
        public int InfoFieldCount { get; set; }
        public int FormatFieldCount { get; set; }
        public bool HasFormatColumn { get; set; }
        public bool HasSampleColumns { get; set; }
        public int SampleCount { get; set; }
        public List<FilterDescription> FilterDescriptions { get; set; } = new List<FilterDescription>();
        public VcfStatistics Statistics { get; set; }
        public VariantTypeCounts VariantTypeCounts { get; set; }
        public QualityMetrics QualityMetrics { get; set; }
        public Dictionary<string, ChromosomeStats> ChromosomeDistribution { get; set; }
        public Dictionary<string, int> FilterStats { get; set; }
        public Dictionary<string, int> ConsequenceStats { get; set; }
        public DateTime AnalysisTimestamp { get; set; }
    }

    /// <summary>
    /// 过滤描述
    /// </summary>
    public class FilterDescription
    {
        public string Id { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// VCF统计信息
    /// </summary>
    public class VcfStatistics
    {
        // 基本统计
        public long TotalVariants { get; set; }
        public long VariantsWithId { get; set; }
        public long PassedVariants { get; set; }
        public long FilteredVariants { get; set; }
        public long HighQualityVariants { get; set; }
        public long LowQualityVariants { get; set; }
        public long VeryLowQualityVariants { get; set; }
        public long VariantsWithDepth { get; set; }
        public long VariantsWithAF { get; set; }
        public long VariantsWithGenotype { get; set; }
        public long VariantsWithSampleDepth { get; set; }
        public long VariantsWithGenotypeQuality { get; set; }
        public long TotalAlleles { get; set; }
        public long TotalDepth { get; set; }

        // 位置信息
        public int MinPosition { get; set; } = int.MaxValue;
        public int MaxPosition { get; set; } = int.MinValue;

        // 质量指标
        public double TiTvRatio { get; set; }
        public double HeterozygousRate { get; set; }
        public double HomozygousAltRate { get; set; }

        // 分布
        public Dictionary<string, ChromosomeStats> TopChromosomes { get; set; }
    }

    /// <summary>
    /// 变异类型计数
    /// </summary>
    public class VariantTypeCounts
    {
        public long TotalVariants { get; set; }
        public long SnpCount { get; set; }
        public long InsertionCount { get; set; }
        public long DeletionCount { get; set; }
        public long IndelCount { get; set; }
        public long MnpCount { get; set; }
        public long MultiallelicCount { get; set; }
        public long Transitions { get; set; }
        public long Transversions { get; set; }
    }

    /// <summary>
    /// 质量指标
    /// </summary>
    /// <summary>
    /// 质量指标
    /// </summary>
    public class QualityMetrics
    {
        // QUAL分数
        public List<double> QualValues { get; set; } = new List<double>();
        public double MeanQual { get; set; }
        public double MedianQual { get; set; }
        public double QualStdDev { get; set; }

        // 深度相关
        public List<int> DepthValues { get; set; } = new List<int>();
        public double MeanDepth { get; set; }
        public double MedianDepth { get; set; }

        // 等位基因频率
        public List<double> AfValues { get; set; } = new List<double>();
        public double MeanAF { get; set; }
        public double MedianAF { get; set; }

        // 其他质量指标
        public List<double> MqValues { get; set; } = new List<double>();
        public List<double> QdValues { get; set; } = new List<double>();
        public List<double> FsValues { get; set; } = new List<double>();  // 修正：改为 List<double>
        public List<int> Mq0Values { get; set; } = new List<int>();

        // 样本相关
        public List<string> Genotypes { get; set; } = new List<string>();
        public List<int> SampleDepthValues { get; set; } = new List<int>();
        public List<int> GqValues { get; set; } = new List<int>();
        public List<int> AdValues { get; set; } = new List<int>();

        // 计算值
        public double MeanGQ { get; set; }
        public double MedianGQ { get; set; }
    }
    /// <summary>
    /// 染色体统计
    /// </summary>
    public class ChromosomeStats
    {
        public string Chromosome { get; set; }
        public long VariantCount { get; set; }

        // 可选：添加其他染色体相关统计
        public long SnpCount { get; set; }
        public long IndelCount { get; set; }
        public double AverageQuality { get; set; }
    }
}