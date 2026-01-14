using Downloader;
using LLC_MOD_Toolbox;
using LLC_MOD_Toolbox.Interfaces;
using LLC_MOD_Toolbox.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SevenZip;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace LLC_MOD_Toolbox.Services
{
    /// <summary>
    /// 文件工具服务实现
    /// 提供文件操作、下载、解压等常用功能
    /// </summary>
    public class FileUtilityService : IFileUtilityService
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// 初始化FileUtilityService
        /// </summary>
        public FileUtilityService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("LLC_MOD_Toolbox");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        public async System.Threading.Tasks.Task DownloadFileAsync(
            string url,
            string path,
            System.EventHandler<Downloader.DownloadProgressChangedEventArgs>? progressChanged = null,
            System.EventHandler<System.ComponentModel.AsyncCompletedEventArgs>? completed = null)
        {
            Log.logger.Info($"下载 {url} 到 {path}");
            var downloadOpt = new DownloadConfiguration()
            {
                BufferBlockSize = 10240,
                ChunkCount = 8,
                MaxTryAgainOnFailover = 5,
            };
            var downloader = new Downloader.DownloadService(downloadOpt);

            if (progressChanged != null)
            {
                downloader.DownloadProgressChanged += progressChanged;
            }

            if (completed != null)
            {
                downloader.DownloadFileCompleted += completed;
            }

            await downloader.DownloadFileTaskAsync(url, path);
        }

        /// <summary>
        /// 下载文件（无进度报告）
        /// </summary>
        public async System.Threading.Tasks.Task DownloadFileAsyncWithoutProgress(string url, string path)
        {
            Log.logger.Info($"下载 {url} 到 {path}");
            var downloadOpt = new DownloadConfiguration()
            {
                BufferBlockSize = 10240,
                ChunkCount = 8,
                MaxTryAgainOnFailover = 5,
            };
            var downloader = new Downloader.DownloadService(downloadOpt);
            await downloader.DownloadFileTaskAsync(url, path);
        }

        /// <summary>
        /// 自动选择下载节点下载文件
        /// </summary>
        public async System.Threading.Tasks.Task DownloadFileAutoAsync(
            string file,
            string path,
            string? useEndPoint = null,
            string? defaultEndPoint = null)
        {
            Log.logger.Info($"自动选择下载节点式下载文件 文件: {file}  路径: {path}");

            if (!string.IsNullOrEmpty(useEndPoint))
            {
                string downloadUrl = string.Format(useEndPoint, file);
                await DownloadFileAsync(downloadUrl, path);
            }
            else
            {
                string downloadUrl = string.Format(defaultEndPoint ?? "", file);
                await DownloadFileAsync(downloadUrl, path);
            }
        }

        /// <summary>
        /// 解压7z文件
        /// </summary>
        public void Unarchive(string archivePath, string outputDirectory)
        {
            using SevenZipExtractor extractor = new(archivePath);
            extractor.ExtractArchive(outputDirectory);
        }

        /// <summary>
        /// 计算文件的SHA256哈希值
        /// </summary>
        public string CalculateSHA256(string filePath)
        {
            using var sha256 = SHA256.Create();
            using var fileStream = File.OpenRead(filePath);
            byte[] hashBytes = sha256.ComputeHash(fileStream);
            string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            Log.logger.Info($"计算位置为 {filePath} 的文件的Hash结果为：{hash}");
            return hash;
        }

        /// <summary>
        /// 删除文件（如果存在）
        /// </summary>
        public void DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                Log.logger.Info($"删除文件： {path}");
                File.Delete(path);
            }
            else
            {
                Log.logger.Info($"文件不存在： {path}");
            }
        }

        /// <summary>
        /// 删除目录（如果存在）
        /// </summary>
        public void DeleteDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Log.logger.Info($"删除目录： {path}");
                Directory.Delete(path, true);
            }
            else
            {
                Log.logger.Info($"目录不存在： {path}");
            }
        }

        /// <summary>
        /// 获取URL文本内容
        /// </summary>
        public async System.Threading.Tasks.Task<string> GetURLText(
            string url,
            bool reportError = true,
            int maxRetries = 3,
            int delayMs = 300,
            bool parseErrorJson = false)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                Log.logger.Error("URL不能为空");
                return string.Empty;
            }

            Log.logger.Info($"获取 {url} 文本内容。");

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    if (attempt > 1)
                    {
                        Log.logger.Info($"第 {attempt} 次尝试获取 {url}");
                    }

                    HttpResponseMessage response = await _httpClient.GetAsync(url);

                    // 检查是否为403状态码且需要解析错误JSON
                    if (response.StatusCode == HttpStatusCode.Forbidden && parseErrorJson)
                    {
                        string errorContent = await response.Content.ReadAsStringAsync();
                        HandleErrorJson(errorContent, url);
                        return string.Empty;
                    }

                    response.EnsureSuccessStatusCode();

                    string result = await response.Content.ReadAsStringAsync();

                    if (attempt > 1)
                    {
                        Log.logger.Info($"第 {attempt} 次尝试成功获取内容");
                    }

                    return result;
                }
                catch (MirrorChyanException)
                {
                    // MirrorChyan异常直接向上传播，让调用者能够识别并处理
                    throw;
                }
                catch (HttpRequestException ex) when (ex.Data.Contains("StatusCode") &&
                                                       (HttpStatusCode)ex.Data["StatusCode"] == HttpStatusCode.Forbidden &&
                                                       parseErrorJson)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    bool isLastAttempt = attempt == maxRetries;

                    if (isLastAttempt)
                    {
                        if (reportError)
                        {
                            Log.logger.Error($"获取网址文本内容失败，已重试 {maxRetries} 次。", ex);
                        }
                        else
                        {
                            Log.logger.Error($"获取网址文本内容失败，已重试 {maxRetries} 次。", ex);
                        }
                    }
                    else
                    {
                        Log.logger.Warn($"第 {attempt} 次获取失败，{delayMs}ms 后重试");
                        await System.Threading.Tasks.Task.Delay(delayMs);
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 处理错误JSON的辅助方法
        /// </summary>
        private void HandleErrorJson(string jsonContent, string url)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(jsonContent) ||
                    (!jsonContent.TrimStart().StartsWith("{") && !jsonContent.TrimStart().StartsWith("[")))
                {
                    Log.logger.Warn($"403响应内容不是有效的JSON格式: {url}");
                    return;
                }

                JObject jsonObject = JObject.Parse(jsonContent);
                JToken codeToken = jsonObject["code"];

                if (codeToken != null)
                {
                    int errorCode = codeToken.Value<int>();
                    Log.logger.Error($"MirrorChyan API返回了错误码: {errorCode}");
                    throw new MirrorChyanException(errorCode);
                }
                else
                {
                    Log.logger.Warn($"403响应的JSON中未找到code字段: {url}");
                }
            }
            catch (JsonReaderException ex)
            {
                Log.logger.Warn($"解析403响应JSON失败: {url}, Error: {ex.Message}");
            }
        }

        /// <summary>
        /// 打开URL（使用系统默认浏览器）
        /// </summary>
        public void OpenUrl(string url)
        {
            Log.logger.Info($"打开了网址：{url}");
            ProcessStartInfo psi = new(url)
            {
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        /// <summary>
        /// 检查字体文件是否有效
        /// </summary>
        public bool IsValidFontFile(string filePath)
        {
            if (filePath == "输入字体路径")
            {
                return false;
            }

            // 检查文件扩展名
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            if (extension != ".ttf" && extension != ".otf" && extension != ".ttc")
            {
                return false;
            }

            // 检查文件是否存在
            if (!File.Exists(filePath))
            {
                return false;
            }

            // 尝试加载字体来验证
            try
            {
                using System.Drawing.Text.PrivateFontCollection fontCollection = new();
                fontCollection.AddFontFile(filePath);
                return fontCollection.Families.Length > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取字体家族名称
        /// </summary>
        public string GetFontFamilyName(string filePath)
        {
            try
            {
                using System.Drawing.Text.PrivateFontCollection fontCollection = new();
                fontCollection.AddFontFile(filePath);
                if (fontCollection.Families.Length > 0)
                {
                    return fontCollection.Families[0].Name;
                }
            }
            catch { }

            return Path.GetFileNameWithoutExtension(filePath);
        }
    }
}
