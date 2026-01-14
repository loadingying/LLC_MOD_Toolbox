namespace LLC_MOD_Toolbox.Interfaces
{
    /// <summary>
    /// 灰度测试服务接口
    /// 提供灰度测试Token验证和状态管理
    /// </summary>
    public interface IGreytestService
    {
        /// <summary>
        /// 验证灰度测试Token
        /// </summary>
        /// <param name="token">灰度测试Token</param>
        /// <param name="apiEndpoint">API端点</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>验证结果</returns>
        System.Threading.Tasks.Task<GreytestValidationResult> ValidateTokenAsync(
            string token,
            string apiEndpoint,
            System.Threading.CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取Token信息
        /// </summary>
        /// <param name="token">灰度测试Token</param>
        /// <param name="apiEndpoint">API端点</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>Token信息</returns>
        System.Threading.Tasks.Task<GreytestTokenInfo?> GetTokenInfoAsync(
            string token,
            string apiEndpoint,
            System.Threading.CancellationToken cancellationToken = default);

        /// <summary>
        /// 是否启用灰度测试模式
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// 获取灰度测试下载URL
        /// </summary>
        string? DownloadUrl { get; }

        /// <summary>
        /// 获取当前Token
        /// </summary>
        string? CurrentToken { get; }

        /// <summary>
        /// 启用灰度测试模式
        /// </summary>
        /// <param name="token">灰度测试Token</param>
        /// <param name="downloadUrl">下载URL</param>
        void Enable(string token, string downloadUrl);

        /// <summary>
        /// 禁用灰度测试模式
        /// </summary>
        void Disable();
    }

    /// <summary>
    /// 灰度测试验证结果
    /// </summary>
    public class GreytestValidationResult
    {
        /// <summary>
        /// 是否验证成功
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Token状态
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Token备注
        /// </summary>
        public string? Note { get; set; }
    }

    /// <summary>
    /// 灰度测试Token信息
    /// </summary>
    public class GreytestTokenInfo
    {
        /// <summary>
        /// Token状态（test=测试中, 其他=已停止）
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Token备注
        /// </summary>
        public string Note { get; set; } = string.Empty;

        /// <summary>
        /// 下载URL
        /// </summary>
        public string DownloadUrl { get; set; } = string.Empty;
    }
}
