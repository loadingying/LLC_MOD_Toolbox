using LLC_MOD_Toolbox.Services;
using Xunit;
using System.IO;

namespace LLC_MOD_Toolbox.Tests.Services
{
    /// <summary>
    /// ModUninstallService单元测试
    /// </summary>
    public class ModUninstallServiceTests
    {
        private readonly ModUninstallService _uninstallService;
        private readonly string _testDirectory;

        public ModUninstallServiceTests()
        {
            _uninstallService = new ModUninstallService();
            _testDirectory = Path.Combine(Path.GetTempPath(), "LLC_MOD_Toolbox_Tests");
        }

        public void Dispose()
        {
            // 清理测试目录
            if (Directory.Exists(_testDirectory))
            {
                try
                {
                    Directory.Delete(_testDirectory, true);
                }
                catch
                {
                    // 忽略清理失败
                }
            }
        }

        [Fact]
        public void DeleteLanguagePack_WithNonExistentDirectory_ShouldNotThrow()
        {
            // Act & Assert - should not throw
            _uninstallService.DeleteLanguagePack("nonexistent_directory");
        }

        [Fact]
        public void DeleteBepInEx_WithNonExistentDirectory_ShouldNotThrow()
        {
            // Act & Assert - should not throw
            _uninstallService.DeleteBepInEx("nonexistent_directory");
        }

        [Fact]
        public void DeleteMelonLoader_WithNonExistentDirectory_ShouldNotThrow()
        {
            // Act & Assert - should not throw
            _uninstallService.DeleteMelonLoader("nonexistent_directory");
        }

        [Fact]
        public async Task UninstallAllAsync_WithNonExistentDirectory_ShouldNotThrow()
        {
            // Act & Assert - should not throw
            await _uninstallService.UninstallAllAsync("nonexistent_directory");
        }

        [Fact]
        public async Task UninstallAllAsync_WithTestDirectory_ShouldComplete()
        {
            // Arrange
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
            Directory.CreateDirectory(_testDirectory);

            // 创建测试文件和目录
            Directory.CreateDirectory(Path.Combine(_testDirectory, "LimbusCompany_Data", "Lang", "LLC_zh-CN"));
            Directory.CreateDirectory(Path.Combine(_testDirectory, "BepInEx"));
            Directory.CreateDirectory(Path.Combine(_testDirectory, "MelonLoader"));
            File.Create(Path.Combine(_testDirectory, "doorstop_config.ini")).Dispose();

            // Act
            await _uninstallService.UninstallAllAsync(_testDirectory);

            // Assert
            Assert.True(Directory.Exists(_testDirectory)); // 根目录应该仍然存在
            Assert.False(Directory.Exists(Path.Combine(_testDirectory, "LimbusCompany_Data", "Lang", "LLC_zh-CN")));
            Assert.False(Directory.Exists(Path.Combine(_testDirectory, "BepInEx")));
            Assert.False(Directory.Exists(Path.Combine(_testDirectory, "MelonLoader")));
            Assert.False(File.Exists(Path.Combine(_testDirectory, "doorstop_config.ini")));
        }
    }
}
