using LLC_MOD_Toolbox.Interfaces;
using LLC_MOD_Toolbox.Services;
using Xunit;
using Assert = Xunit.Assert;

namespace LLC_MOD_Toolbox.Tests.Services
{
    /// <summary>
    /// ProgressService单元测试
    /// 测试进度服务的启动、停止、更新等功能
    /// </summary>
    public class ProgressServiceTests
    {
        private readonly IProgressService _progressService;

        public ProgressServiceTests()
        {
            _progressService = new ProgressService();
        }

        [Fact]
        public void CurrentProgress_Initially_ShouldBeZero()
        {
            // Act
            var progress = _progressService.CurrentProgress;

            // Assert
            Assert.Equal(0, progress);
        }

        [Fact]
        public void IsRunning_Initially_ShouldBeFalse()
        {
            // Act
            var isRunning = _progressService.IsRunning;

            // Assert
            Assert.False(isRunning);
        }

        [Fact]
        public void Start_ShouldSetIsRunningToTrue()
        {
            // Act
            _progressService.Start();

            // Assert
            Assert.True(_progressService.IsRunning);

            // Cleanup
            _progressService.Stop();
        }

        [Fact]
        public void Stop_AfterStart_ShouldSetIsRunningToFalse()
        {
            // Arrange
            _progressService.Start();

            // Act
            _progressService.Stop();

            // Assert
            Assert.False(_progressService.IsRunning);
        }

        [Fact]
        public void SetProgress_WithValidValue_ShouldUpdateProgress()
        {
            // Act
            _progressService.SetProgress(50.0f);

            // Assert
            Assert.Equal(50.0f, _progressService.CurrentProgress);
        }

        [Fact]
        public void SetProgress_WithValueAbove100_ShouldClampTo100()
        {
            // Act
            _progressService.SetProgress(150.0f);

            // Assert
            Assert.Equal(100.0f, _progressService.CurrentProgress);
        }

        [Fact]
        public void SetProgress_WithValueBelow0_ShouldClampTo0()
        {
            // Act
            _progressService.SetProgress(-10.0f);

            // Assert
            Assert.Equal(0.0f, _progressService.CurrentProgress);
        }

        [Fact]
        public void AddProgress_ShouldIncreaseProgress()
        {
            // Arrange
            _progressService.SetProgress(30.0f);

            // Act
            _progressService.AddProgress(20.0f);

            // Assert
            Assert.Equal(50.0f, _progressService.CurrentProgress);
        }

        [Fact]
        public void AddProgress_ThatExceeds100_ShouldClampTo100()
        {
            // Arrange
            _progressService.SetProgress(90.0f);

            // Act
            _progressService.AddProgress(20.0f);

            // Assert
            Assert.Equal(100.0f, _progressService.CurrentProgress);
        }

        [Fact]
        public void Reset_ShouldSetProgressToZero()
        {
            // Arrange
            _progressService.SetProgress(75.0f);

            // Act
            _progressService.Reset();

            // Assert
            Assert.Equal(0.0f, _progressService.CurrentProgress);
        }

        [Fact]
        public void ProgressChanged_ShouldFireWhenProgressChanges()
        {
            // Arrange
            float? capturedProgress = null;
            _progressService.ProgressChanged += (sender, progress) =>
            {
                capturedProgress = progress;
            };

            // Act
            _progressService.SetProgress(42.0f);

            // Assert
            Assert.NotNull(capturedProgress);
            Assert.Equal(42.0f, capturedProgress.Value);
        }

        [Fact]
        public void Start_ShouldResetProgressToZero()
        {
            // Arrange
            _progressService.SetProgress(50.0f);

            // Act
            _progressService.Start();

            // Assert
            Assert.Equal(0.0f, _progressService.CurrentProgress);

            // Cleanup
            _progressService.Stop();
        }

        [Fact]
        public void MultipleStartCalls_ShouldNotChangeState()
        {
            // Arrange
            _progressService.Start();

            // Act
            _progressService.Start();

            // Assert
            Assert.True(_progressService.IsRunning);

            // Cleanup
            _progressService.Stop();
        }

        [Fact]
        public void MultipleStopCalls_ShouldNotThrow()
        {
            // Arrange
            _progressService.Start();

            // Act & Assert - Should not throw
            _progressService.Stop();
            _progressService.Stop(); // Second stop should not throw

            Assert.False(_progressService.IsRunning);
        }
    }
}
