using LLC_MOD_Toolbox.Models;

namespace LLC_MOD_Toolbox.Interfaces
{
    /// <summary>
    /// 节点管理服务接口
    /// 处理节点配置的加载、查询和切换逻辑
    /// </summary>
    public interface INodeManagementService
    {
        /// <summary>
        /// 初始化节点列表（从JSON文件加载）
        /// </summary>
        /// <param name="nodeListJsonPath">NodeList.json文件路径</param>
        /// <param name="internationalMode">是否为国际模式</param>
        void InitializeNodes(string nodeListJsonPath, bool internationalMode);

        /// <summary>
        /// 获取下载节点列表
        /// </summary>
        List<Node> GetDownloadNodes();

        /// <summary>
        /// 获取API节点列表
        /// </summary>
        List<Node> GetApiNodes();

        /// <summary>
        /// 根据名称查找下载节点Endpoint
        /// </summary>
        /// <param name="nodeName">节点名称</param>
        /// <returns>节点Endpoint，未找到返回空字符串</returns>
        string? FindDownloadNodeEndpoint(string nodeName);

        /// <summary>
        /// 根据名称查找API节点Endpoint
        /// </summary>
        /// <param name="nodeName">节点名称</param>
        /// <returns>节点Endpoint，未找到返回空字符串</returns>
        string? FindApiNodeEndpoint(string nodeName);

        /// <summary>
        /// 获取默认下载节点Endpoint
        /// </summary>
        string? GetDefaultDownloadNodeEndpoint();

        /// <summary>
        /// 获取默认API节点Endpoint
        /// </summary>
        string? GetDefaultApiNodeEndpoint();

        /// <summary>
        /// 是否已初始化
        /// </summary>
        bool IsInitialized { get; }
    }
}
