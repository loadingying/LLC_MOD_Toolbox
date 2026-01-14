using LLC_MOD_Toolbox.Interfaces;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace LLC_MOD_Toolbox.Services
{
    /// <summary>
    /// 灰度测试服务实现
    /// 提供灰度测试Token验证和状态管理
    /// </summary>
    public class GreytestService : IGreytestService
    {
        private readonly HttpClient _httpClient;
        private bool _isEnabled;
        private string? _downloadUrl;
        private string? _currentToken;

        /// <summary>
        /// 初始化GreytestService
        /// </summary>
        public GreytestService()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "LLC_MOD_Toolbox");
        }

        /// <summary>
        /// 是否启用灰度测试模式
        /// </summary>
        public bool IsEnabled => _isEnabled;

        /// <summary>
        /// 获取灰度测试下载URL
        /// </summary>
        public string? DownloadUrl => _downloadUrl;

        /// <summary>
        /// 获取当前Token
        /// </summary>
        public string? CurrentToken => _currentToken;

        /// <summary>
        /// 验证灰度测试Token
        /// </summary>
        public async System.Threading.Tasks.Task<GreytestValidationResult> ValidateTokenAsync(
            string token,
            string apiEndpoint,
            System.Threading.CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    return new GreytestValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "Token为空"
                    };
                }

                // 获取Token信息
                var tokenInfo = await GetTokenInfoAsync(token, apiEndpoint, cancellationToken);

                if (tokenInfo == null)
                {
                    return new GreytestValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "无法获取Token信息"
                    };
                }

                // 验证Token状态
                if (tokenInfo.Status != "test")
                {
                    return new GreytestValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "Token已停止测试",
                        Status = tokenInfo.Status,
                        Note = tokenInfo.Note
                    };
                }

                return new GreytestValidationResult
                {
                    IsValid = true,
                    Status = tokenInfo.Status,
                    Note = tokenInfo.Note
                };
            }
            catch (Exception ex)
            {
                return new GreytestValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"验证Token失败：{ex.Message}"
                };
            }
        }

        /// <summary>
        /// 获取Token信息
        /// </summary>
        public async System.Threading.Tasks.Task<GreytestTokenInfo?> GetTokenInfoAsync(
            string token,
            string apiEndpoint,
            System.Threading.CancellationToken cancellationToken = default)
        {
            try
            {
                var tokenUrl = string.Format(apiEndpoint, $"v2/grey_test/get_token?code={token}");

                var response = await _httpClient.GetAsync(tokenUrl, cancellationToken);

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var tokenObject = JObject.Parse(content);

                return new GreytestTokenInfo
                {
                    Status = tokenObject["status"]?.Value<string>() ?? string.Empty,
                    Note = tokenObject["note"]?.Value<string>() ?? string.Empty,
                    DownloadUrl = string.Format(apiEndpoint, $"v2/grey_test/get_file?code={token}")
                };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 启用灰度测试模式
        /// </summary>
        public void Enable(string token, string downloadUrl)
        {
            _isEnabled = true;
            _currentToken = token;
            _downloadUrl = downloadUrl;
        }

        /// <summary>
        /// 禁用灰度测试模式
        /// </summary>
        public void Disable()
        {
            _isEnabled = false;
            _currentToken = null;
            _downloadUrl = null;
        }
    }
}
