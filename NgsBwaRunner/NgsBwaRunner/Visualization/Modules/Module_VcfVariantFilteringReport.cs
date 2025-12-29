// Visualization/Modules/Module_VcfVariantFilteringReport.cs
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
    /// VCF变异筛选和分类报告模块
    /// 基于临床和研究标准对变异进行筛选、分类和优先级排序
    /// </summary>
    [VisualizationModule(
        "vcf.variant.filtering",
        "变异筛选分类报告",
        "基于临床/研究标准对变异进行筛选、分类和优先级排序的详细报告",
        85
    )]
    public class Module_VcfVariantFilteringReport : IVisualizationModule
    {
        // 模块属性
        public string ModuleId => "vcf.variant.filtering";
        public string DisplayName => "变异筛选分类报告";
        public string Description => "基于临床/研究标准对变异进行筛选、分类和优先级排序的详细报告";
        public int Priority => 85;
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 执行模块生成筛选分类报告
        /// </summary>
        public async Task<VisualizationResult> ExecuteAsync(VisualizationContext context)
        {
            try
            {
                // 确定要分析的VCF文件（优先使用过滤后的）
                string vcfFilePath = !string.IsNullOrEmpty(context.FilteredVcfFilePath) &&
                                    File.Exists(context.FilteredVcfFilePath) ?
                                    context.FilteredVcfFilePath :
                                    context.VcfFilePath;

                if (string.IsNullOrEmpty(vcfFilePath) || !File.Exists(vcfFilePath))
                {
                    return VisualizationResult.ErrorResult("VCF文件不存在",
                        $"VCF文件路径: {vcfFilePath ?? "未提供"}");
                }

                // 报告进度
                context.ProgressReporter?.Invoke("开始分析变异筛选分类...", 10);

                // 解析VCF文件并筛选变异
                var filteringResult = await AnalyzeAndFilterVariantsAsync(
                    vcfFilePath, context.ProgressReporter, 10, 70);

                // 生成报告
                context.ProgressReporter?.Invoke("生成筛选分类报告...", 70);

                string reportFileName = $"{context.SampleName ?? "unknown"}_variant_filtering_report.txt";
                string reportPath = Path.Combine(context.OutputDirectory ?? ".", reportFileName);

                await GenerateFilteringReportAsync(filteringResult, reportPath, context);

                context.ProgressReporter?.Invoke("变异筛选分类报告完成", 100);

                // 准备返回消息
                var message = new StringBuilder();
                message.AppendLine("✅ 变异筛选分类报告生成成功");
                message.AppendLine($"📊 分析变异数: {filteringResult.TotalVariants:N0}");
                message.AppendLine($"🎯 高优先级变异: {filteringResult.HighPriorityVariants:N0}");
                message.AppendLine($"⚠️  中等优先级变异: {filteringResult.MediumPriorityVariants:N0}");
                message.AppendLine($"📁 报告文件: {reportFileName}");

                return VisualizationResult.SuccessResult(
                    message.ToString(),
                    reportPath
                );
            }
            catch (Exception ex)
            {
                Utils.Logger.LogError($"变异筛选分类模块执行失败: {ex.Message}");
                return VisualizationResult.ErrorResult(
                    $"生成变异筛选分类报告失败: {ex.Message}",
                    ex.ToString()
                );
            }
        }

        /// <summary>
        /// 分析和筛选变异
        /// </summary>
        private async Task<VariantFilteringResult> AnalyzeAndFilterVariantsAsync(
            string vcfFilePath, Action<string, int> progressReporter,
            int startProgress, int endProgress)
        {
            var result = new VariantFilteringResult
            {
                FilePath = vcfFilePath,
                FileName = Path.GetFileName(vcfFilePath)
            };

            var variants = new List<VariantRecord>();
            var priorityCounts = new Dictionary<PriorityLevel, int>
            {
                { PriorityLevel.High, 0 },
                { PriorityLevel.Medium, 0 },
                { PriorityLevel.Low, 0 }
            };

            var categoryCounts = new Dictionary<VariantCategory, int>
            {
                { VariantCategory.Exonic, 0 },
                { VariantCategory.Splicing, 0 },
                { VariantCategory.Regulatory, 0 },
                { VariantCategory.Noncoding, 0 },
                { VariantCategory.Intergenic, 0 }
            };

            int lineCount = 0;
            int variantCount = 0;

            using (var reader = new StreamReader(vcfFilePath))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    lineCount++;

                    // 跳过注释行
                    if (line.StartsWith("#"))
                        continue;

                    // 进度报告
                    if (lineCount % 1000 == 0)
                    {
                        int progress = startProgress + (int)((endProgress - startProgress) *
                            (double)lineCount / 100000);
                        progressReporter?.Invoke($"分析变异... 已处理 {lineCount:N0} 行", progress);
                    }

                    // 解析变异记录
                    var variant = ParseVariantRecord(line);
                    if (variant != null)
                    {
                        variants.Add(variant);
                        variantCount++;

                        // 确定优先级
                        var priority = DetermineVariantPriority(variant);
                        priorityCounts[priority]++;

                        // 分类计数
                        var category = DetermineVariantCategory(variant);
                        if (categoryCounts.ContainsKey(category))
                            categoryCounts[category]++;
                    }
                }
            }

            // 筛选高优先级变异
            var highPriorityVariants = variants
                .Where(v => DetermineVariantPriority(v) == PriorityLevel.High)
                .ToList();

            // 筛选中等优先级变异
            var mediumPriorityVariants = variants
                .Where(v => DetermineVariantPriority(v) == PriorityLevel.Medium)
                .ToList();

            // 保存结果
            result.TotalVariants = variantCount;
            result.AllVariants = variants;
            result.HighPriorityVariants = highPriorityVariants;
            result.MediumPriorityVariants = mediumPriorityVariants;
            result.PriorityCounts = priorityCounts;
            result.CategoryCounts = categoryCounts;
            result.HighPriorityCount = priorityCounts[PriorityLevel.High];
            result.MediumPriorityCount = priorityCounts[PriorityLevel.Medium];
            result.LowPriorityCount = priorityCounts[PriorityLevel.Low];
            result.AnalysisTimestamp = DateTime.Now;

            return result;
        }

        /// <summary>
        /// 解析变异记录
        /// </summary>
        private VariantRecord ParseVariantRecord(string line)
        {
            var fields = line.Split('\t');
            if (fields.Length < 8) return null;

            var variant = new VariantRecord
            {
                Chromosome = fields[0],
                Position = int.Parse(fields[1]),
                Id = fields[2],
                Reference = fields[3],
                Alternate = fields[4],
                Quality = double.TryParse(fields[5], out double qual) ? qual : 0,
                Filter = fields[6],
                Info = fields.Length > 7 ? fields[7] : "",
                Format = fields.Length > 8 ? fields[8] : "",
                Samples = fields.Length > 9 ? fields.Skip(9).ToArray() : new string[0]
            };

            // 解析INFO字段
            ParseVariantInfo(variant);

            return variant;
        }

        /// <summary>
        /// 解析变异INFO字段
        /// </summary>
        private void ParseVariantInfo(VariantRecord variant)
        {
            if (string.IsNullOrEmpty(variant.Info) || variant.Info == ".")
                return;

            var infoPairs = variant.Info.Split(';');

            foreach (var pair in infoPairs)
            {
                var keyValue = pair.Split('=');
                if (keyValue.Length < 2) continue;

                string key = keyValue[0];
                string value = keyValue[1];

                switch (key)
                {
                    case "DP":
                        if (int.TryParse(value, out int dp))
                            variant.Depth = dp;
                        break;

                    case "AF":
                        var afValues = value.Split(',');
                        if (afValues.Length > 0 && double.TryParse(afValues[0], out double af))
                            variant.AlleleFrequency = af;
                        break;

                    case "MQ":
                        if (double.TryParse(value, out double mq))
                            variant.MappingQuality = mq;
                        break;

                    case "QD":
                        if (double.TryParse(value, out double qd))
                            variant.QualityDepth = qd;
                        break;

                    case "FS":
                        if (double.TryParse(value, out double fs))
                            variant.StrandBias = fs;
                        break;

                    case "CSQ":
                    case "ANN":
                        variant.Annotation = value;
                        ParseConsequenceInfo(variant, value);
                        break;
                }
            }
        }

        /// <summary>
        /// 解析变异后果信息
        /// </summary>
        private void ParseConsequenceInfo(VariantRecord variant, string annotation)
        {
            try
            {
                // 简化处理：取第一个注释
                var consequence = annotation.Split('|').FirstOrDefault();
                if (!string.IsNullOrEmpty(consequence))
                {
                    variant.Consequence = consequence;

                    // 判断是否为功能影响变异
                    var significantConsequences = new[]
                    {
                        "stop_gained", "stop_lost", "start_lost", "frameshift_variant",
                        "splice_acceptor_variant", "splice_donor_variant", "missense_variant"
                    };

                    variant.IsFunctionallySignificant = significantConsequences
                        .Any(c => consequence.Contains(c));
                }
            }
            catch
            {
                // 忽略解析错误
            }
        }

        /// <summary>
        /// 确定变异优先级
        /// </summary>
        private PriorityLevel DetermineVariantPriority(VariantRecord variant)
        {
            // 优先级判定逻辑
            var criteria = new List<VariantPriorityCriterion>();
            int priorityScore = 0;

            // 1. 功能重要性
            if (variant.IsFunctionallySignificant)
            {
                priorityScore += 3;
                criteria.Add(new VariantPriorityCriterion
                {
                    Name = "功能重要变异",
                    Score = 3,
                    Description = "影响蛋白功能的变异（错义、无义、移码等）"
                });
            }

            // 2. 质量分数
            if (variant.Quality >= 50)
            {
                priorityScore += 2;
                criteria.Add(new VariantPriorityCriterion
                {
                    Name = "高可信度变异",
                    Score = 2,
                    Description = "质量分数 ≥ 50"
                });
            }
            else if (variant.Quality >= 30)
            {
                priorityScore += 1;
                criteria.Add(new VariantPriorityCriterion
                {
                    Name = "中等可信度变异",
                    Score = 1,
                    Description = "质量分数 30-50"
                });
            }

            // 3. 深度
            if (variant.Depth >= 20)
            {
                priorityScore += 2;
                criteria.Add(new VariantPriorityCriterion
                {
                    Name = "深度充分",
                    Score = 2,
                    Description = "测序深度 ≥ 20X"
                });
            }
            else if (variant.Depth >= 10)
            {
                priorityScore += 1;
                criteria.Add(new VariantPriorityCriterion
                {
                    Name = "深度适中",
                    Score = 1,
                    Description = "测序深度 10-20X"
                });
            }

            // 4. 等位基因频率（低频变异更值得关注）
            if (variant.AlleleFrequency > 0 && variant.AlleleFrequency < 0.01)
            {
                priorityScore += 2;
                criteria.Add(new VariantPriorityCriterion
                {
                    Name = "罕见变异",
                    Score = 2,
                    Description = "等位基因频率 < 1%"
                });
            }

            // 5. 过滤状态
            if (variant.Filter == "PASS" || variant.Filter == ".")
            {
                priorityScore += 1;
                criteria.Add(new VariantPriorityCriterion
                {
                    Name = "通过过滤",
                    Score = 1,
                    Description = "通过所有质量控制过滤"
                });
            }

            variant.PriorityScore = priorityScore;
            variant.PriorityCriteria = criteria;

            // 基于总分确定优先级
            if (priorityScore >= 5)
                return PriorityLevel.High;
            else if (priorityScore >= 3)
                return PriorityLevel.Medium;
            else
                return PriorityLevel.Low;
        }

        /// <summary>
        /// 确定变异类别
        /// </summary>
        private VariantCategory DetermineVariantCategory(VariantRecord variant)
        {
            if (string.IsNullOrEmpty(variant.Consequence))
                return VariantCategory.Unknown;

            var consequence = variant.Consequence.ToLower();

            if (consequence.Contains("exon") || consequence.Contains("missense") ||
                consequence.Contains("nonsense") || consequence.Contains("frameshift"))
                return VariantCategory.Exonic;

            if (consequence.Contains("splice"))
                return VariantCategory.Splicing;

            if (consequence.Contains("regulatory") || consequence.Contains("promoter") ||
                consequence.Contains("enhancer"))
                return VariantCategory.Regulatory;

            if (consequence.Contains("intron") || consequence.Contains("utr") ||
                consequence.Contains("non_coding"))
                return VariantCategory.Noncoding;

            return VariantCategory.Intergenic;
        }

        /// <summary>
        /// 生成筛选分类报告
        /// </summary>
        private async Task GenerateFilteringReportAsync(
            VariantFilteringResult result, string reportPath, VisualizationContext context)
        {
            var report = new StringBuilder();

            // 1. 报告标题
            report.AppendLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            report.AppendLine("║                    变异筛选和分类报告                                       ║");
            report.AppendLine("║                基于临床/研究标准的变异优先级排序                            ║");
            report.AppendLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            report.AppendLine();
            report.AppendLine($"📅 报告生成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"📁 样本名称: {context.SampleName ?? "未知样本"}");
            report.AppendLine($"📊 分析文件: {result.FileName}");
            report.AppendLine();

            // 2. 总览统计
            report.AppendLine("════════════════════════════════════════════════════════════════════════════════");
            report.AppendLine("📈 变异筛选总览");
            report.AppendLine("────────────────────────────────────────────────────────────────────────────────");

            int highPriority = result.PriorityCounts[PriorityLevel.High];
            int mediumPriority = result.PriorityCounts[PriorityLevel.Medium];
            int lowPriority = result.PriorityCounts[PriorityLevel.Low];

            report.AppendLine($"   • 总变异数: {result.TotalVariants:N0}");
            report.AppendLine($"   • 🎯 高优先级变异: {highPriority:N0} ({GetPercentage(highPriority, result.TotalVariants):F1}%)");
            report.AppendLine($"   • ⚠️  中等优先级变异: {mediumPriority:N0} ({GetPercentage(mediumPriority, result.TotalVariants):F1}%)");
            report.AppendLine($"   • 📄 低优先级变异: {lowPriority:N0} ({GetPercentage(lowPriority, result.TotalVariants):F1}%)");
            report.AppendLine();

            // 3. 变异类别分布
            report.AppendLine("════════════════════════════════════════════════════════════════════════════════");
            report.AppendLine("🧬 变异功能类别分布");
            report.AppendLine("────────────────────────────────────────────────────────────────────────────────");

            foreach (var kv in result.CategoryCounts.OrderByDescending(x => x.Value))
            {
                string categoryName = GetCategoryDisplayName(kv.Key);
                double percentage = GetPercentage(kv.Value, result.TotalVariants);
                string icon = GetCategoryIcon(kv.Key);

                report.AppendLine($"   • {icon} {categoryName,-20} {kv.Value,8:N0} ({percentage,6:F1}%)");
            }
            report.AppendLine();

            // 4. 高优先级变异列表（前20个）
            if (result.HighPriorityVariants.Count > 0)
            {
                report.AppendLine("════════════════════════════════════════════════════════════════════════════════");
                report.AppendLine("🎯 高优先级变异列表（按优先级排序）");
                report.AppendLine("────────────────────────────────────────────────────────────────────────────────");

                int countToShow = Math.Min(20, result.HighPriorityVariants.Count);
                var topHighPriority = result.HighPriorityVariants
                    .OrderByDescending(v => v.PriorityScore)
                    .Take(countToShow)
                    .ToList();

                report.AppendLine($"   {"位置",-20} {"类型",-15} {"功能",-20} {"得分",-6} {"说明"}");
                report.AppendLine($" {new string('-', 20)} {new string('-', 15)} {new string('-', 20)} {new string('-', 6)} {new string('-', 30)}");

                foreach (var variant in topHighPriority)
                {
                    string location = $"{variant.Chromosome}:{variant.Position}";
                    string type = GetVariantType(variant);
                    string function = variant.Consequence?.Split('|').FirstOrDefault() ?? "未知";
                    string score = $"{variant.PriorityScore}";
                    string description = GetPriorityDescription(variant);

                    report.AppendLine($"   • {location,-20} {type,-15} {function,-20} {score,-6} {description}");
                }

                if (result.HighPriorityVariants.Count > countToShow)
                {
                    report.AppendLine($"   ... 还有 {result.HighPriorityVariants.Count - countToShow} 个高优先级变异");
                }
                report.AppendLine();
            }

            // 5. 优先级判定标准说明
            report.AppendLine("════════════════════════════════════════════════════════════════════════════════");
            report.AppendLine("📋 优先级判定标准");
            report.AppendLine("────────────────────────────────────────────────────────────────────────────────");

            report.AppendLine("   高优先级 (5+分): 需要立即关注和验证的变异");
            report.AppendLine("   中等优先级 (3-4分): 值得进一步分析的变异");
            report.AppendLine("   低优先级 (0-2分): 常规变异，可作为参考");
            report.AppendLine();

            report.AppendLine("   评分标准:");
            report.AppendLine("   - 功能重要变异: +3分 (错义、无义、移码等)");
            report.AppendLine("   - 高可信度变异: +2分 (QUAL ≥ 50)");
            report.AppendLine("   - 中等可信度变异: +1分 (QUAL 30-50)");
            report.AppendLine("   - 深度充分: +2分 (DP ≥ 20)");
            report.AppendLine("   - 深度适中: +1分 (DP 10-20)");
            report.AppendLine("   - 罕见变异: +2分 (AF < 1%)");
            report.AppendLine("   - 通过过滤: +1分 (FILTER = PASS)");
            report.AppendLine();

            // 6. 筛选建议
            report.AppendLine("════════════════════════════════════════════════════════════════════════════════");
            report.AppendLine("💡 后续分析建议");
            report.AppendLine("────────────────────────────────────────────────────────────────────────────────");

            var recommendations = GenerateFilteringRecommendations(result);
            for (int i = 0; i < recommendations.Count; i++)
            {
                report.AppendLine($"{i + 1}. {recommendations[i]}");
            }
            report.AppendLine();

            // 7. 生成可导出的变异列表文件
            if (result.HighPriorityVariants.Count > 0)
            {
                string variantListPath = GenerateVariantListFile(result, context);
                report.AppendLine("════════════════════════════════════════════════════════════════════════════════");
                report.AppendLine("📁 导出文件");
                report.AppendLine("────────────────────────────────────────────────────────────────────────────────");
                report.AppendLine($"   高优先级变异列表: {Path.GetFileName(variantListPath)}");
                report.AppendLine($"   包含 {result.HighPriorityVariants.Count:N0} 个高优先级变异的详细信息");
                report.AppendLine();
            }

            // 8. 报告结束
            report.AppendLine("════════════════════════════════════════════════════════════════════════════════");
            report.AppendLine("📄 报告结束");
            report.AppendLine($"生成工具: NGS BWA Runner - 变异筛选分类模块 v1.0");
            report.AppendLine($"分析完成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

            await File.WriteAllTextAsync(reportPath, report.ToString());
            Utils.Logger.LogInfo($"变异筛选分类报告已保存: {reportPath}");
        }

        /// <summary>
        /// 生成变异列表文件
        /// </summary>
        private string GenerateVariantListFile(VariantFilteringResult result, VisualizationContext context)
        {
            string fileName = $"{context.SampleName ?? "unknown"}_high_priority_variants.tsv";
            string filePath = Path.Combine(context.OutputDirectory ?? ".", fileName);

            var sb = new StringBuilder();

            // 表头
            sb.AppendLine("染色体\t位置\tID\t参考\t替代\t质量\t过滤\t深度\tAF\t功能\t优先级得分\t优先级");

            // 数据行
            var sortedVariants = result.HighPriorityVariants
                .OrderByDescending(v => v.PriorityScore)
                .ThenBy(v => v.Chromosome)
                .ThenBy(v => v.Position);

            foreach (var variant in sortedVariants)
            {
                sb.AppendLine($"{variant.Chromosome}\t" +
                             $"{variant.Position}\t" +
                             $"{variant.Id}\t" +
                             $"{variant.Reference}\t" +
                             $"{variant.Alternate}\t" +
                             $"{variant.Quality:F1}\t" +
                             $"{variant.Filter}\t" +
                             $"{variant.Depth}\t" +
                             $"{variant.AlleleFrequency:F4}\t" +
                             $"{variant.Consequence ?? "未知"}\t" +
                             $"{variant.PriorityScore}\t" +
                             $"高优先级");
            }

            File.WriteAllText(filePath, sb.ToString());
            return filePath;
        }

        /// <summary>
        /// 计算百分比
        /// </summary>
        private double GetPercentage(int part, int total)
        {
            return total > 0 ? (double)part / total * 100 : 0;
        }

        /// <summary>
        /// 获取类别显示名称
        /// </summary>
        private string GetCategoryDisplayName(VariantCategory category)
        {
            switch (category)
            {
                case VariantCategory.Exonic: return "编码区变异";
                case VariantCategory.Splicing: return "剪接位点变异";
                case VariantCategory.Regulatory: return "调控区变异";
                case VariantCategory.Noncoding: return "非编码区变异";
                case VariantCategory.Intergenic: return "基因间区变异";
                default: return "未知类别";
            }
        }

        /// <summary>
        /// 获取类别图标
        /// </summary>
        private string GetCategoryIcon(VariantCategory category)
        {
            switch (category)
            {
                case VariantCategory.Exonic: return "🧬";
                case VariantCategory.Splicing: return "✂️";
                case VariantCategory.Regulatory: return "🎛️";
                case VariantCategory.Noncoding: return "📄";
                case VariantCategory.Intergenic: return "🌌";
                default: return "❓";
            }
        }

        /// <summary>
        /// 获取变异类型
        /// </summary>
        private string GetVariantType(VariantRecord variant)
        {
            if (variant.Reference.Length == 1 && variant.Alternate.Length == 1)
                return "SNP";
            else if (variant.Reference.Length > variant.Alternate.Length)
                return "缺失";
            else if (variant.Reference.Length < variant.Alternate.Length)
                return "插入";
            else
                return "MNP";
        }

        /// <summary>
        /// 获取优先级描述
        /// </summary>
        private string GetPriorityDescription(VariantRecord variant)
        {
            var reasons = new List<string>();

            if (variant.IsFunctionallySignificant)
                reasons.Add("功能重要");
            if (variant.Quality >= 50)
                reasons.Add("高可信度");
            if (variant.Depth >= 20)
                reasons.Add("深度充分");
            if (variant.AlleleFrequency < 0.01 && variant.AlleleFrequency > 0)
                reasons.Add("罕见变异");

            return string.Join(" + ", reasons);
        }

        /// <summary>
        /// 生成筛选建议
        /// </summary>
        private List<string> GenerateFilteringRecommendations(VariantFilteringResult result)
        {
            var recommendations = new List<string>();

            recommendations.Add("优先验证高优先级变异，特别是功能重要的变异");

            if (result.HighPriorityVariants.Count > 10)
            {
                recommendations.Add($"发现{result.HighPriorityVariants.Count}个高优先级变异，建议使用Sanger测序验证关键变异");
            }

            if (result.PriorityCounts[PriorityLevel.High] == 0)
            {
                recommendations.Add("未发现高优先级变异，可能需要调整筛选标准或检查数据质量");
            }

            recommendations.Add("对于临床样本，建议将高优先级变异与临床表型关联分析");
            recommendations.Add("使用IGV等工具可视化高优先级变异在原始BAM文件中的支持情况");
            recommendations.Add("将变异列表导入ANNOVAR进行数据库注释，获取更多临床信息");

            return recommendations;
        }
    }

    // ==================== 支持类定义 ====================

    /// <summary>
    /// 变异筛选结果
    /// </summary>
    public class VariantFilteringResult
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public int TotalVariants { get; set; }
        public List<VariantRecord> AllVariants { get; set; }
        public List<VariantRecord> HighPriorityVariants { get; set; }
        public List<VariantRecord> MediumPriorityVariants { get; set; }
        public Dictionary<PriorityLevel, int> PriorityCounts { get; set; }
        public Dictionary<VariantCategory, int> CategoryCounts { get; set; }
        public int HighPriorityCount { get; set; }
        public int MediumPriorityCount { get; set; }
        public int LowPriorityCount { get; set; }
        public DateTime AnalysisTimestamp { get; set; }
    }

    /// <summary>
    /// 变异记录
    /// </summary>
    public class VariantRecord
    {
        public string Chromosome { get; set; }
        public int Position { get; set; }
        public string Id { get; set; }
        public string Reference { get; set; }
        public string Alternate { get; set; }
        public double Quality { get; set; }
        public string Filter { get; set; }
        public string Info { get; set; }
        public string Format { get; set; }
        public string[] Samples { get; set; }

        // 解析的字段
        public int Depth { get; set; }
        public double AlleleFrequency { get; set; }
        public double MappingQuality { get; set; }
        public double QualityDepth { get; set; }
        public double StrandBias { get; set; }
        public string Annotation { get; set; }
        public string Consequence { get; set; }
        public bool IsFunctionallySignificant { get; set; }

        // 优先级信息
        public int PriorityScore { get; set; }
        public PriorityLevel Priority { get; set; }
        public List<VariantPriorityCriterion> PriorityCriteria { get; set; }
    }

    /// <summary>
    /// 优先级标准
    /// </summary>
    public class VariantPriorityCriterion
    {
        public string Name { get; set; }
        public int Score { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// 优先级级别
    /// </summary>
    public enum PriorityLevel
    {
        High,
        Medium,
        Low
    }

    /// <summary>
    /// 变异类别
    /// </summary>
    public enum VariantCategory
    {
        Exonic,
        Splicing,
        Regulatory,
        Noncoding,
        Intergenic,
        Unknown
    }
}