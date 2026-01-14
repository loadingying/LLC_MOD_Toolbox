using LLC_MOD_Toolbox.Interfaces;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace LLC_MOD_Toolbox.Services
{
    /// <summary>
    /// 下载服务实现
    /// 使用Downloader库提供文件下载和网络请求功能
    /// </summary>
    public class DownloadService : IDownloadService
    {
        private readonly HttpClient _httpClient;
        private readonly string _userAgent = "LLC_MOD_Toolbox";

        /// <summary>
        /// 初始化下载服务
        /// </summary>
        public DownloadService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", _userAgent);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        internal DownloadService(HttpMessageHandler handler)
        {
            _httpClient = new HttpClient(handler);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", _userAgent);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        /// <summary>
        /// 异步下载文件
        /// </summary>
        public async System.Threading.Tasks.Task DownloadFileAsync(
            string url,
            string destination,
            System.IProgress<float>? progress = null,
            System.Threading.CancellationToken cancellationToken = default)
        {
            var downloadOpt = new Downloader.DownloadConfiguration()
            {
                BufferBlockSize = 10240,
                ChunkCount = 8,
                MaxTryAgainOnFailover = 5,
            };

            var downloader = new Downloader.DownloadService(downloadOpt);

            if (progress != null)
            {
                downloader.DownloadProgressChanged += (s, e) =>
                {
                    progress.Report((float)e.ProgressPercentage);
                };
            }

            await downloader.DownloadFileTaskAsync(url, destination);
        }

        /// <summary>
        /// 异步下载并获取文本内容
        /// </summary>
        public async System.Threading.Tasks.Task<string> DownloadStringAsync(
            string url,
            System.Threading.CancellationToken cancellationToken = default)
        {
            // 使用重试机制
            int maxRetries = 3;
            int delayMs = 300;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    var response = await _httpClient.GetAsync(url, cancellationToken);
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync(cancellationToken);
                }
                catch (HttpRequestException) when (attempt < maxRetries)
                {
                    await System.Threading.Tasks.Task.Delay(delayMs, cancellationToken);
                    delayMs *= 2; // 指数退避
                }
            }

            // 最后一次尝试
            var finalResponse = await _httpClient.GetAsync(url, cancellationToken);
            finalResponse.EnsureSuccessStatusCode();
            return await finalResponse.Content.ReadAsStringAsync(cancellationToken);
        }

        /// <summary>
        /// 异步下载并反序列化JSON
        /// </summary>
        public async System.Threading.Tasks.Task<T> DownloadJsonAsync<T>(
            string url,
            System.Threading.CancellationToken cancellationToken = default)
        {
            var json = await DownloadStringAsync(url, cancellationToken);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json)
                ?? throw new InvalidOperationException($"无法反序列化JSON到类型 {typeof(T).Name}");
        }

        /// <summary>
        /// 计算文件的SHA256哈希值
        /// </summary>
        public string CalculateSHA256(string filePath)
        {
            using var sha256 = SHA256.Create();
            using var fileStream = System.IO.File.OpenRead(filePath);
            byte[] hashBytes = sha256.ComputeHash(fileStream);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
