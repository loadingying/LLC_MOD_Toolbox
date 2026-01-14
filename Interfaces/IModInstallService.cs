namespace LLC_MOD_Toolbox.Interfaces
{
    /// <summary>
    /// 模组安装服务接口
    /// 处理模组安装、字体安装、版本检查等核心逻辑
    /// </summary>
    public interface IModInstallService
    {
        /// <summary>
        /// 安装字体
        /// </summary>
        /// <param name="limbusCompanyDir">游戏目录</param>
        /// <param name="useMirrorChyan">是否使用MirrorChyan</param>
        /// <param name="mirrorChyanToken">MirrorChyan Token</param>
        /// <param name="useGithub">是否使用Github</param>
        /// <param name="useEndPoint">使用的端点</param>
        /// <param name="defaultEndPoint">默认端点</param>
        /// <param name="onProgress">进度回调</param>
        System.Threading.Tasks.Task InstallFontAsync(
            string limbusCompanyDir,
            bool useMirrorChyan,
            string mirrorChyanToken,
            bool useGithub,
            string? useEndPoint = null,
            string? defaultEndPoint = null,
            System.Action<float>? onProgress = null);

        /// <summary>
        /// 安装模组
        /// </summary>
        /// <param name="limbusCompanyDir">游戏目录</param>
        /// <param name="useMirrorChyan">是否使用MirrorChyan</param>
        /// <param name="mirrorChyanToken">MirrorChyan Token</param>
        /// <param name="useGithub">是否使用Github</param>
        /// <param name="useAPIEndPoint">API端点</param>
        /// <param name="useEndPoint">下载节点端点</param>
        /// <param name="defaultEndPoint">默认下载节点端点</param>
        /// <param name="onProgress">进度回调</param>
        System.Threading.Tasks.Task<bool> InstallModAsync(
            string limbusCompanyDir,
            bool useMirrorChyan,
            string mirrorChyanToken,
            bool useGithub,
            string? useAPIEndPoint = null,
            string? useEndPoint = null,
            string? defaultEndPoint = null,
            System.Action<float>? onProgress = null);

        /// <summary>
        /// 检查是否需要安装字体
        /// </summary>
        /// <param name="limbusCompanyDir">游戏目录</param>
        /// <returns>是否需要安装</returns>
        bool NeedsFontInstallation(string limbusCompanyDir);

        /// <summary>
        /// 检查是否需要安装模组
        /// </summary>
        /// <param name="limbusCompanyDir">游戏目录</param>
        /// <param name="currentVersion">当前版本</param>
        /// <param name="latestVersion">最新版本</param>
        /// <returns>是否需要安装</returns>
        bool NeedsModInstallation(string limbusCompanyDir, int currentVersion, int latestVersion);

        /// <summary>
        /// 获取当前安装的模组版本
        /// </summary>
        /// <param name="limbusCompanyDir">游戏目录</param>
        /// <returns>当前版本号</returns>
        System.Threading.Tasks.Task<int> GetCurrentModVersionAsync(string limbusCompanyDir);

        /// <summary>
        /// 获取字体信息（从MirrorChyan）
        /// </summary>
        /// <param name="mirrorChyanToken">Token</param>
        /// <returns>(下载URL, SHA256)</returns>
        System.Threading.Tasks.Task<(string url, string sha256)> GetFontInfoFromMirrorChyanAsync(string mirrorChyanToken);

        /// <summary>
        /// 获取模组信息（从MirrorChyan）
        /// </summary>
        /// <param name="mirrorChyanToken">Token</param>
        /// <returns>(版本号, 下载URL, SHA256)</returns>
        System.Threading.Tasks.Task<(int version, string url, string sha256)> GetModInfoFromMirrorChyanAsync(string mirrorChyanToken);

        /// <summary>
        /// 验证文件哈希
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="expectedHash">期望的哈希值</param>
        /// <returns>是否匹配</returns>
        bool VerifyFileHash(string filePath, string expectedHash);

        /// <summary>
        /// 解压并安装模组文件
        /// </summary>
        /// <param name="archivePath">压缩文件路径</param>
        /// <param name="targetDirectory">目标目录</param>
        /// <param name="deleteAfterExtract">解压后是否删除压缩文件</param>
        void ExtractAndInstallMod(string archivePath, string targetDirectory, bool deleteAfterExtract = true);
    }
}
