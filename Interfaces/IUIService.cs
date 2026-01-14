using LLC_MOD_Toolbox.Interfaces;
using System.Windows;

namespace LLC_MOD_Toolbox.Interfaces
{
    /// <summary>
    /// UI服务接口
    /// 统一管理UI状态和控制逻辑
    /// </summary>
    public interface IUIService
    {
        /// <summary>
        /// 全局操作是否启用
        /// </summary>
        bool GlobalOperationsEnabled { get; }

        /// <summary>
        /// 全局操作状态变化事件
        /// </summary>
        event EventHandler<bool>? GlobalOperationsChanged;

        /// <summary>
        /// 禁用所有全局操作
        /// </summary>
        Task DisableGlobalOperationsAsync();

        /// <summary>
        /// 启用所有全局操作
        /// </summary>
        Task EnableGlobalOperationsAsync();

        /// <summary>
        /// 显示消息对话框
        /// </summary>
        Task ShowMessageAsync(string message, string title = "提示");

        /// <summary>
        /// 显示确认对话框
        /// </summary>
        Task<bool> ShowConfirmAsync(string message, string title = "确认");

        /// <summary>
        /// 打开URL
        /// </summary>
        Task OpenUrlAsync(string url);

        /// <summary>
        /// 刷新页面状态
        /// </summary>
        Task RefreshPageStateAsync();

        /// <summary>
        /// 导航到指定页面
        /// </summary>
        Task NavigateToPageAsync(string pageName);

        /// <summary>
        /// 设置控件启用状态
        /// </summary>
        Task SetControlEnabledAsync(string controlName, bool enabled);

        /// <summary>
        /// 设置控件可见性
        /// </summary>
        Task SetControlVisibilityAsync(string controlName, bool visible);
    }
}
