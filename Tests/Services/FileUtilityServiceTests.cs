using LLC_MOD_Toolbox.Services;
using Xunit;
using System.IO;

namespace LLC_MOD_Toolbox.Tests.Services
{
    /// <summary>
    /// FileUtilityService单元测试
    /// </summary>
    public class FileUtilityServiceTests
    {
        private readonly FileUtilityService _fileUtilityService;
        private readonly string _testTempDir;

        public FileUtilityServiceTests()
        {
            _fileUtilityService = new FileUtilityService();
            _testTempDir = Path.Combine(Path.GetTempPath(), "LLC_Toolbox_Tests");
            Directory.CreateDirectory(_testTempDir);
        }

        public void Dispose()
        {
            if (Directory.Exists(_testTempDir))
            {
                try
                {
                    Directory.Delete(_testTempDir, true);
                }
                catch { }
            }
        }

        [Fact]
        public void CalculateSHA256_WithValidFile_ShouldReturnCorrectHash()
        {
            // Arrange
            string testFile = Path.Combine(_testTempDir, "test.txt");
            File.WriteAllText(testFile, "test content");

            // Act
            string hash = _fileUtilityService.CalculateSHA256(testFile);

            // Assert
            Assert.NotNull(hash);
            Assert.Equal(64, hash.Length); // SHA256是64个十六进制字符
            Assert.Matches("^[a-f0-9]{64}$", hash);
        }

        [Fact]
        public void DeleteFile_WithExistingFile_ShouldDeleteFile()
        {
            // Arrange
            string testFile = Path.Combine(_testTempDir, "delete_test.txt");
            File.WriteAllText(testFile, "test");

            // Act
            _fileUtilityService.DeleteFile(testFile);

            // Assert
            Assert.False(File.Exists(testFile));
        }

        [Fact]
        public void DeleteFile_WithNonExistentFile_ShouldNotThrow()
        {
            // Arrange
            string testFile = Path.Combine(_testTempDir, "nonexistent.txt");

            // Act & Assert
            var exception = Record.Exception(() => _fileUtilityService.DeleteFile(testFile));
            Assert.Null(exception);
        }

        [Fact]
        public void DeleteDirectory_WithExistingDirectory_ShouldDeleteDirectory()
        {
            // Arrange
            string testDir = Path.Combine(_testTempDir, "delete_test_dir");
            Directory.CreateDirectory(testDir);
            File.WriteAllText(Path.Combine(testDir, "test.txt"), "test");

            // Act
            _fileUtilityService.DeleteDirectory(testDir);

            // Assert
            Assert.False(Directory.Exists(testDir));
        }

        [Fact]
        public void IsValidFontFile_WithInvalidExtension_ShouldReturnFalse()
        {
            // Arrange
            string testFile = Path.Combine(_testTempDir, "test.exe");
            File.WriteAllText(testFile, "fake exe");

            // Act
            bool result = _fileUtilityService.IsValidFontFile(testFile);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValidFontFile_WithNonExistentFile_ShouldReturnFalse()
        {
            // Arrange
            string testFile = Path.Combine(_testTempDir, "nonexistent.ttf");

            // Act
            bool result = _fileUtilityService.IsValidFontFile(testFile);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetFontFamilyName_WithNonFontFile_ShouldReturnFileName()
        {
            // Arrange
            string testFile = Path.Combine(_testTempDir, "testfile.txt");
            File.WriteAllText(testFile, "not a font");

            // Act
            string name = _fileUtilityService.GetFontFamilyName(testFile);

            // Assert
            Assert.Equal("testfile", name);
        }
    }
}
