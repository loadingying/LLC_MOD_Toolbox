namespace LLC_MOD_Toolbox.Interfaces
{
    /// <summary>
    /// 版本服务接口
    /// 提供版本检查和获取功能
    /// </summary>
    public interface IVersionService
    {
        /// <summary>
        /// 获取当前已安装的模组版本
        /// </summary>
        /// <param name="limbusCompanyDir">边狱公司游戏目录</param>
        /// <returns>当前版本号，未安装返回0，失败返回-1</returns>
        System.Threading.Tasks.Task<int> GetCurrentVersionAsync(string limbusCompanyDir);

        /// <summary>
        /// 获取最新的模组版本（从MirrorChyan）
        /// </summary>
        /// <param name="mirrorChyanToken">Mirror酱CDK</param>
        /// <returns>最新版本号，失败返回-100</returns>
        System.Threading.Tasks.Task<int> GetLatestVersionFromMirrorChyanAsync(string mirrorChyanToken);

        /// <summary>
        /// 获取最新的模组版本（从官方API）
        /// </summary>
        /// <param name="apiEndpoint">API端点</param>
        /// <returns>最新版本号，失败返回-100</returns>
        System.Threading.Tasks.Task<int> GetLatestVersionFromOfficialAsync(string apiEndpoint);

        /// <summary>
        /// 检查是否需要更新
        /// </summary>
        /// <param name="limbusCompanyDir">边狱公司游戏目录</param>
        /// <param name="useMirrorChyan">是否使用MirrorChyan</param>
        /// <param name="mirrorChyanToken">Mirror酱CDK（如果使用MirrorChyan）</param>
        /// <param name="apiEndpoint">官方API端点（如果不使用MirrorChyan）</param>
        /// <returns>版本检查结果</returns>
        System.Threading.Tasks.Task<VersionCheckResult> CheckForUpdateAsync(
            string limbusCompanyDir,
            bool useMirrorChyan,
            string mirrorChyanToken = "",
            string apiEndpoint = "");

        /// <summary>
        /// 格式化版本文本显示
        /// </summary>
        /// <param name="currentVersion">当前版本</param>
        /// <param name="latestVersion">最新版本</param>
        /// <returns>格式化后的文本对象</returns>
        VersionTextInfo FormatVersionText(int currentVersion, int latestVersion);
    }

    /// <summary>
    /// 版本检查结果
    /// </summary>
    public class VersionCheckResult
    {
        /// <summary>
        /// 是否需要更新
        /// </summary>
        public bool NeedUpdate { get; set; }

        /// <summary>
        /// 当前版本号
        /// </summary>
        public int CurrentVersion { get; set; }

        /// <summary>
        /// 最新版本号
        /// </summary>
        public int LatestVersion { get; set; }

        /// <summary>
        /// 当前版本文本
        /// </summary>
        public string CurrentVersionText { get; set; } = string.Empty;

        /// <summary>
        /// 最新版本文本
        /// </summary>
        public string LatestVersionText { get; set; } = string.Empty;

        /// <summary>
        /// 是否获取成功
        /// </summary>
        public bool IsSuccess { get; set; } = true;

        /// <summary>
        /// 错误消息（如果失败）
        /// </summary>
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// 版本文本信息
    /// </summary>
    public class VersionTextInfo
    {
        /// <summary>
        /// 当前版本文本
        /// </summary>
        public string CurrentVersionText { get; set; } = string.Empty;

        /// <summary>
        /// 最新版本文本
        /// </summary>
        public string LatestVersionText { get; set; } = string.Empty;
    }
}
