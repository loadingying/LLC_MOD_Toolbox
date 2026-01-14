using LLC_MOD_Toolbox.Interfaces;
using Newtonsoft.Json.Linq;
using System.IO;

namespace LLC_MOD_Toolbox.Services
{
    /// <summary>
    /// 资源服务实现
    /// 管理应用资源和缓存
    /// </summary>
    public class ResourceService : IResourceService
    {
        private readonly IPathService _pathService;
        private JObject? _hashCacheObject;
        private readonly Dictionary<string, DateTime> _cacheTimestamps = new();

        /// <summary>
        /// Hash缓存文件路径
        /// </summary>
        private string HashCachePath => Path.Combine(_pathService.GameDirectory ?? "",
            "LimbusCompany_Data", "LLC_zh-CN", "Info", "hash_cache.json");

        /// <summary>
        /// 初始化ResourceService
        /// </summary>
        public ResourceService(IPathService pathService)
        {
            _pathService = pathService ?? throw new ArgumentNullException(nameof(pathService));
        }

        /// <summary>
        /// 获取或创建Hash缓存对象
        /// </summary>
        public JObject? GetOrCreateHashCache()
        {
            if (_hashCacheObject != null)
            {
                return _hashCacheObject;
            }

            try
            {
                if (!File.Exists(HashCachePath))
                {
                    _hashCacheObject = new JObject();
                    Log.logger.Info("创建新的Hash缓存对象");
                    return _hashCacheObject;
                }

                var json = File.ReadAllText(HashCachePath);
                _hashCacheObject = JObject.Parse(json);
                Log.logger.Info("加载Hash缓存成功");
                return _hashCacheObject;
            }
            catch (Exception ex)
            {
                Log.logger.Error("加载Hash缓存失败", ex);
                _hashCacheObject = new JObject();
                return _hashCacheObject;
            }
        }

        /// <summary>
        /// 保存Hash缓存到文件
        /// </summary>
        public async Task SaveHashCacheAsync()
        {
            try
            {
                if (_hashCacheObject == null)
                {
                    Log.logger.Warn("Hash缓存对象为空，无法保存");
                    return;
                }

                // 确保目录存在
                var directory = Path.GetDirectoryName(HashCachePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = _hashCacheObject.ToString();
                await File.WriteAllTextAsync(HashCachePath, json);
                Log.logger.Info("Hash缓存已保存");
            }
            catch (Exception ex)
            {
                Log.logger.Error("保存Hash缓存失败", ex);
            }
        }

        /// <summary>
        /// 清除所有缓存
        /// </summary>
        public async Task ClearAllCacheAsync()
        {
            try
            {
                _hashCacheObject = null;
                _cacheTimestamps.Clear();

                // 删除缓存文件
                if (File.Exists(HashCachePath))
                {
                    File.Delete(HashCachePath);
                }

                Log.logger.Info("所有缓存已清除");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Log.logger.Error("清除缓存失败", ex);
            }
        }

        /// <summary>
        /// 获取缓存大小（字节）
        /// </summary>
        public long GetCacheSize()
        {
            try
            {
                if (!File.Exists(HashCachePath))
                {
                    return 0;
                }

                var fileInfo = new FileInfo(HashCachePath);
                return fileInfo.Length;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 检查缓存是否有效
        /// </summary>
        public bool IsCacheValid(string cacheKey, TimeSpan maxAge)
        {
            if (!_cacheTimestamps.ContainsKey(cacheKey))
            {
                return false;
            }

            var timestamp = _cacheTimestamps[cacheKey];
            var age = DateTime.Now - timestamp;
            return age <= maxAge;
        }

        /// <summary>
        /// 更新缓存时间戳
        /// </summary>
        public void UpdateCacheTimestamp(string cacheKey)
        {
            _cacheTimestamps[cacheKey] = DateTime.Now;
        }
    }
}
