using LLC_MOD_Toolbox.Services;
using Xunit;

namespace LLC_MOD_Toolbox.Tests.Services
{
    /// <summary>
    /// LauncherService单元测试
    /// 测试快捷方式创建功能
    /// </summary>
    public class LauncherServiceTests
    {
        private readonly LauncherService _launcherService;

        public LauncherServiceTests()
        {
            _launcherService = new LauncherService();
        }

        [Fact]
        public void ShortcutExists_WithNonExistentShortcut_ShouldReturnFalse()
        {
            // Act
            var exists = _launcherService.ShortcutExists("non_existent_test_shortcut.lnk");

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public void GetHotUpdateInstructions_ShouldReturnNonEmptyString()
        {
            // Act
            var instructions = _launcherService.GetHotUpdateInstructions();

            // Assert
            Assert.NotNull(instructions);
            Assert.NotEmpty(instructions);
            Assert.Contains("快捷方式启动", instructions);
            Assert.Contains("Steam启动", instructions);
        }

        [Fact]
        public void CreateDesktopShortcut_WithDefaultParameters_ShouldReturnValidPath()
        {
            // Arrange
            var testShortcutName = "test_llc_launcher.lnk";

            try
            {
                // Act
                var path = _launcherService.CreateDesktopShortcut(testShortcutName);

                // Assert
                Assert.NotNull(path);
                Assert.Contains("test_llc_launcher.lnk", path);
                Assert.True(_launcherService.ShortcutExists(testShortcutName));
            }
            finally
            {
                // Cleanup - 删除测试快捷方式
                var desktopPath = System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory),
                    testShortcutName);

                if (System.IO.File.Exists(desktopPath))
                {
                    System.IO.File.Delete(desktopPath);
                }
            }
        }

        [Fact]
        public void CreateDesktopShortcut_WithCustomArguments_ShouldUseProvidedArguments()
        {
            // Arrange
            var testShortcutName = "test_llc_custom_args.lnk";
            var customArguments = "-custom -test";

            try
            {
                // Act
                var path = _launcherService.CreateDesktopShortcut(
                    testShortcutName,
                    customArguments,
                    "Test Description");

                // Assert
                Assert.NotNull(path);
                Assert.True(_launcherService.ShortcutExists(testShortcutName));
            }
            finally
            {
                // Cleanup
                var desktopPath = System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory),
                    testShortcutName);

                if (System.IO.File.Exists(desktopPath))
                {
                    System.IO.File.Delete(desktopPath);
                }
            }
        }
    }
}
