using LLC_MOD_Toolbox.Interfaces;
using LLC_MOD_Toolbox.Services;
using LLC_MOD_Toolbox.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace LLC_MOD_Toolbox.Services
{
    /// <summary>
    /// 服务定位器
    /// 提供依赖注入容器和单例服务访问
    /// </summary>
    public static class ServiceLocator
    {
        private static IServiceProvider? _serviceProvider;

        /// <summary>
        /// 配置服务容器
        /// </summary>
        public static void ConfigureServices()
        {
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

            // 注册单例服务
            services.AddSingleton<IConfigService, ConfigService>();
            services.AddSingleton<IPathService, PathService>();
            services.AddSingleton<IDownloadService, DownloadService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IInstallService, InstallService>();
            services.AddSingleton<IFontService, FontService>();
            services.AddSingleton<IGreytestService, GreytestService>();
            services.AddSingleton<ILoadingTextService, LoadingTextService>();
            services.AddSingleton<IAnnouncementService, AnnouncementService>();
            services.AddSingleton<IMirrorChyanService, MirrorChyanService>();
            services.AddSingleton<IProgressService, ProgressService>();
            services.AddSingleton<IUIService, UIService>();
            services.AddSingleton<IResourceService, ResourceService>();
            services.AddSingleton<IErrorService, ErrorService>();
            services.AddSingleton<ILauncherService, LauncherService>();
            services.AddSingleton<IVersionService, VersionService>();
            services.AddSingleton<IFileUtilityService, FileUtilityService>();
            services.AddSingleton<IModInstallService, ModInstallService>();
            services.AddSingleton<IModUninstallService, ModUninstallService>();
            services.AddSingleton<IGachaSimulatorService, GachaSimulatorService>();
            services.AddSingleton<INodeManagementService, NodeManagementService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<ILinkService, LinkService>();
            services.AddSingleton<IEasterEggService, EasterEggService>();
            services.AddSingleton<OfficialApiClient>();
            services.AddSingleton<MirrorChyanApiClient>();

            // 注册ViewModels（每次请求创建新实例）
            services.AddTransient<MainViewModel>();

            _serviceProvider = services.BuildServiceProvider();
        }

        /// <summary>
        /// 获取服务实例
        /// </summary>
        public static T GetService<T>()
        {
            if (_serviceProvider == null)
                throw new InvalidOperationException("服务容器未初始化，请先调用ConfigureServices()");

            var service = _serviceProvider.GetService<T>();
            if (service == null)
                throw new InvalidOperationException($"无法获取服务类型：{typeof(T).Name}");

            return service;
        }

        /// <summary>
        /// 尝试获取服务实例
        /// </summary>
        public static bool TryGetService<T>(out T? service) where T : class
        {
            if (_serviceProvider == null)
            {
                service = null;
                return false;
            }

            service = _serviceProvider.GetService<T>();
            return service != null;
        }

        /// <summary>
        /// 创建MainWindowViewModel并绑定到主窗口
        /// </summary>
        public static MainViewModel CreateMainViewModel(MainWindow mainWindow)
        {
            // 获取所有必需的服务
            var configService = GetService<IConfigService>();
            var pathService = GetService<IPathService>();
            var dialogService = GetService<IDialogService>();
            var downloadService = GetService<IDownloadService>();
            var fileUtilityService = GetService<IFileUtilityService>();
            var modInstallService = GetService<IModInstallService>();
            var modUninstallService = GetService<IModUninstallService>();
            var gachaSimulatorService = GetService<IGachaSimulatorService>();
            var nodeManagementService = GetService<INodeManagementService>();
            var versionService = GetService<IVersionService>();
            var navigationService = GetService<INavigationService>();
            var launcherService = GetService<ILauncherService>();
            var linkService = GetService<ILinkService>();

            // 创建MainViewModel实例（使用Transient，每次创建新实例）
            var viewModel = new MainViewModel(
                configService,
                pathService,
                dialogService,
                downloadService,
                fileUtilityService,
                modInstallService,
                modUninstallService,
                gachaSimulatorService,
                nodeManagementService,
                versionService,
                navigationService,
                launcherService,
                linkService);

            // 设置所有者窗口（用于窗口控制命令）
            viewModel.SetOwnerWindow(mainWindow);

            return viewModel;
        }
    }
}
