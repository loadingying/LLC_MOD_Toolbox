using LLC_MOD_Toolbox.Interfaces;
using LLC_MOD_Toolbox.Models;
using LLC_MOD_Toolbox.Services;
using Moq;
using System.IO;
using Xunit;

namespace LLC_MOD_Toolbox.Tests.Services
{
    public class InstallServiceTests
    {
        [Fact]
        public async System.Threading.Tasks.Task ValidateEnvironmentAsync_WithoutGamePath_ShouldFail()
        {
            var service = CreateService(pathService =>
            {
                pathService.SetupGet(p => p.GameDirectory).Returns((string?)null);
            });

            var result = await service.ValidateEnvironmentAsync();

            Assert.False(result.IsValid);
            Assert.Contains("未设置游戏路径", result.Errors);
        }

        [Fact]
        public async System.Threading.Tasks.Task ValidateEnvironmentAsync_WithInvalidPath_ShouldFail()
        {
            var service = CreateService(pathService =>
            {
                pathService.SetupGet(p => p.GameDirectory).Returns(@"C:\Invalid");
                pathService.Setup(p => p.ValidateGamePath(It.IsAny<string>())).Returns(false);
            });

            var result = await service.ValidateEnvironmentAsync();

            Assert.False(result.IsValid);
            Assert.Contains("游戏路径无效", result.Errors);
        }

        [Fact]
        public async System.Threading.Tasks.Task ValidateEnvironmentAsync_WithLegacyFiles_ShouldWarn()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), $"llc_install_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);
            File.WriteAllText(Path.Combine(tempDir, "version.dll"), string.Empty);
            File.WriteAllText(Path.Combine(tempDir, "winhttp.dll"), string.Empty);

            try
            {
                var service = CreateService(pathService =>
                {
                    pathService.SetupGet(p => p.GameDirectory).Returns(tempDir);
                    pathService.Setup(p => p.ValidateGamePath(tempDir)).Returns(true);
                });

                var result = await service.ValidateEnvironmentAsync();

                Assert.True(result.IsValid);
                Assert.Contains("检测到MelonLoader框架（已过时），建议卸载", result.Warnings);
                Assert.Contains("检测到BepInEx框架（旧版本），建议卸载", result.Warnings);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        private static InstallService CreateService(Action<Mock<IPathService>> setupPath)
        {
            var downloadService = new Mock<IDownloadService>();
            var pathService = new Mock<IPathService>();
            var configService = new Mock<IConfigService>();
            var dialogService = new Mock<IDialogService>();

            setupPath(pathService);

            configService.SetupGet(c => c.AppSettings).Returns(new AppSettings());

            return new InstallService(
                downloadService.Object,
                pathService.Object,
                configService.Object,
                dialogService.Object);
        }
    }
}
