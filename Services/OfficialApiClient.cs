using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace LLC_MOD_Toolbox.Services
{
    /// <summary>
    /// 官方API客户端
    /// 提供官方API的调用功能
    /// </summary>
    public class OfficialApiClient
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// 初始化官方API客户端
        /// </summary>
        public OfficialApiClient()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "LLC_MOD_Toolbox");
        }

        /// <summary>
        /// 获取最新版本号
        /// </summary>
        /// <param name="apiEndpoint">API端点</param>
        /// <param name="useGithub">是否使用GitHub</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>版本号，失败返回-100</returns>
        public async System.Threading.Tasks.Task<int> GetLatestVersionAsync(
            string apiEndpoint,
            bool useGithub = false,
            System.Threading.CancellationToken cancellationToken = default)
        {
            try
            {
                string version;

                if (useGithub)
                {
                    // 从GitHub API获取
                    var response = await _httpClient.GetAsync(
                        "https://api.github.com/repos/LocalizeLimbusCompany/LocalizeLimbusCompany/releases/latest",
                        cancellationToken);

                    response.EnsureSuccessStatusCode();
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    var json = JObject.Parse(content);
                    version = json["tag_name"]?.Value<string>() ?? string.Empty;
                }
                else
                {
                    // 从官方API获取
                    var response = await _httpClient.GetAsync(
                        string.Format(apiEndpoint, "v2/resource/get_version"),
                        cancellationToken);

                    response.EnsureSuccessStatusCode();
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    var json = JObject.Parse(content);
                    version = json["version"]?.Value<string>() ?? string.Empty;
                }

                if (string.IsNullOrEmpty(version))
                {
                    return -100;
                }

                return int.Parse(version);
            }
            catch (Exception)
            {
                return -100;
            }
        }

        /// <summary>
        /// 获取Hash值
        /// </summary>
        /// <param name="apiEndpoint">API端点</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>Hash对象</returns>
        public async System.Threading.Tasks.Task<JObject?> GetHashAsync(
            string apiEndpoint,
            System.Threading.CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    string.Format(apiEndpoint, "v2/hash/get_hash"),
                    cancellationToken);

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                return JObject.Parse(content);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 获取公告信息
        /// </summary>
        /// <param name="apiEndpoint">API端点</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>公告内容</returns>
        public async System.Threading.Tasks.Task<string?> GetAnnouncementAsync(
            string apiEndpoint,
            System.Threading.CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    string.Format(apiEndpoint, "v2/announcement/get_anno"),
                    cancellationToken);

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return content;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 获取加载文本
        /// </summary>
        /// <param name="apiEndpoint">API端点</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>加载文本列表</returns>
        public async System.Threading.Tasks.Task<List<string>?> GetLoadingTextAsync(
            string apiEndpoint,
            System.Threading.CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    string.Format(apiEndpoint, "v2/loading/get_loading"),
                    cancellationToken);

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var json = JObject.Parse(content);

                var texts = json["text"]?.ToObject<List<string>>();
                return texts;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
