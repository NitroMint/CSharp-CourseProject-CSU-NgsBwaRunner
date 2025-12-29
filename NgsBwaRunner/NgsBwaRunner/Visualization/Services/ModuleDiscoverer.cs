// Core/Visualization/Services/ModuleDiscoverer.cs
using NgsBwaRunner.Visualization.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NgsBwaRunner.Visualization.Services
{
    /// <summary>
    /// 模块发现器 - 自动发现所有可视化模块
    /// </summary>
    public class ModuleDiscoverer
    {
        private readonly List<IVisualizationModule> _modules = new();

        /// <summary>
        /// 发现并加载所有可视化模块
        /// </summary>
        public IEnumerable<IVisualizationModule> DiscoverAllModules()
        {
            _modules.Clear();

            // 1. 扫描当前程序集
            ScanAssembly(Assembly.GetExecutingAssembly());

            // 2. 扫描Modules目录下的DLL（未来扩展用）
            var modulesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Modules");
            if (Directory.Exists(modulesDir))
            {
                foreach (var dllFile in Directory.GetFiles(modulesDir, "*.dll"))
                {
                    try
                    {
                        var assembly = Assembly.LoadFrom(dllFile);
                        ScanAssembly(assembly);
                    }
                    catch (Exception ex)
                    {
                        Utils.Logger.LogWarning($"加载模块程序集失败: {dllFile}, 错误: {ex.Message}");
                    }
                }
            }

            // 按优先级排序
            return _modules.OrderBy(m => m.Priority).ToList();
        }

        private void ScanAssembly(Assembly assembly)
        {
            try
            {
                // 查找所有实现了IVisualizationModule接口的类
                var moduleTypes = assembly.GetTypes()
                    .Where(t => typeof(IVisualizationModule).IsAssignableFrom(t)
                             && !t.IsAbstract
                             && !t.IsInterface
                             && t.GetCustomAttribute<Attributes.VisualizationModuleAttribute>() != null);

                foreach (var type in moduleTypes)
                {
                    try
                    {
                        var module = Activator.CreateInstance(type) as IVisualizationModule;
                        if (module != null)
                        {
                            _modules.Add(module);
                            Utils.Logger.LogInfo($"发现可视化模块: {module.DisplayName} (ID: {module.ModuleId}, 优先级: {module.Priority})");
                        }
                    }
                    catch (Exception ex)
                    {
                        Utils.Logger.LogError($"创建模块实例失败: {type.Name}, 错误: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.Logger.LogError($"扫描程序集失败: {assembly.FullName}, 错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取启用的模块
        /// </summary>
        public IEnumerable<IVisualizationModule> GetEnabledModules()
        {
            return _modules.Where(m => m.IsEnabled).OrderBy(m => m.Priority);
        }
    }
}