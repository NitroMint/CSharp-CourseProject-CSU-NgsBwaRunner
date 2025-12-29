// Visualization/Modules/Module_SamProfessionalAnalysis.cs
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
    /// SAM文件专业级分析报告模块（完全修复版）
    /// 支持单端和双端测序，包含完善的MAPQ评估体系
    /// </summary>
    [VisualizationModule(
        "sam.professional.analysis",
        "SAM专业分析报告",
        "生成SAM文件的专业级生物信息学分析报告，支持单端/双端测序，包含深度统计和质量评估",
        90
    )]
    public class Module_SamProfessionalAnalysis : IVisualizationModule
    {
        // 模块属性
        public string ModuleId => "sam.professional.analysis";
        public string DisplayName => "SAM专业分析报告";
        public string Description => "生成SAM文件的专业级生物信息学分析报告，支持单端/双端测序，包含深度统计和质量评估";
        public int Priority => 90;
        public bool IsEnabled { get; set; } = true;

        // 私有变量
        private List<int> _mapqValues = new List<int>();
        private List<int> _readLengths = new List<int>();
        private List<int> _insertSizes = new List<int>();
        private bool _isPairedEnd = false;
        private int _totalPairedReads = 0;

        /// <summary>
        /// 执行模块生成专业报告
        /// </summary>
        public async Task<VisualizationResult> ExecuteAsync(VisualizationContext context)
        {
            try
            {
                // 验证输入文件
                if (string.IsNullOrEmpty(context.SamFilePath) || !File.Exists(context.SamFilePath))
                {
                    return VisualizationResult.ErrorResult("SAM文件不存在或路径无效",
                        $"SAM文件路径: {context.SamFilePath ?? "null"}");
                }

                // 报告进度
                context.ProgressReporter?.Invoke("开始深度解析SAM文件...", 10);

                // 执行深度分析
                var analysis = await PerformDeepAnalysisAsync(context.SamFilePath, context.ProgressReporter);

                if (analysis.TotalReads == 0)
                {
                    return VisualizationResult.ErrorResult("SAM文件为空或格式无效");
                }

                // 生成报告
                context.ProgressReporter?.Invoke("生成专业分析报告...", 70);

                string reportFileName = $"{context.SampleName ?? "unknown"}_sam_professional_analysis.txt";
                string reportPath = Path.Combine(context.OutputDirectory ?? ".", reportFileName);

                await GenerateProfessionalReportAsync(analysis, reportPath, context);

                context.ProgressReporter?.Invoke("SAM专业分析完成", 100);

                return VisualizationResult.SuccessResult(
                    $"✅ SAM专业分析报告生成成功\n" +
                    $"📊 总Reads: {analysis.TotalReads:N0} ({(_isPairedEnd ? "双端" : "单端")})\n" +
                    $"📈 比对率: {analysis.AlignmentRate:P1}\n" +
                    $"🎯 平均MAPQ: {analysis.Statistics.MeanMAPQ:F1}\n" +
                    $"📁 报告文件: {reportFileName}",
                    reportPath
                );
            }
            catch (Exception ex)
            {
                Utils.Logger.LogError($"SAM专业分析模块执行失败: {ex.Message}");
                return VisualizationResult.ErrorResult(
                    $"生成SAM专业分析报告失败: {ex.Message}",
                    ex.ToString()
                );
            }
        }

        /// <summary>
        /// 执行深度分析
        /// </summary>
        private async Task<SamAnalysisResult> PerformDeepAnalysisAsync(string samFilePath, Action<string, int> progressReporter)
        {
            var result = new SamAnalysisResult();
            var stats = new SamStatistics();

            // 初始化列表
            _mapqValues.Clear();
            _readLengths.Clear();
            _insertSizes.Clear();
            _isPairedEnd = false;
            _totalPairedReads = 0;

            // CIGAR操作统计
            var cigarStats = new Dictionary<string, long>
            {
                ["M"] = 0,
                ["I"] = 0,
                ["D"] = 0,
                ["S"] = 0,
                ["H"] = 0,
                ["N"] = 0,
                ["P"] = 0,
                ["="] = 0,
                ["X"] = 0
            };

            // 用于检测测序类型的临时变量
            int firstReadFlag = -1;
            bool pairedEndDetected = false;

            int lineCount = 0;
            int sampledReads = 0;
            const int maxSamples = 50000;
            int totalLines = await CountLinesAsync(samFilePath);

            using (var reader = new StreamReader(samFilePath))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    lineCount++;

                    // 跳过头部行
                    if (line.StartsWith("@"))
                    {
                        ParseHeaderLine(line, result);
                        continue;
                    }

                    // 进度报告
                    if (lineCount % 5000 == 0)
                    {
                        progressReporter?.Invoke($"深度解析中... 已处理 {lineCount:N0} 行",
                            15 + (int)(40.0 * lineCount / Math.Min(totalLines, maxSamples * 2)));
                    }

                    // 采样限制
                    if (sampledReads >= maxSamples)
                    {
                        result.IsSampled = true;
                        result.SampleSize = maxSamples;
                        break;
                    }

                    // 解析SAM记录
                    ParseSamRecord(line, stats, cigarStats, ref pairedEndDetected, ref firstReadFlag);
                    sampledReads++;
                }
            }

            // 确定测序类型
            _isPairedEnd = pairedEndDetected;
            result.IsPairedEnd = _isPairedEnd;

            // 计算衍生统计
            CalculateStatistics(result, stats, cigarStats);

            result.TotalLines = lineCount;
            result.TotalReads = stats.TotalReads;
            result.SampledReads = sampledReads;
            result.Statistics = stats;

            return result;
        }

        /// <summary>
        /// 统计文件行数
        /// </summary>
        private async Task<int> CountLinesAsync(string filePath)
        {
            int count = 0;
            using (var reader = new StreamReader(filePath))
            {
                while (await reader.ReadLineAsync() != null)
                {
                    count++;
                    if (count > 100000) break; // 只统计前10万行用于进度估算
                }
            }
            return count;
        }

        /// <summary>
        /// 解析头部行
        /// </summary>
        private void ParseHeaderLine(string line, SamAnalysisResult result)
        {
            if (line.StartsWith("@SQ"))
            {
                // 参考序列信息
                var parts = line.Split('\t');
                string sn = "";
                long ln = 0;

                foreach (var part in parts)
                {
                    if (part.StartsWith("SN:"))
                        sn = part.Substring(3);
                    else if (part.StartsWith("LN:"))
                        long.TryParse(part.Substring(3), out ln);
                }

                if (!string.IsNullOrEmpty(sn) && ln > 0)
                {
                    result.ReferenceSequences[sn] = ln;
                }
            }
            else if (line.StartsWith("@RG"))
            {
                result.ReadGroups.Add(line);
            }
            else if (line.StartsWith("@PG"))
            {
                result.Programs.Add(line);

                // 从程序信息推断测序类型
                if (line.Contains("bwa-mem") || line.Contains("paired") || line.Contains("PE"))
                {
                    _isPairedEnd = true;
                }
            }
        }

        /// <summary>
        /// 解析SAM记录
        /// </summary>
        private void ParseSamRecord(string line, SamStatistics stats, Dictionary<string, long> cigarStats,
            ref bool pairedEndDetected, ref int firstReadFlag)
        {
            var fields = line.Split('\t');
            if (fields.Length < 11) return;

            stats.TotalReads++;

            // 1. 解析FLAG
            if (int.TryParse(fields[1], out int flag))
            {
                // 检测测序类型（只检测前几条记录）
                if (firstReadFlag == -1)
                {
                    firstReadFlag = flag;
                }
                else if (!pairedEndDetected)
                {
                    // 检查是否是配对read的标志位（0x1）
                    if ((flag & 0x1) != 0 || (firstReadFlag & 0x1) != 0)
                    {
                        pairedEndDetected = true;
                        _isPairedEnd = true;
                    }
                }

                UpdateFlagStatistics(stats, flag);
            }

            // 2. MAPQ
            if (int.TryParse(fields[4], out int mapq))
            {
                _mapqValues.Add(mapq);

                // 根据MAPQ值分类
                if (mapq >= 40) stats.VeryHighQualityReads++;
                else if (mapq >= 30) stats.HighQualityReads++;
                else if (mapq >= 20) stats.MediumQualityReads++;
                else if (mapq >= 10) stats.LowQualityReads++;
                else stats.VeryLowQualityReads++;

                if (mapq == 0) stats.ZeroMAPQReads++;
                if (mapq == 255) stats.MaxMAPQReads++;
            }

            // 3. CIGAR
            string cigar = fields[5];
            if (cigar != "*")
            {
                ParseCigarString(cigar, cigarStats, stats);
            }
            else
            {
                stats.ReadsWithNoCigar++;
            }

            // 4. Read长度
            string sequence = fields.Length > 9 ? fields[9] : "";
            int readLength = GetReadLength(sequence, cigar);
            _readLengths.Add(readLength);
            stats.TotalBases += readLength;

            // 5. 插入片段长度（仅对配对测序有意义）
            if (fields.Length > 8 && fields[8] != "0" && fields[8] != "*")
            {
                if (int.TryParse(fields[8], out int insertSize))
                {
                    int absInsertSize = Math.Abs(insertSize);
                    _insertSizes.Add(absInsertSize);
                    stats.InsertSizeCount++;
                    stats.TotalInsertSize += absInsertSize;
                }
            }

            // 6. 可选字段分析（仅分析前几条记录以提高速度）
            if (sampledReads < 1000 && fields.Length > 11)
            {
                AnalyzeOptionalFields(fields, stats);
            }
        }

        /// <summary>
        /// 解析CIGAR字符串
        /// </summary>
        private void ParseCigarString(string cigar, Dictionary<string, long> cigarStats, SamStatistics stats)
        {
            string currentNumber = "";
            foreach (char c in cigar)
            {
                if (char.IsDigit(c))
                {
                    currentNumber += c;
                }
                else if (char.IsLetter(c))
                {
                    string op = c.ToString();
                    if (cigarStats.ContainsKey(op))
                    {
                        if (long.TryParse(currentNumber, out long length))
                        {
                            cigarStats[op] += length;

                            // 更新详细统计
                            switch (op)
                            {
                                case "M":
                                    stats.MatchBases += length;
                                    stats.TotalAlignedBases += length;
                                    break;
                                case "I":
                                    stats.InsertionBases += length;
                                    stats.InsertionCount++;
                                    stats.TotalIndelBases += length;
                                    break;
                                case "D":
                                    stats.DeletionBases += length;
                                    stats.DeletionCount++;
                                    stats.TotalIndelBases += length;
                                    break;
                                case "S":
                                    stats.SoftClipBases += length;
                                    stats.SoftClipCount++;
                                    break;
                                case "H":
                                    stats.HardClipBases += length;
                                    stats.HardClipCount++;
                                    break;
                                case "=":
                                    stats.SequenceMatchBases += length;
                                    stats.TotalAlignedBases += length;
                                    break;
                                case "X":
                                    stats.SequenceMismatchBases += length;
                                    stats.MismatchCount++;
                                    stats.TotalAlignedBases += length;
                                    break;
                            }
                        }
                    }
                    currentNumber = "";
                }
            }
        }

        /// <summary>
        /// 获取read长度
        /// </summary>
        private int GetReadLength(string sequence, string cigar)
        {
            // 优先使用序列长度
            if (!string.IsNullOrEmpty(sequence) && sequence != "*")
                return sequence.Length;

            // 从CIGAR计算
            if (cigar == "*") return 0;

            int length = 0;
            string currentNumber = "";

            foreach (char c in cigar)
            {
                if (char.IsDigit(c))
                {
                    currentNumber += c;
                }
                else if (char.IsLetter(c))
                {
                    string op = c.ToString();
                    if (int.TryParse(currentNumber, out int opLength))
                    {
                        if (op == "M" || op == "I" || op == "S" || op == "=" || op == "X")
                            length += opLength;
                    }
                    currentNumber = "";
                }
            }

            return length;
        }

        /// <summary>
        /// 分析可选字段
        /// </summary>
        private void AnalyzeOptionalFields(string[] fields, SamStatistics stats)
        {
            for (int i = 11; i < Math.Min(fields.Length, 20); i++) // 只检查前几个字段
            {
                string field = fields[i];
                if (field.StartsWith("NM:i:"))
                {
                    if (int.TryParse(field.Substring(5), out int editDistance))
                    {
                        stats.EditDistanceSum += editDistance;
                        stats.ReadsWithEditDistance++;
                    }
                }
                else if (field.StartsWith("AS:i:"))
                {
                    if (int.TryParse(field.Substring(5), out int alignmentScore))
                    {
                        stats.AlignmentScoreSum += alignmentScore;
                        stats.ReadsWithAlignmentScore++;
                    }
                }
                else if (field.StartsWith("XS:i:"))
                {
                    stats.SecondaryAlignments++;
                }
                else if (field.StartsWith("SA:Z:"))
                {
                    stats.SupplementaryAlignments++;
                }
            }
        }

        /// <summary>
        /// 更新FLAG统计
        /// </summary>
        private void UpdateFlagStatistics(SamStatistics stats, int flag)
        {
            // 检查是否是配对read
            if ((flag & 0x1) != 0)
            {
                _totalPairedReads++;
            }

            // 是否未比对
            if ((flag & 0x4) != 0)
            {
                stats.UnmappedReads++;
                return; // 未比对reads不再统计其他标志
            }

            stats.AlignedReads++;

            // 各种FLAG统计
            if ((flag & 0x2) != 0) stats.ProperlyPairedReads++;
            if ((flag & 0x8) != 0) stats.MateUnmappedReads++;
            if ((flag & 0x10) != 0) stats.ReverseStrandReads++;
            if ((flag & 0x20) != 0) stats.MateReverseStrandReads++;
            if ((flag & 0x40) != 0) stats.FirstInPairReads++;
            if ((flag & 0x80) != 0) stats.SecondInPairReads++;
            if ((flag & 0x100) != 0) stats.SecondaryAlignmentsByFlag++;
            if ((flag & 0x200) != 0) stats.QCFailedReads++;
            if ((flag & 0x400) != 0) stats.DuplicateReads++;
            if ((flag & 0x800) != 0) stats.SupplementaryAlignments++;
        }

        /// <summary>
        /// 计算统计信息
        /// </summary>
        private void CalculateStatistics(SamAnalysisResult result, SamStatistics stats, Dictionary<string, long> cigarStats)
        {
            // 1. 基本统计
            if (_mapqValues.Count > 0)
            {
                stats.MeanMAPQ = _mapqValues.Average();
                stats.MedianMAPQ = CalculateMedian(_mapqValues.Select(x => (double)x).ToArray());
                stats.MaxMAPQ = _mapqValues.Max();
                stats.MinMAPQ = _mapqValues.Min();

                // MAPQ分布统计
                stats.QualityDistribution = new Dictionary<string, double>
                {
                    ["极高质量(MAPQ≥40)"] = (double)stats.VeryHighQualityReads / stats.TotalReads,
                    ["高质量(MAPQ≥30)"] = (double)stats.HighQualityReads / stats.TotalReads,
                    ["中等质量(MAPQ≥20)"] = (double)stats.MediumQualityReads / stats.TotalReads,
                    ["低质量(MAPQ≥10)"] = (double)stats.LowQualityReads / stats.TotalReads,
                    ["极低质量(MAPQ<10)"] = (double)stats.VeryLowQualityReads / stats.TotalReads,
                    ["多位置比对(MAPQ=0)"] = (double)stats.ZeroMAPQReads / stats.TotalReads
                };
            }

            if (_readLengths.Count > 0)
            {
                stats.MeanReadLength = _readLengths.Average();
                stats.MedianReadLength = CalculateMedian(_readLengths.Select(x => (double)x).ToArray());
                stats.MinReadLength = _readLengths.Min();
                stats.MaxReadLength = _readLengths.Max();
            }

            if (_insertSizes.Count > 0)
            {
                stats.MeanInsertSize = _insertSizes.Average();
                stats.MedianInsertSize = CalculateMedian(_insertSizes.Select(x => (double)x).ToArray());
                stats.MinInsertSize = _insertSizes.Min();
                stats.MaxInsertSize = _insertSizes.Max();
                stats.InsertSizeStdDev = CalculateStandardDeviation(_insertSizes);
            }

            // 2. 比率计算
            stats.AlignmentRate = stats.TotalReads > 0 ? (double)stats.AlignedReads / stats.TotalReads : 0;
            stats.UnmappedRate = stats.TotalReads > 0 ? (double)stats.UnmappedReads / stats.TotalReads : 0;
            stats.DuplicateRate = stats.TotalReads > 0 ? (double)stats.DuplicateReads / stats.TotalReads : 0;

            // 正确配对率（仅对双端测序有意义）
            if (_isPairedEnd && stats.AlignedReads > 0)
            {
                stats.ProperPairRate = (double)stats.ProperlyPairedReads / stats.AlignedReads;
            }
            else
            {
                stats.ProperPairRate = 0;
            }

            // 高质量reads比例
            stats.HighQualityRate = stats.TotalReads > 0 ?
                (double)(stats.VeryHighQualityReads + stats.HighQualityReads) / stats.TotalReads : 0;

            // 3. CIGAR统计
            stats.CigarOperationTypes = new Dictionary<string, long>(cigarStats);
            stats.TotalAlignedBases = stats.MatchBases + stats.SequenceMatchBases + stats.SequenceMismatchBases;

            if (stats.TotalAlignedBases > 0)
            {
                stats.MismatchRate = (double)stats.SequenceMismatchBases / stats.TotalAlignedBases;
                stats.IndelRate = (double)stats.TotalIndelBases / stats.TotalAlignedBases;
            }

            if (stats.TotalBases > 0)
            {
                stats.SoftClipRate = (double)stats.SoftClipBases / stats.TotalBases;
                stats.HardClipRate = (double)stats.HardClipBases / stats.TotalBases;
            }

            // 4. 编辑距离
            if (stats.ReadsWithEditDistance > 0)
            {
                stats.MeanEditDistance = (double)stats.EditDistanceSum / stats.ReadsWithEditDistance;
            }

            // 5. 测序类型检测
            stats.IsPairedEnd = _isPairedEnd;
            stats.PairedReadsRatio = stats.TotalReads > 0 ? (double)_totalPairedReads / stats.TotalReads : 0;

            // 保存到结果
            result.AlignmentRate = stats.AlignmentRate;
            result.IsPairedEnd = _isPairedEnd;
        }

        /// <summary>
        /// 计算中位数
        /// </summary>
        private double CalculateMedian(double[] values)
        {
            if (values == null || values.Length == 0)
                return 0;

            var sorted = values.OrderBy(x => x).ToArray();
            int mid = sorted.Length / 2;

            if (sorted.Length % 2 == 0)
                return (sorted[mid - 1] + sorted[mid]) / 2.0;
            else
                return sorted[mid];
        }

        /// <summary>
        /// 计算标准差
        /// </summary>
        private double CalculateStandardDeviation(List<int> values)
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
        private async Task GenerateProfessionalReportAsync(SamAnalysisResult analysis,
            string reportPath, VisualizationContext context)
        {
            var report = new StringBuilder();
            var stats = analysis.Statistics;

            // 1. 报告标题
            report.AppendLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            report.AppendLine($"║              SAM文件专业级分析报告 ({(_isPairedEnd ? "双端测序" : "单端测序")})                    ║");
            report.AppendLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            report.AppendLine();
            report.AppendLine($"📅 报告生成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"📁 样本名称: {context.SampleName ?? "未知样本"}");
            report.AppendLine($"📊 SAM文件: {Path.GetFileName(context.SamFilePath)}");
            report.AppendLine($"🎯 测序类型: {(_isPairedEnd ? "双端测序 (Paired-end)" : "单端测序 (Single-end)")}");
            report.AppendLine($"🔢 分析Reads数: {analysis.TotalReads:N0}");
            if (analysis.IsSampled)
            {
                report.AppendLine($"⚠️  采样分析: 基于 {analysis.SampleSize:N0} 条reads (总行数: {analysis.TotalLines:N0})");
            }
            report.AppendLine();

            // 2. 核心质量指标（根据测序类型调整）
            report.AppendLine("════════════════════════════════════════════════════════════════════════════════");
            report.AppendLine($"🎯 核心质量指标 ({(_isPairedEnd ? "双端测序" : "单端测序")})");
            report.AppendLine("────────────────────────────────────────────────────────────────────────────────");

            AppendMetric(report, "比对率", stats.AlignmentRate, 0.9, 0.7, true, true);
            AppendMetric(report, "平均MAPQ", stats.MeanMAPQ, 40, 25, false, false);
            AppendMetric(report, "重复率", stats.DuplicateRate, 0.1, 0.3, false, true);

            if (_isPairedEnd)
            {
                AppendMetric(report, "正确配对率", stats.ProperPairRate, 0.8, 0.6, true, true);
                if (_insertSizes.Count > 0)
                {
                    AppendMetric(report, "平均插入片段", stats.MeanInsertSize, 300, 150, false, false, "bp");
                }
            }
            else
            {
                report.AppendLine($"  正确配对率                     N/A                     (单端测序不适用)");
            }

            AppendMetric(report, "高质量Reads比例", stats.HighQualityRate, 0.8, 0.5, true, true);
            report.AppendLine();

            // 3. MAPQ详细分析
            report.AppendLine("════════════════════════════════════════════════════════════════════════════════");
            report.AppendLine("🎯 MAPQ质量详细分析");
            report.AppendLine("────────────────────────────────────────────────────────────────────────────────");

            report.AppendLine($"📊 MAPQ统计摘要:");
            report.AppendLine($"   • 平均值: {stats.MeanMAPQ:F1}");
            report.AppendLine($"   • 中位数: {stats.MedianMAPQ:F1}");
            report.AppendLine($"   • 范围: {stats.MinMAPQ} - {stats.MaxMAPQ}");
            report.AppendLine();

            if (stats.QualityDistribution != null)
            {
                report.AppendLine($"📈 MAPQ分布:");
                foreach (var kv in stats.QualityDistribution.OrderByDescending(k =>
                    k.Key.Contains("极高质量") ? 5 :
                    k.Key.Contains("高质量") ? 4 :
                    k.Key.Contains("中等") ? 3 :
                    k.Key.Contains("低质量") ? 2 : 1))
                {
                    report.AppendLine($"   • {kv.Key,-25} {kv.Value,8:P1}");
                }
                report.AppendLine();
            }

            // 4. 比对详细统计
            report.AppendLine("════════════════════════════════════════════════════════════════════════════════");
            report.AppendLine("📊 详细统计信息");
            report.AppendLine("────────────────────────────────────────────────────────────────────────────────");

            report.AppendLine($"📈 比对统计:");
            report.AppendLine($"   • 总Reads数: {stats.TotalReads:N0}");
            report.AppendLine($"   • 比对上的Reads: {stats.AlignedReads:N0} ({stats.AlignmentRate:P1})");
            report.AppendLine($"   • 未比对Reads: {stats.UnmappedReads:N0} ({stats.UnmappedRate:P1})");
            report.AppendLine($"   • 重复Reads: {stats.DuplicateReads:N0} ({stats.DuplicateRate:P1})");

            if (_isPairedEnd)
            {
                report.AppendLine($"   • 正确配对Reads: {stats.ProperlyPairedReads:N0} ({stats.ProperPairRate:P1})");
            }
            report.AppendLine();

            report.AppendLine($"📏 Read长度统计:");
            report.AppendLine($"   • 平均长度: {stats.MeanReadLength:F1} bp");
            report.AppendLine($"   • 中位数长度: {stats.MedianReadLength:F1} bp");
            report.AppendLine($"   • 范围: {stats.MinReadLength} - {stats.MaxReadLength} bp");
            report.AppendLine($"   • 总碱基数: {stats.TotalBases:N0} bp");
            report.AppendLine();

            if (_isPairedEnd && _insertSizes.Count > 0)
            {
                report.AppendLine($"📐 插入片段统计 (双端测序):");
                report.AppendLine($"   • 平均长度: {stats.MeanInsertSize:F1} bp");
                report.AppendLine($"   • 中位数长度: {stats.MedianInsertSize:F1} bp");
                report.AppendLine($"   • 范围: {stats.MinInsertSize} - {stats.MaxInsertSize} bp");
                report.AppendLine($"   • 标准差: {stats.InsertSizeStdDev:F1} bp");
                report.AppendLine($"   • 有效样本数: {_insertSizes.Count:N0}");
                report.AppendLine();
            }

            // 5. CIGAR分析
            if (stats.CigarOperationTypes != null && stats.CigarOperationTypes.Count > 0)
            {
                report.AppendLine("════════════════════════════════════════════════════════════════════════════════");
                report.AppendLine("🔧 CIGAR操作分析");
                report.AppendLine("────────────────────────────────────────────────────────────────────────────────");

                long totalCigarOps = stats.CigarOperationTypes.Values.Sum();
                report.AppendLine($"总CIGAR操作长度: {totalCigarOps:N0} bp");
                report.AppendLine($"比对总碱基数: {stats.TotalAlignedBases:N0} bp");
                report.AppendLine();

                // 显示主要的CIGAR操作
                var majorOps = stats.CigarOperationTypes
                    .Where(kv => kv.Value > 0)
                    .OrderByDescending(kv => kv.Value);

                foreach (var kv in majorOps)
                {
                    string opName = GetCigarOperationDescription(kv.Key);
                    double percentage = totalCigarOps > 0 ? (double)kv.Value / totalCigarOps * 100 : 0;
                    report.AppendLine($"  {kv.Key,-2} {opName,-12} {kv.Value,10:N0} bp ({percentage,6:F2}%)");
                }
                report.AppendLine();

                report.AppendLine($"📊 CIGAR衍生指标:");
                report.AppendLine($"   • 匹配碱基数: {stats.MatchBases:N0} bp");
                report.AppendLine($"   • 精确匹配碱基: {stats.SequenceMatchBases:N0} bp");
                report.AppendLine($"   • 错配碱基数: {stats.SequenceMismatchBases:N0} bp ({stats.MismatchRate:P3})");
                report.AppendLine($"   • 插入/缺失: {stats.InsertionBases:N0}/{stats.DeletionBases:N0} bp ({stats.IndelRate:P3})");
                report.AppendLine($"   • 软裁剪碱基: {stats.SoftClipBases:N0} bp ({stats.SoftClipRate:P3})");
                report.AppendLine($"   • 硬裁剪碱基: {stats.HardClipBases:N0} bp ({stats.HardClipRate:P3})");
                report.AppendLine();
            }

            // 6. 比对特征
            report.AppendLine("════════════════════════════════════════════════════════════════════════════════");
            report.AppendLine("🧬 比对特征");
            report.AppendLine("────────────────────────────────────────────────────────────────────────────────");

            report.AppendLine($"🔄 链特异性:");
            report.AppendLine($"   • 反向链Reads: {stats.ReverseStrandReads:N0}");
            if (_isPairedEnd)
            {
                report.AppendLine($"   • 配对链反向: {stats.MateReverseStrandReads:N0}");
                report.AppendLine($"   • 第一端Reads: {stats.FirstInPairReads:N0}");
                report.AppendLine($"   • 第二端Reads: {stats.SecondInPairReads:N0}");
            }
            report.AppendLine();

            report.AppendLine($"🎯 比对特征:");
            report.AppendLine($"   • 无CIGAR的Reads: {stats.ReadsWithNoCigar:N0}");
            report.AppendLine($"   • 二次比对: {stats.SecondaryAlignments:N0}");
            report.AppendLine($"   • 补充比对: {stats.SupplementaryAlignments:N0}");
            report.AppendLine($"   • QC失败Reads: {stats.QCFailedReads:N0}");
            report.AppendLine();

            if (stats.ReadsWithEditDistance > 0)
            {
                report.AppendLine($"📐 编辑距离:");
                report.AppendLine($"   • 平均编辑距离: {stats.MeanEditDistance:F2}");
                report.AppendLine($"   • 分析Reads数: {stats.ReadsWithEditDistance:N0}");
                report.AppendLine();
            }

            // 7. 数据质量评估（根据测序类型调整）
            report.AppendLine("════════════════════════════════════════════════════════════════════════════════");
            report.AppendLine("📋 数据质量综合评估");
            report.AppendLine("────────────────────────────────────────────────────────────────────────────────");

            var assessments = PerformQualityAssessment(stats, _isPairedEnd);
            foreach (var assessment in assessments)
            {
                report.AppendLine(assessment);
            }
            report.AppendLine();

            // 8. 建议（根据测序类型调整）
            report.AppendLine("════════════════════════════════════════════════════════════════════════════════");
            report.AppendLine("💡 建议和改进措施");
            report.AppendLine("────────────────────────────────────────────────────────────────────────────────");

            var recommendations = GenerateRecommendations(stats, _isPairedEnd);
            for (int i = 0; i < recommendations.Count; i++)
            {
                report.AppendLine($"{i + 1}. {recommendations[i]}");
            }
            report.AppendLine();

            // 9. 报告结束
            report.AppendLine("════════════════════════════════════════════════════════════════════════════════");
            report.AppendLine("📄 报告结束");
            report.AppendLine($"生成工具: NGS BWA Runner - 专业级SAM分析模块 v2.0");
            report.AppendLine($"测序类型: {(_isPairedEnd ? "双端测序" : "单端测序")}");
            report.AppendLine($"分析完成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"注: 本报告基于{(analysis.IsSampled ? "采样" : "全部")}数据分析生成");

            await File.WriteAllTextAsync(reportPath, report.ToString());
            Utils.Logger.LogInfo($"SAM专业分析报告已保存: {reportPath}");
        }

        /// <summary>
        /// 获取CIGAR操作描述
        /// </summary>
        private string GetCigarOperationDescription(string op)
        {
            return op switch
            {
                "M" => "匹配(可能错配)",
                "=" => "序列匹配",
                "X" => "序列错配",
                "I" => "插入",
                "D" => "缺失",
                "N" => "跳过",
                "S" => "软裁剪",
                "H" => "硬裁剪",
                "P" => "填充",
                _ => "未知操作"
            };
        }

        /// <summary>
        /// 追加指标行
        /// </summary>
        private void AppendMetric(StringBuilder sb, string label, double value,
            double excellent, double good, bool higherIsBetter, bool isPercentage = false, string unit = "")
        {
            string valueStr;
            if (isPercentage)
                valueStr = $"{value:P1}";
            else if (!string.IsNullOrEmpty(unit))
                valueStr = $"{value:F1} {unit}";
            else
                valueStr = $"{value:F1}";

            string assessment = GetQualityAssessment(value, excellent, good, higherIsBetter);

            sb.AppendLine($"  {label,-25} {valueStr,20} {assessment,20}");
        }

        /// <summary>
        /// 获取质量评估（新版MAPQ评估标准）
        /// </summary>
        private string GetQualityAssessment(double value, double excellent, double good, bool higherIsBetter)
        {
            if (higherIsBetter)
            {
                if (value >= excellent) return "✅ 优秀";
                if (value >= good) return "⚠️  一般";
                return "❌ 需改进";
            }
            else
            {
                // 对于MAPQ，我们使用新的评估标准
                if (value >= 40) return "✅ 极高质量";
                if (value >= 30) return "✅ 高质量";
                if (value >= 20) return "⚠️  中等质量";
                if (value >= 10) return "⚠️  较低质量";
                return "❌ 低质量";
            }
        }

        /// <summary>
        /// 执行质量评估（根据测序类型调整）
        /// </summary>
        private List<string> PerformQualityAssessment(SamStatistics stats, bool isPairedEnd)
        {
            var assessments = new List<string>();

            // 1. 比对率评估
            if (stats.AlignmentRate >= 0.9)
                assessments.Add("✅ 比对率优秀 (≥90%) - 样品和参考序列匹配良好");
            else if (stats.AlignmentRate >= 0.7)
                assessments.Add("⚠️  比对率一般 (70-90%) - 建议检查参考序列匹配度");
            else
                assessments.Add("❌ 比对率较低 (<70%) - 可能存在严重问题，需重新评估");

            // 2. MAPQ质量评估（新版标准）
            if (stats.MeanMAPQ >= 40)
                assessments.Add("✅ MAPQ极高质量 (≥40) - 比对特异性极高，结果非常可靠");
            else if (stats.MeanMAPQ >= 30)
                assessments.Add("✅ MAPQ高质量 (30-40) - 比对特异性高，适合精准分析");
            else if (stats.MeanMAPQ >= 20)
                assessments.Add("⚠️  MAPQ中等质量 (20-30) - 比对特异性一般，需谨慎使用");
            else if (stats.MeanMAPQ >= 10)
                assessments.Add("⚠️  MAPQ较低质量 (10-20) - 比对特异性较差，建议过滤");
            else
                assessments.Add("❌ MAPQ低质量 (<10) - 比对特异性差，强烈建议重新比对");

            // 3. 重复率评估
            if (stats.DuplicateRate <= 0.1)
                assessments.Add("✅ 重复率正常 (≤10%) - 文库复杂度良好");
            else if (stats.DuplicateRate <= 0.3)
                assessments.Add("⚠️  重复率偏高 (10-30%) - 文库复杂度一般");
            else
                assessments.Add("❌ 重复率过高 (>30%) - 可能存在PCR偏向或起始材料不足");

            // 4. 正确配对率评估（仅双端测序）
            if (isPairedEnd)
            {
                if (stats.ProperPairRate >= 0.8)
                    assessments.Add("✅ 正确配对率优秀 (≥80%) - 文库构建质量好");
                else if (stats.ProperPairRate >= 0.6)
                    assessments.Add("⚠️  正确配对率一般 (60-80%) - 文库构建质量可接受");
                else
                    assessments.Add("❌ 正确配对率较低 (<60%) - 可能存在文库构建问题");
            }

            // 5. 错配率评估
            if (stats.MismatchRate <= 0.01)
                assessments.Add("✅ 错配率低 (≤1%) - 序列质量优秀");
            else if (stats.MismatchRate <= 0.02)
                assessments.Add("⚠️  错配率中等 (1-2%) - 在可接受范围内");
            else
                assessments.Add("❌ 错配率高 (>2%) - 可能存在测序错误或参考序列不匹配");

            return assessments;
        }

        /// <summary>
        /// 生成改进建议（根据测序类型调整）
        /// </summary>
        private List<string> GenerateRecommendations(SamStatistics stats, bool isPairedEnd)
        {
            var recommendations = new List<string>();

            // 通用建议
            if (stats.AlignmentRate < 0.7)
            {
                recommendations.Add("比对率偏低：检查参考序列是否与样品匹配，验证测序接头");
            }

            if (stats.MeanMAPQ < 20)
            {
                recommendations.Add("MAPQ质量偏低：考虑增加比对严格度（如提高匹配分数阈值），检查参考序列重复区域");
            }

            if (stats.DuplicateRate > 0.3)
            {
                recommendations.Add("重复率过高：建议增加起始DNA量，优化PCR循环数，或使用Picard MarkDuplicates处理");
            }

            if (stats.MismatchRate > 0.02)
            {
                recommendations.Add("错配率偏高：验证参考序列版本，检查测序质量（Q20/Q30比例），考虑种属差异");
            }

            if (stats.SoftClipRate > 0.1)
            {
                recommendations.Add("软裁剪率较高：检查是否有接头残留，验证read质量，考虑使用cutadapt去除接头");
            }

            // 测序类型特定建议
            if (isPairedEnd)
            {
                if (stats.ProperPairRate < 0.6)
                {
                    recommendations.Add("正确配对率低：检查文库插入片段大小分布，验证配对read方向性");
                }

                if (_insertSizes.Count > 0 && stats.MeanInsertSize < 100)
                {
                    recommendations.Add("插入片段过短：可能是DNA降解或文库构建问题，建议检查样品质量");
                }

                recommendations.Add("双端测序数据处理：建议使用Picard CollectInsertSizeMetrics评估插入片段分布");
            }
            else
            {
                recommendations.Add("单端测序分析：注意某些分析（如SV检测、精确插入片段分析）可能受限");
            }

            // 通用最佳实践
            recommendations.Add("常规质控：使用FastQC进行原始数据质量评估，MultiQC进行综合报告");
            recommendations.Add("变异检测前处理：建议使用GATK最佳实践流程（BQSR、Indel重比对等）");
            recommendations.Add("表达定量：对于RNA-seq，建议使用featureCounts或HTSeq进行基因计数");

            return recommendations;
        }

        /// <summary>
        /// 格式化文件大小
        /// </summary>
        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        // 私有变量引用
        private int sampledReads = 0;
    }

    // ==================== 支持类定义 ====================

    /// <summary>
    /// SAM分析结果
    /// </summary>
    public class SamAnalysisResult
    {
        public long TotalReads { get; set; }
        public long TotalLines { get; set; }
        public int SampledReads { get; set; }
        public bool IsSampled { get; set; }
        public int SampleSize { get; set; }
        public bool IsPairedEnd { get; set; }
        public double AlignmentRate { get; set; }
        public SamStatistics Statistics { get; set; }
        public Dictionary<string, long> ReferenceSequences { get; set; } = new();
        public List<string> ReadGroups { get; set; } = new();
        public List<string> Programs { get; set; } = new();
    }

    /// <summary>
    /// SAM统计信息（完全版）
    /// </summary>
    public class SamStatistics
    {
        // === 基本计数 ===
        public long TotalReads { get; set; }
        public long AlignedReads { get; set; }
        public long UnmappedReads { get; set; }
        public long DuplicateReads { get; set; }
        public long ProperlyPairedReads { get; set; }

        // === MAPQ质量分级 ===
        public long VeryHighQualityReads { get; set; }  // MAPQ ≥ 40
        public long HighQualityReads { get; set; }      // MAPQ ≥ 30
        public long MediumQualityReads { get; set; }    // MAPQ ≥ 20
        public long LowQualityReads { get; set; }       // MAPQ ≥ 10
        public long VeryLowQualityReads { get; set; }   // MAPQ < 10
        public long ZeroMAPQReads { get; set; }         // MAPQ = 0
        public long MaxMAPQReads { get; set; }          // MAPQ = 255

        // === 链特异性和配对信息 ===
        public long ReverseStrandReads { get; set; }
        public long MateReverseStrandReads { get; set; }
        public long FirstInPairReads { get; set; }
        public long SecondInPairReads { get; set; }
        public long MateUnmappedReads { get; set; }

        // === 比对特征 ===
        public long SecondaryAlignmentsByFlag { get; set; }
        public long QCFailedReads { get; set; }
        public long SupplementaryAlignments { get; set; }
        public long SecondaryAlignments { get; set; }
        public long ReadsWithNoCigar { get; set; }

        // === MAPQ统计 ===
        public double MeanMAPQ { get; set; }
        public double MedianMAPQ { get; set; }
        public int MaxMAPQ { get; set; }
        public int MinMAPQ { get; set; }
        public Dictionary<string, double> QualityDistribution { get; set; }

        // === Read长度统计 ===
        public double MeanReadLength { get; set; }
        public double MedianReadLength { get; set; }
        public int MinReadLength { get; set; }
        public int MaxReadLength { get; set; }
        public long TotalBases { get; set; }

        // === 插入片段统计（双端测序） ===
        public double MeanInsertSize { get; set; }
        public double MedianInsertSize { get; set; }
        public double InsertSizeStdDev { get; set; }
        public int MinInsertSize { get; set; }
        public int MaxInsertSize { get; set; }
        public long TotalInsertSize { get; set; }
        public long InsertSizeCount { get; set; }

        // === CIGAR统计 ===
        public Dictionary<string, long> CigarOperationTypes { get; set; }
        public long MatchBases { get; set; }
        public long SequenceMatchBases { get; set; }
        public long SequenceMismatchBases { get; set; }
        public long InsertionBases { get; set; }
        public long InsertionCount { get; set; }
        public long DeletionBases { get; set; }
        public long DeletionCount { get; set; }
        public long SoftClipBases { get; set; }
        public long SoftClipCount { get; set; }
        public long HardClipBases { get; set; }
        public long HardClipCount { get; set; }

        // === 汇总 ===
        public long TotalAlignedBases { get; set; }
        public long TotalIndelBases { get; set; }
        public long MismatchCount { get; set; }

        // === 可选字段统计 ===
        public long ReadsWithEditDistance { get; set; }
        public long EditDistanceSum { get; set; }
        public long ReadsWithAlignmentScore { get; set; }
        public long AlignmentScoreSum { get; set; }

        // === 比率计算 ===
        public double AlignmentRate { get; set; }
        public double UnmappedRate { get; set; }
        public double DuplicateRate { get; set; }
        public double ProperPairRate { get; set; }
        public double HighQualityRate { get; set; }

        // === 错误率 ===
        public double MismatchRate { get; set; }
        public double IndelRate { get; set; }
        public double SoftClipRate { get; set; }
        public double HardClipRate { get; set; }

        // === 编辑距离 ===
        public double MeanEditDistance { get; set; }

        // === 测序类型 ===
        public bool IsPairedEnd { get; set; }
        public double PairedReadsRatio { get; set; }
    }
}