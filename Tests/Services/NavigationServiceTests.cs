using LLC_MOD_Toolbox.Services;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace LLC_MOD_Toolbox.Tests.Services
{
    public class NavigationServiceTests
    {
        [Fact]
        public void NavigateToInstallPage_ShouldShowGachaPage()
        {
            RunSta(() =>
            {
                var service = new NavigationService();
                var pages = CreatePages();
                var controls = CreateControls();

                service.InitializeNavigation(pages, controls);
                service.NavigateToInstallPageAsync("gacha").GetAwaiter().GetResult();

                Assert.Equal("install", service.CurrentPage);
                Assert.Equal("gacha", service.CurrentInstallPage);
                Assert.Equal(Visibility.Visible, pages["GachaPage"].Visibility);
                Assert.Equal(Visibility.Collapsed, pages["AutoInstallPage"].Visibility);
            });
        }

        [Fact]
        public void NavigateToSettings_ShouldShowSettingsPage()
        {
            RunSta(() =>
            {
                var service = new NavigationService();
                var pages = CreatePages();
                var controls = CreateControls();

                service.InitializeNavigation(pages, controls);
                service.NavigateToAsync("settings").GetAwaiter().GetResult();

                Assert.Equal("settings", service.CurrentPage);
                Assert.Equal(Visibility.Visible, pages["SettingsPage"].Visibility);
            });
        }

        [Fact]
        public void RefreshPageState_WithInstalling_ShouldToggleButtons()
        {
            RunSta(() =>
            {
                var service = new NavigationService();
                var pages = CreatePages();
                var controls = CreateControls();

                service.InitializeNavigation(pages, controls);
                service.IsInstalling = true;
                service.RefreshPageStateAsync().GetAwaiter().GetResult();

                Assert.Equal(Visibility.Collapsed, controls["AutoInstallStartButton"].Visibility);
                Assert.Equal(Visibility.Visible, controls["AutoInstallBTIng"].Visibility);
            });
        }

        private static Dictionary<string, Grid> CreatePages()
        {
            return new Dictionary<string, Grid>
            {
                ["AutoInstallPage"] = new Grid(),
                ["FontReplacePage"] = new Grid(),
                ["GachaPage"] = new Grid(),
                ["LinkPage"] = new Grid(),
                ["GreytestPage"] = new Grid(),
                ["SettingsPage"] = new Grid(),
                ["AboutPage"] = new Grid(),
                ["AnnouncementPage"] = new Grid(),
                ["EEPage"] = new Grid()
            };
        }

        private static Dictionary<string, FrameworkElement> CreateControls()
        {
            return new Dictionary<string, FrameworkElement>
            {
                ["CloseHover"] = new Image(),
                ["MinimizeHover"] = new Image(),
                ["AutoInstallHover"] = new Image(),
                ["FontReplaceHover"] = new Image(),
                ["ReplaceInstallHover"] = new Image(),
                ["AutoInstallBTHover"] = new Image(),
                ["AutoInstallDisabled"] = new Grid(),
                ["FontReplaceDisabled"] = new Grid(),
                ["GachaSimDisabled"] = new Grid(),
                ["AutoInstallButton"] = new Button(),
                ["FontReplaceButton"] = new Button(),
                ["GachaSimInstallButton"] = new Button(),
                ["AutoInstallStartButton"] = new Button(),
                ["AutoInstallBTIng"] = new Grid()
            };
        }

        private static void RunSta(Action action)
        {
            Exception? exception = null;
            var done = new ManualResetEvent(false);
            var thread = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
                finally
                {
                    done.Set();
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            done.WaitOne();

            if (exception != null)
            {
                throw new InvalidOperationException("STA test failed.", exception);
            }
        }
    }
}
