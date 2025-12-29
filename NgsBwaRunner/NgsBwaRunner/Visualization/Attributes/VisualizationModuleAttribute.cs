// Visualization/Attributes/VisualizationModuleAttribute.cs
using System;

namespace NgsBwaRunner.Visualization.Attributes
{
    /// <summary>
    /// 可视化模块特性（用于自动发现模块）
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class VisualizationModuleAttribute : Attribute
    {
        /// <summary>
        /// 模块唯一标识
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// 模块显示名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 模块描述
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// 执行优先级（1-100，数值越小越先执行）
        /// </summary>
        public int Priority { get; }

        /// <summary>
        /// 是否自动注册（默认true）
        /// </summary>
        public bool AutoRegister { get; set; } = true;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="id">模块唯一标识</param>
        /// <param name="name">模块显示名称</param>
        /// <param name="description">模块描述</param>
        /// <param name="priority">执行优先级</param>
        public VisualizationModuleAttribute(string id, string name, string description, int priority = 50)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            Priority = priority;
        }
    }
}