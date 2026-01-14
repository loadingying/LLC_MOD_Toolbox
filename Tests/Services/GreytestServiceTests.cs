using LLC_MOD_Toolbox.Interfaces;
using LLC_MOD_Toolbox.Services;
using Xunit;

namespace LLC_MOD_Toolbox.Tests.Services
{
    /// <summary>
    /// GreytestService单元测试
    /// 测试灰度测试Token验证和状态管理
    /// </summary>
    public class GreytestServiceTests
    {
        private readonly IGreytestService _greytestService;

        public GreytestServiceTests()
        {
            _greytestService = new GreytestService();
        }

        [Fact]
        public void IsEnabled_Initially_ShouldReturnFalse()
        {
            // Act
            var isEnabled = _greytestService.IsEnabled;

            // Assert
            Assert.False(isEnabled);
        }

        [Fact]
        public void DownloadUrl_Initially_ShouldBeNull()
        {
            // Act
            var url = _greytestService.DownloadUrl;

            // Assert
            Assert.Null(url);
        }

        [Fact]
        public void CurrentToken_Initially_ShouldBeNull()
        {
            // Act
            var token = _greytestService.CurrentToken;

            // Assert
            Assert.Null(token);
        }

        [Fact]
        public void Enable_ShouldSetIsEnabledToTrue()
        {
            // Act
            _greytestService.Enable("test_token", "http://example.com/test.7z");

            // Assert
            Assert.True(_greytestService.IsEnabled);
            Assert.Equal("test_token", _greytestService.CurrentToken);
            Assert.Equal("http://example.com/test.7z", _greytestService.DownloadUrl);
        }

        [Fact]
        public void Disable_ShouldSetIsEnabledToFalse()
        {
            // Arrange
            _greytestService.Enable("test_token", "http://example.com/test.7z");

            // Act
            _greytestService.Disable();

            // Assert
            Assert.False(_greytestService.IsEnabled);
            Assert.Null(_greytestService.CurrentToken);
            Assert.Null(_greytestService.DownloadUrl);
        }

        [Fact]
        public async Task ValidateTokenAsync_WithEmptyToken_ShouldReturnInvalid()
        {
            // Act
            var result = await _greytestService.ValidateTokenAsync("", "https://api.example.com/{0}");

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Token为空", result.ErrorMessage);
        }

        [Fact]
        public async Task ValidateTokenAsync_WithNullToken_ShouldReturnInvalid()
        {
            // Act
            var result = await _greytestService.ValidateTokenAsync(null!, "https://api.example.com/{0}");

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Token为空", result.ErrorMessage);
        }
    }
}
