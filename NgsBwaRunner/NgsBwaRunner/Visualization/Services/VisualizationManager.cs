// Core/Visualization/Services/VisualizationManager.cs
using NgsBwaRunner.Visualization.Services;
using NgsBwaRunner.Visualization.Interfaces;
using NgsBwaRunner.Visualization.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NgsBwaRunner.Visualization.Services
{
    /// <summary>
    /// 可视化管理器 - 协调所有可视化模块的执行
    /// </summary>
    public class VisualizationManager
    {
        private readonly ModuleDiscoverer _discoverer;
        private readonly PdfExportService _pdfExporter;
        private List<IVisualizationModule> _modules;

        public event EventHandler<string> ModuleStarted;
        public event EventHandler<string> ModuleCompleted;
        public event EventHandler<string> ModuleFailed;

        public VisualizationManager()
        {
            _discoverer = new ModuleDiscoverer();
            _pdfExporter = new PdfExportService();
            _modules = new List<IVisualizationModule>();

            InitializeModules();
        }

        private void InitializeModules()
        {
            try
            {
                _modules = _discoverer.DiscoverAllModules().ToList();
                Utils.Logger.LogInfo($"共发现 {_modules.Count} 个可视化模块");
            }
            catch (Exception ex)
            {
                Utils.Logger.LogError($"初始化可视化模块失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 执行所有可视化模块
        /// </summary>
        public async Task<List<VisualizationResult>> GenerateAllVisualizationsAsync(VisualizationContext context)
        {
            var results = new List<VisualizationResult>();

            // 确保输出目录存在
            Directory.CreateDirectory(context.OutputDirectory);

            // 获取所有启用的模块
            var enabledModules = _modules.Where(m => m.IsEnabled).OrderBy(m => m.Priority);

            int totalModules = enabledModules.Count();
            int currentModule = 0;

            foreach (var module in enabledModules)
            {
                currentModule++;
                string progressMessage = $"正在执行模块 ({currentModule}/{totalModules}): {module.DisplayName}";
                context.ProgressReporter?.Invoke(progressMessage, (int)((currentModule - 1) * 100.0 / totalModules));

                OnModuleStarted(module.ModuleId);

                try
                {
                    var result = await module.ExecuteAsync(context);
                    result.ExecutionTime = System.Diagnostics.Stopwatch.GetElapsedTime(System.Diagnostics.Stopwatch.GetTimestamp());

                    if (result.Success)
                    {
                        results.Add(result);
                        Utils.Logger.LogSuccess($"模块执行成功: {module.DisplayName}");
                        OnModuleCompleted(module.ModuleId);
                    }
                    else
                    {
                        Utils.Logger.LogError($"模块执行失败: {module.DisplayName} - {result.Error}");
                        OnModuleFailed(module.ModuleId);
                    }
                }
                catch (Exception ex)
                {
                    Utils.Logger.LogError($"模块执行异常: {module.DisplayName} - {ex.Message}");
                    results.Add(VisualizationResult.ErrorResult($"模块执行异常: {ex.Message}"));
                    OnModuleFailed(module.ModuleId);
                }
            }

            // 如果要求合并PDF，则合并所有生成的PDF
            if (context.CombinePdf && results.Any(r => r.Success && r.OutputFile != null))
            {
                try
                {
                    var pdfFiles = results.Where(r => r.Success && r.OutputFile != null && Path.GetExtension(r.OutputFile).ToLower() == ".pdf")
                                          .Select(r => r.OutputFile).ToList();

                    if (pdfFiles.Count > 0)
                    {
                        string combinedPdf = Path.Combine(context.OutputDirectory, $"{context.SampleName}_combined_visualization.pdf");
                        await _pdfExporter.CombinePdfFilesAsync(pdfFiles, combinedPdf);

                        Utils.Logger.LogSuccess($"合并PDF完成: {combinedPdf}");
                    }
                }
                catch (Exception ex)
                {
                    Utils.Logger.LogError($"合并PDF失败: {ex.Message}");
                }
            }

            context.ProgressReporter?.Invoke("可视化完成", 100);
            return results;
        }

        /// <summary>
        /// 获取所有模块信息
        /// </summary>
        public List<ModuleInfo> GetModuleInfos()
        {
            return _modules.Select(m => new ModuleInfo
            {
                Id = m.ModuleId,
                Name = m.DisplayName,
                Description = m.Description,
                Priority = m.Priority,
                IsEnabled = m.IsEnabled
            }).ToList();
        }

        /// <summary>
        /// 启用/禁用模块
        /// </summary>
        public void SetModuleEnabled(string moduleId, bool enabled)
        {
            var module = _modules.FirstOrDefault(m => m.ModuleId == moduleId);
            if (module != null)
            {
                module.IsEnabled = enabled;
                Utils.Logger.LogInfo($"设置模块 {module.DisplayName} 为 {(enabled ? "启用" : "禁用")}");
            }
        }

        protected virtual void OnModuleStarted(string moduleId) => ModuleStarted?.Invoke(this, moduleId);
        protected virtual void OnModuleCompleted(string moduleId) => ModuleCompleted?.Invoke(this, moduleId);
        protected virtual void OnModuleFailed(string moduleId) => ModuleFailed?.Invoke(this, moduleId);
    }

    /// <summary>
    /// 模块信息
    /// </summary>
    public class ModuleInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Priority { get; set; }
        public bool IsEnabled { get; set; }
    }
}