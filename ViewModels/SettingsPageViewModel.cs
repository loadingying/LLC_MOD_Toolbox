using LLC_MOD_Toolbox.Interfaces;
using LLC_MOD_Toolbox.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Diagnostics;
using System.IO;

namespace LLC_MOD_Toolbox.ViewModels
{
    /// <summary>
    /// 设置页面ViewModel
    /// 处理节点切换、模组卸载、Mirror酱配置等功能
    /// </summary>
    public class SettingsPageViewModel : ViewModelBase
    {
        private readonly IConfigService _configService;
        private readonly IPathService _pathService;
        private readonly IInstallService _installService;
        private readonly IDialogService _dialogService;
        private readonly ILauncherService _launcherService;

        private ObservableCollection<string> _nodeOptions = new();
        private ObservableCollection<string> _apiNodeOptions = new();
        private string _selectedNode = string.Empty;
        private string _selectedApiNode = string.Empty;
        private bool _isMirrorChyanEnabled;
        private string _mirrorChyanToken = string.Empty;
        private bool _isUninstalling;

        // 节点列表（用于endpoint解析）
        private static List<Node> nodeList = new List<Node>();
        private static List<Node> apiList = new List<Node>();

        // Github模式和Mirror酱模式状态
        private static bool useGithub = false;
        private static bool isMirrorChyanMode = false;
        private static bool APPChangeAPIUI = false;

        /// <summary>
        /// 初始化SettingsPageViewModel
        /// </summary>
        public SettingsPageViewModel(
            IConfigService configService,
            IPathService pathService,
            IInstallService installService,
            IDialogService dialogService,
            ILauncherService launcherService)
        {
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
            _pathService = pathService ?? throw new ArgumentNullException(nameof(pathService));
            _installService = installService ?? throw new ArgumentNullException(nameof(installService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _launcherService = launcherService ?? throw new ArgumentNullException(nameof(launcherService));

            // 初始化命令
            UninstallCommand = new AsyncRelayCommand(
                ExecuteUninstallAsync,
                CanExecuteUninstall);

            SendLauncherCommand = new RelayCommand(
                ExecuteSendLauncher);

            SendHelpLauncherCommand = new RelayCommand(
                ExecuteSendHelpLauncher);

            ConfigureMirrorChyanCommand = new RelayCommand(
                ExecuteConfigureMirrorChyan);

            RefreshNodesCommand = new AsyncRelayCommand(
                ExecuteRefreshNodesAsync);

            OpenUrlCommand = new RelayCommand<string>(
                ExecuteOpenUrl);

            ShowHotUpdateHelpCommand = new RelayCommand(
                ExecuteShowHotUpdateHelp);

            // 初始化数据
            _nodeOptions = new ObservableCollection<string>();
            _apiNodeOptions = new ObservableCollection<string>();
            _ = InitializeAsync();
        }

        #region 属性

        /// <summary>
        /// 节点选项列表
        /// </summary>
        public ObservableCollection<string> NodeOptions
        {
            get => _nodeOptions;
            set => SetProperty(ref _nodeOptions, value);
        }

        /// <summary>
        /// API节点选项列表
        /// </summary>
        public ObservableCollection<string> ApiNodeOptions
        {
            get => _apiNodeOptions;
            set => SetProperty(ref _apiNodeOptions, value);
        }

        /// <summary>
        /// 选中的节点
        /// </summary>
        public string SelectedNode
        {
            get => _selectedNode;
            set
            {
                if (SetProperty(ref _selectedNode, value))
                {
                    _ = OnNodeChangedAsync(value);
                }
            }
        }

        /// <summary>
        /// 选中的API节点
        /// </summary>
        public string SelectedApiNode
        {
            get => _selectedApiNode;
            set
            {
                if (SetProperty(ref _selectedApiNode, value))
                {
                    _ = OnApiNodeChangedAsync(value);
                }
            }
        }

        /// <summary>
        /// 是否启用Mirror酱
        /// </summary>
        public bool IsMirrorChyanEnabled
        {
            get => _isMirrorChyanEnabled;
            set => SetProperty(ref _isMirrorChyanEnabled, value);
        }

        /// <summary>
        /// Mirror酱 Token
        /// </summary>
        public string MirrorChyanToken
        {
            get => _mirrorChyanToken;
            set => SetProperty(ref _mirrorChyanToken, value);
        }

        /// <summary>
        /// 是否正在卸载
        /// </summary>
        public bool IsUninstalling
        {
            get => _isUninstalling;
            set => SetProperty(ref _isUninstalling, value);
        }

        /// <summary>
        /// 节点ComboBox是否启用（Mirror酱模式下禁用）
        /// </summary>
        public bool AreNodeComboboxesEnabled => !isMirrorChyanMode;

        #endregion

        #region 命令

        /// <summary>
        /// 卸载命令
        /// </summary>
        public AsyncRelayCommand UninstallCommand { get; }

        /// <summary>
        /// 发送启动器命令
        /// </summary>
        public RelayCommand SendLauncherCommand { get; }

        /// <summary>
        /// 发送帮助启动器命令
        /// </summary>
        public RelayCommand SendHelpLauncherCommand { get; }

        /// <summary>
        /// 配置Mirror酱命令
        /// </summary>
        public RelayCommand ConfigureMirrorChyanCommand { get; }

        /// <summary>
        /// 刷新节点命令
        /// </summary>
        public AsyncRelayCommand RefreshNodesCommand { get; }

        /// <summary>
        /// 打开URL命令
        /// </summary>
        public RelayCommand<string> OpenUrlCommand { get; }

        /// <summary>
        /// 显示热更新帮助命令
        /// </summary>
        public RelayCommand ShowHotUpdateHelpCommand { get; }

        #endregion

        #region 命令实现

        private async System.Threading.Tasks.Task ExecuteUninstallAsync()
        {
            try
            {
                var confirm = _dialogService.ShowConfirm(
                    "删除后你需要重新安装汉化补丁。\n确定继续吗？",
                    "警告");

                if (!confirm)
                    return;

                IsUninstalling = true;

                await _installService.UninstallAsync();

                _dialogService.ShowMessage("删除完成。", "提示");
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"卸载失败：{ex.Message}");
            }
            finally
            {
                IsUninstalling = false;
            }
        }

        private bool CanExecuteUninstall()
        {
            return !IsUninstalling;
        }

        private void ExecuteSendLauncher()
        {
            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string shortcutPath = System.IO.Path.Combine(desktopPath, "LimbusCompany with LLC.lnk");

                // 使用WshShell创建快捷方式（与旧项目保持一致）
                var shellType = Type.GetTypeFromProgID("WScript.Shell");
                dynamic? shell = Activator.CreateInstance(shellType);
                var shortcut = shell?.CreateShortcut(shortcutPath);

                string currentDir = AppDomain.CurrentDomain.BaseDirectory;
                string exePath = System.IO.Path.Combine(currentDir, "LLC_MOD_Toolbox.exe");

                if (shortcut != null)
                {
                    shortcut.TargetPath = exePath;
                    shortcut.Arguments = "-launcher";
                    shortcut.WorkingDirectory = currentDir;
                    shortcut.Description = "启动边狱公司并检查汉化更新";
                    shortcut.IconLocation = System.IO.Path.Combine(currentDir, "PublicResource", "favicon.ico");
                    shortcut.Save();
                    _dialogService.ShowMessage("快捷方式已创建。\n可在桌面上找到\"LimbusCompany with LLC\"启动。", "提示");
                }
                else
                {
                    _dialogService.ShowError("创建快捷方式失败。", "错误");
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"发送启动器失败：{ex.Message}");
            }
        }

        private void ExecuteSendHelpLauncher()
        {
            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string shortcutPath = System.IO.Path.Combine(desktopPath, "LimbusCompany with LLC-帮助.lnk");

                // 创建快捷方式（与旧项目保持一致）
                var shellType = Type.GetTypeFromProgID("WScript.Shell");
                dynamic? shell = Activator.CreateInstance(shellType);
                var shortcut = shell?.CreateShortcut(shortcutPath);

                string currentDir = AppDomain.CurrentDomain.BaseDirectory;
                string exePath = System.IO.Path.Combine(currentDir, "LLC_MOD_Toolbox.exe");

                if (shortcut != null)
                {
                    shortcut.TargetPath = exePath;
                    shortcut.Arguments = "-launcher";
                    shortcut.WorkingDirectory = currentDir;
                    shortcut.Description = "LimbusCompany with LLC - 启动器模式";
                    shortcut.IconLocation = System.IO.Path.Combine(currentDir, "PublicResource", "favicon.ico");
                    shortcut.Save();
                    _dialogService.ShowMessage("快捷方式已创建。\n可在桌面上找到\"LimbusCompany with LLC-帮助\"启动。", "提示");
                }
                else
                {
                    _dialogService.ShowError("创建快捷方式失败。", "错误");
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"发送帮助启动器失败：{ex.Message}");
            }
        }

        private void ExecuteConfigureMirrorChyan()
        {
            try
            {
                if (isMirrorChyanMode)
                {
                    // Mirror酱已启用，确认是否禁用
                    bool result = _dialogService.ShowConfirm(
                        "确定要禁用Mirror酱吗？\n关闭后，你可以在设置重新开启Mirror酱的服务。",
                        "提示");

                    if (!result)
                    {
                        return;
                    }

                    // 删除密钥文件并禁用
                    SecureStringStorage.DeleteSecretFile();
                    isMirrorChyanMode = false;
                    _configService.UpdateAndSave(settings =>
                    {
                        settings.mirrorChyan.enable = false;
                    });

                    _dialogService.ShowMessage(
                        "已禁用Mirror酱并删除你的Mirror酱CDK。\n为了处理，软件将关闭，再次启动后效果生效。",
                        "提示");

                    Application.Current.Shutdown();
                }
                else
                {
                    // Mirror酱未启用，提示输入CDK
                    var buttons = new List<DialogButton>
                    {
                        new DialogButton("确定", true, false),
                        new DialogButton("取消", false, true)
                    };

                    var result = UniversalDialog.ShowInput(
                        "请输入你的 Mirror 酱 CDK。\n你可以在 Mirror 酱官网购买。",
                        "输入秘钥",
                        "Mirror 酱 CDK",
                        InputType.Password,
                        buttons,
                        Application.Current.MainWindow);

                    if (result.IsCanceled)
                    {
                        return;
                    }

                    if (result.IsSuccess && !string.IsNullOrEmpty(result.Input))
                    {
                        SetupMirrorChyanMode(result.Input);
                        _dialogService.ShowMessage(
                            "Mirror酱秘钥设置成功。\n为了处理，软件将关闭，再次启动后效果生效。",
                            "提示");
                        Application.Current.Shutdown();
                    }
                    else
                    {
                        _dialogService.ShowMessage("设置失败。", "提示");
                    }
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"配置Mirror酱失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 设置Mirror酱模式
        /// </summary>
        private void SetupMirrorChyanMode(string token)
        {
            Log.logger.Info("设置Mirror酱模式。");
            isMirrorChyanMode = true;
            MirrorChyanToken = token.Trim();
            SecureStringStorage.SaveToken(MirrorChyanToken);
            _configService.UpdateAndSave(settings =>
            {
                settings.mirrorChyan.enable = true;
            });
        }

        /// <summary>
        /// 公共静态方法：设置Mirror酱模式状态（由MainWindowWD调用）
        /// </summary>
        public static void SetMirrorChyanMode(bool enabled)
        {
            bool wasEnabled = isMirrorChyanMode;
            isMirrorChyanMode = enabled;

            // 如果状态发生变化，需要通知UI更新
            // 但由于这是静态方法，无法直接访问实例属性
            // 所以需要在导航到设置页面时刷新UI
        }

        /// <summary>
        /// 刷新Mirror酱UI状态（导航到设置页面时调用）
        /// </summary>
        public async System.Threading.Tasks.Task RefreshMirrorChyanUIAsync()
        {
            try
            {
                // 如果是Mirror酱模式，更新节点选项
                if (isMirrorChyanMode)
                {
                    // 清空原有选项
                    NodeOptions.Clear();
                    ApiNodeOptions.Clear();

                    // 添加Mirror酱提示
                    NodeOptions.Add("已使用Mirror酱");
                    ApiNodeOptions.Add("已使用Mirror酱");

                    // 设置选中项
                    _selectedNode = "已使用Mirror酱";
                    _selectedApiNode = "已使用Mirror酱";

                    // 通知UI更新
                    OnPropertyChanged(nameof(SelectedNode));
                    OnPropertyChanged(nameof(SelectedApiNode));
                    OnPropertyChanged(nameof(AreNodeComboboxesEnabled));
                }
                else
                {
                    // 重新加载节点选项
                    await LoadNodeOptionsAsync();

                    // 设置当前选中的节点
                    _selectedNode = _configService.AppSettings.nodeSelect.defaultNode;
                    _selectedApiNode = _configService.AppSettings.nodeSelect.defaultApiNode;

                    // 通知UI更新
                    OnPropertyChanged(nameof(SelectedNode));
                    OnPropertyChanged(nameof(SelectedApiNode));
                    OnPropertyChanged(nameof(AreNodeComboboxesEnabled));
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"刷新Mirror酱UI失败：{ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task ExecuteRefreshNodesAsync()
        {
            await LoadNodeOptionsAsync();
        }

        /// <summary>
        /// 执行打开URL命令
        /// </summary>
        private void ExecuteOpenUrl(string url)
        {
            if (!string.IsNullOrWhiteSpace(url))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    Log.logger.Error($"打开URL失败：{url}", ex);
                    _dialogService.ShowError($"无法打开链接：{ex.Message}");
                }
            }
        }

        /// <summary>
        /// 执行显示热更新帮助命令
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
                _dialogService.ShowError($"获取帮助失败：{ex.Message}");
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 根据节点名称查找endpoint
        /// </summary>
        private static string FindNodeEndpoint(string name)
        {
            foreach (var node in nodeList)
            {
                if (node.Name == name)
                {
                    return node.Endpoint;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 根据API节点名称查找endpoint
        /// </summary>
        private static string FindAPIEndpoint(string name)
        {
            foreach (var api in apiList)
            {
                if (api.Name == name)
                {
                    return api.Endpoint;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 设置下载节点配置
        /// </summary>
        private void SetDownloadNodeConfig(string node)
        {
            _configService.AppSettings.nodeSelect.defaultNode = node;
            _configService.Save();
        }

        /// <summary>
        /// 设置API节点配置
        /// </summary>
        private void SetApiNodeConfig(string api)
        {
            _configService.AppSettings.nodeSelect.defaultApiNode = api;
            _configService.Save();
        }

        private async System.Threading.Tasks.Task InitializeAsync()
        {
            try
            {
                // 加载Mirror酱配置
                IsMirrorChyanEnabled = _configService.AppSettings.mirrorChyan.enable;

                // 重要：从配置加载 isMirrorChyanMode 状态
                isMirrorChyanMode = _configService.AppSettings.mirrorChyan.enable;

                // 加载节点选项
                await LoadNodeOptionsAsync();

                // 如果是Mirror酱模式，修改节点选项
                if (isMirrorChyanMode)
                {
                    // 清空原有选项
                    NodeOptions.Clear();
                    ApiNodeOptions.Clear();

                    // 添加Mirror酱提示
                    NodeOptions.Add("已使用Mirror酱");
                    ApiNodeOptions.Add("已使用Mirror酱");

                    // 设置选中项
                    _selectedNode = "已使用Mirror酱";
                    _selectedApiNode = "已使用Mirror酱";
                }
                else
                {
                    // 设置当前选中的节点（不触发切换事件）
                    // 直接设置私有字段，避免触发属性变更事件
                    _selectedNode = _configService.AppSettings.nodeSelect.defaultNode;
                    _selectedApiNode = _configService.AppSettings.nodeSelect.defaultApiNode;
                }

                // 通知UI更新（不触发切换逻辑）
                OnPropertyChanged(nameof(SelectedNode));
                OnPropertyChanged(nameof(SelectedApiNode));
                OnPropertyChanged(nameof(AreNodeComboboxesEnabled));
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"初始化设置页面失败：{ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task LoadNodeOptionsAsync()
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    // 从NodeList.json加载节点列表
                    var nodeListPath = System.IO.Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "Config/NodeList.json");

                    if (!System.IO.File.Exists(nodeListPath))
                        return;

                    var json = System.IO.File.ReadAllText(nodeListPath);
                    var root = Newtonsoft.Json.JsonConvert.DeserializeObject<RootModel>(json);

                    if (root != null)
                    {
                        // 加载到静态列表（用于endpoint解析）
                        nodeList = root.DownloadNode ?? new List<Node>();
                        apiList = root.ApiNode ?? new List<Node>();

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            NodeOptions.Clear();
                            ApiNodeOptions.Clear();

                            NodeOptions.Add("恢复默认");
                            foreach (var node in nodeList)
                            {
                                NodeOptions.Add(node.Name);
                            }
                            NodeOptions.Add("Github直连");

                            ApiNodeOptions.Add("恢复默认");
                            foreach (var api in apiList)
                            {
                                ApiNodeOptions.Add(api.Name);
                            }

                            // 加载默认endpoint
                            if (!string.IsNullOrEmpty(_configService.AppSettings.nodeSelect.defaultNode))
                            {
                                if (_configService.AppSettings.nodeSelect.defaultNode == "Github直连")
                                {
                                    useGithub = true;
                                }
                                else
                                {
                                    useGithub = false;
                                }
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _dialogService.ShowError($"加载节点列表失败：{ex.Message}");
                    });
                }
            });
        }

        private async System.Threading.Tasks.Task OnNodeChangedAsync(string? newNode)
        {
            if (string.IsNullOrWhiteSpace(newNode))
                return;

            await System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    // Mirror酱模式下禁用节点切换
                    if (isMirrorChyanMode)
                        return;

                    if (newNode == "恢复默认")
                    {
                        // 恢复默认节点
                        useGithub = false;
                        SetDownloadNodeConfig("");
                    }
                    else if (newNode == "Github直连")
                    {
                        // Github直连模式
                        if (!IsLaunching)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                _dialogService.ShowMessage(
                                    "如果您没有使用代理软件（包括Watt Toolkit）\n请不要使用此节点。\n" +
                                    "Github由于不可抗力因素，对国内网络十分不友好。\n" +
                                    "如果您是国外用户，才应该使用此选项。",
                                    "警告");
                            });
                        }
                        SetDownloadNodeConfig("Github直连");
                        useGithub = true;
                    }
                    else
                    {
                        // 普通节点
                        var endpoint = FindNodeEndpoint(newNode);
                        useGithub = false;
                        SetDownloadNodeConfig(newNode);

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            _dialogService.ShowMessage("切换成功。", "提示");
                        });
                    }
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _dialogService.ShowError($"切换节点失败：{ex.Message}");
                    });
                }
            });
        }

        /// <summary>
        /// 是否正在启动（用于控制Github警告是否显示）
        /// </summary>
        private bool IsLaunching { get; set; } = false;

        /// <summary>
        /// 设置是否正在启动状态
        /// </summary>
        public void SetLaunchingState(bool launching)
        {
            IsLaunching = launching;
        }

        /// <summary>
        /// 获取Mirror酱模式状态
        /// </summary>
        public bool GetMirrorChyanMode()
        {
            return isMirrorChyanMode;
        }

        /// <summary>
        /// 设置Github模式状态
        /// </summary>
        public void SetGithubMode(bool enabled)
        {
            useGithub = enabled;
        }

        /// <summary>
        /// 获取Github模式状态
        /// </summary>
        public bool GetGithubMode()
        {
            return useGithub;
        }

        /// <summary>
        /// 设置API节点（用于Github模式下自动切换API）
        /// </summary>
        public void SetApiNodeProgrammatically(string nodeName)
        {
            APPChangeAPIUI = true;
            SelectedApiNode = nodeName;
        }

        private async System.Threading.Tasks.Task OnApiNodeChangedAsync(string? newNode)
        {
            if (string.IsNullOrWhiteSpace(newNode))
                return;

            Log.logger.Info($"API节点切换开始：{newNode}");

            await System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    // Mirror酱模式下禁用API节点切换
                    if (isMirrorChyanMode)
                    {
                        Log.logger.Info("Mirror酱模式下禁用API节点切换");
                        return;
                    }

                    // Github模式下禁止切换API
                    if (!useGithub)
                    {
                        if (newNode == "恢复默认")
                        {
                            // 恢复默认API节点
                            Log.logger.Info("恢复默认API节点");
                            SetApiNodeConfig("");
                        }
                        else
                        {
                            // 普通API节点
                            var endpoint = FindAPIEndpoint(newNode);
                            Log.logger.Info($"切换API节点：{newNode}，Endpoint：{endpoint}");
                            SetApiNodeConfig(newNode);

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                _dialogService.ShowMessage("切换成功。", "提示");
                            });
                        }
                    }
                    else if (APPChangeAPIUI == false)
                    {
                        // Github模式下强制恢复默认
                        Log.logger.Info("Github模式下禁止切换API，强制恢复默认");
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            SelectedApiNode = "恢复默认";
                            _dialogService.ShowMessage(
                                "切换失败。\n无法在节点为Github直连的情况下切换API。",
                                "提示");
                        });
                    }
                    APPChangeAPIUI = false;
                    Log.logger.Info("API节点切换完成");
                }
                catch (Exception ex)
                {
                    Log.logger.Error("切换API节点失败", ex);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _dialogService.ShowError($"切换API节点失败：{ex.Message}");
                    });
                }
            });
        }

        #endregion
    }
}
