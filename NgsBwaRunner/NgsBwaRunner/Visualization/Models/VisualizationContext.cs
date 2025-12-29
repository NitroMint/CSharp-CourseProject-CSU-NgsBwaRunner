// Core/Visualization/Models/VisualizationContext.cs
using System.Collections.Generic;

namespace NgsBwaRunner.Visualization.Models
{
    /// <summary>
    /// 可视化执行上下文
    /// </summary>
    public class VisualizationContext
    {
        /// <summary>
        /// SAM文件路径
        /// </summary>
        public string SamFilePath { get; set; }

        /// <summary>
        /// VCF文件路径（过滤前）
        /// </summary>
        public string VcfFilePath { get; set; }

        /// <summary>
        /// 过滤后的VCF文件路径
        /// </summary>
        public string FilteredVcfFilePath { get; set; }

        /// <summary>
        /// 输出目录
        /// </summary>
        public string OutputDirectory { get; set; }

        /// <summary>
        /// 样本名称
        /// </summary>
        public string SampleName { get; set; }

        /// <summary>
        /// 是否合并PDF
        /// </summary>
        public bool CombinePdf { get; set; }

        /// <summary>
        /// 是否高分辨率输出
        /// </summary>
        public bool HighResolution { get; set; }

        /// <summary>
        /// 额外参数
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; }

        /// <summary>
        /// 进度报告回调
        /// </summary>
        public Action<string, int> ProgressReporter { get; set; }

        public VisualizationContext()
        {
            Parameters = new Dictionary<string, object>();
            CombinePdf = true;
            HighResolution = true;
        }
    }
}

// Core/Visualization/Models/VisualizationResult.cs
namespace NgsBwaRunner.Visualization.Models
{
    /// <summary>
    /// 可视化结果
    /// </summary>
    public class VisualizationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string OutputFile { get; set; }
        public string Error { get; set; }
        public TimeSpan ExecutionTime { get; set; }

        public static VisualizationResult SuccessResult(string message, string outputFile = null)
        {
            return new VisualizationResult
            {
                Success = true,
                Message = message,
                OutputFile = outputFile
            };
        }

        public static VisualizationResult ErrorResult(string message, string error = null)
        {
            return new VisualizationResult
            {
                Success = false,
                Message = message,
                Error = error
            };
        }
    }
}