using System;
using System.IO;
using System.Windows.Forms;
using ScottPlot;
namespace NgsBwaRunner
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 检查工具目录
            CheckToolsDirectory();

            // 全局异常处理
            AppDomain.CurrentDomain.UnhandledException += GlobalExceptionHandler;

            try
            {
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"启动失败：{ex.Message}", "启动错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static void GlobalExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            Utils.Logger.LogError($"未处理异常: {ex.Message}");
            MessageBox.Show($"程序发生未处理异常：{ex.Message}", "错误",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        static void CheckToolsDirectory()
        {
            string toolsDir = "./tools";
            if (!Directory.Exists(toolsDir))
            {
                Directory.CreateDirectory(toolsDir);
                MessageBox.Show($"已创建tools目录\n请将以下文件放入 {Path.GetFullPath(toolsDir)} 目录:\n" +
                              "1. bwa (Linux可执行文件)\n2. samtools\n3. bcftools",
                              "工具目录", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}