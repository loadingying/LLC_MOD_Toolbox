using LLC_MOD_Toolbox.Models;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;

namespace LLC_MOD_Toolbox.Services
{
    /// <summary>
    /// Mirror酱API客户端
    /// 提供Mirror酱镜像服务的API调用功能
    /// </summary>
    public class MirrorChyanApiClient
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://mirrorchyan.com/api";

        /// <summary>
        /// 初始化Mirror酱API客户端
        /// </summary>
        public MirrorChyanApiClient()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "LLC_MOD_Toolbox");
        }

        /// <summary>
        /// 获取资源信息
        /// </summary>
        /// <param name="resourceName">资源名称（如LLCCN-Font）</param>
        /// <param name="token">CDK</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>资源信息（URL和SHA256）</returns>
        public async System.Threading.Tasks.Task<(string url, string sha256)> GetResourceInfoAsync(
            string resourceName,
            string token,
            System.Threading.CancellationToken cancellationToken = default)
        {
            try
            {
                var url = $"{BaseUrl}/resources/{resourceName}/latest?user_agent=LLC_MOD_Toolbox&current_version=&cdk={token}";

                var response = await _httpClient.GetAsync(url, cancellationToken);

                // 检查是否为错误响应
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    HandleErrorResponse(errorContent);
                    return (string.Empty, string.Empty);
                }

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                var json = JObject.Parse(content);

                var data = json["data"];
                if (data == null)
                {
                    return (string.Empty, string.Empty);
                }

                var downloadUrl = data["url"]?.Value<string>() ?? string.Empty;
                var sha256 = data["sha256"]?.Value<string>() ?? string.Empty;

                return (downloadUrl, sha256);
            }
            catch (MirrorChyanException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"获取Mirror酱资源失败: {resourceName}", ex);
            }
        }

        /// <summary>
        /// 获取模组信息
        /// </summary>
        /// <param name="token">CDK</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>模组版本信息</returns>
        public async System.Threading.Tasks.Task<(int version, string url, string sha256)> GetModInfoAsync(
            string token,
            System.Threading.CancellationToken cancellationToken = default)
        {
            try
            {
                var url = $"{BaseUrl}/resources/LLCCN-Mod/latest?user_agent=LLC_MOD_Toolbox&current_version=&cdk={token}";

                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    HandleErrorResponse(errorContent);
                    return (-1, string.Empty, string.Empty);
                }

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                var json = JObject.Parse(content);

                var data = json["data"];
                if (data == null)
                {
                    return (-1, string.Empty, string.Empty);
                }

                var version = data["version"]?.Value<int>() ?? -1;
                var downloadUrl = data["url"]?.Value<string>() ?? string.Empty;
                var sha256 = data["sha256"]?.Value<string>() ?? string.Empty;

                return (version, downloadUrl, sha256);
            }
            catch (MirrorChyanException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("获取Mirror酱模组信息失败", ex);
            }
        }

        /// <summary>
        /// 处理Mirror酱错误响应
        /// </summary>
        private void HandleErrorResponse(string errorContent)
        {
            try
            {
                var errorJson = JObject.Parse(errorContent);
                var code = errorJson["code"]?.Value<int>();

                if (code.HasValue)
                {
                    throw new MirrorChyanException(code.Value);
                }

                throw new MirrorChyanException(0);
            }
            catch (MirrorChyanException)
            {
                throw;
            }
            catch
            {
                throw new MirrorChyanException(0);
            }
        }

        /// <summary>
        /// 验证CDK是否有效
        /// </summary>
        /// <param name="token">CDK</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>是否有效</returns>
        public async System.Threading.Tasks.Task<bool> ValidateTokenAsync(
            string token,
            System.Threading.CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await GetResourceInfoAsync("LLCCN-Font", token, cancellationToken);
                return !string.IsNullOrEmpty(result.url);
            }
            catch
            {
                return false;
            }
        }
    }
}
