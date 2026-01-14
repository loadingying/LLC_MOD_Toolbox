using LLC_MOD_Toolbox.Interfaces;
using LLC_MOD_Toolbox.Services;
using Xunit;

namespace LLC_MOD_Toolbox.Tests.Services
{
    /// <summary>
    /// VersionService单元测试
    /// 测试版本检查功能
    /// </summary>
    public class VersionServiceTests
    {
        private readonly VersionService _versionService;

        public VersionServiceTests()
        {
            _versionService = new VersionService();
        }

        [Fact]
        public void FormatVersionText_WithBothVersionsValid_ShouldReturnCorrectText()
        {
            // Arrange
            int currentVersion = 12345;
            int latestVersion = 12346;

            // Act
            var info = _versionService.FormatVersionText(currentVersion, latestVersion);

            // Assert
            Assert.Equal("当前版本：12345（可更新）", info.CurrentVersionText);
            Assert.Equal("最新版本：12346", info.LatestVersionText);
        }

        [Fact]
        public void FormatVersionText_WithSameVersions_ShouldReturnNoUpdateText()
        {
            // Arrange
            int currentVersion = 12345;
            int latestVersion = 12345;

            // Act
            var info = _versionService.FormatVersionText(currentVersion, latestVersion);

            // Assert
            Assert.Equal("当前版本：12345", info.CurrentVersionText);
            Assert.Equal("最新版本：12345", info.LatestVersionText);
        }

        [Fact]
        public void FormatVersionText_WithCurrentVersionZero_ShouldReturnNotInstalled()
        {
            // Arrange
            int currentVersion = 0;
            int latestVersion = 12345;

            // Act
            var info = _versionService.FormatVersionText(currentVersion, latestVersion);

            // Assert
            Assert.Equal("当前版本：未安装", info.CurrentVersionText);
            Assert.Equal("最新版本：12345", info.LatestVersionText);
        }

        [Fact]
        public void FormatVersionText_WithLatestVersionFailed_ShouldReturnErrorText()
        {
            // Arrange
            int currentVersion = 12345;
            int latestVersion = -100;

            // Act
            var info = _versionService.FormatVersionText(currentVersion, latestVersion);

            // Assert
            Assert.Equal("当前版本：12345", info.CurrentVersionText);
            Assert.Equal("最新版本：获取失败", info.LatestVersionText);
        }

        [Fact]
        public void FormatVersionText_WithCurrentVersionFailed_ShouldReturnParseError()
        {
            // Arrange
            int currentVersion = -1;
            int latestVersion = 12345;

            // Act
            var info = _versionService.FormatVersionText(currentVersion, latestVersion);

            // Assert
            Assert.Equal("当前版本：解析失败", info.CurrentVersionText);
            Assert.Equal("最新版本：12345", info.LatestVersionText);
        }

        [Fact]
        public async Task GetCurrentVersionAsync_WithNonExistentDirectory_ShouldReturnZero()
        {
            // Arrange
            string nonExistentDir = "C:\\NonExistent\\LimbusCompany";

            // Act
            var version = await _versionService.GetCurrentVersionAsync(nonExistentDir);

            // Assert
            Assert.Equal(0, version);
        }

        [Fact]
        public void FormatVersionText_WithBothVersionsFailed_ShouldReturnErrorText()
        {
            // Arrange
            int currentVersion = -1;
            int latestVersion = -100;

            // Act
            var info = _versionService.FormatVersionText(currentVersion, latestVersion);

            // Assert
            Assert.Equal("当前版本：解析失败", info.CurrentVersionText);
            Assert.Equal("最新版本：获取失败", info.LatestVersionText);
        }
    }
}
