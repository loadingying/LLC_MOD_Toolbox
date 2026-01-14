using LLC_MOD_Toolbox.Interfaces;
using LLC_MOD_Toolbox.Services;
using Moq;
using System.IO;
using Xunit;

namespace LLC_MOD_Toolbox.Tests.Services
{
    public class ModInstallServiceTests
    {
        [Fact]
        public void NeedsFontInstallation_WhenNoFontFiles_ShouldReturnTrue()
        {
            var tempDir = CreateGameDir();
            try
            {
                var service = CreateService();
                Assert.True(service.NeedsFontInstallation(tempDir));
            }
            finally
            {
                Cleanup(tempDir);
            }
        }

        [Fact]
        public void NeedsFontInstallation_WhenFontExists_ShouldReturnFalse()
        {
            var tempDir = CreateGameDir();
            var fontDir = Path.Combine(tempDir, "LimbusCompany_Data", "Lang", "LLC_zh-CN", "Font", "Context");
            Directory.CreateDirectory(fontDir);
            File.WriteAllText(Path.Combine(fontDir, "ChineseFont.ttf"), string.Empty);
            try
            {
                var service = CreateService();
                Assert.False(service.NeedsFontInstallation(tempDir));
            }
            finally
            {
                Cleanup(tempDir);
            }
        }

        [Fact]
        public void NeedsModInstallation_WhenNoVersionFile_ShouldReturnTrue()
        {
            var tempDir = CreateGameDir();
            try
            {
                var service = CreateService();
                Assert.True(service.NeedsModInstallation(tempDir, 0, 100));
            }
            finally
            {
                Cleanup(tempDir);
            }
        }

        [Fact]
        public void NeedsModInstallation_WhenCurrentIsLatest_ShouldReturnFalse()
        {
            var tempDir = CreateGameDir();
            var infoDir = Path.Combine(tempDir, "LimbusCompany_Data", "Lang", "LLC_zh-CN", "Info");
            Directory.CreateDirectory(infoDir);
            File.WriteAllText(Path.Combine(infoDir, "version.json"), "{\"version\":100}");
            try
            {
                var service = CreateService();
                Assert.False(service.NeedsModInstallation(tempDir, 100, 100));
            }
            finally
            {
                Cleanup(tempDir);
            }
        }

        [Fact]
        public async System.Threading.Tasks.Task GetCurrentModVersionAsync_WithValidJson_ShouldReturnVersion()
        {
            var tempDir = CreateGameDir();
            var infoDir = Path.Combine(tempDir, "LimbusCompany_Data", "Lang", "LLC_zh-CN", "Info");
            Directory.CreateDirectory(infoDir);
            File.WriteAllText(Path.Combine(infoDir, "version.json"), "{\"version\":123}");
            try
            {
                var service = CreateService();
                var version = await service.GetCurrentModVersionAsync(tempDir);
                Assert.Equal(123, version);
            }
            finally
            {
                Cleanup(tempDir);
            }
        }

        [Fact]
        public void VerifyFileHash_WithMatchingHash_ShouldReturnTrue()
        {
            var fileUtility = new Mock<IFileUtilityService>();
            fileUtility.Setup(f => f.CalculateSHA256("file")).Returns("abc");

            var service = CreateService(fileUtility);
            Assert.True(service.VerifyFileHash("file", "abc"));
        }

        [Fact]
        public void ExtractAndInstallMod_ShouldUnarchiveAndDelete()
        {
            var fileUtility = new Mock<IFileUtilityService>();
            var service = CreateService(fileUtility);

            service.ExtractAndInstallMod("archive.7z", "target");

            fileUtility.Verify(f => f.Unarchive("archive.7z", "target"), Times.Once);
            fileUtility.Verify(f => f.DeleteFile("archive.7z"), Times.Once);
        }

        [Fact]
        public void ExtractAndInstallMod_WithDeleteDisabled_ShouldNotDelete()
        {
            var fileUtility = new Mock<IFileUtilityService>();
            var service = CreateService(fileUtility);

            service.ExtractAndInstallMod("archive.7z", "target", deleteAfterExtract: false);

            fileUtility.Verify(f => f.Unarchive("archive.7z", "target"), Times.Once);
            fileUtility.Verify(f => f.DeleteFile("archive.7z"), Times.Never);
        }

        private static ModInstallService CreateService(Mock<IFileUtilityService>? fileUtility = null)
        {
            var fileUtilityService = fileUtility ?? new Mock<IFileUtilityService>();
            return new ModInstallService(fileUtilityService.Object, new OfficialApiClient());
        }

        private static string CreateGameDir()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), $"llc_mod_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);
            return tempDir;
        }

        private static void Cleanup(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
    }
}
