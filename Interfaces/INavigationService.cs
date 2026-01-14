using System.Windows;
using System.Windows.Controls;

namespace LLC_MOD_Toolbox.Interfaces
{
    /// <summary>
    /// 页面导航服务接口
    /// 负责管理页面切换和UI状态更新
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// 当前主页面
        /// </summary>
        string CurrentPage { get; }

        /// <summary>
        /// 当前安装子页面
        /// </summary>
        string CurrentInstallPage { get; }

        /// <summary>
        /// 是否正在安装
        /// </summary>
        bool IsInstalling { get; set; }

        /// <summary>
        /// 初始化导航服务
        /// </summary>
        /// <param name="pages">页面字典（页面名 -> Grid）</param>
        /// <param name="controls">控件字典（控件名 -> FrameworkElement）</param>
        void InitializeNavigation(Dictionary<string, Grid> pages, Dictionary<string, FrameworkElement> controls);

        /// <summary>
        /// 导航到指定页面
        /// </summary>
        /// <param name="pageName">页面名称</param>
        Task NavigateToAsync(string pageName);

        /// <summary>
        /// 导航到安装子页面
        /// </summary>
        /// <param name="installPageName">安装子页面名称（auto/font/gacha）</param>
        Task NavigateToInstallPageAsync(string installPageName);

        /// <summary>
        /// 刷新页面状态
        /// </summary>
        Task RefreshPageStateAsync();

        /// <summary>
        /// 设置页面可见性
        /// </summary>
        /// <param name="pageName">页面名称</param>
        /// <param name="visible">是否可见</param>
        Task SetPageVisibilityAsync(string pageName, bool visible);

        /// <summary>
        /// 设置控件可见性
        /// </summary>
        /// <param name="controlName">控件名称</param>
        /// <param name="visible">是否可见</param>
        Task SetControlVisibilityAsync(string controlName, bool visible);

        /// <summary>
        /// 设置控件启用状态
        /// </summary>
        /// <param name="controlName">控件名称</param>
        /// <param name="enabled">是否启用</param>
        Task SetControlEnabledAsync(string controlName, bool enabled);
    }
}
