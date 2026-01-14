using LLC_MOD_Toolbox.Services;
using Xunit;

namespace LLC_MOD_Toolbox.Tests.Services
{
    /// <summary>
    /// GachaSimulatorService单元测试
    /// </summary>
    public class GachaSimulatorServiceTests
    {
        private readonly GachaSimulatorService _gachaService;

        public GachaSimulatorServiceTests()
        {
            // 由于FileUtilityService依赖外部资源，这里使用mock或简化测试
            var fileUtilityService = new FileUtilityService();
            _gachaService = new GachaSimulatorService(fileUtilityService);
        }

        [Fact]
        public void IsInitialized_BeforeInit_ShouldReturnFalse()
        {
            // Assert
            Assert.False(_gachaService.IsInitialized);
        }

        [Fact]
        public void FormatPersonalText_With1Star_ShouldReturnCorrectFormat()
        {
            // Arrange
            var result = new LLC_MOD_Toolbox.Interfaces.GachaResult
            {
                Name = "测试人格",
                Unique = 1,
                FormattedText = "[★]测试人格",
                Color = "#B88345"
            };

            // Act
            string text = _gachaService.FormatPersonalText(result);

            // Assert
            Assert.Equal("[★]测试人格", text);
        }

        [Fact]
        public void FormatPersonalText_With2Star_ShouldReturnCorrectFormat()
        {
            // Arrange
            var result = new LLC_MOD_Toolbox.Interfaces.GachaResult
            {
                Name = "测试人格",
                Unique = 2,
                FormattedText = "[★★]测试人格",
                Color = "#CA1400"
            };

            // Act
            string text = _gachaService.FormatPersonalText(result);

            // Assert
            Assert.Equal("[★★]测试人格", text);
        }

        [Fact]
        public void FormatPersonalText_With3Star_ShouldReturnCorrectFormat()
        {
            // Arrange
            var result = new LLC_MOD_Toolbox.Interfaces.GachaResult
            {
                Name = "测试人格",
                Unique = 3,
                FormattedText = "[★★★]测试人格",
                Color = "#FCC404"
            };

            // Act
            string text = _gachaService.FormatPersonalText(result);

            // Assert
            Assert.Equal("[★★★]测试人格", text);
        }

        [Fact]
        public void GetPersonalStats_WithMixedStars_ShouldReturnCorrectStats()
        {
            // Arrange
            var results = new List<LLC_MOD_Toolbox.Interfaces.GachaResult>
            {
                new() { Name = "1星", Unique = 1, FormattedText = "[★]1星", Color = "#B88345" },
                new() { Name = "1星", Unique = 1, FormattedText = "[★]1星", Color = "#B88345" },
                new() { Name = "2星", Unique = 2, FormattedText = "[★★]2星", Color = "#CA1400" },
                new() { Name = "3星", Unique = 3, FormattedText = "[★★★]3星", Color = "#FCC404" }
            };

            // Act
            int[] stats = _gachaService.GetPersonalStats(results);

            // Assert
            Assert.Equal(3, stats.Length);
            Assert.Equal(2, stats[0]); // 1星
            Assert.Equal(1, stats[1]); // 2星
            Assert.Equal(1, stats[2]); // 3星
        }

        [Fact]
        public void GetPersonalStats_WithAll1Star_ShouldReturnCorrectStats()
        {
            // Arrange
            var results = new List<LLC_MOD_Toolbox.Interfaces.GachaResult>
            {
                new() { Name = "1星", Unique = 1, FormattedText = "[★]1星", Color = "#B88345" },
                new() { Name = "1星", Unique = 1, FormattedText = "[★]1星", Color = "#B88345" },
                new() { Name = "1星", Unique = 1, FormattedText = "[★]1星", Color = "#B88345" }
            };

            // Act
            int[] stats = _gachaService.GetPersonalStats(results);

            // Assert
            Assert.Equal(3, stats[0]); // 3个1星
            Assert.Equal(0, stats[1]); // 0个2星
            Assert.Equal(0, stats[2]); // 0个3星
        }
    }
}
