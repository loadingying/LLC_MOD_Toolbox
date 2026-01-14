namespace LLC_MOD_Toolbox.Interfaces
{
    /// <summary>
    /// 下载服务接口
    /// 提供文件下载、网络请求等功能
    /// </summary>
    public interface IDownloadService
    {
        /// <summary>
        /// 异步下载文件
        /// </summary>
        /// <param name="url">下载URL</param>
        /// <param name="destination">目标文件路径</param>
        /// <param name="progress">进度报告回调</param>
        /// <param name="cancellationToken">取消令牌</param>
        System.Threading.Tasks.Task DownloadFileAsync(
            string url,
            string destination,
            System.IProgress<float>? progress = null,
            System.Threading.CancellationToken cancellationToken = default);

        /// <summary>
        /// 异步下载并获取文本内容
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>文本内容</returns>
        System.Threading.Tasks.Task<string> DownloadStringAsync(
            string url,
            System.Threading.CancellationToken cancellationToken = default);

        /// <summary>
        /// 异步下载并反序列化JSON
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="url">URL</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>反序列化后的对象</returns>
        System.Threading.Tasks.Task<T> DownloadJsonAsync<T>(
            string url,
            System.Threading.CancellationToken cancellationToken = default);

        /// <summary>
        /// 计算文件的SHA256哈希值
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>SHA256哈希值（十六进制字符串）</returns>
        string CalculateSHA256(string filePath);
    }
}
