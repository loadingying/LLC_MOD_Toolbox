using LLC_MOD_Toolbox;
using LLC_MOD_Toolbox.Interfaces;
using LLC_MOD_Toolbox.Models;
using Newtonsoft.Json.Linq;
using System.IO;

namespace LLC_MOD_Toolbox.Services
{
    /// <summary>
    /// 模组安装服务实现
    /// 处理模组安装、字体安装、版本检查等核心逻辑
    /// </summary>
    public class ModInstallService : IModInstallService
    {
        private readonly IFileUtilityService _fileUtilityService;
        private readonly OfficialApiClient _officialApiClient;

        /// <summary>
        /// 初始化ModInstallService
        /// </summary>
        public ModInstallService(IFileUtilityService fileUtilityService, OfficialApiClient officialApiClient)
        {
            _fileUtilityService = fileUtilityService ?? throw new ArgumentNullException(nameof(fileUtilityService));
            _officialApiClient = officialApiClient ?? throw new ArgumentNullException(nameof(officialApiClient));
        }

        /// <summary>
        /// 检查是否需要安装字体
        /// </summary>
        public bool NeedsFontInstallation(string limbusCompanyDir)
        {
            string fontDir = Path.Combine(limbusCompanyDir, "LimbusCompany_Data", "Lang", "LLC_zh-CN", "Font", "Context");
            string fontChinese = Path.Combine(fontDir, "ChineseFont.ttf");
            string fontBackup = Path.Combine(limbusCompanyDir, "LimbusCompany_Data", "Lang", "LLC_zh-CN", "BackupFont", "ChineseFont.ttf.bak");

            return !File.Exists(fontChinese) && !File.Exists(fontBackup);
        }

        /// <summary>
        /// 检查是否需要安装模组
        /// </summary>
        public bool NeedsModInstallation(string limbusCompanyDir, int currentVersion, int latestVersion)
        {
            string versionJsonPath = Path.Combine(limbusCompanyDir, "LimbusCompany_Data", "Lang", "LLC_zh-CN", "Info", "version.json");

            // 模组不存在
            if (!File.Exists(versionJsonPath))
            {
                return true;
            }

            // 当前版本为0（未安装）
            if (currentVersion == 0)
            {
                return true;
            }

            // 当前版本小于最新版本
            return currentVersion < latestVersion;
        }

        /// <summary>
        /// 获取当前安装的模组版本
        /// </summary>
        public async System.Threading.Tasks.Task<int> GetCurrentModVersionAsync(string limbusCompanyDir)
        {
            string versionJsonPath = Path.Combine(limbusCompanyDir, "LimbusCompany_Data", "Lang", "LLC_zh-CN", "Info", "version.json");

            if (!File.Exists(versionJsonPath))
            {
                return 0; // 未安装
            }

            try
            {
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
        /// 安装字体
        /// </summary>
        public async System.Threading.Tasks.Task InstallFontAsync(
            string limbusCompanyDir,
            bool useMirrorChyan,
            string mirrorChyanToken,
            bool useGithub,
            string? useEndPoint = null,
            string? defaultEndPoint = null,
            System.Action<float>? onProgress = null)
        {
            Log.logger.Info("正在安装字体文件。");

            string fontDir = Path.Combine(limbusCompanyDir, "LimbusCompany_Data", "Lang", "LLC_zh-CN", "Font", "Context");
            Directory.CreateDirectory(fontDir);

            string fontZIPFile = Path.Combine(limbusCompanyDir, "LLCCN-Font.7z");

            if (useMirrorChyan)
            {
                await InstallFontFromMirrorChyanAsync(limbusCompanyDir, mirrorChyanToken, fontZIPFile, onProgress);
            }
            else
            {
                await InstallFontFromDefaultAsync(limbusCompanyDir, useGithub, useEndPoint ?? "", defaultEndPoint ?? "", fontZIPFile, onProgress);
            }
        }

        /// <summary>
        /// 从MirrorChyan安装字体
        /// </summary>
        private async System.Threading.Tasks.Task InstallFontFromMirrorChyanAsync(
            string limbusCompanyDir,
            string mirrorChyanToken,
            string fontZIPFile,
            System.Action<float>? onProgress)
        {
            var (url, sha256) = await GetFontInfoFromMirrorChyanAsync(mirrorChyanToken);

            if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(sha256))
            {
                throw new Exception("获取字体信息失败");
            }

            await _fileUtilityService.DownloadFileAsync(url, fontZIPFile);

            if (!VerifyFileHash(fontZIPFile, sha256))
            {
                _fileUtilityService.DeleteFile(fontZIPFile);
                throw new Exception("字体哈希校验失败");
            }

            ExtractAndInstallMod(fontZIPFile, limbusCompanyDir);
        }

        /// <summary>
        /// 从默认源安装字体
        /// </summary>
        private async System.Threading.Tasks.Task InstallFontFromDefaultAsync(
            string limbusCompanyDir,
            bool useGithub,
            string useEndPoint,
            string defaultEndPoint,
            string fontZIPFile,
            System.Action<float>? onProgress)
        {
            if (useGithub)
            {
                await _fileUtilityService.DownloadFileAsync(
                    "https://raw.githubusercontent.com/LocalizeLimbusCompany/LocalizeLimbusCompany/refs/heads/main/Fonts/LLCCN-Font.7z",
                    fontZIPFile);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(useEndPoint) && string.IsNullOrWhiteSpace(defaultEndPoint))
                {
                    throw new Exception("下载节点为空，无法下载字体。");
                }

                await _fileUtilityService.DownloadFileAutoAsync("LLCCN-Font.7z", fontZIPFile, useEndPoint, defaultEndPoint);
            }

            ExtractAndInstallMod(fontZIPFile, limbusCompanyDir);
        }

        /// <summary>
        /// 安装模组
        /// </summary>
        public async System.Threading.Tasks.Task<bool> InstallModAsync(
            string limbusCompanyDir,
            bool useMirrorChyan,
            string mirrorChyanToken,
            bool useGithub,
            string? useAPIEndPoint = null,
            string? useEndPoint = null,
            string? defaultEndPoint = null,
            System.Action<float>? onProgress = null)
        {
            Log.logger.Info("开始安装模组。");

            string langDir = Path.Combine(limbusCompanyDir, "LimbusCompany_Data/Lang/LLC_zh-CN");
            string versionJsonPath = Path.Combine(langDir, "Info", "version.json");
            string limbusLocalizeZipPath = Path.Combine(limbusCompanyDir, "LimbusLocalize.7z");

            int latestVersion = -1;
            int currentVersion = await GetCurrentModVersionAsync(limbusCompanyDir);

            if (useMirrorChyan)
            {
                var (version, url, sha256) = await GetModInfoFromMirrorChyanAsync(mirrorChyanToken);
                latestVersion = version;

                if (latestVersion == -100)
                {
                    return false;
                }

                if (currentVersion >= latestVersion)
                {
                    Log.logger.Info("模组无需更新。");
                    return true;
                }

                await _fileUtilityService.DownloadFileAsync(url, limbusLocalizeZipPath);

                if (!VerifyFileHash(limbusLocalizeZipPath, sha256))
                {
                    _fileUtilityService.DeleteFile(limbusLocalizeZipPath);
                    throw new Exception("校验Hash失败");
                }

                ExtractAndInstallMod(limbusLocalizeZipPath, limbusCompanyDir);
                return true;
            }
            else
            {
                // 从默认源安装的逻辑
                return await InstallModFromDefaultAsync(limbusCompanyDir, useGithub, useAPIEndPoint ?? "", useEndPoint, defaultEndPoint, limbusLocalizeZipPath, onProgress);
            }
        }

        /// <summary>
        /// 从默认源安装模组
        /// </summary>
        private async System.Threading.Tasks.Task<bool> InstallModFromDefaultAsync(
            string limbusCompanyDir,
            bool useGithub,
            string useAPIEndPoint,
            string? useEndPoint,
            string? defaultEndPoint,
            string limbusLocalizeZipPath,
            System.Action<float>? onProgress)
        {
            try
            {
                var apiEndpoint = GetApiEndpointOrDefault(useAPIEndPoint);

                int latestVersion = await _officialApiClient.GetLatestVersionAsync(apiEndpoint, useGithub);
                if (latestVersion <= 0 || latestVersion == -100)
                {
                    Log.logger.Error("获取最新版本失败，无法安装模组。");
                    return false;
                }

                var hashObj = await _officialApiClient.GetHashAsync(apiEndpoint);
                if (hashObj == null)
                {
                    Log.logger.Error("获取Hash信息失败，无法安装模组。");
                    return false;
                }

                // 使用正确的字段名：main_hash（API不返回download_link，需要用节点URL构建下载链接）
                string sha256 = hashObj["main_hash"]?.Value<string>() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(sha256))
                {
                    Log.logger.Error("Hash为空，无法安装模组。");
                    return false;
                }
                
                // 使用节点URL下载文件（与旧代码一致）
                string fileName = $"LimbusLocalize_{latestVersion}.7z";
                Log.logger.Info($"下载模组文件：{fileName}，端点：{useEndPoint ?? defaultEndPoint}");
                onProgress?.Invoke(0.1f);
                await _fileUtilityService.DownloadFileAutoAsync(fileName, limbusLocalizeZipPath, useEndPoint, defaultEndPoint);
                onProgress?.Invoke(0.7f);

                if (!VerifyFileHash(limbusLocalizeZipPath, sha256))
                {
                    _fileUtilityService.DeleteFile(limbusLocalizeZipPath);
                    throw new Exception("校验Hash失败");
                }

                // 在后台线程中执行解压操作，避免阻塞UI
                await System.Threading.Tasks.Task.Run(() =>
                {
                    ExtractAndInstallMod(limbusLocalizeZipPath, limbusCompanyDir);
                });
                onProgress?.Invoke(1f);
                return true;
            }
            catch (Exception ex)
            {
                Log.logger.Error("从默认源安装模组失败。", ex);
                return false;
            }
        }

        /// <summary>
        /// 获取字体信息（从MirrorChyan）
        /// </summary>
        public async System.Threading.Tasks.Task<(string url, string sha256)> GetFontInfoFromMirrorChyanAsync(string mirrorChyanToken)
        {
            try
            {
                Log.logger.Info("获取字体MirrorChyan链接。");
                string url = $"https://mirrorchyan.com/api/resources/LLCCN-Font/latest?user_agent=LLC_MOD_Toolbox&current_version=&cdk={mirrorChyanToken}";
                string raw = await _fileUtilityService.GetURLText(url, parseErrorJson: true);

                if (string.IsNullOrEmpty(raw))
                {
                    return (string.Empty, string.Empty);
                }

                var json = ParseMirrorChyanJson(raw);
                string downloadUrl = json["data"]?["url"]?.Value<string>() ?? string.Empty;
                string sha256 = json["data"]?["sha256"]?.Value<string>() ?? string.Empty;

                return (downloadUrl, sha256);
            }
            catch (MirrorChyanException)
            {
                // MirrorChyan异常直接向上传播，让UI层显示用户友好的错误消息
                throw;
            }
            catch (Exception ex)
            {
                Log.logger.Error("获取字体MirrorChyan链接失败。", ex);
                return (string.Empty, string.Empty);
            }
        }

        /// <summary>
        /// 获取模组信息（从MirrorChyan）
        /// </summary>
        public async System.Threading.Tasks.Task<(int version, string url, string sha256)> GetModInfoFromMirrorChyanAsync(string mirrorChyanToken)
        {
            try
            {
                Log.logger.Info("获取模组标签。");
                string url = $"https://mirrorchyan.com/api/resources/LLC/latest?user_agent=LLC_MOD_Toolbox&current_version=&cdk={mirrorChyanToken}";
                string raw = await _fileUtilityService.GetURLText(url, parseErrorJson: true);

                var json = ParseMirrorChyanJson(raw);
                string version = json["data"]?["version_name"]?.Value<string>() ?? "0";
                int parseVersion = int.Parse(version);
                string downloadUrl = json["data"]?["url"]?.Value<string>() ?? string.Empty;
                string sha256 = json["data"]?["sha256"]?.Value<string>() ?? string.Empty;

                return (parseVersion, downloadUrl, sha256);
            }
            catch (MirrorChyanException)
            {
                // MirrorChyan异常直接向上传播，让UI层显示用户友好的错误消息
                throw;
            }
            catch (Exception ex)
            {
                Log.logger.Error("获取模组信息失败。", ex);
                return (-100, string.Empty, string.Empty);
            }
        }

        /// <summary>
        /// 解析MirrorChyan返回的Json
        /// </summary>
        private JObject ParseMirrorChyanJson(string json)
        {
            JObject parsed = JObject.Parse(json);
            int code = parsed["code"]?.Value<int>() ?? -1;
            if (code != 0)
            {
                throw new MirrorChyanException(code);
            }
            return parsed;
        }

        /// <summary>
        /// 验证文件哈希
        /// </summary>
        public bool VerifyFileHash(string filePath, string expectedHash)
        {
            string actualHash = _fileUtilityService.CalculateSHA256(filePath);
            return actualHash == expectedHash;
        }

        /// <summary>
        /// 解压并安装模组文件
        /// </summary>
        public void ExtractAndInstallMod(string archivePath, string targetDirectory, bool deleteAfterExtract = true)
        {
            Log.logger.Info($"解压 {archivePath} 中。");
            _fileUtilityService.Unarchive(archivePath, targetDirectory);

            if (deleteAfterExtract)
            {
                Log.logger.Info($"删除 {archivePath} 。");
                _fileUtilityService.DeleteFile(archivePath);
            }
        }

        private static string GetApiEndpointOrDefault(string apiEndpoint)
        {
            return string.IsNullOrWhiteSpace(apiEndpoint)
                ? "https://api.zeroasso.top/{0}"
                : apiEndpoint;
        }
    }
}
