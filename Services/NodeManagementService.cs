using LLC_MOD_Toolbox;
using LLC_MOD_Toolbox.Interfaces;
using LLC_MOD_Toolbox.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.IO;

namespace LLC_MOD_Toolbox.Services
{
    /// <summary>
    /// 节点管理服务实现
    /// 处理节点配置的加载、查询和切换逻辑
    /// </summary>
    public class NodeManagementService : INodeManagementService
    {
        private List<Node> _downloadNodes = new();
        private List<Node> _apiNodes = new();
        private bool _isInitialized = false;
        private string? _defaultDownloadNodeEndpoint;
        private string? _defaultApiNodeEndpoint;

        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// 初始化节点列表（从JSON文件加载）
        /// </summary>
        public void InitializeNodes(string nodeListJsonPath, bool internationalMode)
        {
            if (!File.Exists(nodeListJsonPath))
            {
                Log.logger.Error($"节点配置文件不存在：{nodeListJsonPath}");
                return;
            }

            try
            {
                var jsonSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new CamelCaseNamingStrategy()
                    }
                };

                string jsonContent = File.ReadAllText(nodeListJsonPath);
                var rootModel = JsonConvert.DeserializeObject<RootModel>(jsonContent, jsonSettings);

                if (rootModel == null)
                {
                    Log.logger.Error("反序列化节点配置失败");
                    return;
                }

                _downloadNodes = rootModel.DownloadNode ?? new List<Node>();
                _apiNodes = rootModel.ApiNode ?? new List<Node>();

                // 添加调试日志
                Log.logger.Info($"下载节点数量：{_downloadNodes.Count}");
                foreach (var node in _downloadNodes)
                {
                    Log.logger.Info($"  - {node.Name}: {node.Endpoint}, IsDefault={node.IsDefault}");
                }

                Log.logger.Info($"API节点数量：{_apiNodes.Count}");
                foreach (var api in _apiNodes)
                {
                    Log.logger.Info($"  - {api.Name}: {api.Endpoint}, IsDefault={api.IsDefault}");
                }

                // 确定默认下载节点
                foreach (var node in _downloadNodes)
                {
                    if (node.IsDefault && !internationalMode)
                    {
                        _defaultDownloadNodeEndpoint = node.Endpoint;
                    }
                    if (node.Endpoint == "https://cdn-download.zeroasso.top/files/{0}" && internationalMode)
                    {
                        Log.logger.Info("获取到国际下载节点。");
                        _defaultDownloadNodeEndpoint = node.Endpoint;
                    }
                }

                // 确定默认API节点
                foreach (var api in _apiNodes)
                {
                    if (api.IsDefault && !internationalMode)
                    {
                        _defaultApiNodeEndpoint = api.Endpoint;
                        Log.logger.Info($"使用默认API节点：{api.Name}");
                    }
                    if (api.Endpoint == "https://cdn-api.zeroasso.top/{0}" && internationalMode)
                    {
                        Log.logger.Info("获取到国际API节点。");
                        _defaultApiNodeEndpoint = api.Endpoint;
                    }
                }

                // 如果没有找到默认API节点，使用第一个作为后备
                if (string.IsNullOrEmpty(_defaultApiNodeEndpoint) && _apiNodes.Count > 0)
                {
                    _defaultApiNodeEndpoint = _apiNodes[0].Endpoint;
                    Log.logger.Warn($"未找到默认API节点，使用第一个节点：{_apiNodes[0].Name}");
                }

                _isInitialized = true;
                Log.logger.Info($"API数量：{_apiNodes.Count}");
                Log.logger.Info($"节点数量：{_downloadNodes.Count}");
                Log.logger.Info($"默认API端点：{_defaultApiNodeEndpoint ?? "空"}");
            }
            catch (Exception ex)
            {
                Log.logger.Error("初始化节点配置失败", ex);
            }
        }

        /// <summary>
        /// 获取下载节点列表
        /// </summary>
        public List<Node> GetDownloadNodes()
        {
            if (!_isInitialized)
            {
                Log.logger.Warn("节点管理服务未初始化");
                return new List<Node>();
            }
            return _downloadNodes;
        }

        /// <summary>
        /// 获取API节点列表
        /// </summary>
        public List<Node> GetApiNodes()
        {
            if (!_isInitialized)
            {
                Log.logger.Warn("节点管理服务未初始化");
                return new List<Node>();
            }
            return _apiNodes;
        }

        /// <summary>
        /// 根据名称查找下载节点Endpoint
        /// </summary>
        public string? FindDownloadNodeEndpoint(string nodeName)
        {
            if (!_isInitialized)
            {
                Log.logger.Warn("节点管理服务未初始化");
                return string.Empty;
            }

            foreach (var node in _downloadNodes)
            {
                if (node.Name == nodeName)
                {
                    return node.Endpoint;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 根据名称查找API节点Endpoint
        /// </summary>
        public string? FindApiNodeEndpoint(string nodeName)
        {
            if (!_isInitialized)
            {
                Log.logger.Warn("节点管理服务未初始化");
                return string.Empty;
            }

            foreach (var api in _apiNodes)
            {
                if (api.Name == nodeName)
                {
                    return api.Endpoint;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 获取默认下载节点Endpoint
        /// </summary>
        public string? GetDefaultDownloadNodeEndpoint()
        {
            if (!_isInitialized)
            {
                Log.logger.Warn("节点管理服务未初始化");
                return string.Empty;
            }
            return _defaultDownloadNodeEndpoint;
        }

        /// <summary>
        /// 获取默认API节点Endpoint
        /// </summary>
        public string? GetDefaultApiNodeEndpoint()
        {
            if (!_isInitialized)
            {
                Log.logger.Warn("节点管理服务未初始化");
                return string.Empty;
            }
            return _defaultApiNodeEndpoint;
        }
    }
}
