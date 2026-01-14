namespace LLC_MOD_Toolbox.Models
{
    /// <summary>
    /// 节点列表根模型
    /// 用于反序列化NodeList.json，包含下载节点和API节点
    /// </summary>
    public class RootModel
    {
        /// <summary>
        /// 下载节点列表
        /// </summary>
        public List<Node> DownloadNode { get; set; } = new List<Node>();

        /// <summary>
        /// API节点列表
        /// </summary>
        public List<Node> ApiNode { get; set; } = new List<Node>();

        /// <summary>
        /// Loading文本列表（用于loadingText.json）
        /// </summary>
        public List<string>? LoadingText { get; set; }

        /// <summary>
        /// Loading文本更新日期（用于loadingText.json）
        /// </summary>
        public string? LoadingDate { get; set; }
    }

    /// <summary>
    /// 节点信息模型
    /// 表示单个下载或API节点的配置信息
    /// </summary>
    public class Node
    {
        /// <summary>
        /// 节点显示名称（如"镇江节点"、"CloudFlare CDN"）
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 节点端点URL模板
        /// 使用{0}作为占位符，例如：https://example.com/files/{0}
        /// </summary>
        public string Endpoint { get; set; } = string.Empty;

        /// <summary>
        /// 是否为默认节点
        /// 根据用户地区（国内/国际）自动选择合适的默认节点
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// 节点类型（可选，用于区分不同类型的节点）
        /// </summary>
        public string? Type { get; set; }
    }
}
