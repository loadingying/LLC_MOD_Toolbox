namespace LLC_MOD_Toolbox.Interfaces
{
    /// <summary>
    /// Loading文本服务接口
    /// </summary>
    public interface ILoadingTextService
    {
        /// <summary>
        /// 获取Loading文本列表
        /// </summary>
        List<string> LoadingTexts { get; }

        /// <summary>
        /// 获取Loading文本更新日期
        /// </summary>
        string? LoadingDate { get; }

        /// <summary>
        /// 获取随机Loading文本
        /// </summary>
        string GetRandomText();

        /// <summary>
        /// 获取权重随机Loading文本
        /// </summary>
        string GetWeightedRandomText();

        /// <summary>
        /// 从API加载Loading文本
        /// </summary>
        System.Threading.Tasks.Task LoadFromApiAsync(string apiEndpoint, System.Threading.CancellationToken cancellationToken = default);

        /// <summary>
        /// 从本地文件加载Loading文本
        /// </summary>
        void LoadFromFile(string filePath);

        /// <summary>
        /// 保存Loading文本到文件
        /// </summary>
        void SaveToFile(string filePath);

        /// <summary>
        /// 检查是否需要更新Loading文本（超过14天）
        /// </summary>
        bool NeedsUpdate();

        /// <summary>
        /// 下载新的Loading文本
        /// </summary>
        System.Threading.Tasks.Task<bool> DownloadNewTextsAsync(bool internationalMode);
    }
}
