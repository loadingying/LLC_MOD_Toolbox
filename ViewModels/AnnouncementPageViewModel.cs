using LLC_MOD_Toolbox.Interfaces;
using LLC_MOD_Toolbox.Models;
using System.Windows;
using System.Windows.Threading;

namespace LLC_MOD_Toolbox.ViewModels
{
    /// <summary>
    /// 公告页面ViewModel
    /// 处理公告获取、显示和倒计时功能
    /// </summary>
    public class AnnouncementPageViewModel : ViewModelBase
    {
        private readonly IConfigService _configService;
        private readonly IAnnouncementService _announcementService;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;

        private string _announcementText = string.Empty;
        private string _countdownText = string.Empty;
        private bool _hasAnnouncement;
        private bool _isVisible;
        private bool _isCloseEnabled;
        private int _countdownSeconds;
        private DispatcherTimer? _countdownTimer;

        /// <summary>
        /// 初始化AnnouncementPageViewModel
        /// </summary>
        public AnnouncementPageViewModel(
            IConfigService configService,
            IAnnouncementService announcementService,
            IDialogService dialogService,
            INavigationService navigationService)
        {
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
            _announcementService = announcementService ?? throw new ArgumentNullException(nameof(announcementService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            // 初始化命令
            CloseAnnouncementCommand = new RelayCommand(ExecuteCloseAnnouncement);

        }

        #region 属性

        /// <summary>
        /// 公告文本内容
        /// </summary>
        public string AnnouncementText
        {
            get => _announcementText;
            set => SetProperty(ref _announcementText, value);
        }

        /// <summary>
        /// 倒计时文本
        /// </summary>
        public string CountdownText
        {
            get => _countdownText;
            set => SetProperty(ref _countdownText, value);
        }

        /// <summary>
        /// 是否有公告
        /// </summary>
        public bool HasAnnouncement
        {
            get => _hasAnnouncement;
            set => SetProperty(ref _hasAnnouncement, value);
        }

        /// <summary>
        /// 公告是否可见
        /// </summary>
        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        /// <summary>
        /// 是否允许关闭公告
        /// </summary>
        public bool IsCloseEnabled
        {
            get => _isCloseEnabled;
            set => SetProperty(ref _isCloseEnabled, value);
        }

        /// <summary>
        /// 倒计时秒数
        /// </summary>
        public int CountdownSeconds
        {
            get => _countdownSeconds;
            set => SetProperty(ref _countdownSeconds, value);
        }

        #endregion

        #region 命令

        /// <summary>
        /// 关闭公告命令
        /// </summary>
        public RelayCommand CloseAnnouncementCommand { get; }

        #endregion

        #region 命令实现

        private void ExecuteCloseAnnouncement()
        {
            try
            {
                if (!IsCloseEnabled)
                {
                    return;
                }

                // 停止倒计时
                _countdownTimer?.Stop();

                // 隐藏公告
                IsVisible = false;
                HasAnnouncement = false;
                IsCloseEnabled = false;
                CountdownText = string.Empty;

                _ = _navigationService.NavigateToInstallPageAsync("auto");
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"关闭公告失败：{ex.Message}");
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 检查新公告
        /// </summary>
        public async System.Threading.Tasks.Task<bool> CheckAnnouncementAsync()
        {
            try
            {
                if (!_configService.AppSettings.announcement.getAnno)
                {
                    Log.logger.Info("公告获取已关闭，跳过检查。");
                    return false;
                }

                var node = _configService.AppSettings.nodeSelect.defaultApiNode;
                var apiEndpoint = GetApiEndpoint(node);
                var currentVersion = _configService.AppSettings.announcement.annoVersion;

                Log.logger.Info($"公告检查开始，当前版本：{currentVersion}，节点：{node}");
                Log.logger.Info($"公告API端点：{apiEndpoint}");

                var announcement = await _announcementService.CheckForNewAnnouncementAsync(
                    apiEndpoint,
                    currentVersion);

                if (announcement == null)
                {
                    Log.logger.Warn("公告获取失败或返回空。");
                    return false;
                }

                if (!announcement.IsNew)
                {
                    Log.logger.Info($"无新公告。最新版本：{announcement.Version}，当前版本：{currentVersion}");
                    return false;
                }

                Log.logger.Info($"发现新公告，版本：{announcement.Version}，级别：{announcement.Level}");

                // 更新版本
                _configService.UpdateAndSave(settings =>
                {
                    settings.announcement.annoVersion = announcement.Version;
                });

                // 设置公告内容
                AnnouncementText = announcement.Content;
                HasAnnouncement = true;
                IsVisible = true;

                // 根据级别设置倒计时
                CountdownSeconds = announcement.CountdownSeconds;
                if (CountdownSeconds <= 0)
                {
                    CountdownText = string.Empty;
                    IsCloseEnabled = true;
                    return true;
                }

                // 启动倒计时
                IsCloseEnabled = false;
                StartCountdown();

                return true;
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"检查公告失败：{ex.Message}");
                return false;
            }
        }

        #endregion

        #region 私有方法

        private string GetApiEndpoint(string nodeName)
        {
            var nodeListPath = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Config",
                "NodeList.json");

            if (!System.IO.File.Exists(nodeListPath))
            {
                return "https://api.zeroasso.top/{0}";
            }

            try
            {
                var json = System.IO.File.ReadAllText(nodeListPath);
                var root = Newtonsoft.Json.JsonConvert.DeserializeObject<RootModel>(json);

                if (root?.ApiNode != null)
                {
                    var node = root.ApiNode.FirstOrDefault(n => n.Name == nodeName && n.IsDefault);
                    if (node != null)
                    {
                        return node.Endpoint;
                    }
                }

                return root?.ApiNode.FirstOrDefault()?.Endpoint ?? "https://api.zeroasso.top/{0}";
            }
            catch
            {
                return "https://api.zeroasso.top/{0}";
            }
        }

        private void StartCountdown()
        {
            _countdownTimer?.Stop();
            _countdownTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            _countdownTimer.Tick += (s, e) =>
            {
                if (CountdownSeconds > 0)
                {
                    CountdownSeconds--;
                    CountdownText = CountdownSeconds > 0
                        ? $"{CountdownSeconds}秒后自动关闭"
                        : string.Empty;
                }
                else
                {
                    _countdownTimer?.Stop();
                    IsCloseEnabled = true;
                    CountdownText = string.Empty;
                }
            };

            _countdownTimer.Start();
            UpdateCountdownText();
        }

        private void UpdateCountdownText()
        {
            CountdownText = CountdownSeconds > 0
                ? $"{CountdownSeconds}秒后自动关闭"
                : string.Empty;
        }

        #endregion
    }
}
