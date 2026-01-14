namespace LLC_MOD_Toolbox.Interfaces
{
    /// <summary>
    /// 启动器服务接口
    /// 提供快捷方式创建和管理功能
    /// </summary>
    public interface ILauncherService
    {
        /// <summary>
        /// 创建桌面快捷方式
        /// </summary>
        /// <param name="shortcutName">快捷方式名称</param>
        /// <param name="arguments">启动参数</param>
        /// <param name="description">快捷方式描述</param>
        /// <returns>创建的快捷方式完整路径</returns>
        string CreateDesktopShortcut(
            string shortcutName = "LimbusCompany with LLC.lnk",
            string arguments = "-launcher",
            string description = "启动边狱公司并检查汉化更新");

        /// <summary>
        /// 检查快捷方式是否已存在
        /// </summary>
        /// <param name="shortcutName">快捷方式名称</param>
        /// <returns>如果存在返回true，否则返回false</returns>
        bool ShortcutExists(string shortcutName = "LimbusCompany with LLC.lnk");

        /// <summary>
        /// 获取热更新使用说明文本
        /// </summary>
        /// <returns>使用说明文本</returns>
        string GetHotUpdateInstructions();
    }
}
