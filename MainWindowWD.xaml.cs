
// 用于处理后端逻辑。
/*
 * You may think you know what the following code does.
 * But you dont. Trust me.
 * Fiddle with it, and youll spend many a sleepless
 * night cursing the moment you thought youd be clever
 * enough to "optimize" the code below.
 * Now close this file and go play with something else.
 * 你可能会认为你读得懂以下的代码。但是你不会懂的，相信我吧。
 * 要是你尝试玩弄这段代码的话，你将会在无尽的通宵中不断地咒骂自己为什么会认为自己聪明到可以优化这段代码。
 * 现在请关闭这个文件去玩点别的吧。
*/
using Downloader;
using LLC_MOD_Toolbox.Interfaces;
using LLC_MOD_Toolbox.Models;
using LLC_MOD_Toolbox.Services;
using LLC_MOD_Toolbox.ViewModels;
using log4net.Config;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using SevenZip;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using static LLC_MOD_Toolbox.SimpleDnsChecker;

namespace LLC_MOD_Toolbox
{
    public partial class MainWindow : Window
    {
        private static string? useEndPoint;
        private static string? useAPIEndPoint;
        private static string limbusCompanyDir = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 1973530", "InstallLocation", null) as string
            ?? string.Empty;
        private static string limbusCompanyGameDir = Path.Combine(limbusCompanyDir, "LimbusCompany.exe");
        private static readonly string currentDir = AppDomain.CurrentDomain.BaseDirectory;
        private static List<Node> nodeList = [];
        private static List<Node> apiList = [];
        private static int installPhase = 0;
        private readonly DispatcherTimer progressTimer;
        private float progressPercentage = 0;
        private bool isNewestModVersion = true;
        // 注：灰度测试状态已迁移到IGreytestService管理
        private readonly string VERSION = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";
        // 配置
        private static ConfigurationManager configuation = ConfigurationManager.GetInstance(Path.Combine(currentDir, "Config", "config.json"));
        // 启动器模式
        private static bool isLauncherMode = Environment.GetCommandLineArgs().Contains("-launcher");
        // 注：MirrorChyan状态已迁移到IMirrorChyanService管理

        internal bool isLaunching = false;

        // MVVM ViewModels
        private ViewModels.SettingsPageViewModel? _settingsViewModel;
        private ViewModels.GachaSimulatorPageViewModel? _gachaSimulatorViewModel;
        private ViewModels.FontReplacePageViewModel? _fontReplaceViewModel;
        private ViewModels.AnnouncementPageViewModel? _announcementViewModel;
        private ViewModels.GreytestPageViewModel? _greytestViewModel;

        // Services
        private Interfaces.IErrorService? _errorService;
        private Interfaces.ILauncherService? _launcherService;
        private Interfaces.IVersionService? _versionService;

        public MainWindow()
        {
            InitializeComponent();
            progressTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(0.05)
            };
            progressTimer.Tick += ProgressTime_Tick;
        }

        private async void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // 初始化MVVM架构的ViewModel
            InitializeViewModel();

            // 初始化服务层
            InitializeServices();

            XmlConfigurator.Configure();
            isLaunching = true;
            Log.logger.Info("—————新日志分割线—————");
            Log.logger.Info("工具箱已进入加载流程。");
            Log.logger.Info("We have a lift off.");
            Log.logger.Info($"WPF架构工具箱 版本：{VERSION} 。");
            await DisableGlobalOperations();
            // 设置网络协议
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
            using HttpClient client = new HttpClient
            {
                DefaultRequestVersion = HttpVersion.Version11,
                DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact
            };
            CheckMirrorChyan();
            await CheckLoadingText();
            InitNode();
            // Mirror酱模式的节点选项已在ViewModel中处理，无需在此操作ComboBox
            await RefreshPage();
            // 版本检查已在ViewModel初始化时自动执行
            CheckLimbusCompanyPath();
            SevenZipBase.SetLibraryPath(Path.Combine(currentDir, "Resources/7z.dll"));
            await CheckAnno();
            CheckLCBPath();
            bool needUpdate = await ChangeHomePageVersion();
            AdaptFuckingPM.CheckAdapt(limbusCompanyDir);
            if (!isLauncherMode)
            {
                LaunchUpdateLoadingThread();
                await ChangeEEPic();
                // 模组安装状态检查已在ViewModel中处理
                await CheckDNS();
            }
            if(isLauncherMode && !hasNewAnno && !needUpdate)
            {
                try
                {
                    OpenUrl("steam://rungameid/1973530");
                }
                catch (Exception ex)
                {
                    Log.logger.Error("出现了问题： ", ex);
                    UniversalDialog.ShowMessage("出现了问题。\n" + ex.ToString(), "提示", null, this);
                }
                Environment.Exit(0);
            }
            if ((configuation.Settings.install.installWhenLaunch || isLauncherMode) && !hasNewAnno && needUpdate)
            {
                ViewModel?.InstallCommand.Execute(null);
            }
            await EnableGlobalOperations();
            isLaunching = false;
            Log.logger.Info("加载流程完成。");
        }

        #region MVVM初始化

        /// <summary>
        /// 初始化ViewModel实例
        /// </summary>
        private void InitializeViewModel()
        {
            try
            {
                // 从ServiceLocator获取服务
                var configService = ServiceLocator.GetService<IConfigService>();
                var pathService = ServiceLocator.GetService<IPathService>();
                var installService = ServiceLocator.GetService<IInstallService>();
                var dialogService = ServiceLocator.GetService<IDialogService>();
                var downloadService = ServiceLocator.GetService<LLC_MOD_Toolbox.Interfaces.IDownloadService>();
                _errorService = ServiceLocator.GetService<IErrorService>();
                _launcherService = ServiceLocator.GetService<ILauncherService>();
                _versionService = ServiceLocator.GetService<IVersionService>();

                // 初始化MainViewModel
                ViewModel = ServiceLocator.CreateMainViewModel(this);
                this.DataContext = ViewModel;

                // 监听MainViewModel的属性变化
                ViewModel.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(ViewModels.MainViewModel.IsInstalling))
                    {
                        isInstalling = ViewModel.IsInstalling;
                    }
                    if (e.PropertyName == nameof(ViewModels.MainViewModel.InstallProgress))
                    {
                        _ = System.Threading.Tasks.Task.Run(async () => await ChangeProgressValue(ViewModel.InstallProgress));
                    }
                };

                // 创建页面ViewModel实例
                _settingsViewModel = new ViewModels.SettingsPageViewModel(
                    configService,
                    pathService,
                    installService,
                    dialogService,
                    _launcherService);

                _gachaSimulatorViewModel = new ViewModels.GachaSimulatorPageViewModel(
                    configService,
                    dialogService);

                _fontReplaceViewModel = new ViewModels.FontReplacePageViewModel(
                    ServiceLocator.GetService<IFontService>(),
                    pathService,
                    dialogService);

                _announcementViewModel = new ViewModels.AnnouncementPageViewModel(
                    configService,
                    ServiceLocator.GetService<IAnnouncementService>(),
                    dialogService,
                    ServiceLocator.GetService<INavigationService>());

                _announcementViewModel.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(ViewModels.AnnouncementPageViewModel.IsVisible)
                        && _announcementViewModel != null
                        && !_announcementViewModel.IsVisible)
                    {
                        _ = ChangeLeftButtonStatu(true);
                        isInAnno = false;
                    }
                };

                _greytestViewModel = new ViewModels.GreytestPageViewModel(
                    ServiceLocator.GetService<IGreytestService>(),
                    configService,
                    dialogService);

                // 设置各个页面的DataContext（用于XAML绑定）
                SettingsPage.DataContext = _settingsViewModel;
                GachaPage.DataContext = _gachaSimulatorViewModel;
                FontReplacePage.DataContext = _fontReplaceViewModel;
                AnnouncementPage.DataContext = _announcementViewModel;
                GreytestPage.DataContext = _greytestViewModel;

                // 订阅GachaSimulatorPageViewModel的GachaResults集合变化，以更新抽卡结果UI
                if (_gachaSimulatorViewModel != null)
                {
                    _gachaSimulatorViewModel.GachaResults.CollectionChanged += (s, e) =>
                    {
                        _ = UpdateGachaResultsUI();
                    };
                }

                // 订阅FontReplacePageViewModel的属性变化，以更新预览字体资源
                if (_fontReplaceViewModel != null)
                {
                    _fontReplaceViewModel.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(ViewModels.FontReplacePageViewModel.PreviewFontFamily))
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                var customFont = new FontFamily(new Uri(_fontReplaceViewModel.FontFilePath), "#" + _fontReplaceViewModel.PreviewFontFamily);
                                this.Resources["GlobalPreviewFont"] = customFont;
                                this.Resources["GlobalPreviewFontSize"] = _fontReplaceViewModel.PreviewFontSize;
                                this.Resources["GlobalPreviewSmallFontSize"] = _fontReplaceViewModel.PreviewSmallFontSize;
                            });
                        }
                    };
                }

                // 订阅GreytestPageViewModel的IsEnabled属性变化，显示灰度测试Logo
                if (_greytestViewModel != null)
                {
                    _greytestViewModel.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(ViewModels.GreytestPageViewModel.IsEnabled))
                        {
                            var greytestService = ServiceLocator.GetService<IGreytestService>();
                            if (greytestService.IsEnabled)
                            {
                                Log.logger.Info($"灰度测试模式已启用：DownloadUrl={greytestService.DownloadUrl}");
                                _ = ChangeLogoToTest();
                            }
                        }
                    };
                }

                // 订阅NavigationService的事件以刷新Mirror酱UI状态
                var navigationService = ServiceLocator.GetService<INavigationService>();
                if (navigationService is Services.NavigationService navService)
                {
                    navService.OnNavigatedToSettings += async () =>
                    {
                        if (_settingsViewModel != null)
                        {
                            await _settingsViewModel.RefreshMirrorChyanUIAsync();
                        }
                    };
                }

                Log.logger.Info("ViewModel初始化完成");
            }
            catch (Exception ex)
            {
                Log.logger.Error($"初始化ViewModel失败: {ex.Message}", ex);
                MessageBox.Show($"初始化ViewModel失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion


        #region 代理方法（用于向后兼容，转发到ViewModel/Service）

        /// <summary>
        /// 初始化节点代理方法
        /// </summary>
        public void InitNode()
        {
            if (ViewModel != null)
            {
                ViewModel.InitializeNodesCommand.Execute(null);
                useEndPoint = ViewModel.UseEndPoint;
                useAPIEndPoint = string.IsNullOrWhiteSpace(ViewModel.UseAPIEndPoint)
                    ? ViewModel.NodeManagement.GetDefaultApiNodeEndpoint()
                    : ViewModel.UseAPIEndPoint;
            }
        }

        /// <summary>
        /// 检查LimbusCompany路径代理方法
        /// </summary>
        private void CheckLimbusCompanyPath()
        {
            if (ViewModel == null)
            {
                return;
            }

            if (configuation.Settings.general.skipLCBPathCheck
                && !string.IsNullOrWhiteSpace(configuation.Settings.general.LCBPath))
            {
                limbusCompanyDir = configuation.Settings.general.LCBPath;
                ViewModel.Path.GameDirectory = limbusCompanyDir;
                ViewModel.LimbusCompanyDir = limbusCompanyDir;
                Log.logger.Info("跳过检查路径。");
            }
            else
            {
                bool confirmed = false;

                if (string.IsNullOrWhiteSpace(limbusCompanyDir))
                {
                    try
                    {
                        limbusCompanyDir = ViewModel.Path.FindGamePath(
                            appId: "1973530",
                            executableName: "LimbusCompany.exe");
                        Log.logger.Info($"找到 Limbus Company 安装路径：{limbusCompanyDir}");
                    }
                    catch (Exception ex)
                    {
                        Log.logger.Info($"未找到：{ex.Message}");
                    }
                }

                if (!string.IsNullOrWhiteSpace(limbusCompanyDir))
                {
                    confirmed = UniversalDialog.ShowConfirm(
                        $"这是您的边狱公司地址吗？\n{limbusCompanyDir}",
                        "检查路径");
                }

                if (confirmed)
                {
                    Log.logger.Info("用户确认路径。");
                    configuation.UpdateConfig(settings =>
                    {
                        settings.general.LCBPath = limbusCompanyDir;
                        settings.general.skipLCBPathCheck = true;
                    });
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(limbusCompanyDir))
                    {
                        Log.logger.Warn("未能找到 Limbus Company 目录，手动选择模式。");
                        UniversalDialog.ShowMessage("未能找到 Limbus Company 目录。请手动选择。", "提示", null, null);
                    }
                    else
                    {
                        Log.logger.Warn("用户否认 Limbus Company 目录正确性。");
                    }

                    var fileDialog = new OpenFileDialog
                    {
                        Title = "请选择你的边狱公司游戏文件",
                        Multiselect = false,
                        InitialDirectory = limbusCompanyDir,
                        Filter = "LimbusCompany.exe|LimbusCompany.exe",
                        FileName = "LimbusCompany.exe"
                    };

                    if (fileDialog.ShowDialog() == true)
                    {
                        limbusCompanyDir = Path.GetDirectoryName(fileDialog.FileName) ?? limbusCompanyDir;
                        limbusCompanyGameDir = Path.GetFullPath(fileDialog.FileName);
                    }

                    if (string.IsNullOrWhiteSpace(limbusCompanyDir)
                        || !ViewModel.Path.ValidateGamePath(limbusCompanyDir))
                    {
                        Log.logger.Error("选择了错误目录，关闭。");
                        UniversalDialog.ShowMessage("选择目录有误，没有在当前目录找到游戏。", "错误", null, null);
                        System.Windows.Application.Current.Shutdown();
                        return;
                    }

                    Log.logger.Info("找到了正确目录。");
                    configuation.UpdateConfig(settings =>
                    {
                        settings.general.LCBPath = limbusCompanyDir;
                        settings.general.skipLCBPathCheck = true;
                    });
                }

                ViewModel.Path.GameDirectory = limbusCompanyDir;
                ViewModel.LimbusCompanyDir = limbusCompanyDir;
            }

            limbusCompanyGameDir = Path.Combine(limbusCompanyDir, "LimbusCompany.exe");
            Log.logger.Info($"游戏路径：{limbusCompanyDir}");
        }

        /// <summary>
        /// 检查LCB路径代理方法
        /// </summary>
        public void CheckLCBPath()
        {
            CheckLimbusCompanyPath();
        }

        /// <summary>
        /// 节点选择变化事件（临时实现，待迁移到MVVM绑定）
        /// </summary>
        private async void NodeComboboxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: 迁移到MVVM模式，通过绑定ViewModel的SelectedNode属性实现
            var mirrorService = ServiceLocator.GetService<IMirrorChyanService>();
            if (mirrorService?.IsEnabled == true) return;
            await Task.CompletedTask;
        }

        /// <summary>
        /// API节点选择变化事件（临时实现，待迁移到MVVM绑定）
        /// </summary>
        private async void APIComboboxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: 迁移到MVVM模式，通过绑定ViewModel的SelectedApi属性实现
            var mirrorService = ServiceLocator.GetService<IMirrorChyanService>();
            if (mirrorService?.IsEnabled == true) return;
            await Task.CompletedTask;
        }

        /// <summary>
        /// 打开URL代理方法
        /// </summary>
        public static void OpenUrl(string url)
        {
            try
            {
                Log.logger.Info("打开URL：" + url);
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Log.logger.Error("打开URL失败", ex);
            }
        }

        /// <summary>
        /// 获取URL文本代理方法
        /// </summary>
        private async Task<string> GetURLText(string url, bool reportError = true, int maxRetries = 3, int delayMs = 300, bool parseErrorJson = false)
        {
            if (ViewModel != null)
            {
                return await ViewModel.FileUtility.GetURLText(url, reportError, maxRetries, delayMs, parseErrorJson);
            }
            return string.Empty;
        }

        /// <summary>
        /// 下载文件代理方法
        /// </summary>
        private async Task DownloadFileAsync(string url, string path)
        {
            if (ViewModel != null)
            {
                await ViewModel.FileUtility.DownloadFileAsync(url, path);
            }
        }

        /// <summary>
        /// 解压文件代理方法
        /// </summary>
        public static void Unarchive(string archivePath, string output)
        {
            try
            {
                using SevenZipExtractor extractor = new(archivePath);
                extractor.ExtractArchive(output);
                Log.logger.Info($"解压完成：{archivePath} -> {output}");
            }
            catch (Exception ex)
            {
                Log.logger.Error("解压失败", ex);
                throw;
            }
        }

        #endregion

        #region 进度条系统
        public async void ProgressTime_Tick(object? sender, EventArgs e)
        {
            await ChangeProgressValue(progressPercentage);
        }
        public void StartProgressTimer()
        {
            progressPercentage = 0;
            progressTimer.Start();
        }

        public void StopProgressTimer()
        {
            progressTimer.Stop();
        }
        #endregion
        #region 灰度测试
        // 注：灰度测试Token验证已迁移到GreytestPageViewModel
        // 以下方法供安装流程和ViewModel回调使用
        private async Task ChangeLogoToTest()
        {
            await this.Dispatcher.BeginInvoke(() =>
            {
                ZALogo.Visibility = Visibility.Visible;
            });
        }
        private async Task InstallGreytestMod()
        {
            await Task.Run(async () =>
            {
                Log.logger.Info("灰度测试模式已开启。开始安装灰度模组。");
                installPhase = 2;
                isNewestModVersion = false;
                
                // 从IGreytestService获取下载URL
                var greytestService = ServiceLocator.GetService<IGreytestService>();
                var downloadUrl = greytestService.DownloadUrl;
                
                if (string.IsNullOrEmpty(downloadUrl))
                {
                    Log.logger.Error("灰度测试下载URL为空");
                    return;
                }
                
                await DownloadFileAsync(downloadUrl, limbusCompanyDir + "/LimbusLocalize_Dev.7z");
                Unarchive(limbusCompanyDir + "/LimbusLocalize_Dev.7z", limbusCompanyDir);
                File.Delete(limbusCompanyDir + "/LimbusLocalize_Dev.7z");
                Log.logger.Info("灰度模组安装完成。");
            });
        }
        #endregion
        #region 抽卡模拟器

        // UI状态变量（必须保留，被MainWindow.xaml.cs引用）
        private static bool isInitGacha = false;

        /// <summary>
        /// 初始化抽卡模拟器（UI入口方法）
        /// </summary>
        private async Task InitGacha()
        {
            try
            {
                Log.logger.Info("初始化抽卡模拟器");

                // 使用ViewModel中的GachaSimulatorService
                if (ViewModel != null && !ViewModel.GachaSimulator.IsInitialized)
                {
                    string gachaDataUrl = "https://download.zeroasso.top/wiki/wiki_personal.json";
                    bool success = await ViewModel.GachaSimulator.InitializeAsync(gachaDataUrl);

                    if (!success)
                    {
                        Log.logger.Error("初始化抽卡模拟器失败");
                        UniversalDialog.ShowMessage("初始化失败。请检查网络情况。", "", null, this);
                        return;
                    }
                }

                isInitGacha = true;
                UniversalDialog.ShowMessage("初始化完成。", "提示", null, this);
                Log.logger.Info("抽卡模拟器初始化完成");
            }
            catch (Exception ex)
            {
                Log.logger.Error("初始化抽卡模拟器失败", ex);
                UniversalDialog.ShowMessage("初始化失败：\n" + ex.Message, "", null, this);
            }
        }

        // ========================================================================
        // 注释：以下方法已迁移到GachaSimulatorService (2025-01-05 MVVM重构)
        // 包含功能：抽卡逻辑、人格管理、概率计算等
        // 使用方式：ViewModel.GachaSimulator.MethodName()
        // ========================================================================
        #endregion
        #region 错误处理
        /// <summary>
        /// 用于错误处理。
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="CloseWindow">是否关闭窗体。</param>
        /// <param name="advice">提供建议</param>
        public void ErrorReport(Exception ex, bool CloseWindow, string advice = "")
        {
            if (_errorService != null)
            {
                _errorService.ReportError(ex, CloseWindow, advice);
            }
            else
            {
                // Fallback if service is not available
                Log.logger.Error("出现了问题：\n", ex);
                string errorMessage = ReturnExceptionText(ex);
                string message = CloseWindow
                    ? $"运行中出现了问题，且在这个错误发生后，工具箱将关闭。\n{advice}若要反馈，请带上链接或日志。\n反馈请勿！请勿截图此页面！\n错误分析原因：\n{errorMessage}"
                    : $"运行中出现了问题。但你仍然能够使用工具箱（大概）。\n{advice}若要反馈，请带上链接或日志。\n反馈请勿！请勿截图此页面！\n——————————\n错误分析原因：\n{errorMessage}";

                UniversalDialog.ShowMessage(message, "错误", null, this);

                if (CloseWindow)
                {
                    Application.Current.Shutdown();
                }
            }
        }
        public void ErrorReportMirrorChyan(MirrorChyanException ex, bool CloseWindow)
        {
            if (_errorService != null)
            {
                _errorService.ReportMirrorChyanError(ex, CloseWindow);
            }
            else
            {
                // Fallback if service is not available
                Log.logger.Error("访问 Mirror 酱服务中出现了错误\n", ex);
                string message = CloseWindow
                    ? $"访问 Mirror 酱服务出现了问题，且在这个错误发生后，工具箱将关闭。\n出现该问题原因：{ex.Message}\n若要反馈，请带上链接或日志。\n反馈请勿！请勿截图此页面！"
                    : $"访问 Mirror 酱服务出现了问题。但你仍然能够使用工具箱（大概）。\n出现该问题原因：{ex.Message}\n若要反馈，请带上链接或日志。\n反馈请勿！请勿截图此页面！";

                UniversalDialog.ShowMessage(message, "错误", null, this);

                if (CloseWindow)
                {
                    Application.Current.Shutdown();
                }
            }
        }
        public static string ReturnExceptionText(Exception ex)
        {
            if (ex is (System.Net.WebException) || (ex is HttpRequestException) || (ex is HttpProtocolException) || (ex is System.Net.Sockets.SocketException) || (ex is System.Net.HttpListenerException) || (ex is HttpIOException))
            {
                return "网络链接错误，请尝试更换节点，关闭加速器或代理后再试。\n您也可以尝试在官网“常见问题”进行排查。";
            }
            else if (ex is SevenZipException)
            {
                return "解压出现问题，大概率为网络问题。\n请尝试更换节点，关闭加速器或代理后再试。\n您也可以尝试在官网“常见问题”进行排查。";
            }
            else if (ex is FileNotFoundException)
            {
                return "无法找到文件，可能是网络问题，也可能是边狱公司路径出现错误。\n请尝试更换节点，关闭加速器或代理后再试。\n您也可以尝试在官网“常见问题”进行排查。";
            }
            else if (ex is UnauthorizedAccessException)
            {
                return "无权限访问文件，请尝试以管理员身份启动，也可能是你打开了边狱公司？";
            }
            else if (ex is IOException)
            {
                return "文件访问出现问题。\n可能是文件已被边狱公司占用？\n您可以尝试关闭边狱公司。";
            }
            else if (ex is HashException)
            {
                return "文件损坏。\n大概率为网络问题，请尝试更换节点，关闭加速器或代理后再试。\n您也可以尝试在官网“常见问题”进行排查。";
            }
            return "未知错误原因，错误已记录至日志，请查看官网“常见问题”进行排查。\n如果没有解决，请尝试进行反馈。";
        }
        #endregion
        #region 公告系统
        private DispatcherTimer? _AnnoTimer;
        private int annoLastTime = 0;
        private bool isInAnno = false;
        private bool hasNewAnno = false;
        private async Task CheckAnno()
        {
            if (_announcementViewModel == null)
            {
                return;
            }

            try
            {
                bool hasAnnouncement = await _announcementViewModel.CheckAnnouncementAsync();
                if (!hasAnnouncement)
                {
                    return;
                }

                await ChangeLeftButtonStatu(false);
                isInAnno = true;
                hasNewAnno = true;

                var navigationService = ServiceLocator.GetService<INavigationService>();
                if (navigationService != null)
                {
                    await navigationService.NavigateToAsync("anno");
                }
            }
            catch (Exception ex)
            {
                Log.logger.Error("检查公告失败。", ex);
            }
        }
        private async void AnnoTimer_Tick(object? sender, EventArgs e)
        {
            if (annoLastTime > 0)
            {
                annoLastTime -= 1;
                await ChangeAnnoTip(annoLastTime);
            }
            else
            {
                isInAnno = false;
                await AnnoCountEnd();
                _AnnoTimer?.Stop();
            }
        }
        private async void AnnoucementButtonClick(object sender, RoutedEventArgs e)
        {
            await AlreadyReadAnno();
        }
        #endregion
        #region 启动器发生器
        private void LauncherSender(object sender, EventArgs e)
        {
            if (_launcherService != null)
            {
                try
                {
                    var shortcutPath = _launcherService.CreateDesktopShortcut();
                    UniversalDialog.ShowMessage("快捷方式已创建。\n可在桌面上找到\"LimbusCompany with LLC\"启动。", "提示", null, this);
                }
                catch (Exception ex)
                {
                    Log.logger.Error("创建快捷方式失败", ex);
                    UniversalDialog.ShowMessage($"创建快捷方式失败：{ex.Message}", "错误", null, this);
                }
            }
            else
            {
                // Fallback to original implementation if service is not available
                Log.logger.Warn("LauncherService未初始化，使用备用方法");
                CreateShortcutFallback();
            }
        }

        /// <summary>
        /// 备用快捷方式创建方法（当Service不可用时）
        /// </summary>
        private void CreateShortcutFallback()
        {
            // 获取桌面路径
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            // 快捷方式完整保存路径
            string shortcutPath = Path.Combine(desktopPath, "LimbusCompany with LLC.lnk");

            try
            {
                // 使用反射创建WshShell对象，避免COM引用问题
                Type? wshShellType = Type.GetTypeFromProgID("WScript.Shell");
                if (wshShellType == null)
                {
                    Log.logger.Error("无法获取 WScript.Shell 类型");
                    UniversalDialog.ShowMessage("创建快捷方式失败：无法访问 Windows Script Host。", "错误", null, this);
                    return;
                }

                dynamic? wsh = System.Activator.CreateInstance(wshShellType);
                if (wsh == null)
                {
                    Log.logger.Error("无法创建 WScript.Shell 实例");
                    UniversalDialog.ShowMessage("创建快捷方式失败：无法创建 WScript.Shell 实例。", "错误", null, this);
                    return;
                }

                // 创建快捷方式
                dynamic? shortcut = wsh.CreateShortcut(shortcutPath);
                if (shortcut == null)
                {
                    Log.logger.Error("无法创建快捷方式对象");
                    UniversalDialog.ShowMessage("创建快捷方式失败：无法创建快捷方式对象。", "错误", null, this);
                    return;
                }

                // 设置快捷方式属性
                shortcut.TargetPath = Path.Combine(currentDir, "LLC_MOD_Toolbox.exe");
                shortcut.Arguments = "-launcher";
                shortcut.WorkingDirectory = currentDir;
                shortcut.Description = "启动边狱公司并检查汉化更新";
                shortcut.IconLocation = Path.Combine(currentDir, "PublicResource", "favicon.ico");
                shortcut.Save();

                Log.logger.Info($"快捷方式已创建: {shortcutPath}");
                UniversalDialog.ShowMessage("快捷方式已创建。\n可在桌面上找到\"LimbusCompany with LLC\"启动。", "提示", null, this);
            }
            catch (Exception ex)
            {
                Log.logger.Error("创建快捷方式失败", ex);
                UniversalDialog.ShowMessage($"创建快捷方式失败：{ex.Message}", "错误", null, this);
            }
        }
        private void LauncherHelper(object sender, EventArgs e)
        {
            OpenUrl("https://www.zeroasso.top/docs/install/hotupdate");
        }
        internal void HowToUseHotUpdate(object sender, EventArgs e)
        {
            var instructions = _launcherService?.GetHotUpdateInstructions()
                ?? "无法获取热更新使用说明。";
            UniversalDialog.ShowMessage(instructions, "热更新教程", null, this);
        }
        #endregion
        #region Loading文本
        // 注：Loading文本核心逻辑已迁移到ILoadingTextService
        private ILoadingTextService? _loadingTextService;
        
        private async Task CheckLoadingText()
        {
            try
            {
                _loadingTextService = ServiceLocator.GetService<ILoadingTextService>();
                var loadingTextFilePath = Path.Combine(currentDir, "Config", "loadingText.json");
                
                // 从文件加载
                _loadingTextService.LoadFromFile(loadingTextFilePath);
                
                // 检查是否需要更新（超过14天）
                if (_loadingTextService.NeedsUpdate())
                {
                    Log.logger.Info("Loading文本需要更新。");
                    bool success = await _loadingTextService.DownloadNewTextsAsync(
                        configuation.Settings.general.internationalMode);
                    if (success)
                    {
                        _loadingTextService.SaveToFile(loadingTextFilePath);
                    }
                }
                
                // 获取权重随机文本
                string loadingText = _loadingTextService.GetWeightedRandomText();
                Log.logger.Info("Loading文本：" + loadingText);
                await ChangeLoadingText(loadingText);
            }
            catch (Exception ex)
            {
                Log.logger.Error("加载Loading文本失败", ex);
                await ChangeLoadingText("正在为您加载工具箱...");
            }
        }
        
        private void LaunchUpdateLoadingThread()
        {
            Thread updateLoadingThread = new Thread(async () =>
            {
                while (true)
                {
                    await Task.Delay(TimeSpan.FromSeconds(30));
                    await TurnAnotherLoadingText();
                }
            });
            updateLoadingThread.Start();
        }
        
        private async Task TurnAnotherLoadingText()
        {
            if (_loadingTextService == null)
            {
                Log.logger.Error("LoadingTextService未初始化");
                return;
            }
            
            string loadingText = _loadingTextService.GetWeightedRandomText();
            Log.logger.Info("Loading文本：" + loadingText);
            await ChangeLoadingText(loadingText);
        }
        #endregion
        #region 字体替换
        // 注：字体替换Click事件已迁移到FontReplacePageViewModel
        // 字体替换辅助方法（供ViewModel回调使用）
        private bool IsValidFontFile(string filePath)
        {
            if (filePath == "输入字体路径")
            {
                return false;
            }
            // 1. 检查文件扩展名
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            if (extension != ".ttf" && extension != ".otf" && extension != ".ttc")
            {
                return false;
            }

            // 2. 检查文件是否存在并且可以访问
            if (!File.Exists(filePath))
            {
                return false;
            }

            // 3. 尝试加载字体来验证它是否真的是字体文件
            try
            {
                // 尝试加载字体，如果不是有效的字体文件，这将引发异常
                using (System.Drawing.Text.PrivateFontCollection fontCollection = new System.Drawing.Text.PrivateFontCollection())
                {
                    fontCollection.AddFontFile(filePath);
                    return fontCollection.Families.Length > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private string GetFontFamilyName(string filePath)
        {
            // 获取字体的家族名称
            try
            {
                using (System.Drawing.Text.PrivateFontCollection fontCollection = new System.Drawing.Text.PrivateFontCollection())
                {
                    fontCollection.AddFontFile(filePath);
                    if (fontCollection.Families.Length > 0)
                    {
                        return fontCollection.Families[0].Name;
                    }
                }
            }
            catch { }

            // 如果无法获取字体名称，返回一个默认值
            return Path.GetFileNameWithoutExtension(filePath);
        }
        #endregion
        #region MirrorChyan
        internal void CheckMirrorChyan()
        {
            // 如果是首用，显示介绍并获取token
            if (!configuation.Settings.mirrorChyan.notice)
            {
                HandleFirstTimeSetup();
                return;
            }

            // 如果已启用，加载token
            if (configuation.Settings.mirrorChyan.enable && SecureStringStorage.HasSavedData())
            {
                HandleExistingSetup();
            }
        }

        private void HandleFirstTimeSetup()
        {
            string token = ShowMirrorChyanDialog();
            // 标记已显示过提示
            configuation.UpdateConfig(settings =>
            {
                settings.mirrorChyan.notice = true;
            });

            if (string.IsNullOrWhiteSpace(token))
            {
                return;
            }

            // 设置Mirror酱模式
            SetupMirrorChyanMode(token);
            Log.logger.Info("MirrorChyan Mode 已开启。");
            Log.logger.Info("MirrorChyan Token： 什么，这种东西当然不可能放日志了，我是傻吗");
            // 有的时候也是需要测试的
            // Log.logger.Info("MirrorChyan Token：" + token);
        }

        private void HandleExistingSetup()
        {
            var mirrorChyanService = ServiceLocator.GetService<IMirrorChyanService>();
            var token = SecureStringStorage.LoadToken();
            // Also only for debug
            // Log.logger.Info("MirrorChyan Token：" + token);

            if (!string.IsNullOrWhiteSpace(token))
            {
                Log.logger.Info("设置Mirror酱模式。");
                this.Dispatcher.Invoke(() =>
                {
                    MirrorChyanLogo.Visibility = Visibility.Visible;
                });
                mirrorChyanService?.Enable(token, saveToConfig: false);
                MirrorChyanConfigButtonLabelChanger(true);
                return;
            }

            // token加载失败，询问用户是否重新输入
            bool result = UniversalDialog.ShowConfirm("读取Mirror酱秘钥失败，你想要再输入一次秘钥吗？", "提示", this);

            if (result)
            {
                HandleTokenReInput();
            }
            else
            {
                DisableMirrorChyanMode();
            }
        }

        private void HandleTokenReInput()
        {
            var token = ShowMirrorChyanDialog();

            if (string.IsNullOrWhiteSpace(token))
                return;

            SetupMirrorChyanMode(token);
            Log.logger.Info("MirrorChyan Mode 已开启。");
        }

        private void SetupMirrorChyanMode(string token)
        {
            Log.logger.Info("设置Mirror酱模式。");
            this.Dispatcher.Invoke(() =>
            {
                MirrorChyanLogo.Visibility = Visibility.Visible;
            });
            
            var mirrorChyanService = ServiceLocator.GetService<IMirrorChyanService>();
            var trimmedToken = token.Trim();
            mirrorChyanService?.Enable(trimmedToken);
            SecureStringStorage.SaveToken(trimmedToken);
            MirrorChyanConfigButtonLabelChanger(true);

            // 同步更新ViewModels中的静态字段
            ViewModels.SettingsPageViewModel.SetMirrorChyanMode(true);
        }

        private void DisableMirrorChyanMode()
        {
            var mirrorChyanService = ServiceLocator.GetService<IMirrorChyanService>();
            mirrorChyanService?.Disable();
            Log.logger.Info("MirrorChyan Mode 已关闭。");

            // 同步更新ViewModels中的静态字段
            ViewModels.SettingsPageViewModel.SetMirrorChyanMode(false);
        }

        private string? ShowMirrorChyanDialog()
        {
            const string message = "Mirror酱是一个第三方应用分发平台，让开源应用的更新更简单。\n" +
                                  "用户付费使用，收益与开发者共享。\n" +
                                  "如果你拥有Mirror酱秘钥，能够缓解你在使用本软件时可能遇到的网络问题。\n" +
                                  "没有？没关系，你也可以忽略本提示，零协会仍然提供免费镜像源。\n" +
                                  "想了解一下？点击右下角按钮。";

            const string title = "等下，你有Mirror酱秘钥吗？";

            return InputMirrorChyan.ShowDialog(message, title, "Mirror酱秘钥", true, this);
        }
        internal JObject ParseMirrorChyanJson(string json)
        {
            JObject parsed = JObject.Parse(json);
            int code = parsed["code"]?.Value<int>() ?? -1;
            if (code != 0)
            {
                throw new MirrorChyanException(code);
            }
            return parsed;
        }
        internal void MirrorChyanConfigButtonLabelChanger(bool mirrorChyanMode)
        {
            Dispatcher.Invoke(() =>
            {
                if (mirrorChyanMode)
                {
                    MirrorChyanConfigButtonLabel.Content = "禁用";
                }
                else
                {
                    MirrorChyanConfigButtonLabel.Content = "填写秘钥";
                }
            });
        }
        internal void MirrorChyanConfigButtonSender(object sender, RoutedEventArgs e)
        {
            var mirrorChyanService = ServiceLocator.GetService<IMirrorChyanService>();
            if (mirrorChyanService?.IsEnabled == true)
            {
                bool result = UniversalDialog.ShowConfirm("确定要禁用Mirror酱吗？\n关闭后，你可以在设置重新开启Mirror酱的服务。", "提示", this);
                if (!result)
                {
                    return;
                }
                mirrorChyanService.Disable();
                mirrorChyanService.ClearSavedToken();
                UniversalDialog.ShowMessage("已禁用Mirror酱并删除你的Mirror酱CDK。\n为了处理，软件将关闭，再次启动后效果生效。", "提示", null, this);
                Application.Current.Shutdown();
            }
            else
            {
                var result = UniversalDialog.ShowInput(
                    "请输入你的 Mirror 酱 CDK。\n你可以在 Mirror 酱官网购买。",
                    "输入秘钥",
                    "Mirror 酱 CDK",
                    InputType.Password,
                    [new DialogButton("确定", true, false), new DialogButton("取消", false, true)],
                    this);
                if (result.IsCanceled)
                {
                    return;
                }
                if (result.IsSuccess && !string.IsNullOrEmpty(result.Input))
                {
                    mirrorChyanService?.Enable(result.Input);
                    UniversalDialog.ShowMessage("Mirror酱秘钥设置成功。\n为了处理，软件将关闭，再次启动后效果生效。", "提示", null, this);
                    Application.Current.Shutdown();
                }
                else
                {
                    UniversalDialog.ShowMessage("设置失败。", "提示", null, this);
                }
            }
        }
        #endregion
        #region 改主页版本号
        internal async Task<bool> ChangeHomePageVersion()
        {
            if (_versionService == null)
            {
                Log.logger.Error("VersionService未初始化");
                await ShowVersionError();
                return false;
            }

            try
            {
                // 从IMirrorChyanService获取状态
                var mirrorService = ServiceLocator.GetService<IMirrorChyanService>();
                var isMirrorMode = mirrorService?.IsEnabled ?? false;
                var mirrorToken = mirrorService?.Token ?? string.Empty;
                
                // 使用VersionService检查更新
                var result = await _versionService.CheckForUpdateAsync(
                    limbusCompanyDir,
                    isMirrorMode,
                    mirrorToken,
                    useAPIEndPoint);

                await Dispatcher.BeginInvoke(() =>
                {
                    NowVersionText.Text = result.CurrentVersionText;
                    LatestVersionText.Text = result.LatestVersionText;
                });

                return result.NeedUpdate;
            }
            catch (Exception ex)
            {
                Log.logger.Error("检查版本失败", ex);
                await ShowVersionError();
                return false;
            }
        }
        
        private async Task ShowVersionError()
        {
            await Dispatcher.BeginInvoke(() =>
            {
                NowVersionText.Text = "当前版本：获取失败";
                LatestVersionText.Text = "最新版本：获取失败";
            });
        }
        #endregion

        #region 抽卡结果UI更新
        /// <summary>
        /// 更新抽卡结果UI，将GachaResults的前10个结果显示在GachaText1-10 Labels
        /// </summary>
        private async Task UpdateGachaResultsUI()
        {
            if (_gachaSimulatorViewModel == null) return;

            await this.Dispatcher.InvokeAsync(() =>
            {
                // 获取所有GachaText Labels
                var labels = new System.Windows.Controls.Label[] 
                { 
                    GachaText1, GachaText2, GachaText3, GachaText4, GachaText5,
                    GachaText6, GachaText7, GachaText8, GachaText9, GachaText10
                };

                // 首先隐藏所有Labels
                foreach (var label in labels)
                {
                    label.Visibility = Visibility.Collapsed;
                }

                // 获取最新的10个抽卡结果
                var results = _gachaSimulatorViewModel.GachaResults.Take(10).ToList();

                // 更新每个Label
                for (int i = 0; i < results.Count && i < labels.Length; i++)
                {
                    var result = results[i];
                    var label = labels[i];
                    if (label.Content is System.Windows.Controls.TextBlock textBlock)
                    {
                        // 根据星级显示不同格式
                        string stars = new string('★', result.Rarity);
                        textBlock.Text = $"[{stars}] {result.Personality}";
                        
                        // 根据星级设置颜色
                        string color = result.Rarity switch
                        {
                            1 => "#B88345",  // 1星
                            2 => "#CA1400",  // 2星
                            3 => "#FCC404",  // 3星
                            _ => "#B88345"
                        };
                        textBlock.Foreground = new SolidColorBrush(
                            (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color));
                    }
                    label.Visibility = Visibility.Visible;
                }
            });
        }
        #endregion
    }
}
