using LLC_MOD_Toolbox.Interfaces;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace LLC_MOD_Toolbox.Services
{
    /// <summary>
    /// 公告服务实现
    /// 负责检查、获取和管理系统公告
    /// </summary>
    public class AnnouncementService : IAnnouncementService
    {
        private readonly HttpClient _httpClient;
        private int? _lastReadVersion;

        /// <summary>
        /// 初始化AnnouncementService
        /// </summary>
        public AnnouncementService()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
        }

        /// <summary>
        /// 检查是否有新公告
        /// </summary>
        public async System.Threading.Tasks.Task<AnnouncementInfo?> CheckForNewAnnouncementAsync(
            string apiEndpoint,
            int currentVersion,
            System.Threading.CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(apiEndpoint))
                {
                    Log.logger.Warn("API端点为空，跳过公告检查");
                    return null;
                }

                // 获取最新公告
                var announcement = await FetchAnnouncementAsync(apiEndpoint, cancellationToken);
                if (announcement == null)
                {
                    return null;
                }

                // 比较版本号
                announcement.IsNew = announcement.Version > currentVersion;

                if (announcement.IsNew)
                {
                    Log.logger.Info($"发现新公告，版本：{announcement.Version}，当前版本：{currentVersion}");
                }
                else
                {
                    Log.logger.Info($"无新公告。最新版本：{announcement.Version}，当前版本：{currentVersion}");
                }

                return announcement;
            }
            catch (Exception ex)
            {
                Log.logger.Error("检查公告失败", ex);
                return null;
            }
        }

        /// <summary>
        /// 从API获取公告信息
        /// </summary>
        public async System.Threading.Tasks.Task<AnnouncementInfo?> FetchAnnouncementAsync(
            string apiEndpoint,
            System.Threading.CancellationToken cancellationToken = default)
        {
            try
            {
                string url = string.Format(apiEndpoint, "v2/announcement/get_anno");
                Log.logger.Info($"从API获取公告，URL：{url}");

                string response = await _httpClient.GetStringAsync(url, cancellationToken);
                if (string.IsNullOrWhiteSpace(response))
                {
                    Log.logger.Warn("API返回空响应");
                    return null;
                }

                // 解析JSON
                var json = JObject.Parse(response);
                int version = json["version"]?.Value<int>() ?? 0;
                string content = json["anno"]?.Value<string>() ?? string.Empty;
                string levelStr = json["level"]?.Value<string>() ?? "normal";

                // 解析公告级别
                if (!Enum.TryParse<AnnouncementLevel>(levelStr, true, out var level))
                {
                    level = AnnouncementLevel.Normal;
                }

                // 处理换行符
                content = content.Replace("\\n", "\n");

                var announcement = new AnnouncementInfo
                {
                    Version = version,
                    Content = content,
                    Level = level,
                    CountdownSeconds = GetCountdownSeconds(level)
                };

                Log.logger.Info($"成功获取公告，版本：{version}，级别：{level}");
                return announcement;
            }
            catch (HttpRequestException ex)
            {
                Log.logger.Error("网络请求失败，无法获取公告", ex);
                return null;
            }
            catch (Exception ex)
            {
                Log.logger.Error("解析公告数据失败", ex);
                return null;
            }
        }

        /// <summary>
        /// 根据公告级别获取倒计时秒数
        /// </summary>
        public int GetCountdownSeconds(AnnouncementLevel level)
        {
            return level switch
            {
                AnnouncementLevel.Normal => 0,
                AnnouncementLevel.Important => 5,
                AnnouncementLevel.Special => 15,
                _ => 0
            };
        }

        /// <summary>
        /// 标记公告为已读
        /// </summary>
        public void MarkAsRead(int version)
        {
            _lastReadVersion = version;
            Log.logger.Info($"公告已标记为已读，版本：{version}");
        }

        /// <summary>
        /// 清除已读标记
        /// </summary>
        public void ClearReadStatus()
        {
            _lastReadVersion = null;
            Log.logger.Info("已清除公告已读标记");
        }
    }
}
