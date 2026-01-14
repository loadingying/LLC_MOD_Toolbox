using System.Windows;

namespace LLC_MOD_Toolbox.Interfaces
{
    /// <summary>
    /// 对话框服务接口
    /// 提供各种对话框的显示功能
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// 显示消息对话框
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="title">标题</param>
        /// <param name="owner">所有者窗口</param>
        void ShowMessage(string message, string title = "提示", Window? owner = null);

        /// <summary>
        /// 显示确认对话框
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="title">标题</param>
        /// <param name="owner">所有者窗口</param>
        /// <returns>用户选择结果</returns>
        bool ShowConfirm(string message, string title = "确认", Window? owner = null);

        /// <summary>
        /// 显示错误对话框
        /// </summary>
        /// <param name="message">错误消息</param>
        /// <param name="title">标题</param>
        /// <param name="owner">所有者窗口</param>
        void ShowError(string message, string title = "错误", Window? owner = null);

        /// <summary>
        /// 显示警告对话框
        /// </summary>
        /// <param name="message">警告消息</param>
        /// <param name="title">标题</param>
        /// <param name="owner">所有者窗口</param>
        void ShowWarning(string message, string title = "警告", Window? owner = null);

        /// <summary>
        /// 显示文件选择对话框
        /// </summary>
        /// <param name="title">对话框标题</param>
        /// <param name="filter">文件过滤器</param>
        /// <param name="multiSelect">是否允许多选</param>
        /// <returns>选中的文件路径列表，取消时返回空列表</returns>
        System.Threading.Tasks.Task<List<string>> ShowOpenFileDialogAsync(
            string title = "选择文件",
            string filter = "所有文件 (*.*)|*.*",
            bool multiSelect = false);

        /// <summary>
        /// 显示文件夹选择对话框
        /// </summary>
        /// <param name="description">描述</param>
        /// <returns>选中的文件夹路径，取消时返回null</returns>
        System.Threading.Tasks.Task<string?> ShowFolderDialogAsync(string description = "选择文件夹");
    }
}
