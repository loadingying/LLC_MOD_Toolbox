using LLC_MOD_Toolbox.Interfaces;
using LLC_MOD_Toolbox.ViewModels;
using Moq;
using Xunit;
using GachaResult = LLC_MOD_Toolbox.ViewModels.GachaResult;
using System;
using System.Threading.Tasks;

namespace LLC_MOD_Toolbox.Tests.ViewModels
{
    /// <summary>
    /// GachaSimulatorPageViewModel单元测试
    /// 测试抽卡模拟器的逻辑
    /// </summary>
    public class GachaSimulatorPageViewModelTests
    {
        private readonly Mock<IConfigService> _mockConfigService;
        private readonly Mock<IDialogService> _mockDialogService;
        private readonly GachaSimulatorPageViewModel _viewModel;

        public GachaSimulatorPageViewModelTests()
        {
            _mockConfigService = new Mock<IConfigService>();
            _mockDialogService = new Mock<IDialogService>();
            _viewModel = new GachaSimulatorPageViewModel(
                _mockConfigService.Object,
                _mockDialogService.Object);
        }

        [Fact]
        public void GachaResults_ShouldBeEmpty_Initially()
        {
            // Act
            var results = _viewModel.GachaResults;

            // Assert
            Assert.NotNull(results);
            Assert.Empty(results);
        }

        [Fact]
        public void IsGachaInProgress_ShouldBeFalse_Initially()
        {
            // Act
            var isInProgress = _viewModel.IsGachaInProgress;

            // Assert
            Assert.False(isInProgress);
        }

        [Fact]
        public void TotalPulls_ShouldBeZero_Initially()
        {
            // Act
            var totalPulls = _viewModel.TotalPulls;

            // Assert
            Assert.Equal(0, totalPulls);
        }

        [Fact]
        public void StarCounts_ShouldBeZero_Initially()
        {
            // Act & Assert
            Assert.Equal(0, _viewModel.Star3Count);
            Assert.Equal(0, _viewModel.Star2Count);
            Assert.Equal(0, _viewModel.Star1Count);
        }

        [Fact]
        public void GachaCommand_ShouldBeExecutable_Initially()
        {
            // Act
            var canExecute = _viewModel.GachaCommand.CanExecute(null);

            // Assert
            Assert.True(canExecute);
        }

        [Fact]
        public async Task ExecuteGachaAsync_ShouldAddResults()
        {
            // Arrange
            var initialCount = _viewModel.GachaResults.Count;
            var initialTotal = _viewModel.TotalPulls;

            // Act
            _viewModel.GachaCommand.Execute(null);
            await WaitForGachaAsync(_viewModel, initialTotal);

            // Assert
            Assert.Equal(10, _viewModel.GachaResults.Count - initialCount);
            Assert.Equal(10, _viewModel.TotalPulls);
        }

        [Fact]
        public async Task ExecuteGachaAsync_ShouldUpdateStatistics()
        {
            // Arrange
            var initialTotal = _viewModel.TotalPulls;

            // Act
            _viewModel.GachaCommand.Execute(null);
            await WaitForGachaAsync(_viewModel, initialTotal);

            // Assert
            Assert.Equal(initialTotal + 10, _viewModel.TotalPulls);
            Assert.True(_viewModel.Star3Count + _viewModel.Star2Count + _viewModel.Star1Count == 10);
        }

        [Fact]
        public void ClearResultsCommand_ShouldClearAllResults()
        {
            // Arrange
            _viewModel.GachaResults.Add(new GachaResult
            {
                Rarity = 3,
                Personality = "Test Personality",
                Color = "#CA1400",
                Timestamp = DateTime.Now
            });
            _viewModel.TotalPulls = 10;
            _viewModel.Star3Count = 1;
            _viewModel.Star2Count = 2;
            _viewModel.Star1Count = 7;

            // Act
            _viewModel.ClearResultsCommand.Execute(null);

            // Assert
            Assert.Empty(_viewModel.GachaResults);
            Assert.Equal(0, _viewModel.TotalPulls);
            Assert.Equal(0, _viewModel.Star3Count);
            Assert.Equal(0, _viewModel.Star2Count);
            Assert.Equal(0, _viewModel.Star1Count);
        }

        [Fact]
        public void GachaResult_DisplayText_ShouldIncludeStars()
        {
            // Arrange
            var result = new GachaResult
            {
                Rarity = 3,
                Personality = "Test Personality",
                Color = "#CA1400",
                Timestamp = DateTime.Now
            };

            // Act
            var displayText = result.DisplayText;

            // Assert
            Assert.Contains("★★★", displayText);
            Assert.Contains("Test Personality", displayText);
        }

        private static async Task WaitForGachaAsync(GachaSimulatorPageViewModel viewModel, int initialTotalOrCount)
        {
            var timeoutAt = DateTime.UtcNow.AddSeconds(3);
            while (DateTime.UtcNow < timeoutAt)
            {
                if (viewModel.TotalPulls >= initialTotalOrCount + 10)
                {
                    return;
                }

                await Task.Delay(50);
            }
        }
    }
}
