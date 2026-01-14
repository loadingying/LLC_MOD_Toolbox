using LLC_MOD_Toolbox.Interfaces;
using LLC_MOD_Toolbox.Services;
using Xunit;
using System.IO;

namespace LLC_MOD_Toolbox.Tests.Services
{
    /// <summary>
    /// FontService单元测试
    /// 测试字体验证、替换和还原功能
    /// </summary>
    public class FontServiceTests
    {
        private readonly IFontService _fontService;

        public FontServiceTests()
        {
            _fontService = new FontService();
        }

        [Fact]
        public void IsValidFontFile_WithNullPath_ShouldReturnFalse()
        {
            // Act
            var result = _fontService.IsValidFontFile(null);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValidFontFile_WithEmptyPath_ShouldReturnFalse()
        {
            // Act
            var result = _fontService.IsValidFontFile(string.Empty);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValidFontFile_WithInvalidExtension_ShouldReturnFalse()
        {
            // Arrange
            var tempFile = Path.GetTempFileName() + ".txt";

            try
            {
                // Act
                var result = _fontService.IsValidFontFile(tempFile);

                // Assert
                Assert.False(result);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public void IsValidFontFile_WithNonExistentFile_ShouldReturnFalse()
        {
            // Act
            var result = _fontService.IsValidFontFile("C:\\NonExistent\\Font.ttf");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HasBackupFont_WithNullDirectory_ShouldReturnFalse()
        {
            // Act
            var result = _fontService.HasBackupFont(null);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HasBackupFont_WithEmptyDirectory_ShouldReturnFalse()
        {
            // Act
            var result = _fontService.HasBackupFont(string.Empty);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HasChineseFontInstalled_WithNullDirectory_ShouldReturnFalse()
        {
            // Act
            var result = _fontService.HasChineseFontInstalled(null);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HasChineseFontInstalled_WithEmptyDirectory_ShouldReturnFalse()
        {
            // Act
            var result = _fontService.HasChineseFontInstalled(string.Empty);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ReplaceFontAsync_WithNullFontPath_ShouldReturnErrorMessage()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            try
            {
                // Act
                var result = await _fontService.ReplaceFontAsync(null!, tempDir);

                // Assert
                Assert.False(result.Success);
                Assert.Equal("字体文件无效", result.ErrorMessage);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public async Task ReplaceFontAsync_WithInvalidFontPath_ShouldReturnErrorMessage()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            try
            {
                // Act
                var result = await _fontService.ReplaceFontAsync("invalid.ttf", tempDir);

                // Assert
                Assert.False(result.Success);
                Assert.Equal("字体文件无效", result.ErrorMessage);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public async Task RestoreFontAsync_WithNullDirectory_ShouldReturnErrorMessage()
        {
            // Act
            var result = await _fontService.RestoreFontAsync(null!);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.ErrorMessage);
        }

        [Fact]
        public async Task RestoreFontAsync_WithNoBackup_ShouldReturnErrorMessage()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            try
            {
                // Act
                var result = await _fontService.RestoreFontAsync(tempDir);

                // Assert
                Assert.False(result.Success);
                Assert.Equal("没有找到备份字体文件", result.ErrorMessage);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }
    }
}
