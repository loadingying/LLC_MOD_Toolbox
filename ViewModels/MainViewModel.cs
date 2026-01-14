using LLC_MOD_Toolbox.Interfaces;
using LLC_MOD_Toolbox.Models;
using System.Windows;

namespace LLC_MOD_Toolbox.ViewModels
{
    /// <summary>
    /// 主窗口ViewModel
    /// 协调各个服务，提供UI数据绑定和命令处理
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly IConfigService _configService;
        private readonly IPathService _pathService;
        private readonly IDialogService _dialogService;
        private readonly IDownloadService _downloadService;
        private readonly IFileUtilityService _fileUtilityService;
        private readonly IModInstallService _modInstallService;
        private readonly IModUninstallService _modUninstallService;
        private readonly IGachaSimulatorService _gachaSimulatorService;
        private readonly INodeManagementService _nodeManagementService;
        private readonly IVersionService _versionService;
        private readonly INavigationService _navigationService;
        private readonly ILauncherService _launcherService;
        private readonly ILinkService _linkService;

        // 窗口引用（用于窗口控制命令）
        private Window? _ownerWindow;

        // 彩蛋状态
        private bool _eeOpening = false;
        private bool _eeEntered = false;
        private bool _isInAnno = false;

        // UI状态属性
        private bool _isInstalling;
        private float _installProgress;
        private string _currentVersionText = "当前版本：检查中...";
        private string _latestVersionText = "最新版本：检查中...";
        private bool _isGameRunning;
        private string _limbusCompanyDir = string.Empty;
        private string _useEndPoint = string.Empty;
        private string _useAPIEndPoint = string.Empty;
        private bool _useGithub = false;
        private bool _isMirrorChyanMode = false;
        private string _mirrorChyanToken = string.Empty;
        private int _installPhase = 0;
        private bool _isNewestModVersion = true;
        private bool _isGachaInitialized = false;

        // 节点列表
        private List<Node> _nodeItemList = new();
        private List<Node> _apiItemList = new();
        private Node? _selectedNode;
        private Node? _selectedApi;

        /// <summary>
        /// 初始化MainViewModel
        /// </summary>
        public MainViewModel(
            IConfigService configService,
            IPathService pathService,
            IDialogService dialogService,
            IDownloadService downloadService,
            IFileUtilityService fileUtilityService,
            IModInstallService modInstallService,
            IModUninstallService modUninstallService,
            IGachaSimulatorService gachaSimulatorService,
            INodeManagementService nodeManagementService,
            IVersionService versionService,
            INavigationService navigationService,
            ILauncherService launcherService,
            ILinkService linkService)
        {
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
            _pathService = pathService ?? throw new ArgumentNullException(nameof(pathService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _downloadService = downloadService ?? throw new ArgumentNullException(nameof(downloadService));
            _fileUtilityService = fileUtilityService ?? throw new ArgumentNullException(nameof(fileUtilityService));
            _modInstallService = modInstallService ?? throw new ArgumentNullException(nameof(modInstallService));
            _modUninstallService = modUninstallService ?? throw new ArgumentNullException(nameof(modUninstallService));
            _gachaSimulatorService = gachaSimulatorService ?? throw new ArgumentNullException(nameof(gachaSimulatorService));
            _nodeManagementService = nodeManagementService ?? throw new ArgumentNullException(nameof(nodeManagementService));
            _versionService = versionService ?? throw new ArgumentNullException(nameof(versionService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _launcherService = launcherService ?? throw new ArgumentNullException(nameof(launcherService));
            _linkService = linkService ?? throw new ArgumentNullException(nameof(linkService));
            
            // 确保链接服务已初始化
            _linkService.InitializeDefaultLinks();

            // 初始化命令
            InstallCommand = new AsyncRelayCommand(
                ExecuteInstallAsync,
                CanExecuteInstall);

            UninstallCommand = new AsyncRelayCommand(
                ExecuteUninstallAsync,
                CanExecuteUninstall);

            RefreshVersionCommand = new AsyncRelayCommand(
                ExecuteRefreshVersionAsync);

            OpenGameCommand = new RelayCommand(
                ExecuteOpenGame,
                CanExecuteOpenGame);

            InitializeNodesCommand = new RelayCommand(
                ExecuteInitializeNodes);

            MinimizeCommand = new RelayCommand(
                ExecuteMinimize,
                CanExecuteMinimize);

            CloseCommand = new RelayCommand(
                ExecuteClose);

            OpenUrlCommand = new RelayCommand<string>(
                ExecuteOpenUrl);

            NavigateToAutoInstallCommand = new AsyncRelayCommand(
                () => _navigationService.NavigateToInstallPageAsync("auto"));

            NavigateToFontReplaceCommand = new AsyncRelayCommand(
                () => _navigationService.NavigateToInstallPageAsync("font"));

            NavigateToGachaSimCommand = new AsyncRelayCommand(
                ExecuteNavigateToGachaSimAsync);

            NavigateToInstallCommand = new AsyncRelayCommand(
                () => _navigationService.NavigateToAsync("install"));

            NavigateToLinkCommand = new AsyncRelayCommand(
                () => _navigationService.NavigateToAsync("link"));

            NavigateToGreytestCommand = new AsyncRelayCommand(
                () => _navigationService.NavigateToAsync("greytest"));

            NavigateToSettingsCommand = new AsyncRelayCommand(
                () => _navigationService.NavigateToAsync("settings"));

            NavigateToAboutCommand = new AsyncRelayCommand(
                () => _navigationService.NavigateToAsync("about"));

            NavigateToEECommand = new AsyncRelayCommand(
                () => _navigationService.NavigateToAsync("ee"));

            ShowHotUpdateHelpCommand = new RelayCommand(
                ExecuteShowHotUpdateHelp);

            TriggerEasterEggCommand = new RelayCommand(
                ExecuteTriggerEasterEgg,
                CanExecuteTriggerEasterEgg);

            SendLauncherCommand = new RelayCommand(
                ExecuteSendLauncher);

            SendHelpLauncherCommand = new RelayCommand(
                ExecuteSendHelpLauncher);

            // 初始化数据
            _ = InitializeAsync();
        }

        /// <summary>
        /// 设置所有者窗口
        /// </summary>
        public void SetOwnerWindow(Window window)
        {
            _ownerWindow = window ?? throw new ArgumentNullException(nameof(window));
        }

        #region 属性

        /// <summary>
        /// 是否正在安装
        /// </summary>
        public bool IsInstalling
        {
            get => _isInstalling;
            set => SetProperty(ref _isInstalling, value);
        }

        /// <summary>
        /// 安装进度（0-100）
        /// </summary>
        public float InstallProgress
        {
            get => _installProgress;
            set => SetProperty(ref _installProgress, value);
        }

        /// <summary>
        /// 当前版本文本
        /// </summary>
        public string CurrentVersionText
        {
            get => _currentVersionText;
            set => SetProperty(ref _currentVersionText, value);
        }

        /// <summary>
        /// 最新版本文本
        /// </summary>
        public string LatestVersionText
        {
            get => _latestVersionText;
            set => SetProperty(ref _latestVersionText, value);
        }

        /// <summary>
        /// 游戏是否正在运行
        /// </summary>
        public bool IsGameRunning
        {
            get => _isGameRunning;
            set => SetProperty(ref _isGameRunning, value);
        }

        /// <summary>
        /// Limbus Company游戏目录
        /// </summary>
        public string LimbusCompanyDir
        {
            get => _limbusCompanyDir;
            set => SetProperty(ref _limbusCompanyDir, value);
        }

        /// <summary>
        /// 当前使用的下载节点端点
        /// </summary>
        public string UseEndPoint
        {
            get => _useEndPoint;
            set => SetProperty(ref _useEndPoint, value);
        }

        /// <summary>
        /// 当前使用的API节点端点
        /// </summary>
        public string UseAPIEndPoint
        {
            get => _useAPIEndPoint;
            set => SetProperty(ref _useAPIEndPoint, value);
        }

        /// <summary>
        /// 是否使用Github直连
        /// </summary>
        public bool UseGithub
        {
            get => _useGithub;
            set => SetProperty(ref _useGithub, value);
        }

        /// <summary>
        /// 是否为MirrorChyan模式
        /// </summary>
        public bool IsMirrorChyanMode
        {
            get => _isMirrorChyanMode;
            set => SetProperty(ref _isMirrorChyanMode, value);
        }

        /// <summary>
        /// MirrorChyan Token
        /// </summary>
        public string MirrorChyanToken
        {
            get => _mirrorChyanToken;
            set => SetProperty(ref _mirrorChyanToken, value);
        }

        /// <summary>
        /// 是否为最新模组版本
        /// </summary>
        public bool IsNewestModVersion
        {
            get => _isNewestModVersion;
            set => SetProperty(ref _isNewestModVersion, value);
        }

        /// <summary>
        /// 下载节点列表
        /// </summary>
        public List<Node> NodeItemList
        {
            get => _nodeItemList;
            set => SetProperty(ref _nodeItemList, value);
        }

        /// <summary>
        /// API节点列表
        /// </summary>
        public List<Node> ApiItemList
        {
            get => _apiItemList;
            set => SetProperty(ref _apiItemList, value);
        }

        /// <summary>
        /// 选中的下载节点
        /// </summary>
        public Node? SelectedNode
        {
            get => _selectedNode;
            set => SetProperty(ref _selectedNode, value);
        }

        /// <summary>
        /// 选中的API节点
        /// </summary>
        public Node? SelectedApi
        {
            get => _selectedApi;
            set => SetProperty(ref _selectedApi, value);
        }

        /// <summary>
        /// 配置服务
        /// </summary>
        public IConfigService Config => _configService;

        /// <summary>
        /// 路径服务
        /// </summary>
        public IPathService Path => _pathService;

        /// <summary>
        /// 文件工具服务
        /// </summary>
        public IFileUtilityService FileUtility => _fileUtilityService;

        /// <summary>
        /// 模组安装服务
        /// </summary>
        public IModInstallService ModInstall => _modInstallService;

        /// <summary>
        /// 模组卸载服务
        /// </summary>
        public IModUninstallService ModUninstall => _modUninstallService;

        /// <summary>
        /// 抽卡模拟器服务
        /// </summary>
        public IGachaSimulatorService GachaSimulator => _gachaSimulatorService;

        /// <summary>
        /// 节点管理服务
        /// </summary>
        public INodeManagementService NodeManagement => _nodeManagementService;

        /// <summary>
        /// 版本服务
        /// </summary>
        public IVersionService Version => _versionService;

        /// <summary>
        /// 链接字典（通过服务获取）
        /// </summary>
        public Dictionary<string, string> LinkDictionary => _linkService.GetLinks();

        #endregion

        #region 命令

        /// <summary>
        /// 安装命令
        /// </summary>
        public AsyncRelayCommand InstallCommand { get; }

        /// <summary>
        /// 卸载命令
        /// </summary>
        public AsyncRelayCommand UninstallCommand { get; }

        /// <summary>
        /// 刷新版本命令
        /// </summary>
        public AsyncRelayCommand RefreshVersionCommand { get; }

        /// <summary>
        /// 打开游戏命令
        /// </summary>
        public RelayCommand OpenGameCommand { get; }

        /// <summary>
        /// 初始化节点命令
        /// </summary>
        public RelayCommand InitializeNodesCommand { get; }

        /// <summary>
        /// 最小化窗口命令
        /// </summary>
        public RelayCommand MinimizeCommand { get; }

        /// <summary>
        /// 关闭窗口命令
        /// </summary>
        public RelayCommand CloseCommand { get; }

        /// <summary>
        /// 打开URL命令
        /// </summary>
        public RelayCommand<string> OpenUrlCommand { get; }

        /// <summary>
        /// 导航到自动安装页面命令
        /// </summary>
        public AsyncRelayCommand NavigateToAutoInstallCommand { get; }

        /// <summary>
        /// 导航到字体替换页面命令
        /// </summary>
        public AsyncRelayCommand NavigateToFontReplaceCommand { get; }

        /// <summary>
        /// 导航到抽卡模拟器页面命令
        /// </summary>
        public AsyncRelayCommand NavigateToGachaSimCommand { get; }

        /// <summary>
        /// 导航到安装页面命令
        /// </summary>
        public AsyncRelayCommand NavigateToInstallCommand { get; }

        /// <summary>
        /// 导航到链接页面命令
        /// </summary>
        public AsyncRelayCommand NavigateToLinkCommand { get; }

        /// <summary>
        /// 导航到灰度测试页面命令
        /// </summary>
        public AsyncRelayCommand NavigateToGreytestCommand { get; }

        /// <summary>
        /// 导航到设置页面命令
        /// </summary>
        public AsyncRelayCommand NavigateToSettingsCommand { get; }

        /// <summary>
        /// 导航到关于页面命令
        /// </summary>
        public AsyncRelayCommand NavigateToAboutCommand { get; }

        /// <summary>
        /// 导航到彩蛋页面命令
        /// </summary>
        public AsyncRelayCommand NavigateToEECommand { get; }

        /// <summary>
        /// 显示热更新帮助命令
        /// </summary>
        public RelayCommand ShowHotUpdateHelpCommand { get; }

        /// <summary>
        /// 触发彩蛋命令
        /// </summary>
        public RelayCommand TriggerEasterEggCommand { get; }

        /// <summary>
        /// 发送启动器到桌面命令
        /// </summary>
        public RelayCommand SendLauncherCommand { get; }

        /// <summary>
        /// 发送帮助启动器到桌面命令
        /// </summary>
        public RelayCommand SendHelpLauncherCommand { get; }

        #endregion

        #region 命令实现

        /// <summary>
        /// 执行安装命令
        /// </summary>
        private async System.Threading.Tasks.Task ExecuteInstallAsync()
        {
            try
            {
                IsInstalling = true;
                UpdateInstallProgress(0);

                // 检查游戏路径
                if (string.IsNullOrWhiteSpace(_limbusCompanyDir))
                {
                    var needFind = _dialogService.ShowConfirm(
                        "未找到游戏路径，是否自动查找？",
                        "提示");

                    if (needFind)
                    {
                        try
                        {
                            _pathService.FindGamePath();
                            LimbusCompanyDir = _pathService.GameDirectory;
                            _dialogService.ShowMessage($"游戏路径：{_pathService.GameDirectory}");
                        }
                        catch (Exception ex)
                        {
                            _dialogService.ShowError($"查找游戏路径失败：{ex.Message}");
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }
                }

                // 检查游戏是否运行
                if (_pathService.IsGameRunning())
                {
                    var continueInstall = _dialogService.ShowConfirm(
                        "检测到游戏正在运行，建议关闭游戏后继续安装。\n是否继续？",
                        "警告");

                    if (!continueInstall)
                    {
                        return;
                    }
                }

                // 执行安装
                UpdateInstallProgress(10);

                // 重新从配置读取Mirror酱状态，确保使用最新配置
                _isMirrorChyanMode = _configService.AppSettings.mirrorChyan.enable;
                if (_isMirrorChyanMode)
                {
                    try
                    {
                        _mirrorChyanToken = SecureStringStorage.LoadToken();
                    }
                    catch
                    {
                        _mirrorChyanToken = string.Empty;
                    }
                }

                // 检查是否需要安装字体
                bool needFont = _modInstallService.NeedsFontInstallation(_limbusCompanyDir);
                if (needFont)
                {
                    UpdateInstallProgress(20);
                    await _modInstallService.InstallFontAsync(
                        _limbusCompanyDir,
                        _isMirrorChyanMode,
                        _mirrorChyanToken,
                        _useGithub,
                        _useEndPoint,
                        _nodeManagementService.GetDefaultDownloadNodeEndpoint(),
                        progress => UpdateInstallProgress(20 + progress * 0.3f));
                }

                UpdateInstallProgress(50);

                // 检查是否需要安装模组
                int currentVersion = await _modInstallService.GetCurrentModVersionAsync(_limbusCompanyDir);
                int latestVersion;

                if (_isMirrorChyanMode)
                {
                    if (string.IsNullOrWhiteSpace(_mirrorChyanToken))
                    {
                        _dialogService.ShowError("Mirror酱 Token 未设置，无法获取最新版本。");
                        return;
                    }

                    latestVersion = await _versionService.GetLatestVersionFromMirrorChyanAsync(_mirrorChyanToken);
                }
                else
                {
                    var apiEndpoint = string.IsNullOrWhiteSpace(_useAPIEndPoint)
                        ? _nodeManagementService.GetDefaultApiNodeEndpoint() ?? string.Empty
                        : _useAPIEndPoint;

                    latestVersion = await _versionService.GetLatestVersionFromOfficialAsync(apiEndpoint);
                }

                if (latestVersion <= 0 || latestVersion == -100)
                {
                    _dialogService.ShowError("无法获取最新版本，安装已终止。");
                    return;
                }

                bool needMod = _modInstallService.NeedsModInstallation(_limbusCompanyDir, currentVersion, latestVersion);
                if (needMod)
                {
                UpdateInstallProgress(60);
                bool success = await _modInstallService.InstallModAsync(
                    _limbusCompanyDir,
                    _isMirrorChyanMode,
                    _mirrorChyanToken,
                    _useGithub,
                    _useAPIEndPoint,
                    _useEndPoint,
                    _nodeManagementService.GetDefaultDownloadNodeEndpoint(),
                    progress => UpdateInstallProgress(60 + progress * 0.4f));

                    if (!success)
                    {
                        _dialogService.ShowError("模组安装失败");
                        return;
                    }
                }

                UpdateInstallProgress(100);

                Log.logger.Info("安装完成，准备显示确认对话框。");

                // 根据是否实际安装了内容显示不同的提示（使用确认对话框，与旧项目一致）
                bool runResult;
                if (needMod || needFont)
                {
                    IsNewestModVersion = false;
                    runResult = _dialogService.ShowConfirm("安装已完成！\n点击『是』立刻运行边狱公司。\n点击『否』关闭弹窗。\n加载时请耐心等待。", "提示");
                }
                else
                {
                    IsNewestModVersion = true;
                    runResult = _dialogService.ShowConfirm("没有检测到新版本模组！\n您的模组已经为最新。\n点击『是』立刻运行边狱公司。\n点击『否』关闭弹窗。\n加载时请耐心等待。", "提示");
                }

                Log.logger.Info($"用户选择结果：{runResult}");

                // 如果用户点击『是』，启动游戏（非阻塞方式）
                if (runResult)
                {
                    try
                    {
                        ExecuteOpenUrl("LaunchGame");
                    }
                    catch (Exception ex)
                    {
                        Log.logger.Error("启动游戏失败", ex);
                        _dialogService.ShowError($"启动游戏失败：{ex.Message}");
                    }
                }

                Log.logger.Info("安装流程完成。");
            }
            catch (Models.MirrorChyanException ex)
            {
                Log.logger.Error("Mirror酱错误", ex);
                _dialogService.ShowError($"访问 Mirror 酱服务出现了问题。\n{ex.Message}\n若要反馈，请带上链接或日志。");
            }
            catch (Exception ex)
            {
                Log.logger.Error("安装失败", ex);
                _dialogService.ShowError($"安装失败：{ex.Message}");
            }
            finally
            {
                IsInstalling = false;
                UpdateInstallProgress(0);
            }
        }

        private void UpdateInstallProgress(float value)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null || dispatcher.CheckAccess())
            {
                InstallProgress = value;
                return;
            }

            dispatcher.BeginInvoke(() =>
            {
                InstallProgress = value;
            });
        }

        private bool CanExecuteInstall()
        {
            return !IsInstalling;
        }

        /// <summary>
        /// 执行卸载命令
        /// </summary>
        private async System.Threading.Tasks.Task ExecuteUninstallAsync()
        {
            try
            {
                var confirm = _dialogService.ShowConfirm(
                    "删除后你需要重新安装汉化补丁。\n确定继续吗？",
                    "警告");

                if (!confirm)
                    return;

                IsInstalling = true;

                await _modUninstallService.UninstallAllAsync(_limbusCompanyDir);

                _dialogService.ShowMessage("删除完成。", "提示");

                // 刷新版本信息
                CurrentVersionText = "当前版本：未安装";
            }
            catch (Exception ex)
            {
                Log.logger.Error("卸载失败", ex);
                _dialogService.ShowError($"卸载失败：{ex.Message}");
            }
            finally
            {
                IsInstalling = false;
            }
        }

        private bool CanExecuteUninstall()
        {
            return !IsInstalling;
        }

        /// <summary>
        /// 执行刷新版本命令
        /// </summary>
        private async System.Threading.Tasks.Task ExecuteRefreshVersionAsync()
        {
            try
            {
                Log.logger.Info("开始刷新版本信息");
                CurrentVersionText = "当前版本：检查中...";
                LatestVersionText = "最新版本：检查中...";

                if (string.IsNullOrWhiteSpace(_limbusCompanyDir))
                {
                    Log.logger.Warn("游戏路径为空，无法检查版本");
                    CurrentVersionText = "当前版本：未找到游戏";
                    LatestVersionText = "最新版本：未知";
                    return;
                }

                // 从配置重新读取Mirror酱状态，确保使用最新配置
                _isMirrorChyanMode = _configService.AppSettings.mirrorChyan.enable;
                if (_isMirrorChyanMode)
                {
                    try
                    {
                        _mirrorChyanToken = SecureStringStorage.LoadToken();
                    }
                    catch
                    {
                        _mirrorChyanToken = string.Empty;
                    }
                }

                // 检查当前版本
                Log.logger.Info("检查当前已安装的模组版本");
                int currentVersion = await _modInstallService.GetCurrentModVersionAsync(_limbusCompanyDir);
                CurrentVersionText = currentVersion == 0
                    ? "当前版本：未安装"
                    : $"当前版本：{currentVersion}";
                Log.logger.Info($"当前版本：{currentVersion}，显示文本：{CurrentVersionText}");

                // 检查最新版本
                Log.logger.Info("检查最新的模组版本");
                var versionResult = await _versionService.CheckForUpdateAsync(
                    _limbusCompanyDir,
                    _isMirrorChyanMode,
                    _mirrorChyanToken,
                    _useAPIEndPoint);

                if (!versionResult.IsSuccess)
                {
                    // 显示错误信息
                    LatestVersionText = versionResult.LatestVersionText;
                    Log.logger.Warn($"版本检查失败：{versionResult.ErrorMessage}");

                    // 如果是Mirror酱密钥无效，显示错误对话框
                    if (!string.IsNullOrEmpty(versionResult.ErrorMessage) &&
                        versionResult.ErrorMessage.Contains("秘钥无效"))
                    {
                        _dialogService.ShowError(versionResult.ErrorMessage);
                    }

                    Log.logger.Info("版本信息刷新完成");
                    return;
                }

                if (versionResult.NeedUpdate)
                {
                    LatestVersionText = $"最新版本：{versionResult.LatestVersion} (有更新)";
                    IsNewestModVersion = false;
                    Log.logger.Info($"有新版本：{versionResult.LatestVersion}");
                }
                else
                {
                    LatestVersionText = $"最新版本：{versionResult.LatestVersion}";
                    IsNewestModVersion = true;
                    Log.logger.Info($"已是最新版本：{versionResult.LatestVersion}");
                }

                Log.logger.Info("版本信息刷新完成");
            }
            catch (Exception ex)
            {
                Log.logger.Error("检查版本失败", ex);
                CurrentVersionText = "当前版本：检查失败";
                LatestVersionText = "最新版本：检查失败";
            }
        }

        /// <summary>
        /// 执行打开游戏命令
        /// </summary>
        private void ExecuteOpenGame()
        {
            try
            {
                System.Diagnostics.Process.Start("steam://rungameid/1973530");
            }
            catch (Exception ex)
            {
                Log.logger.Error("启动游戏失败", ex);
                _dialogService.ShowError($"启动游戏失败：{ex.Message}");
            }
        }

        private bool CanExecuteOpenGame()
        {
            return !IsGameRunning;
        }

        /// <summary>
        /// 初始化节点列表
        /// </summary>
        private void ExecuteInitializeNodes()
        {
            try
            {
                string nodeListJsonPath = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Config/NodeList.json");

                bool internationalMode = _configService.AppSettings.general.internationalMode;

                _nodeManagementService.InitializeNodes(nodeListJsonPath, internationalMode);

                // 获取节点列表
                NodeItemList = _nodeManagementService.GetDownloadNodes();
                ApiItemList = _nodeManagementService.GetApiNodes();

                // 设置默认端点
                UseEndPoint = _nodeManagementService.GetDefaultDownloadNodeEndpoint() ?? string.Empty;
                UseAPIEndPoint = _nodeManagementService.GetDefaultApiNodeEndpoint() ?? string.Empty;

                Log.logger.Info($"节点初始化完成：下载节点{NodeItemList.Count}个，API节点{ApiItemList.Count}个");
            }
            catch (Exception ex)
            {
                Log.logger.Error("初始化节点失败", ex);
                _dialogService.ShowError($"初始化节点失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 执行最小化窗口命令
        /// </summary>
        private void ExecuteMinimize()
        {
            if (_ownerWindow != null)
            {
                _ownerWindow.WindowState = WindowState.Minimized;
            }
        }

        private bool CanExecuteMinimize()
        {
            return _ownerWindow != null;
        }

        /// <summary>
        /// 执行关闭窗口命令
        /// </summary>
        private void ExecuteClose()
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// 执行发送启动器到桌面命令
        /// </summary>
        private void ExecuteSendLauncher()
        {
            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string shortcutPath = System.IO.Path.Combine(desktopPath, "都市零协会工具箱.lnk");

                // 创建快捷方式
                var shellType = Type.GetTypeFromProgID("WScript.Shell");
                dynamic? shell = Activator.CreateInstance(shellType!);
                var shortcut = shell?.CreateShortcut(shortcutPath);

                string exePath = Environment.ProcessPath ?? System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? "";
                shortcut.TargetPath = exePath;
                shortcut.WorkingDirectory = System.IO.Path.GetDirectoryName(exePath);
                shortcut.Description = "都市零协会工具箱";
                shortcut.Save();

                _dialogService.ShowMessage("已发送启动器到桌面！", "提示");
                Log.logger.Info("已发送启动器到桌面");
            }
            catch (Exception ex)
            {
                Log.logger.Error("发送启动器失败", ex);
                _dialogService.ShowError($"发送启动器失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 执行发送帮助启动器到桌面命令
        /// </summary>
        private void ExecuteSendHelpLauncher()
        {
            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string shortcutPath = System.IO.Path.Combine(desktopPath, "都市零协会工具箱-帮助.lnk");

                // 创建快捷方式（带 -launcher 参数）
                var shellType = Type.GetTypeFromProgID("WScript.Shell");
                dynamic? shell = Activator.CreateInstance(shellType!);
                var shortcut = shell?.CreateShortcut(shortcutPath);

                string exePath = Environment.ProcessPath ?? System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? "";
                shortcut.TargetPath = exePath;
                shortcut.Arguments = "-launcher";
                shortcut.WorkingDirectory = System.IO.Path.GetDirectoryName(exePath);
                shortcut.Description = "都市零协会工具箱 - 启动器模式";
                shortcut.Save();

                _dialogService.ShowMessage("已发送帮助启动器到桌面！", "提示");
                Log.logger.Info("已发送帮助启动器到桌面");
            }
            catch (Exception ex)
            {
                Log.logger.Error("发送帮助启动器失败", ex);
                _dialogService.ShowError($"发送帮助启动器失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 执行打开URL命令
        /// </summary>
        private void ExecuteOpenUrl(string buttonName)
        {
            string? url = _linkService.GetUrl(buttonName);
            if (!string.IsNullOrWhiteSpace(url))
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                    Log.logger.Info($"打开链接：{url}");
                }
                catch (Exception ex)
                {
                    Log.logger.Error($"打开链接失败：{url}", ex);
                    _dialogService.ShowError($"无法打开链接：{ex.Message}");
                }
            }
            else if (!string.IsNullOrWhiteSpace(buttonName))
            {
                // 如果是直接传入的URL（用于帮助按钮等）
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = buttonName,
                        UseShellExecute = true
                    });
                    Log.logger.Info($"打开链接：{buttonName}");
                }
                catch (Exception ex)
                {
                    Log.logger.Error($"打开链接失败：{buttonName}", ex);
                    _dialogService.ShowError($"无法打开链接：{ex.Message}");
                }
            }
        }

        /// <summary>
        /// 执行导航到抽卡模拟器页面
        /// </summary>
        private async System.Threading.Tasks.Task ExecuteNavigateToGachaSimAsync()
        {
            // 首次进入抽卡模拟器时显示确认对话框
            if (!_isGachaInitialized)
            {
                bool confirmed = _dialogService.ShowConfirm(
                    "本抽卡模拟器资源来源自维基，可能信息更新不准时。\n本模拟器 不 会 对您的游戏数据造成任何影响。\n若您已知悉，请点击【是】进行初始化。",
                    "提示");

                if (!confirmed)
                {
                    return;
                }

                // 导航到抽卡页面
                await _navigationService.NavigateToInstallPageAsync("gacha");

                // 显示初始化完成提示
                _dialogService.ShowMessage("初始化完成。", "提示");
                _isGachaInitialized = true;
            }
            else
            {
                // 已初始化，直接导航
                await _navigationService.NavigateToInstallPageAsync("gacha");
            }
        }

        /// <summary>
        /// 执行显示热更新帮助
        /// </summary>
        private void ExecuteShowHotUpdateHelp()
        {
            try
            {
                var instructions = _launcherService.GetHotUpdateInstructions()
                    ?? "无法获取热更新使用说明。";
                _dialogService.ShowMessage(instructions, "热更新教程");
            }
            catch (Exception ex)
            {
                Log.logger.Error("获取热更新帮助失败", ex);
                _dialogService.ShowError($"无法获取热更新帮助：{ex.Message}");
            }
        }

        /// <summary>
        /// 执行触发彩蛋
        /// </summary>
        private void ExecuteTriggerEasterEgg()
        {
            if (!_eeOpening && !_eeEntered && !_isInAnno)
            {
                Log.logger.Info("不要点了>_<");
                _eeOpening = true;
                _eeEntered = false;
                _ = ShowEasterEggButtonAsync();
            }
        }

        private bool CanExecuteTriggerEasterEgg()
        {
            return !_eeOpening && !_eeEntered && !_isInAnno;
        }

        /// <summary>
        /// 显示彩蛋按钮
        /// </summary>
        private async Task ShowEasterEggButtonAsync()
        {
            if (_ownerWindow != null)
            {
                await _ownerWindow.Dispatcher.BeginInvoke(() =>
                {
                    // TODO: 需要从NavigationService获取EEOption控件
                    // 当前先记录日志
                    Log.logger.Info("彩蛋按钮应该显示");
                });
            }
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 异步初始化
        /// </summary>
        private async System.Threading.Tasks.Task InitializeAsync()
        {
            try
            {
                // 加载Mirror酱配置
                _isMirrorChyanMode = _configService.AppSettings.mirrorChyan.enable;

                // 加载Mirror酱 Token
                if (_isMirrorChyanMode)
                {
                    try
                    {
                        _mirrorChyanToken = SecureStringStorage.LoadToken();
                    }
                    catch
                    {
                        _mirrorChyanToken = string.Empty;
                    }
                }

                // 检查游戏运行状态
                await CheckGameStatusAsync();

                // 加载路径配置
                var configPath = _configService.AppSettings.general.LCBPath;
                if (!string.IsNullOrWhiteSpace(configPath) && _pathService.ValidateGamePath(configPath))
                {
                    _pathService.GameDirectory = configPath;
                    LimbusCompanyDir = configPath;
                }

                // 初始化节点
                ExecuteInitializeNodes();

                // 检查版本
                await ExecuteRefreshVersionAsync();

                // 定期检查游戏状态
                var timer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(5)
                };
                timer.Tick += async (s, e) => await CheckGameStatusAsync();
                timer.Start();
            }
            catch (Exception ex)
            {
                Log.logger.Error("初始化失败", ex);
                _dialogService.ShowError($"初始化失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 检查游戏状态
        /// </summary>
        private async System.Threading.Tasks.Task CheckGameStatusAsync()
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                var running = _pathService.IsGameRunning();
                if (IsGameRunning != running)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        IsGameRunning = running;
                    });
                }
            });
        }

        #endregion
    }
}
