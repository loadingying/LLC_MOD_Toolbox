using LLC_MOD_Toolbox.Services;
using Xunit;
using System.IO;

namespace LLC_MOD_Toolbox.Tests.Services
{
    /// <summary>
    /// NodeManagementService单元测试
    /// 测试节点管理服务的加载、初始化和默认节点选择
    /// </summary>
    public class NodeManagementServiceTests : IDisposable
    {
        private readonly string _testNodeListPath;
        private readonly NodeManagementService _nodeService;

        public NodeManagementServiceTests()
        {
            // 创建测试用的NodeList.json
            _testNodeListPath = Path.Combine(Path.GetTempPath(), $"test_nodelist_{Guid.NewGuid()}.json");
            CreateTestNodeListFile();

            _nodeService = new NodeManagementService();
        }

        [Fact]
        public void InitializeNodes_ShouldLoadAllNodes()
        {
            // Act
            _nodeService.InitializeNodes(_testNodeListPath, internationalMode: false);

            // Assert
            var downloadNodes = _nodeService.GetDownloadNodes();
            var apiNodes = _nodeService.GetApiNodes();

            Assert.Equal(3, downloadNodes.Count);
            Assert.Equal(2, apiNodes.Count);
        }

        [Fact]
        public void InitializeNodes_ShouldDeserializeNodeProperties()
        {
            // Act
            _nodeService.InitializeNodes(_testNodeListPath, internationalMode: false);

            // Assert
            var downloadNodes = _nodeService.GetDownloadNodes();
            var autoNode = downloadNodes.FirstOrDefault(n => n.Name == "自动选择节点");

            Assert.NotNull(autoNode);
            Assert.Equal("https://api.zeroasso.top/v2/download/files?file_name={0}", autoNode.Endpoint);
            Assert.True(autoNode.IsDefault);
        }

        [Fact]
        public void GetDefaultDownloadNodeEndpoint_ShouldReturnDefaultNode()
        {
            // Arrange
            _nodeService.InitializeNodes(_testNodeListPath, internationalMode: false);

            // Act
            var endpoint = _nodeService.GetDefaultDownloadNodeEndpoint();

            // Assert
            Assert.Equal("https://api.zeroasso.top/v2/download/files?file_name={0}", endpoint);
        }

        [Fact]
        public void GetDefaultApiNodeEndpoint_ShouldReturnDefaultNode()
        {
            // Arrange
            _nodeService.InitializeNodes(_testNodeListPath, internationalMode: false);

            // Act
            var endpoint = _nodeService.GetDefaultApiNodeEndpoint();

            // Assert
            Assert.Equal("https://api.zeroasso.top/{0}", endpoint);
        }

        [Fact]
        public void GetDefaultApiNodeEndpoint_ShouldUseFirstNodeAsFallback()
        {
            // Arrange - 创建没有默认节点的配置
            var noDefaultPath = Path.Combine(Path.GetTempPath(), $"test_nodelist_nodefault_{Guid.NewGuid()}.json");
            CreateTestNodeListFileNoDefault(noDefaultPath);

            var service = new NodeManagementService();
            service.InitializeNodes(noDefaultPath, internationalMode: false);

            // Act
            var endpoint = service.GetDefaultApiNodeEndpoint();

            // Assert - 应该使用第一个节点作为后备
            Assert.NotNull(endpoint);
            Assert.Contains("api.zeroasso.top", endpoint);

            // Cleanup
            if (File.Exists(noDefaultPath))
                File.Delete(noDefaultPath);
        }

        [Fact]
        public void FindApiNodeEndpoint_ShouldReturnCorrectEndpoint()
        {
            // Arrange
            _nodeService.InitializeNodes(_testNodeListPath, internationalMode: false);

            // Act
            var endpoint = _nodeService.FindApiNodeEndpoint("CloudFlare CDN API(海外)");

            // Assert
            Assert.Equal("https://cdn-api.zeroasso.top/{0}", endpoint);
        }

        [Fact]
        public void InitializeNodes_InternationalMode_ShouldUseCDNNode()
        {
            // Act
            _nodeService.InitializeNodes(_testNodeListPath, internationalMode: true);

            // Assert
            var apiEndpoint = _nodeService.GetDefaultApiNodeEndpoint();
            Assert.Equal("https://cdn-api.zeroasso.top/{0}", apiEndpoint);
        }

        private void CreateTestNodeListFile()
        {
            var json = @"{
  ""downloadNode"": [
    {
      ""name"": ""自动选择节点"",
      ""endpoint"": ""https://api.zeroasso.top/v2/download/files?file_name={0}"",
      ""isDefault"": true
    },
    {
      ""name"": ""零协会镇江节点"",
      ""endpoint"": ""https://download.zeroasso.top/files/{0}"",
      ""isDefault"": false
    },
    {
      ""name"": ""CloudFlare CDN(海外)"",
      ""endpoint"": ""https://cdn-download.zeroasso.top/files/{0}"",
      ""isDefault"": false
    }
  ],
  ""apiNode"": [
    {
      ""name"": ""零协会官方 API"",
      ""endpoint"": ""https://api.zeroasso.top/{0}"",
      ""isDefault"": true
    },
    {
      ""name"": ""CloudFlare CDN API(海外)"",
      ""endpoint"": ""https://cdn-api.zeroasso.top/{0}"",
      ""isDefault"": false
    }
  ]
}";
            File.WriteAllText(_testNodeListPath, json);
        }

        private void CreateTestNodeListFileNoDefault(string path)
        {
            var json = @"{
  ""downloadNode"": [
    {
      ""name"": ""节点1"",
      ""endpoint"": ""https://download.zeroasso.top/files/{0}"",
      ""isDefault"": false
    }
  ],
  ""apiNode"": [
    {
      ""name"": ""零协会官方 API"",
      ""endpoint"": ""https://api.zeroasso.top/{0}"",
      ""isDefault"": false
    }
  ]
}";
            File.WriteAllText(path, json);
        }

        public void Dispose()
        {
            // 清理测试文件
            if (File.Exists(_testNodeListPath))
            {
                try
                {
                    File.Delete(_testNodeListPath);
                }
                catch
                {
                    // 忽略删除失败
                }
            }
        }
    }
}
