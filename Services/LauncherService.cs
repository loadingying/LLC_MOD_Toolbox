using LLC_MOD_Toolbox;
using LLC_MOD_Toolbox.Interfaces;
using System.IO;
using System.Reflection;

namespace LLC_MOD_Toolbox.Services
{
    /// <summary>
    /// 启动器服务实现
    /// 提供快捷方式创建和管理功能
    /// </summary>
    public class LauncherService : ILauncherService
    {
        private readonly string _currentDirectory;

        /// <summary>
        /// 初始化LauncherService
        /// </summary>
        public LauncherService()
        {
            _currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        }

        /// <summary>
        /// 创建桌面快捷方式
        /// </summary>
        public string CreateDesktopShortcut(
            string shortcutName = "LimbusCompany with LLC.lnk",
            string arguments = "-launcher",
            string description = "启动边狱公司并检查汉化更新")
        {
            try
            {
                // 获取桌面路径
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string shortcutPath = Path.Combine(desktopPath, shortcutName);

                // 使用反射动态创建快捷方式，避免COM引用问题
                CreateShortcutViaReflection(shortcutPath, arguments, description);

                Log.logger.Info($"快捷方式已创建: {shortcutPath}");
                return shortcutPath;
            }
            catch (System.Exception ex)
            {
                Log.logger.Error("创建快捷方式失败", ex);
                throw;
            }
        }

        /// <summary>
        /// 检查快捷方式是否已存在
        /// </summary>
        public bool ShortcutExists(string shortcutName = "LimbusCompany with LLC.lnk")
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string shortcutPath = Path.Combine(desktopPath, shortcutName);
            return File.Exists(shortcutPath);
        }

        /// <summary>
        /// 获取热更新使用说明文本
        /// </summary>
        public string GetHotUpdateInstructions()
        {
            return "你想要知道怎么用热更新？那你可找对地方了兄弟！" +
                   "\n你现在有两种方式，随你便，你想用哪个就用哪个：" +
                   "\n1. 从快捷方式启动" +
                   "\n - 点击旁边的发送按钮" +
                   "\n - 后续在桌面点击 LimbusCompany with LLC即可在启动游戏前检查是否更新汉化并自动安装" +
                   "\n2. 从Steam启动" +
                   "\n - 打开安装器的所在文件夹，选中LLC_MOD_Toolbox.exe，并复制其地址" +
                   "\n * 怎么复制地址：右键LLC_MOD_Toolbox-复制文件地址" +
                   "\n - 打开您的Steam库页面，在最左下角唤起\"添加非Steam游戏\"菜单" +
                   "\n - 在该菜单中选择并打开LLC_MOD_Toolbox.exe，直接在文件名那里粘贴你刚刚复制的地址然后回车就可以了" +
                   "\n - LLC_MOD_Toolbox将会出现在选单内，确认其选中状态并确认添加" +
                   "\n - 在您的Steam库中找到LLC_MOD_Toolbox，在启动选项内填入-launcher(全小写)" +
                   "\n这两种方法效果相同，根据自己喜好选择。";
        }

        /// <summary>
        /// 使用反射创建快捷方式，避免直接COM引用
        /// </summary>
        private void CreateShortcutViaReflection(string shortcutPath, string arguments, string description)
        {
            try
            {
                // 尝试使用WshShell COM对象创建快捷方式
                Type? wshShellType = Type.GetTypeFromProgID("WScript.Shell");
                if (wshShellType != null)
                {
                    dynamic? wsh = System.Activator.CreateInstance(wshShellType);
                    if (wsh != null)
                    {
                        dynamic? shortcut = wsh.CreateShortcut(shortcutPath);
                        if (shortcut != null)
                        {
                            shortcut.TargetPath = Path.Combine(_currentDirectory, "LLC_MOD_Toolbox.exe");
                            shortcut.Arguments = arguments;
                            shortcut.WorkingDirectory = _currentDirectory;
                            shortcut.Description = description;
                            shortcut.IconLocation = Path.Combine(_currentDirectory, "PublicResource", "favicon.ico");
                            shortcut.Save();
                            return;
                        }
                    }
                }

                // 如果COM不可用，抛出异常
                throw new System.Exception("无法创建WScript.Shell对象，请确保系统支持Windows Script Host。");
            }
            catch (System.Exception ex)
            {
                Log.logger.Error("使用反射创建快捷方式失败", ex);
                throw;
            }
        }
    }
}
