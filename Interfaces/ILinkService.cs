namespace LLC_MOD_Toolbox.Interfaces
{
    /// <summary>
    /// 链接服务接口
    /// 管理外部链接的配置和跳转
    /// </summary>
    public interface ILinkService
    {
        /// <summary>
        /// 获取所有已配置的链接
        /// </summary>
        /// <returns>链接字典（控件名 -> URL）</returns>
        System.Collections.Generic.Dictionary<string, string> GetLinks();

        /// <summary>
        /// 根据控件名获取URL
        /// </summary>
        /// <param name="controlName">控件名称</param>
        /// <returns>URL字符串，如果不存在则返回null</returns>
        string? GetUrl(string controlName);

        /// <summary>
        /// 添加链接
        /// </summary>
        /// <param name="controlName">控件名称</param>
        /// <param name="url">URL地址</param>
        void AddLink(string controlName, string url);

        /// <summary>
        /// 移除链接
        /// </summary>
        /// <param name="controlName">控件名称</param>
        /// <returns>是否成功移除</returns>
        bool RemoveLink(string controlName);

        /// <summary>
        /// 初始化默认链接
        /// </summary>
        void InitializeDefaultLinks();
    }
}
