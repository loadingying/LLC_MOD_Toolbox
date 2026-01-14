using LLC_MOD_Toolbox.Interfaces;
using LLC_MOD_Toolbox.Services;
using Xunit;
using System.IO;
using Newtonsoft.Json;
using LLC_MOD_Toolbox.Models;

namespace LLC_MOD_Toolbox.Tests.Services
{
    /// <summary>
    /// LoadingTextService单元测试
    /// 测试Loading文本服务
    /// </summary>
    public class LoadingTextServiceTests
    {
        private readonly LoadingTextService _loadingTextService;

        public LoadingTextServiceTests()
        {
            var configService = new ConfigService(Path.Combine(Path.GetTempPath(), $"test_config_{Guid.NewGuid()}.json"));
            var officialApiClient = new OfficialApiClient();
            _loadingTextService = new LoadingTextService(configService, officialApiClient);
        }

        [Fact]
        public void LoadingTexts_Initially_ShouldBeEmpty()
        {
            // Act
            var texts = _loadingTextService.LoadingTexts;

            // Assert
            Assert.NotNull(texts);
            Assert.Empty(texts);
        }

        [Fact]
        public void GetRandomText_WhenEmpty_ShouldReturnDefault()
        {
            // Act
            var text = _loadingTextService.GetRandomText();

            // Assert
            Assert.Equal("加载中...", text);
        }

        [Fact]
        public void GetRandomText_AfterLoadingFile_ShouldReturnFromFile()
        {
            // Arrange
            var tempFile = Path.Combine(Path.GetTempPath(), $"loading_{Guid.NewGuid()}.json");
            var json = @"{
                ""loadingText"": [
                    ""测试文本1"",
                    ""测试文本2"",
                    ""测试文本3""
                ]
            }";
            File.WriteAllText(tempFile, json);

            try
            {
                // Act
                _loadingTextService.LoadFromFile(tempFile);
                var text = _loadingTextService.GetRandomText();

                // Assert
                Assert.Contains(text, new[] { "测试文本1", "测试文本2", "测试文本3" });
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public void LoadFromFile_WithNonExistentFile_ShouldUseDefaults()
        {
            // Act
            _loadingTextService.LoadFromFile("non_existent_file.json");

            // Assert
            Assert.NotEmpty(_loadingTextService.LoadingTexts);
            Assert.Contains("正在为您加载工具箱...", _loadingTextService.LoadingTexts);
        }

        [Fact]
        public void LoadFromFile_WithInvalidJson_ShouldUseDefaults()
        {
            // Arrange
            var tempFile = Path.Combine(Path.GetTempPath(), $"loading_{Guid.NewGuid()}.json");
            File.WriteAllText(tempFile, "invalid json");

            try
            {
                // Act
                _loadingTextService.LoadFromFile(tempFile);

                // Assert
                Assert.NotEmpty(_loadingTextService.LoadingTexts);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public void GetRandomText_AfterLoadingDefault_ShouldReturnOneOfDefaults()
        {
            // Arrange
            var tempFile = Path.Combine(Path.GetTempPath(), $"loading_{Guid.NewGuid()}.json");

            try
            {
                // Act
                _loadingTextService.LoadFromFile(tempFile);
                var text = _loadingTextService.GetRandomText();

                // Assert
                Assert.NotNull(text);
                Assert.NotEmpty(text);
            }
            finally
            {
                if (File.Exists(tempFile) && File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        #region 新增测试用例

        [Fact]
        public void LoadFromFile_ShouldLoadDate()
        {
            // Arrange
            var tempFile = Path.Combine(Path.GetTempPath(), $"loading_{Guid.NewGuid()}.json");
            var testDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            var json = JsonConvert.SerializeObject(new RootModel
            {
                LoadingText = new List<string> { "测试文本1", "测试文本2" },
                LoadingDate = testDate
            });
            File.WriteAllText(tempFile, json);

            try
            {
                // Act
                _loadingTextService.LoadFromFile(tempFile);

                // Assert
                Assert.Equal(testDate, _loadingTextService.LoadingDate);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public void GetWeightedRandomText_ShouldReturnValidText()
        {
            // Arrange
            var tempFile = Path.Combine(Path.GetTempPath(), $"loading_{Guid.NewGuid()}.json");
            var json = JsonConvert.SerializeObject(new RootModel
            {
                LoadingText = new List<string> { "文本1", "文本2", "文本3" },
                LoadingDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
            });
            File.WriteAllText(tempFile, json);

            try
            {
                // Act
                _loadingTextService.LoadFromFile(tempFile);
                var text = _loadingTextService.GetWeightedRandomText();

                // Assert
                Assert.NotNull(text);
                Assert.NotEmpty(text);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public void SaveToFile_ShouldPersistTextsAndDate()
        {
            // Arrange
            var loadFile = Path.Combine(Path.GetTempPath(), $"loading_load_{Guid.NewGuid()}.json");
            var saveFile = Path.Combine(Path.GetTempPath(), $"loading_save_{Guid.NewGuid()}.json");
            var json = JsonConvert.SerializeObject(new RootModel
            {
                LoadingText = new List<string> { "保存测试1", "保存测试2" },
                LoadingDate = "2024-01-01 12:00"
            });
            File.WriteAllText(loadFile, json);

            try
            {
                // Act
                _loadingTextService.LoadFromFile(loadFile);
                _loadingTextService.SaveToFile(saveFile);

                // Assert
                Assert.True(File.Exists(saveFile));
                var savedJson = File.ReadAllText(saveFile);
                var savedRoot = JsonConvert.DeserializeObject<RootModel>(savedJson);
                Assert.NotNull(savedRoot);
                Assert.Equal(2, savedRoot.LoadingText.Count);
                Assert.NotNull(savedRoot.LoadingDate);
            }
            finally
            {
                if (File.Exists(loadFile))
                    File.Delete(loadFile);
                if (File.Exists(saveFile))
                    File.Delete(saveFile);
            }
        }

        [Fact]
        public void NeedsUpdate_WithOldDate_ShouldReturnTrue()
        {
            // Arrange
            var tempFile = Path.Combine(Path.GetTempPath(), $"loading_{Guid.NewGuid()}.json");
            var oldDate = DateTime.Now.AddDays(-15).ToString("yyyy-MM-dd HH:mm");
            var json = JsonConvert.SerializeObject(new RootModel
            {
                LoadingText = new List<string> { "旧文本" },
                LoadingDate = oldDate
            });
            File.WriteAllText(tempFile, json);

            try
            {
                // Act
                _loadingTextService.LoadFromFile(tempFile);
                var needsUpdate = _loadingTextService.NeedsUpdate();

                // Assert
                Assert.True(needsUpdate);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public void NeedsUpdate_WithRecentDate_ShouldReturnFalse()
        {
            // Arrange
            var tempFile = Path.Combine(Path.GetTempPath(), $"loading_{Guid.NewGuid()}.json");
            var recentDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd HH:mm");
            var json = JsonConvert.SerializeObject(new RootModel
            {
                LoadingText = new List<string> { "新文本" },
                LoadingDate = recentDate
            });
            File.WriteAllText(tempFile, json);

            try
            {
                // Act
                _loadingTextService.LoadFromFile(tempFile);
                var needsUpdate = _loadingTextService.NeedsUpdate();

                // Assert
                Assert.False(needsUpdate);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public void NeedsUpdate_WithNoDate_ShouldReturnTrue()
        {
            // Arrange
            var tempFile = Path.Combine(Path.GetTempPath(), $"loading_{Guid.NewGuid()}.json");
            var json = JsonConvert.SerializeObject(new RootModel
            {
                LoadingText = new List<string> { "无日期文本" },
                LoadingDate = null
            });
            File.WriteAllText(tempFile, json);

            try
            {
                // Act
                _loadingTextService.LoadFromFile(tempFile);
                var needsUpdate = _loadingTextService.NeedsUpdate();

                // Assert
                Assert.True(needsUpdate);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        #endregion
    }
}
