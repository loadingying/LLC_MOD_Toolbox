using LLC_MOD_Toolbox.Interfaces;
using LLC_MOD_Toolbox.Services;
using Moq;
using Xunit;
using System;
using System.IO;

namespace LLC_MOD_Toolbox.Tests.Services
{
    /// <summary>
    /// PathService单元测试
    /// 测试路径服务的验证、查找等功能
    /// </summary>
    public class PathServiceTests
    {
        private readonly Mock<IConfigService> _mockConfigService;
        private readonly IPathService _pathService;

        public PathServiceTests()
        {
            _mockConfigService = new Mock<IConfigService>();
            _pathService = new PathService();
        }

        [Fact]
        public void GameDirectory_ShouldReturnEmpty_Initially()
        {
            // Act
            var gameDir = _pathService.GameDirectory;

            // Assert
            Assert.Null(gameDir);
        }

        [Fact]
        public void SetGameDirectory_ShouldUpdateValue()
        {
            // Arrange
            var testPath = CreateTempGameDirectory(out var cleanup);

            try
            {
                // Act
                _pathService.GameDirectory = testPath;

                // Assert
                Assert.Equal(testPath, _pathService.GameDirectory);
            }
            finally
            {
                cleanup();
            }
        }

        [Fact]
        public void ValidateGamePath_WithInvalidPath_ShouldReturnFalse()
        {
            // Arrange
            var invalidPath = @"C:\InvalidGamePath\That\Does\Not\Exist";

            // Act
            var isValid = _pathService.ValidateGamePath(invalidPath);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void ChineseLangDirectory_ShouldReturnCorrectPath()
        {
            // Arrange
            var gameDir = CreateTempGameDirectory(out var cleanup);

            try
            {
                _pathService.GameDirectory = gameDir;

                // Act
                var langDir = _pathService.ChineseLangDirectory;

                // Assert
                Assert.Equal(Path.Combine(gameDir, "LimbusCompany_Data", "Lang", "LLC_zh-CN"), langDir);
            }
            finally
            {
                cleanup();
            }
        }

        [Fact]
        public void GameDataDirectory_ShouldReturnCorrectPath()
        {
            // Arrange
            var gameDir = CreateTempGameDirectory(out var cleanup);

            try
            {
                _pathService.GameDirectory = gameDir;

                // Act
                var dataDir = _pathService.GameDataDirectory;

                // Assert
                Assert.Equal(Path.Combine(gameDir, "LimbusCompany_Data"), dataDir);
            }
            finally
            {
                cleanup();
            }
        }

        [Fact]
        public void GameExecutablePath_ShouldReturnCorrectPath()
        {
            // Arrange
            var gameDir = CreateTempGameDirectory(out var cleanup);

            try
            {
                _pathService.GameDirectory = gameDir;

                // Act
                var exePath = _pathService.GameExecutablePath;

                // Assert
                Assert.Equal(Path.Combine(gameDir, "LimbusCompany.exe"), exePath);
            }
            finally
            {
                cleanup();
            }
        }

        private static string CreateTempGameDirectory(out Action cleanup)
        {
            var root = Path.Combine(Path.GetTempPath(), $"llc_game_{Guid.NewGuid()}");
            var dataDir = Path.Combine(root, "LimbusCompany_Data");
            Directory.CreateDirectory(dataDir);
            File.WriteAllText(Path.Combine(root, "LimbusCompany.exe"), string.Empty);
            cleanup = () =>
            {
                if (Directory.Exists(root))
                {
                    Directory.Delete(root, true);
                }
            };
            return root;
        }
    }
}
