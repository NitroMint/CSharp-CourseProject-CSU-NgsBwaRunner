// Core/Visualization/Interfaces/IVisualizationModule.cs
using NgsBwaRunner.Visualization.Models;
using System.Threading.Tasks;

namespace NgsBwaRunner.Visualization.Interfaces
{
    /// <summary>
    /// 可视化模块接口（所有可视化模块必须实现）
    /// </summary>
    public interface IVisualizationModule
    {
        /// <summary>
        /// 模块ID（唯一标识）
        /// </summary>
        string ModuleId { get; }

        /// <summary>
        /// 模块显示名称
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// 模块描述
        /// </summary>
        string Description { get; }

        /// <summary>
        /// 执行优先级（1-100，数值越小越先执行）
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// 是否启用
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// 执行模块，生成可视化图表
        /// </summary>
        /// <param name="context">可视化上下文</param>
        /// <returns>可视化结果</returns>
        Task<VisualizationResult> ExecuteAsync(VisualizationContext context);
    }
}