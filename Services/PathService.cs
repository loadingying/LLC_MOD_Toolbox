using LLC_MOD_Toolbox.Interfaces;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace LLC_MOD_Toolbox.Services
{
    /// <summary>
    /// 路径服务实现
    /// 提供游戏路径查找、验证和管理功能
    /// </summary>
    public class PathService : IPathService
    {
        private string? _gameDirectory;
        private const string GameExecutableName = "LimbusCompany.exe";
        private const string GameDataFolderName = "LimbusCompany_Data";
        private const string LangFolderName = "Lang";
        private const string ChineseLangCode = "LLC_zh-CN";

        /// <summary>
        /// 获取或设置Limbus Company游戏安装目录
        /// </summary>
        public string? GameDirectory
        {
            get => _gameDirectory;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    _gameDirectory = null;
                }
                else if (ValidateGamePath(value))
                {
                    _gameDirectory = value;
                }
                else
                {
                    throw new InvalidOperationException($"无效的游戏路径: {value}");
                }
            }
        }

        /// <summary>
        /// 获取游戏可执行文件完整路径
        /// </summary>
        public string? GameExecutablePath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_gameDirectory))
                    return null;

                return Path.Combine(_gameDirectory, GameExecutableName);
            }
        }

        /// <summary>
        /// 获取游戏数据目录
        /// </summary>
        public string? GameDataDirectory
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_gameDirectory))
                    return null;

                return Path.Combine(_gameDirectory, GameDataFolderName);
            }
        }

        /// <summary>
        /// 获取语言包目录
        /// </summary>
        public string? LangDirectory
        {
            get
            {
                var dataDir = GameDataDirectory;
                if (string.IsNullOrWhiteSpace(dataDir))
                    return null;

                return Path.Combine(dataDir, LangFolderName);
            }
        }

        /// <summary>
        /// 获取中文语言包目录
        /// </summary>
        public string? ChineseLangDirectory
        {
            get
            {
                var langDir = LangDirectory;
                if (string.IsNullOrWhiteSpace(langDir))
                    return null;

                return Path.Combine(langDir, ChineseLangCode);
            }
        }

        /// <summary>
        /// 自动查找游戏安装路径
        /// </summary>
        public string FindGamePath(string? appId = null, string? executableName = null)
        {
            appId ??= "1973530";
            executableName ??= GameExecutableName;

            try
            {
                var path = SteamLocator.FindLimbusCompanyPath(appId, executableName);
                if (ValidateGamePath(path))
                {
                    _gameDirectory = path;
                    return path;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("无法自动查找游戏路径", ex);
            }

            throw new DirectoryNotFoundException("未找到Limbus Company安装目录");
        }

        /// <summary>
        /// 验证游戏路径是否有效
        /// </summary>
        public bool ValidateGamePath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            if (!Directory.Exists(path))
                return false;

            var exePath = Path.Combine(path, GameExecutableName);
            if (!File.Exists(exePath))
                return false;

            var dataDir = Path.Combine(path, GameDataFolderName);
            if (!Directory.Exists(dataDir))
                return false;

            return true;
        }

        /// <summary>
        /// 检查游戏是否正在运行
        /// </summary>
        public bool IsGameRunning()
        {
            try
            {
                var processes = Process.GetProcessesByName("LimbusCompany");
                return processes.Length > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
