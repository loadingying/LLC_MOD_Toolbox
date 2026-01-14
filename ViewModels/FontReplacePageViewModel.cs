using LLC_MOD_Toolbox.Interfaces;
using Microsoft.Win32;
using System.Windows;

namespace LLC_MOD_Toolbox.ViewModels
{
    /// <summary>
    /// 字体替换页面ViewModel
    /// 处理字体选择、预览、替换和还原功能
    /// </summary>
    public class FontReplacePageViewModel : ViewModelBase
    {
        private readonly IFontService _fontService;
        private readonly IPathService _pathService;
        private readonly IDialogService _dialogService;

        private string _fontFilePath = string.Empty;
        private double _fontSize = 16;
        private bool _isReplacing;
        private bool _isRestoring;
        private bool _hasBackupFont;
        private bool _hasChineseFontInstalled;

        /// <summary>
        /// 初始化FontReplacePageViewModel
        /// </summary>
        public FontReplacePageViewModel(
            IFontService fontService,
            IPathService pathService,
            IDialogService dialogService)
        {
            _fontService = fontService ?? throw new ArgumentNullException(nameof(fontService));
            _pathService = pathService ?? throw new ArgumentNullException(nameof(pathService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            // 初始化命令
            BrowseFontCommand = new RelayCommand(ExecuteBrowseFont);
            PreviewFontCommand = new AsyncRelayCommand(ExecutePreviewFontAsync);
            ReplaceFontCommand = new AsyncRelayCommand(ExecuteReplaceFontAsync, CanExecuteReplaceFont);
            RestoreFontCommand = new AsyncRelayCommand(ExecuteRestoreFontAsync, CanExecuteRestoreFont);

            // 初始化数据
            _ = InitializeAsync();
        }

        #region 属性

        /// <summary>
        /// 字体文件路径
        /// </summary>
        public string FontFilePath
        {
            get => _fontFilePath;
            set
            {
                if (SetProperty(ref _fontFilePath, value))
                {
                    ReplaceFontCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// 字体大小
        /// </summary>
        public double FontSize
        {
            get => _fontSize;
            set => SetProperty(ref _fontSize, value);
        }

        /// <summary>
        /// 是否正在替换字体
        /// </summary>
        public bool IsReplacing
        {
            get => _isReplacing;
            set
            {
                if (SetProperty(ref _isReplacing, value))
                {
                    ReplaceFontCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// 是否正在还原字体
        /// </summary>
        public bool IsRestoring
        {
            get => _isRestoring;
            set
            {
                if (SetProperty(ref _isRestoring, value))
                {
                    RestoreFontCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// 是否有字体备份
        /// </summary>
        public bool HasBackupFont
        {
            get => _hasBackupFont;
            set
            {
                if (SetProperty(ref _hasBackupFont, value))
                {
                    RestoreFontCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// 是否已安装中文汉化字体
        /// </summary>
        public bool HasChineseFontInstalled
        {
            get => _hasChineseFontInstalled;
            set => SetProperty(ref _hasChineseFontInstalled, value);
        }

        /// <summary>
        /// 预览字体家族名称
        /// </summary>
        public string? PreviewFontFamily { get; private set; }

        /// <summary>
        /// 预览字体大小
        /// </summary>
        public double PreviewFontSize => FontSize;

        /// <summary>
        /// 预览小字体大小
        /// </summary>
        public double PreviewSmallFontSize => FontSize / 16 * 12;

        #endregion

        #region 命令

        /// <summary>
        /// 浏览字体命令
        /// </summary>
        public RelayCommand BrowseFontCommand { get; }

        /// <summary>
        /// 预览字体命令
        /// </summary>
        public AsyncRelayCommand PreviewFontCommand { get; }

        /// <summary>
        /// 替换字体命令
        /// </summary>
        public AsyncRelayCommand ReplaceFontCommand { get; }

        /// <summary>
        /// 还原字体命令
        /// </summary>
        public AsyncRelayCommand RestoreFontCommand { get; }

        #endregion

        #region 命令实现

        private void ExecuteBrowseFont()
        {
            try
            {
                var fileDialog = new OpenFileDialog
                {
                    Title = "请选择你的字体",
                    Filter = "字体文件 (*.ttf;*.otf;*.ttc)|*.ttf;*.otf;*.ttc|所有文件 (*.*)|*.*",
                    Multiselect = false
                };

                if (fileDialog.ShowDialog() == true)
                {
                    FontFilePath = System.IO.Path.GetFullPath(fileDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"浏览字体文件失败：{ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task ExecutePreviewFontAsync()
        {
            try
            {
                // 验证字体大小
                if (FontSize <= 0 || FontSize > 72)
                {
                    _dialogService.ShowMessage("请输入正确的字体大小（1-72）。", "提示");
                    return;
                }

                // 验证字体文件
                if (!_fontService.IsValidFontFile(FontFilePath))
                {
                    _dialogService.ShowMessage("请选择正确的字体文件。", "提示");
                    return;
                }

                // 获取字体家族名称
                var fontFamilyName = _fontService.GetFontFamilyName(FontFilePath);
                PreviewFontFamily = fontFamilyName;

                // 通知UI更新
                OnPropertyChanged(nameof(PreviewFontFamily));
                OnPropertyChanged(nameof(PreviewFontSize));
                OnPropertyChanged(nameof(PreviewSmallFontSize));

                _dialogService.ShowMessage(
                    "已将预览文本切换为自定义字体。\n如果出现部分字显示为默认字体，可能影响游戏内显示。",
                    "提示");
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"预览字体失败：{ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task ExecuteReplaceFontAsync()
        {
            try
            {
                var gameDir = _pathService.GameDirectory;
                if (string.IsNullOrWhiteSpace(gameDir))
                {
                    _dialogService.ShowError("游戏路径未设置，请先设置游戏路径。");
                    return;
                }

                IsReplacing = true;

                var result = await _fontService.ReplaceFontAsync(FontFilePath, gameDir);

                if (result.Success)
                {
                    var message = result.HasBackup
                        ? "字体替换成功（已备份原字体）。\n启动游戏以应用更改。"
                        : "字体替换成功。\n启动游戏以应用更改。";

                    _dialogService.ShowMessage(message, "提示");
                    HasBackupFont = true;
                }
                else
                {
                    _dialogService.ShowError(result.ErrorMessage ?? "字体替换失败");
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"字体替换失败：{ex.Message}");
            }
            finally
            {
                IsReplacing = false;
            }
        }

        private bool CanExecuteReplaceFont()
        {
            return !IsReplacing &&
                   !string.IsNullOrWhiteSpace(FontFilePath) &&
                   _fontService.IsValidFontFile(FontFilePath);
        }

        private async System.Threading.Tasks.Task ExecuteRestoreFontAsync()
        {
            try
            {
                var gameDir = _pathService.GameDirectory;
                if (string.IsNullOrWhiteSpace(gameDir))
                {
                    _dialogService.ShowError("游戏路径未设置，请先设置游戏路径。");
                    return;
                }

                IsRestoring = true;

                var result = await _fontService.RestoreFontAsync(gameDir);

                if (result.Success)
                {
                    _dialogService.ShowMessage("字体还原成功。\n启动游戏以应用更改。", "提示");
                    HasBackupFont = false;
                }
                else
                {
                    _dialogService.ShowError(result.ErrorMessage ?? "字体还原失败");
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"字体还原失败：{ex.Message}");
            }
            finally
            {
                IsRestoring = false;
            }
        }

        private bool CanExecuteRestoreFont()
        {
            return !IsRestoring && HasBackupFont;
        }

        #endregion

        #region 私有方法

        private async System.Threading.Tasks.Task InitializeAsync()
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    var gameDir = _pathService.GameDirectory;
                    if (!string.IsNullOrWhiteSpace(gameDir))
                    {
                        var hasBackup = _fontService.HasBackupFont(gameDir);
                        var hasChinese = _fontService.HasChineseFontInstalled(gameDir);

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            HasBackupFont = hasBackup;
                            HasChineseFontInstalled = hasChinese;
                        });
                    }
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _dialogService.ShowError($"初始化字体页面失败：{ex.Message}");
                    });
                }
            });
        }

        #endregion
    }
}
