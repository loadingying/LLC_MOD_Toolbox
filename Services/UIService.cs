using LLC_MOD_Toolbox.Interfaces;
using System.Diagnostics;
using System.Windows;

namespace LLC_MOD_Toolbox.Services
{
    /// <summary>
    /// UI服务实现
    /// 统一管理UI状态和控制逻辑
    /// </summary>
    public class UIService : IUIService
    {
        private readonly IDialogService _dialogService;
        private bool _globalOperationsEnabled = true;

        /// <summary>
        /// 全局操作状态变化事件
        /// </summary>
        public event EventHandler<bool>? GlobalOperationsChanged;

        /// <summary>
        /// 全局操作是否启用
        /// </summary>
        public bool GlobalOperationsEnabled => _globalOperationsEnabled;

        /// <summary>
        /// 初始化UIService
        /// </summary>
        public UIService(IDialogService dialogService)
        {
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        }

        /// <summary>
        /// 禁用所有全局操作
        /// </summary>
        public async Task DisableGlobalOperationsAsync()
        {
            _globalOperationsEnabled = false;
            OnGlobalOperationsChanged(false);
            Log.logger.Info("全局操作已禁用");
            await Task.CompletedTask;
        }

        /// <summary>
        /// 启用所有全局操作
        /// </summary>
        public async Task EnableGlobalOperationsAsync()
        {
            _globalOperationsEnabled = true;
            OnGlobalOperationsChanged(true);
            Log.logger.Info("全局操作已启用");
            await Task.CompletedTask;
        }

        /// <summary>
        /// 显示消息对话框
        /// </summary>
        public async Task ShowMessageAsync(string message, string title = "提示")
        {
            _dialogService.ShowMessage(message, title);
            await Task.CompletedTask;
        }

        /// <summary>
        /// 显示确认对话框
        /// </summary>
        public async Task<bool> ShowConfirmAsync(string message, string title = "确认")
        {
            var result = _dialogService.ShowConfirm(message, title);
            await Task.CompletedTask;
            return result;
        }

        /// <summary>
        /// 打开URL
        /// </summary>
        public async Task OpenUrlAsync(string url)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(url))
                {
                    Log.logger.Warn("尝试打开空URL");
                    return;
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });

                Log.logger.Info($"打开URL: {url}");
            }
            catch (Exception ex)
            {
                Log.logger.Error("打开URL失败", ex);
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// 刷新页面状态
        /// </summary>
        public async Task RefreshPageStateAsync()
        {
            // UI状态刷新将由订阅者处理
            Log.logger.Info("刷新页面状态");
            await Task.CompletedTask;
        }

        /// <summary>
        /// 导航到指定页面
        /// </summary>
        public async Task NavigateToPageAsync(string pageName)
        {
            Log.logger.Info($"导航到页面: {pageName}");
            await Task.CompletedTask;
        }

        /// <summary>
        /// 设置控件启用状态
        /// </summary>
        public async Task SetControlEnabledAsync(string controlName, bool enabled)
        {
            Log.logger.Info($"控件 {controlName} 启用状态: {enabled}");
            await Task.CompletedTask;
        }

        /// <summary>
        /// 设置控件可见性
        /// </summary>
        public async Task SetControlVisibilityAsync(string controlName, bool visible)
        {
            Log.logger.Info($"控件 {controlName} 可见性: {visible}");
            await Task.CompletedTask;
        }

        /// <summary>
        /// 触发全局操作状态变化事件
        /// </summary>
        protected virtual void OnGlobalOperationsChanged(bool enabled)
        {
            GlobalOperationsChanged?.Invoke(this, enabled);
        }
    }
}
