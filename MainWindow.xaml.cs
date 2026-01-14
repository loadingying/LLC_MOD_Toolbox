// 此文件用来处理前端样式相关逻辑。
// 我恨XML，这辈子都不想写XML了。
// （而且内存占用好多

using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LLC_MOD_Toolbox.Interfaces;
using LLC_MOD_Toolbox.Services;
using LLC_MOD_Toolbox.ViewModels;

namespace LLC_MOD_Toolbox
{

    public partial class MainWindow : Window
    {
        /// <summary>
        /// MainViewModel实例（MVVM架构）
        /// </summary>
        public MainViewModel? ViewModel { get; private set; }

        // 服务层引用
        private INavigationService? _navigationService;
        private IUIService? _uiService;
        private IProgressService? _progressService;
        private ILinkService? _linkService;
        private IEasterEggService? _easterEggService;

        private static bool isInstalling = false;

        /// <summary>
        /// 初始化服务层（优化版 - 延迟加载非关键服务）
        /// </summary>
        public void InitializeServices()
        {
            // 立即初始化关键服务
            _navigationService = ServiceLocator.GetService<INavigationService>();
            _uiService = ServiceLocator.GetService<IUIService>();
            _progressService = ServiceLocator.GetService<IProgressService>();

            // 同步isInstalling状态
            if (ViewModel != null && _navigationService != null)
            {
                _navigationService.IsInstalling = ViewModel.IsInstalling;
            }

            // 初始化NavigationService的页面和控件字典
            var pages = new Dictionary<string, Grid>
            {
                { "AutoInstallPage", AutoInstallPage },
                { "FontReplacePage", FontReplacePage },
                { "LinkPage", LinkPage },
                { "GreytestPage", GreytestPage },
                { "SettingsPage", SettingsPage },
                { "AboutPage", AboutPage },
                { "EEPage", EEPage },
                { "GachaPage", GachaPage },
                { "AnnouncementPage", AnnouncementPage }
            };

            var controls = new Dictionary<string, FrameworkElement>
            {
                { "CloseHover", CloseHover },
                { "MinimizeHover", MinimizeHover },
                { "AutoInstallHover", AutoInstallHover },
                { "FontReplaceHover", FontReplaceHover },
                { "ReplaceInstallHover", ReplaceInstallHover },
                { "AutoInstallBTHover", AutoInstallBTHover },
                { "AutoInstallDisabled", AutoInstallDisabled },
                { "FontReplaceDisabled", FontReplaceDisabled },
                { "GachaSimDisabled", GachaSimDisabled },
                { "AutoInstallButton", AutoInstallButton },
                { "FontReplaceButton", FontReplaceButton },
                { "GachaSimInstallButton", GachaSimInstallButton },
                { "AutoInstallStartButton", AutoInstallStartButton },
                { "AutoInstallBTIng", AutoInstallBTIng }
            };

            _navigationService.InitializeNavigation(pages, controls);

            Log.logger.Info("关键服务初始化完成（非关键服务将延迟加载）");
        }

        /// <summary>
        /// 延迟初始化非关键服务（按需加载）
        /// </summary>
        private void EnsureLinkService()
        {
            if (_linkService == null)
            {
                _linkService = ServiceLocator.GetService<ILinkService>();
                _linkService.InitializeDefaultLinks();
                Log.logger.Debug("LinkService延迟初始化完成");
            }
        }

        /// <summary>
        /// 延迟初始化彩蛋服务（按需加载）
        /// </summary>
        private void EnsureEasterEggService()
        {
            if (_easterEggService == null)
            {
                _easterEggService = ServiceLocator.GetService<IEasterEggService>();
                Log.logger.Debug("EasterEggService延迟初始化完成");
            }
        }

        /// <summary>
        /// 刷新页面状态（代理到NavigationService）
        /// </summary>
        public async Task RefreshPage()
        {
            if (_navigationService != null)
            {
                await _navigationService.RefreshPageStateAsync();
            }
        }
        /// <summary>
        /// 处理窗口拖拽。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowDragMove(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        /// <summary>
        /// 拖拽时更改指针为拖拽样式。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Cursor = System.Windows.Input.Cursors.ScrollAll;
        }
        /// <summary>
        /// 拖拽结束恢复指针样式。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Cursor = System.Windows.Input.Cursors.Arrow;
        }
        /// <summary>
        /// 处理最小化按钮。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MinimizeButtonClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        /// <summary>
        /// 处理关闭按钮。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
        /// <summary>
        /// 处理自动安装按钮。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AutoInstallButtonClick(object sender, RoutedEventArgs e)
        {
            if (_navigationService != null)
            {
                await _navigationService.NavigateToInstallPageAsync("auto");
            }
        }
        private async void FontReplaceButtonClick(object sender, RoutedEventArgs e)
        {
            if (_navigationService != null)
            {
                await _navigationService.NavigateToInstallPageAsync("font");
            }
        }
        private async void GachaSimButtonClick(object sender, RoutedEventArgs e)
        {
            if (!isInitGacha)
            {
                bool messageBoxResult = UniversalDialog.ShowConfirm("本抽卡模拟器资源来源自维基，可能信息更新不准时。\n本模拟器 不 会 对您的游戏数据造成任何影响。\n若您已知悉，请点击【确定】进行初始化。", "提示", this);
                if (messageBoxResult)
                {
                    await InitGacha();
                    if (_navigationService != null)
                    {
                        await _navigationService.NavigateToInstallPageAsync("gacha");
                    }
                }
            }
            else
            {
                if (_navigationService != null)
                {
                    await _navigationService.NavigateToInstallPageAsync("gacha");
                }
            }
        }
        /// <summary>
        /// 处理安装选项按钮。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void InstallOptionClick(object sender, RoutedEventArgs e)
        {
            if (_navigationService != null)
            {
                await _navigationService.NavigateToInstallPageAsync("auto");
            }
        }
        /// <summary>
        /// 处理配置选项按钮。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void LinkOptionClick(object sender, RoutedEventArgs e)
        {
            if (_navigationService != null)
            {
                await _navigationService.NavigateToAsync("link");
            }
        }
        private async void GreytestOptionClick(object sender, RoutedEventArgs e)
        {
            if (_navigationService != null)
            {
                await _navigationService.NavigateToAsync("greytest");
            }
        }
        private async void SettingsOptionClick(object sender, RoutedEventArgs e)
        {
            if (_navigationService != null)
            {
                await _navigationService.NavigateToAsync("settings");
            }
        }
        private async void AboutOptionClick(object sender, RoutedEventArgs e)
        {
            if (_navigationService != null)
            {
                await _navigationService.NavigateToAsync("about");
            }
        }
        private async void EEOptionClick(object sender, RoutedEventArgs e)
        {
            if (_navigationService != null)
            {
                await _navigationService.NavigateToAsync("ee");
            }
        }
        public async Task ChangeProgressValue(float value)
        {
            if (_progressService != null)
            {
                _progressService.SetProgress(value);
            }

            value = (float)Math.Round(value, 1);
            Log.logger.Debug("安装进度：" + value + "%");
            await this.Dispatcher.BeginInvoke(() =>
            {
                RectangleGeometry rectGeometry = new()
                {
                    Rect = new Rect(0, 0, 6.24 * value, 50)
                };
                FullProgress.Clip = rectGeometry;
            });
            Log.logger.Debug("更改进度完成。");
        }
        private async void GreytestInfoButtonClick(object sender, RoutedEventArgs e)
        {
            if (_uiService != null)
            {
                await _uiService.OpenUrlAsync("https://www.zeroasso.top/docs/community/llcdev");
            }
        }
        #region 彩蛋
        public async Task ChangeEEVB(bool b)
        {
            EnsureEasterEggService();
            if (_easterEggService != null)
            {
                await _easterEggService.SetOptionVisibilityAsync(b);
            }

            await this.Dispatcher.BeginInvoke(() =>
            {
                if (b)
                {
                    EEOption.Visibility = Visibility.Visible;
                    EEOption.IsHitTestVisible = true;
                }
                else
                {
                    EEOption.Visibility = Visibility.Collapsed;
                    EEOption.IsHitTestVisible = false;
                }
            });
        }
        public async Task ChangeEEPic()
        {
            EnsureEasterEggService();
            if (_easterEggService == null)
            {
                return;
            }

            var bytes = await _easterEggService.LoadEasterEggImageAsync(
                configuation.Settings.general.internationalMode);

            if (bytes != null)
            {
                try
                {
                    using (var stream = new MemoryStream(bytes))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = stream;
                        bitmap.EndInit();
                        await this.Dispatcher.BeginInvoke(() =>
                        {
                            EEPageImage.Source = bitmap;
                        });
                    }
                }
                catch (Exception ex)
                {
                    Log.logger.Error("显示彩蛋图片失败：" + ex.Message);
                }
            }
        }
        #endregion
        #region 链接
        private async Task<string?> GetSenderName(System.Windows.Controls.Control? control)
        {
            if (control != null)
            {
                string name = string.Empty;
                await this.Dispatcher.BeginInvoke(() =>
                {
                    name = control.Name;
                });
                return name;
            }
            else
            {
                return string.Empty;
            }
        }
        private async void LinkButtonClick(object sender, RoutedEventArgs e)
        {
            EnsureLinkService();
            if (sender != null && _uiService != null && _linkService != null)
            {
                string name = await GetSenderName(sender as System.Windows.Controls.Control);
                var url = _linkService.GetUrl(name);
                if (!string.IsNullOrEmpty(url))
                {
                    await _uiService.OpenUrlAsync(url);
                }
            }
        }

        #endregion
        public async Task ChangeAutoInstallButton()
        {
            Log.logger.Debug("更改自动安装模组。");
            await this.Dispatcher.BeginInvoke(() =>
            {
                AutoInstallStartButtonIMG.Source = BitmapFrame.Create(new Uri("pack://application:,,,/Picture/Update.png"), BitmapCreateOptions.None, BitmapCacheOption.Default);
                AutoInstallBTHover.Source = BitmapFrame.Create(new Uri("pack://application:,,,/Picture/UpdateHover.png"), BitmapCreateOptions.None, BitmapCacheOption.Default);
            });
        }
        public async Task DisableGlobalOperations()
        {
            await this.Dispatcher.BeginInvoke(() =>
            {
                AutoInstallStartButton.IsHitTestVisible = false;
                OverlayGrid.Visibility = Visibility.Visible;
            });
            if (_uiService != null)
            {
                await _uiService.DisableGlobalOperationsAsync();
            }
        }

        public async Task EnableGlobalOperations()
        {
            await this.Dispatcher.BeginInvoke(() =>
            {
                AutoInstallStartButton.IsHitTestVisible = true;
                OverlayGrid.Visibility = Visibility.Collapsed;
            });
            if (_uiService != null)
            {
                await _uiService.EnableGlobalOperationsAsync();
            }
        }
        public async Task ChangeLeftButtonStatu(bool statu)
        {
            await this.Dispatcher.BeginInvoke(() =>
            {
                InstallOption.IsHitTestVisible = statu;
                LinkOption.IsHitTestVisible = statu;
                GreytestOption.IsHitTestVisible = statu;
                SettingOption.IsHitTestVisible = statu;
                AboutOption.IsHitTestVisible = statu;
                EEOption.IsHitTestVisible = statu;
                AutoInstallButton.IsHitTestVisible = statu;
                GachaSimInstallButton.IsHitTestVisible = statu;
            });
        }
        public async Task ChangeAnnoTip(int num)
        {
            await this.Dispatcher.BeginInvoke(() =>
            {
                AnnoucementButtonTip.Text = "由于本次公告较为重要，您需要继续阅读" + num + "秒。";
            });
        }
        public async Task ChangeAnnoText(string text)
        {
            await this.Dispatcher.BeginInvoke(() =>
            {
                AnnoucementText.Text = text;
            });
        }
        public async Task AnnoCountEnd()
        {
            await this.Dispatcher.BeginInvoke(() =>
            {
                AnnoucementButton.IsHitTestVisible = true;
                AnnoucementButtonTip.Visibility = Visibility.Collapsed;
            });
        }
        public async Task AlreadyReadAnno()
        {
            await ChangeLeftButtonStatu(true);
            if (_navigationService != null)
            {
                await _navigationService.NavigateToInstallPageAsync("auto");
            }
            if (configuation.Settings.install.installWhenLaunch || isLauncherMode)
            {
                // 已在MainWindowWD中通过ViewModel.InstallCommand处理
            }
        }
        public async Task ChangeLoadingText(string text)
        {
            await this.Dispatcher.BeginInvoke(() =>
            {
                LoadingText.Text = text;
            });
        }

        #region MVVM支持
        // ViewModel初始化已移至MainWindowWD.xaml.cs
        #endregion
    }
}
