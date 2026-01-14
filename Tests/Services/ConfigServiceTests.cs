using LLC_MOD_Toolbox.Interfaces;
using LLC_MOD_Toolbox.Services;
using Moq;
using Xunit;
using System.IO;

namespace LLC_MOD_Toolbox.Tests.Services
{
    /// <summary>
    /// ConfigService单元测试
    /// 测试配置服务的加载、保存和更新功能
    /// </summary>
    public class ConfigServiceTests : IDisposable
    {
        private readonly string _testConfigPath;
        private readonly IConfigService _configService;

        public ConfigServiceTests()
        {
            // 设置测试配置文件路径
            _testConfigPath = Path.Combine(Path.GetTempPath(), $"test_config_{Guid.NewGuid()}.json");

            // 创建ConfigService实例
            _configService = new ConfigService(_testConfigPath);
        }

        [Fact]
        public void AppSettings_ShouldReturnValidConfig()
        {
            // Act
            var config = _configService.AppSettings;

            // Assert
            Assert.NotNull(config);
            Assert.NotNull(config.general);
            Assert.NotNull(config.install);
            Assert.NotNull(config.nodeSelect);
            Assert.NotNull(config.mirrorChyan);
        }

        [Fact]
        public void SaveConfig_ShouldCreateConfigFile()
        {
            // Act
            _configService.Save();

            // Assert
            Assert.True(File.Exists(_testConfigPath));
        }

        [Fact]
        public void SaveConfig_AndReload_ShouldPersistChanges()
        {
            // Arrange
            var originalPath = _configService.AppSettings.general.LCBPath;
            var newPath = @"C:\TestGamePath";

            // Act
            _configService.AppSettings.general.LCBPath = newPath;
            _configService.Save();

            // 重新加载配置
            var newConfigService = new ConfigService(_testConfigPath);

            // Assert
            Assert.Equal(newPath, newConfigService.AppSettings.general.LCBPath);

            // Cleanup
            _configService.AppSettings.general.LCBPath = originalPath;
        }

        [Fact]
        public void UpdateMirrorChyanEnable_ShouldUpdateConfig()
        {
            // Arrange
            var originalValue = _configService.AppSettings.mirrorChyan.enable;
            var newValue = !originalValue;

            // Act
            _configService.AppSettings.mirrorChyan.enable = newValue;
            _configService.Save();

            // Assert
            Assert.Equal(newValue, _configService.AppSettings.mirrorChyan.enable);

            // Cleanup
            _configService.AppSettings.mirrorChyan.enable = originalValue;
        }

        [Fact]
        public void UpdateDefaultNode_ShouldUpdateConfig()
        {
            // Arrange
            var originalNode = _configService.AppSettings.nodeSelect.defaultNode;
            var newNode = "TestNode";

            // Act
            _configService.AppSettings.nodeSelect.defaultNode = newNode;
            _configService.Save();

            // Assert
            Assert.Equal(newNode, _configService.AppSettings.nodeSelect.defaultNode);

            // Cleanup
            _configService.AppSettings.nodeSelect.defaultNode = originalNode ?? string.Empty;
        }

        public void Dispose()
        {
            // 清理测试文件
            if (File.Exists(_testConfigPath))
            {
                try
                {
                    File.Delete(_testConfigPath);
                }
                catch
                {
                    // 忽略删除失败
                }
            }
        }
    }
}
