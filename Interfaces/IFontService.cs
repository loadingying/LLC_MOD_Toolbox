namespace LLC_MOD_Toolbox.Interfaces
{
    /// <summary>
    /// 字体服务接口
    /// 提供字体验证、替换、还原和预览功能
    /// </summary>
    public interface IFontService
    {
        /// <summary>
        /// 验证字体文件是否有效
        /// </summary>
        /// <param name="filePath">字体文件路径</param>
        /// <returns>是否为有效的字体文件</returns>
        bool IsValidFontFile(string filePath);

        /// <summary>
        /// 获取字体家族名称
        /// </summary>
        /// <param name="filePath">字体文件路径</param>
        /// <returns>字体家族名称</returns>
        string GetFontFamilyName(string filePath);

        /// <summary>
        /// 替换游戏字体
        /// </summary>
        /// <param name="fontFilePath">新字体文件路径</param>
        /// <param name="gameDirectory">游戏目录</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>替换结果</returns>
        System.Threading.Tasks.Task<FontReplaceResult> ReplaceFontAsync(
            string fontFilePath,
            string gameDirectory,
            System.Threading.CancellationToken cancellationToken = default);

        /// <summary>
        /// 还原原始字体
        /// </summary>
        /// <param name="gameDirectory">游戏目录</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>还原结果</returns>
        System.Threading.Tasks.Task<FontRestoreResult> RestoreFontAsync(
            string gameDirectory,
            System.Threading.CancellationToken cancellationToken = default);

        /// <summary>
        /// 检查是否有字体备份
        /// </summary>
        /// <param name="gameDirectory">游戏目录</param>
        /// <returns>是否有备份</returns>
        bool HasBackupFont(string gameDirectory);

        /// <summary>
        /// 检查游戏是否已安装汉化
        /// </summary>
        /// <param name="gameDirectory">游戏目录</param>
        /// <returns>是否已安装汉化</returns>
        bool HasChineseFontInstalled(string gameDirectory);
    }

    /// <summary>
    /// 字体替换结果
    /// </summary>
    public class FontReplaceResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 是否已备份原字体
        /// </summary>
        public bool HasBackup { get; set; }

        /// <summary>
        /// 替换的字体文件路径
        /// </summary>
        public string? ReplacedFontPath { get; set; }
    }

    /// <summary>
    /// 字体还原结果
    /// </summary>
    public class FontRestoreResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 还原的字体文件路径
        /// </summary>
        public string? RestoredFontPath { get; set; }
    }
}
