namespace LLC_MOD_Toolbox.Interfaces
{
    /// <summary>
    /// Mirror酱服务接口
    /// 负责Mirror酱模式的管理、配置和状态查询
    /// </summary>
    public interface IMirrorChyanService
    {
        /// <summary>
        /// 是否启用MirrorChyan模式
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Mirror酱Token（如果已配置）
        /// </summary>
        string? Token { get; }

        /// <summary>
        /// 初始化MirrorChyan服务
        /// 检查本地存储的Token并加载配置
        /// </summary>
        void Initialize();

        /// <summary>
        /// 启用MirrorChyan模式
        /// </summary>
        /// <param name="token">Mirror酱Token</param>
        /// <param name="saveToConfig">是否保存到配置文件</param>
        /// <returns>是否成功启用</returns>
        bool Enable(string token, bool saveToConfig = true);

        /// <summary>
        /// 禁用MirrorChyan模式
        /// </summary>
        /// <param name="saveToConfig">是否保存到配置文件</param>
        void Disable(bool saveToConfig = true);

        /// <summary>
        /// 验证Token是否有效
        /// </summary>
        /// <param name="token">要验证的Token</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>是否有效</returns>
        System.Threading.Tasks.Task<bool> ValidateTokenAsync(
            string token,
            System.Threading.CancellationToken cancellationToken = default);

        /// <summary>
        /// 检查是否有已保存的Token
        /// </summary>
        /// <returns>是否有保存的Token</returns>
        bool HasSavedToken();

        /// <summary>
        /// 清除已保存的Token
        /// </summary>
        void ClearSavedToken();
    }
}
