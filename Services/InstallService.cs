using LLC_MOD_Toolbox.Interfaces;
using LLC_MOD_Toolbox.Models;
using Newtonsoft.Json.Linq;
using SevenZip;
using System.Diagnostics;
using System.IO;

namespace LLC_MOD_Toolbox.Services
{
    /// <summary>
    /// 安装服务实现
    /// 负责模组的安装、卸载和版本管理
    /// </summary>
    public class InstallService : IInstallService
    {
        private readonly IDownloadService _downloadService;
        private readonly IPathService _pathService;
        private readonly IConfigService _configService;
        private readonly IDialogService _dialogService;
        private readonly MirrorChyanApiClient _mirrorChyanClient;
        private readonly OfficialApiClient _officialApiClient;

        // 安装阶段常量
        private const int FONT_INSTALL_PHASE = 1;
        private const int MOD_INSTALL_PHASE = 2;

        /// <summary>
        /// 初始化安装服务
        /// </summary>
        public InstallService(
            IDownloadService downloadService,
            IPathService pathService,
            IConfigService configService,
            IDialogService dialogService)
        {
            _downloadService = downloadService ?? throw new ArgumentNullException(nameof(downloadService));
            _pathService = pathService ?? throw new ArgumentNullException(nameof(pathService));
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            _mirrorChyanClient = new MirrorChyanApiClient();
            _officialApiClient = new OfficialApiClient();
        }

        #region IInstallService 实现

        /// <summary>
        /// 异步安装模组
        /// </summary>
        public async System.Threading.Tasks.Task<InstallResult> InstallAsync(
            System.IProgress<float>? progress = null,
            System.Threading.CancellationToken cancellationToken = default)
        {
            try
            {
                // 1. 验证环境
                progress?.Report(5);
                var validation = await ValidateEnvironmentAsync();
                if (!validation.IsValid)
                {
                    var errorMsg = string.Join("\n", validation.Errors);
                    return new InstallResult
                    {
                        Success = false,
                        ErrorMessage = $"环境验证失败：\n{errorMsg}"
                    };
                }

                // 2. 检查游戏是否运行
                progress?.Report(10);
                if (_pathService.IsGameRunning())
                {
                    var continueInstall = _dialogService.ShowConfirm(
                        "检测到游戏正在运行，建议关闭游戏后继续安装。\n是否继续？",
                        "警告");

                    if (!continueInstall)
                    {
                        return new InstallResult
                        {
                            Success = false,
                            ErrorMessage = "用户取消安装"
                        };
                    }
                }

                // 3. 检查是否有新版本
                progress?.Report(15);
                var currentVersion = await GetCurrentVersionAsync();
                var latestVersion = await GetLatestVersionAsync();

                if (currentVersion != null && latestVersion != null &&
                    currentVersion.Version >= latestVersion.Version)
                {
                    return new InstallResult
                    {
                        Success = true,
                        IsLatestVersion = true,
                        InstalledVersion = currentVersion.Version
                    };
                }

                // 4. 安装字体（如果需要）
                progress?.Report(20);
                var fontResult = await InstallFontInternalAsync(cancellationToken);
                if (!fontResult.Success)
                {
                    return new InstallResult
                    {
                        Success = false,
                        ErrorMessage = $"字体安装失败：{fontResult.ErrorMessage}"
                    };
                }

                // 5. 安装模组
                progress?.Report(fontResult.Progress + 30);
                var modResult = await InstallModInternalAsync(latestVersion, progress, cancellationToken);
                if (!modResult.Success)
                {
                    return new InstallResult
                    {
                        Success = false,
                        ErrorMessage = $"模组安装失败：{modResult.ErrorMessage}"
                    };
                }

                // 6. 配置游戏语言
                progress?.Report(95);
                ConfigureGameLanguage();

                progress?.Report(100);

                return new InstallResult
                {
                    Success = true,
                    IsLatestVersion = false,
                    InstalledVersion = latestVersion?.Version ?? 0
                };
            }
            catch (Exception ex)
            {
                return new InstallResult
                {
                    Success = false,
                    ErrorMessage = $"安装失败：{ex.Message}"
                };
            }
        }

        /// <summary>
        /// 异步卸载模组
        /// </summary>
        public async System.Threading.Tasks.Task UninstallAsync(System.Threading.CancellationToken cancellationToken = default)
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    // 删除语言包
                    var langDir = _pathService.ChineseLangDirectory;
                    if (!string.IsNullOrWhiteSpace(langDir) && System.IO.Directory.Exists(langDir))
                    {
                        System.IO.Directory.Delete(langDir, true);
                    }

                    // 重置游戏语言配置
                    ConfigureGameLanguage("");

                    // 删除BepInEx相关文件
                    DeleteBepInEx();

                    // 删除MelonLoader相关文件
                    DeleteMelonLoader();

                    // 注意：不在服务层弹出对话框，由ViewModel层统一处理用户交互
                }
                catch (Exception ex)
                {
                    _dialogService.ShowError($"卸载失败：{ex.Message}");
                }
            }, cancellationToken);
        }

        /// <summary>
        /// 检查是否有可用更新
        /// </summary>
        public async System.Threading.Tasks.Task<bool> CheckUpdateAsync()
        {
            try
            {
                var current = await GetCurrentVersionAsync();
                var latest = await GetLatestVersionAsync();

                if (current == null || latest == null)
                    return false;

                return latest.Version > current.Version;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取当前已安装的模组版本
        /// </summary>
        public async System.Threading.Tasks.Task<ModVersion?> GetCurrentVersionAsync()
        {
            return await System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    var langDir = _pathService.ChineseLangDirectory;
                    if (string.IsNullOrWhiteSpace(langDir))
                        return null;

                    var versionFile = System.IO.Path.Combine(langDir, "Info", "version.json");
                    if (!System.IO.File.Exists(versionFile))
                        return null;

                    var json = System.IO.File.ReadAllText(versionFile);
                    var versionObj = JObject.Parse(json);
                    var version = versionObj["version"]?.Value<int>();

                    if (version == null)
                        return null;

                    return new ModVersion
                    {
                        Version = version.Value,
                        VersionString = version.ToString()
                    };
                }
                catch
                {
                    return null;
                }
            });
        }

        /// <summary>
        /// 获取最新的模组版本
        /// </summary>
        public async System.Threading.Tasks.Task<ModVersion?> GetLatestVersionAsync()
        {
            try
            {
                var useMirrorChyan = _configService.AppSettings.mirrorChyan.enable;

                if (useMirrorChyan)
                {
                    return await GetLatestVersionFromMirrorChyanAsync();
                }
                else
                {
                    return await GetLatestVersionFromOfficialAsync();
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 验证游戏环境
        /// </summary>
        public async System.Threading.Tasks.Task<ValidationResult> ValidateEnvironmentAsync()
        {
            return await System.Threading.Tasks.Task.Run(() =>
            {
                var result = new ValidationResult();

                // 检查游戏路径
                if (string.IsNullOrWhiteSpace(_pathService.GameDirectory))
                {
                    result.Errors.Add("未设置游戏路径");
                }
                else if (!_pathService.ValidateGamePath(_pathService.GameDirectory))
                {
                    result.Errors.Add("游戏路径无效");
                }

                // 检查是否安装了旧框架
                var gameDir = _pathService.GameDirectory;
                if (!string.IsNullOrWhiteSpace(gameDir))
                {
                    var melonLoaderFile = System.IO.Path.Combine(gameDir, "version.dll");
                    if (System.IO.File.Exists(melonLoaderFile))
                    {
                        result.Warnings.Add("检测到MelonLoader框架（已过时），建议卸载");
                    }

                    var bepinExFile = System.IO.Path.Combine(gameDir, "winhttp.dll");
                    if (System.IO.File.Exists(bepinExFile))
                    {
                        result.Warnings.Add("检测到BepInEx框架（旧版本），建议卸载");
                    }
                }

                result.IsValid = result.Errors.Count == 0;
                return result;
            });
        }

        #endregion

        #region 私有方法 - 字体安装

        /// <summary>
        /// 安装字体（内部方法）
        /// </summary>
        private async System.Threading.Tasks.Task<(bool Success, string? ErrorMessage, float Progress)> InstallFontInternalAsync(
            System.Threading.CancellationToken cancellationToken)
        {
            try
            {
                var gameDir = _pathService.GameDirectory;
                if (string.IsNullOrWhiteSpace(gameDir))
                {
                    return (false, "游戏路径未设置", 0);
                }

                var fontDir = System.IO.Path.Combine(gameDir, "LimbusCompany_Data", "Lang", "LLC_zh-CN", "Font", "Context");
                System.IO.Directory.CreateDirectory(fontDir);

                var fontChinese = System.IO.Path.Combine(fontDir, "ChineseFont.ttf");
                var fontBackup = System.IO.Path.Combine(gameDir, "LimbusCompany_Data", "Lang", "LLC_zh-CN", "BackupFont", "ChineseFont.ttf.bak");

                // 检查是否已安装
                if (System.IO.File.Exists(fontChinese) || System.IO.File.Exists(fontBackup))
                {
                    return (true, null, 20);
                }

                var useMirrorChyan = _configService.AppSettings.mirrorChyan.enable;
                string url;
                string sha256;

                if (useMirrorChyan)
                {
                    // 使用Mirror酱
                    var token = GetMirrorChyanToken();
                    if (string.IsNullOrWhiteSpace(token))
                    {
                        return (false, "Mirror酱 CDK未设置", 0);
                    }

                    (url, sha256) = await _mirrorChyanClient.GetResourceInfoAsync("LLCCN-Font", token, cancellationToken);
                }
                else
                {
                    // 使用官方源
                    var hashObj = await GetHashObjectAsync();
                    if (hashObj == null)
                    {
                        return (false, "无法获取Hash信息", 0);
                    }

                    sha256 = hashObj["font_hash"]?.Value<string>() ?? string.Empty;
                    url = await GetFontDownloadUrlAsync(cancellationToken);
                }

                if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(sha256))
                {
                    return (false, "无法获取字体下载信息", 0);
                }

                // 下载字体包
                var fontZIPFile = System.IO.Path.Combine(gameDir, "LLCCN-Font.7z");
                await _downloadService.DownloadFileAsync(url, fontZIPFile, null, cancellationToken);

                // 校验Hash
                var actualHash = _downloadService.CalculateSHA256(fontZIPFile);
                if (actualHash != sha256)
                {
                    System.IO.File.Delete(fontZIPFile);
                    return (false, "字体包校验失败，请更换节点或稍后重试", 0);
                }

                // 解压字体包
                Unarchive(fontZIPFile, gameDir);
                System.IO.File.Delete(fontZIPFile);

                return (true, null, 20);
            }
            catch (MirrorChyanException ex)
            {
                return (false, $"Mirror酱错误：{ex.Message}", 0);
            }
            catch (Exception ex)
            {
                return (false, $"字体安装失败：{ex.Message}", 0);
            }
        }

        /// <summary>
        /// 获取字体下载URL
        /// </summary>
        private System.Threading.Tasks.Task<string> GetFontDownloadUrlAsync(
            System.Threading.CancellationToken cancellationToken)
        {
            // 优先从GitHub下载
            return System.Threading.Tasks.Task.FromResult("https://raw.githubusercontent.com/LocalizeLimbusCompany/LocalizeLimbusCompany/refs/heads/main/Fonts/LLCCN-Font.7z");
        }

        #endregion

        #region 私有方法 - 模组安装

        /// <summary>
        /// 安装模组（内部方法）
        /// </summary>
        private async System.Threading.Tasks.Task<(bool Success, string? ErrorMessage)> InstallModInternalAsync(
            ModVersion? version,
            System.IProgress<float>? progress,
            System.Threading.CancellationToken cancellationToken)
        {
            try
            {
                var gameDir = _pathService.GameDirectory;
                if (string.IsNullOrWhiteSpace(gameDir))
                {
                    return (false, "游戏路径未设置");
                }

                var langDir = System.IO.Path.Combine(gameDir, "LimbusCompany_Data", "Lang", "LLC_zh-CN");
                var versionJsonPath = System.IO.Path.Combine(langDir, "Info", "version.json");
                var limbusLocalizeZipPath = System.IO.Path.Combine(gameDir, "LimbusLocalize.7z");

                // 检查当前版本
                int currentVersion = -1;
                if (System.IO.File.Exists(versionJsonPath))
                {
                    var json = System.IO.File.ReadAllText(versionJsonPath);
                    var versionObj = JObject.Parse(json);
                    currentVersion = versionObj["version"]?.Value<int>() ?? -1;
                }

                // 获取最新版本
                int latestVersion = version?.Version ?? -1;

                // 检查是否需要安装
                if (currentVersion >= latestVersion && currentVersion != -1)
                {
                    return (true, null);
                }

                bool needInstall = currentVersion == -1 || currentVersion < latestVersion;

                if (!needInstall)
                {
                    return (true, null);
                }

                // 下载并安装模组
                string url;
                string sha256;

                var useMirrorChyan = _configService.AppSettings.mirrorChyan.enable;

                if (useMirrorChyan)
                {
                    var token = GetMirrorChyanToken();
                    if (string.IsNullOrWhiteSpace(token))
                    {
                        return (false, "Mirror酱 CDK未设置");
                    }

                    var modInfo = await _mirrorChyanClient.GetModInfoAsync(token, cancellationToken);
                    url = modInfo.url;
                    sha256 = modInfo.sha256;
                }
                else
                {
                    var hashObj = await GetHashObjectAsync();
                    if (hashObj == null)
                    {
                        return (false, "无法获取Hash信息");
                    }

                    sha256 = hashObj["mod_hash"]?.Value<string>() ?? string.Empty;
                    url = string.Format(hashObj["download_link"]?.Value<string>() ?? "", latestVersion);
                }

                if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(sha256))
                {
                    return (false, "无法获取模组下载信息");
                }

                // 下载模组包
                progress?.Report(50);
                await _downloadService.DownloadFileAsync(url, limbusLocalizeZipPath, null, cancellationToken);

                // 校验Hash
                var actualHash = _downloadService.CalculateSHA256(limbusLocalizeZipPath);
                if (actualHash != sha256)
                {
                    System.IO.File.Delete(limbusLocalizeZipPath);
                    return (false, "模组包校验失败，请更换节点或稍后重试");
                }

                // 解压模组包
                progress?.Report(80);
                Unarchive(limbusLocalizeZipPath, gameDir);
                System.IO.File.Delete(limbusLocalizeZipPath);

                progress?.Report(90);
                return (true, null);
            }
            catch (MirrorChyanException ex)
            {
                return (false, $"Mirror酱错误：{ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"模组安装失败：{ex.Message}");
            }
        }

        #endregion

        #region 私有方法 - 版本检查

        /// <summary>
        /// 从Mirror酱获取最新版本
        /// </summary>
        private async System.Threading.Tasks.Task<ModVersion?> GetLatestVersionFromMirrorChyanAsync()
        {
            try
            {
                var token = GetMirrorChyanToken();
                if (string.IsNullOrWhiteSpace(token))
                {
                    return null;
                }

                var (version, _, _) = await _mirrorChyanClient.GetModInfoAsync(token);
                if (version <= 0)
                {
                    return null;
                }

                return new ModVersion
                {
                    Version = version,
                    VersionString = version.ToString()
                };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 从官方API获取最新版本
        /// </summary>
        private async System.Threading.Tasks.Task<ModVersion?> GetLatestVersionFromOfficialAsync()
        {
            try
            {
                var node = _configService.AppSettings.nodeSelect.defaultApiNode;
                var apiEndpoint = GetApiEndpoint(node);

                var version = await _officialApiClient.GetLatestVersionAsync(apiEndpoint, useGithub: false);
                if (version <= 0)
                {
                    return null;
                }

                return new ModVersion
                {
                    Version = version,
                    VersionString = version.ToString()
                };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取Hash对象
        /// </summary>
        private async System.Threading.Tasks.Task<JObject?> GetHashObjectAsync()
        {
            try
            {
                var node = _configService.AppSettings.nodeSelect.defaultApiNode;
                var apiEndpoint = GetApiEndpoint(node);

                return await _officialApiClient.GetHashAsync(apiEndpoint);
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region 私有方法 - 辅助功能

        /// <summary>
        /// 获取Mirror酱CDK
        /// </summary>
        private string? GetMirrorChyanToken()
        {
            // 从安全存储中获取
            var token = SecureStringStorage.LoadToken();

            // 如果为空，尝试从配置中获取
            if (string.IsNullOrWhiteSpace(token))
            {
                // TODO: 从配置或其他地方获取
                return string.Empty;
            }

            return token;
        }

        /// <summary>
        /// 获取API端点URL
        /// </summary>
        private string GetApiEndpoint(string nodeName)
        {
            // 从NodeList.json读取
            var nodeListPath = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Config/NodeList.json");

            if (!System.IO.File.Exists(nodeListPath))
            {
                return "https://api.zeroasso.top/{0}";
            }

            try
            {
                var json = System.IO.File.ReadAllText(nodeListPath);
                var root = Newtonsoft.Json.JsonConvert.DeserializeObject<RootModel>(json);

                if (root?.ApiNode != null)
                {
                    var node = root.ApiNode.FirstOrDefault(n => n.Name == nodeName && n.IsDefault);
                    if (node != null)
                    {
                        return node.Endpoint;
                    }
                }

                // 返回默认节点
                return root?.ApiNode.FirstOrDefault()?.Endpoint ?? "https://api.zeroasso.top/{0}";
            }
            catch
            {
                return "https://api.zeroasso.top/{0}";
            }
        }

        /// <summary>
        /// 配置游戏语言
        /// </summary>
        private void ConfigureGameLanguage(string langCode = "LLC_zh-CN")
        {
            try
            {
                var dataDir = _pathService.GameDataDirectory;
                if (string.IsNullOrWhiteSpace(dataDir))
                    return;

                var configFile = System.IO.Path.Combine(dataDir, "Lang", "config.json");
                if (!System.IO.File.Exists(configFile))
                    return;

                var json = System.IO.File.ReadAllText(configFile);
                var configObj = JObject.Parse(json);
                configObj["lang"] = langCode;

                System.IO.File.WriteAllText(configFile, configObj.ToString());
            }
            catch (Exception ex)
            {
                _dialogService.ShowWarning($"配置游戏语言失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 解压7z文件
        /// </summary>
        private void Unarchive(string archivePath, string destinationDir)
        {
            var dllPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources/7z.dll");
            SevenZipBase.SetLibraryPath(dllPath);

            using var extractor = new SevenZipExtractor(archivePath);

            // ExtractArchive会自动提取到archivePath所在的目录
            // 我们需要移动文件到目标目录
            var tempDir = Path.GetDirectoryName(archivePath);
            if (tempDir != null)
            {
                extractor.ExtractArchive(tempDir);

                // 移动提取的文件到目标目录
                if (Directory.Exists(tempDir))
                {
                    // 移动所有文件到目标目录
                    foreach (var file in Directory.GetFiles(tempDir, "*.*", SearchOption.AllDirectories))
                    {
                        var relativePath = Path.GetRelativePath(tempDir, file);
                        var destPath = Path.Combine(destinationDir, relativePath);
                        var destDir = Path.GetDirectoryName(destPath);

                        if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                        {
                            Directory.CreateDirectory(destDir);
                        }

                        if (!File.Exists(destPath))
                        {
                            File.Move(file, destPath);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 删除BepInEx框架
        /// </summary>
        private void DeleteBepInEx()
        {
            var gameDir = _pathService.GameDirectory;
            if (string.IsNullOrWhiteSpace(gameDir))
                return;

            // 删除BepInEx目录和相关文件
            var bepinDir = System.IO.Path.Combine(gameDir, "BepInEx");
            if (System.IO.Directory.Exists(bepinDir))
            {
                System.IO.Directory.Delete(bepinDir, true);
            }

            var filesToDelete = new[]
            {
                "doorstop_config.ini",
                "winhttp.dll",
                "winhttp.dll.disabled"
            };

            foreach (var file in filesToDelete)
            {
                var filePath = System.IO.Path.Combine(gameDir, file);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
        }

        /// <summary>
        /// 删除MelonLoader框架
        /// </summary>
        private void DeleteMelonLoader()
        {
            var gameDir = _pathService.GameDirectory;
            if (string.IsNullOrWhiteSpace(gameDir))
                return;

            // 删除MelonLoader目录和相关文件
            var dirsToDelete = new[]
            {
                "MelonLoader",
                "Mods",
                "Plugins",
                "UserData",
                "UserLibs"
            };

            foreach (var dir in dirsToDelete)
            {
                var dirPath = System.IO.Path.Combine(gameDir, dir);
                if (System.IO.Directory.Exists(dirPath))
                {
                    System.IO.Directory.Delete(dirPath, true);
                }
            }

            var filesToDelete = new[]
            {
                "dobby.dll",
                "version.dll"
            };

            foreach (var file in filesToDelete)
            {
                var filePath = System.IO.Path.Combine(gameDir, file);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
        }

        #endregion
    }
}
