using LLC_MOD_Toolbox;
using LLC_MOD_Toolbox.Interfaces;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace LLC_MOD_Toolbox.Services
{
    /// <summary>
    /// Mirror酱密钥无效异常
    /// </summary>
    public class MirrorChyanInvalidKeyException : Exception
    {
        public MirrorChyanInvalidKeyException() : base("Mirror酱秘钥无效或已过期")
        {
        }

        public MirrorChyanInvalidKeyException(string message) : base(message)
        {
        }

        public MirrorChyanInvalidKeyException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// 版本服务实现
    /// 提供版本检查和获取功能
    /// </summary>
    public class VersionService : IVersionService
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// 初始化VersionService
        /// </summary>
        public VersionService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("LLC_MOD_Toolbox");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// 获取当前已安装的模组版本
        /// </summary>
        public async System.Threading.Tasks.Task<int> GetCurrentVersionAsync(string limbusCompanyDir)
        {
            if (string.IsNullOrEmpty(limbusCompanyDir))
            {
                return 0;
            }

            try
            {
                string langDir = Path.Combine(limbusCompanyDir, "LimbusCompany_Data/Lang/LLC_zh-CN");
                string versionJsonPath = Path.Combine(langDir, "Info", "version.json");

                if (!File.Exists(versionJsonPath))
                {
                    return 0; // 未安装
                }

                string json = await File.ReadAllTextAsync(versionJsonPath);
                JObject versionObj = JObject.Parse(json);
                return versionObj["version"]?.Value<int>() ?? 0;
            }
            catch
            {
                return -1; // 解析失败
            }
        }

        /// <summary>
        /// 获取最新的模组版本（从MirrorChyan）
        /// </summary>
        public async System.Threading.Tasks.Task<int> GetLatestVersionFromMirrorChyanAsync(string mirrorChyanToken)
        {
            try
            {
                Log.logger.Info("从MirrorChyan获取模组版本。");
                string url = $"https://mirrorchyan.com/api/resources/LLC/latest?user_agent=LLC_MOD_Toolbox&current_version=&cdk={mirrorChyanToken}";

                // 使用SendAsync而不是GetStringAsync以便检查HTTP状态码
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        Log.logger.Error($"Mirror酱秘钥无效或已过期 (403 Forbidden)");
                        throw new MirrorChyanInvalidKeyException();
                    }
                    response.EnsureSuccessStatusCode();
                }

                string raw = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(raw);
                string version = json["data"]["version_name"]?.Value<string>() ?? "0";

                Log.logger.Info($"汉化模组最新版本为：{version}");
                return int.Parse(version);
            }
            catch (MirrorChyanInvalidKeyException)
            {
                // 重新抛出自定义异常，不捕获
                throw;
            }
            catch (System.Exception ex)
            {
                Log.logger.Error("从MirrorChyan获取模组版本失败。", ex);
                return -100;
            }
        }

        /// <summary>
        /// 获取最新的模组版本（从官方API）
        /// </summary>
        public async System.Threading.Tasks.Task<int> GetLatestVersionFromOfficialAsync(string apiEndpoint)
        {
            try
            {
                Log.logger.Info($"从官方API获取模组版本，API端点：{apiEndpoint}");

                if (string.IsNullOrWhiteSpace(apiEndpoint))
                {
                    Log.logger.Error("API端点为空");
                    return -100;
                }

                string url = string.Format(apiEndpoint, "v2/resource/get_version");
                Log.logger.Info($"请求URL：{url}");

                string raw = await _httpClient.GetStringAsync(url);
                Log.logger.Info($"API响应：{raw}");

                var json = JObject.Parse(raw);
                string version = json["version"]?.Value<string>() ?? "0";

                Log.logger.Info($"汉化模组最新版本为：{version}");
                return int.Parse(version);
            }
            catch (System.Exception ex)
            {
                Log.logger.Error("从官方API获取模组版本失败。", ex);
                return -100;
            }
        }

        /// <summary>
        /// 检查是否需要更新
        /// </summary>
        public async System.Threading.Tasks.Task<VersionCheckResult> CheckForUpdateAsync(
            string limbusCompanyDir,
            bool useMirrorChyan,
            string mirrorChyanToken = "",
            string apiEndpoint = "")
        {
            var result = new VersionCheckResult();

            try
            {
                // 获取最新版本
                int latestVersion;
                if (useMirrorChyan)
                {
                    latestVersion = await GetLatestVersionFromMirrorChyanAsync(mirrorChyanToken);
                }
                else
                {
                    latestVersion = await GetLatestVersionFromOfficialAsync(apiEndpoint);
                }

                result.LatestVersion = latestVersion;

                if (latestVersion == -100)
                {
                    result.LatestVersionText = "最新版本：获取失败";
                    result.IsSuccess = false;
                    result.ErrorMessage = "无法获取最新版本信息";
                    return result;
                }

                result.LatestVersionText = $"最新版本：{latestVersion}";

                // 获取当前版本
                int currentVersion = await GetCurrentVersionAsync(limbusCompanyDir);
                result.CurrentVersion = currentVersion;

                if (currentVersion == 0)
                {
                    result.CurrentVersionText = "当前版本：未安装";
                    result.NeedUpdate = true;
                }
                else if (currentVersion == -1)
                {
                    result.CurrentVersionText = "当前版本：解析失败";
                    result.IsSuccess = false;
                    result.ErrorMessage = "无法解析当前版本信息";
                }
                else
                {
                    result.CurrentVersionText = $"当前版本：{currentVersion}";
                    // 检查是否需要更新
                    if (currentVersion < latestVersion)
                    {
                        result.CurrentVersionText = $"当前版本：{currentVersion}（可更新）";
                        result.NeedUpdate = true;
                    }
                }

                result.IsSuccess = true;
            }
            catch (MirrorChyanInvalidKeyException ex)
            {
                Log.logger.Error("Mirror酱秘钥无效或已过期", ex);
                result.IsSuccess = false;
                result.ErrorMessage = "Mirror酱秘钥无效或已过期，请重新配置";
                result.LatestVersionText = "最新版本：秘钥无效";
            }
            catch (System.Exception ex)
            {
                Log.logger.Error("检查版本失败", ex);
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 格式化版本文本显示
        /// </summary>
        public VersionTextInfo FormatVersionText(int currentVersion, int latestVersion)
        {
            var info = new VersionTextInfo();

            info.LatestVersionText = latestVersion switch
            {
                -100 => "最新版本：获取失败",
                _ => $"最新版本：{latestVersion}"
            };

            info.CurrentVersionText = currentVersion switch
            {
                0 => "当前版本：未安装",
                -1 => "当前版本：解析失败",
                _ when currentVersion < latestVersion => $"当前版本：{currentVersion}（可更新）",
                _ => $"当前版本：{currentVersion}"
            };

            return info;
        }
    }
}
