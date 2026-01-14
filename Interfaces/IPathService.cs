namespace LLC_MOD_Toolbox.Interfaces
{
    /// <summary>
    /// 路径服务接口
    /// 提供游戏路径查找和验证功能
    /// </summary>
    public interface IPathService
    {
        /// <summary>
        /// 获取或设置Limbus Company游戏安装目录
        /// </summary>
        string? GameDirectory { get; set; }

        /// <summary>
        /// 获取游戏可执行文件完整路径
        /// </summary>
        string? GameExecutablePath { get; }

        /// <summary>
        /// 获取游戏数据目录
        /// </summary>
        string? GameDataDirectory { get; }

        /// <summary>
        /// 获取语言包目录
        /// </summary>
        string? LangDirectory { get; }

        /// <summary>
        /// 获取中文语言包目录
        /// </summary>
        string? ChineseLangDirectory { get; }

        /// <summary>
        /// 自动查找游戏安装路径
        /// </summary>
        /// <param name="appId">Steam应用ID</param>
        /// <param name="executableName">可执行文件名</param>
        /// <returns>游戏安装目录</returns>
        string FindGamePath(string? appId = null, string? executableName = null);

        /// <summary>
        /// 验证游戏路径是否有效
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns>如果路径有效返回true，否则返回false</returns>
        bool ValidateGamePath(string? path);

        /// <summary>
        /// 检查游戏是否正在运行
        /// </summary>
        /// <returns>如果游戏正在运行返回true，否则返回false</returns>
        bool IsGameRunning();
    }
}
