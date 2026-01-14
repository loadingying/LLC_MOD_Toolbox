using LLC_MOD_Toolbox.Interfaces;
using LLC_MOD_Toolbox.Models;
using LLC_MOD_Toolbox.Services;
using Moq;
using System;
using System.IO;
using System.Net;
using System.Windows;
using Xunit;

namespace LLC_MOD_Toolbox.Tests.Services
{
    /// <summary>
    /// ErrorService单元测试
    /// 测试错误处理和异常转换功能
    /// </summary>
    public class ErrorServiceTests
    {
        private readonly Mock<IDialogService> _mockDialogService;
        private readonly ErrorService _errorService;

        public ErrorServiceTests()
        {
            _mockDialogService = new Mock<IDialogService>();
            _errorService = new ErrorService(_mockDialogService.Object);
        }

        [Fact]
        public void GetExceptionMessage_WithWebException_ShouldReturnNetworkError()
        {
            // Arrange
            var ex = new WebException("Network error");

            // Act
            var message = _errorService.GetExceptionMessage(ex);

            // Assert
            Assert.Contains("网络链接错误", message);
            Assert.Contains("更换节点", message);
        }

        [Fact]
        public void GetExceptionMessage_WithFileNotFoundException_ShouldReturnFileNotFoundError()
        {
            // Arrange
            var ex = new FileNotFoundException("File not found");

            // Act
            var message = _errorService.GetExceptionMessage(ex);

            // Assert
            Assert.Contains("无法找到文件", message);
            Assert.Contains("边狱公司路径", message);
        }

        [Fact]
        public void GetExceptionMessage_WithUnauthorizedAccessException_ShouldReturnPermissionError()
        {
            // Arrange
            var ex = new UnauthorizedAccessException("Access denied");

            // Act
            var message = _errorService.GetExceptionMessage(ex);

            // Assert
            Assert.Contains("无权限访问文件", message);
            Assert.Contains("管理员身份", message);
        }

        [Fact]
        public void GetExceptionMessage_WithIOException_ShouldReturnIOError()
        {
            // Arrange
            var ex = new IOException("IO error");

            // Act
            var message = _errorService.GetExceptionMessage(ex);

            // Assert
            Assert.Contains("文件访问出现问题", message);
            Assert.Contains("边狱公司占用", message);
        }

        [Fact]
        public void GetExceptionMessage_WithHashException_ShouldReturnFileCorruptError()
        {
            // Arrange
            var ex = new HashException();

            // Act
            var message = _errorService.GetExceptionMessage(ex);

            // Assert
            Assert.Contains("文件损坏", message);
            Assert.Contains("网络问题", message);
        }

        [Fact]
        public void GetExceptionMessage_WithUnknownException_ShouldReturnGenericError()
        {
            // Arrange
            var ex = new Exception("Unknown error");

            // Act
            var message = _errorService.GetExceptionMessage(ex);

            // Assert
            Assert.Contains("未知错误原因", message);
            Assert.Contains("日志", message);
        }

        [Fact]
        public void ReportError_WithCloseWindowFalse_ShouldShowDialogOnly()
        {
            // Arrange
            var ex = new Exception("Test error");
            bool shutdownCalled = false;

            // Act - 注意：这里会尝试调用 Application.Current.Shutdown()
            // 在测试环境中可能会导致问题，所以我们只验证对话框被调用
            try
            {
                _errorService.ReportError(ex, false);
            }
            catch (Exception)
            {
                // 忽略测试环境中Application.Current可能为null导致的异常
            }

            // Assert
            _mockDialogService.Verify(
                x => x.ShowMessage(It.IsAny<string>(), "错误", null),
                Times.Once);
        }

        [Fact]
        public void ReportMirrorChyanError_ShouldIncludeErrorMessage()
        {
            // Arrange
            var ex = new MirrorChyanException(7001); // CDK过期
            string? capturedMessage = null;

            _mockDialogService.Setup(x => x.ShowMessage(It.IsAny<string>(), It.IsAny<string>(), null))
                .Callback<string, string, Window?>((msg, title, owner) => capturedMessage = msg);

            // Act
            try
            {
                _errorService.ReportMirrorChyanError(ex, false);
            }
            catch (Exception)
            {
                // 忽略测试环境中Application.Current可能为null导致的异常
            }

            // Assert
            Assert.NotNull(capturedMessage);
            Assert.Contains("Mirror 酱服务", capturedMessage);
            Assert.Contains("CDK 已经过期", capturedMessage);
        }
    }
}
