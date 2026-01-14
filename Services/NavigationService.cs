using LLC_MOD_Toolbox.Interfaces;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace LLC_MOD_Toolbox.Services
{
    /// <summary>
    /// 页面导航服务实现
    /// 负责管理页面切换和UI状态更新
    /// </summary>
    public class NavigationService : INavigationService
    {
        private string _currentPage = "install";
        private string _currentInstallPage = "auto";
        private bool _isInstalling = false;

        private Dictionary<string, Grid> _pages = new();
        private Dictionary<string, FrameworkElement> _controls = new();
        private Dispatcher? _dispatcher;

        /// <summary>
        /// 导航到设置页面时的事件（用于刷新Mirror酱UI状态）
        /// </summary>
        public event Func<Task>? OnNavigatedToSettings;

        /// <summary>
        /// 当前主页面
        /// </summary>
        public string CurrentPage
        {
            get => _currentPage;
            private set => _currentPage = value;
        }

        /// <summary>
        /// 当前安装子页面
        /// </summary>
        public string CurrentInstallPage
        {
            get => _currentInstallPage;
            private set => _currentInstallPage = value;
        }

        /// <summary>
        /// 是否正在安装
        /// </summary>
        public bool IsInstalling
        {
            get => _isInstalling;
            set => _isInstalling = value;
        }

        /// <summary>
        /// 初始化导航服务
        /// </summary>
        public void InitializeNavigation(Dictionary<string, Grid> pages, Dictionary<string, FrameworkElement> controls)
        {
            _pages = pages ?? throw new ArgumentNullException(nameof(pages));
            _controls = controls ?? throw new ArgumentNullException(nameof(controls));

            // 获取Dispatcher
            if (_pages.Count > 0)
            {
                _dispatcher = _pages.First().Value.Dispatcher;
            }
        }

        /// <summary>
        /// 导航到指定页面
        /// </summary>
        public async Task NavigateToAsync(string pageName)
        {
            CurrentPage = pageName;
            await RefreshPageStateAsync();

            // 如果导航到设置页面，触发事件以刷新Mirror酱UI状态
            if (pageName == "settings" && OnNavigatedToSettings != null)
            {
                await OnNavigatedToSettings.Invoke();
            }

            Log.logger.Info($"导航到页面：{pageName}");
        }

        /// <summary>
        /// 导航到安装子页面
        /// </summary>
        public async Task NavigateToInstallPageAsync(string installPageName)
        {
            CurrentPage = "install";
            CurrentInstallPage = installPageName;
            await RefreshPageStateAsync();
            Log.logger.Info($"导航到安装子页面：{installPageName}");
        }

        /// <summary>
        /// 刷新页面状态
        /// </summary>
        public async Task RefreshPageStateAsync()
        {
            if (_dispatcher == null)
                return;

            if (_dispatcher.CheckAccess())
            {
                ApplyPageState();
                return;
            }

            await _dispatcher.InvokeAsync(() =>
            {
                ApplyPageState();
            });
        }

        private void ApplyPageState()
        {
            // 隐藏所有Hover效果
            SetHoverOpacity("CloseHover", 0);
            SetHoverOpacity("MinimizeHover", 0);
            SetHoverOpacity("AutoInstallHover", 0);
            SetHoverOpacity("FontReplaceHover", 0);
            SetHoverOpacity("ReplaceInstallHover", 0);
            SetHoverOpacity("AutoInstallBTHover", 0);

            // 控制面板按钮显示/隐藏
            bool isInInstallPage = CurrentPage == "install";
            SetControlVisibility("AutoInstallDisabled", !isInInstallPage);
            SetControlVisibility("FontReplaceDisabled", !isInInstallPage);
            SetControlVisibility("GachaSimDisabled", !isInInstallPage);
            SetControlVisibility("AutoInstallButton", isInInstallPage);
            SetControlVisibility("FontReplaceButton", isInInstallPage);
            SetControlVisibility("GachaSimInstallButton", isInInstallPage);
            SetControlEnabled("AutoInstallButton", isInInstallPage);
            SetControlEnabled("FontReplaceButton", isInInstallPage);

            // 控制安装按钮状态
            SetControlVisibility("AutoInstallStartButton", !IsInstalling);
            SetControlEnabled("AutoInstallStartButton", !IsInstalling);
            SetControlVisibility("AutoInstallBTIng", IsInstalling);

            // 显示对应的页面
            ShowCurrentPage();
        }

        /// <summary>
        /// 设置页面可见性
        /// </summary>
        public async Task SetPageVisibilityAsync(string pageName, bool visible)
        {
            if (_dispatcher == null)
                return;

            if (_dispatcher.CheckAccess())
            {
                SetPageVisibility(pageName, visible);
                return;
            }

            await _dispatcher.InvokeAsync(() =>
            {
                SetPageVisibility(pageName, visible);
            });
        }

        /// <summary>
        /// 设置控件可见性
        /// </summary>
        public async Task SetControlVisibilityAsync(string controlName, bool visible)
        {
            if (_dispatcher == null)
                return;

            if (_dispatcher.CheckAccess())
            {
                SetControlVisibility(controlName, visible);
                return;
            }

            await _dispatcher.InvokeAsync(() =>
            {
                SetControlVisibility(controlName, visible);
            });
        }

        /// <summary>
        /// 设置控件启用状态
        /// </summary>
        public async Task SetControlEnabledAsync(string controlName, bool enabled)
        {
            if (_dispatcher == null)
                return;

            if (_dispatcher.CheckAccess())
            {
                SetControlEnabled(controlName, enabled);
                return;
            }

            await _dispatcher.InvokeAsync(() =>
            {
                SetControlEnabled(controlName, enabled);
            });
        }

        #region 私有辅助方法

        /// <summary>
        /// 显示当前页面
        /// </summary>
        private void ShowCurrentPage()
        {
            string targetPageName = CurrentPage switch
            {
                "install" => CurrentInstallPage switch
                {
                    "auto" => "AutoInstallPage",
                    "font" => "FontReplacePage",
                    "gacha" => "GachaPage",
                    _ => "AutoInstallPage"
                },
                "link" => "LinkPage",
                "greytest" => "GreytestPage",
                "settings" => "SettingsPage",
                "about" => "AboutPage",
                "anno" => "AnnouncementPage",
                "ee" => "EEPage",
                _ => "AutoInstallPage"
            };

            // 隐藏所有页面
            foreach (var page in _pages.Values)
            {
                page.Visibility = Visibility.Collapsed;
            }

            // 显示目标页面
            if (_pages.TryGetValue(targetPageName, out var targetPage))
            {
                targetPage.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// 设置控件可见性（同步）
        /// </summary>
        private void SetControlVisibility(string controlName, bool visible)
        {
            if (_controls.TryGetValue(controlName, out var control))
            {
                control.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 设置控件启用状态（同步）
        /// </summary>
        private void SetControlEnabled(string controlName, bool enabled)
        {
            if (_controls.TryGetValue(controlName, out var control))
            {
                if (control is Button button)
                {
                    button.IsHitTestVisible = enabled;
                }
            }
        }

        /// <summary>
        /// 设置Hover透明度
        /// </summary>
        private void SetHoverOpacity(string hoverName, double opacity)
        {
            if (_controls.TryGetValue(hoverName, out var control))
            {
                if (control is System.Windows.Controls.Image image)
                {
                    image.Opacity = opacity;
                }
            }
        }

        private void SetPageVisibility(string pageName, bool visible)
        {
            if (_pages.TryGetValue(pageName, out var page))
            {
                page.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        #endregion
    }
}
