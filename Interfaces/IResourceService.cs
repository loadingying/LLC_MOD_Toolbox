using Newtonsoft.Json.Linq;

namespace LLC_MOD_Toolbox.Interfaces
{
    /// <summary>
    /// 资源服务接口
    /// 管理应用资源和缓存
    /// </summary>
    public interface IResourceService
    {
        /// <summary>
        /// 获取或创建Hash缓存对象
        /// </summary>
        JObject? GetOrCreateHashCache();

        /// <summary>
        /// 保存Hash缓存到文件
        /// </summary>
        Task SaveHashCacheAsync();

        /// <summary>
        /// 清除所有缓存
        /// </summary>
        Task ClearAllCacheAsync();

        /// <summary>
        /// 获取缓存大小（字节）
        /// </summary>
        long GetCacheSize();

        /// <summary>
        /// 检查缓存是否有效
        /// </summary>
        bool IsCacheValid(string cacheKey, TimeSpan maxAge);

        /// <summary>
        /// 更新缓存时间戳
        /// </summary>
        void UpdateCacheTimestamp(string cacheKey);
    }
}
