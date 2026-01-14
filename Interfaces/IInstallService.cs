using LLC_MOD_Toolbox.Models;

namespace LLC_MOD_Toolbox.Interfaces
{
    /// <summary>
    /// 安装服务接口
    /// 提供模组安装、卸载、版本检查等功能
    /// </summary>
    public interface IInstallService
    {
        /// <summary>
        /// 异步安装模组
        /// </summary>
        /// <param name="progress">进度报告回调</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>安装结果</returns>
        System.Threading.Tasks.Task<InstallResult> InstallAsync(
            System.IProgress<float>? progress = null,
            System.Threading.CancellationToken cancellationToken = default);

        /// <summary>
        /// 异步卸载模组
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        System.Threading.Tasks.Task UninstallAsync(System.Threading.CancellationToken cancellationToken = default);

        /// <summary>
        /// 检查是否有可用更新
        /// </summary>
        /// <returns>如果有新版本返回true，否则返回false</returns>
        System.Threading.Tasks.Task<bool> CheckUpdateAsync();

        /// <summary>
        /// 获取当前已安装的模组版本
        /// </summary>
        System.Threading.Tasks.Task<ModVersion?> GetCurrentVersionAsync();

        /// <summary>
        /// 获取最新的模组版本
        /// </summary>
        System.Threading.Tasks.Task<ModVersion?> GetLatestVersionAsync();

        /// <summary>
        /// 验证游戏环境
        /// </summary>
        /// <returns>验证结果</returns>
        System.Threading.Tasks.Task<ValidationResult> ValidateEnvironmentAsync();
    }

    /// <summary>
    /// 安装结果
    /// </summary>
    public class InstallResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 错误消息（失败时）
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 是否为最新版本
        /// </summary>
        public bool IsLatestVersion { get; set; }

        /// <summary>
        /// 安装的版本号
        /// </summary>
        public int InstalledVersion { get; set; }
    }

    /// <summary>
    /// 模组版本信息
    /// </summary>
    public class ModVersion
    {
        /// <summary>
        /// 版本号
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// 版本字符串
        /// </summary>
        public string VersionString { get; set; } = string.Empty;

        /// <summary>
        /// 发布日期
        /// </summary>
        public System.DateTime? ReleaseDate { get; set; }
    }

    /// <summary>
    /// 环境验证结果
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// 是否验证通过
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// 验证错误列表
        /// </summary>
        public List<string> Errors { get; set; } = new();

        /// <summary>
        /// 验证警告列表
        /// </summary>
        public List<string> Warnings { get; set; } = new();
    }
}
