using LLC_MOD_Toolbox.Interfaces;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using WpfMessageBox = System.Windows.MessageBox;

namespace LLC_MOD_Toolbox.Services
{
    /// <summary>
    /// 对话框服务实现
    /// 提供各种对话框的显示功能
    /// 使用自定义UniversalDialog以保持与原项目一致的样式
    /// </summary>
    public class DialogService : IDialogService
    {
        private static bool IsHeadless()
        {
            return string.Equals(Environment.GetEnvironmentVariable("LLCMT_HEADLESS"), "1", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 显示消息对话框
        /// </summary>
        public void ShowMessage(string message, string title = "提示", Window? owner = null)
        {
            if (IsHeadless() || Application.Current == null)
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    UniversalDialog.ShowMessage(message, title, owner: owner);
                }
                catch
                {
                    // 如果UniversalDialog失败，回退到标准MessageBox
                    if (owner != null && owner.IsVisible)
                    {
                        WpfMessageBox.Show(owner, message, title, MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        WpfMessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            });
        }

        /// <summary>
        /// 显示确认对话框
        /// </summary>
        public bool ShowConfirm(string message, string title = "确认", Window? owner = null)
        {
            if (IsHeadless() || Application.Current == null)
            {
                return false;
            }

            var result = false;
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    result = UniversalDialog.ShowConfirm(message, title, owner);
                }
                catch
                {
                    // 如果UniversalDialog失败，回退到标准MessageBox
                    if (owner != null && owner.IsVisible)
                    {
                        result = WpfMessageBox.Show(owner, message, title, MessageBoxButton.YesNo, MessageBoxImage.Question)
                            == MessageBoxResult.Yes;
                    }
                    else
                    {
                        result = WpfMessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question)
                            == MessageBoxResult.Yes;
                    }
                }
            });
            return result;
        }

        /// <summary>
        /// 显示错误对话框
        /// </summary>
        public void ShowError(string message, string title = "错误", Window? owner = null)
        {
            if (IsHeadless() || Application.Current == null)
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    var buttons = new List<DialogButton>
                    {
                        new DialogButton("确认", true, false)
                    };
                    UniversalDialog.ShowMessage(message, title, buttons, owner);
                }
                catch
                {
                    // 如果UniversalDialog失败，回退到标准MessageBox
                    if (owner != null && owner.IsVisible)
                    {
                        WpfMessageBox.Show(owner, message, title, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        WpfMessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            });
        }

        /// <summary>
        /// 显示警告对话框
        /// </summary>
        public void ShowWarning(string message, string title = "警告", Window? owner = null)
        {
            if (IsHeadless() || Application.Current == null)
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    var buttons = new List<DialogButton>
                    {
                        new DialogButton("确认", true, false)
                    };
                    UniversalDialog.ShowMessage(message, title, buttons, owner);
                }
                catch
                {
                    // 如果UniversalDialog失败，回退到标准MessageBox
                    if (owner != null && owner.IsVisible)
                    {
                        WpfMessageBox.Show(owner, message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        WpfMessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            });
        }

        /// <summary>
        /// 显示文件选择对话框
        /// </summary>
        public async System.Threading.Tasks.Task<List<string>> ShowOpenFileDialogAsync(
            string title = "选择文件",
            string filter = "所有文件 (*.*)|*.*",
            bool multiSelect = false)
        {
            if (IsHeadless() || Application.Current == null)
            {
                return new List<string>();
            }

            return await System.Threading.Tasks.Task.Run(() =>
            {
                var result = new List<string>();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    var dialog = new OpenFileDialog
                    {
                        Title = title,
                        Filter = filter,
                        Multiselect = multiSelect
                    };

                    if (dialog.ShowDialog() == true)
                    {
                        result = dialog.FileNames.ToList();
                    }
                });

                return result;
            });
        }

        /// <summary>
        /// 显示文件夹选择对话框
        /// </summary>
        public async System.Threading.Tasks.Task<string?> ShowFolderDialogAsync(string description = "选择文件夹")
        {
            if (IsHeadless() || Application.Current == null)
            {
                return null;
            }

            return await System.Threading.Tasks.Task.Run(() =>
            {
                string? selectedPath = null;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    var dialog = new OpenFileDialog
                    {
                        Title = description,
                        Filter = "文件夹|*.*",
                        CheckFileExists = false,
                        CheckPathExists = true,
                        ValidateNames = false,
                        FileName = "选择文件夹"
                    };

                    if (dialog.ShowDialog() == true)
                    {
                        var directory = Path.GetDirectoryName(dialog.FileName);
                        if (!string.IsNullOrWhiteSpace(directory))
                        {
                            selectedPath = directory;
                        }
                    }
                });

                return selectedPath;
            });
        }
    }
}
