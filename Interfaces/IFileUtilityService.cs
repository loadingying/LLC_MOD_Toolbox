namespace LLC_MOD_Toolbox.Interfaces
{
    /// <summary>
    /// 文件工具服务接口
    /// 提供文件操作、下载、解压等常用功能
    /// </summary>
    public interface IFileUtilityService
    {
        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="url">下载地址</param>
        /// <param name="path">保存路径</param>
        /// <param name="progressChanged">进度变化回调</param>
        /// <param name="completed">完成回调</param>
        System.Threading.Tasks.Task DownloadFileAsync(
            string url,
            string path,
            System.EventHandler<Downloader.DownloadProgressChangedEventArgs>? progressChanged = null,
            System.EventHandler<System.ComponentModel.AsyncCompletedEventArgs>? completed = null);

        /// <summary>
        /// 下载文件（无进度报告）
        /// </summary>
        System.Threading.Tasks.Task DownloadFileAsyncWithoutProgress(string url, string path);

        /// <summary>
        /// 自动选择下载节点下载文件
        /// </summary>
        /// <param name="file">文件名</param>
        /// <param name="path">保存路径</param>
        /// <param name="useEndPoint">使用的端点</param>
        /// <param name="defaultEndPoint">默认端点</param>
        System.Threading.Tasks.Task DownloadFileAutoAsync(string file, string path, string? useEndPoint = null, string? defaultEndPoint = null);

        /// <summary>
        /// 解压7z文件
        /// </summary>
        /// <param name="archivePath">压缩文件路径</param>
        /// <param name="outputDirectory">输出目录</param>
        void Unarchive(string archivePath, string outputDirectory);

        /// <summary>
        /// 计算文件的SHA256哈希值
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>SHA256哈希值（小写十六进制）</returns>
        string CalculateSHA256(string filePath);

        /// <summary>
        /// 删除文件（如果存在）
        /// </summary>
        /// <param name="path">文件路径</param>
        void DeleteFile(string path);

        /// <summary>
        /// 删除目录（如果存在）
        /// </summary>
        /// <param name="path">目录路径</param>
        void DeleteDirectory(string path);

        /// <summary>
        /// 获取URL文本内容
        /// </summary>
        /// <param name="url">URL地址</param>
        /// <param name="reportError">是否报告错误</param>
        /// <param name="maxRetries">最大重试次数</param>
        /// <param name="delayMs">重试延迟（毫秒）</param>
        /// <param name="parseErrorJson">是否解析错误JSON</param>
        System.Threading.Tasks.Task<string> GetURLText(
            string url,
            bool reportError = true,
            int maxRetries = 3,
            int delayMs = 300,
            bool parseErrorJson = false);

        /// <summary>
        /// 打开URL（使用系统默认浏览器）
        /// </summary>
        /// <param name="url">URL地址</param>
        void OpenUrl(string url);

        /// <summary>
        /// 检查字体文件是否有效
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
    }
}
