namespace LLC_MOD_Toolbox.Interfaces
{
    /// <summary>
    /// 公告级别
    /// </summary>
    public enum AnnouncementLevel
    {
        /// <summary>
        /// 普通公告 - 不需要强制阅读
        /// </summary>
        Normal = 0,

        /// <summary>
        /// 重要公告 - 需要5秒倒计时
        /// </summary>
        Important = 1,

        /// <summary>
        /// 特别公告 - 需要15秒倒计时
        /// </summary>
        Special = 2
    }

    /// <summary>
    /// 公告信息模型
    /// </summary>
    public class AnnouncementInfo
    {
        /// <summary>
        /// 公告版本号
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// 公告内容
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 公告级别
        /// </summary>
        public AnnouncementLevel Level { get; set; }

        /// <summary>
        /// 是否为新公告（与本地版本比较）
        /// </summary>
        public bool IsNew { get; set; }

        /// <summary>
        /// 倒计时秒数（根据级别计算）
        /// </summary>
        public int CountdownSeconds { get; set; }
    }

    /// <summary>
    /// 公告服务接口
    /// 负责检查、获取和管理系统公告
    /// </summary>
    public interface IAnnouncementService
    {
        /// <summary>
        /// 检查是否有新公告
        /// </summary>
        /// <param name="apiEndpoint">API端点</param>
        /// <param name="currentVersion">当前本地公告版本</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>公告信息，如果没有新公告则返回null</returns>
        System.Threading.Tasks.Task<AnnouncementInfo?> CheckForNewAnnouncementAsync(
            string apiEndpoint,
            int currentVersion,
            System.Threading.CancellationToken cancellationToken = default);

        /// <summary>
        /// 从API获取公告信息
        /// </summary>
        /// <param name="apiEndpoint">API端点</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>公告信息</returns>
        System.Threading.Tasks.Task<AnnouncementInfo?> FetchAnnouncementAsync(
            string apiEndpoint,
            System.Threading.CancellationToken cancellationToken = default);

        /// <summary>
        /// 根据公告级别获取倒计时秒数
        /// </summary>
        /// <param name="level">公告级别</param>
        /// <returns>倒计时秒数</returns>
        int GetCountdownSeconds(AnnouncementLevel level);

        /// <summary>
        /// 标记公告为已读
        /// </summary>
        /// <param name="version">公告版本号</param>
        void MarkAsRead(int version);

        /// <summary>
        /// 清除已读标记
        /// </summary>
        void ClearReadStatus();
    }
}
